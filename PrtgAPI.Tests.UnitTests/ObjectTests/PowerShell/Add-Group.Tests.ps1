. $PSScriptRoot\Support\Standalone.ps1

Describe "Add-Group" -Tag @("PowerShell", "UnitTest") {
    It "adds a group using default parameters" {
        SetAddressValidatorResponse "addgroup2.htm?name_=Servers&id=1&"

        $params = New-GroupParameters Servers

        $probe = Run Probe { Get-Probe }

        $probe | Add-Group $params
    }

    It "adds a group with the basic parameter set" {
        SetAddressValidatorResponse "addgroup2.htm?name_=New+Group&id=2211&"

        $group = Run Group { Get-Group }

        $group | Add-Group "New Group"
    }
}