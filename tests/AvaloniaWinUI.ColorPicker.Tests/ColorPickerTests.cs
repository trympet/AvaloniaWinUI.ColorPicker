using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

using Xunit;

namespace AvaloniaWinUI.ColorPicker.Tests
{
    public class ColorPickerTests
    {
        [AvaloniaFact]
        public void ColorPickerTest()
        {
            var colorPicker = new ColorPicker();
            Assert.NotNull(colorPicker);

            Assert.Equal(Colors.White, colorPicker.Color);
            Assert.Null(colorPicker.PreviousColor);
            Assert.False(colorPicker.IsAlphaEnabled);
            Assert.True(colorPicker.IsColorSpectrumVisible);
            Assert.True(colorPicker.IsColorPreviewVisible);
            Assert.True(colorPicker.IsColorSliderVisible);
            Assert.True(colorPicker.IsAlphaSliderVisible);
            Assert.False(colorPicker.IsMoreButtonVisible);
            Assert.True(colorPicker.IsColorChannelTextInputVisible);
            Assert.True(colorPicker.IsAlphaTextInputVisible);
            Assert.True(colorPicker.IsHexInputVisible);
            Assert.Equal(0, colorPicker.MinHue);
            Assert.Equal(359, colorPicker.MaxHue);
            Assert.Equal(0, colorPicker.MinSaturation);
            Assert.Equal(100, colorPicker.MaxSaturation);
            Assert.Equal(0, colorPicker.MinValue);
            Assert.Equal(100, colorPicker.MaxValue);
            Assert.Equal(ColorSpectrumShape.Box, colorPicker.ColorSpectrumShape);
            Assert.Equal(ColorSpectrumComponents.HueSaturation, colorPicker.ColorSpectrumComponents);

            // Clamping the min and max properties changes the color value,
            // so let's test this new value before we change those.
            colorPicker.Color = Colors.Green;
            Assert.Equal(Colors.Green, colorPicker.Color);

            colorPicker.PreviousColor = Colors.Red;
            colorPicker.IsAlphaEnabled = true;
            colorPicker.IsColorSpectrumVisible = false;
            colorPicker.IsColorPreviewVisible = false;
            colorPicker.IsColorSliderVisible = false;
            colorPicker.IsAlphaSliderVisible = false;
            colorPicker.IsMoreButtonVisible = true;
            colorPicker.IsColorChannelTextInputVisible = false;
            colorPicker.IsAlphaTextInputVisible = false;
            colorPicker.IsHexInputVisible = false;
            colorPicker.MinHue = 10;
            colorPicker.MaxHue = 300;
            colorPicker.MinSaturation = 10;
            colorPicker.MaxSaturation = 90;
            colorPicker.MinValue = 10;
            colorPicker.MaxValue = 90;
            colorPicker.ColorSpectrumShape = ColorSpectrumShape.Ring;
            colorPicker.ColorSpectrumComponents = ColorSpectrumComponents.HueValue;

            Assert.NotEqual(Colors.Green, colorPicker.Color);
            Assert.Equal(Colors.Red, colorPicker.PreviousColor);
            Assert.True(colorPicker.IsAlphaEnabled);
            Assert.False(colorPicker.IsColorSpectrumVisible);
            Assert.False(colorPicker.IsColorPreviewVisible);
            Assert.False(colorPicker.IsColorSliderVisible);
            Assert.False(colorPicker.IsAlphaSliderVisible);
            Assert.True(colorPicker.IsMoreButtonVisible);
            Assert.False(colorPicker.IsColorChannelTextInputVisible);
            Assert.False(colorPicker.IsAlphaTextInputVisible);
            Assert.False(colorPicker.IsHexInputVisible);
            Assert.Equal(10, colorPicker.MinHue);
            Assert.Equal(300, colorPicker.MaxHue);
            Assert.Equal(10, colorPicker.MinSaturation);
            Assert.Equal(90, colorPicker.MaxSaturation);
            Assert.Equal(10, colorPicker.MinValue);
            Assert.Equal(90, colorPicker.MaxValue);
            Assert.Equal(ColorSpectrumShape.Ring, colorPicker.ColorSpectrumShape);
            Assert.Equal(ColorSpectrumComponents.HueValue, colorPicker.ColorSpectrumComponents);
        }

        [AvaloniaFact]
        public void ColorPickerEventsTest()
        {
            var colorPicker = new ColorPicker();

            colorPicker.ColorChanged += (object? sender, ColorChangedEventArgs args) =>
            {
                Assert.Equal(args.OldColor, Colors.White);
                Assert.Equal(args.NewColor, Colors.Green);
            };

            colorPicker.Color = Colors.Green;
        }

        [AvaloniaFact]
        public void ColorSpectrumTest()
        {
            var colorSpectrum = new ColorSpectrum();
            Assert.NotNull(colorSpectrum);

            Assert.Equal(Colors.White, colorSpectrum.Color);
            Assert.Equal(new Vector4() { X = 0.0f, Y = 0.0f, Z = 1.0f, W = 1.0f }, colorSpectrum.HsvColor);
            Assert.Equal(0, colorSpectrum.MinHue);
            Assert.Equal(359, colorSpectrum.MaxHue);
            Assert.Equal(0, colorSpectrum.MinSaturation);
            Assert.Equal(100, colorSpectrum.MaxSaturation);
            Assert.Equal(0, colorSpectrum.MinValue);
            Assert.Equal(100, colorSpectrum.MaxValue);
            Assert.Equal(ColorSpectrumShape.Box, colorSpectrum.Shape);
            Assert.Equal(ColorSpectrumComponents.HueSaturation, colorSpectrum.Components);

            colorSpectrum.Color = Colors.Green;
            colorSpectrum.MinHue = 10;
            colorSpectrum.MaxHue = 300;
            colorSpectrum.MinSaturation = 10;
            colorSpectrum.MaxSaturation = 90;
            colorSpectrum.MinValue = 10;
            colorSpectrum.MaxValue = 90;
            colorSpectrum.Shape = ColorSpectrumShape.Ring;
            colorSpectrum.Components = ColorSpectrumComponents.HueValue;

            Assert.Equal(Colors.Green, colorSpectrum.Color);

            // We'll probably encounter some level of rounding error here,
            // so we want to check that the HSV color is *close* to what's expected,
            // not exactly equal.
            Assert.True(Math.Abs(colorSpectrum.HsvColor.X - 120.0) < 0.1);
            Assert.True(Math.Abs(colorSpectrum.HsvColor.Y - 1.0) < 0.1);
            Assert.True(Math.Abs(colorSpectrum.HsvColor.Z - 0.5) < 0.1);

            Assert.Equal(10, colorSpectrum.MinHue);
            Assert.Equal(300, colorSpectrum.MaxHue);
            Assert.Equal(10, colorSpectrum.MinSaturation);
            Assert.Equal(90, colorSpectrum.MaxSaturation);
            Assert.Equal(10, colorSpectrum.MinValue);
            Assert.Equal(90, colorSpectrum.MaxValue);
            Assert.Equal(ColorSpectrumShape.Ring, colorSpectrum.Shape);
            Assert.Equal(ColorSpectrumComponents.HueValue, colorSpectrum.Components);

            colorSpectrum.HsvColor = new Vector4() { X = 120.0f, Y = 1.0f, Z = 1.0f, W = 1.0f };

            Assert.Equal(Color.FromArgb(255, 0, 255, 0), colorSpectrum.Color);
            Assert.Equal(new Vector4() { X = 120.0f, Y = 1.0f, Z = 1.0f, W = 1.0f }, colorSpectrum.HsvColor);
        }

        [AvaloniaFact]
        public void ValidateHueRange()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var colorPicker = new ColorPicker
                {
                    MinHue = -1
                };
            });
        }

        [AvaloniaFact]
        public async Task VerifyClearingHexInputFieldDoesNotCrash()
        {
            var colorPicker = new ColorPicker
            {
                IsHexInputVisible = true
            };

            var window = new Window
            {
                Content = colorPicker
            };
            window.Show();

            await WaitForLoadedAsync(colorPicker).ConfigureAwait(true);

            var hexTextBox = FindVisualChildren<TextBox>(colorPicker, "PART_HexTextBox").FirstOrDefault();
            Assert.NotNull(hexTextBox);

            Assert.False(string.IsNullOrEmpty(hexTextBox!.Text), "Hex TextBox should have not been empty.");

            // Clearing the hex input field should not crash the app.
            hexTextBox.Text = "";
        }

        [AvaloniaFact]
        public async Task VerifyClearingAlphaChannelInputFieldDoesNotCrash()
        {
            var colorPicker = new ColorPicker
            {
                IsAlphaEnabled = true
            };

            var window = new Window
            {
                Content = colorPicker
            };
            window.Show();

            await WaitForLoadedAsync(colorPicker).ConfigureAwait(true);

            var alphaChannelTextBox = FindVisualChildren<TextBox>(colorPicker, "PART_AlphaTextBox").FirstOrDefault();
            Assert.NotNull(alphaChannelTextBox);

            Assert.False(string.IsNullOrEmpty(alphaChannelTextBox!.Text), "Alpha channel TextBox should have not been empty.");

            // Clearing the alpha channel input field should not crash the app.
            alphaChannelTextBox.Text = "";
        }

        [AvaloniaFact]
        public async Task VerifyVisualTree()
        {
            var colorPicker = new ColorPicker { Name = "PART_ColorPicker", IsAlphaEnabled = true, Width = 300, Height = 600 };

            var page = new ContentControl
            {
                Content = colorPicker
            };
            var window = new Window
            {
                Content = page
            };
            window.Show();

            await WaitForLoadedAsync(colorPicker).ConfigureAwait(true);

            var control = FindVisualChildren<ColorPicker>(window, "PART_ColorPicker").FirstOrDefault();
            Assert.Equal(colorPicker, control);
        }

        [AvaloniaFact]
        public async Task VerifySpectrumFocusToolTip()
        {
            var colorPicker = new ColorPicker();
            var focusTarget = new Button { Content = "Focus target" };
            var window = new Window
            {
                Content = new StackPanel
                {
                    Children = { colorPicker, focusTarget }
                }
            };

            try
            {
                window.Show();
                await WaitForLoadedAsync(colorPicker).ConfigureAwait(true);

                var spectrum = FindVisualChildren<ColorSpectrum>(colorPicker, "PART_ColorSpectrum").Single();
                var selectionEllipse = FindVisualChildren<Control>(spectrum, "PART_SelectionEllipse").Single();
                spectrum.Focusable = true;

                Assert.True(focusTarget.Focus());
                Assert.True(spectrum.Focus());
                Assert.True(ToolTip.GetIsOpen(selectionEllipse));

                Assert.True(focusTarget.Focus());
                Assert.False(ToolTip.GetIsOpen(selectionEllipse));
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(ColorSpectrumShape.Box, false)]
        [InlineData(ColorSpectrumShape.Box, true)]
        [InlineData(ColorSpectrumShape.Ring, false)]
        [InlineData(ColorSpectrumShape.Ring, true)]
        public async Task VerifyTemplatesAcrossThemesAndSpectrumShapes(ColorSpectrumShape shape, bool useDarkTheme)
        {
            var colorPicker = new ColorPicker
            {
                ColorSpectrumShape = shape,
                IsAlphaEnabled = true
            };
            var window = new Window
            {
                Content = colorPicker,
                RequestedThemeVariant = useDarkTheme ? ThemeVariant.Dark : ThemeVariant.Light
            };

            try
            {
                window.Show();
                await WaitForLoadedAsync(colorPicker).ConfigureAwait(true);

                var spectrum = FindVisualChildren<ColorSpectrum>(colorPicker, "PART_ColorSpectrum").Single();
                var shapeName = shape == ColorSpectrumShape.Box ? "PART_SpectrumRectangle" : "PART_SpectrumEllipse";
                var spectrumShape = FindVisualChildren<Shape>(spectrum, shapeName).Single();

                Assert.Equal(shape, spectrum.Shape);
                Assert.True(spectrumShape.IsVisible);
                Assert.NotNull(Assert.IsType<ImageBrush>(spectrumShape.Fill).Source);

                colorPicker.Color = Colors.Blue;
                Assert.Equal(Colors.Blue, spectrum.Color);

                window.RequestedThemeVariant = useDarkTheme ? ThemeVariant.Light : ThemeVariant.Dark;
                Assert.Equal(window.RequestedThemeVariant, colorPicker.ActualThemeVariant);
                Assert.NotNull(Assert.IsType<ImageBrush>(spectrumShape.Fill).Source);
            }
            finally
            {
                window.Close();
            }
        }

        [AvaloniaTheory]
        [InlineData(ColorSpectrumShape.Box)]
        [InlineData(ColorSpectrumShape.Ring)]
        public async Task VerifySpectrumWithReversedBounds(ColorSpectrumShape shape)
        {
            var spectrum = new ColorSpectrum
            {
                Width = 256,
                Height = 256,
                Shape = shape,
                MinHue = 200,
                MaxHue = 100,
                MinSaturation = 80,
                MaxSaturation = 20,
                MinValue = 80,
                MaxValue = 20
            };
            var window = new Window { Content = spectrum };

            try
            {
                window.Show();
                await WaitForLoadedAsync(spectrum).ConfigureAwait(true);

                var shapeName = shape == ColorSpectrumShape.Box ? "PART_SpectrumRectangle" : "PART_SpectrumEllipse";
                var spectrumShape = FindVisualChildren<Shape>(spectrum, shapeName).Single();
                Assert.NotNull(Assert.IsType<ImageBrush>(spectrumShape.Fill).Source);
            }
            finally
            {
                window.Close();
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(Visual depObj, string name)
            where T : Visual
        {
            if (depObj == null)
            {
                yield break;
            }

            foreach (var child in depObj.GetVisualChildren())
            {
                if (child is T typedChild && typedChild.Name == name)
                {
                    yield return typedChild;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child, name))
                {
                    yield return childOfChild;
                }
            }
        }

        public static async Task WaitForLoadedAsync(Visual depObj)
        {
            if (depObj == null)
            {
                throw new ArgumentNullException(nameof(depObj));
            }

            if (depObj.IsAttachedToVisualTree())
            {
                await Task.Yield();
                return;
            }

            var taskCts = new TaskCompletionSource<bool>();

            depObj.AttachedToVisualTree += (s, a) => taskCts.SetResult(true);

            await taskCts.Task.ConfigureAwait(false);
            await Task.Yield();
        }
    }
}
