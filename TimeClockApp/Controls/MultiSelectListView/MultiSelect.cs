﻿namespace TimeClockApp.Controls.MultiSelectListView
{
    /// <summary>
    /// Original author <see href="https://github.com/TBertuzzi"/>
    /// Original source code and project <see href="https://github.com/TBertuzzi/Bertuzzi.MAUI.MultiSelectListView"/>
    /// Copyright ©2022 tbertuzzi
    /// </summary>
    public class MultiSelect
    {
        public static readonly BindableProperty EnableProperty =
             BindableProperty.CreateAttached("Enable", typeof(bool), typeof(ListView), false, propertyChanged: OnMultiSelectChanged);

        public static bool GetEnable(BindableObject view) => (bool)view.GetValue(EnableProperty);

        public static void SetEnable(BindableObject view, bool value) => view.SetValue(EnableProperty, value);

        private static void OnMultiSelectChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ListView listView = bindable as ListView;
            if (listView != null)
            {
                // Remove event
                listView.ItemSelected -= OnItemSelected;

                // Add new event to Multiple Select
                if (true.Equals(newValue))
                {
                    listView.ItemSelected += OnItemSelected;
                }
            }
        }

        private static void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            SelectableItem item = e.SelectedItem as SelectableItem;
            if (item != null)
            {
                // toggle the selection property
                item.IsSelected = !item.IsSelected;
            }

            // deselect the item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
