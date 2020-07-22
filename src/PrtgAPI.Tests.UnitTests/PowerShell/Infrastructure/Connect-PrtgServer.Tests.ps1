. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

Describe "Connect-PrtgServer" -Tag @("PowerShell", "UnitTest") {

    It "utilizes a password" {
        Disconnect-PrtgServer

        try
        {
            Connect-PrtgServer http://127.0.0.1 (New-Credential username password)
            throw "Connection should not have succeeded"
        }
        catch
        {
            if($_.exception.message -notlike "*Not Found*" -and $_.exception.message -notlike "*Connection refused*" -and $_.exception.message -notlike "*Server rejected HTTP connection*")
            {
                throw
            }
        }
    }

    It "utilizes a passhash" {
        Disconnect-PrtgServer

        Connect-PrtgServer prtg.example.com (New-Credential prtgadmin 12345678) -PassHash
    }

    It "doesn't require a password" {
        Disconnect-PrtgServer

        try
        {
            Connect-PrtgServer http://127.0.0.1 (New-Credential username)
            throw "Connection should not have succeeded"
        }
        catch
        {
            if($_.exception.message -notlike "*Not Found*" -and $_.exception.message -notlike "*Connection refused*" -and $_.exception.message -notlike "*Server rejected HTTP connection*")
            {
                throw
            }
        }
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
        Connect-PrtgServer prtg.example.com (New-Credential username 12345678) -PassHash -Progress:$false
    }

    It "passes thru a PrtgClient" {
        $client = Connect-PrtgServer prtg.example.com (New-Credential username 12345678) -PassHash -PassThru -Force

        $realClient = Get-PrtgClient

        $client | Should Be $realClient
    }

    It "specifies a log level" {

        $client = Connect-PrtgServer prtg.example.com (New-Credential username 12345678) -Passhash -PassThru -Force -LogLevel Trace,Response

        $client.LogLevel | Should Be "Trace, Response"
    }
}