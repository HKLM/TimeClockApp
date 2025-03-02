# EFMigrator Readme

### Migrations How to use EFMigrator

To create the initial migration files needed for the database. Requires the Entity Framework Core tools to be installed. .NET Core CLI tool was used during development, but either tool set will work.

#### Verify type of EF tools (Package Manager Console tools or .NET Core CLI tools)

1. Test if Package Manager Console tools by open a PowerShell console and enter the command
```
Get-Help about_EntityFrameworkCore
```
2. If you get a error message, test if .NET Core CLI tools in the same console, enter the following command
```
dotnet ef
```
3. If that also gives a error, follow instructions at [https://learn.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools)

#### .NET Core CLI instructions

1. In Visual Studio 2022, Set the 'Solution Configuration' to `DebugMigrator`
2. Set the `EFMigrator` project as the 'Start up project'
3. Build the solution.
4. Right click on the EFMigrator project and select `Open in Terminal`.
5. In the Console window (PowerShell or Command Prompt), enter the command 
```
dotnet ef migrations add InitialCreate -c TimeClockApp.Shared.Models.DataBackendContext -p ..\TimeClockApp.Shared --configuration DebugMigrator --no-build --verbose
```
6. After the command completes. Set the 'Solution Configuration' to `Debug` or `Release`
7. Set the `TimeClockApp` project as the 'Start up project'

#### Package Manager Console tools instructions

1. In Visual Studio 2022, Set the 'Solution Configuration' to `DebugMigrator`
2. Set the `EFMigrator` project as the 'Start up project'
3. View->Other Windows->Package Manager Console
4. In the Package Manager Console window, set the 'Default project' to `TimeClockApp.Shared`
5. In the Package Manager Console, enter the command 
```
add-migration Initial -Context TimeClockApp.Shared.Models.DataBackendContext -Verbose
```
6. After the command completes. Set the 'Solution Configuration' to `Debug` or `Release`
7. Set the `TimeClockApp` project as the 'Start up project'


Thank you to @taublast for his guide on getting migrations to work with maui android projects. [github.com/taublast/MauiEF](https://github.com/taublast/MauiEF)
