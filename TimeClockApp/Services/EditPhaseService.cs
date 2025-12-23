using System.Data;

namespace TimeClockApp.Services
{
    public class EditPhaseService : TimeCardDataStore
    {
        public ObservableCollection<Phase> GetEditPhaseList() => Context.Phase
                .OrderBy(e => e.PhaseTitle)
                .ToObservableCollection();

        public bool AddNewPhase(string phaseName)
        {
            if (phaseName == null || Context.Phase.Any(x => x.PhaseTitle.Contains(phaseName)))
                return false;
            try
            {
                Phase p = new(phaseName);

                Context.Add<Phase>(p);
                return Context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
            return false;
        }

        public bool UpdatePhase(string name, int id)
        {
            if (id < 2 || name == null || name?.Length == 0 || Context.Phase.Any(x => x.PhaseTitle.Contains(name)))
                return false;
            try
            {
                Phase item = GetPhase(id);
                if (item != null)
                {
                    item.PhaseTitle = name;
                    Context.Update<Phase>(item);
                    return Context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
            return false;
        }

        public async Task<bool> DeletePhase(Phase item)
        {
            if (item == null)
                return false;
            try
            {
                Context.Remove<Phase>(item);
                return await Context.SaveChangesAsync().ConfigureAwait(false) > 0;
            }
            catch (AggregateException ax)
            {
                TimeClockApp.Shared.Exceptions.FlattenAggregateException.ShowAggregateException(ax);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
            return false;
        }
    }
}
