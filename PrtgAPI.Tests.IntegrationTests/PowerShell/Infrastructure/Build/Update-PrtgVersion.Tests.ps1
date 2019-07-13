. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

Describe "Update-PrtgVersion_IT" -Tag @("PowerShell", "Build_IT") {
    It "updates version on core" -Skip:(SkipBuildTest) {
        $originalVersion = (Get-PrtgVersion).File

        try
        {
            Update-PrtgVersion

            $newVersion = (Get-PrtgVersion).File

            $newStr = "$($originalVersion.Major).$($originalVersion.Minor).$($originalVersion.Build + 1).$($originalVersion.Revision)"

            $newVersion | Should Be $newStr
        }
        finally
        {
            Set-PrtgVersion $originalVersion
        }
    }

    It "updates version on desktop" -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
        $originalVersion = (Get-PrtgVersion -Legacy).File

        try
        {
            Update-PrtgVersion -Legacy

            $newVersion = (Get-PrtgVersion -Legacy).File

            $newStr = "$($originalVersion.Major).$($originalVersion.Minor).$($originalVersion.Build + 1).$($originalVersion.Revision)"

            $newVersion | Should Be $newStr
        }
        finally
        {
            Set-PrtgVersion $originalVersion -Legacy
        }
    }
}