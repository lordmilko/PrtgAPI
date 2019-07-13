. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

$testCases = @(
    @{name = "Debug"}
    @{name = "Release"}
)

Describe "New-PrtgPackage_IT" -Tag @("PowerShell", "Build_IT") {
    It "creates packages on core for <name>" -TestCases $testCases -Skip:(SkipBuildTest) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Configuration $name

        if($name -eq "Debug" -or $PSEdition -ne "Core" -or ($PSEdition -eq "Core" -and $IsWindows))
        {
            New-PrtgPackage -Configuration $name
        }
        else
        {
            { New-PrtgPackage -Configuration $name } | Should Throw "$name packages can only be created on Windows."
        }
    }

    It "creates packages on desktop for <name>" -TestCases $testCases -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Configuration $name -Legacy

        New-PrtgPackage -Configuration $name -Legacy
    }
}