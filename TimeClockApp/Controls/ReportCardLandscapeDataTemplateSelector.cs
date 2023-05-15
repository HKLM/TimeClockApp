namespace TimeClockApp.Controls
{
    public partial class ReportCardLandscapeDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnpaidLandscapeTemplate { get; set; }
        public DataTemplate PaidLandscapeTemplate { get; set; }
        /// <summary>
        /// Template for when TimeCard_Status is ClockedIn
        /// </summary>
        public DataTemplate OnTheClockLandscapeTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container) =>
            ((TimeCard)item).TimeCard_Status.Equals(ShiftStatus.ClockedIn) ? OnTheClockLandscapeTemplate : (((TimeCard)item).TimeCard_Status.Equals(ShiftStatus.Paid) ? PaidLandscapeTemplate : UnpaidLandscapeTemplate);
    }
}
