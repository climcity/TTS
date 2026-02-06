using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using TTS.AccessSupport;
using TTS.StateManagement;
using TTS.VoiceEngine;

namespace TTS
{
    public partial class KioskDisplay : Window
    {
        private readonly PresentationController _pc;
        private readonly VoiceOrchestrator _vo;
        private readonly UniversalAccessManager _uam;
        private byte _fontFlag;
        
        public KioskDisplay()
        {
            InitializeComponent();
            
            _pc = new PresentationController();
            _vo = new VoiceOrchestrator();
            _uam = new UniversalAccessManager(Application.Current);
            _fontFlag = 0;
            
            LoadV();
            WireH();
            ApplyA();
            
            DataContext = _pc;
        }
        
        private void LoadV()
        {
            var vl = _vo.QueryAvailableVoices();
            VoiceSelector.ItemsSource = vl;
            VoiceSelector.DisplayMemberPath = "DisplayLabel";
            VoiceSelector.SelectedValuePath = "Identifier";
            
            var kv = vl.FirstOrDefault(v => v.IsKoreanVoice);
            if (kv != null)
            {
                VoiceSelector.SelectedValue = kv.Identifier;
                _pc.SelectedVoice = kv.Identifier;
                _vo.SwitchToVoice(kv.Identifier);
            }
            else if (vl.Any())
            {
                VoiceSelector.SelectedIndex = 0;
                _pc.SelectedVoice = vl[0].Identifier;
            }
        }
        
        private void WireH()
        {
            _pc.ActionTriggered += (s, e) =>
            {
                switch (e.ActionType)
                {
                    case "Vocalize": DoV(); break;
                    case "Halt": DoH(); break;
                    case "Clear": Ann("Cleared"); break;
                }
            };
            
            _vo.OnTransition += pkt =>
            {
                if (pkt.Phase == "started")
                    Ann("Started");
                else if (pkt.Phase == "completed")
                    Dispatcher.Invoke(() =>
                    {
                        _pc.IsProcessingVocalization = false;
                        _pc.RefreshAllCommands();
                        Ann("Done");
                    });
            };
            
            SpeedSlider.ValueChanged += (s, e) => _vo.ModifyPitch((int)e.NewValue);
            VolumeSlider.ValueChanged += (s, e) => _vo.ModifyAmplitude((int)e.NewValue);
            VoiceSelector.SelectionChanged += OnVC;
            
            HighContrastToggle.Click += (s, e) => DoTT();
            LargeFontToggle.Click += (s, e) => DoFT();
            
            AttachH();
        }
        
        private void DoV()
        {
            var txt = _pc.CurrentText;
            if (string.IsNullOrWhiteSpace(txt)) return;
            
            try
            {
                _vo.VocalizeText(txt);
                _pc.IsProcessingVocalization = true;
                _pc.RefreshAllCommands();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DoH()
        {
            _vo.HaltVocalization();
            _pc.IsProcessingVocalization = false;
            _pc.RefreshAllCommands();
            Ann("Stopped");
        }
        
        private void OnVC(object s, SelectionChangedEventArgs e)
        {
            if (VoiceSelector.SelectedValue is string id)
            {
                if (_vo.SwitchToVoice(id))
                {
                    var v = VoiceSelector.SelectedItem as VoiceDescriptor;
                    Ann($"Voice: {v?.DisplayLabel}");
                }
            }
        }
        
        private void DoTT()
        {
            _uam.ToggleHighContrastTheme();
            var msg = _uam.IsHighContrastEnabled ? "HC On" : "Standard";
            Ann(msg);
        }
        
        private void DoFT()
        {
            _fontFlag = (byte)((_fontFlag + 1) & 1);
            _uam.AdjustFontScaling(_fontFlag == 1);
            Ann(_fontFlag == 1 ? "Large" : "Normal");
        }
        
        private void AttachH()
        {
            VocalizeButton.MouseEnter += (s, e) => AutomationProperties.SetName(LiveRegionAnnouncer, "Vocalize");
            HaltButton.MouseEnter += (s, e) => AutomationProperties.SetName(LiveRegionAnnouncer, "Halt");
            ClearButton.MouseEnter += (s, e) => AutomationProperties.SetName(LiveRegionAnnouncer, "Clear");
        }
        
        private void Ann(string msg)
        {
            _uam.AnnounceToScreenReader(LiveRegionAnnouncer, msg);
        }
        
        private void ApplyA()
        {
            _uam.EnsureButtonAccessibility(VocalizeButton, "Vocalize");
            _uam.EnsureButtonAccessibility(HaltButton, "Halt");
            _uam.EnsureButtonAccessibility(ClearButton, "Clear");
            _uam.ConfigureTextInputAccessibility(MainTextInput, "Text", "Enter text");
            _uam.OptimizeSliderAccessibility(SpeedSlider, "Speed", "");
            _uam.OptimizeSliderAccessibility(VolumeSlider, "Volume", "%");
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _vo?.Dispose();
            base.OnClosed(e);
        }
    }
}
