# PrtgAPI

[![Build status](https://img.shields.io/appveyor/ci/lordmilko/prtgapi.svg)](https://ci.appveyor.com/project/lordmilko/prtgapi)
[![NuGet](https://img.shields.io/nuget/v/PrtgAPI.svg)](https://www.nuget.org/packages/PrtgAPI/)

PrtgAPI is a C#/PowerShell library that abstracts away the complexity of interfacing with the [PRTG HTTP API](https://prtg.paessler.com/api.htm?tabid=2&username=demo&password=demodemo).

PrtgAPI implements a collection of methods and enumerations that help create and execute the varying HTTP GET requests required to interface with PRTG. Upon executing a request, PrtgAPI will deserialize the result into an object (Sensor, Device, Probe, etc) that the programmer can further interface with.

PrtgAPI also provides a secondary, optional module *PrtgAPI.CustomSensors* which provides a collection of wrapper functions for generating output in *PRTG EXE/Script Advanced* custom sensors. For more information, see [PrtgAPI.CustomSensors](https://github.com/lordmilko/PrtgAPI.CustomSensors).

Useful things you can do with PrtgAPI:
* Generate reports based on custom queries
* Monitor missing sensors (such as Veeam Backups) and devices (in your domain)
* Create and modify new sensors (from existing ones)
* Deploy notification triggers to individual sensors for specific clients
* Maintain standard naming/alerting settings across your environment
* Pause/resume items from external systems (such as pre/post event scripts and scheduled tasks)

## Installation

### NuGet

```powershell
Install-Package PrtgAPI
```

PrtgAPI is available on both [nuget.org](https://www.nuget.org/packages/PrtgAPI/) and [PowerShell Gallery](https://www.powershellgallery.com/packages/PrtgAPI/). Both packages are completely identical, however the nuget.org package also has corresponding symbols on symbolsource.org (for use with Visual Studio).

If you have both the nuget.org and PowerShell Gallery package sources installed on your machine, you will need to specify the source you wish to install from, e.g.
```powershell
Install-Package PrtgAPI -Source PSGallery
```

### Manual

1. Download the [latest build](https://ci.appveyor.com/api/projects/lordmilko/prtgapi/artifacts/PrtgAPI/bin/Release/PrtgAPI.zip)
2. Right click **PrtgAPI.zip** -> **Properties**
3. On the *General* tab, under *Security* select **Unblock**
4. Unzip the file
5. Add a reference to *PrtgAPI.dll* to your project, or import the *PrtgAPI* module into PowerShell (see below). Alternatively, you can run the included **PrtgAPI.cmd** file to open a prompt and import the PrtgAPI module for you.

## Compilation

PrtgAPI requires Visual Studio 2015. If you wish to run any unit tests, ensure *Test -> Test Settings -> Keep Test Execution Engine Running* is unticked to prevent the PowerShell tests from locking the assemblies (preventing recompilation or moving the files somewhere else).

If you wish to run unit tests, it is advised to group the tests in *Test Explorer* by **Project**  to separate unit tests from integration tests.

If you wish to run integration tests, it is recommended to create a separate server for integration testing. When integration tests are run, PrtgAPI will create a backup of your PRTG configuration, run its tests, and then revert the server to its original settings. If integration tests do not run to their completion, PrtgAPI will automatically restore the previous config upon next executing integration tests.

To configure PrtgAPI for integration testing, please specify values for all fields listed in `PrtgAPI.Tests.IntegrationTests\Settings.cs` with values specific to your server. The system running integration tests must be able to connect directly to the server over the network. When specifying the credentials to connect to your server, you must specify a local user on the server; domain users are not currently supported.

## Usage (C#)
All actions in PrtgAPI revolve around a core class: `PrtgClient`

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "password");
```

When a `PrtgClient` is created, it will immediately attempt to retrieve your account's passhash (an alternative to using a password) from your PRTG Server. For added security, your PassHash is then used for all future PRTG Requests made during the life of your program.

For further security, you are able to pass your passhash to `PrtgClient` instead of using your password. Simply extract your passhash from your `client` object's `PassHash` property, then tell the `PrtgClient` constructor to use the passhash instead.

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
var downSensors = client.GetSensors(Status.Down);
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
var names = client.GetSensors(Status.Unknown).Select(s => s.Name).ToList();
```

Many method parameters are implemented as `params`, allowing you to specify multiple values just by adding additional commas.

```c#
var variousSensors = client.GetSensors(Status.Down, Status.Up, Status.DownAcknowledged);
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

**Warning: PrtgAPI does not currently properly handle channel based triggers (such as Threshold triggers) applied directly to sensor objects. Avoid using PrtgAPI to interact with these triggers**

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

Notification Triggers can also be removed from objects. Triggers can be removed either by specfying the the object ID/sub ID of the trigger, or a `NotificationTrigger` object retrieved from a previous call to `GetNotificationTriggers`

```c#
client.RemoveNotificationTrigger(1234, 3);

var trigger = client.GetNotificationTriggers(1234).First();
client.RemoveNotificationTrigger(trigger);
```

Please note: the object ID/sub ID overload of `RemoveNotificationTrigger` does not currently prevent you from removing a trigger from an object when that trigger is in fact inherited from another object. It is unknown whether PRTG allows this behaviour. The `RemoveNotificationTrigger` overload that takes a `NotificationTrigger` as its parameter _does_ perform this check.

### Object Settings
Values of object settings can be enumerated and manipulated via two groups of overloaded methods: `Get<type>Properties` and `SetObjectProperty` where `<type>` is one of Sensor, Device, Group or Probe.

```c#
//Retrieve all properties of sensor 2001
var settings = client.GetSensorProperties(2001);
```

If a property has a dependent property (e.g. the Inheritance setting of a section), modifying the child property will automatically update the parent property such that the child properly can properly take effect
```c#
//Update the scanning interval of object with ID 2001. Will also set ObjectProperty.InheritInterval to false.
client.SetObjectProperty(2001, ObjectProperty.Interval, ScanningInterval.ThirtySeconds);
```

Properties supported by `SetObjectProperty` are type safe. When a value is specified, PrtgAPI looks up the type of the object against the corresponding property that would be returned in `Get<type>Properties`.
Values do not necessarily need to exactly match the target property type (e.g. string values for enums), however if PrtgAPI cannot parse the value into the target type an exception will be thrown indicating the failure.

```c#
//Specify the string representation of a ScanningInterval. TimeSpans are also supported
client.SetObjectProperty(2001, ObjectProperty.Interval, "00:00:30");
```

Channel settings can also be manipulated
```c#
//Set the upper error limit of channel 1 of the sensor with object ID 2001 to 30
client.SetObjectProperty(2001, 1, ChannelProperty.UpperErrorLimit, 30);
```

Settings that are not currently by PrtgAPI can still be interfaced with, via `GetObjectPropertyRaw` and `SetObjectPropertyRaw` respectively. Raw property methods are not type safe, and can cause minor corruption to objects
if not used carefully (can be easily rectified in the PRTG UI).

To use raw methods, the raw property name must be specified. This can typically be determined by inspecting the `name` attribute of each setting's corresponding `<input/>` tag

```c#
//Update the name property of object with ID 1001
client.SetObjectProperty(1001, "name_", "newName");
```

Typically property names end in an underscore, with the exception being properties that control inheritance. If an invalid property name is specified, PRTG will silently ignore your request. As such, it is important to verify
your property name works before continuing to use it in your program.

### Pausing / Resuming

Objects can be paused and resumed using the `Pause` and `Resume` methods respectively. A message and a pause duration can be optionally specified. If no time is given, the object is paused indefinitely.

```c#
//Pause object with ID 2001 indefinitely
client.PauseObject(2001);
```
```c#
//Pause object with ID 2002 for 60 minutes
client.PauseObject(2002, 60, "Paused for the next 60 minutes!");
```
```c#
//Resume object with ID 2001
client.ResumeObject(2001);
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

If you have installed PrtgAPI from NuGet or have placed PrtgAPI on your PSModulePath, you do not need to run `Import-Module`.

To connect to your PRTG Server, first run

```powershell
Connect-PrtgServer prtg.mycoolsite.com
```

You will then be prompted to enter your PRTG username and password. To use your PassHash instead of your password, specify the `-PassHash` switch. If you do not know your PassHash, you can retrieve it once authenticated via `Get-PrtgClient`

```powershell
Connect-PrtgServer prtg.mycoolsite.com -PassHash
```

If you are scripting against PrtgAPI, you can use the included `New-Credential` cmdlet to bypass the authentication prompt.

```powershell
Connect-PrtgServer prtg.mycoolsite.com (New-Credential prtgadmin supersecretpassword)
```

To avoid entering your username and password every time you use PrtgAPI, you can define `GoPrtg` connections in your `$Profile` to automatically connect for you.

```powershell
Install-GoPrtgServer
```
```powershell
# Connect to your preferred PRTG Server
GoPrtg
```

`GoPrtg` supports storing multiple servers, each with an optional alias. `GoPrtg` should only be used for casual automation on your work PC. For scripting purposes it is recommended to use `Get-Credential` or `New-Credential` instead. For more information on GoPrtg please see the [wiki](https://github.com/lordmilko/PrtgAPI/wiki/Store-Credentials).

The following cmdlets are currently supported by PrtgAPI

```powershell
Add-NotificationTrigger
Acknowledge-Sensor
Clone-Device
Clone-Group
Clone-Sensor
Connect-GoPrtgServer
Connect-PrtgServer
Disable-PrtgProgress
Disconnect-PrtgServer
Edit-NotificationTriggerProperty
Enable-PrtgProgress
Get-Channel
Get-Device
Get-GoPrtgServer
Get-Group
Get-ModificationHistory
Get-NotificationAction
Get-NotificationTrigger
Get-Probe
Get-PrtgClient
Get-Sensor
Get-SensorHistory
Get-SensorTotals
Install-GoPrtgServer
Move-Object
New-Credential
New-NotificationTriggerParameter
New-SearchFilter # Alias: flt
New-SensorFactoryDefinition
Open-PrtgObject
Pause-Object
Refresh-Object
Remove-NotificationTrigger
Remove-Object
Rename-Object
Resume-Object
Set-ChannelProperty
Set-GoPrtgAlias
Set-ObjectPosition
Set-ObjectProperty # See Get-Help about_SensorSettings for currently supported properties
Set-NotificationTrigger
Simulate-ErrorStatus
Sort-PrtgObject
Start-AutoDiscovery
Uninstall-GoPrtgServer
Update-GoPrtgCredential
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
Get-Sensor -Filter (New-SearchFilter device contains banana) | Remove-Object
```

The same thing can also be done using the `Get-Device` and `Get-Sensor` cmdlets in parallel, however note for each device returned a separate call will be made to retrieve each device's sensors

```powershell
Get-Device *banana* | Get-Sensor | Remove-Object # Easier to remember, but less computationally efficient
```

Get all WMI sensors

```powershell
Get-Sensor -Tags wmi*
```

Objects can be opened in your web browser for viewing in the PRTG Web UI

```powershell

Get-Sensor -Count 2 | Open-PrtgObject

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

Properties/settings of objects can be retrieved and modified via the `Get-ObjectProperty` and `Set-ObjectProperty` cmdlets respectively.

```powershell
# Retrieve all settings of sensor with ID 1001
Get-Sensor -Id 1001 | Get-ObjectProperty
```
Properties will automatically set the values of any properties they depend on to be activated
```powershell
# Set the scanning interval of the device with ID 2002. Will also set InheritInterval to $false
Get-Device -Id 2002 | Set-ObjectProperty Interval "00:00:30"
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

Inner objects know how to receive a variety of objects via the pipeline. As such it is not necessary to include every intermediate object type

```powershell
Get-Probe | Get-Sensor
```

If `Connect-PrtgServer` is executed outside of a script or the PowerShell ISE, PrtgAPI will by default display advanced progress details whenever two cmdlets are chained together. This can be overridden by either setting the `$ProgressPreference` within PowerShell, specifying `-Progress` parameter with `Connect-PrtgServer`, or by using the `Enable-PrtgProgress` and `Disable-PrtgProgress` cmdlets respectively.

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

When entering decimal numbers with `Set-ChannelProperty` it is important the number format matches that of the PRTG Server; i.e. if PRTG uses commas to represent decimal places, you too must use commas to represent decimal places. If the correct number format is not used, PRTG will likely truncate the specified value, leading to undesirable results. As PowerShell treats commas as arrays, it is important to indicate to PowerShell your value is not an array, such as by representing it as a string

```powershell
$channels | Set-ChannelProperty UpperErrorLimit "1,3"
```

Notification Triggers can be retrieved via the `Get-NotificationTrigger` cmdlet.

**Warning: PrtgAPI does not currently properly handle channel based triggers (such as Threshold triggers) applied directly to sensor objects. Avoid using PrtgAPI to interact with these triggers**

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

For advanced scenarios requiring multiple properties be manipulated, the `Add-NotificationTrigger` and `Set-NotificationTrigger` cmdlets should be used. In order to add or edit a notification trigger, a `TriggerParameters` object must first be constructed using the `New-NotificationTriggerParameter` cmdlet. `New-NotificationTriggerParameter` has a number of parameter sets depending on the operation you are trying to perform.

The easiest way to create a new `TriggerParameters` object is to pipe in an existing notification trigger. This will pre-populate all properties of the object with the properties of the existing trigger. This provides an excellent means of deploying or even moving triggers across a variety of PRTG Objects.

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

$parameters | Set-NotificationTrigger
```

### Access Underlying Methods

The underlying `PrtgClient` of a connection can be accessed via the `Get-PrtgClient` cmdlet. Accessing the `PrtgClient` object directly allows invoking methods from PowerShell that do not yet have cmdlet counterparts

```powershell
$parameters = CreateMyAwesomeObjectParameters
(Get-PrtgClient).UpdateAwesomeObject($parameters)
```

## Intersting Things PrtgAPI Does

### PrtgAPI

#### Deserialization

Often the values reported by PRTG need to be transformed in some way during or after the deserialization process, such as an empty string being converted into `null` or a DateTime/TimeSpan being correctly parsed. As the `System.Xml` `XmlSerializer` requires that all target properties be `public`, this presents a variety of issues, requiring "dummy" properties that accept the raw deserialized output and then are correctly parsed via the getter of the "actual" property. Such "raw" properties make a mess of your object's interface, bloat your intellisense and mess up your PowerShell output.

PrtgAPI works around this by implementing its own custom `XmlSerializer`. Unlike the built in `XmlSerializer` which generates a dynamic assembly for highly efficient deserialization, PrtgAPI relies upon reflection to iterate over each object property and bind each XML value based the value of each type/propertie's `XmlElement`, `XmlAttribute` and `XmlEnum` attributes. This allows PrtgAPI to bind raw values to `protected` members that are then parsed by the getters of their `public` coutnerparts. The PrtgAPI `XmlSerializer` also has the sense to eliminate a number of common pain points, such as converting empty strings to null.

#### Cmdlet Based Event Handlers

#### Inter-Cmdlet Progress

### PrtgAPI.Tests

#### Test Startup/Shutdown

Before and after each test begins, a number of common tasks must be performed. For example, in PowerShell we must load the PrtgAPI and PrtgAPI.Tests.* assemblies into the session. We cannot just do this once in one test file and forget about it, as tests are split across a number of files and could be run one at a time via Test Explorer.

.NET tests perform common initialization/cleanup via `AssemblyInitialize`/`AssemblyCleanup`/`TestInitialize` methods defined in common base classes of all tests.

Common startup/shutdown tasks can be defined in Pester via the `BeforeAll`/`AfterAll` functions, however PrtgAPI abstracts that a step further by completely impersonating the `Describw` function. When tests call the `Describe` function they trigger PrtgAPI's `Describe`, which in turn triggers the Pester `Describe` with our `BeforeAll`/`AfterAll` blocks pre-populated

```powershell

. $PSScriptRoot\Common.ps1

function Describe($name, $script) {
    
    Pester\Describe $name {
        BeforeAll {
            PerformStartupTasks
        }
        
        AfterAll {
            PerformShutdownTasks
        }
        
        & $script
    }
}
```

Different `Describe` overrides can be defined in different files, allowing tests to perform cleanup in different ways based on their functionality (such as `Get-` only tests not needing to perform cleanup on the integration test server). Methods such as `AssemblyInitialize` in our .NET test assembly can be triggered via our common startup functions, allowing existing testing functionality to be reused.

### Test Server State Restoration

### Mock WriteProgress

#### Logging

Integration tests can take an extremely long time to complete, can run in any order and can even cross contaminate. By intercepting key test methods and sprinkling tests with basic logging code, detailed state information can be written to a log file (%temp%\PrtgAPI.IntegrationTests.log) which can be tailed and monitored during the execution of tests

```
24/06/2017 11:46:00 AM [22952:58] C#     : Pinging ci-prtg-1
24/06/2017 11:46:00 AM [22952:58] C#     : Connecting to local server
24/06/2017 11:46:00 AM [22952:58] C#     : Retrieving service details
24/06/2017 11:46:00 AM [22952:58] C#     : Backing up PRTG Config
24/06/2017 11:46:01 AM [22952:58] C#     : Refreshing CI device
24/06/2017 11:46:01 AM [22952:58] C#     : Ready for tests
24/06/2017 11:46:01 AM [22952:58] PS     :     Running unsafe test 'Acknowledge-Sensor_IT'
24/06/2017 11:46:01 AM [22952:58] PS     :         Running test 'can acknowledge indefinitely'
24/06/2017 11:46:01 AM [22952:58] PS     :             Acknowledging sensor indefinitely
24/06/2017 11:46:01 AM [22952:58] PS     :             Refreshing object and sleeping for 30 seconds
24/06/2017 11:46:31 AM [22952:58] PS     :             Pausing object for 1 minute and sleeping 5 seconds
24/06/2017 11:46:36 AM [22952:58] PS     :             Resuming object
24/06/2017 11:46:37 AM [22952:58] PS     :             Refreshing object and sleeping for 30 seconds
24/06/2017 11:47:07 AM [22952:58] PS !!! :             Expected: {Down} But was:  {PausedUntil}
24/06/2017 11:47:07 AM [22952:58] PS     :         Running test 'can acknowledge for duration'
24/06/2017 11:47:07 AM [22952:58] PS     :             Acknowledging sensor for 1 minute
24/06/2017 11:47:07 AM [22952:58] PS     :             Sleeping for 60 seconds
24/06/2017 11:48:07 AM [22952:58] PS     :             Refreshing object and sleeping for 30 seconds
24/06/2017 11:48:37 AM [22952:58] PS     :             Test completed successfully
24/06/2017 11:48:37 AM [22952:58] PS     :         Running test 'can acknowledge until'
24/06/2017 11:48:38 AM [22952:58] PS     :             Acknowledging sensor until 24/06/2017 11:49:38 AM
24/06/2017 11:48:38 AM [22952:58] PS     :             Sleeping for 60 seconds
24/06/2017 11:49:38 AM [22952:58] PS     :             Refreshing object and sleeping for 30 seconds
24/06/2017 11:50:08 AM [22952:58] PS     :             Test completed successfully
24/06/2017 11:50:08 AM [22952:58] PS     : Performing cleanup tasks
24/06/2017 11:50:08 AM [22952:58] C#     : Cleaning up after tests
24/06/2017 11:50:08 AM [22952:58] C#     : Connecting to server
24/06/2017 11:50:08 AM [22952:58] C#     : Retrieving service details
24/06/2017 11:50:08 AM [22952:58] C#     : Stopping service
24/06/2017 11:50:21 AM [22952:58] C#     : Restoring config
24/06/2017 11:50:21 AM [22952:58] C#     : Starting service
24/06/2017 11:50:24 AM [22952:58] C#     : Finished
24/06/2017 11:50:25 AM [22952:63] PS     : PRTG service may still be starting up; pausing for 60 seconds
24/06/2017 11:51:31 AM [22952:63] PS     :     Running safe test 'Get-NotificationAction_IT'
```

DateTime, PID, TID, execution environment and exception details are all easily visible. Showing three exclamation marks against rows that contain a failure is probably the greatest feature of the entire project.