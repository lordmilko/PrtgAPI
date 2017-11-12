. $PSScriptRoot\Support\Standalone.ps1

Describe "Connect-PrtgServer" {

    InitializeUnitTestModules

    It "utilizes a passhash" {
        Disconnect-PrtgServer

        Connect-PrtgServer prtg.example.com (New-Credential prtgadmin 12345678) -PassHash
    }

    It "throws connecting to another server without -Force" {
        Disconnect-PrtgServer

        Connect-PrtgServer prtg.example.com (New-Credential prtgadmin 12345678) -PassHash

        { Connect-PrtgServer prtg.example2.com (New-Credential username password) } | Should Throw "Already connected to server"
    }

    It "connects to another server by specifying -Force" {
        Connect-PrtgServer prtg.example2.com (New-Credential username 12345678) -PassHash -Force
    }

    It "requires a connection to use a cmdlet" {
        Disconnect-PrtgServer

        { Get-Sensor } | Should Throw "You are not connected to a PRTG Server"
    }

    It "specifies a retry count and delay" {
        Disconnect-PrtgServer

        Connect-PrtgServer prtg.example.com (New-Credential username 12345678) -PassHash -RetryCount 20 -RetryDelay 400

        $client = Get-PrtgClient

        $client.RetryCount | Should Be 20
        $client.RetryDelay | Should Be 400

        Disconnect-PrtgServer
    }

    It "specifies no progress" {
        Connect-PrtgServer prtg.example.com (New-Credential username 12345678) -PassHash -Progress $false
    }
}