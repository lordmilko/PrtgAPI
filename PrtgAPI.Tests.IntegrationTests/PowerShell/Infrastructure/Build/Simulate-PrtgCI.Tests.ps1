. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

$testCases = @(
    @{name = "Debug"}
    @{name = "Release"}
)

Describe "Simulate-PrtgCI_IT" -Tag @("PowerShell", "Build_IT") {

    It "simulates Appveyor on core" -Skip:(SkipBuildTest) {

        if(Test-IsWindows)
        {
            WithNewProcess "Simulate-PrtgCI -Configuration Release"
        }
        else
        {
            { Simulate-PrtgCI -Configuration Release } | Should Throw "Appveyor can only be simulated on Windows"
        }
    }

    It "simulates Appveyor on desktop for <name>" -TestCases $testCases -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
        param($name)

        WithNewProcess "Simulate-PrtgCI -Configuration $name -Legacy" "powershell"
    }

    It "simulates Travis" -Skip:(SkipBuildTest) {

        WithNewProcess "Simulate-PrtgCI -Travis -Configuration Release" "pwsh"
    }
}