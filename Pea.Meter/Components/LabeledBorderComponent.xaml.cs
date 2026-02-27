using Microsoft.Maui.Controls.Shapes;

namespace Pea.Meter.Components;

public partial class LabeledBorder : Grid
{
    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(LabeledBorder), string.Empty);

    public static readonly BindableProperty InnerContentProperty =
        BindableProperty.Create(nameof(InnerContent), typeof(Microsoft.Maui.Controls.View), typeof(LabeledBorder), null,
            propertyChanged: OnInnerContentChanged);

    public static readonly BindableProperty StrokeProperty =
        BindableProperty.Create(nameof(Stroke), typeof(Color), typeof(LabeledBorder), Colors.Gray);

    public static readonly BindableProperty StrokeThicknessProperty =
        BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(LabeledBorder), 1.0);

    public static readonly BindableProperty LabelForegroundProperty =
        BindableProperty.Create(nameof(LabelForeground), typeof(Color), typeof(LabeledBorder), Colors.Black);

    public static readonly BindableProperty BackgroundColorProperty =
        BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(LabeledBorder), Colors.Transparent);

    public static readonly BindableProperty CornerRadiusProperty =
        BindableProperty.Create(nameof(CornerRadius), typeof(float), typeof(LabeledBorder), 8f,
            propertyChanged: OnCornerRadiusChanged);

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public Microsoft.Maui.Controls.View? InnerContent
    {
        get => (Microsoft.Maui.Controls.View?)GetValue(InnerContentProperty);
        set => SetValue(InnerContentProperty, value);
    }

    public Color Stroke
    {
        get => (Color)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public Color LabelForeground
    {
        get => (Color)GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public float CornerRadius
    {
        get => (float)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public LabeledBorder()
    {
        InitializeComponent();
        ApplyCornerRadius();
    }

    private static void OnInnerContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LabeledBorder control && newValue is Microsoft.Maui.Controls.View contentView && control.ContentGrid != null)
        {
            if (control.ContentGrid.Children.Count > 1)
            {
                control.ContentGrid.Children.RemoveAt(1);
            }
            control.ContentGrid.Add(contentView);
            Grid.SetRow(contentView, 1);
        }
    }

    private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LabeledBorder control)
            control.ApplyCornerRadius();
    }

    private void ApplyCornerRadius()
    {
        if (InnerBorder != null)
        {
            InnerBorder.StrokeShape = new RoundRectangle { CornerRadius = CornerRadius };
        }
    }
}
