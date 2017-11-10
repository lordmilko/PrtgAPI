. $PSScriptRoot\Support\Standalone.ps1

Describe "Move-Object" -Tag @("PowerShell", "UnitTest") {

    SetActionResponse

    It "can execute" {
        $device = Run Device { Get-Device }

        $device | Move-Object 1234
    }
}