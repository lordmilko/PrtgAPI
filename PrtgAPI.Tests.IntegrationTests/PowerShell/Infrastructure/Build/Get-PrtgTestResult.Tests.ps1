. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

$testCases = @(
    @{name = "Debug"}
    @{name = "Release"}
)

Describe "Get-PrtgTestResult_IT" -Tag @("PowerShell", "Build_IT") {
    
    It "gets test results from core for <name>" -TestCases $testCases -Skip:(SkipBuildTest) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Configuration $name

        WithNewProcess "Invoke-PrtgTest -Configuration $name -Build"

        Get-PrtgTestResult
    }
    
    It "gets test results from desktop for <name>" -TestCases $testCases -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {

        param($name)

        Clear-PrtgBuild -Full

        Invoke-PrtgBuild -Configuration $name -Legacy

        WithNewProcess "Invoke-PrtgTest -Configuration $name -Legacy -Build" "powershell"

        Get-PrtgTestResult
    }
}