# PrtgAPI
PrtgAPI is a C#/PowerShell library that abstracts away the complexity of interfacing with the [PRTG HTTP API](https://prtg.paessler.com/api.htm?tabid=2).

PrtgAPI implements a collection of methods and enumerations that help create and execute the varying HTTP GET requests required to interface with PRTG. Upon executing a request, PrtgAPI will deserialize the result into an object (Sensor, Device, Probe, etc) that the programmer can further interface with.

# Usage (C#)
All actions in PrtgAPI revolve around a core class: `PrtgClient`

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "password");
```

When a `PrtgClient` is created, it will immediately attempt to retrieve your user's passhash (an alternative to using a password) from your PRTG Server. For added security, your PassHash is then used for all future PRTG Requests made during the life of your program.

For further security, you are able to use your passhash to `PrtgClient` instead of using your password. Simply create a breakpoint after the `PrtgClient` constructor call has executed, copy your passhash out of your `request` object's private `passhash` property then tell the constructor to use the passhash instead.

```c#
var client = new PrtgClient("prtg.mycoolsite.com", "username", "1234567890", AuthMode.PassHash);
```

## Lists

PrtgAPI provides a series of method overloads for retrieving all sorts of data in a variety of different ways.

To retrieve a list of all sensors, call the `GetSensors()` method.

```c#
var sensors = client.GetSensors();
```

For groups, call `GetGroups()`; Devices, call `GetDevices()`, etc.

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

## Object Settings
Values of object settings can be enumerated and manipulated via two groups of overloaded methods: `GetObjectProperty` and `SetObjectProperty`

```c#
//Retrieve the name of object with ID 2001
var name = client.GetObjectProperty(2001, BasicObjectSetting.Name);
```
```
//Update the name of object with ID 2001
var name = client.SetObjectProperty(2001, BasicObjectSetting.Name, "a brand new name!");
```
By default, `GetObjectProperty` will return a `string` containing the value you requested. If you know for a fact the property is of another type (an enum defined by PrtgAPI, or an integer) you can request GetObjectProperty cast its return value to its "true" data type.

```c#
var priorityNum = client.GetObjectProperty<int>(2001, BasicObjectSetting.Priority);
var priorityEnum = client.GetObjectProperty<Priority>(2001, BasicObjectSetting.Priority);
```

## Pausing / Resuming

Objects can be paused and resumed using the `Pause` and `Resume` methods respectively. A message and a pause duration can be optionally specified. If no time is given, the object is paused indefinitely.

```c#
//Pause object with ID 2001 indefinitely
client.Pause(2001);
```
```c#
//Pause object with ID 2002 for 60 minutes
client.Pause(2002, "Paused for the next 60 minutes!", 60);
```
```c#
//Resume object with ID 2001
client.Resume(2001);
```

# Custom Requests
For those that which to execute custom requests (i.e. those not yet supported by PrtgAPI, or those not known to be supported by PRTG) it is possible to craft custom requests that do whatever you like.

```c#
//Return only the "name" and "object ID" properties, limiting the number of results returned to 100.
var parameters = new SensorParameters()
{
    Columns = new[] { Property.Name, Property.ObjId }
    Count = 100
};

var sensors = client.GetSensors(parameters);
```
PrtgAPI implements a number of built-in parameter types that automatically specify the type of content their requests will retrieve. If you wish to implement your own custom parameters, you can do so by manipulating the base `Parameters` class.

# PowerShell

PrtgAPI features a number of PowerShell cmdlets that encapsulate the core functionality of the C# interface. To compile for PowerShell, ensure you have PowerShell 5 installed (Windows Management Framework 5.0), select the _PowerShell (Release)_ configuration in Visual Studio. This will create a _PrtgAPI_ folder under _bin\PowerShell (Release)_ you can then copy wherever you like and import into PowerShell, as follows:
```powershell
Import-Module "C:\path\to\PrtgAPI" -DisableNameChecking
```

Once loaded, you can connect to your PRTG Server

```powershell
Connect-PrtgServer prtg.mycoolsite.com (Get-Credential)
```

To use your PassHash instead of your password, specify the `-PassHash` switch. If you do not know your PassHash, you can retrieve it once authenticated via `Get-PrtgServer`

To authenticate using your PassHash:

```powershell
Connect-PrtgServer prtg.mycoolsite.com (Get-Credential) -PassHash
```

The following cmdlets are currently supported

```powershell
Acknowledge-Sensor
Connect-PrtgServer
Disconnect-PrtgServer
Get-Channel
Get-Device
Get-Group
Get-Probe
Get-PrtgServer
Get-Sensor
Get-SensorTotals
New-Credential # Allows creating PSCredentials programmatically
New-SearchFilter
Pause-Object
Refresh-Object
Remove-Object
Rename-Object
Set-ChannelProperty # Currently supports limit related properties
```

For details on supported parameters run `Get-Help <cmdlet>` within PowerShell

## Examples

Get all ping sensors

```powershell
Get-Sensor ping
```
Get all devices whose names contain dc

```powershell
Get-Device *dc*
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
Get-Sensor | Select -First 1 | Get-Channel
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
