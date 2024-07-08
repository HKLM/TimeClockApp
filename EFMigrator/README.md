# EFMigrator Readme

### Migrations How to use EFMigrator

To create the initial migration files needed for the database

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
