using SlimObservable;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var obj = new TestObservable();
            obj.PropertyChanged += Obj_PropertyChanged;
            obj.Id = 1;
        }

        private static void Obj_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine($"{e.PropertyName}");
        }
    }

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
}
