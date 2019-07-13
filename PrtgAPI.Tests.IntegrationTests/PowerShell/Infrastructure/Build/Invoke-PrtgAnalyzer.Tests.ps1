. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

Describe "Invoke-PrtgAnalyzer_IT" -Tag @("PowerShell", "Build_IT") {
    It "analyzes solution" -Skip:(SkipBuildTest) {
        WithNewProcess "Invoke-PrtgAnalyzer | Out-Null"
    }
}