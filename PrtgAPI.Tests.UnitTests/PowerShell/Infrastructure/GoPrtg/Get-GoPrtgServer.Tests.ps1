. $PSScriptRoot\..\..\..\Support\PowerShell\GoPrtg.ps1

Describe "Get-GoPrtgServer" -Tag @("PowerShell", "UnitTest") {

    BeforeAll { GoPrtgBeforeAll    }

    BeforeEach { GoPrtgBeforeEach }
    AfterEach { GoPrtgAfterEach }

    AfterAll { GoPrtgAfterAll }

    It "can retrieve servers" {
        #todo: allow specifying the servers you want to get-goprtgserver, alias or server wildcard

        Install-GoPrtgServer dev

        $servers = Get-GoPrtgServer

        $servers | Should Not BeNullOrEmpty

        $servers.Server | Should Be prtg.example.com
        $servers.Alias | Should Be dev
        $servers.UserName | Should Be username
    }

    It "sets empty aliases to null" {
        Install-GoPrtgServer

        $server = Get-GoPrtgServer

        $server.Alias | Should Be $null
    }

    It "can retrieve servers by server name" {
        InstallMultipleInProfile

        $server = Get-GoPrtgServer prtg.example2.com

        $server.Server | Should Be prtg.example2.com
    }

    It "can retrieve servers by server wildcard" {
        InstallMultipleInProfile

        try
        {
            Connect-PrtgServer prtg2.example.com (New-Credential username3 12345678) -PassHash -Force

            Install-GoPrtgServer
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        $servers = Get-GoPrtgServer prtg.*

        $servers.Count | Should Be 2
        $servers[0].Server | Should Be prtg.example.com
        $servers[1].Server | Should Be prtg.example2.com
    }

    It "can retrieve servers by alias" {
        InstallMultipleWithAlias

        $server = Get-GoPrtgServer dev

        $server.Server | Should Be prtg.example2.com
        $server.Alias | Should Be dev
    }

    It "warns when GoPrtg isn't installed" {
        Get-GoPrtgServer | Should Be "`nGoPrtg is not installed. Run Install-GoPrtgServer first to install a GoPrtg server.`n"
    }

    It "warns when profile doesn't exist" {
        Get-GoPrtgServer | Should Be "`nGoPrtg is not installed. Run Install-GoPrtgServer first to install a GoPrtg server.`n"
    }
}
