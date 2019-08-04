. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

Describe "Set-PrtgVersion_IT" -Tag @("PowerShell", "Build_IT") {
    It "sets version on core" -Skip:(SkipBuildTest) {
        $originalVersion = Get-PrtgVersion

        try
        {
            Set-PrtgVersion 1.2.3

            $newVersion = Get-PrtgVersion

            $newVersion.Package | Should Be "1.2.3"
        }
        finally
        {
            Set-PrtgVersion $originalVersion.File
        }
    }

    It "sets version on desktop" -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
        $originalVersion = Get-PrtgVersion -Legacy

        try
        {
            Set-PrtgVersion 1.2.3 -Legacy

            $newVersion = Get-PrtgVersion -Legacy

            $newVersion.Package | Should Be "1.2.3"
        }
        finally
        {
            Set-PrtgVersion $originalVersion.File -Legacy
        }
    }
}