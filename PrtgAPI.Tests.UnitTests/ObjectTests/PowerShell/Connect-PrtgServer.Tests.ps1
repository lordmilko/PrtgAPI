. $PSScriptRoot\Support\Standalone.ps1

Describe "Connect-PrtgServer" {

    InitializeUnitTestModules

    It "requires a connection" {
        Disconnect-PrtgServer

        { Get-Sensor } | Should Throw "You are not connected to a PRTG Server"
    }
}