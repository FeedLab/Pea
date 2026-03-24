using Pea.Meter.Models;
using Syncfusion.Maui.Buttons;

namespace Pea.Meter.Components.Configuration;

public partial class LanguageItemComponent : Border
{
    public static readonly BindableProperty FlagSourceProperty =
        BindableProperty.Create(nameof(FlagSource), typeof(string), typeof(LanguageItemComponent), propertyChanged: OnFlagSourceChanged);

    public static readonly BindableProperty LanguageNameProperty =
        BindableProperty.Create(nameof(LanguageName), typeof(string), typeof(LanguageItemComponent), propertyChanged: OnLanguageNameChanged);

    public static readonly BindableProperty LanguageValueProperty =
        BindableProperty.Create(nameof(LanguageValue), typeof(string), typeof(LanguageItemComponent), propertyChanged: OnLanguageValueChanged);

    public static readonly BindableProperty IsCheckedProperty =
        BindableProperty.Create(nameof(IsChecked), typeof(bool), typeof(LanguageItemComponent), false, BindingMode.TwoWay, propertyChanged: OnIsCheckedChanged);

    public static readonly BindableProperty CultureCodeProperty =
        BindableProperty.Create(nameof(CultureCode), typeof(string), typeof(LanguageItemComponent), ConfigurationLanguageModel.DefaultCultureCode, BindingMode.TwoWay, propertyChanged: OnCultureCodeChanged);

    public string CultureCode
    {
        get => (string)GetValue(CultureCodeProperty);
        set => SetValue(CultureCodeProperty, value);
    }   
    
    public string FlagSource
    {
        get => (string)GetValue(FlagSourceProperty);
        set => SetValue(FlagSourceProperty, value);
    }

    public string LanguageName
    {
        get => (string)GetValue(LanguageNameProperty);
        set => SetValue(LanguageNameProperty, value);
    }

    public string LanguageValue
    {
        get => (string)GetValue(LanguageValueProperty);
        set => SetValue(LanguageValueProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public event EventHandler<StateChangedEventArgs>? StateChanged;

    public LanguageItemComponent()
    {
        InitializeComponent();
        RadioButton.StateChanged += OnRadioButtonStateChanged;
    }

    private static void OnCultureCodeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LanguageItemComponent component)
        {
            component.CultureCode = newValue as string;
        }
    }
    
    private static void OnFlagSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LanguageItemComponent component)
        {
            component.FlagImage.Source = newValue as string;
        }
    }

    private static void OnLanguageNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LanguageItemComponent component)
        {
            component.RadioButton.Text = newValue as string;
        }
    }

    private static void OnLanguageValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LanguageItemComponent component)
        {
            component.RadioButton.Value = newValue;
        }
    }

    private static void OnIsCheckedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LanguageItemComponent component && newValue is bool isChecked)
        {
            component.RadioButton.IsChecked = isChecked;
        }
    }

    private void OnRadioButtonStateChanged(object? sender, StateChangedEventArgs e)
    {
        IsChecked = e.IsChecked ?? false;
        StateChanged?.Invoke(this, e);
    }
}
