namespace PrtgAPI
{
    /*   
    Todo
    ----------------

    how do we get channels that are configured to hide from tables to show in get-channel

    -add support for all commented out object properties in all the various object property tests
    -add support for properties we havent written up yet across sensors, devices, groups and probes

    -make rename-object prompt whether you want to do it, and add a force parameter
    -when we're asked whether we want to do things, the default option should be no. fix all modification cmdlets that have shouldprocess
     maybe we should make priority a fancy class? then we can say tostring is numeric <- dont know what that means, but if we're a class we can set priority using numbers instead of words
     todo: pretty much ALL PrtgClient methods need to validate their inputs arent null or empty
    -make schedule an enum class that lists common schedules and lets you specify custom ones

    Project
    -Maybe remove ObjectId properties for all objects but prtgtablecmdlet. and maybe channel. you can do anything by ID by getting a root level type (e.g. sensor) and piping it
        -maybe not, its good to be able to specify an id. maybe we should add MORE places you can specify ids!

    -test httpclient failure
     we can use remove-object on the root prtg object to test this

    -look into implementing this: https://msdn.microsoft.com/en-us/library/bb204629(v=vs.85).aspx - confirmimpact, etc

    -upload our empty settings file then say dont track it
     git update-index --assume-unchanged <file> and --no-assume-unchanged

    README
    -add powershell example for getting deprecated sensors

    Async
    -can we run a task for x seconds before we demand it switches the context back to us so we can do something then resume it. we could use this to get the sensor totals
        and if its taking too long THEN display a progress bar
    -for getsensorsasync, add support for getrawobject totals taking a params filter so we can get the totals when we have a filter. update prtgtablecmdlet accordingly
    -have our new streaming methods specify a Count where we want to, and then make streamobjects check if count is null and if so set it to 500

    Clone
    -whats the response when theres an error?

    GetSensor
    -tags always need to filter using "contains. we're fine for the powershell version, but i think we need some sort of filteroperator override for the c# api. perhaps
        a filteroperatoroverrideattribute. we can override it during the PrtgRequestMessage construction, but then we need to filter the results once the request has completed
    -should the LastValue property be a number? if so, when sensors are paused they are "-" so clearly it should be a nullable double or something?
    -there is a "listend" property when you make a request. 1 means you've gotten all the sensors now. i dont think this will help considering previously we asked for the remaining ones and we got more than we expected
    -when getsensors fails it throws an aggregateexception. i think we should unwrap the inner exception, or count how many there were and iterate through them?

    GetSensorHistory
    -should return a list of channelhistory instead?
    -rename sensorhistorydata to sensorhistory?
    -how to make it return more than 500 results
    -the sensorhistory class needs to be made public again

    NewNotificationTriggerParameter
    -maybe change objectid and source ordering? we need to be able to pipe objectid's in. is this currently possible? maybe just add valuefrompipeline/add by property name for objectid?
     may need to rename id to objectid for that one
    -TODO - we need to do a TONNE more testing setting fields to null and checking they react the way theyre meant to for edit and add mode
     need to write unit tests to automate this testing

    NotificationTriggers
    -test we can successfully create triggers of all types with all parameters specified

    PrtgClient
    -check the xml returned in the executerequest xml method doesnt say there was an error
     it looks like we already do this when its an xml request, however theres also a bug here in that we automatically try to deserialize
     some xml without checking whether its xml or not; this could result in an exception in the exception handler!
    -we need to be able to handle errors on json requests or otherwise

    SearchFilter
    -my documentation in my readme.md file says equals is case sensitive, but it actually doesnt appear to be. whats up with that?

    */
}
