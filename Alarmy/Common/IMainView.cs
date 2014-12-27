using System;

namespace Alarmy.Common
{
    internal interface IMainView
    {
        event EventHandler<AlarmEventArgs> OnCancelRequest;
        event EventHandler<AlarmEventArgs> OnChangeRequest;
        event EventHandler<AlarmEventArgs> OnCompleteRequest;
        event EventHandler<AlarmEventArgs> OnHushRequest;
        event EventHandler OnNewRequest;
        event EventHandler OnEnableSoundChange;
        event EventHandler OnPopupOnAlarmChange;
        event EventHandler OnSmartAlarmChange;
        event EventHandler OnHideRequest;
        event EventHandler OnExitRequest;
        event EventHandler OnLoad;
        event EventHandler OnClosing;

        bool EnableSound { get; set; }
        bool PopupOnAlarm { get; set; }
        bool SmartAlarm { get; set; }        
        bool Visible { get; set; }

        void Show();
        void Hide();

        string AskCancelReason(IAlarm alarm);
        AlarmMetadata AskAlarmMetadata(IAlarm alarm = null);

        void AddAlarm(IAlarm alarm);
        void RemoveAlarm(IAlarm alarm);
        void UpdateAlarm(IAlarm alarm);
    }
}
