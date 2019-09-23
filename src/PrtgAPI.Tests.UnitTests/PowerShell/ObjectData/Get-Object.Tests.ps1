. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-Object" -Tag @("PowerShell", "UnitTest") {

    function GetEmpty
    {
        return @{
            Sensors = 0
            Devices = 0
            Groups = 0
            Probes = 0
            Notifications = 0
            Schedules = 0
        }
    }

    It "can deserialize" {
        $objects = Get-Object
        $objects.Count | Should Be 1
        $objects.GetType().Name | Should Be "PrtgObject"
    }

    $cases = @(
        @{ name = "sensor";       content = "Sensors";       objName = "Volume IO _Total0";       id = 4000},
        @{ name = "device";       content = "Devices";       objName = "Probe Device0";           id = 3000},
        @{ name = "group";        content = "Groups";        objName = "Windows Infrastructure0"; id = 2000},
        @{ name = "probe";        content = "Probes";        objName = "127.0.0.10";              id = 1000},
        @{ name = "notification"; content = "Notifications"; objName = "Email and push notification to admin"; id = 301},
        @{ name = "schedule";     content = "Schedules";     objName = "Weekdays [GMT+0800]";     id = 601}

    )

    It "can resolve a single <name>" -TestCases $cases {

        param($content, $objName, $id)

        $count = GetEmpty
        $count[$content] = 1

        $result = RunCustomCount $count {
            Get-Object -Id $id
        }

        $result | Should Be $objName
    }

    It "can resolve multiple objects" {

        SetMultiTypeResponse

        $obj = Get-Object -Id 1000,2000

        $obj.Count | Should Be 2
        $obj[0].Name | Should Be "127.0.0.10"
        $obj[0].GetType().Name | Should Be "PrtgObject"
        $obj[1].Name | Should Be "Windows Infrastructure0"
        $obj[1].GetType().Name | Should Be "PrtgObject"

        $resolved = Get-Object -Id 1000,2000 -Resolve

        $resolved[0].Name | Should Be "127.0.0.10"
        $resolved[0].GetType().Name | Should Be "Probe"
        $resolved[1].Name | Should Be "Windows Infrastructure0"
        $resolved[1].GetType().Name | Should Be "Group"
    }

    It "can resolve all types" {
        $objs = Get-Object -Resolve
        $objs[0].GetType().Name | Should Be "Sensor"
        $objs[1].GetType().Name | Should Be "Device"
        $objs[2].GetType().Name | Should Be "Group"
        $objs[3].GetType().Name | Should Be "Probe"
        $objs[4].GetType().Name | Should Be "Schedule"
        $objs[5].GetType().Name | Should Be "NotificationAction"
    }

    It "doesn't resolve any objects" {
        $count = GetEmpty

        $result = RunCustomCount $count {
            Get-Object -Id 1001
        }

        $result | Should Be $null
    }

    It "filters by enum types" {

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_type=Device", [Request]::DefaultObjectFlags)
        )

        $objs = Get-Object -Type Device

        $objs.Count | Should Be 1
        $objs.Type | Should Be "Device"
    }

    It "filters by string types" {
        
        SetAddressValidatorResponse @(
            [Request]::Objects("filter_type=wmilogicaldiskv2", [Request]::DefaultObjectFlags)
        )
        
        $objs = Get-Object -Type wmilogicaldiskv2
        $objs.Count | Should Be 1
        $objs.Type | Should Be "Sensor (wmilogicaldiskv2)"
        $objs.Type.StringValue | Should Be "wmilogicaldiskv2"
    }

    It "filters by enum types and string types at once" {

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_type=wmilogicaldiskv2&filter_type=device", [Request]::DefaultObjectFlags)
        )
        
        $objs = Get-Object -Type wmilogicaldiskv2,device
        $objs.Count | Should Be 2
        $objs[0].Type | Should Be "Device"
        $objs[1].Type | Should Be "Sensor (wmilogicaldiskv2)"
        $objs[1].Type.StringValue | Should Be "wmilogicaldiskv2"
    }

    It "doesn't filter types when specifying sensors server side" {

        SetAddressValidatorResponse @(
            [Request]::Objects("count=0", $null)
            [Request]::Objects("count=500", [UrlFlag]::Columns)
        )

        $objs = Get-Object -Type Sensor,Device

        $objs.Count | Should Be 2
        $objs[0].Type | Should Be "Device"
        $objs[1].Type | Should Be "Sensor (wmilogicaldiskv2)"
        $objs[1].Type.StringValue | Should Be "wmilogicaldiskv2"
    }

    It "pipes from another object" {

        SetAddressValidatorResponse @(
            [Request]::Objects("filter_objid=1000", [Request]::DefaultObjectFlags)
            [Request]::Objects("filter_parentid=1000", [Request]::DefaultObjectFlags)
        )

        Get-Object -Id 1000 | Get-Object
    }
}