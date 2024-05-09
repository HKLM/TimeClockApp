using TimeClockApp.Shared.Models;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Starting DB Migration...");

using (var db = new DataBackendContext())
{
    var all = db.Employee.ToList();
}
