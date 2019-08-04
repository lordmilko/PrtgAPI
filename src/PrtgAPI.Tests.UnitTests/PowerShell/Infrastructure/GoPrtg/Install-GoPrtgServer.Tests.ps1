. $PSScriptRoot\..\..\..\Support\PowerShell\GoPrtg.ps1

Describe "Install-GoPrtgServer" -Tag @("PowerShell", "UnitTest") {

    $nl = [Environment]::NewLine

    $baseExpected = ("########################### Start GoPrtg Servers ###########################$nl$nl" +
                    "function __goPrtgGetServers {@($nl    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl)}$nl$nl" +
                    "############################ End GoPrtg Servers ############################$nl").Replace("``","````")

    BeforeAll { GoPrtgBeforeAll    }

    BeforeEach { GoPrtgBeforeEach }
    AfterEach { GoPrtgAfterEach }

    AfterAll { GoPrtgAfterAll }

    It "creates a new profile and profile folder if one doesn't exist" {

        $needsRestore = $false

        $folder = $null

        $folder = Split-Path $Profile -Parent

        if(Test-Path $folder)
        {
            $needsRestore = $true

            Rename-Item $folder "$folder.tmp"
        }

        try
        {
            Install-GoPrtgServer

            $content = gc $Profile -Raw

            $content | Should BeLike $baseExpected
        }
        finally
        {
            if($needsRestore)
            {
                Remove-Item $folder -Force -Recurse

                Rename-Item "$folder.tmp" $folder
            }
        }
    }

    It "installs correctly in new profile" {
        Install-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should BeLike $baseExpected
    }

    It "installs correctly in existing empty profile" {
        InstallInEmptyProfile $baseExpected
    }

    It "installs correctly in profile with content in it" {
        InstallInProfileWithContent $baseExpected
    }

    It "installs correctly in profile with content in it without trailing newline" {
        Add-Content $Profile "Write-Host `"hello`"" -NoNewline

        Install-GoPrtgServer

        $expected = "Write-Host `"hello`"$nl$baseExpected"

        $content = gc $Profile -Raw

        $content | Should BeLike $expected
    }

    It "installs multiple servers in new profile" {
        InstallMultipleInProfile
    }

    It "installs multiple servers in existing empty profile" {
        New-Item $Profile -Type File -Force

        InstallMultipleInProfile
    }

    It "installs multiple servers correctly in profile with content in it" {

        New-Item $Profile -Type File -Force

        Add-Content $Profile "Write-Host `"hello`""

        Install-GoPrtgServer

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        $content = gc $Profile -Raw

        $expected = "Write-Host `"hello`"$nl"
        $expected += "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "installs multiple servers correctly in profile adding content, server, content, then another server" {

        New-Item $Profile -Type File -Force

        Add-Content $Profile "Write-Host `"hello`""

        Install-GoPrtgServer

        Add-Content $Profile "Write-Host `"goodbye`""

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            Install-GoPrtgServer
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        $content = gc $Profile -Raw

        $expected = "Write-Host `"hello`"$nl"
        $expected += "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"
        $expected += "Write-Host `"goodbye`"$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "installs with alias" {
        Install-GoPrtgServer prod

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "installs with a null alias" {
        Install-GoPrtgServer $null

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "installs with an empty string alias" {
        Install-GoPrtgServer ""

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "treats a null and empty alias as being the same with the same server but different usernames" {
        Install-GoPrtgServer

        try
        {
            Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

            $client = Get-PrtgClient

            { Install-GoPrtgServer "" } | Should Throw "Cannot add server '$($client.Server)': a record for the server already exists without an alias. Please update the alias of this record with Set-GoPrtgAlias and try again."
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "treats a null and empty alias as being the same with a different server" {
        Install-GoPrtgServer

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username 12345678) -PassHash -Force

            $client = Get-PrtgClient

            Install-GoPrtgServer ""
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        $contents = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example2.com```",,```"username```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $contents | Should BeLike $expected
    }

    It "installs multiple with alias" {
        InstallMultipleWithAlias
    }

    It "installs multiple entries for a server with different usernames and aliases" {
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

        $contents = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example.com```",```"dev```",```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $contents | Should BeLike $expected
    }

    It "throws when username for server exists already" {

        $client = Get-PrtgClient

        Install-GoPrtgServer

        { Install-GoPrtgServer } | Should Throw "Cannot add server '$($client.Server)': a record for the user '$($client.UserName)' already exists."
    }

    #todo: update-goprtgserver should allow specifying the server to update explicitly

    It "throws when server exists with alias but new record for same server is missing alias" {

        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

            $client = Get-PrtgClient

            { Install-GoPrtgServer } | Should Throw "Cannot add server '$($client.Server)': an alias must be specified to differentiate this connection from an existing connection with the same server address."
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "throws when server exists already without alias on existing record" {

        Install-GoPrtgServer

        try
        {
            Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

            $client = Get-PrtgClient

            { Install-GoPrtgServer } | Should Throw "Cannot add server '$($client.Server)': a record for the server already exists without an alias. Please update the alias of this record with Set-GoPrtgAlias and try again."
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "throws when alias already exists" {
        Install-GoPrtgServer prod

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            { Install-GoPrtgServer prod } | Should Throw "Cannot add server 'prtg.example2.com' with alias 'prod': a record for the alias already exists. For more information see 'Get-GoPrtgServer prod'"
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "throws installing a duplicate username/server combination even with a different alias" {
        Install-GoPrtgServer prod
        { Install-GoPrtgServer dev } | Should Throw "Cannot add server 'prtg.example.com': a record for the user 'username' already exists. To update the alias of this record use Set-GoPrtgAlias. To reinstall this record, first uninstall with Uninstall-GoPrtgServer and then re-run Install-GoPrtgServer."
    }

    It "throws when getServers function is missing" {
        Install-GoPrtgServer

        # Simulate the function being missing from the file

        Remove-Item Function:\__goPrtgGetServers

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            { Install-GoPrtgServer dev } | Should Throw "GoPrtg header and footer are present in PowerShell profile, however __goPrtgGetServers function was not loaded into the current session. Please verify the function has not been corrupted or remove the GoPrtg header and footer and re-run Install-GoPrtgServer."
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }

    It "throws when GoPrtg start block is missing" {
        Install-GoPrtgServer

        $contents = gc $Profile

        $newContents = $contents | where {$_ -ne "########################### Start GoPrtg Servers ###########################"}

        Set-Content $Profile $newContents

        { Install-GoPrtgServer } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
    }

    It "throws when GoPrtg end block is missing" {
        Install-GoPrtgServer

        $contents = gc $Profile

        $newContents = $contents | where {$_ -ne "############################ End GoPrtg Servers ############################"}

        Set-Content $Profile $newContents

        { Install-GoPrtgServer } | Should Throw "GoPrtg Servers end line '############################ End GoPrtg Servers ############################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
    }

    It "throws when both the header and footer have been removed" {

        InstallInProfileFunctionWithoutHeaderFooter

        try
        {
            Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

            { Install-GoPrtgServer } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' and end line"
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }
    }
}