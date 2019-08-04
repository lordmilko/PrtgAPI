. $PSScriptRoot\..\..\..\Support\PowerShell\GoPrtg.ps1

Describe "Uninstall-GoPrtgServer" -Tag @("PowerShell", "UnitTest") {

    $nl = [Environment]::NewLine

    $baseExpected = ("########################### Start GoPrtg Servers ###########################$nl$nl" +
                    "function __goPrtgGetServers {@($nl    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl)}$nl" +
                    "$nl############################ End GoPrtg Servers ############################$nl").Replace("``", "````")

    BeforeAll { GoPrtgBeforeAll    }

    BeforeEach { GoPrtgBeforeEach }
    AfterEach { GoPrtgAfterEach }

    AfterAll { GoPrtgAfterAll }

    It "uninstalls correctly in empty profile" {
        InstallInEmptyProfile $baseExpected

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should BeNullOrEmpty
    }

    It "uninstalls correctly in profile with single existing line" {
        InstallInProfileWithContent $baseExpected $false

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should Be "Write-Host `"hello`"$nl"
    }

    It "uninstalls correctly in profile with multiple existing lines" {
        InstallInProfileWithContent $baseExpected $true

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should Be "Write-Host `"hello`"$($nl)Write-Host `"`what what?`"$nl"
    }

    It "uninstalls correctly in profile between lines" {
        InstallInProfileWithContent $baseExpected $true

        Add-Content $Profile "Write-Host `"test1`""

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should Be "Write-Host `"hello`"$($nl)Write-Host `"`what what?`"$($nl)Write-Host `"test1`"$nl"
    }

    It "uninstalls single entry when server specified" {
        InstallMultipleInProfile

        Uninstall-GoPrtgServer prtg.example.com

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "uninstalls matching entries when wildcard server specified" {
        
        Install-GoPrtgServer

        try
        {
            Connect-PrtgServer prtg3.example.com (New-Credential username3 12345678) -PassHash -Force

            Install-GoPrtgServer

            Connect-PrtgServer prtg.example2.com (New-Credential username2 87654321) -PassHash -Force

            Install-GoPrtgServer
        }
        finally
        {
            Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
        }

        $content = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",$nl"
        $expected += "    `"```"prtg3.example.com```",,```"username3```",```"*```"`",$nl"
        $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected

        Uninstall-GoPrtgServer prtg.*

        $finalContent = gc $Profile -Raw

        $finalExpected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $finalExpected += "function __goPrtgGetServers {@($nl"
        $finalExpected += "    `"```"prtg3.example.com```",,```"username3```",```"*```"`"$nl"
        $finalExpected += ")}$nl$nl"
        $finalExpected += "############################ End GoPrtg Servers ############################$nl"

        $finalExpected = $finalExpected.Replace("``", "````")

        $finalContent | Should BeLike $finalExpected
    }

    It "uninstalls everything when last wildcard server specified" {
        Install-GoPrtgServer

        Uninstall-GoPrtgServer prtg.*

        $contents = gc $Profile -Raw

        $contents | Should BeNullOrEmpty
    }
    
    It "uninstalls when alias specified" {
        InstallMultipleWithAlias

        Uninstall-GoPrtgServer prod

        $contents = gc $Profile -Raw

        $expected = "########################### Start GoPrtg Servers ###########################$nl$nl"
        $expected += "function __goPrtgGetServers {@($nl"
        $expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"$nl"
        $expected += ")}$nl$nl"
        $expected += "############################ End GoPrtg Servers ############################$nl"

        $expected = $expected.Replace("``", "````")

        $contents | Should BeLike $expected
    }

    It "uninstalls all when -Force specified" {

        InstallMultipleWithAlias

        Uninstall-GoPrtgServer -Force

        $contents = gc $Profile -Raw

        $contents | Should BeNullOrEmpty
    }

    It "removes getServers function from global scope" {
        Install-GoPrtgServer

        $servers = __goPrtgGetServers
        $servers | Should Not BeNullOrEmpty

        Uninstall-GoPrtgServer

        { __goPrtgGetServers } | Should Throw "The term '__goPrtgGetServers' is not recognized as the name of a cmdlet"
    }

    It "removes the global function" {
        Install-GoPrtgServer
        
        Uninstall-GoPrtgServer

        { __goPrtgGetServers } | Should Throw "The term '__goPrtgGetServers' is not recognized as the name of a cmdlet"
    }

    It "updates the global function" {
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

        Uninstall-GoPrtgServer prtg.example.com

        Get-GoPrtgServer | Should Be "@{[!]=[ ]; Server=prtg.example2.com; Alias=; UserName=username2}"
    }

    It "throws when server specified that doesn't exist" {
        InstallMultipleInProfile

        { Uninstall-GoPrtgServer banana } | Should Throw "'banana' is not a valid server name or alias. To view all saved servers, run Get-GoPrtgServer"
    }

    It "throws uninstalling when not installed" {
        New-Item -Type File $Profile -Force
        { Uninstall-GoPrtgServer } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer"
    }

    It "throws uninstalling when profile doesn't exist" {
        { Uninstall-GoPrtgServer } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
    }

    It "throws uninstalling with multiple entries and server not specified" {
        InstallMultipleInProfile

        { Uninstall-GoPrtgServer } | Should Throw "Cannot remove servers; server name or alias must be specified when multiple entries exist. To remove all servers, specify -Force"
    }

    It "throws when getServers function is missing" {
        Install-GoPrtgServer

        Remove-Item Function:\__goPrtgGetServers

        { Uninstall-GoPrtgServer } | Should Throw "GoPrtg header and footer are present in PowerShell profile, however __goPrtgGetServers function was not loaded into the current session. Please verify the function has not been corrupted or remove the GoPrtg header and footer and re-run Install-GoPrtgServer."
    }

    It "throws when GoPrtg start block is missing" {
        Install-GoPrtgServer

        $contents = gc $Profile

        $newContents = $contents | where {$_ -ne "########################### Start GoPrtg Servers ###########################"}

        Set-Content $Profile $newContents

        { Uninstall-GoPrtgServer } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
    }

    It "throws when GoPrtg end block is missing" {
        Install-GoPrtgServer

        $contents = gc $Profile

        $newContents = $contents | where {$_ -ne "############################ End GoPrtg Servers ############################"}

        Set-Content $Profile $newContents

        { Uninstall-GoPrtgServer } | Should Throw "GoPrtg Servers end line '############################ End GoPrtg Servers ############################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
    }
    
    It "throws when both the header and footer have been removed" {
        InstallInProfileFunctionWithoutHeaderFooter

        { Uninstall-GoPrtgServer } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' and end line"
    }
}