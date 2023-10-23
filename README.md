# nimbbl-dotnet-sdk

SDK for dotnet web API applications.

## Working with project

### Build

```
dotnet build 
```

### Test

```
dotnet test
```

### Generate assembly package

```
dotnet pack
```

## Integration

### Adding reference to the SDK

#### Through Assembly reference

- Generate the assembly by going through the previous steps
- Add the assembly as reference into the current project.
  ```xml
  <ItemGroup>
      <Reference Include="Nimbbl.Sdk.Rest">
         <HintPath>path\to\Nimbbl.Sdk.Rest.dll</HintPath>
      </Reference>
  </ItemGroup>
  ```
- Build the project

#### Through Project reference

- Add a reference to the `nimbbl sdk` csproj in your current project.
  ```xml
  <ItemGroup>
      <ProjectReference Include="path\to\Nimbbl.Sdk.Rest\Nimbbl.Sdk.Rest.csproj" />
  </ItemGroup>
  ```
- Build the project

### Instantiating the `NimbbleClient`

- Add following code to the Startup logic.
  ```c#
  var nimbbl = await NimbblClient.CreateAsync()
  ```

### Creating the order

```c#
var orders = (await NimbblClient.CreateAsync()).Orders
var order= orders.CreateAsync(orderRequest);
```