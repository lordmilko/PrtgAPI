function New-PackageManager
{
    return [PackageManager]::new()
}

function Get-PackageSourceEx { Get-PackageSource }

class PackageManager
{
    static [string]$RepoName
    static [string]$RepoLocation
    static [string]$PackageLocation

    static PackageManager()
    {
        [PackageManager]::RepoName = "TempRepository"
        [PackageManager]::RepoLocation = Join-Path $env:TEMP ([PackageManager]::RepoName)
        [PackageManager]::PackageLocation = Join-Path $env:TEMP "TempPackages"
    }

    #region C#

    [void]InstallCSharpPackageSource()
    {
        $this.InstallPackageSource(
            "CSharp",
            { Get-PackageSourceEx },
            { Register-PackageSource -Name ([PackageManager]::RepoName) -Location ([PackageManager]::RepoLocation) -ProviderName "NuGet" -Trusted },
            { Unregister-PackageSource -Name ([PackageManager]::RepoName) -Location ([PackageManager]::RepoLocation) -ProviderName "NuGet" -Force -ErrorAction SilentlyContinue }
        )
    }

    [void]UninstallCSharpPackageSource()
    {
        $this.UninstallPackageSource(
            "CSharp",
            { Unregister-PackageSource -Name ([PackageManager]::RepoName) -Location ([PackageManager]::RepoLocation) -ProviderName "NuGet" -Force }
        )
    }

    #endregion
    #region PowerShell

    [void]InstallPowerShellRepository()
    {
        $this.InstallPackageSource(
            "PowerShell",
            { Get-PSRepository },
            { Register-PSRepository -Name ([PackageManager]::RepoName) -SourceLocation ([PackageManager]::RepoLocation) -PublishLocation ([PackageManager]::RepoLocation) -InstallationPolicy Trusted },
            { Unregister-PSRepository ([PackageManager]::RepoName) }
        )
    }

    [void]UninstallPowerShellRepository()
    {
        $this.UninstallPackageSource(
            "PowerShell",
            { Unregister-PSRepository ([PackageManager]::RepoName) }
        )
    }

    #endregion
    #region Generic Package Source

    [void]InstallPackageSource($language, $exists, $register, $unregister)
    {
        Write-LogInfo "`t`tInstalling temp $language repository"

        if(Test-Path ([PackageManager]::RepoLocation))
        {
            Write-LogError "`t`t`tRemoving repository folder left over from previous run..."

            Remove-Item ([PackageManager]::RepoLocation) -Recurse -Force
        }

        Write-LogInfo "`t`t`tCreating repository folder"
        New-Item -ItemType Directory ([PackageManager]::RepoLocation) | Out-Null

        if((& $exists) | where name -eq ([PackageManager]::RepoName))
        {
            Write-LogError "`t`t`tRemoving repository left over from previous run..."
            & $unregister
        }

        Write-LogInfo "`t`t`tRegistering temp repository"
        & $register | Out-Null
    }

    [void]UninstallPackageSource($language, $unregister)
    {
        Write-LogInfo "`t`tUninstalling temp $language repository"

        Write-LogInfo "`t`t`tUnregistering temp repository"
        & $unregister

        Write-LogInfo "`t`t`tRemoving temp repository folder"
        Remove-Item ([PackageManager]::RepoLocation) -Recurse -Force
    }

    #endregion

    [void]WithTempCopy($folderName, $script)
    {
        $tempPath = Join-Path ([PackageManager]::RepoLocation) "TempOutput\$(Split-Path $folderName -Leaf)"

        Copy-Item -Path $folderName -Destination $tempPath -Recurse -Force

        try
        {
            & $script $tempPath
        }
        finally
        {
            Remove-Item "$([PackageManager]::RepoLocation)\TempOutput" -Recurse -Force
        }
    }
}