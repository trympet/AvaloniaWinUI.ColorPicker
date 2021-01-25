using Avalonia;

namespace AvaloniaWinUI.ColorPicker
{
    public partial class ColorPickerSlider
    {
        public ColorPickerHsvChannel ColorChannel
        {
            get => GetValue(ColorChannelProperty);
            set => SetValue(ColorChannelProperty, value);
        }

        public static readonly StyledProperty<ColorPickerHsvChannel> ColorChannelProperty =
            AvaloniaProperty.Register<ColorPickerSlider, ColorPickerHsvChannel>(nameof(ColorChannel), ColorPickerHsvChannel.Value);
    }

}
