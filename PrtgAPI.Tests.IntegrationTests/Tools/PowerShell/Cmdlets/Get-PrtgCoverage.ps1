function Get-PrtgCoverage
{
    [CmdletBinding()]
    param ()

    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12

    Invoke-WebRequest https://gist.githubusercontent.com/lordmilko/5291d64509dab5bd6c2d4556df988371/raw/Get-CodeCoverage.ps1 -OutFile "$env:temp\Get-CodeCoverage.ps1"
    . $env:temp\Get-CodeCoverage.ps1

    $folder = GetProjectRoot

    Get-CodeCoverage -BuildFolder $folder "Debug"

    $date = (Get-Date).ToString("yyyy-MM-dd_HH-mm-ss")

    $dir = "$env:temp\PrtgCoverage_$($date)"

    Write-Host -ForegroundColor Cyan "Generating coverage report in $dir"

    Create-CoverageReport -TargetDir $dir

    start $dir\index.htm
}