. $PSScriptRoot\Support\Standalone.ps1

Describe "Get-PrtgStatus" {

    $item = (New-Object PrtgAPI.Tests.UnitTests.ObjectTests.ServerStatusTests).GetItem()

    SetResponseAndClientWithArguments "ServerStatusResponse" $item

    It "can execute" {
        $status = Get-PrtgStatus

        $status.GetType().Name | Should Be ServerStatus
    }
}