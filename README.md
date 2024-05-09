# TimeClockApp

## .NET 9 MAUI app for Android 10+ using a SQLite database.

Simple app for employer to track employees time cards for payroll use. Supports multiple employees over different projects.

Supports Dark theme.

### Screenshots

| screenshots | screenshots |
| --- | --- |
| ![Screenshot 1](/images/Screenshot_1.png) | ![Screenshot 2](/images/Screenshot_2.png) |
| ![Screenshot 3](/images/Screenshot_3.png) | ![Screenshot 4](/images/Screenshot_4.png) |

### NOTICE

Database has been changed between v1 to v1.4. If you have any data you want to keep, before upgrading to version 1.4, backup your current data by using `Tools`->`Backup`->`Export Data` This will create a ZIP file containing all the SQLite table data in CSV files.
Then after upgrading, use `Tools`->`Backup`->`Import Data` to restore your data.

### Notes

Built using .NET 9.0-preview3
and the following nuget packages:
```
"CommunityToolkit.Maui" Version="9.0.0"
"CommunityToolkit.Mvvm" Version="8.2.2"
"CsvHelper" Version="32.0.1"
"Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0-preview.3.24172.4"
```

To change the default items that are added during the database creation,
see the file [TimeClockApp.Shared\Models\DataBackendContext.cs](/TimeClockApp.Shared/Models/DataBackendContext.cs)

### Release Notes

1.4
* Fix issue with multiple timecards on the same day, that would not correctly add up the hours for overtime.
* Fix issue with double overtime (As per overtime in USA any work over 8 hours in a single day is overtime. After the 12th hour it is double overtime)
* Redesigned the payroll detail page.
* Fixed issue with the backup of the entire sqlite database file. Can only make a copy of current database. Still can not import that file. Only import via CVS files is supported
* Removed unnessesary Wages table from database
* Fixed issue of a timecard from a prior date that was not clocked out of.
* Now uses EF migrations to create and update the database.  See migrations section below

1.0
* Initial Release

### Migrations

To create the initial migration files needed for the database

1. Set the 'Solution Configuration' to `DebugMigrator`
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
