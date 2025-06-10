# Sysinfocus Automation Tools
A comprehensive scaffolder for .NET Minimal API and Blazor WebAssembly Standalone projects with incremental feature addition using DTOs from the shared project.

## Requirements
- .NET 9.0
- `dotnet-ef` tool install by executing `dotnet tool install -g dotnet-ef`
- Git (as every action is committed, so you can revert back if something breaks)

## Install
Note: This package is in `beta` and may contain issues/bugs. Please log issues/bugs using **Issues** in this repo.
```
dotnet tool install -g Sysinfocus.Automation.CLI --prerelease
```

## Create a new solution
Follow these steps to create a new solution:
```
sa new <name> <database>
```
- Change to the folder where you want to create a new solution. For eg: if I want to create a new solution in `C:\Projects` folder.
- Once in `C:\Projects` folder, type and run `sa new Demo` to create a new solution with `Demo` namespace and `Sqlite` database.
- You can create solution with other databases like `SqlServer / MySql / Oracle / PostgreSQL / MongoDB`. To create with any of these, type and run `sa new Demo oracle`. This will create `Demo` solution with `Oracle` database.
- Goto the API project and change the `appsettings.Development.json` file with appropriate Connection string and database name (for MongoDB).
- Once you create solution, run migrate to initialize the database by executing `sa migrate Initial` where `Initial` is the name of the migration to create.
- Once migration is successful, run update to update the database by executing `sa update`.

## Adding a feature
In order to add a feature, you have to create a DTO file in the Shared project's DTO folder. Check example DTO below. You need to make sure the following:
```
sa add <name> <dto>
```
- The feature name and DTO name should be consistent. For eg: if feature name is `Comments`, DTO should be `CommentDto`.
- The name of the DTO file show end with 'Dto'
- The `namespace` should match as per your generated solutions' name.
- It should be placed in the Shared project's `DTO` folder.
- You can change and extend the example DTO given as per your required properties.
- The comments shown in the example are actually used to render appropriate component, whether to show or ignore the property in the Add, Update or Listing operation, etc.,
- Once you create a feature, run migrate to initialize the database by executing `sa migrate Feature` where `Feature` is the name of the migration to create.
- Once migration is successful, run update to update the database by executing `sa update`.  
```
namespace Demo.Shared.DTO;
public sealed class MCPDto : ModelValidator
{
    public Guid Id { get; set; } // NoAdd NoUpdate NoList
    public string Name { get; set; } = string.Empty;
    public string? Overview { get; set; } // Textarea
    public bool OfficialVendor { get; set; } // Switch

    public override Dictionary<string, string>? Errors()
    {
        var errors = new Dictionary<string, string>();
        if (!Name.IsLength(5, 100)) errors.TryAdd("Name", "Name must be between 5 and 100 chars.");
        return errors.Count == 0 ? null : errors;
    }
}
```

## Running migrations
```
sa migrate <name>
```

## Updating migrations
```
sa update
```

## Running projects
- You can run both API and UI project by executing `sa run` command

## Videos

Watch video demonstrating the solution generation. 

[![Watch the video](https://img.youtube.com/vi/RTsoD0UxzGA/0.jpg)](https://www.youtube.com/watch?v=RTsoD0UxzGA)


Watch video for the output of the generated solution.

[![Watch the video](https://img.youtube.com/vi/kJLhqg1zCmU/0.jpg)](https://www.youtube.com/watch?v=kJLhqg1zCmU)
