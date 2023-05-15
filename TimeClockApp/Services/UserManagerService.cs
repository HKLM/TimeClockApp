namespace TimeClockApp.Services
{
    public class UserManagerService : TimeCardDataStore
    {
        public bool AddNewEmployee(string employeeName, double payRate, string jobTitle)
        {
            if (employeeName == null || Context.Employee.Any(x => x.Employee_Name.Contains(employeeName)))
                return false;

            try
            {
                Employee p = new(employeeName, payRate, jobTitle);

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

        public Employee GetEmployee(int employeeID) => Context.Employee.Find(employeeID);

        public bool UpdateEmployee(int id, string name, double payRate, EmploymentStatus employed)
        {
            if (id == 0)
                return false;

            Employee item = GetEmployee(id);
            if (item != null)
            {
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

        public bool FireEmployee(int id)
        {
            if (id == 0)
                return false;

            try
            {
                Employee item = GetEmployee(id);
                if (item != null)
                {
                    if (Context.Employee.Count() > 1)
                    {
                        item.Employee_Employed = EmploymentStatus.NotEmployed;
                        Context.Update<Employee>(item);
                        return (Context.SaveChanges() > 0);
                    }
                    else
                        ShowPopupError("There must be atleast one employee.", "ERROR");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.InnerException);
                //throw;
            }
            return false;
        }

        public bool UpdateEmploymentStatus(Employee item, EmploymentStatus employed)
        {
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
    }
}
