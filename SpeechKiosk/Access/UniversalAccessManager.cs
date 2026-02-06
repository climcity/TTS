using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpeechKiosk.Access
{
    /// <summary>
    /// 접근성 기능을 관리하는 UniversalAccessManager
    /// </summary>
    public static class UniversalAccessManager
    {
        /// <summary>
        /// UI 요소에 접근성 속성을 설정하는 헬퍼 메서드
        /// </summary>
        public static void ConfigureAccessibilityAttributes(
            UIElement element, 
            string automationName, 
            string helpMessage = null,
            AutomationLiveSetting liveLevel = AutomationLiveSetting.Off)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            AutomationProperties.SetName(element, automationName);
            
            if (!string.IsNullOrEmpty(helpMessage))
            {
                AutomationProperties.SetHelpText(element, helpMessage);
            }

            if (liveLevel != AutomationLiveSetting.Off)
            {
                AutomationProperties.SetLiveSetting(element, liveLevel);
            }
        }

        /// <summary>
        /// 버튼에 대한 완전한 접근성 설정
        /// </summary>
        public static void SetupButtonAccessibility(
            Button button,
            string buttonLabel,
            string detailedHelp,
            int tabSequence)
        {
            if (button == null) return;

            ConfigureAccessibilityAttributes(button, buttonLabel, detailedHelp);
            button.TabIndex = tabSequence;
            
            // 최소 터치 타겟 크기 보장 (44x44 픽셀)
            if (button.MinWidth < 44)
                button.MinWidth = 44;
            if (button.MinHeight < 44)
                button.MinHeight = 44;
        }

        /// <summary>
        /// 입력 필드에 대한 접근성 설정
        /// </summary>
        public static void SetupInputAccessibility(
            TextBox inputField,
            string fieldLabel,
            string guidanceText,
            int tabSequence)
        {
            if (inputField == null) return;

            ConfigureAccessibilityAttributes(inputField, fieldLabel, guidanceText);
            inputField.TabIndex = tabSequence;
            
            // 포커스 시각적 효과 강화
            inputField.GotFocus += (s, e) => {
                inputField.BorderThickness = new Thickness(3);
                inputField.BorderBrush = new SolidColorBrush(Colors.DodgerBlue);
            };
            
            inputField.LostFocus += (s, e) => {
                inputField.BorderThickness = new Thickness(1);
                inputField.BorderBrush = new SolidColorBrush(Colors.Gray);
            };
        }

        /// <summary>
        /// 슬라이더 접근성 설정
        /// </summary>
        public static void SetupSliderAccessibility(
            Slider slider,
            string sliderPurpose,
            string usageInstructions,
            int tabSequence)
        {
            if (slider == null) return;

            ConfigureAccessibilityAttributes(slider, sliderPurpose, usageInstructions);
            slider.TabIndex = tabSequence;
            slider.IsSnapToTickEnabled = true;
            slider.TickFrequency = 1;
        }

        /// <summary>
        /// 콤보박스 접근성 설정
        /// </summary>
        public static void SetupComboBoxAccessibility(
            ComboBox comboBox,
            string selectionPurpose,
            string selectionHelp,
            int tabSequence)
        {
            if (comboBox == null) return;

            ConfigureAccessibilityAttributes(comboBox, selectionPurpose, selectionHelp);
            comboBox.TabIndex = tabSequence;
        }

        /// <summary>
        /// 라이브 영역 알림을 발생시킴 (화면 낭독기용)
        /// </summary>
        public static void AnnounceToScreenReader(UIElement element, string message)
        {
            if (element == null) return;

            var peer = UIElementAutomationPeer.FromElement(element);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        }

        /// <summary>
        /// 고대비 모드를 위한 색상 테마 적용
        /// </summary>
        public static class ContrastThemeManager
        {
            public static void ApplyHighContrastTheme(Window window)
            {
                if (window == null) return;

                var highContrastDict = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Design/HighContrastVisuals.xaml", UriKind.Absolute)
                };

                window.Resources.MergedDictionaries.Clear();
                window.Resources.MergedDictionaries.Add(highContrastDict);
            }

            public static void ApplyStandardTheme(Window window)
            {
                if (window == null) return;

                var standardDict = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/Design/StandardVisuals.xaml", UriKind.Absolute)
                };

                window.Resources.MergedDictionaries.Clear();
                window.Resources.MergedDictionaries.Add(standardDict);
            }
        }

        /// <summary>
        /// 키보드 단축키 지원을 위한 헬퍼
        /// </summary>
        public static class KeyboardShortcutHelper
        {
            public static void RegisterGlobalShortcuts(Window window, 
                Action onVocalize, 
                Action onStop, 
                Action onClear)
            {
                window.PreviewKeyDown += (sender, e) =>
                {
                    // Ctrl+V: 음성 출력
                    if (e.Key == System.Windows.Input.Key.V && 
                        (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                    {
                        onVocalize?.Invoke();
                        e.Handled = true;
                    }
                    // Escape: 중지
                    else if (e.Key == System.Windows.Input.Key.Escape)
                    {
                        onStop?.Invoke();
                        e.Handled = true;
                    }
                    // Ctrl+Delete: 텍스트 지우기
                    else if (e.Key == System.Windows.Input.Key.Delete && 
                        (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
                    {
                        onClear?.Invoke();
                        e.Handled = true;
                    }
                };
            }
        }

        /// <summary>
        /// WCAG 2.1 AA 기준 색상 대비를 검증
        /// </summary>
        public static bool ValidateColorContrast(Color foreground, Color background)
        {
            // 상대 휘도 계산
            double GetRelativeLuminance(Color color)
            {
                double RsRGB = color.R / 255.0;
                double GsRGB = color.G / 255.0;
                double BsRGB = color.B / 255.0;

                double R = (RsRGB <= 0.03928) ? RsRGB / 12.92 : Math.Pow((RsRGB + 0.055) / 1.055, 2.4);
                double G = (GsRGB <= 0.03928) ? GsRGB / 12.92 : Math.Pow((GsRGB + 0.055) / 1.055, 2.4);
                double B = (BsRGB <= 0.03928) ? BsRGB / 12.92 : Math.Pow((BsRGB + 0.055) / 1.055, 2.4);

                return 0.2126 * R + 0.7152 * G + 0.0722 * B;
            }

            double L1 = GetRelativeLuminance(foreground);
            double L2 = GetRelativeLuminance(background);

            double contrastRatio = (Math.Max(L1, L2) + 0.05) / (Math.Min(L1, L2) + 0.05);

            // WCAG AA 기준: 일반 텍스트 4.5:1, 큰 텍스트 3:1
            return contrastRatio >= 4.5;
        }
    }
}
