# Alarmy
Alarmy is a multi-user/multi-computer alarm software. Different users on the same computer or different computers can run the application using a shared database simultaneously. Alarms and their statuses are rapidly synchronized without any user interaction. When an alarm is created or completed; it gets immediately effective on all computers. 

The application provides very useful features for multi-user environment; such as automatically mutes when the screen is locked. These features are explained in details below. These features can be controlled for each different computer or user separately. For example; the alarm sound can be muted on front desk computers but enabled on back-of-house computers.   

The application registers itself automatically to the most-left side of the screen and does not collide with other windows. This allows users to use the application without needing the sound. Ringing alarms are indicated as blinking yellow background color. 

## Features  
This section describes available features in the application. These features can be controlled for each computer and each user separately using startup arguments.

### Sound  
Alarm sound can be enabled or disabled. The default ring is "Telephone" but can be customized by replacing the "alarm.wav" file in the application directory. 
Right click to a blank area and click to "Sound" to mute or unmute alarms.  

### Popup on Alarm  
The application can be minimized/hid by users. When "Popup on Alarm" is enabled, it will automatically pop-up while alarm is ringing. You may disable this behavior by right clicking to a blank area and unchecking "Popup on Alarm".  

### Smart Alarm  
If "Smart Alarm" is enabled, alarms will not ring when the screen is locked. This is useful on shared computers. In these environments, if a user locks the screen; others may not be able to unlock to hush the alarm. To switch off smart alarm; right click to a blank area and click to "Smart Alarm".  

### Basic Operations  
#### Hide  
You can hide the application by right clicking to a blank area and then clicking to "Hide". You can make the application visible again by clicking to the icon in the system tray (next to the clock).  

#### Creating Alarms  
1. Right click to blank area and click to "New Alarm"  
2. Enter alarm details and press "Save"

#### Editing Existing Alarm  
1. Right click to an alarm and click to "Change"
2. Make necessary changes and click to "Save"

### Alarm Actions  
#### Hush  
"Hush" silences a ringing alarm on all computers. This is suitable while the guest is being called by an agent.  
To silence an alarm, click to the ringing alarm and choose "Hush"
Hushed alarms are shown with "(Hushed)" appended to the Status.
Hush gets reset when the alarm is changed, completed or cancelled.  

#### Complete  
When the alarm action is completed (e.g. Wake up call is finished), right click to the ringing - or missed - alarm and click to "Complete".   
Completed alarms are indicated with green background color.  

#### Cancel   
When the action cannot be completed, it is advised to cancel alarms instead of completing for reporting purposes (e.g., Guest cancels the wake-up call). 
To cancel an alarm, right click to the alarm and click to "Cancel".  
Cancelled alarms are indicated with "Gray" background color.

## Arguments
-a=*path*, --alarmSound=*path*
Sets alarm sound wave file path (default: alarm.wav)

-c=*val*, --checkInterval=*val*
Sets alarm check interval in seconds (default: 15). This value specifies how frequently alarms should be checked.

-m, --mute
Mutes the application on start (default: false)

-np, --dontPopup 
Alarm window does not popup when an alarm is ringing. (default: false)

-ns, --noSmartAlarm
Disables Smart Alarm feature (default: false)

-h, --hidden
Hides alarm window during startup. (default: false)

-g=*val*, --alarmListGroupInterval=*val*
Sets groupping interval (in minutes) of alarm list. (default: 15)

-db=*path*, --database=*path*
Sets path of the database file (default: %TEMP%\alarms.db"

-f=*val*, --freshness=*val*
Sets max age of completed/cancelled alarms in minutes. These alarms will be purged from alarm database. (default: 120)

-ra, --remindAll
When this option is used, application shows a reminder window for each completed alarm. (default: false)

-ri=*val*, --reminderInterval=*val*
Sets interval (in seconds) of the reminders. (default: 300)

-rr=*val*, --repositoryInterval=*val*
Specifies how frequently (in seconds) database file should queried of database refresh. (default: 60)

## Configuration Options
Defaults of arguments described above can be modified from the configuration file (Alarmy.exe.config). There are few additional configuration options:

DatePickerFormat: Date format for the date picker in alarm form
ImportDateFormat: Date format of the dates of import source.
ImportCaptionFormat: String format for the caption value.
ImportCaptionPatterns: RegExp patterns for the caption values.
ImportHasHeaders: Has import file headers?
