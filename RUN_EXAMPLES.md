# How to Run Examples/Program.cs

## Quick Start

### 1. Navigate to Examples directory
```bash
cd Examples
```

### 2. Create .env file
```bash
# Copy the example file
cp .env.example .env

# Edit .env with your credentials
# Required: NIMBBL_ACCESS_KEY and NIMBBL_ACCESS_SECRET
```

### 3. Run the program
```bash
# From Examples directory
dotnet run

# OR from SDK root directory
dotnet run --project Examples
```

## What the Program Does

The `Program.cs` is an **interactive CLI menu** that lets you:
- Generate merchant tokens
- Create orders
- Process payments
- Handle refunds
- Test webhooks
- And more...

## Configuration

Edit `.env` file with your Nimbbl credentials:

```env
NIMBBL_ACCESS_KEY=your_actual_access_key
NIMBBL_ACCESS_SECRET=your_actual_access_secret
NIMBBL_API_HOST=https://api.nimbbl.tech  # Optional
```

## Troubleshooting

1. **"Please update .env file"** - Make sure `.env` exists and has valid credentials
2. **Build errors** - Run `dotnet restore` from SDK root
3. **API errors** - Check your credentials are correct
