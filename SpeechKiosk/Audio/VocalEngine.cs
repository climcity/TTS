using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;

namespace SpeechKiosk.Audio
{
    /// <summary>
    /// 음성 출력을 담당하는 엔진 (VocalEngine - 독창적인 음성 처리 클래스)
    /// </summary>
    public class VocalEngine : IDisposable
    {
        private SpeechSynthesizer _synthesizer;
        private bool _isCurrentlySpeaking;
        private Queue<string> _speechQueue;

        // 음성 속성을 위한 커스텀 구조체
        public struct VoiceCharacteristics
        {
            public int PitchLevel { get; set; }  // -10 ~ +10
            public int VolumeLevel { get; set; }  // 0 ~ 100
            public string VoiceName { get; set; }
        }

        private VoiceCharacteristics _currentCharacteristics;

        public event EventHandler<string> SpeechCompleted;
        public event EventHandler<string> SpeechStarted;

        public VocalEngine()
        {
            _synthesizer = new SpeechSynthesizer();
            _speechQueue = new Queue<string>();
            _isCurrentlySpeaking = false;
            
            // 기본 음성 설정
            _currentCharacteristics = new VoiceCharacteristics
            {
                PitchLevel = 0,
                VolumeLevel = 80,
                VoiceName = ""
            };

            _synthesizer.SpeakCompleted += OnSynthesizerCompleted;
            _synthesizer.SpeakStarted += OnSynthesizerStarted;
        }

        /// <summary>
        /// 시스템에 설치된 음성 목록을 가져옴
        /// </summary>
        public List<VoiceDescriptor> GetAvailableVoices()
        {
            var voiceList = new List<VoiceDescriptor>();
            var installedVoices = _synthesizer.GetInstalledVoices();

            foreach (var voice in installedVoices)
            {
                if (voice.Enabled)
                {
                    var info = voice.VoiceInfo;
                    voiceList.Add(new VoiceDescriptor
                    {
                        DisplayName = info.Name,
                        Language = info.Culture.DisplayName,
                        IsKorean = info.Culture.TwoLetterISOLanguageName == "ko",
                        Gender = info.Gender.ToString()
                    });
                }
            }

            return voiceList;
        }

        /// <summary>
        /// 텍스트를 음성으로 변환하여 출력 (비동기)
        /// </summary>
        public void VocalizeText(string textContent)
        {
            if (string.IsNullOrWhiteSpace(textContent))
                return;

            try
            {
                ApplyCurrentCharacteristics();
                _synthesizer.SpeakAsync(textContent);
                _isCurrentlySpeaking = true;
            }
            catch (Exception ex)
            {
                // 음성 출력 실패 시 예외 처리
                System.Diagnostics.Debug.WriteLine($"음성 출력 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 현재 재생 중인 음성을 중지
        /// </summary>
        public void HaltVocalization()
        {
            if (_isCurrentlySpeaking)
            {
                _synthesizer.SpeakAsyncCancelAll();
                _isCurrentlySpeaking = false;
            }
        }

        /// <summary>
        /// 음성의 피치(속도)를 조정
        /// </summary>
        public void ModifyPitchLevel(int pitchValue)
        {
            // 범위 제한: -10 ~ +10
            pitchValue = Math.Max(-10, Math.Min(10, pitchValue));
            _currentCharacteristics.PitchLevel = pitchValue;
        }

        /// <summary>
        /// 음량 레벨을 조정
        /// </summary>
        public void ModifyVolumeLevel(int volumeValue)
        {
            // 범위 제한: 0 ~ 100
            volumeValue = Math.Max(0, Math.Min(100, volumeValue));
            _currentCharacteristics.VolumeLevel = volumeValue;
        }

        /// <summary>
        /// 사용할 음성을 선택
        /// </summary>
        public bool SelectVoiceByName(string voiceName)
        {
            try
            {
                _synthesizer.SelectVoice(voiceName);
                _currentCharacteristics.VoiceName = voiceName;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 현재 설정된 음성 특성을 적용
        /// </summary>
        private void ApplyCurrentCharacteristics()
        {
            _synthesizer.Rate = _currentCharacteristics.PitchLevel;
            _synthesizer.Volume = _currentCharacteristics.VolumeLevel;
            
            if (!string.IsNullOrEmpty(_currentCharacteristics.VoiceName))
            {
                try
                {
                    _synthesizer.SelectVoice(_currentCharacteristics.VoiceName);
                }
                catch
                {
                    // 음성 선택 실패 시 기본 음성 사용
                }
            }
        }

        private void OnSynthesizerCompleted(object sender, SpeakCompletedEventArgs e)
        {
            _isCurrentlySpeaking = false;
            SpeechCompleted?.Invoke(this, "음성 출력 완료");
        }

        private void OnSynthesizerStarted(object sender, SpeakStartedEventArgs e)
        {
            SpeechStarted?.Invoke(this, "음성 출력 시작");
        }

        public bool IsSpeaking => _isCurrentlySpeaking;

        public void Dispose()
        {
            if (_synthesizer != null)
            {
                _synthesizer.SpeakAsyncCancelAll();
                _synthesizer.Dispose();
                _synthesizer = null;
            }
        }
    }

    /// <summary>
    /// 음성 정보를 담는 커스텀 구조체
    /// </summary>
    public class VoiceDescriptor
    {
        public string DisplayName { get; set; }
        public string Language { get; set; }
        public bool IsKorean { get; set; }
        public string Gender { get; set; }
    }
}
