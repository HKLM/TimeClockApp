namespace TimeClockApp.Services
{
    public class UserManagerService : TimeCardDataStore
    {
        public bool AddNewEmployee(string employeeName, double payRate, string jobTitle)
        {
            if (string.IsNullOrEmpty(employeeName) || Context.Employee.Any(x => x.Employee_Name == employeeName))
                return false;

            try
            {
                Employee p = new()
                {
                    Employee_Name = employeeName,
                    Employee_PayRate = payRate,
                    JobTitle = jobTitle
                };

                Context.Add<Employee>(p);
                return (Context.SaveChanges() > 0);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.InnerException);
                //throw;
            }
            return false;
        }

        public bool UpdateEmployee(int employeeId, string name, double payRate, EmploymentStatus employed)
        {
            if (employeeId == 0)
                return false;

            Employee item = GetEmployee(employeeId);
            if (item != null)
            {
                if (!string.IsNullOrEmpty(name))
                    item.Employee_Name = name;

                item.Employee_PayRate = payRate;
                item.Employee_Employed = employed;

                try
                {
                    Context.Update<Employee>(item);
                    return (Context.SaveChanges() > 0);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.InnerException);
                    //throw;
                }
            }
            return false;
        }

        public bool UpdateEmployee(int employeeId, EmploymentStatus employed)
        {
            if (employeeId == 0)
                return false;

            Employee item = GetEmployee(employeeId);
            if (item != null)
            {
                item.Employee_Employed = employed;

                try
                {
                    Context.Update<Employee>(item);
                    return (Context.SaveChanges() > 0);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.InnerException);
                    //throw;
                }
            }
            return false;
        }


        public bool FireEmployee(int employeeId)
        {
            if (employeeId == 0)
                return false;

            try
            {
                if (Context.Employee.Count() > 1)
                {
                    Employee item = GetEmployee(employeeId);
                    if (item != null)
                    {
                        item.Employee_Employed = EmploymentStatus.NotEmployed;
                        Context.Update<Employee>(item);
                        return (Context.SaveChanges() > 0);
                    }
                }
                else
                    ShowPopupError("There must be atleast one employee.", "ERROR");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.InnerException);
                //throw;
            }
            return false;
        }
    }
}
