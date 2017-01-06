using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrtgAPI
{
    /*   
    Todo
    ----------------

    Project
    -remove ps1 and readme.md powershell 5 requirement

    -Maybe remove ObjectId properties for all objects but prtgtablecmdlet. and maybe channel. you can do anything by ID by getting a root level type (e.g. sensor) and piping it
        -maybe not, its good to be able to specify an id. maybe we should add MORE places you can specify ids!

    -search for any _raw values and fix them

    -test prtgapi.cmd on a computer with a default execution policy

    Async
    -can we run a task for x seconds before we demand it switches the context back to us so we can do something then resume it. we could use this to get the sensor totals
        and if its taking too long THEN display a progress bar
    -is there some way we can specify a cancellation token or something for a web request download?

    SearchFilter
    -my documentation in my readme.md file says equals is case sensitive, but it actually doesnt appear to be. whats up with that?

    NotificationTriggers
    -test we can successfully create triggers of all types with all parameters specified

    GetSensorHistory
    -should return a list of channelhistory instead?
    -rename sensorhistorydata to sensorhistory?
    -how to make it return more than 500 results
    -the sensorhistory class needs to be made public again

    NotificationAction
    -add additional columns

    GetSensor
    -tags always need to filter using "contains. we're fine for the powershell version, but i think we need some sort of filteroperator override for the c# api. perhaps
        a filteroperatoroverrideattribute. we can override it during the prtgurl construction, but then we need to filter the results once the request has completed
    -if you specify a status filter of "acknowledged" does it autocomplete to downacknowledged?
    -should the LastValue property be a number? if so, when sensors are paused they are "-" so clearly it should be a nullable double or something?
    -there is a "listend" property when you make a request. 1 means you've gotten all the sensors now. i dont think this will help considering previously we asked for the remaining ones and we got more than we expected
    -when getsensors fails it throws an aggregateexception. i think we should unwrap the inner exception, or count how many there were and iterate through them?

    SensorSettings
    -not complete
    -internal for now
    -the schedule class is also internal

    ScanningInterval
    -this type has been made internal but if we're not going to even have individual getobjectproperty/setobjectproperty methods we can delete it

    Get[Sensor|Device|Group|Probe]
    -how do we do wildcard matching in the middle of a name? (e.g. "a*m"). potential solution: do two filter_xyz's and then do a final powershell wildcardmatch class on the result
    -any phrase for a property that can be specified needs to be able to use wildcards

    GetChannel
    -test on drods pc. see if the output is all messed up and middle aligned. is it a windows 10 thing? we can also test on my win 10 vm
    -the downtime channel never displays a value. it should be 0% when up, 100% when down

    GetStatus
    -internal for now
    -need to rename fields to be nice names

    Clone
    -Parse the response and get the new object id
    -whats the response when theres an error?

    SensorTotals
    -Remove _raw values

    ServerStatus
    -Remove _raw values
    */
}
