using TimeClockApp.Models;

namespace TimeClockApp.Controls
{
    public partial class TeamStatusDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ActiveTeamTemplate { get; set; }
        public DataTemplate InactiveTeamTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((Employee)item).Employee_Employed == EmploymentStatus.Employed ? ActiveTeamTemplate : InactiveTeamTemplate;
        }
    }
}
