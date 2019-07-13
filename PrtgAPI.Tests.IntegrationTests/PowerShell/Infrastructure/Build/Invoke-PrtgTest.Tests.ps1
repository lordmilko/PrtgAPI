. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

$testCases = @(
    @{name = "Debug"}
    @{name = "Release"}
)

Describe "Invoke-PrtgTest_IT" -Tag @("PowerShell", "Build_IT") {
    
    It "tests on core" -Skip:(SkipBuildTest) {

        Clear-PrtgBuild -Full

        # Need a Release candidate to get a PowerShell Core version of PrtgAPI.Tests.UnitTests
        Invoke-PrtgBuild -Configuration Release

        WithNewProcess "Invoke-PrtgTest -Configuration Release -Build"
    }
    
    It "tests on desktop for <name>" -TestCases $testCases -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Legacy -Configuration Release

        WithNewProcess "Invoke-PrtgTest -Legacy -Configuration Release -Build" "powershell"
    }
}