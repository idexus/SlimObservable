# Summary

A C# library to automatically generate observable properties declared in an interface.

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

# License

The MIT License, Copyright (c) 2023 Pawel Krzywdzinski