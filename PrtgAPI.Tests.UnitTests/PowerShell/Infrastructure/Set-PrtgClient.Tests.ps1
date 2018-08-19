. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function Connect
{
    Connect-PrtgServer prtg.example.com (New-Credential prtgadmin 12345678) -PassHash -Force
}

Describe "Set-PrtgClient" -Tag @("PowerShell", "UnitTest") {

    $testCases = @(
        @{ name = "RetryCount"; newValue = 10 }
        @{ name = "RetryDelay"; newValue = 30 }
        @{ name = "LogLevel";   newValue = "Trace, Response"}
    )

    It "updates client property <name>" -TestCases $testCases {
        param($name, $newValue)

        Connect

        $client = (Get-PrtgClient)

        $client | Should Not BeNullOrEmpty

        $original = $client.$name

        $original | Should Not Be $newValue

        $splat = @{
            $name = $newValue
        }

        Set-PrtgClient @splat

        $client.$name | Should Be $newValue

        Disconnect-PrtgServer
    }

    It "ignoring invalid SSL requests replaces the PrtgClient and copies all properties" {
        Connect

        $original = Get-PrtgClient
        $original.RetryCount = 10
        $original.RetryDelay = 20
        $original.LogLevel = "Response"

        Set-PrtgClient -IgnoreSSL

        $newClient = Get-PrtgClient

        $newClient.RetryCount | Should Be $original.RetryCount
        $newClient.RetryDelay | Should Be $original.RetryDelay
        $newClient.LogLevel | Should Be $original.LogLevel

        $original.RetryCount = 30
        $newClient.RetryCount | Should Not Be $original.RetryCount

        Disconnect-PrtgServer
    }

    It "updates properties when IgnoreSSL is specified" {

        Connect

        $original = Get-PrtgClient

        $new = Set-PrtgClient -IgnoreSSL -RetryCount 5 -PassThru

        $original.RetryCount | Should Not Be $new.RetryCount

        Disconnect-PrtgServer
    }

    It "updates multiple properties at once" {

        Connect

        $client = Get-PrtgClient

        Set-PrtgClient -RetryCount 10 -LogLevel trace,response

        $client.RetryCount | Should Be 10
        $client.LogLevel | Should Be "Trace, Response"

        Disconnect-PrtgServer
    }

    It "passes thru when -PassThru is specified" {

        Connect

        $client = Get-PrtgClient

        Set-PrtgClient -RetryCount 10 -PassThru | Should Be $client

        Set-PrtgClient -RetryCount 15 -IgnoreSSL -PassThru | Should Not Be $client

        Disconnect-PrtgServer
    }

    It "enables/disables progress" {
        Connect

        Set-PrtgClient -Progress
        Set-PrtgClient -Progress:$false

        Disconnect-PrtgServer
    }

    It "requires a connection to a PRTG Server" {
        { Set-PrtgClient -RetryDelay 5 } | Should Throw "You are not connected to a PRTG Server"
    }
}