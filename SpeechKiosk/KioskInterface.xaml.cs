using System;
using System.Windows;
using System.Windows.Controls;
using SpeechKiosk.Brain;
using SpeechKiosk.Access;

namespace SpeechKiosk
{
    public partial class KioskInterface : Window
    {
        private PresentationBrain _brainController;
        
        public KioskInterface()
        {
            InitializeComponent();
            _brainController = new PresentationBrain();
            SetupAccessibilityFeatures();
            SetupEventHandlers();
            LoadAvailableVoices();
            
            // 환영 메시지 재생
            Loaded += (s, e) => _brainController.PlayWelcomeAnnouncement();
        }
        
        private void SetupAccessibilityFeatures()
        {
            // 입력 영역 접근성
            UniversalAccessManager.SetupInputAccessibility(
                MessageInputBox, 
                "메시지 입력", 
                "음성으로 읽을 텍스트를 입력하세요", 
                4);
            
            // 버튼 접근성
            UniversalAccessManager.SetupButtonAccessibility(
                VocalizeBtn, "음성 출력", "입력한 텍스트를 음성으로 읽습니다", 5);
            UniversalAccessManager.SetupButtonAccessibility(
                StopBtn, "중지", "현재 재생 중인 음성을 중지합니다", 6);
            UniversalAccessManager.SetupButtonAccessibility(
                ClearBtn, "초기화", "입력한 텍스트를 모두 지웁니다", 7);
            
            // 슬라이더 접근성
            UniversalAccessManager.SetupSliderAccessibility(
                SpeedSlider, "속도 조절", "음성 재생 속도를 조절합니다", 8);
            UniversalAccessManager.SetupSliderAccessibility(
                VolumeSlider, "볼륨 조절", "음성 출력 볼륨을 조절합니다", 9);
            
            // 콤보박스 접근성
            UniversalAccessManager.SetupComboBoxAccessibility(
                VoiceSelector, "음성 선택", "사용할 음성을 선택합니다", 10);
            
            // 키보드 단축키
            UniversalAccessManager.KeyboardShortcutHelper.RegisterGlobalShortcuts(
                this,
                () => VocalizeBtn_Click(null, null),
                () => StopBtn_Click(null, null),
                () => ClearBtn_Click(null, null)
            );
        }
        
        private void SetupEventHandlers()
        {
            VocalizeBtn.Click += VocalizeBtn_Click;
            StopBtn.Click += StopBtn_Click;
            ClearBtn.Click += ClearBtn_Click;
            ContrastToggleBtn.Click += ContrastToggleBtn_Click;
            FontIncreaseBtn.Click += FontIncreaseBtn_Click;
            FontDecreaseBtn.Click += FontDecreaseBtn_Click;
            
            Phrase1Btn.Click += (s, e) => QuickPhrase("안녕하세요");
            Phrase2Btn.Click += (s, e) => QuickPhrase("도움이 필요하신가요?");
            Phrase3Btn.Click += (s, e) => QuickPhrase("감사합니다");
            
            SpeedSlider.ValueChanged += (s, e) => 
                _brainController.SpeedSetting = (int)e.NewValue;
            VolumeSlider.ValueChanged += (s, e) => 
                _brainController.AmplitudeSetting = (int)e.NewValue;
            VoiceSelector.SelectionChanged += (s, e) => {
                if (VoiceSelector.SelectedItem != null)
                    _brainController.SelectedVoiceIdentifier = 
                        ((ComboBoxItem)VoiceSelector.SelectedItem).Tag.ToString();
            };
        }
        
        private void LoadAvailableVoices()
        {
            var voices = _brainController.GetAvailableVoicesList();
            foreach (var voice in voices)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{voice.DisplayName} ({voice.Language})",
                    Tag = voice.DisplayName
                };
                VoiceSelector.Items.Add(item);
            }
            if (VoiceSelector.Items.Count > 0)
                VoiceSelector.SelectedIndex = 0;
        }
        
        private void VocalizeBtn_Click(object sender, RoutedEventArgs e)
        {
            _brainController.MessageContent = MessageInputBox.Text;
            _brainController.VocalizeCommand.Execute(null);
        }
        
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _brainController.StopVocalizationCommand.Execute(null);
        }
        
        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageInputBox.Text = string.Empty;
            _brainController.MessageContent = string.Empty;
        }
        
        private void QuickPhrase(string phrase)
        {
            MessageInputBox.Text = phrase;
            _brainController.MessageContent = phrase;
            _brainController.VocalizeCommand.Execute(null);
        }
        
        private void ContrastToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            _brainController.UseHighContrastMode = !_brainController.UseHighContrastMode;
            if (_brainController.UseHighContrastMode)
                UniversalAccessManager.ContrastThemeManager.ApplyHighContrastTheme(this);
            else
                UniversalAccessManager.ContrastThemeManager.ApplyStandardTheme(this);
        }
        
        private void FontIncreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            _brainController.IncreaseFontCommand.Execute(null);
            ApplyFontScale();
        }
        
        private void FontDecreaseBtn_Click(object sender, RoutedEventArgs e)
        {
            _brainController.DecreaseFontCommand.Execute(null);
            ApplyFontScale();
        }
        
        private void ApplyFontScale()
        {
            var scale = _brainController.TextMagnification;
            MessageInputBox.FontSize = 16 * scale;
        }
    }
}
