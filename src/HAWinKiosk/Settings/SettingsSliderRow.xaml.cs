using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace HAWinKiosk.Settings;

public partial class SettingsSliderRow : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(SettingsSliderRow),
            new PropertyMetadata("", (d, e) =>
            {
                if (d is SettingsSliderRow r)
                    r.LabelText.Text = e.NewValue as string ?? "";
            }));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(SettingsSliderRow),
            new PropertyMetadata(0.0, (d, e) =>
            {
                if (d is SettingsSliderRow r)
                    r.ValueSlider.Minimum = (double)e.NewValue;
            }));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(SettingsSliderRow),
            new PropertyMetadata(100.0, (d, e) =>
            {
                if (d is SettingsSliderRow r)
                    r.ValueSlider.Maximum = (double)e.NewValue;
            }));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(SettingsSliderRow),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) =>
            {
                if (d is SettingsSliderRow r && e.NewValue is double v)
                {
                    if (Math.Abs(r.ValueSlider.Value - v) > 0.001)
                        r.ValueSlider.Value = v;
                    r.ValueLabel.Text = ((int)Math.Round(r.ValueSlider.Value)).ToString();
                    if (Math.Abs(r.ValueSlider.Value - v) > 0.001)
                        r.SetCurrentValue(ValueProperty, r.ValueSlider.Value);
                }
            }));

    public event RoutedPropertyChangedEventHandler<double>? ValueChanged;

    public SettingsSliderRow()
    {
        InitializeComponent();
        // A dependency property's default does not invoke its changed callback.
        // WPF Slider has its own default range, so initialize it explicitly.
        ValueSlider.Minimum = Minimum;
        ValueSlider.Maximum = Maximum;
        ValueSlider.Value = Value;
        ValueLabel.Text = ((int)Math.Round(ValueSlider.Value)).ToString();
        ValueSlider.ValueChanged += (_, e) =>
        {
            SetCurrentValue(ValueProperty, e.NewValue);
            ValueLabel.Text = ((int)Math.Round(e.NewValue)).ToString();
            ValueChanged?.Invoke(this, e);
        };
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Slider Slider => ValueSlider;
    internal string DisplayedValue => ValueLabel.Text;
}
