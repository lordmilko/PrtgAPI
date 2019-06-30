function Get-CIDependency
{
    $dependencies = @(
        #@{ Name = "chocolatey";               Chocolatey = $true;      Upgrade = $true }
        @{ Name = "codecov";                  Chocolatey = $true }
        @{ Name = "opencover.portable";       Chocolatey = $true;      CommandName = "opencover.console" }
        @{ Name = "reportgenerator.portable"; Chocolatey = $true;      CommandName = "reportgenerator" }
        @{ Name = "vswhere";                  Chocolatey = $true }
        @{ Name = "NuGet";                    PackageProvider = $true; MinimumVersion = "2.8.5.201" }
        @{ Name = "PowerShellGet";            PowerShell = $true;      MinimumVersion = "2.0.0" }
        @{ Name = "Pester";                   PowerShell = $true;      Version = "3.4.6" }
    )

    return $dependencies
}
