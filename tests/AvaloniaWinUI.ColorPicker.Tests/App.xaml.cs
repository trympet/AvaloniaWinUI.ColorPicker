using Avalonia;
using Avalonia.Markup.Xaml;

namespace AvaloniaWinUI.ColorPicker.Tests
{
    public class App : Application
    {
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
