. $PSScriptRoot\..\..\Support\PowerShell\UnitTest.ps1

Describe "Get-NotificationAction" -Tag @("PowerShell", "UnitTest") {

    It "can deserialize" {
        $actions = Get-NotificationAction
        $actions.Count | Should Be 1
    }

    It "can filter by Id" {

        $expected = @(
            "api/table.xml?content=notifications&columns=baselink,type,tags,active,objid,name&count=*&filter_objid=301&",
            "controls/editnotification.htm?id=300&"
        )

        SetAddressValidatorResponse $expected

        Get-NotificationAction -Id 301
    }

    It "can filter by name" {
        $expected = @(
            "api/table.xml?content=notifications&columns=baselink,type,tags,active,objid,name&count=*&filter_name=@sub(admin)&",
            "controls/editnotification.htm?id=300&"
        )

        SetAddressValidatorResponse $expected

        Get-NotificationAction *admin*
    }

    It "can filter by tags" {
        $obj1 = GetItem
        $obj2 = GetItem

        $obj1.ObjId = 300
        $obj1.Tags = "testappletest"
        $obj2.ObjId = 301
        $obj2.Tags = "testbananatest"

        WithItems ($obj1, $obj2) {
            $actions = Get-NotificationAction -Tags *apple*
            $actions.Count | Should Be 1
        }
    }
}