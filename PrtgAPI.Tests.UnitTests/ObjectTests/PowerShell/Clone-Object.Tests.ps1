. $PSScriptRoot\Support\Standalone.ps1

function SetCloneResponse
{
    $client = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses.CloneResponse))

    SetPrtgClient $client
}

Describe "Clone-Object" -Tag @("PowerShell", "UnitTest") {

    SetCloneResponse

    It "Retries resolving an object" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor.Count | Should Be 1

        $output = [string]::Join("`n",(&{try { $sensor | Clone-Object 1234 3>&1 | %{$_.Message} } catch [exception] { }}))

        $expected = "'Copy-Object' failed to resolve sensor: object is still being created. Retries remaining: 4`n" +
                    "'Copy-Object' failed to resolve sensor: object is still being created. Retries remaining: 3`n" +
                    "'Copy-Object' failed to resolve sensor: object is still being created. Retries remaining: 2`n" +
                    "'Copy-Object' failed to resolve sensor: object is still being created. Retries remaining: 1"

        $output | Should Be $expected
    }

    It "Clones a trigger" {
        $group = Run Group { Get-Group }

        $triggers = Run NotificationTrigger { $group | Get-Trigger }

        $triggers | Clone-Object 5678 -Resolve:$false
    }

    It "Clones a source ID" {

        $devices = WithResponse "MultiTypeResponse" {
            Get-Device -Count 1
        }

        try
        {
            # MultiTypeResponse will respond with an object to the request for sensors, despite the fact this is a device
            SetAddressValidatorResponse @(
                "api/table.xml?content=sensors&columns=probe,group,favorite,lastvalue,device,downtime,downtimetime,downtimesince,uptime,uptimetime,uptimesince,knowntime,cumsince,lastcheck,lastup,lastdown,minigraph,schedule,basetype,baselink,parentid,notifiesx,interval,intervalx,access,dependency,position,status,comments,priority,message,type,tags,active,objid,name&count=*&filter_objid=1234&"
                "api/duplicateobject.htm?id=4000&name=Volume+IO+_Total0&targetid=3000&"
                "api/table.xml?content=sensors&columns=probe,group,favorite,lastvalue,device,downtime,downtimetime,downtimesince,uptime,uptimetime,uptimesince,knowntime,cumsince,lastcheck,lastup,lastdown,minigraph,schedule,basetype,baselink,parentid,notifiesx,interval,intervalx,access,dependency,position,status,comments,priority,message,type,tags,active,objid,name&count=*&filter_objid=9999&"
            )

            $devices | Clone-Object -SourceId 1234
        }
        finally
        {
            SetCloneResponse
        }
    }

    It "throws cloning an unknown source ID" {
        $devices = Run Device { Get-Device }

        WithResponseArgs "AddressValidatorResponse" "a" {
            { $devices | Clone-Object -SourceId -1 } | Should Throw "Cannot clone object with ID '-1' as it is not a sensor, device or group"
        }
    }

    It "doesn't resolve a sensor/group" {
        $sensor = Run Sensor { Get-Sensor }

        $result = $sensor | Clone-Object 1234 "new sensor" -Resolve:$false

        $result.GetType().Name | Should Be "PSCustomObject"
    }

    It "doesn't resolve a device" {
        $device = Run Device { Get-Device }

        $result = $device | Clone-Object 1234 "new device" -Resolve:$false

        $result.GetType().Name | Should Be "PSCustomObject"
    }

    It "executes with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }
        $device = Run Device { Get-Device }
        $group = Run Group { Get-Group }
        $trigger = Run NotificationTrigger { $group | Get-Trigger | Select -First 1 }

        $sensor | Clone-Object 1234 -WhatIf
        $device | Clone-Object 1234 -WhatIf
        $group | Clone-Object 1234 -WhatIf
        $trigger | Clone-Object 1234 -WhatIf
    }
}