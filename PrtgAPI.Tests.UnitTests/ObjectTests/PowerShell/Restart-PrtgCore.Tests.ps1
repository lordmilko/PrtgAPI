. $PSScriptRoot\Support\Standalone.ps1

Describe "Restart-PrtgCore" -Tag @("PowerShell", "UnitTest") {
    It "restarts the core server" {

        $startDate = (Get-Date).ToString("yyyy-MM-dd-HH-mm-ss")

        SetAddressValidatorResponse @(
            "api/restartserver.htm?"
            "api/table.xml?content=messages&" +
                "columns=datetime,parent,status,sensor,device,group,probe,priority,message,type,tags,active,objid,name&" +
                "count=500&filter_status=1&filter_dstart=$startDate&"
        )

        Restart-PrtgCore -Force -Wait -Timeout 3600
    }

    It "completes all waiting stages" {
        SetResponseAndClient "RestartPrtgCoreResponse"

        Restart-PrtgCore -Force
    }
}