. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Add-Group" -Tag @("PowerShell", "UnitTest") {
    It "adds a group using default parameters" {
        SetAddressValidatorResponse "addgroup2.htm?name_=Servers&id=1&"

        $params = New-GroupParameters Servers

        $probe = Run Probe { Get-Probe }

        $probe | Add-Group $params -Resolve:$false
    }

    It "adds a group with the basic parameter set" {
        SetAddressValidatorResponse "addgroup2.htm?name_=New+Group&id=2211&"

        $group = Run Group { Get-Group }

        $group | Add-Group "New Group" -Resolve:$false
    }

    It "resolves a created sensor" {
        SetResponseAndClientWithArguments "DiffBasedResolveResponse" $false

        $params = New-GroupParameters Servers

        $probe = Run Probe { Get-Probe }

        $group = $probe | Add-Group $params -Resolve

        $group.Id | Should Be 1002
    }
}