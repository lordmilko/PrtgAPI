. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

Describe "Get-PrtgVersion_IT" -Tag @("PowerShell", "Build_IT") {
    It "gets the version on core" -Skip:(SkipBuildTest) {
        Get-PrtgVersion
    }

    It "gets the version on desktop" -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
        Get-PrtgVersion -Legacy
    }
}