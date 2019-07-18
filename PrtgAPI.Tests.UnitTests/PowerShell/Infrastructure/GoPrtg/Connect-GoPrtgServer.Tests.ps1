. $PSScriptRoot\..\..\..\Support\PowerShell\GoPrtg.ps1

Describe "Connect-GoPrtgServer" -Tag @("PowerShell", "UnitTest") {

    BeforeAll { GoPrtgBeforeAll    }

    BeforeEach { GoPrtgBeforeEach }
    AfterEach { GoPrtgAfterEach }

    AfterAll { GoPrtgAfterAll }

    It "can connect to default server" {
        Install-GoPrtgServer

        Disconnect-PrtgServer

        Connect-GoPrtgServer | Should Be "`nConnected to prtg.example.com as username`n"
    }

    It "can connect to alias when single server exists" {
        Install-GoPrtgServer test

        Disconnect-PrtgServer

        Connect-GoPrtgServer test | Should Be "`nConnected to prtg.example.com as username`n"
    }

    It "can connect to specified server when single server exists" {
        Install-GoPrtgServer

        Disconnect-PrtgServer

        Connect-GoPrtgServer prtg.example.com | Should Be "`nConnected to prtg.example.com as username`n"
    }

    It "can connected to specified server when multiple servers exist" {
        Install-GoPrtgServer

        Disconnect-PrtgServer

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        Connect-GoPrtgServer prtg.example2.com | Should Be "`nConnected to prtg.example2.com as username2`n"
    }

    It "can connect to specified alias when multiple servers exist" {
        Install-GoPrtgServer test1

        Disconnect-PrtgServer

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer test2
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        Disconnect-PrtgServer

        Connect-GoPrtgServer test1 | Should Be "`nConnected to prtg.example.com as username`n"

        Disconnect-PrtgServer

        Connect-GoPrtgServer test2 | Should Be "`nConnected to prtg.example2.com as username2`n"
    }

    It "authenticates with the original server details" {
        Install-GoPrtgServer

        Disconnect-PrtgServer

        Connect-GoPrtgServer

        $client = Get-PrtgClient

        $client.Server | Should Be "prtg.example.com"
        $client.UserName | Should Be "username"
        $client.PassHash | Should Be "passhash"
    }

    It "warns when server is already connected" {
        Install-GoPrtgServer

        Connect-GoPrtgServer | Should Be "`nAlready connected to prtg.example.com as username`n"
    }

    It "warns when GoPrtg is not installed" {
        New-Item $Profile -Type File -Force

        Connect-GoPrtgServer | Should Be "`nNo GoPrtg servers are installed. Please install a server first using Install-GoPrtgServer`n"
    }

    It "warns when profile doesn't exist" {
        Connect-GoPrtgServer | Should Be "`nNo GoPrtg servers are installed. Please install a server first using Install-GoPrtgServer`n"
    }

    It "throws when multiple servers match the server expression" {
        Install-GoPrtgServer

        Disconnect-PrtgServer

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer

            Connect-PrtgServer test.example3.com (New-Credential username3 12345678) -PassHash -Force

            Install-GoPrtgServer
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        $response = Connect-GoPrtgServer p*

        $response | Should Be @(
            "`nAmbiguous server specified. The following servers matched the specified server name or alias",
            "@{[!]=[*]; Server=prtg.example.com; Alias=; UserName=username}",
            "@{[!]=[ ]; Server=prtg.example2.com; Alias=; UserName=username2}"
        )
    }
}
