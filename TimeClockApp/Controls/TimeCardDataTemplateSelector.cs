namespace TimeClockApp.Controls
{
    public partial class TimeCardDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StartTemplate { get; set; }
        public DataTemplate EndTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return !(item is TimeCard) ? StartTemplate
                : ((TimeCard)item).TimeCard_Status != ShiftStatus.ClockedIn ? StartTemplate : EndTemplate;
        }
    }
}
