# TimeClockApp

## .NET 9 MAUI app for Android 10+ using a SQLite database.

Simple app for employer to track employees time cards for payroll use. Supports multiple employees over different projects.

Supports Dark theme.

### Screenshots

| screenshots | screenshots |
| --- | --- |
| ![Screenshot 1](/images/Screenshot_1.png) | ![Screenshot 2](/images/Screenshot_2.png) |
| ![Screenshot 3](/images/Screenshot_3.png) | ![Screenshot 4](/images/Screenshot_4.png) |

### Breaking Changes

1.8
Add version check during data Import, Prior exported data will not be allowed to be imported in. 
(It is possible with some edits to the csv files to make compatible. [TimeClockApp/discussions](https://github.com/HKLM/TimeClockApp/discussions) Use the project discussions page and ask me for details if you need to do this)

### Notes

Built using .NET 9.0-preview6
and the following nuget packages:
```
"CommunityToolkit.Maui" Version="9.0.2"
"CommunityToolkit.Mvvm" Version="8.2.2"
"CsvHelper" Version="33.0.1"
"Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0-preview.6.24327.4"
```

To change the default items that are added during the database creation,
see the file [TimeClockApp.Shared\Models\DataBackendContext.cs](/TimeClockApp.Shared/Models/DataBackendContext.cs)

### Release Notes

1.8 
* Moved ExpenseType from a Enum into a database table. 
* Added EditExpenseTypePage. 
* Auto add Expense entry for the amount paid out for payroll along with a entry for WorkersComp amount.
* Reorder the Shell Flyout menu. 
* Add version check during data Import, Prior exported data will not be allowed to be imported in. (It is possible but the Expense.csv and Config.csv files would need edit to make compatible)

1.7 
* minor bug fixes. Code cleanup.
* Fixed issue when deploying in Release mode to physical device, app would crash upon startup.

1.6 
* Add ReportPage to view the total cost for a project, date range, per employee, etc. 
* Fixed issue with PayrollDetail grouping.  
* Moved FileService and SQLiteSetting class to TimeClockApp.Shared 
* Removed the project TimeClockApp.FileHelper

1.4
* Fix issue with multiple timecards on the same day, that would not correctly add up the hours for overtime.
* Fix issue with double overtime (As per overtime in USA any work over 8 hours in a single day is overtime. After the 12th hour it is double overtime)
* Redesigned the payroll detail page.
* Fixed issue with the backup of the entire sqlite database file. Can only make a copy of current database. Still can not import that file. Only import via CVS files is supported
* Removed unnecessary Wages table from database
* Fixed issue of a timecard from a prior date that was not clocked out of.
* Now uses EF migrations to create and update the database.  See migrations section below
* Database has been changed between v1 to v1.4. If you have any data you want to keep, before upgrading to version 1.4, backup your current data by using `Tools`->`Backup`->`Export Data` This will create a ZIP file containing all the SQLite table data in CSV files.
Then after upgrading, use `Tools`->`Backup`->`Import Data` to restore your data.

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
