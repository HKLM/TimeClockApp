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

#### .NET Core CLI instructions (ef tools installed globally)

1. In Visual Studio 2026, Set the 'Solution Configuration' to `SQLite Migration`
2. Set the `EFMigrator` project as the 'Start up project'
3. Build the solution.
4. Right click on the `TimeClockApp.Shared` project and select `Open in Terminal`.
5. In the Console window (PowerShell or Command Prompt), enter the command 
```
dotnet ef migrations add --project "TimeClockApp.Shared.csproj" --startup-project "..\EFMigrator\EFMigrator.csproj" --context TimeClockApp.Shared.Models.DataBackendContext --configuration "SQLite Migration" --no-build  "Initial" --output-dir "Migrations"
```
6. After the command completes. Set the 'Solution Configuration' to `Debug` or `Release`
7. Set the `TimeClockApp` project as the 'Start up project'

#### .NET Core CLI instructions (ef tools installed locally)

1. Same as above, but in step 5 use the command
```
dotnet tool run dotnet-ef  migrations add --project "TimeClockApp.Shared.csproj" --startup-project "..\EFMigrator\EFMigrator.csproj" --context TimeClockApp.Shared.Models.DataBackendContext --configuration "SQLite Migration" --no-build  "Initial" --output-dir "Migrations"
```

#### Package Manager Console tools instructions

1. In Visual Studio 2026, Set the 'Solution Configuration' to `SQLite Migration`
2. Set the `EFMigrator` project as the 'Start up project'
3. View->Other Windows->Package Manager Console
4. In the Package Manager Console window, set the 'Default project' to `TimeClockApp.Shared`
5. In the Package Manager Console, enter the command 
```
add-migration Initial -Context TimeClockApp.Shared.Models.DataBackendContext -Verbose
```
6. After the command completes. Set the 'Solution Configuration' to `Debug` or `Release`
7. Set the `TimeClockApp` project as the 'Start up project'


#### .NET Core CLI instructions for MSSQL migrations (ef tools installed globally)

Note: This is experimental. A dedicated MSSQL server will need to be acessable for the app to function. The solution will also have to be built for MSSQL.

PREREQUISITES:
1. MSSQL server acessable (local or remote) with a database created for the app to use. Your login will need to have the `db_admin` database role permission.
2. If you have already created SQLite migration files, you MUST delete them from the `TimeClockApp.Shared/Migrations` folder before proceeding.
3. Edit the `TimeClockApp/TimeClockApp.csproj` file, change the 'DefineConstants' from SQLITE to MSSQL.
```xml
  <PropertyGroup>
    <DefineConstants>$(DefineConstants);SQLITE</DefineConstants>
  </PropertyGroup>
```
To this:
```xml
  <PropertyGroup>
    <DefineConstants>$(DefineConstants);MSSQL</DefineConstants>
  </PropertyGroup>
```
4. Edit the `TimeClockApp.Shared/TimeClockApp.Shared.csproj` file, change the 'Otherwise' PropertyGroup from this:
```xml
  <Otherwise>
    <PropertyGroup>
      <!--SET THE DATABASE TYPE HERE SQLITE OR MSSQL-->
      <DefineConstants>$(DefineConstants);SQLITE</DefineConstants>
    </PropertyGroup>
  </Otherwise>
```
To this:
```xml
  <Otherwise>
    <PropertyGroup>
      <!--SET THE DATABASE TYPE HERE SQLITE OR MSSQL-->
      <DefineConstants>$(DefineConstants);MSSQL</DefineConstants>
    </PropertyGroup>
  </Otherwise>
```
5. Edit the `TimeClockApp.Shared/MSSQLSetting.cs` file. On line 21 change the connection string to point to your MSSQL server.
```csharp
private const string mSQLconnStr = "(localdb)\\MSSQLLocalDB;Initial Catalog=TimeClockApp;Persist Security Info=True;User ID=username;Password=password;Encrypt=True;Trust Server Certificate=True;Authentication=SqlPassword;";
```
6. Remove the nuget package `Microsoft.EntityFrameworkCore.Sqlite` from the `TimeClockApp.Shared` project and the `EFMigrator` project.
7. Add the nuget package `Microsoft.EntityFrameworkCore.SqlServer` to the `TimeClockApp.Shared` project and the `EFMigrator` project.
8. In Visual Studio 2026, Set the 'Solution Configuration' to `MSSQL Migration`
9. Set the `EFMigrator` project as the 'Start up project'
10. IMPORTANT: Select the root solution `Solution 'TimeClockApp'`. Right click on it and select `Clean Solution`
11. Right click on it again and select `Rebuild Solution`.
12. Right click on the `TimeClockApp.Shared` project and select `Open in Terminal`.
13. In the Console window (PowerShell or Command Prompt), enter the command
```
dotnet ef migrations add --project "TimeClockApp.Shared.csproj" --startup-project "..\EFMigrator\EFMigrator.csproj" --context TimeClockApp.Shared.Models.DataBackendContext --configuration "MSSQL Migration" --no-build  "Initial" --output-dir "Migrations"
```
14. After the command completes. Set the 'Solution Configuration' to `Debug` or `Release`
15. Set the `TimeClockApp` project as the 'Start up project'

#### TROUBLESHOOTING MSSQL:

- It is recommended to use migrations to create the database tables in the MSSQL database. 
- As a last resort and if for some reason the tables are not being created properly in the MSSQL database. It is recommended to delete any existing tables created by EF. 
Then you can try using the sql file `EFMigrator/MSSQL/TimeClockApp.sql`. Use at your own risk. It may need to be modified to work with your server settings. 
Then run migrations after to ensure the migrations history table is created and up to date.

### Acknowledgements
Thank you to @taublast for his guide on getting migrations to work with maui android projects. [github.com/taublast/MauiEF](https://github.com/taublast/MauiEF)
