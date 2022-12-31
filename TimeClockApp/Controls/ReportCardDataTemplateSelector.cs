using TimeClockApp.Models;

namespace TimeClockApp.Controls
{
    public partial class ReportCardDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnpaidTemplate { get; set; }
        public DataTemplate PaidTemplate { get; set; }
        /// <summary>
        /// Template for when TimeCard_Status is ClockedIn
        /// </summary>
        public DataTemplate OnTheClockTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
            ((TimeCard)item).TimeCard_Status.Equals(ShiftStatus.ClockedIn) ? OnTheClockTemplate : (((TimeCard)item).TimeCard_Status.Equals(ShiftStatus.Paid) ? PaidTemplate : UnpaidTemplate);
    }
}
