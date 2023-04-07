# Summary

A C# library to automatically generate observable properties declared in an interface. 

It uses `INotifyPropertyChanged`.

# Usage

### Declaration

```cs
[ObservableProperties]
public interface TestObservableProperties
{
    [DefaultValue(5)]
    [PropertyCallback(nameof(TestObservable.OnIdChanged))]
    int Id { get; set; }

    string Name { get; set; }
    string Description { get; set; }
}

[ObservableObject]
public partial class TestObservable : TestObservableProperties
{
    internal void OnIdChanged(object sender, PropertyCallbackArgs<int> args)
    {
        Console.WriteLine($"Value changed: {args.PropertyName} {args.OldValue} {args.NewValue}");
    }
}
```

<details>
<summary>Auto-Generated Code</summary>

```cs
    
public partial class TestObservable : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    private void InvokePropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    // ----- properties -----

    private int _id = (int)5;
    public int Id
    {
        get { return _id; }
        set
        {
            if (_id != value)
            {                    
                var callbackArgs = new PropertyCallbackArgs<int>("Id", _id, value);
                OnIdChanged(this, callbackArgs);
                if (callbackArgs.NewValue != callbackArgs.OldValue)
                {
                    _id = callbackArgs.NewValue;
                    InvokePropertyChanged(nameof(Id));
                }
            }
        }
    }
    
    private string _name = default(string);
    public string Name
    {
        get { return _name; }
        set
        {
            if (_name != value)
            {                    
                _name = value;
                InvokePropertyChanged(nameof(Name));
            }
        }
    }
    
    private string _description = default(string);
    public string Description
    {
        get { return _description; }
        set
        {
            if (_description != value)
            {                    
                _description = value;
                InvokePropertyChanged(nameof(Description));
            }
        }
    }
}
    
```

</details>

# Nuget

[https://www.nuget.org/packages/SlimObservable](https://www.nuget.org/packages/SlimObservable)

# License

The MIT License, Copyright (c) 2023 Pawel Krzywdzinski
