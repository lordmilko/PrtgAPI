function Invoke-AppveyorAfterBuild
{
    [CmdletBinding()]
    param()

    Set-AppveyorVersion
}