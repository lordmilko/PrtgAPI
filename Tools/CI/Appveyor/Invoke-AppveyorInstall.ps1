function Invoke-AppveyorInstall
{
    Write-LogHeader "Installing build dependencies"

    $dependencies = @(
        { choco upgrade chocolatey --limitoutput --no-progress -y }
        { cinst pester --version 3.4.6 --no-progress --limitoutput -y }
        { cinst codecov --no-progress --limitoutput -y }
        { cinst opencover.portable --no-progress --limitoutput -y }
        { cinst reportgenerator.portable --no-progress --limitoutput -y }
    )

    foreach($dependency in $dependencies)
    {
        Write-LogInfo "`tExecuting $dependency"

        Invoke-Process $dependency
    }

    if(!(Get-PackageProvider | where name -eq nuget))
    {
        Write-LogInfo "`tInstalling NuGet package provider"
        Install-PackageProvider -Name NuGet -MinimumVersion '2.8.5.201' -Force
    }
    else
    {
        Write-LogInfo "`tNot installing NuGet package provider as it is already installed"
    }
}