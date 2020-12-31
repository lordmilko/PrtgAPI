function Invoke-AppveyorTest
{
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Executing tests"

    Invoke-AppveyorPesterTest $IsCore
    Invoke-AppveyorCSharpTest $IsCore
}

function Invoke-AppveyorPesterTest($IsCore)
{
    if($PSEdition -eq "Desktop" -and !$env:APPVEYOR -and $IsCore)
    {
        Write-LogInfo "`tExecuting PowerShell tests under PowerShell Core"

        pwsh -NonInteractive -Command "Invoke-Pester '$env:APPVEYOR_BUILD_FOLDER\src\PrtgAPI.Tests.UnitTests\PowerShell' -EnableExit"

        $failed = $LASTEXITCODE

        if($failed -gt 0)
        {
            throw "$failed Pester tests failed"
        }
    }
    else
    {
        $result = Invoke-CIPowerShellTest $env:APPVEYOR_BUILD_FOLDER -IsCore:$IsCore

        if($env:APPVEYOR)
        {
            foreach($test in $result.TestResult)
            {
                $appveyorTestArgs = @{
                    Name = GetAppveyorTestName $test
                    Framework = "Pester"
                    Filename = "$($test.Describe).Tests.ps1"
                    Outcome = GetAppveyorTestOutcome $test
                    ErrorMessage = $test.FailureMessage
                    Duration = [long]$test.Time.TotalMilliseconds
                }

                Add-AppveyorTest @appveyorTestArgs
            }
        }

        if($result.FailedCount -gt 0)
        {
            throw "$($result.FailedCount) Pester tests failed"
        }
    }
}

function GetAppveyorTestName($test)
{
    $name = $test.Describe

    if(![string]::IsNullOrEmpty($test.Context))
    {
        $name += ": $($test.Context)"
    }

    $name += ": $($test.Name)"

    return $name
}

function GetAppveyorTestOutcome($test)
{
    switch($test.Result)
    {
        "Passed" { "Passed" }
        "Failed" { "Failed" }
        "Skipped" { "Skipped" }
        "Pending" { "NotRunnable" }
        "Inconclusive" { "Inconclusive" }
        default {
            throw "Test $(GetAppveyorTestName $test) completed with unknown result '$_'"
        }
    }
}

function Invoke-AppveyorCSharpTest($IsCore)
{
    if($IsCore)
    {
        $additionalArgs = @(
            "--filter"
            "TestCategory!=SkipCI"
        )

        # .NET Core is not currently supported https://github.com/appveyor/ci/issues/2212
        <#if($env:APPVEYOR)
        {
            $additionalArgs += "--logger:Appveyor"
        }#>

        Invoke-CICSharpTest $env:APPVEYOR_BUILD_FOLDER $additionalArgs -IsCore:$IsCore
    }
    else
    {
        $additionalArgs = @(
            "/TestCaseFilter:TestCategory!=SkipCI"
        )

        if($env:APPVEYOR)
        {
            $additionalArgs += "/logger:Appveyor"
        }

        Invoke-CICSharpTest $env:APPVEYOR_BUILD_FOLDER $additionalArgs -IsCore:$IsCore
    }
}