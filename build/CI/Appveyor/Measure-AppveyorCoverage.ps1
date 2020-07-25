function Measure-AppveyorCoverage
{
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Calculating code coverage"

    Get-CodeCoverage -IsCore:$IsCore -Configuration $env:CONFIGURATION

    $lineCoverage = Get-LineCoverage

    $threshold = 95.3

    if($lineCoverage -lt $threshold)
    {
        $msg = "Code coverage was $lineCoverage%. Coverage must be higher than $threshold%"

        Write-LogError $msg

        throw $msg
    }
    else
    {
        Write-LogInfo "`tCoverage report completed with $lineCoverage% code coverage"

        if($env:APPVEYOR)
        {
            Write-LogInfo "`tUploading coverage to codecov"
            Invoke-Process { cmd /c "codecov -f `"$env:temp\opencover.xml`" 2> nul" } -WriteHost
        }
    }
}