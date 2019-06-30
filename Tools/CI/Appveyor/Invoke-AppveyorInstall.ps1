function Invoke-AppveyorInstall
{
    Write-LogHeader "Installing build dependencies"

    Install-CIDependency -Log
}