using System;
using System.Windows;
using TTS.VoiceEngine;

namespace TTS
{
    public partial class LaunchPoint : Application
    {
        private VoiceOrchestrator _welcome;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DoWelcome();
        }
        
        private void DoWelcome()
        {
            try
            {
                _welcome = new VoiceOrchestrator();
                
                var voices = _welcome.QueryAvailableVoices();
                var ko = voices.Find(v => v.IsKoreanVoice);
                
                if (ko != null)
                {
                    _welcome.SwitchToVoice(ko.Identifier);
                }
                
                _welcome.VocalizeText("환영합니다. 음성 안내 시스템입니다.");
                
                _welcome.OnTransition += pkt =>
                {
                    if (pkt.Phase == "completed")
                    {
                        _welcome?.Dispose();
                        _welcome = null;
                    }
                };
            }
            catch
            {
                // Silent fail
            }
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            _welcome?.Dispose();
            base.OnExit(e);
        }
    }
}
