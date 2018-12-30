$vstest = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

function Invoke-AppveyorTest
{
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Executing tests"

    Invoke-AppveyorPesterTest
    Invoke-AppveyorCSharpTest
}

function Invoke-AppveyorPesterTest
{
    Write-LogInfo "`tExecuting PowerShell tests"

    $result = Invoke-Pester $env:APPVEYOR_BUILD_FOLDER\PrtgAPI.Tests.UnitTests\PowerShell -PassThru

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

function Invoke-AppveyorCSharpTest
{
    Write-LogInfo "`tExecuting C# tests"

    if($IsCore)
    {
        throw ".NET Core is not currently supported"
    }
    else
    {
        $vstestArgs = @(
            "/TestCaseFilter:TestCategory!=SkipCI"
            "$env:APPVEYOR_BUILD_FOLDER\PrtgAPI.Tests.UnitTests\bin\$env:CONFIGURATION\PrtgAPI.Tests.UnitTests.dll"
        )

        if($env:APPVEYOR)
        {
            $vstestArgs += "/logger:Appveyor"
        }

        Invoke-Process { & $vstest @vstestArgs } -Host
    }
}