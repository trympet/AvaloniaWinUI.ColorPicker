using System;
using System.Globalization;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Themes.Fluent;
using Avalonia.VisualTree;

namespace AvaloniaWinUI.ColorPicker.Sample.Views
{
    public partial class ColorPickerPage : UserControl
    {
        private ToolTip? colorNameToolTip;
        private Rectangle? spectrumRectangle;
        private Control? colorSpectrumInputTarget;
        private Rectangle? previousColorRectangle;
        private Panel? selectionEllipsePanel;
        private Ellipse? selectionEllipse;
        private Button? moreButton;

        public ColorPickerPage()
        {
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            InitializeComponent(true);

            // Initialize the ColorPicker to a known default color so we have a known starting point.
            ColorPicker.Color = Colors.Red;
            ColorPicker.ColorChanged += ColorPickerColorChanged;
            ColorPicker.TemplateApplied += ColorPickerTemplateApplied;

            this.FindControl<ComboBox>("ColorSpectrumShapeComboBox").SelectionChanged += ColorSpectrumShapeComboBoxSelectionChanged;
            this.FindControl<ComboBox>("ColorSpectrumComponentsComboBox").SelectionChanged += ColorSpectrumComponentsComboBoxSelectionChanged;

            spectrumRectangle = null;
            colorSpectrumInputTarget = null;
            previousColorRectangle = null;
            moreButton = null;

            RedTextBlock.Text = ColorPicker.Color.R.ToString(CultureInfo.InvariantCulture);
            GreenTextBlock.Text = ColorPicker.Color.G.ToString(CultureInfo.InvariantCulture);
            BlueTextBlock.Text = ColorPicker.Color.B.ToString(CultureInfo.InvariantCulture);
            AlphaTextBlock.Text = ColorPicker.Color.A.ToString(CultureInfo.InvariantCulture);
        }

        private async void ColorPickerTemplateApplied(object? sender, TemplateAppliedEventArgs e)
        {
            await System.Threading.Tasks.Task.Yield();

            spectrumRectangle = FindVisualChildByName(ColorPicker, "PART_SpectrumRectangle") as Rectangle;

            if (spectrumRectangle != null)
            {
                spectrumRectangle.GetPropertyChangedObservable(Shape.FillProperty).AddClassHandler<Rectangle>(SpectrumRectangleFillChanged);
            }

            colorSpectrumInputTarget = FindVisualChildByName(ColorPicker, "PART_InputTarget") as Control;

            if (colorSpectrumInputTarget != null)
            {
                UpdateHeightFromInputTarget();
                colorSpectrumInputTarget.GetPropertyChangedObservable(Control.BoundsProperty).AddClassHandler<Control>(ColorSpectrumImageSizeChanged);
            }

            var comboBox = FindVisualChildByName(ColorPicker, "PART_ColorRepresentationComboBox") as ComboBox;

            previousColorRectangle = FindVisualChildByName(ColorPicker, "PART_PreviousColorRectangle") as Rectangle;

            if (previousColorRectangle != null)
            {
                previousColorRectangle.GetPropertyChangedObservable(Shape.FillProperty).AddClassHandler<Rectangle>(PreviousColorRectangleFillChanged);
            }

            selectionEllipsePanel = FindVisualChildByName(ColorPicker, "PART_SelectionEllipsePanel") as Panel;

            if (selectionEllipsePanel != null && previousColorRectangle != null)
            {
                previousColorRectangle!.GetPropertyChangedObservable(Canvas.LeftProperty).AddClassHandler<Panel>(SelectionEllipsePositionChanged);
                previousColorRectangle.GetPropertyChangedObservable(Canvas.TopProperty).AddClassHandler<Panel>(SelectionEllipsePositionChanged);

                UpdateSelectionEllipsePosition();
            }

            selectionEllipse = FindVisualChildByName(ColorPicker, "PART_SelectionEllipse") as Ellipse;

            if (selectionEllipse != null)
            {
                selectionEllipse.GetPropertyChangedObservable(Ellipse.StrokeProperty).AddClassHandler<Ellipse>(SelectionEllipseStrokeChanged);

                UpdateSelectionEllipseColor();

                colorNameToolTip = ToolTip.GetTip(selectionEllipse) as ToolTip;

                if (colorNameToolTip != null)
                {
                    colorNameToolTip.GetPropertyChangedObservable(ToolTip.ContentProperty).AddClassHandler<ToolTip>(ColorNameToolTipContentChanged);
                    UpdateSelectedColorName();
                }
            }
        }

        public IVisual? FindVisualChildByName(IVisual parent, string name)
        {
            if ((parent as Control)?.Name == name)
            {
                return parent;
            }

            foreach (var child in parent.GetVisualChildren())
            {
                var result = FindVisualChildByName(child, name);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public void SpectrumRectangleFillChanged(object o, AvaloniaPropertyChangedEventArgs p)
        {
            if (spectrumRectangle != null && spectrumRectangle.Fill != null)
            {
                ColorSpectrumLoadedCheckBox.IsChecked = true;
            }
        }

        public void PreviousColorRectangleFillChanged(object o, AvaloniaPropertyChangedEventArgs p)
        {
            if (previousColorRectangle != null)
            {
                var previousColorBrush = previousColorRectangle.Fill as SolidColorBrush;

                if (previousColorBrush == null)
                {
                    PreviousRedTextBlock.Text = "";
                    PreviousGreenTextBlock.Text = "";
                    PreviousBlueTextBlock.Text = "";
                    PreviousAlphaTextBlock.Text = "";
                }
                else
                {
                    var previousColor = previousColorBrush.Color;

                    PreviousRedTextBlock.Text = previousColor.R.ToString(CultureInfo.InvariantCulture);
                    PreviousGreenTextBlock.Text = previousColor.G.ToString(CultureInfo.InvariantCulture);
                    PreviousBlueTextBlock.Text = previousColor.B.ToString(CultureInfo.InvariantCulture);
                    PreviousAlphaTextBlock.Text = previousColor.A.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public void SelectionEllipseStrokeChanged(object o, AvaloniaPropertyChangedEventArgs p)
        {
            UpdateSelectionEllipseColor();
        }

        public void UpdateSelectionEllipseColor()
        {
            if (selectionEllipse != null)
            {
                var selectionEllipseStrokeBrush = selectionEllipse.Stroke as SolidColorBrush;

                if (selectionEllipseStrokeBrush != null)
                {
                    var selectionEllipseColor = selectionEllipseStrokeBrush.Color;

                    EllipseRedTextBlock.Text = selectionEllipseColor.R.ToString(CultureInfo.InvariantCulture);
                    EllipseGreenTextBlock.Text = selectionEllipseColor.G.ToString(CultureInfo.InvariantCulture);
                    EllipseBlueTextBlock.Text = selectionEllipseColor.B.ToString(CultureInfo.InvariantCulture);
                    EllipseAlphaTextBlock.Text = selectionEllipseColor.A.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public void SelectionEllipsePositionChanged(object o, AvaloniaPropertyChangedEventArgs p)
        {
            UpdateSelectionEllipsePosition();
        }

        public void UpdateSelectionEllipsePosition()
        {
            if (selectionEllipse != null
                && selectionEllipsePanel != null)
            {
                var ellipseX = Canvas.GetLeft(selectionEllipsePanel) + (selectionEllipsePanel.Width / 2);
                var ellipseY = Canvas.GetTop(selectionEllipsePanel) + (selectionEllipsePanel.Height / 2);

                EllipseXTextBlock.Text = Math.Round(ellipseX).ToString(CultureInfo.InvariantCulture);
                EllipseYTextBlock.Text = Math.Round(ellipseY).ToString(CultureInfo.InvariantCulture);
            }
        }

        public void ColorNameToolTipContentChanged(object o, AvaloniaPropertyChangedEventArgs p)
        {
            UpdateSelectedColorName();
        }

        public void UpdateSelectedColorName()
        {
            SelectedColorNameTextBlock.Text = colorNameToolTip?.Content as string ?? "";
        }

        public void ColorSpectrumImageSizeChanged(object? sender, object e)
        {
            UpdateHeightFromInputTarget();
        }

        public void UpdateHeightFromInputTarget()
        {
            if (colorSpectrumInputTarget != null)
            {
                WidthTextBlock.Text = colorSpectrumInputTarget.Bounds.Width.ToString(CultureInfo.InvariantCulture);
                HeightTextBlock.Text = colorSpectrumInputTarget.Bounds.Height.ToString(CultureInfo.InvariantCulture);
            }
        }

        public void ColorSpectrumShapeComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender!;

            switch (comboBox.SelectedIndex)
            {
                case 0:
                    ColorPicker.ColorSpectrumShape = ColorSpectrumShape.Box;
                    break;
                case 1:
                    ColorPicker.ColorSpectrumShape = ColorSpectrumShape.Ring;
                    break;
            }
        }

        public void ColorSpectrumComponentsComboBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox)sender!;

            switch (comboBox.SelectedIndex)
            {
                case 0:
                    ColorPicker.ColorSpectrumComponents = ColorSpectrumComponents.HueSaturation;
                    break;
                case 1:
                    ColorPicker.ColorSpectrumComponents = ColorSpectrumComponents.HueValue;
                    break;
                case 2:
                    ColorPicker.ColorSpectrumComponents = ColorSpectrumComponents.SaturationHue;
                    break;
                case 3:
                    ColorPicker.ColorSpectrumComponents = ColorSpectrumComponents.SaturationValue;
                    break;
                case 4:
                    ColorPicker.ColorSpectrumComponents = ColorSpectrumComponents.ValueHue;
                    break;
                case 5:
                    ColorPicker.ColorSpectrumComponents = ColorSpectrumComponents.ValueSaturation;
                    break;
            }
        }

        public void ColorPickerColorChanged(object? sender, ColorChangedEventArgs args)
        {
            ColorFromEventRectangle.Fill = new SolidColorBrush(args.NewColor);

            RedTextBlock.Text = args.NewColor.R.ToString(CultureInfo.InvariantCulture);
            GreenTextBlock.Text = args.NewColor.G.ToString(CultureInfo.InvariantCulture);
            BlueTextBlock.Text = args.NewColor.B.ToString(CultureInfo.InvariantCulture);
            AlphaTextBlock.Text = args.NewColor.A.ToString(CultureInfo.InvariantCulture);

            OldRedTextBlock.Text = args.OldColor.R.ToString(CultureInfo.InvariantCulture);
            OldGreenTextBlock.Text = args.OldColor.G.ToString(CultureInfo.InvariantCulture);
            OldBlueTextBlock.Text = args.OldColor.B.ToString(CultureInfo.InvariantCulture);
            OldAlphaTextBlock.Text = args.OldColor.A.ToString(CultureInfo.InvariantCulture);
        }

        public void ThemeLightButtonClick(object? sender, RoutedEventArgs args)
        {
            Application.Current!.Styles.Clear();
            Application.Current.Styles.Add(new FluentTheme(new Uri("avares://AvaloniaWinUI.ColorPicker.Sample")) { Mode = FluentThemeMode.Light });
            Application.Current.Styles.Add(new StyleInclude(new Uri("avares://AvaloniaWinUI.ColorPicker.Sample")) { Source = new Uri("avares://AvaloniaWinUI.ColorPicker/Themes/Fluent.xaml") });

            if (moreButton != null)
            {
                MoreButtonBackgroundTextBlock.Text = (moreButton.Background as SolidColorBrush)?.Color.ToString();
                MoreButtonForegroundTextBlock.Text = (moreButton.Foreground as SolidColorBrush)?.Color.ToString();
                MoreButtonBorderBrushTextBlock.Text = (moreButton.BorderBrush as SolidColorBrush)?.Color.ToString();
            }
        }

        public void ThemeDarkButtonClick(object? sender, RoutedEventArgs e)
        {
            Application.Current!.Styles.Clear();
            Application.Current.Styles.Add(new FluentTheme(new Uri("avares://AvaloniaWinUI.ColorPicker.Sample")) { Mode = FluentThemeMode.Dark });
            Application.Current.Styles.Add(new StyleInclude(new Uri("avares://AvaloniaWinUI.ColorPicker.Sample")) { Source = new Uri("avares://AvaloniaWinUI.ColorPicker/Themes/Fluent.xaml") });
            if (moreButton != null)
            {
                MoreButtonBackgroundTextBlock.Text = (moreButton.Background as SolidColorBrush)?.Color.ToString();
                MoreButtonForegroundTextBlock.Text = (moreButton.Foreground as SolidColorBrush)?.Color.ToString();
                MoreButtonBorderBrushTextBlock.Text = (moreButton.BorderBrush as SolidColorBrush)?.Color.ToString();
            }
        }

        public void ColorWhiteButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.Color = Colors.White;
        }

        public void ColorRedButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.Color = Colors.Red;
        }

        public void ColorGreenButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.Color = Colors.Green;
        }

        public void ColorBlueButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.Color = Colors.Blue;
        }

        public void PreviousColorNullButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.PreviousColor = null;
        }

        public void PreviousColorRedButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.PreviousColor = Colors.Red;
        }

        public void PreviousColorGreenButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.PreviousColor = Colors.Green;
        }

        public void PreviousColorBlueButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.PreviousColor = Colors.Blue;
        }

        public void PreviousColorCurrentColorButtonClick(object? sender, RoutedEventArgs e)
        {
            ColorPicker.PreviousColor = ColorPicker.Color;
        }

        public void TestPageGotFocus(object? sender, RoutedEventArgs e)
        {
            var focusedElement = e.Source as Control;

            CurrentlyFocusedElementTextBlock.Text = focusedElement != null
                ? string.IsNullOrEmpty(focusedElement.Name) ? "(an unnamed element)" : focusedElement.Name
                : "(nothing)";
        }

        public void RTLCheckBoxChecked(object? sender, RoutedEventArgs e)
        {
            //this.ColorPicker.FlowDirection = FlowDirection.RightToLeft;
        }

        public void RTLCheckBoxUnchecked(object? sender, RoutedEventArgs e)
        {
            //this.ColorPicker.FlowDirection = FlowDirection.LeftToRight;
        }
    }
}
