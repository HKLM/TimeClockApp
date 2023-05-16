# TimeClockApp
 
## .NET 8 MAUI app for Android 10+ using a SQLite database.

Simple app for employer to track employees time cards for payroll use. Supports multiple employees over different projects.

### Screenshots

| screenshots | screenshots |
| --- | --- |
| ![Screenshot 1](/images/Screenshot_1.png) | ![Screenshot 2](/images/Screenshot_2.png) |
| ![Screenshot 3](/images/Screenshot_3.png) | ![Screenshot 4](/images/Screenshot_4.png) |

### NOTICE

Before upgrading to a new version, backup your current data by using Tools->Backup->Export. This will create a ZIP file containing all the SQLite table data in CSV files.
Then after upgrading, use the Import tool to restore your data.

### Notes

Built using the following nuget packages:
```
"CommunityToolkit.Maui" Version="5.1.0"
"CommunityToolkit.Mvvm" Version="8.2.0"
"CsvHelper" Version="30.0.1"
"Microsoft.EntityFrameworkCore" Version="8.0.0-preview.3.23174.2"
"Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0-preview.3.23174.2"
```

To change the default items that are added during the database creation,
see the file [Models\DatabackendContext.cs](/TimeClockApp/Models/DatabackendContext.cs)


