. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function VerifyValue($value)
{
    $value | Should Not BeNullOrEmpty
    $value | Should Not Be Unknown
}

Describe "Get-PrtgClient" -Tag @("PowerShell", "UnitTest") {
    It "returns a PrtgClient" {

        SetMultiTypeResponse

        $client = Get-PrtgClient

        $client.Server | Should Be prtg.example.com
    }

    It "is `$null when a client has not been established" {
        Disconnect-PrtgServer

        Get-PrtgClient | Should Be $null
    }

    It "displays diagnostic information" {

        SetMultiTypeResponse

        $result = Get-PrtgClient -Diag

        VerifyValue $result.PSVersion
        VerifyValue $result.PSEdition
        VerifyValue $result.OS
        VerifyValue $result.PrtgAPIVersion
        VerifyValue $result.Culture
        VerifyValue $result.CLRVersion
        VerifyValue $result.PrtgVersion
        VerifyValue $result.PrtgLanguage
    }
    
    It "throws attempting to display diagnostic information when a client has not been established" {
        Disconnect-PrtgServer

        { Get-PrtgClient -Diag } | Should Throw "You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer."
    }
}