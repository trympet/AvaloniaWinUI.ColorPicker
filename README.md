# AvaloniaWinUI.ColorPicker

A WinUI-style color picker for Avalonia.

## Requirements

- Avalonia 12
- The Avalonia Fluent theme

## Usage

Include the picker styles after the Fluent theme in your application:

```xml
<Application.Styles>
	<FluentTheme />
	<StyleInclude Source="avares://AvaloniaWinUI.ColorPicker/Themes/Fluent.xaml" />
</Application.Styles>
```

Add the control to a view:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:ui="using:AvaloniaWinUI.ColorPicker">
	<ui:ColorPicker Color="Red" IsAlphaEnabled="True" />
</UserControl>
```

Set the application's `RequestedThemeVariant` to switch between light and dark themes.

## Build and Test

```shell
dotnet build AvaloniaWinUI.ColorPicker.sln
dotnet test tests/AvaloniaWinUI.ColorPicker.Tests/AvaloniaWinUI.ColorPicker.Tests.csproj
dotnet run --project samples/AvaloniaWinUI.ColorPicker.Sample/AvaloniaWinUI.ColorPicker.Sample.csproj
```
