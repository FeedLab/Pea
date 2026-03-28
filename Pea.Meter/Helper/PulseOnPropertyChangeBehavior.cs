namespace Pea.Meter.Helper;

public class PulseOnPropertyChangeBehavior : Behavior<Label>
{
    public static readonly BindableProperty PropertyToWatchProperty =
        BindableProperty.Create(nameof(PropertyToWatch), typeof(object), typeof(PulseOnPropertyChangeBehavior), propertyChanged: OnPropertyToWatchChanged);

    public object PropertyToWatch
    {
        get => GetValue(PropertyToWatchProperty);
        set => SetValue(PropertyToWatchProperty, value);
    }

    public Color HighlightColor { get; set; } = Colors.DarkRed;
    public uint Length { get; set; } = 250;

    private static void OnPropertyToWatchChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var behavior = (PulseOnPropertyChangeBehavior)bindable;
        behavior.Animate(behavior.AssociatedObject);
    }

    protected override void OnAttachedTo(Label bindable)
    {
        base.OnAttachedTo(bindable);
        AssociatedObject = bindable;
        bindable.BindingContextChanged += OnBindingContextChanged;
        BindingContext = bindable.BindingContext;
    }

    protected override void OnDetachingFrom(Label bindable)
    {
        base.OnDetachingFrom(bindable);
        bindable.BindingContextChanged -= OnBindingContextChanged;
        AssociatedObject = null;
    }

    private void OnBindingContextChanged(object sender, EventArgs e)
    {
        BindingContext = ((BindableObject)sender).BindingContext;
    }

    public Label AssociatedObject { get; private set; }

    private async void Animate(Label label)
    {
        label.CancelAnimations();
        var originalColor = label.TextColor;

        await AnimateColor(label, originalColor, HighlightColor, Length);
        await AnimateColor(label, HighlightColor, originalColor, Length);
    }

    private static Task AnimateColor(Label label, Color from, Color to, uint length)
    {
        var tcs = new TaskCompletionSource<bool>();
        new Animation(t => label.TextColor = Color.FromRgba(
                from.Red   + (to.Red   - from.Red)   * t,
                from.Green + (to.Green - from.Green) * t,
                from.Blue  + (to.Blue  - from.Blue)  * t,
                from.Alpha + (to.Alpha - from.Alpha) * t))
            .Commit(label, "ColorAnim", 16, length, Easing.Linear, (_, _) => tcs.SetResult(true));
        return tcs.Task;
    }
}
