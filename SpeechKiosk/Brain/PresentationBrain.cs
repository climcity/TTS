using System;
using System.ComponentModel;
using System.Windows.Input;
using SpeechKiosk.Audio;

namespace SpeechKiosk.Brain
{
    /// <summary>
    /// UI 상태 및 로직을 관리하는 PresentationBrain 클래스
    /// </summary>
    public class PresentationBrain : INotifyPropertyChanged
    {
        private VocalEngine _vocalEngine;
        
        // 상태 필드들
        private string _messageContent;
        private int _speedSetting;
        private int _amplitudeSetting;
        private string _selectedVoiceIdentifier;
        private bool _isAudioPlaying;
        private bool _useHighContrastMode;
        private double _textMagnification;

        public PresentationBrain()
        {
            _vocalEngine = new VocalEngine();
            
            // 초기 설정값
            _messageContent = string.Empty;
            _speedSetting = 0;  // 중간 속도
            _amplitudeSetting = 80;  // 80% 볼륨
            _textMagnification = 1.0;  // 기본 크기
            _useHighContrastMode = false;

            // 이벤트 연결
            _vocalEngine.SpeechStarted += (s, e) => IsAudioPlaying = true;
            _vocalEngine.SpeechCompleted += (s, e) => IsAudioPlaying = false;

            // 커맨드 초기화
            InitializeCommands();
        }

        #region 속성 (Properties)

        public string MessageContent
        {
            get => _messageContent;
            set
            {
                if (_messageContent != value)
                {
                    _messageContent = value;
                    OnPropertyChanged(nameof(MessageContent));
                }
            }
        }

        public int SpeedSetting
        {
            get => _speedSetting;
            set
            {
                if (_speedSetting != value)
                {
                    _speedSetting = value;
                    _vocalEngine.ModifyPitchLevel(value);
                    OnPropertyChanged(nameof(SpeedSetting));
                }
            }
        }

        public int AmplitudeSetting
        {
            get => _amplitudeSetting;
            set
            {
                if (_amplitudeSetting != value)
                {
                    _amplitudeSetting = value;
                    _vocalEngine.ModifyVolumeLevel(value);
                    OnPropertyChanged(nameof(AmplitudeSetting));
                }
            }
        }

        public string SelectedVoiceIdentifier
        {
            get => _selectedVoiceIdentifier;
            set
            {
                if (_selectedVoiceIdentifier != value)
                {
                    _selectedVoiceIdentifier = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        _vocalEngine.SelectVoiceByName(value);
                    }
                    OnPropertyChanged(nameof(SelectedVoiceIdentifier));
                }
            }
        }

        public bool IsAudioPlaying
        {
            get => _isAudioPlaying;
            private set
            {
                if (_isAudioPlaying != value)
                {
                    _isAudioPlaying = value;
                    OnPropertyChanged(nameof(IsAudioPlaying));
                }
            }
        }

        public bool UseHighContrastMode
        {
            get => _useHighContrastMode;
            set
            {
                if (_useHighContrastMode != value)
                {
                    _useHighContrastMode = value;
                    OnPropertyChanged(nameof(UseHighContrastMode));
                }
            }
        }

        public double TextMagnification
        {
            get => _textMagnification;
            set
            {
                if (_textMagnification != value)
                {
                    _textMagnification = value;
                    OnPropertyChanged(nameof(TextMagnification));
                }
            }
        }

        #endregion

        #region 커맨드 (Commands)

        public ICommand VocalizeCommand { get; private set; }
        public ICommand StopVocalizationCommand { get; private set; }
        public ICommand ClearTextCommand { get; private set; }
        public ICommand QuickPhraseCommand { get; private set; }
        public ICommand ToggleContrastCommand { get; private set; }
        public ICommand IncreaseFontCommand { get; private set; }
        public ICommand DecreaseFontCommand { get; private set; }

        private void InitializeCommands()
        {
            VocalizeCommand = new ActionCommand(
                execute: () => {
                    if (!string.IsNullOrWhiteSpace(MessageContent))
                    {
                        _vocalEngine.VocalizeText(MessageContent);
                    }
                },
                canExecute: () => !string.IsNullOrWhiteSpace(MessageContent) && !IsAudioPlaying
            );

            StopVocalizationCommand = new ActionCommand(
                execute: () => _vocalEngine.HaltVocalization(),
                canExecute: () => IsAudioPlaying
            );

            ClearTextCommand = new ActionCommand(
                execute: () => MessageContent = string.Empty,
                canExecute: () => !string.IsNullOrWhiteSpace(MessageContent)
            );

            QuickPhraseCommand = new ActionCommand<string>(
                execute: (phrase) => {
                    MessageContent = phrase;
                    _vocalEngine.VocalizeText(phrase);
                }
            );

            ToggleContrastCommand = new ActionCommand(
                execute: () => UseHighContrastMode = !UseHighContrastMode
            );

            IncreaseFontCommand = new ActionCommand(
                execute: () => {
                    if (TextMagnification < 2.0)
                        TextMagnification += 0.25;
                }
            );

            DecreaseFontCommand = new ActionCommand(
                execute: () => {
                    if (TextMagnification > 0.75)
                        TextMagnification -= 0.25;
                }
            );
        }

        #endregion

        #region 메서드 (Methods)

        /// <summary>
        /// 환영 메시지 자동 재생
        /// </summary>
        public void PlayWelcomeAnnouncement()
        {
            var welcomeMsg = "환영합니다. 음성 안내 시스템입니다. 키오스크를 이용하실 수 있습니다.";
            _vocalEngine.VocalizeText(welcomeMsg);
        }

        /// <summary>
        /// 사용 가능한 음성 목록 가져오기
        /// </summary>
        public System.Collections.Generic.List<VoiceDescriptor> GetAvailableVoicesList()
        {
            return _vocalEngine.GetAvailableVoices();
        }

        #endregion

        #region INotifyPropertyChanged 구현

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// 커스텀 ActionCommand 구현 (일반 액션용)
    /// </summary>
    public class ActionCommand : ICommand
    {
        private readonly Action _executeAction;
        private readonly Func<bool> _canExecuteFunc;

        public ActionCommand(Action execute, Func<bool> canExecute = null)
        {
            _executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteFunc = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc == null || _canExecuteFunc();
        }

        public void Execute(object parameter)
        {
            _executeAction();
        }
    }

    /// <summary>
    /// 커스텀 ActionCommand 구현 (파라미터가 있는 액션용)
    /// </summary>
    public class ActionCommand<T> : ICommand
    {
        private readonly Action<T> _executeAction;
        private readonly Func<T, bool> _canExecuteFunc;

        public ActionCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _executeAction = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecuteFunc = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecuteFunc == null || _canExecuteFunc((T)parameter);
        }

        public void Execute(object parameter)
        {
            _executeAction((T)parameter);
        }
    }
}
