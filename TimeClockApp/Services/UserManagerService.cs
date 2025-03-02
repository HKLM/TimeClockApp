namespace TimeClockApp.Services
{
    public class UserManagerService : TimeCardDataStore
    {
        public bool AddNewEmployee(string employeeName, double payRate, string jobTitle, EmploymentStatus employed)
        {
            if (string.IsNullOrEmpty(employeeName) || Context.Employee.Any(x => x.Employee_Name == employeeName))
                return false;

            try
            {
                Employee p = new(employeeName, payRate, employed, jobTitle);
                Context.Add<Employee>(p);
                return (Context.SaveChanges() > 0);
            }
            catch (Exception e)
            {
                Log.WriteLine($"{e.Message}\n{e.InnerException}");
                //throw;
            }
            return false;
        }

        public bool UpdateEmployee(int employeeId, string name, double payRate, EmploymentStatus employed, string jobTitle)
        {
            if (employeeId == 0 || string.IsNullOrEmpty(name))
                return false;

            Employee item = GetEmployee(employeeId);
            if (item == null) return false;

            if (!string.IsNullOrEmpty(name))
                item.Employee_Name = name;

            item.Employee_PayRate = payRate;
            item.Employee_Employed = employed;
            item.JobTitle = jobTitle;
            Context.Update<Employee>(item);
            return (Context.SaveChanges() > 0);
        }

        public bool UpdateEmployee(int employeeId, EmploymentStatus employed)
        {
            if (employeeId == 0)
                return false;

            Employee item = GetEmployee(employeeId);
            if (item == null) return false;

            item.Employee_Employed = employed;
            Context.Update<Employee>(item);
            return (Context.SaveChanges() > 0);
        }

        public bool FireEmployee(int employeeId)
        {
            if (employeeId == 0)
                return false;

            if (Context.Employee.Count() > 1)
            {
                return UpdateEmployee(employeeId, EmploymentStatus.NotEmployed);
            }
            else
            {
                ShowPopupError("There must be atleast one employee.", "ERROR");
            }
            return false;
        }
    }
}
