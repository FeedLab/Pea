using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Pea.Meter.Controls
{
    public class StrokedLabel : GraphicsView
    {
        private readonly StrokedTextDrawable drawable;

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(StrokedLabel), string.Empty, propertyChanged: OnChanged);

        public static readonly BindableProperty FillColorProperty =
            BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(StrokedLabel), Colors.White, propertyChanged: OnChanged);

        public static readonly BindableProperty StrokeColorProperty =
            BindableProperty.Create(nameof(StrokeColor), typeof(Color), typeof(StrokedLabel), Colors.Black, propertyChanged: OnChanged);

        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(float), typeof(StrokedLabel), 32f, propertyChanged: OnChanged);

        public static readonly BindableProperty StrokeWidthProperty =
            BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(StrokedLabel), 2f, propertyChanged: OnChanged);

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public Color FillColor { get => (Color)GetValue(FillColorProperty); set => SetValue(FillColorProperty, value); }
        public Color StrokeColor { get => (Color)GetValue(StrokeColorProperty); set => SetValue(StrokeColorProperty, value); }
        public float FontSize { get => (float)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public float StrokeWidth { get => (float)GetValue(StrokeWidthProperty); set => SetValue(StrokeWidthProperty, value); }

        public StrokedLabel()
        {
            drawable = new StrokedTextDrawable();
            Drawable = drawable;
        }

        private static void OnChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (StrokedLabel)bindable;
            control.drawable.Text = control.Text;
            control.drawable.FillColor = control.FillColor;
            control.drawable.StrokeColor = control.StrokeColor;
            control.drawable.FontSize = control.FontSize;
            control.drawable.StrokeWidth = control.StrokeWidth;
            control.Invalidate();
        }
    }

    public class StrokedTextDrawable : IDrawable
    {
        public string Text { get; set; } = string.Empty;
        public Color FillColor { get; set; } = Colors.White;
        public Color StrokeColor { get; set; } = Colors.Black;
        public float FontSize { get; set; } = 32;
        public float StrokeWidth { get; set; } = 2;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontSize = FontSize;

            // Draw stroke by drawing text multiple times slightly offset
            canvas.FontColor = StrokeColor;
            for (float dx = -StrokeWidth; dx <= StrokeWidth; dx++)
            {
                for (float dy = -StrokeWidth; dy <= StrokeWidth; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    canvas.DrawString(Text, dirtyRect.X + dx, dirtyRect.Y + dy, dirtyRect.Width, dirtyRect.Height,
                        HorizontalAlignment.Center, VerticalAlignment.Center);
                }
            }

            // Draw fill
            canvas.FontColor = FillColor;
            canvas.DrawString(Text, dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}