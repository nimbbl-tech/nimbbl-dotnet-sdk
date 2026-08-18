#!/bin/bash
#
# Publish the Nimbbl .NET SDK (Nimbbl.Sdk.Rest) to NuGet.org.
#
# Usage:   NUGET_API_KEY=<key> ./publish.sh <version>
# Example: NUGET_API_KEY=oy2... ./publish.sh 1.4.0-alpha.1
#
# Notes:
#   - Pass the version WITHOUT a leading 'v' (the git tag adds it).
#   - The version MUST match <Version> in Nimbbl.Sdk.Rest/Nimbbl.Sdk.Rest.csproj
#     (single source of truth) — update the csproj first.
#   - The NuGet API key is read from the NUGET_API_KEY env var. NEVER hardcode it.
#   - Prerelease (e.g. -alpha.1) is auto-detected by NuGet from the version suffix.
#   - Set PUBLISH_YES=1 to skip the interactive confirmation (for CI).
#
set -euo pipefail

CSPROJ="Nimbbl.Sdk.Rest/Nimbbl.Sdk.Rest.csproj"
SLN="Nimbbl.Sdk.Rest.sln"
TEST_PROJ="Nimbbl.Sdk.Rest.Test/Nimbbl.Sdk.Rest.Test.csproj"
SOURCE="https://api.nuget.org/v3/index.json"
ARTIFACTS="./artifacts"
# Credential-free tests only (integration tests needing NIMBBL_ACCESS_KEY are excluded).
TEST_FILTER="FullyQualifiedName~SignatureVerifierTest|FullyQualifiedName~CentralMaskerTest|FullyQualifiedName~EncryptionTest|FullyQualifiedName~PayloadHelperUtilsTest|FullyQualifiedName~PreAuthE2ETest|FullyQualifiedName~E2ETest|FullyQualifiedName~PaymentInitiateMockTest"

# Load NUGET_API_KEY (and any other vars) from a local .env file next to this script,
# if present. .env is gitignored — never commit your real key. See .env.example.
# An already-exported NUGET_API_KEY in the shell takes precedence over the .env value.
ENV_FILE="$(dirname "$0")/.env"
if [ -f "$ENV_FILE" ] && [ -z "${NUGET_API_KEY:-}" ]; then
  echo "Loading environment from $ENV_FILE"
  set -a
  # shellcheck disable=SC1090
  . "$ENV_FILE"
  set +a
fi

VERSION="${1:-}"

# --- 0. Argument + environment guards -------------------------------------------------
if [ -z "$VERSION" ]; then
  echo "Usage: NUGET_API_KEY=<key> $0 <version>"
  echo "Example: NUGET_API_KEY=oy2... $0 1.4.0-alpha.1"
  exit 1
fi

if [[ "$VERSION" == v* ]]; then
  echo "Error: pass the version WITHOUT a leading 'v' (e.g. 1.4.0-alpha.1, not v1.4.0-alpha.1)."
  exit 1
fi

# SemVer X.Y.Z with an optional -prerelease suffix (e.g. 1.4.0, 1.4.0-alpha.1, 1.4.0-rc.2).
if ! [[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z.-]+)?$ ]]; then
  echo "Error: '$VERSION' is not a valid semantic version (expected X.Y.Z or X.Y.Z-suffix)."
  exit 1
fi

if [ -z "${NUGET_API_KEY:-}" ]; then
  echo "Error: NUGET_API_KEY is not set. Export it at runtime (never hardcode):"
  echo "  export NUGET_API_KEY=<your-nuget-api-key>"
  exit 1
fi

# --- 0b. Version must match csproj <Version> (single source of truth) ------------------
CSPROJ_VERSION=$(grep -oE '<Version>[^<]+</Version>' "$CSPROJ" | sed -E 's/<\/?Version>//g' | head -1)
if [ -z "$CSPROJ_VERSION" ]; then
  echo "Error: could not read <Version> from $CSPROJ."
  exit 1
fi
if [ "$CSPROJ_VERSION" != "$VERSION" ]; then
  echo "Error: version mismatch — argument '$VERSION' != csproj <Version> '$CSPROJ_VERSION'."
  echo "Update <Version> in $CSPROJ to '$VERSION' first, or pass '$CSPROJ_VERSION'."
  exit 1
fi
echo "Version $VERSION matches csproj <Version>."

# --- 1. No uncommitted changes --------------------------------------------------------
if ! git diff-index --quiet HEAD --; then
  echo "Error: uncommitted changes detected. Commit or stash them before publishing."
  exit 1
fi
echo "Working tree clean."

# --- 2. Quality gate: build (Release) + credential-free test suite --------------------
echo "Building (Release)..."
dotnet build "$SLN" -c Release --nologo
echo "Running credential-free test suite..."
dotnet test "$TEST_PROJ" -c Release --nologo --filter "$TEST_FILTER"
echo "Quality gate passed (build + tests green)."

# --- 3. Pack --------------------------------------------------------------------------
rm -rf "$ARTIFACTS"
echo "Packing $VERSION..."
dotnet pack "$CSPROJ" -c Release -o "$ARTIFACTS" --nologo
NUPKG=$(ls "$ARTIFACTS"/*.nupkg | head -1)
echo "Built package: $NUPKG"

# --- 4. Confirm (irreversible) --------------------------------------------------------
echo ""
echo "About to PUBLISH '$NUPKG' to $SOURCE"
echo "This is irreversible — NuGet allows unlist, not delete."
if [ "${PUBLISH_YES:-}" != "1" ]; then
  read -r -p "Type 'publish' to continue: " CONFIRM
  if [ "$CONFIRM" != "publish" ]; then
    echo "Aborted. The .nupkg is in $ARTIFACTS if you want to push manually."
    exit 1
  fi
fi

# --- 5. Push to NuGet.org -------------------------------------------------------------
echo "Pushing to NuGet.org..."
dotnet nuget push "$NUPKG" --api-key "$NUGET_API_KEY" --source "$SOURCE" --skip-duplicate

# --- 6. Tag the release ---------------------------------------------------------------
BRANCH=$(git rev-parse --abbrev-ref HEAD)
echo "Tagging v$VERSION (branch: $BRANCH)..."
git tag "v$VERSION"
git push origin "v$VERSION"

echo ""
echo "Published Nimbbl.Sdk.Rest $VERSION and pushed tag v$VERSION."
echo "Prerelease consumers install with: dotnet add package Nimbbl.Sdk.Rest --prerelease"
