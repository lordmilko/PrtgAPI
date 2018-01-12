function Get-PrtgCoverage
{
    [CmdletBinding()]
    param ()

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

function GetProjectRoot
{
    $path = (Get-Module "PrtgAPI.Tests").Path | split-path

    Write-Host "path is $path"

    $junction = gi $path | select -expand target

    if($junction -ne $null)
    {
        $path = $junction
    }

    $moduleName = "PrtgAPI.Tests.IntegrationTests"
    $rootIndex = $path.ToLower().IndexOf($moduleName.ToLower())

    if($rootIndex -eq -1)
    {
        throw "Could not identity root folder"
    }

    $rootFolder = $path.Substring(0, $rootIndex)      # e.g. C:\PrtgAPI

    Write-Host -ForegroundColor Cyan "Building PrtgAPI from $rootFolder"

    return $rootFolder
}