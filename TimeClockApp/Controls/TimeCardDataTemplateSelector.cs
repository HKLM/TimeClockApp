namespace TimeClockApp.Controls
{
    public partial class TimeCardDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StartTemplate { get; set; }
        public DataTemplate EndTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return item is TimeCard { TimeCard_Status: ShiftStatus.ClockedIn } 
                ? EndTemplate 
                : StartTemplate;
        }
    }
}
