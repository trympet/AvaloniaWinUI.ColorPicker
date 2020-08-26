using System;

using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Controls;
using Avalonia.Styling;

namespace Avalonia.ColorPicker
{
    public class ColorPickerSlider : Slider, IStyleable
    {
        public static readonly StyledProperty<ColorPickerHsvChannel> ColorChannelProperty =
            AvaloniaProperty.Register<ColorPickerSlider, ColorPickerHsvChannel>(nameof(ColorChannel), ColorPickerHsvChannel.Value);

        static ColorPickerSlider()
        {
            ValueProperty.Changed.AddClassHandler<ColorPickerSlider>(OnValueChanged);
        }

        private ToolTip? _toolTip;

        private static void OnValueChanged(ColorPickerSlider sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (sender._toolTip is ToolTip toolTip)
            {
                toolTip.Content = sender.GetToolTipString();

                // ToolTip doesn't currently provide any way to re-run its placement logic if its placement target moves,
                // so toggling IsEnabled induces it to do that without incurring any visual glitches.
                toolTip.IsEnabled = false;
                toolTip.IsEnabled = true;
            }
        }

        public ColorPickerHsvChannel ColorChannel
        {
            get => GetValue(ColorChannelProperty);
            set => SetValue(ColorChannelProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _toolTip = e.NameScope.Find<ToolTip>("ToolTip");
        }

        protected override void OnKeyDown(KeyEventArgs args)
        {
            if ((Orientation == Orientation.Horizontal &&
                    args.Key != Key.Left &&
                    args.Key != Key.Right) ||
                (Orientation == Orientation.Vertical &&
                    args.Key != Key.Up &&
                    args.Key != Key.Down))
            {
                base.OnKeyDown(args);
                return;
            }

            var parentColorPicker = GetParentColorPicker();

            if (parentColorPicker == null)
            {
                return;
            }
            var isControlDown = args.KeyModifiers.HasFlagCustom(KeyModifiers.Control);

            double minBound;
            double maxBound;

            var currentHsv = parentColorPicker.CurrentHsv;
            double currentAlpha = 0;

            switch (ColorChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    minBound = parentColorPicker.MinHue;
                    maxBound = parentColorPicker.MaxHue;
                    currentHsv.H = Value;
                    break;

                case ColorPickerHsvChannel.Saturation:
                    minBound = parentColorPicker.MinSaturation;
                    maxBound = parentColorPicker.MaxSaturation;
                    currentHsv.S = Value / 100;
                    break;

                case ColorPickerHsvChannel.Value:
                    minBound = parentColorPicker.MinValue;
                    maxBound = parentColorPicker.MaxValue;
                    currentHsv.V = Value / 100;
                    break;

                case ColorPickerHsvChannel.Alpha:
                    minBound = 0;
                    maxBound = 100;
                    currentAlpha = Value / 100;
                    break;

                default:
                    throw new NotSupportedException();
            }

            // FlowDirection is missed in Avalonia
            var shouldInvertHorizontalDirection = false;

            var direction =
                ((args.Key == Key.Left && !shouldInvertHorizontalDirection) ||
                    (args.Key == Key.Right && shouldInvertHorizontalDirection) ||
                    args.Key == Key.Up) ?
                IncrementDirection.Lower :
                IncrementDirection.Higher;

            var amount = isControlDown ? IncrementAmount.Large : IncrementAmount.Small;

            if (ColorChannel != ColorPickerHsvChannel.Alpha)
            {
                currentHsv = ColorHelpers.IncrementColorChannel(currentHsv, ColorChannel, direction, amount, false /* shouldWrap */, minBound, maxBound);
            }
            else
            {
                currentAlpha = ColorHelpers.IncrementAlphaChannel(currentAlpha, direction, amount, false /* shouldWrap */, minBound, maxBound);
            }

            switch (ColorChannel)
            {
                case ColorPickerHsvChannel.Hue:
                    Value = currentHsv.H;
                    break;

                case ColorPickerHsvChannel.Saturation:
                    Value = currentHsv.S * 100;
                    break;

                case ColorPickerHsvChannel.Value:
                    Value = currentHsv.V * 100;
                    break;

                case ColorPickerHsvChannel.Alpha:
                    Value = currentAlpha * 100;
                    break;

                default:
                    throw new NotSupportedException();
            }

            args.Handled = true;
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            if (_toolTip != null)
            {
                _toolTip.Content = GetToolTipString();
                _toolTip.IsEnabled = true;
                ToolTip.SetIsOpen(this, true);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (_toolTip != null)
            {
                ToolTip.SetIsOpen(this, false);
            }
        }

        private ColorPicker GetParentColorPicker()
        {
            return this.FindAncestorOfType<ColorPicker>();
        }

        private string GetToolTipString()
        {
            var sliderValue = (uint)Math.Round(Value);

            if (ColorChannel == ColorPickerHsvChannel.Alpha)
            {
                return string.Format(LocalizedStrings.ToolTipStringAlphaSlider, sliderValue);
            }
            else
            {
                var parentColorPicker = GetParentColorPicker();
                if (parentColorPicker != null)
                {
                    var currentHsv = parentColorPicker.CurrentHsv;
                    string localizedString;

                    switch (ColorChannel)
                    {
                        case ColorPickerHsvChannel.Hue:
                            currentHsv.H = Value;
                            localizedString = LocalizedStrings.ToolTipStringHueSliderWithColorName;
                            break;

                        case ColorPickerHsvChannel.Saturation:
                            localizedString = LocalizedStrings.ToolTipStringSaturationSliderWithColorName;
                            currentHsv.S = Value / 100;
                            break;

                        case ColorPickerHsvChannel.Value:
                            localizedString = LocalizedStrings.ToolTipStringValueSliderWithColorName;
                            currentHsv.V = Value / 100;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    return string.Format(
                        localizedString,
                        sliderValue,
                        ColorHelpers.ToDisplayName(ColorHelpers.ColorFromRgba(ColorHelpers.HsvToRgb(currentHsv))));
                }
                else
                {
                    string localizedString;
                    switch (ColorChannel)
                    {
                        case ColorPickerHsvChannel.Hue:
                            localizedString = LocalizedStrings.ToolTipStringHueSliderWithoutColorName;
                            break;
                        case ColorPickerHsvChannel.Saturation:
                            localizedString = LocalizedStrings.ToolTipStringSaturationSliderWithoutColorName;
                            break;
                        case ColorPickerHsvChannel.Value:
                            localizedString = LocalizedStrings.ToolTipStringValueSliderWithoutColorName;
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    return string.Format(
                        localizedString,
                        sliderValue);
                }
            }
        }
    }
}
