. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

$testCases = @(
    @{name = "Debug"}
    @{name = "Release"}
)

Describe "Clear-PrtgBuild_IT" -Tag @("PowerShell", "Build_IT") {
    It "clears the last build on core for <name>" -TestCases $testCases -Skip:(SkipBuildTest) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Configuration $name

        Clear-PrtgBuild -Configuration $name
    }

    It "clears the last build on desktop for <name>" -TestCases $testCases -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Configuration $name -Legacy

        Clear-PrtgBuild -Configuration $name -Legacy
    }

    It "clears all files" -Skip:(SkipBuildTest) {
        Clear-PrtgBuild -Full

        Invoke-PrtgBuild

        Clear-PrtgBuild -Full
    }
}