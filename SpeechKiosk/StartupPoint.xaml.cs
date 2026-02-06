using System.Windows;
using SpeechKiosk.Brain;

namespace SpeechKiosk
{
    /// <summary>
    /// 애플리케이션 시작점 StartupPoint
    /// </summary>
    public partial class StartupPoint : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 앱 시작 시 초기화 로직
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}
