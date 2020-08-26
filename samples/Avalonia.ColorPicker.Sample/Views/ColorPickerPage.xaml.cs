using System;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Avalonia.ColorPicker.Sample.Views
{
    public class ColorPickerPage : UserControl
    {
        private ToolTip? colorNameToolTip;
        private Rectangle? spectrumRectangle;
        private Control? colorSpectrumInputTarget;
        private Rectangle? previousColorRectangle;
        private Panel? selectionEllipsePanel;
        private Ellipse? selectionEllipse;
        private Button? moreButton;

        public ColorPicker ColorPicker => this.FindControl<ColorPicker>("ColorPicker");
        public TextBlock CurrentlyFocusedElementTextBlock => this.FindControl<TextBlock>("CurrentlyFocusedElementTextBlock");
        public Rectangle ColorFromEventRectangle => this.FindControl<Rectangle>("ColorFromEventRectangle");
        public TextBlock SelectedColorNameTextBlock => this.FindControl<TextBlock>("SelectedColorNameTextBlock");
        public TextBlock RedTextBlock => this.FindControl<TextBlock>("RedTextBlock");
        public TextBlock GreenTextBlock => this.FindControl<TextBlock>("GreenTextBlock");
        public TextBlock BlueTextBlock => this.FindControl<TextBlock>("BlueTextBlock");
        public TextBlock AlphaTextBlock => this.FindControl<TextBlock>("AlphaTextBlock");
        public TextBlock OldRedTextBlock => this.FindControl<TextBlock>("OldRedTextBlock");
        public TextBlock OldGreenTextBlock => this.FindControl<TextBlock>("OldGreenTextBlock");
        public TextBlock OldBlueTextBlock => this.FindControl<TextBlock>("OldBlueTextBlock");
        public TextBlock OldAlphaTextBlock => this.FindControl<TextBlock>("OldAlphaTextBlock");
        public TextBlock PreviousRedTextBlock => this.FindControl<TextBlock>("PreviousRedTextBlock");
        public TextBlock PreviousGreenTextBlock => this.FindControl<TextBlock>("PreviousGreenTextBlock");
        public TextBlock PreviousBlueTextBlock => this.FindControl<TextBlock>("PreviousBlueTextBlock");
        public TextBlock PreviousAlphaTextBlock => this.FindControl<TextBlock>("PreviousAlphaTextBlock");
        public TextBlock EllipseRedTextBlock => this.FindControl<TextBlock>("EllipseRedTextBlock");
        public TextBlock EllipseGreenTextBlock => this.FindControl<TextBlock>("EllipseGreenTextBlock");
        public TextBlock EllipseBlueTextBlock => this.FindControl<TextBlock>("EllipseBlueTextBlock");
        public TextBlock EllipseAlphaTextBlock => this.FindControl<TextBlock>("EllipseAlphaTextBlock");
        public TextBlock MoreButtonForegroundTextBlock => this.FindControl<TextBlock>("MoreButtonForegroundTextBlock");
        public TextBlock MoreButtonBackgroundTextBlock => this.FindControl<TextBlock>("MoreButtonBackgroundTextBlock");
        public TextBlock MoreButtonBorderBrushTextBlock => this.FindControl<TextBlock>("MoreButtonBorderBrushTextBlock");
        public CheckBox ColorSpectrumLoadedCheckBox => this.FindControl<CheckBox>("ColorSpectrumLoadedCheckBox");
        public TextBlock WidthTextBlock => this.FindControl<TextBlock>("WidthTextBlock");
        public TextBlock HeightTextBlock => this.FindControl<TextBlock>("HeightTextBlock");
        public TextBlock EllipseXTextBlock => this.FindControl<TextBlock>("EllipseXTextBlock");
        public TextBlock EllipseYTextBlock => this.FindControl<TextBlock>("EllipseYTextBlock");

        public ColorPickerPage()
        {
            InitializeComponent();
        }

        public void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

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

            RedTextBlock.Text = ColorPicker.Color.R.ToString();
            GreenTextBlock.Text = ColorPicker.Color.G.ToString();
            BlueTextBlock.Text = ColorPicker.Color.B.ToString();
            AlphaTextBlock.Text = ColorPicker.Color.A.ToString();
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

            if (selectionEllipsePanel != null)
            {
                previousColorRectangle.GetPropertyChangedObservable(Canvas.LeftProperty).AddClassHandler<Panel>(SelectionEllipsePositionChanged);
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

                    PreviousRedTextBlock.Text = previousColor.R.ToString();
                    PreviousGreenTextBlock.Text = previousColor.G.ToString();
                    PreviousBlueTextBlock.Text = previousColor.B.ToString();
                    PreviousAlphaTextBlock.Text = previousColor.A.ToString();
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

                    EllipseRedTextBlock.Text = selectionEllipseColor.R.ToString();
                    EllipseGreenTextBlock.Text = selectionEllipseColor.G.ToString();
                    EllipseBlueTextBlock.Text = selectionEllipseColor.B.ToString();
                    EllipseAlphaTextBlock.Text = selectionEllipseColor.A.ToString();
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

                EllipseXTextBlock.Text = Math.Round(ellipseX).ToString();
                EllipseYTextBlock.Text = Math.Round(ellipseY).ToString();
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
                WidthTextBlock.Text = colorSpectrumInputTarget.Bounds.Width.ToString();
                HeightTextBlock.Text = colorSpectrumInputTarget.Bounds.Height.ToString();
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

            RedTextBlock.Text = args.NewColor.R.ToString();
            GreenTextBlock.Text = args.NewColor.G.ToString();
            BlueTextBlock.Text = args.NewColor.B.ToString();
            AlphaTextBlock.Text = args.NewColor.A.ToString();

            OldRedTextBlock.Text = args.OldColor.R.ToString();
            OldGreenTextBlock.Text = args.OldColor.G.ToString();
            OldBlueTextBlock.Text = args.OldColor.B.ToString();
            OldAlphaTextBlock.Text = args.OldColor.A.ToString();
        }

        public void ThemeLightButtonClick(object? sender, RoutedEventArgs args)
        {
            if (moreButton != null)
            {
                //this.RequestedTheme = ElementTheme.Light;
                MoreButtonBackgroundTextBlock.Text = (moreButton.Background as SolidColorBrush)?.Color.ToString();
                MoreButtonForegroundTextBlock.Text = (moreButton.Foreground as SolidColorBrush)?.Color.ToString();
                MoreButtonBorderBrushTextBlock.Text = (moreButton.BorderBrush as SolidColorBrush)?.Color.ToString();
            }
        }

        public void ThemeDarkButtonClick(object? sender, RoutedEventArgs e)
        {
            if (moreButton != null)
            {
                //this.RequestedTheme = ElementTheme.Dark;
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

            if (focusedElement != null)
            {
                CurrentlyFocusedElementTextBlock.Text = string.IsNullOrEmpty(focusedElement.Name) ? "(an unnamed element)" : focusedElement.Name;
            }
            else
            {
                CurrentlyFocusedElementTextBlock.Text = "(nothing)";
            }
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
