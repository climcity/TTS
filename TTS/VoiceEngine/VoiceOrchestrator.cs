using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;

namespace TTS.VoiceEngine
{
    // 음성 오케스트레이터: 독특한 이벤트 체인 + 슬롯 기반 메모리 관리
    public class VoiceOrchestrator : IDisposable
    {
        private SpeechSynthesizer _synth;
        private JobSlotManager _slots;
        private SpeedController _speedCtrl;
        private GainController _gainCtrl;
        private int _activeCount;
        
        public delegate void StateTransition(VocalEventPacket pkt);
        public event StateTransition OnTransition;
        
        public VoiceOrchestrator()
        {
            _synth = new SpeechSynthesizer();
            _slots = new JobSlotManager(32);
            _speedCtrl = new SpeedController();
            _gainCtrl = new GainController();
            _activeCount = 0;
            
            _synth.SpeakCompleted += (s, e) => ProcessLifecycle("completed");
            _synth.SpeakStarted += (s, e) => ProcessLifecycle("started");
            
            SyncParameters();
        }
        
        public void VocalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            
            var job = new VocalJob
            {
                Text = text,
                Timestamp = DateTime.Now,
                Id = Guid.NewGuid().ToString("N").Substring(0, 10)
            };
            
            _slots.Add(job);
            System.Threading.Interlocked.Increment(ref _activeCount);
            
            try
            {
                _synth.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                System.Threading.Interlocked.Decrement(ref _activeCount);
                throw new InvalidOperationException($"Synthesis error: {ex.Message}", ex);
            }
        }
        
        public void HaltVocalization()
        {
            if (_activeCount > 0)
            {
                _synth.SpeakAsyncCancelAll();
                _slots.Clear();
                System.Threading.Interlocked.Exchange(ref _activeCount, 0);
            }
        }
        
        public void ModifyPitch(int delta)
        {
            _speedCtrl.Update(delta);
            SyncParameters();
        }
        
        public void ModifyAmplitude(int level)
        {
            _gainCtrl.Update(level);
            SyncParameters();
        }
        
        private void SyncParameters()
        {
            _synth.Rate = _speedCtrl.Value;
            _synth.Volume = _gainCtrl.Value;
        }
        
        public List<VoiceDescriptor> QueryAvailableVoices()
        {
            var results = new List<VoiceDescriptor>();
            var voices = _synth.GetInstalledVoices();
            
            int idx = 0;
            foreach (var v in voices)
            {
                if (!v.Enabled) continue;
                
                var info = v.VoiceInfo;
                var isKorean = info.Culture.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase);
                
                results.Add(new VoiceDescriptor
                {
                    Identifier = info.Name,
                    DisplayLabel = BuildLabel(info),
                    CultureCode = info.Culture.Name,
                    Gender = info.Gender.ToString(),
                    IsKoreanVoice = isKorean,
                    SequenceIndex = idx++
                });
            }
            
            return results.OrderByDescending(r => r.IsKoreanVoice ? 100 : 0).ThenBy(r => r.DisplayLabel).ToList();
        }
        
        private string BuildLabel(VoiceInfo info)
        {
            string genderIcon = info.Gender == VoiceGender.Female ? "♀" : info.Gender == VoiceGender.Male ? "♂" : "⊙";
            return $"{info.Name} [{info.Culture.DisplayName}] {genderIcon}";
        }
        
        public bool SwitchToVoice(string id)
        {
            try
            {
                _synth.SelectVoice(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public string CurrentVoiceIdentifier => _synth.Voice?.Name ?? "None";
        public bool IsActivelyVocalizing => _activeCount > 0;
        
        private void ProcessLifecycle(string phase)
        {
            if (phase == "completed")
            {
                System.Threading.Interlocked.Decrement(ref _activeCount);
            }
            
            OnTransition?.Invoke(new VocalEventPacket { Phase = phase, Time = DateTime.Now });
        }
        
        public void Dispose()
        {
            HaltVocalization();
            _synth?.Dispose();
        }
    }
    
    public class JobSlotManager
    {
        private VocalJob[] _array;
        private int _writeIdx;
        private int _size;
        
        public JobSlotManager(int capacity)
        {
            _size = capacity;
            _array = new VocalJob[capacity];
            _writeIdx = 0;
        }
        
        public void Add(VocalJob job)
        {
            _array[_writeIdx] = job;
            _writeIdx = (_writeIdx + 1) % _size;
        }
        
        public void Clear()
        {
            Array.Clear(_array, 0, _size);
            _writeIdx = 0;
        }
    }
    
    public struct VocalJob
    {
        public string Text;
        public DateTime Timestamp;
        public string Id;
    }
    
    public class SpeedController
    {
        private int _val;
        
        public void Update(int v)
        {
            _val = Math.Max(-10, Math.Min(10, v));
        }
        
        public int Value => _val;
    }
    
    public class GainController
    {
        private int _val;
        
        public GainController()
        {
            _val = 75;
        }
        
        public void Update(int v)
        {
            _val = Math.Max(0, Math.Min(100, v));
        }
        
        public int Value => _val;
    }
    
    public struct VocalEventPacket
    {
        public string Phase;
        public DateTime Time;
    }
    
    public class VoiceDescriptor
    {
        public string Identifier { get; set; }
        public string DisplayLabel { get; set; }
        public string CultureCode { get; set; }
        public string Gender { get; set; }
        public bool IsKoreanVoice { get; set; }
        public int SequenceIndex { get; set; }
    }
}
