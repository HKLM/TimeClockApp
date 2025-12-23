# TimeClockApp

## .NET 10 MAUI app for Android 10+ using a SQLite database.

Simple app for employer to track employees time cards for payroll use. Supports multiple employees over different projects.

Supports Dark theme.

### Screenshots

| screenshots | screenshots |
| --- | --- |
| ![Screenshot 1](/images/Screenshot_1.png) | ![Screenshot 2](/images/Screenshot_2.png) |
| ![Screenshot 3](/images/Screenshot_3.png) | ![Screenshot 4](/images/Screenshot_4.png) |

### Breaking Changes

2.0
Is not compatible with prior exported data. Same as in version 1.8, it is possible but requires edits to the csv files before importing the data.

1.8
Add version check during data Import, Prior exported data will not be allowed to be imported in. 
(It is possible with some edits to the csv files to make compatible. [TimeClockApp/discussions](https://github.com/HKLM/TimeClockApp/discussions) Use the project discussions page and ask me for details if you need to do this)

### Notes

Built using .NET 10.0.1
and the following nuget packages:
```
"CommunityToolkit.Maui" Version="13.0.0"
"CommunityToolkit.Mvvm" Version="8.4.0"
"Microsoft.Maui.Controls" Version="10.0.2"
"CsvHelper" Version="33.1.0"
"Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.1"
```

To change the default items that are added during the database creation,
see the file [TimeClockApp.Shared\Models\DataBackendContext.cs](/TimeClockApp.Shared/Models/DataBackendContext.cs)

To change the name of the database file for creation,
see the property 'sQLiteDbFileName' in file [TimeClockApp\TimeClockApp.Shared\SQLiteSetting.cs](/TimeClockApp.Shared/SQLiteSetting.cs)

### Release Notes
2.1
* Minor bug fixes
* Many code cleanup and optimizations
* Added experimental support to use MSSQL server as the database backend.
* Fixed issue of not working on windows platform.
* Updated to .NET 10.0.1 and latest nuget packages.
* Improved exception logging for async methods.
* Improved UI responsiveness when loading large data sets.
* Improved overall app stability.

2.0
* Add invoice and Invoice detail pages
* bug fixes
* Removed unused code
* Fixed NumericValidationBehavior and TextValidationBehavior not working
* Better exception error logging with for async methods

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



### Migrations How to use EFMigrator
See [EFMigrator\README.md](/EFMigrator/README.md)

