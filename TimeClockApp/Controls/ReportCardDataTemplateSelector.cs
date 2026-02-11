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

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            ShiftStatus status = ((TimeCard)item).TimeCard_Status;
            return status == ShiftStatus.ClockedIn ? OnTheClockTemplate : 
                   status == ShiftStatus.Paid ? PaidTemplate : 
                   UnpaidTemplate;
        }
    }
}
