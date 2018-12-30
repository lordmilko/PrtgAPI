$global:ErrorActionPreference = "Stop"
$ErrorActionPreference = "Stop"

. $PSScriptRoot\Functions\Import-ModuleFunctions.ps1
. Import-ModuleFunctions "$PSScriptRoot\Functions"

function Get-MSBuild
{
    "C:\Program Files (x86)\MSBuild\14.0\bin\amd64\msbuild.exe"
}

Export-ModuleMember Get-MSBuild
