. $PSScriptRoot\Support\UnitTest.ps1

Describe "Get-Sensor" -Tag @("PowerShell", "UnitTest") {
    
    It "can deserialize" {
        $sensors = Get-Sensor
        $sensors.Count | Should Be 1
    }

    It "can filter valid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $sensors = Get-Sensor -Tags *banana*
            $sensors.Count | Should Be 1
        }
    }

    It "can ignore invalid wildcards" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $sensors = Get-Sensor -Tags *apple*
            $sensors.Count | Should Be 0
        }
    }

    It "throws when -Id is $null" {
        { Get-Sensor -Id $null } | Should Throw "The -Id parameter was specified however the parameter value was null"
    }

    <#It "can pipe from devices" {

        $deviceId = 2001

        $devices = Run Device {
            $obj = GetItem

            $obj.ObjId = $deviceId

            WithItems ($obj) {
                Get-Device
            }
        }

        $obj1 = GetItem
        $obj2 = GetItem

        $obj1.ParentId = $deviceId
        $obj2.ParentId = $deviceId + 1
        #isnt this incorrect. we want two sensors, and one device

        WithItems ($obj1, $obj2) {
            $sensors = $devices | Get-Sensor

            #the check we need to do needs to be in the mock response - it needs to do some validation for us on the contents of the request

            $sensors.Count | Should Be 1

            $sensors.ParentId | Should Be $deviceId
        }
    }#>
}

