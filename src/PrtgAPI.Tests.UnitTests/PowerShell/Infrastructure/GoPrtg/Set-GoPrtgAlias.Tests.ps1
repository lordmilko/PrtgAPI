. $PSScriptRoot\..\..\..\Support\PowerShell\GoPrtg.ps1

Describe "Set-GoPrtgAlias" -Tag @("PowerShell", "UnitTest") {
    BeforeAll { GoPrtgBeforeAll    }

    BeforeEach { GoPrtgBeforeEach }
    AfterEach { GoPrtgAfterEach }

    AfterAll { GoPrtgAfterAll }

    $nl = [Environment]::NewLine

    It "sets an alias on a record that doesn't have one" {
        Install-GoPrtgServer

        Set-GoPrtgAlias test

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "sets an alias on a record that does have one" {
        Install-GoPrtgServer prod

        Set-GoPrtgAlias dev

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"dev```",```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "sets an alias on the first server when multiple servers are installed" {
        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer dev
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        Disconnect-PrtgServer

        Connect-GoPrtgServer prod

        Set-GoPrtgAlias test

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "sets an alias on the last server when multiple servers are installed" {
        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer dev
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        Disconnect-PrtgServer

        Connect-GoPrtgServer dev

        Set-GoPrtgAlias test

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example2.com```",```"test```",```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "sets an alias when profile has content before function" {
        Add-Content $Profile "Write-Host `"hello`""

        Install-GoPrtgServer prod

        Set-GoPrtgAlias test

        $content = gc $Profile -Raw

        $expected = "Write-Host `"hello`"$nl"
        $expected += "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "sets an alias when profile has content before and after function" {
        Add-Content $Profile "Write-Host `"hello`""

        Install-GoPrtgServer prod

        Add-Content $Profile "Write-Host `"goodbye`""

        Set-GoPrtgAlias test

        $content = gc $Profile -Raw

        $expected = "Write-Host `"hello`"$nl"
        $expected += "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"
        $expected += "Write-Host `"goodbye`"$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "removes an alias on a record that has one" {
        Install-GoPrtgServer prod

        Set-GoPrtgAlias $null

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "removes an alias on a record that doesn't have one" {
        Install-GoPrtgServer

        Set-GoPrtgAlias $null

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "updates the global function" {
        Install-GoPrtgServer prod
        Set-GoPrtgAlias dev

        Get-GoPrtgServer dev | Should Be "@{[!]=[*]; Server=prtg.example.com; Alias=dev; UserName=username}"
    }

    It "throws setting a duplicate alias" {
        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer dev
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        Connect-GoPrtgServer dev

        { Set-GoPrtgAlias prod } | Should Throw "Cannot set alias for server 'prtg.example2.com': a record with alias 'prod' already exists. For more information see Get-GoPrtgServer."
    }

    It "throws removing an alias on a duplicate server" {
        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer dev
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        Connect-GoPrtgServer dev

        { Set-GoPrtgAlias } | Should Throw "Cannot remove alias of server: multiple entries for server 'prtg.example.com' are stored within GoPrtg. To remove this alias uninstall all other entries for this server. For more information see Get-GoPrtgServer."
    }

    It "throws when not connected to a PRTG Server" {
        Install-GoPrtgServer

        Disconnect-PrtgServer

        { Set-GoPrtgAlias } | Should Throw "You are not connected to a PRTG Server. Please connect first using GoPrtg [<server>]."
    }

    It "throws when GoPrtg is not installed" {
        New-Item $Profile -Type File -Force

        { Set-GoPrtgAlias } | Should Throw "GoPrtg is not installed. Run Install-GoPrtgServer <alias> to install a server with the specified alias."
    }

    It "throws when profile doesn't exist" {
        { Set-GoPrtgAlias } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
    }

    It "throws when the connected PRTG Server is not a GoPrtg server" {
        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            { Set-GoPrtgAlias } | Should Throw "Server 'prtg.example2.com' is not a valid GoPrtg server. To install this server, run Install-GoPrtgServer [<alias>]"
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "throws trying to update the alias of an uninstalled server when another server exists under different credentials" {
        Install-GoPrtgServer first

        try
        {
            Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

            { Set-GoPrtgAlias second } | Should Throw "is a valid GoPrtg server, however you are not authenticated as a valid user"
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "throws when both the header and footer have been removed" {
        InstallInProfileFunctionWithoutHeaderFooter

        { Set-GoPrtgAlias dev } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' and end line"
    }

    <#
    it sets an alias on a record that doesnt have one
on one that does have one
sets an alias to nothing
prevents setting an alias to nothing in scenarios that would result in valid situations we normally
dont allow when doing install-goprtgserver <look those up>
throws when not connected to a prtg server
throws when the goprtgserver is not installed
throws when its been installed twice with the same name using different aliases
-should we maybe prevent that with install-goprtgserver even if they are using different aliases?
    #>
}
