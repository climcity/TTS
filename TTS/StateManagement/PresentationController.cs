using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace TTS.StateManagement
{
    public class PresentationController : INotifyPropertyChanged
    {
        private DataVault _vault;
        private CmdRouter _router;
        private PropBroadcaster _caster;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<ActionEventArgs> ActionTriggered;
        
        public PresentationController()
        {
            _vault = new DataVault();
            _router = new CmdRouter();
            _caster = new PropBroadcaster();
            
            SetupCommands();
            _caster.Register(prop => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)));
        }
        
        private void SetupCommands()
        {
            VocalizeCommand = _router.Build(
                () => Fire("Vocalize", _vault.TextBuf),
                () => !string.IsNullOrWhiteSpace(_vault.TextBuf) && !_vault.Busy
            );
            
            HaltCommand = _router.Build(
                () => Fire("Halt", null),
                () => _vault.Busy
            );
            
            ClearCommand = _router.Build(
                () => { CurrentText = ""; Fire("Clear", null); },
                () => true
            );
            
            InsertPhraseCommand = _router.BuildGeneric<string>(
                phrase => CurrentText = phrase,
                phrase => !string.IsNullOrEmpty(phrase)
            );
        }
        
        public string CurrentText
        {
            get => _vault.TextBuf;
            set
            {
                if (_vault.TextBuf != value)
                {
                    _vault.TextBuf = value;
                    _caster.Notify(nameof(CurrentText));
                    _caster.Notify(nameof(HasTextContent));
                }
            }
        }
        
        public int VocalSpeed
        {
            get => _vault.SpeedOff;
            set
            {
                int clamped = Bound(value, -10, 10);
                if (_vault.SpeedOff != clamped)
                {
                    _vault.SpeedOff = clamped;
                    _caster.Notify(nameof(VocalSpeed));
                    _caster.Notify(nameof(SpeedDisplayText));
                }
            }
        }
        
        public int VocalVolume
        {
            get => _vault.VolLvl;
            set
            {
                int clamped = Bound(value, 0, 100);
                if (_vault.VolLvl != clamped)
                {
                    _vault.VolLvl = clamped;
                    _caster.Notify(nameof(VocalVolume));
                    _caster.Notify(nameof(VolumeDisplayText));
                }
            }
        }
        
        public string SelectedVoice
        {
            get => _vault.VoiceIdStr;
            set
            {
                if (_vault.VoiceIdStr != value)
                {
                    _vault.VoiceIdStr = value;
                    _caster.Notify(nameof(SelectedVoice));
                }
            }
        }
        
        public bool IsProcessingVocalization
        {
            get => _vault.Busy;
            set
            {
                if (_vault.Busy != value)
                {
                    _vault.Busy = value;
                    _caster.Notify(nameof(IsProcessingVocalization));
                }
            }
        }
        
        public bool HasTextContent => !string.IsNullOrWhiteSpace(_vault.TextBuf);
        
        public string SpeedDisplayText
        {
            get
            {
                var map = new Dictionary<int, string>
                {
                    [-10] = "최저속도", [-7] = "저속", [-4] = "느림",
                    [0] = "표준", [3] = "빠름", [6] = "고속", [10] = "최고속"
                };
                
                foreach (var k in map.Keys.OrderByDescending(x => x))
                {
                    if (_vault.SpeedOff >= k) return map[k];
                }
                return "표준";
            }
        }
        
        public string VolumeDisplayText => $"{_vault.VolLvl}%";
        
        public ICommand VocalizeCommand { get; private set; }
        public ICommand HaltCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand InsertPhraseCommand { get; private set; }
        
        private void Fire(string type, object data)
        {
            ActionTriggered?.Invoke(this, new ActionEventArgs { ActionType = type, Data = data });
        }
        
        private int Bound(int v, int min, int max)
        {
            return Math.Max(min, Math.Min(max, v));
        }
        
        public void RefreshAllCommands()
        {
            _router.RefreshAll();
        }
    }
    
    internal class DataVault
    {
        public string TextBuf { get; set; } = "";
        public int SpeedOff { get; set; } = 0;
        public int VolLvl { get; set; } = 75;
        public string VoiceIdStr { get; set; } = null;
        public bool Busy { get; set; } = false;
    }
    
    internal class PropBroadcaster
    {
        private Action<string> _handler;
        
        public void Register(Action<string> h)
        {
            _handler = h;
        }
        
        public void Notify(string prop)
        {
            _handler?.Invoke(prop);
        }
    }
    
    internal class CmdRouter
    {
        private List<CmdNode> _nodes = new List<CmdNode>();
        
        public CmdNode Build(Action exec, Func<bool> canExec = null)
        {
            var node = new CmdNode(exec, canExec);
            _nodes.Add(node);
            return node;
        }
        
        public GenericCmdNode<T> BuildGeneric<T>(Action<T> exec, Func<T, bool> canExec = null)
        {
            return new GenericCmdNode<T>(exec, canExec);
        }
        
        public void RefreshAll()
        {
            foreach (var n in _nodes)
                n.Refresh();
        }
    }
    
    internal class CmdNode : ICommand
    {
        private Action _exec;
        private Func<bool> _can;
        
        public event EventHandler CanExecuteChanged;
        
        public CmdNode(Action exec, Func<bool> can)
        {
            _exec = exec;
            _can = can;
        }
        
        public bool CanExecute(object p) => _can?.Invoke() ?? true;
        public void Execute(object p) => _exec?.Invoke();
        public void Refresh() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    
    internal class GenericCmdNode<T> : ICommand
    {
        private Action<T> _exec;
        private Func<T, bool> _can;
        
        public event EventHandler CanExecuteChanged;
        
        public GenericCmdNode(Action<T> exec, Func<T, bool> can)
        {
            _exec = exec;
            _can = can;
        }
        
        public bool CanExecute(object p)
        {
            if (p == null && typeof(T).IsValueType) return false;
            return _can?.Invoke((T)p) ?? true;
        }
        
        public void Execute(object p) => _exec?.Invoke((T)p);
    }
    
    public class ActionEventArgs : EventArgs
    {
        public string ActionType { get; set; }
        public object Data { get; set; }
    }
}
