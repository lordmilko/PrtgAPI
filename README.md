# PrtgAPI

[![Build status](https://img.shields.io/appveyor/ci/lordmilko/prtgapi.svg)](https://ci.appveyor.com/project/lordmilko/prtgapi)
[![NuGet](https://img.shields.io/nuget/v/PrtgAPI.svg)](https://www.nuget.org/packages/PrtgAPI/)
[![Coverage](https://img.shields.io/codecov/c/github/lordmilko/PrtgAPI.svg)](https://codecov.io/gh/lordmilko/PrtgAPI)

![PrtgAPI](https://raw.githubusercontent.com/lordmilko/PrtgAPI/master/assets/PrtgAPI.png)

PrtgAPI is a C#/PowerShell library for managing and maintaining [PRTG Network Monitor](https://www.paessler.com/prtg).

PrtgAPI abstracts away the complexity of interfacing with PRTG via a collection of type safe methods and cmdlets, enabling you to develop powerful applications for managing your network.

Useful things you can do with PrtgAPI:
* Generate reports based on custom queries
* Monitor the monitoring, such as missing sensors for backup jobs or devices for new domain members!
* Create and modify sensors, devices, groups, and more! Create objects from scratch, or clone from existing!
* Generate complex sensor factory definitions
* Deploy notification triggers to individual sensors for specific clients
* Maintain standard naming/alerting/object settings across your environment
* Pause/resume items from external systems (such as pre/post event scripts and scheduled tasks)
* Batch do *anything!*

For information on features that are currently in the pipeline, check out the [Roadmap](https://github.com/lordmilko/PrtgAPI/projects/1).

PrtgAPI also provides a secondary, optional module *PrtgAPI.CustomSensors* which provides a collection of wrapper functions for generating output in *PRTG EXE/Script Advanced* custom sensors. For more information, see [PrtgAPI.CustomSensors](https://github.com/lordmilko/PrtgAPI.CustomSensors).

## Installation

### NuGet

```powershell
Install-Package PrtgAPI
```

PrtgAPI is available on both [nuget.org](https://www.nuget.org/packages/PrtgAPI/) and [PowerShell Gallery](https://www.powershellgallery.com/packages/PrtgAPI/). Both packages are completely identical, however the nuget.org package also has corresponding symbols on symbolsource.org (for use with Visual Studio). In order to install PrtgAPI from the PowerShell Gallery you must be running PowerShell 5+.

If you have both the nuget.org and PowerShell Gallery package sources installed on your machine, you will need to specify the source you wish to install from, e.g.
```powershell
Install-Package PrtgAPI -Source PSGallery
```

### Manual

1. Download the [latest build](https://ci.appveyor.com/api/projects/lordmilko/prtgapi/artifacts/PrtgAPI.PowerShell/bin/Release/PrtgAPI.zip)
2. Right click **PrtgAPI.zip** -> **Properties**
3. On the *General* tab, under *Security* select **Unblock**
4. Unzip the file
5. Add a reference to *PrtgAPI.dll* to your project, or import the *PrtgAPI* module into PowerShell via `Import-Module C:\path\to\PrtgAPI`. Alternatively, you can run the included **PrtgAPI.cmd** file to open a prompt and import the PrtgAPI module for you.

## Compilation

For details on compiling PrtgAPI please see [the wiki](https://github.com/lordmilko/PrtgAPI/wiki/Compilation)

## Usage

For detailed usage instructions please see [the wiki](https://github.com/lordmilko/PrtgAPI/wiki).

The following provides a general overview of some of the capabilities of PrtgAPI. For more info on each section, click the appropriate link to be taken to the corresponding wiki page.

### Overview (C#)

#### Authentication

All actions in PrtgAPI revolve around a core class: [PrtgClient](https://github.com/lordmilko/PrtgAPI/wiki/Getting-Started)

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "password");
```

When a `PrtgClient` is created, it will immediately attempt to retrieve your account's passhash (an alternative to using a password) from your PRTG Server. For added security, your PassHash is then used for all future PRTG Requests made during the life of your program.

For further security, you are able to immediately pass your passhash to `PrtgClient` instead of using your password. Simply extract your passhash from your `client` object's `PassHash` property, then tell the `PrtgClient` constructor to use the passhash instead.

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "1234567890", AuthMode.PassHash);
```

#### Objects

[Many](https://github.com/lordmilko/PrtgAPI/wiki/Sensors) [types](https://github.com/lordmilko/PrtgAPI/wiki/Devices) [of](https://github.com/lordmilko/PrtgAPI/wiki/Probes) [objects](https://github.com/lordmilko/PrtgAPI/wiki/Channels) can be retrieved from PRTG.

```c#
//Get all devices present in your system
var devices = client.GetDevices();
```

PrtgAPI defines a variety of method overloads for filtering requested objects, from simple to [complex](https://github.com/lordmilko/PrtgAPI/wiki/Filters#c) search criteria.

```c#
//List all sensors in a "down" or "down acknowledged" state.
var downSensors = client.GetSensors(Status.Down, Status.DownAcknowledged).Select(s => s.Name).ToList();
```

```c#
//List all devices under probes whose name contains "chicago"
var chicagoProbeDevices = client.GetDevices(Property.Probe, FilterOperator.Contains, "chicago");
```

Async is fine too

```c#
//List all sensors under the Device with Object ID 2000.
var childSensors = await client.GetSensorsAsync(Property.ParentId, 2000);
```

You can even use `IQueryable`!

```c#
//Get all Ping sensors for devices whose name contains "dc" on the Perth Office probe.

var perthDCPingSensors = client.QuerySensors(
    s => s.Type == "ping" && s.Device.Contains("dc") && s.Probe == "perth Office"
).Take(3);
```

#### Object Manipulation

PrtgAPI can manipulate objects in a variety of ways, including [pausing, acknowledging and resuming](https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation), [reorganizing objects](https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization) as well as [retrieving and modifying object properties](https://github.com/lordmilko/PrtgAPI/wiki/Property-Manipulation).

```c#
//Acknowledge all down sensors for 10 minutes
var sensors = client.GetSensors(Status.Down);

foreach (var sensor in sensors)
{
    client.AcknowledgeSensor(sensor.Id, 10, "Go away!");
}
```
```c#
//Standardize all "Ping" sensors to using the name "Ping", without the whole word capitalized
var sensors = client.GetSensors(Property.Name, "ping");

foreach (var sensor in sensors)
{
    client.RenameObject(sensor.Id, "Ping");
}
```
```c#
//Set the upper error limit on the "Total" channel of all WMI CPU Load sensors to 90%
var sensors = client.GetSensors(Property.Tags, "wmicpuloadsensor");
var channels = sensors.SelectMany(s => client.GetChannels(s.Id, "Total"));

foreach (var channel in channels)
{
    client.SetObjectProperty(channel.SensorId, channel.Id, ChannelProperty.UpperErrorLimit, 90);
}
```

Many operations will let you specify multiple Object IDs at once, allowing you to modify potentially thousands of objects in a single request

```c#
//Acknowledge all down sensors indefinitely via a single request.
var sensors = client.GetSensors(Status.Down);

client.AcknowledgeSensor(sensors.Select(s => s.Id).ToArray());
```

#### Object Creation

You can construct an entirely new object hierarchy all within your application.

First, create a group

```c#
//Add a new group named "Servers" to the object with ID 1
var group = client.AddGroup(1, "Servers");
```

Then, add a device to it

```c#
//Add a new device named "dc-1" to the "Servers" group
var device = client.AddDevice(group.Id, "dc-1");
```

We'll add a new sensor

```c#
//Create a brand new Ping sensor and add it to our device
var pingParams = client.GetDynamicSensorParameters(device.Id, "ping");
var sensor = client.AddSensor(device.Id, pingParams).Single();
```

And since we've already defined some notification triggers elsewhere, let's copy one in!

```c#
//Get the "PagerDuty" notification trigger from the object with ID 2001
var existingTrigger = client.GetNotificationTriggers(2001).First(t => t.OnNotificationAction.Name.Contains("PagerDuty"));

//Add it to our Ping sensor
var triggerParams = new StateTriggerParameters(sensor.Id, existingTrigger);
var newTrigger = client.AddNotificationTrigger(triggerParams);
```

Madness!

For a comprehensive overview of the functionality of PrtgAPI and detailed usage instructions, please see [the wiki](https://github.com/lordmilko/PrtgAPI/wiki)

### Overview (PowerShell)

To connect to your PRTG server, simply run

```powershell
Connect-PrtgServer prtg.mycoolsite.com
```

You will then be prompted to enter your PRTG username and password. Your `PassHash` [can also be used](https://github.com/lordmilko/PrtgAPI/wiki/Getting-Started) instead of specifying your password.

If you are scripting against PrtgAPI, you can use the included `New-Credential` cmdlet to bypass the authentication prompt.

```powershell
Connect-PrtgServer prtg.mycoolsite.com (New-Credential prtgadmin supersecretpassword)
```

To avoid entering your username and password every time you use PrtgAPI, you can define [GoPrtg](https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials) connections in your `$Profile` to automatically connect for you.

If `Connect-PrtgServer` is executed outside of a script or the PowerShell ISE, PrtgAPI will by default display advanced progress details whenever two cmdlets are chained together. This can be overridden in a [variety of ways](https://github.com/lordmilko/PrtgAPI/wiki/Progress).

PrtgAPI supports a wide variety of operations, each of which taking pipeline input from each other where applicable

```powershell
Add-NotificationTrigger
Add-Device
Add-Group
Add-Sensor
Acknowledge-Sensor
Approve-Probe
Backup-PrtgConfig
Clear-PrtgCache
Clone-Object
Connect-GoPrtgServer
Connect-PrtgServer
Disable-PrtgProgress
Disconnect-PrtgServer
Edit-NotificationTriggerProperty
Enable-PrtgProgress
Get-Channel
Get-Device
Get-DeviceTemplate
Get-GoPrtgServer
Get-Group
Get-ModificationHistory
Get-NotificationAction
Get-NotificationTrigger
Get-Object
Get-ObjectLog
Get-ObjectProperty
Get-Probe
Get-PrtgClient
Get-PrtgSchedule
Get-PrtgStatus
Get-Sensor
Get-SensorFactorySource
Get-SensorHistory
Get-SensorTarget
Get-SensorTotals
Get-SensorType
Get-SystemInfo
Install-GoPrtgServer
Load-PrtgConfigFile
Move-Object
New-Credential
New-NotificationTriggerParameters
New-SearchFilter
New-Sensor
New-SensorFactoryDefinition
New-SensorParameters
New-DeviceParameters
New-GroupParameters
Open-PrtgObject
Pause-Object
Refresh-Object
Refresh-SystemInfo
Remove-NotificationTrigger
Remove-Object
Rename-Object
Restart-Probe
Restart-PrtgCore
Resume-Object
Set-ChannelProperty
Set-GoPrtgAlias
Set-ObjectPosition
Set-ObjectProperty
Set-PrtgClient
Set-NotificationTrigger
Simulate-ErrorStatus
Sort-PrtgObject
Start-AutoDiscovery
Uninstall-GoPrtgServer
Update-GoPrtgCredential
```

All cmdlets include complete `Get-Help` documentation, including a cmdlet overview, parameter descriptions and example usages. For an overview of a cmdlet see `Get-Help <cmdlet>` or `Get-Help <cmdlet> -Full` for complete documentation.

#### Objects

[Many](https://github.com/lordmilko/PrtgAPI/wiki/Sensors) [types](https://github.com/lordmilko/PrtgAPI/wiki/Devices) [of](https://github.com/lordmilko/PrtgAPI/wiki/Probes) [objects](https://github.com/lordmilko/PrtgAPI/wiki/Channels) can be retrieved from PRTG.

Get all ping [sensors](https://github.com/lordmilko/PrtgAPI/wiki/Sensors)

```powershell
C:\> Get-Sensor ping # pipe to Format-List to view all properties!

Name                Id      Device      Group           Probe           Status
----                --      ------      -----           -----           ------
PING                2010    dc1         Servers         Local Probe     Up
Ping                2011    dc2         Servers         Local Probe     Down
Ping                2012    exch1       Servers         Remote Probe    DownAcknowledged
```
Get all [devices](https://github.com/lordmilko/PrtgAPI/wiki/Devices) whose names contain "dc"

```powershell
C:\> Get-Device *dc*

Name                Id      Status    Host      Group           Probe
----                --      ------    ----      -----           -----
dc1                 2001    Up        10.0.0.1  Servers         Local Probe
dc2                 2002    Down      dc-2      Servers         Local Probe

```

Get the [channels](https://github.com/lordmilko/PrtgAPI/wiki/Channels) of a sensor

```powershell
C:\> Get-Sensor | Select -First 1 | Get-Channel

Name                SensorId    Id    LastValue LimitsEnabled UpperErrorLimit LowerErrorLimit ErrorLimitMessage
----                --------    --    --------- ------------- --------------- --------------- -----------------
Total               3001         0       0.32 %          True              95                 PANIC!! PANIC!!!
Processor 1         3001         1         <1 %         False
```

Inner objects know how to receive a variety of objects via the pipeline. As such it is not necessary to include every intermediate object type

```powershell
Get-Probe | Get-Sensor
```

#### Object Manipulation

[Delete](https://github.com/lordmilko/PrtgAPI/wiki/Object-Organization) all sensors whose device name contains "banana"

```powershell
Get-Device *banana* | Get-Sensor | Remove-Object
```

Objects can be [opened in your web browser](https://github.com/lordmilko/PrtgAPI/wiki/Miscellaneous) for viewing in the PRTG Web UI

```powershell
Get-Sensor -Count 2 | Open-PrtgObject
```

Properties/settings of objects can be [retrieved and modified](https://github.com/lordmilko/PrtgAPI/wiki/Property-Manipulation) via the `Get-ObjectProperty` and `Set-ObjectProperty` cmdlets respectively. Properties will automatically set the values of any properties they depend on to be activated

```powershell
# Retrieve all settings of sensor with ID 1001
Get-Sensor -Id 1001 | Get-ObjectProperty
```
```powershell
# Set the scanning interval of the device with ID 2002. Will also set InheritInterval to $false
Get-Device -Id 2002 | Set-ObjectProperty Interval 00:00:30
```

[Acknowledge](https://github.com/lordmilko/PrtgAPI/wiki/State-Manipulation) all down sensors

```powershell
# Sensors can be paused -Forever, -Until a given date, or for a specified -Duration (in minutes) with
# an optional -Message
Get-Sensor -Status Down | Acknowledge-Sensor -Until (Get-Date).AddDays(1) -Message "Hi Mom!"
```

Cmdlets can be chained together, in order from outer object to inner object (i.e. Probe -> Group -> Group -> Device -> Sensor -> Channel)

```powershell
$sensors = Get-Probe | Select -Last 1 | Get-Group | Select -Last 2 | Get-Device | Select -First 1 | Get-Sensor
$sensors | Get-Channel perc* | Set-ChannelProperty UpperErrorLimit 100
```

#### Object Creation

PowerShell makes it easy to construct complex object hierarchies

```powershell
# Create a new group and a new device
$device = Get-Probe contoso | Add-Group "Servers" | Add-Device "exch-1"

# Create sensors for all of your Exchange Services
$params = $device | Get-SensorTarget WmiService *exchange* -Params
$device | Add-Sensor $params
```

By throwing your data into a CSV, you can perform [arbitrarily complex configurations](https://github.com/lordmilko/PrtgAPI/issues/13#issuecomment-383493681) with minimal effort.

#### Other Objects

Notification triggers can be [retrieved, added and modified](https://github.com/lordmilko/PrtgAPI/wiki/Notification-Triggers), sensor factories can be [generated](https://github.com/lordmilko/PrtgAPI/wiki/Sensor-Factories), historical details can be [viewed](https://github.com/lordmilko/PrtgAPI/wiki/Historical-Information) and much much more. For full usage instructions on PrtgAPI please see [the wiki](https://github.com/lordmilko/PrtgAPI/wiki)

## Interesting Things PrtgAPI Does

* [Custom Deserialization](https://github.com/lordmilko/PrtgAPI/wiki/Interesting-Techniques#deserialization)
* Cmdlet Based Event Handlers
* Inter-Cmdlet Progress
* Securely Storing Credentials
* [Test Startup/Shutdown](https://github.com/lordmilko/PrtgAPI/wiki/Interesting-Techniques#test-startupstartdown)
* Test Server State Restoration
* Mock WriteProgress
* [Test Logging](https://github.com/lordmilko/PrtgAPI/wiki/Interesting-Techniques#logging)
* [PowerShell Property Binding](https://github.com/lordmilko/PrtgAPI/wiki/Interesting-Techniques#powershell-property-binding)
* Dynamic PowerShell Formats
* Hybrid IQueryable / IEnumerable requests
* [Debugging Expression Trees as C#](https://github.com/lordmilko/PrtgAPI/wiki/Interesting-Techniques#debugging-expression-trees-as-c)
