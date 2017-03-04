# PrtgAPI

[![Build status](https://img.shields.io/appveyor/ci/lordmilko/prtgapi.svg)](https://ci.appveyor.com/project/lordmilko/prtgapi)

PrtgAPI is a C#/PowerShell library that abstracts away the complexity of interfacing with the [PRTG HTTP API](https://prtg.paessler.com/api.htm?tabid=2).

PrtgAPI implements a collection of methods and enumerations that help create and execute the varying HTTP GET requests required to interface with PRTG. Upon executing a request, PrtgAPI will deserialize the result into an object (Sensor, Device, Probe, etc) that the programmer can further interface with.

PrtgAPI also ships with a secondary, optional module *PrtgAPI.CustomSensors* which provides a collection of wrapper functions for generating output in *PRTG EXE/Script Advanced* custom sensors. For more information, see *PrtgAPI.CustomSensors* below.

## Installation

1. Download the [latest build](https://ci.appveyor.com/api/projects/lordmilko/prtgapi/artifacts/PrtgAPI/bin/Release/PrtgAPI.zip)
2. Right click **PrtgAPI.zip** -> **Properties**
3. On the *General* tab, under *Security* select **Unblock**
4. Unzip the file
5. Add a reference to *PrtgAPI.dll* to your project, or import the *PrtgAPI* module into PowerShell (see below). Alternatively, you can run the included **PrtgAPI.cmd** file to open a prompt and import the PrtgAPI module for you.

## Compilation

PrtgAPI requires Visual Studio 2015. If you wish to run any unit tests, ensure *Test -> Test Settings -> Keep Test Execution Engine Running* is unticked to prevent the PowerShell tests from locking the assemblies (preventing recompilation or moving the files somewhere else).

If you wish to run unit tests, it is advised to group the tests in *Test Explorer* by **Project**  to separate unit tests from integration tests.

If you wish to run integration tests, it is recommended to create a separate server for integration testing. When integration tests are run, PrtgAPI will create a backup of your PRTG configuration, run its tests, and then revert the server to its original settings. If integration tests do not run to their completion, as a safety measure you will be required to manually delete (or restore) the `PRTG Configuration.dat` file under %temp% on the PRTG Server.

To configure PrtgAPI for integration testing, please specify values for all fields listed in `PrtgAPI.Tests.IntegrationTests\Settings.cs` with values specific to your server. The server running integration tests must be able to connect directly to the server over the network. When specifying the credentials to connect to your server, you must specify a local user on the server; domain users are not currently supported.

## Usage (C#)
All actions in PrtgAPI revolve around a core class: `PrtgClient`

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "password");
```

When a `PrtgClient` is created, it will immediately attempt to retrieve your user's passhash (an alternative to using a password) from your PRTG Server. For added security, your PassHash is then used for all future PRTG Requests made during the life of your program.

For further security, you are able to use your passhash to `PrtgClient` instead of using your password. Simply create a breakpoint after the `PrtgClient` constructor call has executed, copy your passhash out of your `request` object's private `passhash` property then tell the constructor to use the passhash instead.

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "1234567890", AuthMode.PassHash);
```

### Lists

PrtgAPI provides a series of method overloads for retrieving all sorts of data in a variety of different ways.

To retrieve a list of all sensors, call the `GetSensors` method.

```c#
var sensors = client.GetSensors();
```

For groups, call `GetGroups`; Devices, call `GetDevices`, etc.

Typically however, you'll want to apply one or more filters to limit the number of objects returned (and increase the speed of the HTTP GET request).

```c#
//List all sensors in a "down" state.
var downSensors = client.GetSensors(SensorStatus.Down);
```
```c#
//List all devices under probes whose name contains "chicago"
var chicagoProbeDevices = client.GetDevices(Property.Probe, FilterOperator.Contains, "chicago");
```
```c#
//List all sensors under the Device with Object ID 2000.
var childSensors = client.GetSensors(Property.ParentId, 2000);
```

PrtgAPI methods that return values typically return a `List` of objects, allowing you to use LINQ to retrieve the values you're really after.

```c#
var names = client.GetSensors(SensorStatus.Unknown).Select(s => s.Name).ToList();
```

Many method parameters are implemented as `params`, allowing you to specify multiple values just by adding additional commas.

```c#
var variousSensors = client.GetSensors(SensorStatus.Down, SensorStatus.Up, SensorStatus.DownAcknowledged);
```
```c#
//Get all Ping sensors for devices whose name contains "dc" on the Perth Office probe.
var filters = new[]
{
    new SearchFilter(Property.Type, "ping"),
    new SearchFilter(Property.Device, FilterOperator.Contains, "dc"),
    new SearchFilter(Property.Probe, "Perth Office")
};

var perthDCPingSensors = client.GetSensors(filters);
```

### Notification Triggers

The Notification Triggers of an object can be accessed via the `GetNotificationTriggers` method

```c#
//Get Notification Triggers for the object with ID 1234
var triggers = client.GetNotificationTriggers(1234);
```

Notification triggers can also be added and modified via the `AddNotificationTrigger` and `SetNotificationTrigger` methods respectively.

To update triggers with `SetNotificationTrigger` you must construct a `TriggerParameters` object of the trigger type you wish to construct.

The following trigger parameter types are available:
* `StateTriggerParameters`
* `ChangeTriggerParameters`
* `SpeedTriggerParameters`
* `VolumeTriggerParameters`
* `ThresholdTriggerParameters`

Trigger parameter objects can be created in different ways for three different use cases:
* Creating a trigger from an existing trigger
* Editing an existing trigger
* Creating a new trigger from scratch.

```c#

//Create a new State Trigger on the object with ID 1234

var actions = client.GetNotificationActions();

var parameters = new StateTriggerParameters(1234) //By default sensor state "Down" will be used as the trigger
{
    OnNotificationAction = actions.First() //TriggerParameters have a variety of fields that can be specified
};

client.AddNotificationTrigger(parameters);
```

Some properties *must* have values in order to prevent issues with PRTG. Attempting to nullify these properties in parameters for creating new triggers will generate an exception.

When editing triggers, by default all properties are null. Only properties that are set on the object will be modified when the request is executed. If you decide you no longer wish to modify a property, setting it to `null` will remove it from the object.

```powershell

var parameters = new ThresholdTriggerParameters(1234, 1) //Modify the trigger with Sub ID 1 on the object with ID 1234
{
	Condition = TriggerCondition.Above //Change only the trigger's condition property
}

client.SetNotificationTrigger(parameters);

```

Setting a *notification action* to `null` when adding or editing trigger parameters will cause the action to be set to the *empty notification action*. As a result of this, any notification action properties that are set when editing a notification trigger cannot be unset without creating a new object.

Notification Triggers can also be removed from objects. Trigers can be removed either by specfying the the object ID/sub ID of the trigger, or a `NotificationTrigger` object retrieved from a previous call to `GetNotificationTriggers`

```c#
client.RemoveNotificationTrigger(1234, 3);

var trigger = client.GetNotificationTriggers(1234).First();
client.RemoveNotificationTrigger(trigger);
```

Please note: the object ID/sub ID overload of `RemoveNotificationTrigger` does not currently prevent you from removing a trigger from an object when that trigger is in fact inherited from another object. It is unknown whether PRTG allows this behaviour. The `NotificationTrigger` overload _does_ perform this check.

### Object Settings
Values of object settings can be enumerated and manipulated via two groups of overloaded methods: `GetObjectProperty` and `SetObjectProperty`

```c#
//Retrieve the name of object with ID 2001
var name = client.GetObjectProperty(2001, BasicObjectSetting.Name);
```
```c#
//Update the name of object with ID 2001
var name = client.SetObjectProperty(2001, BasicObjectSetting.Name, "a brand new name!");
```
By default, `GetObjectProperty` will return a `string` containing the value you requested. If you know for a fact the property is of another type (an enum defined by PrtgAPI, or an integer) you can request GetObjectProperty cast its return value to its "true" data type.

```c#
var priorityNum = client.GetObjectProperty<int>(2001, BasicObjectSetting.Priority);
var priorityEnum = client.GetObjectProperty<Priority>(2001, BasicObjectSetting.Priority);
```

### Pausing / Resuming

Objects can be paused and resumed using the `Pause` and `Resume` methods respectively. A message and a pause duration can be optionally specified. If no time is given, the object is paused indefinitely.

```c#
//Pause object with ID 2001 indefinitely
client.Pause(2001);
```
```c#
//Pause object with ID 2002 for 60 minutes
client.Pause(2002, 60, "Paused for the next 60 minutes!");
```
```c#
//Resume object with ID 2001
client.Resume(2001);
```

### Custom Requests
For those that which to execute custom requests (i.e. those not yet supported by PrtgAPI, or those not known to be supported by PRTG) it is possible to craft custom requests that do whatever you like.

```c#
//Specify the super secret "foobar" parameter, limiting the number of results returned to 100.
var parameters = new SensorParameters()
{
    [Parameter.Custom] = new CustomParameter("foobar", "1"),
    Count = 100
};

var sensors = client.GetSensors(parameters);
```
PrtgAPI implements a number of built-in parameter types that automatically specify the type of content their requests will retrieve. If you wish to implement your own custom parameters, you can do so by manipulating the base `Parameters` class.

## PowerShell

PrtgAPI features a number of PowerShell cmdlets that encapsulate the core functionality of the C# interface. When compiling, a _PrtgAPI_ folder will be created under the Debug/Release folder. You can then copy wherever you like and import into PowerShell, as follows:
```powershell
Import-Module "C:\path\to\PrtgAPI"
```

Once loaded, you can connect to your PRTG Server

```powershell
Connect-PrtgServer prtg.mycoolsite.com (Get-Credential) # ProTip: You can omit (Get-Credential)
```

To use your PassHash instead of your password, specify the `-PassHash` switch. If you do not know your PassHash, you can retrieve it once authenticated via `Get-PrtgClient`

```powershell
Connect-PrtgServer prtg.mycoolsite.com (Get-Credential) -PassHash
```

If you are scripting against PrtgAPI, you can use the included `New-Credential` cmdlet to bypass the authentication prompt.

```powershell
Connect-PrtgServer prtg.mycoolsite.com (New-Credential prtgadmin supersecretpassword)
```

The following cmdlets are currently supported

```powershell
Add-NotificationTrigger
Acknowledge-Sensor
Connect-PrtgServer
Disconnect-PrtgServer
Edit-NotificationTriggerProperty
Get-Channel
Get-Device
Get-Group
Get-NotificationAction
Get-NotificationTrigger
Get-Probe
Get-PrtgClient
Get-Sensor
Get-SensorTotals
New-Credential
New-NotificationTriggerParameter
New-SearchFilter
Pause-Object
Refresh-Object
Remove-NotificationTrigger
Remove-Object
Rename-Object
Set-ChannelProperty # Currently supports limit and spike related properties
Set-NotificationTrigger
```

All cmdlets include complete `Get-Help` documentation, including a cmdlet overview, parameter descriptions and example usages. For an overview of a cmdlet see `Get-Help <cmdlet>` or `Get-Help <cmdlet> -Full` for complete documentation.

### Examples

Get all ping sensors

```powershell
C:\> Get-Sensor ping # pipe to Format-List to view all properties!

Name                Id      Device      Group           Probe           Status
----                --      ------      -----           -----           ------
PING                2010    dc1         Servers         Local Probe     Up
Ping                2011    dc2         Servers         Local Probe     Down
Ping                2012    exch1       Servers         Remote Probe    DownAcknowledged
```
Get all devices whose names contain "dc"

```powershell
C:\> Get-Device *dc*

Name                Id      Status      Group           Probe
----                --      ------      -----           -----
dc1                 2001    Up          Servers         Local Probe
dc2                 2002    Down        Servers         Local Probe

```

If you request all sensors with no filter and have more than 500 sensors in your system, `Get-Sensor` will show a progress bar and execute as if it were an `IEnumerable<Task<>>`, generating multiple sensor requests to your server in parallel and printing results as they come in.

```powershell
Get-Sensor # For great justice!
```

Delete all sensors whose device name contains "banana"

```powershell
Get-Sensor -Filter (New-SearchFilter device contains banana)|Remove-Object
```

Get all WMI sensors

```powershell
Get-Sensor -Tags wmi*
```

Multiple filters can be specified to further limit the results (and speed up the query!)

```powershell
# Any method of creating an array will do

$a = New-SearchFilter name equals Ping # equals is case sensitive!
$b = New-SearchFilter device contains dc

Get-Sensor -Filter ($a,$b)
```
You can also filter via the pipeline
```powershell
 # Use the unary operator , to pipe all items at once!
,($a,$b) | Get-Sensor
```
Get the channels of a sensor

```powershell
C:\> Get-Sensor | Select -First 1 | Get-Channel

Name                SensorId    Id    LastValue LimitsEnabled UpperErrorLimit LowerErrorLimit ErrorLimitMessage
----                --------    --    --------- ------------- --------------- --------------- -----------------
Total               3001         0       0.32 %          True              95                 PANIC!! PANIC!!!
Processor 1         3001         1         <1 %         False
```

You can also get the channels of a sensor by specifying its Sensor ID
```powershell
Get-Channel -SensorId 1234
```

Acknowledge all down sensors

```powershell
# Sensors can be paused -Forever, -Until a given date, or for a specified -Duration (in minutes) with
# an optional -Message
Get-Sensor -Status Down | Acknowledge-Sensor -Until (Get-Date).AddDays(1) -Message "Hi Mom!"
```

Pause all acknowledged sensors forever.

```powershell
Get-Sensor -Status DownAck | Pause-Object -Forever # "DownAck" automatically resolves to "DownAcknowledged"
```

Cmdlets can be chained together, in order from outer object to inner object (i.e. Probe -> Group -> Group -> Device -> Sensor -> Channel)

```powershell
$sensors = Get-Probe | Select -Last 1 | Get-Group | Select -Last 2 | Get-Device | Select -First 1 | Get-Sensor
$sensors | Get-Channel perc* | Set-ChannelProperty UpperErrorLimit 100
```

Inner objects know how to receive a variety of objects via the pipeline. As such it is not necessary to include very intermediate object type

```powershell
Get-Probe | Get-Sensor
```

When using `Set-ChannelProperty` on channels that use custom units, take into account the unit when specifying your value. e.g. a sensor may have a "display value" in megabytes, however its actual value may be in *bytes*. You can confirm the numeric value of a channel by referring to the `LastValueNumeric` property.

```powershell
C:\> Get-Sensor *mem* | Get-Channel *mem* | fl Name,Last*

Name             : Percent Available Memory
LastValue        : 27 %
LastValueNumeric : 27

Name             : Available Memory
LastValue        : 1,116 MByte
LastValueNumeric : 1169711104
```

Notification Triggers can be retrieved via the `Get-NotificationTrigger` cmdlet.

```powershell

C:\> Get-Probe | Get-NotificationTrigger

Type        ObjectId SubId Inherited ParentId Latency Condition Threshold Units      OnNotificationAction
----        -------- ----- --------- -------- ------- --------- --------- -----      --------------------
Change      1        8     False     1                Change                         Ticket Notification
State       1        1     True      0        600     Equals    Down                 Email Administrator
Threshold   1        7     False     1        60      NotEquals 5                    Email PRTG Alerts Mailbox
Speed       1        5     False     1        60      Above     3         TByte/Day  SMS Escalation Team 1
Volume      1        6     False     1                Equals    6         KByte/Hour SMS Escalation Team 1
```

Triggers can be filtered to those of a specified type by specifying the `-Type` parameter. To filter out inherited triggers, specify `-Inherited $false`.

Notification Triggers and Actions can be added or edited via the `Edit-NotificationTriggerProperty`, `Add-NotificationTrigger` and `Set-NotificationTrigger` cmdlets.

`Edit-NotificationTriggerProperty` provides the simplest means of modifying a trigger, allowing you to pipe a variety of values across the pipeline for modifying a single property

```powershell
$action = Get-NotificationAction *admin* | Select -First 1
Get-Probe | Get-NotificationTrigger -Type Volume | Edit-NotificationTriggerProperty OnNotificationAction $action
```

For advanced scenarios requiring one or more properties be manipulated, the `Add-NotificationTrigger` and `Set-NotificationTrigger` cmdlets should be used. In order to add or edit a notification trigger, a `TriggerParameters` object must first be constructed, using the `New-NotificationTriggerParameter` cmdlet. `New-NotificationTriggerParameter` has a number of parameter sets depending on the operation you are trying to perform.

The easiest way to create a new `TriggerParameters` object is to pipe in an existing notification trigger. This will pre-populate all properties of the object with the properties of the existing notification trigger. This provides an excellent means of deploying or even moving triggers across a variety of PRTG Objects.

```powershell
# Migrate notification triggers defined on a probe to individual sensors

# Retrieve all objects that will be required for this operation
$probe = Get-Probe | Select -First 1
$triggers = $probe | Get-NotificationTrigger -Inherited $false
$sensors = $probe | Get-Sensor *cpu*

# Add the notification triggers to each sensor
foreach($sensor in $sensors)
{
	$triggers | New-NotificationTriggerParameter $sensor.Id | Add-NotificationTrigger
}

# Remove the triggers from their original source
$triggers | Remove-NotificationTrigger

```

When a trigger is piped to `New-NotificationTriggerParameter` along with a separate Object ID, the trigger parameters will be used to create a trigger on the specified object. If you omit an object ID, the trigger parameters will apply to the object ID specified in the existing `NotificationTrigger`.

```powershell
$trigger = Get-Device | Get-NotificationTrigger *admin* -Inherited $false -Type State | Select -First 1

$parameters = $trigger | New-NotificationTriggerParameter 
$parameters.Latency = 120

$parameters | Add-NotificationTrigger
```

### Access Underlying Methods

The underlying `PrtgClient` of a connection can be accessed via the `Get-PrtgClient` cmdlet. Accessing the `PrtgClient` object directly allows invoking methods from PowerShell that do not yet have cmdlet counterparts

```powershell
$parameters = CreateMyAwesomeObjectParameters
(Get-PrtgClient).UpdateAwesomeObject($parameters)
```

## PrtgAPI.CustomSensors

PrtgAPI.CustomSensors is an optional module for generating the XML output required by *EXE/Script Advanced* custom sensors. `PrtgAPI.CustomSensors` can be found in the build directory of your project, alongside `PrtgAPI`

Typically, to generate a response with one channel the following XML is required

```xml
<Prtg>
    <Result>
        <Channel>First Channel</Channel>
        <Value>10</Value>
    </Result>
</Prtg>
```
The equivalent XML can be generated as follows via PrtgAPI.CustomSensors
```powershell
Prtg {
    Result {
        Channel "First Channel"
        Value 10
    }
}
```

To import PrtgAPI.CustomSensors, run `Import-Module C:\path\to\PrtgAPI.CustomSensors`. If PrtgAPI.CustomSensors is on your PSModulePath, you can simply run `Import-Module PrtgAPI.CustomSensors`

All tags supported by *EXE/Script Advanced* sensors are supported by *PrtgAPI.CustomSensors*. For a list of tags that can be used in EXE/Script Advanced sensors, please see the [documentation on writing Custom Sensors](https://prtg.paessler.com/api.htm?tabid=7)

PrtgAPI.CustomSensors has no dependency on PrtgAPI, and can be installed and run completely separately without issue.