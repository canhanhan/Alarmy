using Alarmy.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alarmy.Tests.Utils
{
    internal abstract class FakeMainView : IMainView
    {
        public event EventHandler OnNewRequest;
        public event EventHandler OnImportRequest;
        public event EventHandler<AlarmEventArgs> OnCancelRequest;
        public event EventHandler<AlarmEventArgs> OnChangeRequest;
        public event EventHandler<AlarmEventArgs> OnCompleteRequest;
        public event EventHandler<AlarmEventArgs> OnHushRequest;
        public event EventHandler<AlarmEventArgs> OnUnhushRequest;

        public event EventHandler OnEnableSoundRequest;
        public event EventHandler OnDisableSoundRequest;
        public event EventHandler OnPopupOnAlarmOn;
        public event EventHandler OnPopupOnAlarmOff;
        public event EventHandler OnSmartAlarmOn;
        public event EventHandler OnSmartAlarmOff;
        public event EventHandler OnHideRequest;
        public event EventHandler OnShowRequest;
        public event EventHandler OnExitRequest;
        public event EventHandler OnLoad;
        public event EventHandler OnClosing;

        public bool Visible { get; set; }
        public bool IsMute { get; set; }
        public bool SoundEnabled { get; set; }
        public bool PopupOnAlarm { get; set; }
        public bool SmartAlarm { get; set; }

        public void SetSound(bool value)
        {
            this.SoundEnabled = value;
            if (this.SoundEnabled)
                this.OnEnableSoundRequest.Invoke();
            else
                this.OnDisableSoundRequest.Invoke();
        }

        public void SetPopupOnAlarm(bool value)
        {
            this.PopupOnAlarm = value;
            if (this.PopupOnAlarm)
                this.OnPopupOnAlarmOn.Invoke();
            else
                this.OnPopupOnAlarmOff.Invoke();
        }

        public void SetSmartAlarm(bool value)
        {
            this.SmartAlarm = value;
            if (this.SmartAlarm)
                this.OnSmartAlarmOn.Invoke();
            else
                this.OnSmartAlarmOff.Invoke();
        }

        public abstract void Show();

        public abstract void Hide();

        public abstract string AskCancelReason(IAlarm alarm);

        public abstract ImportContext AskImport();

        public abstract AlarmMetadata AskAlarmMetadata(IAlarm alarm = null);

        public abstract void PlayAlarm();

        public abstract void StopAlarm();

        public abstract void AddAlarm(IAlarm alarm);

        public abstract void RemoveAlarm(IAlarm alarm);

        public abstract void UpdateAlarm(IAlarm alarm);
    }
}
