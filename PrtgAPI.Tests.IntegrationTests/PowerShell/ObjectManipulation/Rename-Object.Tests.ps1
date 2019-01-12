. $PSScriptRoot\..\..\Support\PowerShell\IntegrationTest.ps1

Describe "Rename-Object_IT" -Tag @("PowerShell", "IntegrationTest") {

    $testCases = @(
        @{name = "Sensor"; query = { Get-Sensor -Id (Settings UpSensor) }}
        @{name = "Device"; query = { Get-Device -Id (Settings Device) }}
        @{name = "Group";  query = { Get-Group  -Id (Settings Group) }}
        @{name = "Probe";  query = { Get-Probe  -Id (Settings Probe) }}
    )
    
    It "renames an object" -TestCases $testCases {
        param($query)

        $newName = "newName"

        $obj = & $query

        $obj.Name | Should Not Be $newName

        $obj | Rename-Object $newName

        $newObj = & $query

        $newObj.Name | Should Be $newName

        $newObj | Rename-Object $obj.Name

        $finalObj = & $query

        $finalObj.Name | Should Be $obj.Name
    }

    It "can rename multiple in a single request" {
        $sensor = Get-Sensor -Id (Settings UpSensor)
        $device = Get-Device -Id (Settings Device)

        $newName = "multiName"

        $sensor.Name | Should Not Be $newName
        $device.Name | Should Not Be $newName

        $list = $sensor,$device

        $list.Count | Should Be 2

        $list | Rename-Object $newName

        $newSensor = Get-Sensor -Id (Settings UpSensor)
        $newDevice = Get-Device -Id (Settings Device)

        $newSensor.Name | Should Be $newName
        $newDevice.Name | Should Be $newName
    }
}