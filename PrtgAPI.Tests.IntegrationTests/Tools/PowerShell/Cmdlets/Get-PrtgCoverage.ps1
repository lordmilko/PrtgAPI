function Get-PrtgCoverage
{
    [CmdletBinding()]
    param()

    $folder = GetProjectRoot

    ipmo $folder\Tools\Build\build.psm1

    Get-CodeCoverage -BuildFolder $folder "Debug"

    $date = (Get-Date).ToString("yyyy-MM-dd_HH-mm-ss")

    $dir = "$env:temp\PrtgCoverage_$($date)"

    Write-Host -ForegroundColor Cyan "Generating coverage report in $dir"

    New-CoverageReport -TargetDir $dir

    start $dir\index.htm
}