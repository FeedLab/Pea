using Microsoft.Maui.Controls.Shapes;

namespace Pea.Meter.Components;

public partial class CompassDirectionComponent : Border
{
    private static readonly Color DefaultButtonBackgroundColor = Color.FromArgb("#DCDCDC");
    private static readonly Color DefaultPressedBackgroundColor = Color.FromArgb("#A0A0A0");

    private static readonly (int row, int col, double rotation, int degrees)[] DirectionMap =
    [
        (0, 0, 315, 315), // NW
        (0, 1,   0,   0), // N
        (0, 2,  45,  45), // NE
        (1, 0, 270, 270), // W
        (1, 2,  90,  90), // E
        (2, 0, 225, 225), // SW
        (2, 1, 180, 180), // S
        (2, 2, 135, 135), // SE
    ];

    private readonly Dictionary<int, Border> borders = new();
    private readonly Dictionary<int, Label> arrows = new();
    private Border centerBorder = null!;
    private Label centerLabel = null!;
    private CancellationTokenSource? _spinCts;

    public static readonly BindableProperty LabelTextProperty =
        BindableProperty.Create(nameof(LabelText), typeof(string), typeof(CompassDirectionComponent), string.Empty);

    public static readonly BindableProperty LabelTextColorProperty =
        BindableProperty.Create(nameof(LabelTextColor), typeof(Color), typeof(CompassDirectionComponent), Colors.Black);

    public static readonly BindableProperty ButtonBackgroundColorProperty =
        BindableProperty.Create(nameof(ButtonBackgroundColor), typeof(Color), typeof(CompassDirectionComponent),
            DefaultButtonBackgroundColor,
            propertyChanged: (b, _, newValue) =>
            {
                var control = (CompassDirectionComponent)b;
                var color = (Color)newValue;
                foreach (var (deg, border) in control.borders)
                    if (deg != control.SelectedDirection)
                        border.BackgroundColor = color;
            });

    public static readonly BindableProperty PressAnimationDurationProperty =
        BindableProperty.Create(nameof(PressAnimationDuration), typeof(uint), typeof(CompassDirectionComponent), 1000u);

    public static readonly BindableProperty RaiseAnimationDurationProperty =
        BindableProperty.Create(nameof(RaiseAnimationDuration), typeof(uint), typeof(CompassDirectionComponent), 300u);

    public static readonly BindableProperty CenterBackgroundColorProperty =
        BindableProperty.Create(nameof(CenterBackgroundColor), typeof(Color), typeof(CompassDirectionComponent),
            Colors.Transparent,
            propertyChanged: (b, _, newValue) => ((CompassDirectionComponent)b).centerBorder.BackgroundColor = (Color)newValue);

    public static readonly BindableProperty CenterImageColorProperty =
        BindableProperty.Create(nameof(CenterImageColor), typeof(Color), typeof(CompassDirectionComponent), Colors.DarkSlateGray,
            propertyChanged: (b, _, newValue) => ((CompassDirectionComponent)b).centerLabel.TextColor = (Color)newValue);

    public static readonly BindableProperty PressedBackgroundColorProperty =
        BindableProperty.Create(nameof(PressedBackgroundColor), typeof(Color), typeof(CompassDirectionComponent),
            DefaultPressedBackgroundColor,
            propertyChanged: (b, _, newValue) =>
            {
                var control = (CompassDirectionComponent)b;
                if (control.SelectedDirection >= 0 && control.borders.TryGetValue(control.SelectedDirection, out var selected))
                    selected.BackgroundColor = (Color)newValue;
            });

    public static readonly BindableProperty ArrowIconScaleProperty =
        BindableProperty.Create(nameof(ArrowIconScale), typeof(double), typeof(CompassDirectionComponent), 0.8,
            propertyChanged: (b, _, _) =>
            {
                var control = (CompassDirectionComponent)b;
                foreach (var (deg, border) in control.borders)
                {
                    var size = Math.Min(border.Width, border.Height);
                    if (size > 0 && control.arrows.TryGetValue(deg, out var arrow))
                        arrow.FontSize = size * control.ArrowIconScale;
                }
            });

    public static readonly BindableProperty CenterIconScaleProperty =
        BindableProperty.Create(nameof(CenterIconScale), typeof(double), typeof(CompassDirectionComponent), 0.8);

    public static readonly BindableProperty CenterRotationSpeedProperty =
        BindableProperty.Create(nameof(CenterRotationSpeed), typeof(uint), typeof(CompassDirectionComponent), 3000u,
            propertyChanged: (b, _, _) => ((CompassDirectionComponent)b).UpdateCenterRotation());

    public static readonly BindableProperty ArrowColorProperty =
        BindableProperty.Create(nameof(ArrowColor), typeof(Color), typeof(CompassDirectionComponent), Colors.Blue,
            propertyChanged: (b, _, newValue) =>
            {
                foreach (var lbl in ((CompassDirectionComponent)b).arrows.Values)
                    lbl.TextColor = (Color)newValue;
            });

    public static readonly BindableProperty SelectedDirectionProperty =
        BindableProperty.Create(nameof(SelectedDirection), typeof(int), typeof(CompassDirectionComponent), -1,
            propertyChanged: OnSelectedDirectionChanged);

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public Color LabelTextColor
    {
        get => (Color)GetValue(LabelTextColorProperty);
        set => SetValue(LabelTextColorProperty, value);
    }

    public Color ButtonBackgroundColor
    {
        get => (Color)GetValue(ButtonBackgroundColorProperty);
        set => SetValue(ButtonBackgroundColorProperty, value);
    }

    public uint PressAnimationDuration
    {
        get => (uint)GetValue(PressAnimationDurationProperty);
        set => SetValue(PressAnimationDurationProperty, value);
    }

    public uint RaiseAnimationDuration
    {
        get => (uint)GetValue(RaiseAnimationDurationProperty);
        set => SetValue(RaiseAnimationDurationProperty, value);
    }

    public Color CenterBackgroundColor
    {
        get => (Color)GetValue(CenterBackgroundColorProperty);
        set => SetValue(CenterBackgroundColorProperty, value);
    }

    public Color CenterImageColor
    {
        get => (Color)GetValue(CenterImageColorProperty);
        set => SetValue(CenterImageColorProperty, value);
    }

    public Color PressedBackgroundColor
    {
        get => (Color)GetValue(PressedBackgroundColorProperty);
        set => SetValue(PressedBackgroundColorProperty, value);
    }

    public double ArrowIconScale
    {
        get => (double)GetValue(ArrowIconScaleProperty);
        set => SetValue(ArrowIconScaleProperty, value);
    }

    public double CenterIconScale
    {
        get => (double)GetValue(CenterIconScaleProperty);
        set => SetValue(CenterIconScaleProperty, value);
    }

    public uint CenterRotationSpeed
    {
        get => (uint)GetValue(CenterRotationSpeedProperty);
        set => SetValue(CenterRotationSpeedProperty, value);
    }

    public Color ArrowColor
    {
        get => (Color)GetValue(ArrowColorProperty);
        set => SetValue(ArrowColorProperty, value);
    }

    public int SelectedDirection
    {
        get => (int)GetValue(SelectedDirectionProperty);
        set => SetValue(SelectedDirectionProperty, value);
    }

    public event EventHandler<int>? DirectionChanged;

    public CompassDirectionComponent()
    {
        InitializeComponent();
        BuildButtons();
    }

    private void UpdateCenterRotation()
    {
        _spinCts?.Cancel();
        _spinCts = null;
        centerLabel.AbortAnimation("CenterSpin");

        if (CenterRotationSpeed == 0) return;

        _spinCts = new CancellationTokenSource();
        SpinStep(_spinCts.Token);
    }

    private void SpinStep(CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return;
        var start = centerLabel.Rotation;
        new Animation(v => centerLabel.Rotation = start + v * 360, 0, 1)
            .Commit(centerLabel, "CenterSpin", 16, CenterRotationSpeed, Easing.Linear,
                finished: (_, cancelled) => { if (!cancelled) SpinStep(ct); });
    }

    private void BuildButtons()
    {
        centerLabel = new Label
        {
            Text = "\uf14e",
            FontFamily = "FontSolid",
            FontSize = 24,
            TextColor = CenterImageColor,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
        };

        centerLabel.Loaded += (_, _) => UpdateCenterRotation();

        var centerContent = new Grid();
        centerContent.Children.Add(centerLabel);

        centerBorder = new Border
        {
            IsEnabled = false,
            BackgroundColor = CenterBackgroundColor,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Margin = 3,
            Content = centerContent,
        };
        centerBorder.SizeChanged += (_, _) =>
        {
            var size = Math.Min(centerBorder.Width, centerBorder.Height);
            if (size > 0)
                centerLabel.FontSize = size * CenterIconScale;
        };

        Grid.SetRow(centerBorder, 1);
        Grid.SetColumn(centerBorder, 1);
        CompassGrid.Children.Add(centerBorder);

        foreach (var (row, col, rotation, degrees) in DirectionMap)
        {
            var (container, border, arrow) = CreateDirectionButton(rotation, degrees);
            Grid.SetRow(container, row);
            Grid.SetColumn(container, col);
            CompassGrid.Children.Add(container);
            borders[degrees] = border;
            arrows[degrees] = arrow;
        }
    }

    private (Grid container, Border border, Label arrow) CreateDirectionButton(double rotation, int degrees)
    {
        // Extrusion layers (back → front, offset to simulate depth)
        var arrowHost = new Grid
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Rotation = rotation,
            InputTransparent = true,
        };

        for (int i = 3; i >= 1; i--)
        {
            arrowHost.Children.Add(new Label
            {
                Text = "▲",
                TextColor = Color.FromRgba(0f, 0f, 0f, 0.18f * i),
                Margin = new Thickness(i, i, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                InputTransparent = true,
            });
        }

        var arrow = new Label
        {
            Text = "▲",
            TextColor = ArrowColor,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
        };
        arrowHost.Children.Add(arrow);

        var border = new Border
        {
            BackgroundColor = ButtonBackgroundColor,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Shadow = CreateRaisedShadow(),
            Content = arrowHost,
        };

        border.SizeChanged += (_, _) =>
        {
            var size = Math.Min(border.Width, border.Height);
            if (size > 0)
            {
                var fontSize = size * ArrowIconScale;
                foreach (var child in arrowHost.Children.OfType<Label>())
                    child.FontSize = fontSize;
            }
        };

        var container = new Grid
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Margin = 3,
        };
        container.Children.Add(border);

        var d = degrees;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => SelectedDirection = d;
        container.GestureRecognizers.Add(tap);

        return (container, border, arrow);
    }

    private static Shadow CreateRaisedShadow() => new Shadow
    {
        Brush = new SolidColorBrush(Colors.Black),
        Offset = new Point(3, 3),
        Radius = 5,
        Opacity = 0.35f,
    };

    private static void OnSelectedDirectionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (CompassDirectionComponent)bindable;
        var oldDeg = (int)oldValue;
        var newDeg = (int)newValue;

        if (oldDeg >= 0 && control.borders.TryGetValue(oldDeg, out var oldBorder))
            AnimateRaise(oldBorder, control.ButtonBackgroundColor, control.PressedBackgroundColor, control.RaiseAnimationDuration);

        if (newDeg >= 0 && control.borders.TryGetValue(newDeg, out var newBorder))
        {
            if (oldDeg < 0)
                ApplySunkenImmediate(newBorder, control.PressedBackgroundColor);
            else
                AnimateSink(newBorder, control.ButtonBackgroundColor, control.PressedBackgroundColor, control.PressAnimationDuration);
        }

        control.DirectionChanged?.Invoke(control, newDeg);
    }

    private static void ApplySunkenImmediate(Border border, Color sunkenColor)
    {
        border.AbortAnimation("Raise");
        border.AbortAnimation("Sink");
        border.Shadow = null;
        border.BackgroundColor = sunkenColor;
        border.Scale = 0.92;
    }

    private static void AnimateSink(Border border, Color raisedColor, Color sunkenColor, uint duration)
    {
        border.AbortAnimation("Raise");
        border.Shadow = null;

        var fromR = raisedColor.Red; var fromG = raisedColor.Green; var fromB = raisedColor.Blue;
        var toR = sunkenColor.Red;   var toG = sunkenColor.Green;   var toB = sunkenColor.Blue;
        var fromScale = (float)border.Scale;

        new Animation(t =>
        {
            border.BackgroundColor = new Color(
                (float)(fromR + (toR - fromR) * t),
                (float)(fromG + (toG - fromG) * t),
                (float)(fromB + (toB - fromB) * t));
            border.Scale = fromScale - 0.08 * t;
        }, 0, 1).Commit(border, "Sink", 16, duration, Easing.CubicOut);
    }

    private static void AnimateRaise(Border border, Color raisedColor, Color sunkenColor, uint duration)
    {
        border.AbortAnimation("Sink");
        border.Shadow = CreateRaisedShadow();

        var fromR = sunkenColor.Red; var fromG = sunkenColor.Green; var fromB = sunkenColor.Blue;
        var toR = raisedColor.Red;   var toG = raisedColor.Green;   var toB = raisedColor.Blue;
        var fromScale = (float)border.Scale;

        new Animation(t =>
        {
            border.BackgroundColor = new Color(
                (float)(fromR + (toR - fromR) * t),
                (float)(fromG + (toG - fromG) * t),
                (float)(fromB + (toB - fromB) * t));
            border.Scale = fromScale + (float)(1.0 - fromScale) * (float)t;
        }, 0, 1).Commit(border, "Raise", 16, duration, Easing.CubicOut);
    }
}
