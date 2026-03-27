namespace Pea.Meter.Components;

public partial class DatePickerComponent : ContentView
{
    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(nameof(Label), typeof(string), typeof(DatePickerComponent), string.Empty);

    public static readonly BindableProperty HeaderTextProperty =
        BindableProperty.Create(nameof(HeaderText), typeof(string), typeof(DatePickerComponent), string.Empty);

    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(nameof(SelectedDate), typeof(DateTime), typeof(DatePickerComponent), DateTime.Now, BindingMode.TwoWay);

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime), typeof(DatePickerComponent), DateTime.MinValue);

    public static readonly BindableProperty MaximumDateProperty =
        BindableProperty.Create(nameof(MaximumDate), typeof(DateTime), typeof(DatePickerComponent), DateTime.MaxValue);

    public static readonly BindableProperty GlyphProperty =
        BindableProperty.Create(nameof(Glyph), typeof(string), typeof(DatePickerComponent), "&#x1F4C6;");

    public static readonly BindableProperty TextColorGlyphProperty =
        BindableProperty.Create(nameof(TextColorGlyph), typeof(Color), typeof(DatePickerComponent), Colors.Black);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public DateTime SelectedDate
    {
        get => (DateTime)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public DateTime MinimumDate
    {
        get => (DateTime)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public DateTime MaximumDate
    {
        get => (DateTime)GetValue(MaximumDateProperty);
        set => SetValue(MaximumDateProperty, value);
    }

    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public Color TextColorGlyph
    {
        get => (Color)GetValue(TextColorGlyphProperty);
        set => SetValue(TextColorGlyphProperty, value);
    }

    public event EventHandler<DateTime>? DateSelected;

    public DatePickerComponent()
    {
        InitializeComponent();
    }

    private void OnDateTapGestureTapped(object? sender, TappedEventArgs e)
    {
        DatePicker.IsOpen = true;
    }

    private void OnDatePickerOkButtonClicked(object? sender, EventArgs e)
    {
        if (DatePicker.SelectedDate == null)
            return;

        SelectedDate = DatePicker.SelectedDate.Value;
        DateSelected?.Invoke(this, SelectedDate);
    }
}
