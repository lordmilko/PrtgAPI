. $PSScriptRoot\Support\Standalone.ps1

function SetCloneResponse
{
    $client = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses.CloneResponse))

    SetPrtgClient $client
}

Describe "Clone-Sensor" -Tag @("PowerShell", "UnitTest") {

    SetCloneResponse

    It "Retries resolving an object" {
        $sensor = Run Sensor { Get-Sensor }

        $sensor.Count | Should Be 1

        $output = [string]::Join("`n",(&{try { $sensor | Clone-Sensor 1234 3>&1 | %{$_.Message} } catch [exception] { }}))

        $expected = "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 4`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 3`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 2`n" +
                    "'Copy-Sensor' failed to resolve sensor: object is still being created. Retries remaining: 1"

        $output | Should Be $expected
    }

    It "doesn't resolve a sensor/group" {
        $sensor = Run Sensor { Get-Sensor }

        $result = $sensor | Clone-Sensor 1234 "new sensor" -Resolve:$false

        $result.GetType().Name | Should Be "PSCustomObject"
    }

    It "doesn't resolve a device" {
        $device = Run Device { Get-Device }

        $result = $device | Clone-Device 1234 "new device" -Resolve:$false

        $result.GetType().Name | Should Be "PSCustomObject"
    }

    It "executes with -WhatIf" {
        $sensor = Run Sensor { Get-Sensor }
        $device = Run Device { Get-Device }
        $group = Run Group { Get-Group }

        $sensor | Clone-Sensor 1234 -WhatIf
        $device | Clone-Device 1234 -WhatIf
        $group | Clone-Group 1234 -WhatIf
    }
}