. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Get-PrtgStatus" -Tag @("PowerShell", "UnitTest") {

    $item = (New-Object PrtgAPI.Tests.UnitTests.ObjectData.ServerStatusTests).GetItem()

    SetResponseAndClientWithArguments "ServerStatusResponse" $item

    It "can execute" {
        $status = Get-PrtgStatus

        $status.GetType().Name | Should Be ServerStatus
    }
}