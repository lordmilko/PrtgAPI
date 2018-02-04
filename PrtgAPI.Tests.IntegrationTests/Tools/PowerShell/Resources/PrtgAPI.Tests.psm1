New-Alias PrtgCov Get-PrtgCoverage

class MissingSensorType
{
    [string]$Id
    [string]$Name
    [string]$Description
    [bool]$Missing

    MissingSensorType($id, $name, $description, $missing)
    {
        $this.Id = $id
        $this.Name = $name
        $this.Description = $description
        $this.Missing = $missing
    }
}

function GetProjectRoot
{
    $path = (Get-Module "PrtgAPI.Tests").Path | split-path

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

    Write-Host -ForegroundColor Cyan "Using PrtgAPI at $rootFolder"

    return $rootFolder
}

$functions = Get-ChildItem $PSScriptRoot\*.ps1

foreach($function in $functions)
{
    . $function.FullName
}