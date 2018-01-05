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

. $PSScriptRoot\Get-PrtgLog.ps1
. $PSScriptRoot\Get-MissingSensorTypes.ps1
. $PSScriptRoot\Write-SensorTypes.ps1