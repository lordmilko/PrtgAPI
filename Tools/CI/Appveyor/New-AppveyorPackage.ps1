. "$PSScriptRoot\..\..\..\Tools\CI\Helpers\PackageManager.ps1"

$ProgressPreference = "SilentlyContinue"
$ErrorActionPreference = "Stop"

function New-AppveyorPackage
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    Write-LogHeader "Building NuGet Package (Core: $IsCore)"

    $config = [PSCustomObject]@{
        SolutionRoot          = "$env:APPVEYOR_BUILD_FOLDER"
        CSharpProjectRoot     = "$env:APPVEYOR_BUILD_FOLDER\PrtgAPI"
        CSharpOutputDir       = "$env:APPVEYOR_BUILD_FOLDER\PrtgAPI\bin\$env:CONFIGURATION"
        PowerShellProjectRoot = "$env:APPVEYOR_BUILD_FOLDER\PrtgAPI.PowerShell"
        PowerShellOutputDir   = Get-PowerShellOutputDir $env:APPVEYOR_BUILD_FOLDER $env:CONFIGURATION $IsCore
        Manager               = New-PackageManager
        IsCore                = $IsCore
    }

    Process-CSharpPackage $config
    Process-PowerShellPackage $config
}

#region C#

function Process-CSharpPackage($config)
{
    Write-LogSubHeader "`tProcessing C# package"

    $config.Manager.InstallCSharpPackageSource()

    $csharpArgs = @{
        BuildFolder = $config.SolutionRoot
        OutputFolder = ([PackageManager]::RepoLocation)
        Version = Get-CSharpVersion
        Configuration = $env:CONFIGURATION
        IsCore = $config.IsCore
    }

    New-CSharpPackage @csharpArgs
    Test-CSharpPackage $config

    Move-AppveyorPackages $config

    $config.Manager.UninstallCSharpPackageSource()
}

function Get-CSharpVersion
{
    if($env:APPVEYOR)
    {
        # Trim any version qualifiers (-build.2, etc)
        return $env:APPVEYOR_BUILD_VERSION -replace "-.+"
    }
    else
    {
        return Get-PrtgVersion
    }
}

function Test-CSharpPackage
{
    Write-LogInfo "`t`tTesting package"

    $nupkg = Get-CSharpNupkg

    Extract-Package $nupkg {

        param($extractFolder)

        Test-CSharpPackageDefinition $extractFolder
        Test-CSharpPackageContents $extractFolder
    }

    Test-CSharpPackageInstalls
}

function Test-CSharpPackageDefinition($extractFolder)
{
    Write-LogInfo "`t`t`tValidating package definition"

    $nuspec = gci $extractFolder -Filter *.nuspec

    if(!$nuspec)
    {
        throw "Couldn't find nuspec in folder '$extractFolder'"
    }

    [xml]$content = gc $nuspec.FullName
    $metadata = $content.package.metadata

    # Validate release notes

    $version = Get-PrtgVersion

    if($metadata.version -ne $version)
    {
        throw "Expected package to have version '$version' but instead had version '$($metadata.version)'"
    }

    $expectedUrl = "https://github.com/lordmilko/PrtgAPI/releases/tag/v$version"

    if(!$metadata.releaseNotes.Contains($expectedUrl))
    {
        throw "Release notes did not contain correct release version. Expected notes to contain URL '$expectedUrl'. Release notes were '$($metadata.releaseNotes)'"
    }

    if($config.IsCore)
    {
        if(!$metadata.repository)
        {
            throw "Package did not contain SourceLink details"
        }
    }
}

function Test-CSharpPackageContents($extractFolder)
{
    $legalFolders = @(
        "_rels"
        "lib"
        "package"
    )

    $legalFiles = @(
        "LICENSE"
        "PrtgAPI.nuspec"
        "[Content_Types].xml"
    )

    Test-PackageContents $extractFolder $legalFolders $legalFiles
}

function Test-CSharpPackageInstalls
{
    Write-LogInfo "`t`t`tTesting package installs properly"

    $nupkg = Get-CSharpNupkg
    $packageName = $nupkg.Name -replace ".nupkg",""
    $installPath = "$([PackageManager]::PackageLocation)\$packageName"

    if(IsNuGetPackageInstalled $installPath)
    {
        Write-LogInfo "`t`t`t`t'$packageName' is already installed. Uninstalling package"
        Uninstall-CSharpPackageInternal
    }

    Install-CSharpPackageInternal $installPath
    Test-CSharpPackageInstallInternal
    Uninstall-CSharpPackageInternal
}

function Get-CSharpNupkg
{
    $nupkg = @(gci ([PackageManager]::RepoLocation) -Filter *.nupkg|where name -NotLike "*.symbols.nupkg")

    if(!$nupkg)
    {
        throw "Could not find nupkg for project 'PrtgAPI'"
    }

    if($nupkg.Count -gt 1)
    {
        $str = "Found more than one nupkg for project 'PrtgAPI': "

        $names = $nupkg|select -ExpandProperty name|foreach { "'$_'" }

        $str += [string]::Join(", ", $names)

        throw $str
    }

    return $nupkg
}

function IsNuGetPackageInstalled($installPath)
{
    return (Get-Package PrtgAPI -Destination ([PackageManager]::PackageLocation) -ErrorAction SilentlyContinue) -or (Test-Path $installPath)
}

function Install-CSharpPackageInternal($installPath)
{
    Write-LogInfo "`t`t`t`tInstalling package from $([PackageManager]::RepoName)"

    Install-Package PrtgAPI -Source ([PackageManager]::RepoName) -ProviderName NuGet -Destination ([PackageManager]::PackageLocation) -SkipDependencies | Out-Null

    if(!(Test-Path $installPath))
    {
        throw "Package did not install successfully"
    }
}

function Test-CSharpPackageInstallInternal
{
    $folders = gci "$([PackageManager]::PackageLocation)\PrtgAPI.$(Get-PrtgVersion)\lib\net4*"

    foreach($folder in $folders)
    {
        $dll = $folders.FullName + "\PrtgAPI.dll"

        $result = (powershell -command "Add-Type -Path '$dll'; [PrtgAPI.AuthMode]::Password")

        if($result -ne "Password")
        {
            throw "Module $($folders.Name) was not loaded successfully; attempt to use module returned '$result'"
        }
    }
}

function Uninstall-CSharpPackageInternal
{
    Get-Package PrtgAPI -Provider NuGet -Destination ([PackageManager]::PackageLocation) | Uninstall-Package | Out-Null

    if(Test-Path $installPath)
    {
        throw "Module did not uninstall properly"
    }
}

#endregion
#region PowerShell

function Process-PowerShellPackage($config)
{
    Write-LogSubHeader "`tProcessing PowerShell package"

    $config.Manager.InstallPowerShellRepository()

    if($env:APPVEYOR)
    {
        Update-ModuleManifest "$($config.PowerShellOutputDir)\PrtgAPI.psd1"
    }    

    $powershellArgs = @{
        OutputDir = $config.PowerShellOutputDir
        RepoManager = $config.Manager
        Configuration = $env:CONFIGURATION
        IsCore = $config.IsCore
    }

    New-PowerShellPackage @powershellArgs

    Test-PowerShellPackage

    Move-AppveyorPackages $config "_PowerShell"

    $config.Manager.UninstallPowerShellRepository()
}

function Test-PowerShellPackage
{
    Write-LogInfo "`t`tTesting package"

    $nupkg = Get-CSharpNupkg

    Extract-Package $nupkg {

        param($extractFolder)

        Test-PowerShellPackageDefinition $extractFolder
        Test-PowerShellPackageContents $extractFolder
    }

    Test-PowerShellPackageInstalls
}

function Test-PowerShellPackageDefinition($extractFolder)
{
    Write-LogInfo "`t`t`tValidating package definition"

    $psd1Path = "$extractFolder\PrtgAPI.psd1"

    # Dynamic expression on RootModule checking the PSEdition cannot be parsed by
    # Import-PowerShellDataFile; as such, we need to remove this property

    $fullModule = "fullclr\PrtgAPI.PowerShell.dll"
    $coreModule = "coreclr\PrtgAPI.PowerShell.dll"

    $rootModule = $coreModule

    if($PSEdition -eq "Desktop")
    {
        $rootModule = $fullModule

        if(!(Test-Path $fullModule))
        {
            $rootModule = $coreModule
        }
    }

    Update-ModuleManifest $psd1Path -RootModule $rootModule

    $psd1 = Import-PowerShellDataFile $psd1Path

    $version = Get-PrtgVersion

    $expectedUrl = "https://github.com/lordmilko/PrtgAPI/releases/tag/v$version"

    if(!$psd1.PrivateData.PSData.ReleaseNotes.Contains($expectedUrl))
    {
        throw "Release notes did not contain correct release version. Expected notes to contain URL '$expectedUrl'. Release notes were '$($psd1.PrivateData.PSData.ReleaseNotes)'"
    }

    if($env:APPVEYOR)
    {
        if($psd1.CmdletsToExport -eq "*" -or !($psd1.CmdletsToExport -contains "Get-Sensor"))
        {
            throw "Module manifest was not updated to specify exported cmdlets"
        }

        if($psd1.AliasesToExport -eq "*" -or !($psd1.AliasesToExport -contains "Add-Trigger"))
        {
            throw "Module manifest was not updated to specify exported aliases"
        }
    }
}

function Test-PowerShellPackageContents($extractFolder)
{
    $legalFolders = @(
        "Functions"
        "_rels"
        "package"
        "coreclr"
        "fullclr"
    )

    $legalFiles = @(
        "about_ChannelSettings.help.txt"
        "about_ObjectSettings.help.txt"
        "about_PrtgAPI.help.txt"
        "about_SensorParameters.help.txt"
        "about_SensorSettings.help.txt"
        "PrtgAPI.dll"
        "PrtgAPI.Format.ps1xml"
        "PrtgAPI.nuspec"
        "PrtgAPI.PowerShell.dll"
        "PrtgAPI.PowerShell.xml"
        "PrtgAPI.PowerShell.dll-Help.xml"
        "PrtgAPI.psd1"
        "PrtgAPI.psm1"
        "PrtgAPI.xml"
        "[Content_Types].xml"
    )

    Test-PackageContents $extractFolder $legalFolders $legalFiles
}

function Test-PowerShellPackageInstalls
{
    Write-LogInfo "`t`t`tInstalling Package"

    Hide-Module "PrtgAPI" {

        if(!(Install-Package PrtgAPI -Source ([PackageManager]::RepoName) -AllowClobber)) # TShell has a Get-Device cmdlet
        {
            throw "PrtgAPI did not install properly"
        }

        Write-LogInfo "`t`t`t`tTesting Package cmdlets"

        try
        {
            $exe = Get-PowerShellExecutable

            $resultCmdlet =   (& $exe -command '&{ import-module PrtgAPI; try { Get-Sensor } catch [exception] { $_.exception.message }}')
            $resultFunction = (& $exe -command '&{ import-module PrtgAPI; (New-Credential a b).ToString() }')
        }
        finally
        {
            Write-LogInfo "`t`t`t`tUninstalling Package"

            if(!(Uninstall-Package PrtgAPI))
            {
                throw "PrtgAPI did not uninstall properly"
            }
        }

        Write-LogInfo "`t`t`t`tValidating cmdlet output"

        if($resultCmdlet -ne "You are not connected to a PRTG Server. Please connect first using Connect-PrtgServer.")
        {
            throw $resultCmdlet
        }

        $str = [string]::Join("", $resultFunction)

        if($resultFunction -ne "System.Management.Automation.PSCredential")
        {
            throw $resultFunction
        }
    }
}

function Get-PowerShellExecutable
{
    $package = Get-Module PrtgAPI -ListAvailable

    if($PSEdition -eq "Core")
    {
        return "pwsh.exe"
    }
    else
    {
        $dllPath = Join-Path (Split-Path $package.Path -Parent) "fullclr"

        if(Test-Path $dllPath)
        {
            return "powershell.exe"
        }
        
        return "pwsh.exe"
    }
}

function Hide-Module($name, $script)
{
    $hidden = $false

    $module = Get-Module $name -ListAvailable

    try
    {
        if($module)
        {
            $hidden = $true

            Write-LogInfo "`t`t`t`tRenaming module info files"

            foreach($m in $module)
            {
                # Rename the module info file so the package manager doesn't find it even inside
                # the renamed folder

                $moduleInfo = $m.Path -replace "PrtgAPI.psd1","PSGetModuleInfo.xml"

                if(Test-Path $moduleInfo)
                {
                    Rename-Item $moduleInfo "PSGetModuleInfo_bak.xml"
                }
            }

            Write-LogInfo "`t`t`t`tRenaming module directories"

            foreach($m in $module)
            {
                $path = Get-ModuleFolder $m

                # Check if we haven't already renamed the folder as part of a previous module
                if(Test-Path $path)
                {
                    try
                    {
                        Rename-Item $path "PrtgAPI_bak"
                    }
                    catch
                    {
                        throw "$path could not be renamed to 'PrtgAPI_bak' properly: $($_.Exception.Message)"
                    }

                    if(Test-Path $path)
                    {
                        throw "$path did not rename properly"
                    }
                }
            }
        }

        Write-LogInfo "`t`t`t`tInvoking script"

        & $script
    }
    finally
    {
        if($hidden)
        {
            Write-LogInfo "`t`t`t`tRestoring module directories"

            foreach($m in $module)
            {
                $path = (split-path (Get-ModuleFolder $m) -parent) + "\PrtgAPI_bak"

                # Check if we haven't already renamed the folder as part of a previous module
                if(Test-Path $path)
                {
                    Rename-Item $path "PrtgAPI"
                }
            }

            Write-LogInfo "`t`t`t`tRestoring module info files"

            foreach($m in $module)
            {
                $moduleInfo = $m.Path -replace "PrtgAPI.psd1","PSGetModuleInfo_bak.xml"

                if(Test-Path $moduleInfo)
                {
                    Rename-Item $moduleInfo "PSGetModuleInfo.xml"
                }
            }
        }
    }
}

function Get-ModuleFolder($module)
{
    $path = $m.Path -replace "PrtgAPI.psd1",""

    $versionFolder = "$($m.Version)\"

    if($path.EndsWith($versionFolder))
    {
        $path = $path.Substring(0, $path.Length - $versionFolder.Length)
    }

    return $path
}

#endregion

function Move-AppveyorPackages($suffix, $config)
{
   if($env:APPVEYOR)
   {
        if(!$suffix)
        {
            $suffix = ""
        }

        Move-Packages $suffix $config.SolutionRoot | Out-Null
    }
    else
    {
        Write-LogInfo "`t`t`t`tClearing repo (not running under Appveyor)"
        Clear-Repo
    } 
}

function Clear-Repo
{
    gci -recurse ([PackageManager]::RepoLocation)|remove-item -Recurse -Force
}

function Extract-Package($package, $script)
{
    $originalExtension = $package.Extension
    $newName = $package.Name -replace $originalExtension,".zip"

    $extractFolder = $package.FullName -replace $package.Extension,""

    $newItem = $null

    try
    {
        $newItem = Rename-Item -Path $package.FullName -NewName $newName -PassThru
        Expand-Archive $newItem.FullName $extractFolder

        & $script $extractFolder
    }
    finally
    {
        Remove-Item $extractFolder -Recurse -Force
        Rename-Item $newItem.FullName $package.Name
    }
}

function Test-PackageContents($folder, $legalFolders, $legalFiles)
{
    Write-LogInfo "`t`t`tValidating package contents"

    $contents = gci $extractFolder

    $folders = $contents | where PSIsContainer
    $files = $contents | where { !$_.PSIsContainer }

    Test-PackageContentsInternal "folder" $folders $legalFolders
    Test-PackageContentsInternal "file" $files $legalFiles
}

function Test-PackageContentsInternal($type, $all, $legal)
{
    $illegal = $all | where { $legal -notcontains $_.Name }

    if($illegal)
    {
        $names = $illegal|select -ExpandProperty name|foreach { "'$_'" }

        $str = [string]::Join(", ", $names)

        throw "Package contained illegal $type(s) $str"
    }
}