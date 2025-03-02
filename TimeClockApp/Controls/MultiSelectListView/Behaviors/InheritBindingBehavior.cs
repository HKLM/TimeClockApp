namespace TimeClockApp.Controls.MultiSelectListView
{
    public class InheritBindingBehavior<T> : Behavior<T> where T : BindableObject
    {
        private bool _inheritedBindingContext;

        protected T AssociatedObject { get; private set; }

        protected override void OnAttachedTo(T bindable)
        {
            base.OnAttachedTo(bindable);

            AssociatedObject = bindable;
            InheritBindingContext(bindable);

            bindable.BindingContextChanged += OnBindingContextChanged;
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            InheritBindingContext(AssociatedObject);
        }

        private void InheritBindingContext(T bindableObject)
        {
            if (BindingContext == null || _inheritedBindingContext)
            {
                BindingContext = bindableObject.BindingContext;
                _inheritedBindingContext = true;
            }
        }

        protected override void OnDetachingFrom(T bindable)
        {
            BindingContext = null;
            bindable.BindingContextChanged -= OnBindingContextChanged;
        }
    }
}
