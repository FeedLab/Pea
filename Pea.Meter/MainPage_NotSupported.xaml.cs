using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Pea.Meter
{
    public partial class MainPageNotSupported : ContentPage
    {
        public MainPageNotSupported()
        {
            InitializeComponent();

            Title = Pea.Meter.Resources.Strings.AppResources.MainTitle;
        }
    }

    public class ScaledLabel : Label
    {
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (string.IsNullOrEmpty(Text) || width <= 0)
                return;

            double fontSize = FontSize > 0 ? FontSize : 14;

            var size = Measure(double.PositiveInfinity, double.PositiveInfinity);

            // Shrink until it fits
            while (size.Width > width && fontSize > 1)
            {
                fontSize -= 1;
                FontSize = fontSize;

                // Re-measure after changing font size
                size = Measure(double.PositiveInfinity, double.PositiveInfinity);
            }
        }
    }
}