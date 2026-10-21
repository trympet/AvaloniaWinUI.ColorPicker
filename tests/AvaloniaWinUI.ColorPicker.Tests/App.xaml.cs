using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

namespace AvaloniaWinUI.ColorPicker.Tests
{
    public class App : Application
    {
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                .LogToTrace();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
