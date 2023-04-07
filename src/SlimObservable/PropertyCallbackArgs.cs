namespace SlimObservable
{
    public class PropertyCallbackArgs<T>
	{
        public PropertyCallbackArgs(string propertyName, T oldValue, T newValue)
        {
            PropertyName = propertyName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public string PropertyName { get; }
        public T OldValue { get; }
		public T NewValue { get; set; }
    }
}

