using System;
namespace Alarmy.Common
{
    internal interface ISessionStateProvider
    {
        event EventHandler SessionActivated;
        event EventHandler SessionDeactivated;
        event EventHandler SessionLocked;
        event EventHandler SessionUnlocked;
    }
}
