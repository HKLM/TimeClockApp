namespace TimeClockApp.Controls
{
    public partial class SettingsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IntTemplate { get; set; }
        public DataTemplate StringTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((Config)item).IntValue.HasValue ? IntTemplate : StringTemplate;
        }
    }
}
