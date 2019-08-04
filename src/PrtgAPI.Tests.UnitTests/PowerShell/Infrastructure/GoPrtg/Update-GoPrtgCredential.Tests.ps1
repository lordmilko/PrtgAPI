. $PSScriptRoot\..\..\..\Support\PowerShell\GoPrtg.ps1

Describe "Update-GoPrtgCredential" -Tag @("PowerShell", "UnitTest") {

    $nl = [Environment]::NewLine

    $baseExpected = ("########################### Start GoPrtg Servers ###########################$nl$nl" + 
                    "function __goPrtgGetServers {@($nl    `"```"prtg.example.com```",,```"username```",```"*```"`"$nl)}$nl$nl" + 
                    "############################ End GoPrtg Servers ############################$nl").Replace("``","````")

    BeforeAll { GoPrtgBeforeAll    }

    BeforeEach { GoPrtgBeforeEach }
    AfterEach { GoPrtgAfterEach }

    AfterAll { GoPrtgAfterAll }

    Mock -ModuleName PrtgAPI Connect-PrtgServer {
        param($Server, $Credential, $Force)

        $username = $Credential.GetNetworkCredential().Username
        $password = $Credential.GetNetworkCredential().Password

        $client = New-Object PrtgAPI.PrtgClient -ArgumentList($Server, $username, $password, ([PrtgAPI.AuthMode]::PassHash))

        $type = (Get-PrtgClient).GetType().Assembly.GetType("PrtgAPI.PowerShell.PrtgSessionState")
        $property = $type.GetProperty("Client", [System.Reflection.BindingFlags]::Static -bor [System.Reflection.BindingFlags]::NonPublic)

        $property.SetValue($null, $client)
    }
    
    It "updates the credential" {
        Install-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should BeLike $baseExpected

        Update-GoPrtgCredential (New-Credential prtgadmin newpassword)

        (Get-PrtgClient).PassHash | Should Be newpassword

        $newContent = gc $Profile -Raw

        $content | Should Not Be $newContent
    }

    It "updates the credential from a specified PSCredential" {
        Install-GoPrtgServer

        $content = gc $Profile -Raw

        Update-GoPrtgCredential (New-Credential username newpassword)

        (Get-PrtgClient).PassHash | Should Be newpassword

        $newContent = gc $Profile -Raw

        $content | Should Not Be $newContent
    }
    
    It "updates the username and password when the username is changed" {
        Install-GoPrtgServer

        Update-GoPrtgCredential (New-Credential myuser mypassword)

        $content = gc $Profile -Raw

        $newExpected = $baseExpected.Replace("username", "myuser")

        $content | Should BeLike $newExpected

        (Get-PrtgClient).PassHash | Should Be mypassword
    }

    It "throws specifying an existing username for the current server" {
        Install-GoPrtgServer first

        Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

        Install-GoPrtgServer second

        { Update-GoPrtgCredential (New-Credential username 12345678) } | Should Throw "Cannot update credential: a record with username 'username' for server 'prtg.example.com' already exists"
    }

    It "throws when no GoPrtg servers exist" {
        { Update-GoPrtgCredential } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
    }

    It "throws updating a server that isn't registered with GoPrtg" {
        Install-GoPrtgServer

        Connect-PrtgServer prtg.example2.com (New-Credential username 12345678) -PassHash -Force

        { Update-GoPrtgCredential } | Should Throw "Server 'prtg.example2.com' is not a valid GoPrtg server. To install this server, run Install-GoPrtgServer [<alias>]"
    }

    It "throws when both the header and footer have been removed" {
        InstallInProfileFunctionWithoutHeaderFooter

        { Update-GoPrtgCredential (New-Credential username password) } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' and end line"
    }

    # and maybe also all the tests where we're not even connected to a goprtg server, or dont even have a profile, or the profile is empty...same tests set-goprtgalias uses
    # test updating a credential that isnt in goprtg
    #test replacing the username
    # add some cmdlet based help for every function in the functions folder, including new-credential
    # after doing all commits we need to convert all tabs to spaces
}