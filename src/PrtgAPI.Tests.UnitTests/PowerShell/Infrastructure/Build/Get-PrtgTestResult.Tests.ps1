. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

function global:GetCSharpContent
{
    return @"
<TestRun>
    <Results>
        <UnitTestResult testName="Test1" outcome="Passed" duration="00:00:00.0007141"/>
        <UnitTestResult testName="Test2" outcome="Failed" duration="00:01:00.0007141">
            <Output>
                <ErrorInfo>
                    <Message>Failed :(</Message>
                    <StackTrace>Somewhere</StackTrace>
                </ErrorInfo>
            </Output>
        </UnitTestResult>
    </Results>
</TestRun>
"@
}

function global:GetPowerShellContent
{
    return @"
<test-results>
    <test-suite>
        <results>
            <test-suite name="Describe 1">
                <results>
                    <test-case name="Describe 1.Test 1" result="Success" time="0.0948" />
                </results>
            </test-suite>
            <test-suite name="Describe 2">
                <results>
                    <test-case name="Describe 2.Test 2" result="Failure" time="1.0948">
                        <failure>
                            <message>Failed :(</message>
                            <stack-trace>Somewhere</stack-trace>
                        </failure>
                    </test-case>
                </results>
            </test-suite>
        </results>
    </test-suite>
</test-results>
"@
}

function MockCommands
{
    InModuleScope "PrtgAPI.Build" {
        Mock "Get-ChildItem" {
            param(
                $Path,
                $Filter
            )

            $Path | Should BeLike "*PrtgAPI.Tests.UnitTests*" | Out-Null

            if($Filter -eq "*PowerShell*")
            {
                return [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_PowerShell"
            }
            elseif($Filter -eq "*C#*")
            {
                return [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_C#"
            }

            return @(
                [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_C#"
                [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_PowerShell"
            )
        }

        Mock "Test-Path" {
            return $true
        }

        Mock "Get-Content" {
            param(
                $Path
            )

            if($Path -eq "$(FSRoot)PrtgAPI_C#" -or $Path -eq "$(FSRoot)PrtgAPI_IT_C#")
            {
                return GetCSharpContent
            }
            elseif($Path -eq "$(FSRoot)PrtgAPI_PowerShell" -or $Path -eq "$(FSRoot)PrtgAPI_IT_PowerShell")
            {
                return GetPowerShellContent
            }

            throw "Path was $Path"
        }
    }
}

function MockIntegrationGetChildItem
{
    Mock "Get-ChildItem" {
        param(
            $Path,
            $Filter
        )

        $Path | Should BeLike "*PrtgAPI.Tests.IntegrationTests*" | Out-Null

        if($Filter -eq "*PowerShell*")
        {
            return [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_IT_PowerShell"
        }
        elseif($Filter -eq "*C#*")
        {
            return [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_IT_C#"
        }

        return @(
            [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_IT_C#"
            [System.IO.DirectoryInfo]"$(FSRoot)PrtgAPI_IT_PowerShell"
        )
    } -ModuleName "PrtgAPI.Build"
}

Describe "Get-PrtgTestResult" -Tag @("PowerShell", "Build") {

    It "lists all test results" {
        MockCommands

        $results = Get-PrtgTestResult

        $results.Count | Should Be 4
        $results[0] | Should Be "@{Name=Test1; Outcome=Success; Duration=00:00:00.0010000; Type=C#; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_C#}"
        $results[1] | Should Be "@{Name=Test2; Outcome=Failed; Duration=00:01:00.0010000; Type=C#; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_C#}"
        $results[2] | Should Be "@{Name=Describe 1: Test 1; Outcome=Success; Duration=00:00:00.0950000; Type=PS; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_PowerShell}"
        $results[3] | Should Be "@{Name=Describe 2: Test 2; Outcome=Failed; Duration=00:00:01.0950000; Type=PS; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_PowerShell}"
    }

    It "filters test results to a particular test" {
        MockCommands

        $results = Get-PrtgTestResult "*test *"

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Describe 1: Test 1; Outcome=Success; Duration=00:00:00.0950000; Type=PS; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_PowerShell}"
        $results[1] | Should Be "@{Name=Describe 2: Test 2; Outcome=Failed; Duration=00:00:01.0950000; Type=PS; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_PowerShell}"
    }

    It "filters to C# tests only" {

        MockCommands

        $results = Get-PrtgTestResult -Type C#

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Test1; Outcome=Success; Duration=00:00:00.0010000; Type=C#; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_C#}"
        $results[1] | Should Be "@{Name=Test2; Outcome=Failed; Duration=00:01:00.0010000; Type=C#; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_C#}"
    }

    It "filters to PowerShell tests only" {

        MockCommands

        $results = Get-PrtgTestResult -Type PowerShell

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Describe 1: Test 1; Outcome=Success; Duration=00:00:00.0950000; Type=PS; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_PowerShell}"
        $results[1] | Should Be "@{Name=Describe 2: Test 2; Outcome=Failed; Duration=00:00:01.0950000; Type=PS; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_PowerShell}"
    }

    It "filters to tests that have succeeded" {

        MockCommands

        $results = Get-PrtgTestResult -Outcome Success

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Test1; Outcome=Success; Duration=00:00:00.0010000; Type=C#; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_C#}"
        $results[1] | Should Be "@{Name=Describe 1: Test 1; Outcome=Success; Duration=00:00:00.0950000; Type=PS; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_PowerShell}"
    }

    It "filters to tests that have failed" {

        MockCommands

        $results = Get-PrtgTestResult -Outcome Failed

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Test2; Outcome=Failed; Duration=00:01:00.0010000; Type=C#; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_C#}"
        $results[1] | Should Be "@{Name=Describe 2: Test 2; Outcome=Failed; Duration=00:00:01.0950000; Type=PS; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_PowerShell}"
    }

    It "lists all test files available" {
        MockCommands

        $results = Get-PrtgTestResult -ListAvailable

        $results.Count | Should Be 2
        $results[0] | Should Be "$(FSRoot)PrtgAPI_C#"
        $results[1] | Should Be "$(FSRoot)PrtgAPI_PowerShell"
    }

    It "lists available integration test results" {

        MockCommands
        MockIntegrationGetChildItem

        $results = Get-PrtgTestResult -ListAvailable -Integration

        $results.Count | Should Be 2
        $results[0] | Should Be "$(FSRoot)PrtgAPI_IT_C#"
        $results[1] | Should Be "$(FSRoot)PrtgAPI_IT_PowerShell"
    }

    It "retrieves c# integration test results" {
        MockCommands
        MockIntegrationGetChildItem

        $results = Get-PrtgTestResult -Type C# -Integration

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Test1; Outcome=Success; Duration=00:00:00.0010000; Type=C#; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_IT_C#}"
        $results[1] | Should Be "@{Name=Test2; Outcome=Failed; Duration=00:01:00.0010000; Type=C#; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_IT_C#}"
    }

    It "retrieves PowerShell integration test results" {
        MockCommands
        MockIntegrationGetChildItem

        $results = Get-PrtgTestResult -Type PowerShell -Integration

        $results.Count | Should Be 2
        $results[0] | Should Be "@{Name=Describe 1: Test 1; Outcome=Success; Duration=00:00:00.0950000; Type=PS; Message=; StackTrace=; File=$(FSRoot)PrtgAPI_IT_PowerShell}"
        $results[1] | Should Be "@{Name=Describe 2: Test 2; Outcome=Failed; Duration=00:00:01.0950000; Type=PS; Message=Failed :(; StackTrace=Somewhere; File=$(FSRoot)PrtgAPI_IT_PowerShell}"
    }
}