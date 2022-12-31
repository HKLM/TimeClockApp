namespace TimeClockApp.Controls
{
    public partial class SingleDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TheTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return TheTemplate;
        }
    }
}
