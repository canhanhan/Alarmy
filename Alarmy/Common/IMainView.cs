using System;

namespace Alarmy.Common
{
    internal interface IMainView
    {
        event EventHandler OnNewRequest;
        event EventHandler<AlarmEventArgs> OnCancelRequest;
        event EventHandler<AlarmEventArgs> OnChangeRequest;
        event EventHandler<AlarmEventArgs> OnCompleteRequest;
        event EventHandler<AlarmEventArgs> OnHushRequest;
        event EventHandler<AlarmEventArgs> OnUnhushRequest;
        event EventHandler OnEnableSoundRequest;
        event EventHandler OnDisableSoundRequest;
        event EventHandler OnPopupOnAlarmOn;
        event EventHandler OnPopupOnAlarmOff;
        event EventHandler OnSmartAlarmOn;
        event EventHandler OnSmartAlarmOff;
        event EventHandler OnHideRequest;
        event EventHandler OnShowRequest;
        event EventHandler OnExitRequest;
        event EventHandler OnLoad;
        event EventHandler OnClosing;

        bool Visible { get; }
        bool SoundEnabled { get; set; }
        bool PopupOnAlarm { get; set; }
        bool SmartAlarm { get; set; }  

        string AskCancelReason(IAlarm alarm);
        AlarmMetadata AskAlarmMetadata(IAlarm alarm = null);

        void Show();
        void Hide();

        void PlayAlarm();
        void StopAlarm();

        void AddAlarm(IAlarm alarm);
        void RemoveAlarm(IAlarm alarm);
        void UpdateAlarm(IAlarm alarm);
    }
}
