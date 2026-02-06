using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace TTS.AccessSupport
{
    public class UniversalAccessManager
    {
        private Application _app;
        private ThemeSwitch _theme;
        private FontScale _font;
        private ContrastCheck _contrast;
        
        public UniversalAccessManager(Application app)
        {
            _app = app;
            _theme = new ThemeSwitch(app);
            _font = new FontScale(app);
            _contrast = new ContrastCheck();
        }
        
        public void ToggleHighContrastTheme()
        {
            _theme.Toggle();
        }
        
        public void AdjustFontScaling(bool large)
        {
            _font.Set(large ? 24.0 : 16.0);
        }
        
        public double CurrentFontSize => _font.Size;
        public bool IsHighContrastEnabled => _theme.IsHC;
        
        public void EnhanceElementAccessibility(UIElement e, string name, string help, string role = null)
        {
            if (e == null) return;
            
            AutomationProperties.SetName(e, name);
            AutomationProperties.SetHelpText(e, help);
            
            if (!string.IsNullOrEmpty(role))
                AutomationProperties.SetItemType(e, role);
            
            if (e is Control c)
                c.IsTabStop = true;
        }
        
        public void EnsureButtonAccessibility(Button b, string name, int min = 44)
        {
            if (b == null) return;
            
            b.MinWidth = Math.Max(b.MinWidth, min);
            b.MinHeight = Math.Max(b.MinHeight, min);
            
            AutomationProperties.SetName(b, name);
            AutomationProperties.SetAutomationId(b, $"btn_{name.Replace(" ", "_")}");
        }
        
        public void ConfigureTextInputAccessibility(TextBox t, string label, string desc)
        {
            if (t == null) return;
            
            AutomationProperties.SetName(t, label);
            AutomationProperties.SetHelpText(t, desc);
        }
        
        public void OptimizeSliderAccessibility(Slider s, string label, string unit)
        {
            if (s == null) return;
            
            AutomationProperties.SetName(s, label);
            UpdateSliderHelp(s, label, unit);
            
            s.ValueChanged += (_, __) => UpdateSliderHelp(s, label, unit);
        }
        
        private void UpdateSliderHelp(Slider s, string label, string unit)
        {
            var help = $"{label}: {s.Value}{unit} (Range: {s.Minimum}~{s.Maximum})";
            AutomationProperties.SetHelpText(s, help);
        }
        
        public void AnnounceToScreenReader(UIElement e, string msg)
        {
            if (e == null) return;
            
            if (e is TextBlock tb)
                tb.Text = msg;
            else
                AutomationProperties.SetName(e, msg);
        }
        
        public void EstablishNavigationSequence(UIElement e, int idx)
        {
            if (e is Control c)
            {
                c.TabIndex = idx;
                c.IsTabStop = true;
            }
        }
        
        public bool ValidateContrastRatio(Color fg, Color bg, bool large = false)
        {
            return _contrast.Check(fg, bg, large);
        }
    }
    
    internal class ThemeSwitch
    {
        private Application _app;
        private bool _hc;
        
        public ThemeSwitch(Application app)
        {
            _app = app;
            _hc = false;
        }
        
        public void Toggle()
        {
            _hc = !_hc;
            
            var path = _hc 
                ? "Styling/AdaptiveVisualTheme.xaml" 
                : "Styling/StandardVisualTheme.xaml";
            
            try
            {
                _app.Resources.MergedDictionaries.Clear();
                var dict = new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
                _app.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Theme error: {ex.Message}", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        public bool IsHC => _hc;
    }
    
    internal class FontScale
    {
        private Application _app;
        private double _sz;
        
        public FontScale(Application app)
        {
            _app = app;
            _sz = 16.0;
        }
        
        public void Set(double sz)
        {
            _sz = sz;
            _app.Resources["GlobalFontSize"] = sz;
        }
        
        public double Size => _sz;
    }
    
    internal class ContrastCheck
    {
        public bool Check(Color c1, Color c2, bool large)
        {
            double ratio = Calc(c1, c2);
            double req = large ? 3.0 : 4.5;
            return ratio >= req;
        }
        
        private double Calc(Color c1, Color c2)
        {
            double l1 = Lum(c1);
            double l2 = Lum(c2);
            
            double light = Math.Max(l1, l2);
            double dark = Math.Min(l1, l2);
            
            return (light + 0.05) / (dark + 0.05);
        }
        
        private double Lum(Color c)
        {
            double r = Gamma(c.R / 255.0);
            double g = Gamma(c.G / 255.0);
            double b = Gamma(c.B / 255.0);
            
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }
        
        private double Gamma(double ch)
        {
            return ch <= 0.03928 
                ? ch / 12.92 
                : Math.Pow((ch + 0.055) / 1.055, 2.4);
        }
    }
}
