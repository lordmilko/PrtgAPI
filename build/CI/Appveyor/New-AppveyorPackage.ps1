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
        CSharpProjectRoot     = "$env:APPVEYOR_BUILD_FOLDER\src\PrtgAPI"
        CSharpOutputDir       = "$env:APPVEYOR_BUILD_FOLDER\src\PrtgAPI\bin\$env:CONFIGURATION"
        PowerShellProjectRoot = "$env:APPVEYOR_BUILD_FOLDER\src\PrtgAPI.PowerShell"
        PowerShellOutputDir   = Get-PowerShellOutputDir $env:APPVEYOR_BUILD_FOLDER $env:CONFIGURATION $IsCore
        Manager               = New-PackageManager
        Configuration         = $env:CONFIGURATION
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
        OutputFolder = (PackageManager -RepoLocation)
        Version = Get-CSharpVersion $config.IsCore
        Configuration = $env:CONFIGURATION
        IsCore = $config.IsCore
    }

    New-CSharpPackage @csharpArgs
    Test-CSharpPackage $config

    Move-AppveyorPackages $config

    $config.Manager.UninstallCSharpPackageSource()
}

function Get-CSharpVersion($IsCore)
{
    if($env:APPVEYOR)
    {
        # Trim any version qualifiers (-build.2, etc)
        return $env:APPVEYOR_BUILD_VERSION -replace "-.+"
    }
    else
    {
        return GetVersion $IsCore
    }
}

function Test-CSharpPackage($config)
{
    Write-LogInfo "`t`tTesting package"

    $nupkg = Get-CSharpNupkg

    Extract-Package $nupkg {

        param($extractFolder)

        Test-CSharpPackageDefinition $config $extractFolder
        Test-CSharpPackageContents $config $extractFolder
    }

    Test-CSharpPackageInstalls $config
}

function Test-CSharpPackageDefinition($config, $extractFolder)
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

    $version = GetVersion $config.IsCore

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

function Test-CSharpPackageContents($config, $extractFolder)
{
    $required = @(
        "lib\net452\PrtgAPI.dll"
        "lib\net452\PrtgAPI.xml"
        "package\*"
        "_rels\*"
        "PrtgAPI.nuspec"
        "[Content_Types].xml"
    )

    if($config.IsCore)
    {
        $required += "LICENSE"

        if($env:CONFIGURATION -eq "Release")
        {
            $required += @(
                "lib\netstandard2.0\PrtgAPI.dll"
                "lib\netstandard2.0\PrtgAPI.xml"
            )
        }
        else
        {
            $debugVersion = Get-DebugTargetFramework

            Write-LogInfo "`t`t`t`tUsing debug build '$debugVersion' for testing nupkg contents"

            $required = $required | foreach {
                if($_ -like "*net452*")
                {
                    $_ -replace "net452",$debugVersion
                }
                else
                {
                    $_
                }
            }
        }
    }

    Test-PackageContents $extractFolder $required
}

function Test-CSharpPackageInstalls($config)
{
    Write-LogInfo "`t`t`tTesting package installs properly"

    $nupkg = Get-CSharpNupkg
    $packageName = $nupkg.Name -replace ".nupkg",""
    $installPath = "$(PackageManager -PackageLocation)\$packageName"

    if(IsNuGetPackageInstalled $installPath)
    {
        Write-LogInfo "`t`t`t`t'$packageName' is already installed. Uninstalling package"
        Uninstall-CSharpPackageInternal
    }

    Install-CSharpPackageInternal $installPath
    Test-CSharpPackageInstallInternal $config
    Uninstall-CSharpPackageInternal
}

function Get-CSharpNupkg
{
    $nupkg = @(gci (PackageManager -RepoLocation) -Filter *.nupkg|where { $_.Name -NotLike "*.symbols.nupkg" -and $_.Name -notlike "*.snupkg" })

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
    return (Get-Package PrtgAPI -Destination (PackageManager -PackageLocation) -ErrorAction SilentlyContinue) -or (Test-Path $installPath)
}

function Install-CSharpPackageInternal($installPath)
{
    Write-LogInfo "`t`t`t`tInstalling package from $(PackageManager -RepoName)"

    Install-Package PrtgAPI -Source (PackageManager -RepoName) -ProviderName NuGet -Destination (PackageManager -PackageLocation) -SkipDependencies | Out-Null

    if(!(Test-Path $installPath))
    {
        throw "Package did not install successfully"
    }
}

function Test-CSharpPackageInstallInternal($config)
{
    Write-LogInfo "`t`t`t`tTesting package contents"

    $version = GetVersion $config.IsCore

    $folders = gci "$(PackageManager -PackageLocation)\PrtgAPI.$version\lib\net4*"

    foreach($folder in $folders)
    {
        $dll = Join-Path $folder.FullName "PrtgAPI.dll"

        $result = (powershell -command "Add-Type -Path '$dll'; [PrtgAPI.AuthMode]::Password")

        if($result -ne "Password")
        {
            throw "Module $($folders.Name) was not loaded successfully; attempt to use module returned '$result'"
        }
    }
}

function Uninstall-CSharpPackageInternal
{
    Write-LogInfo "`t`t`t`tUninstalling package"

    Get-Package PrtgAPI -Provider NuGet -Destination (PackageManager -PackageLocation) | Uninstall-Package | Out-Null

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
        PowerShell = $true
        Redist = $true
    }

    New-PowerShellPackage @powershellArgs

    Test-PowerShellPackage $config

    Test-RedistributablePackage $config

    Move-AppveyorPackages $config "_PowerShell"

    $config.Manager.UninstallPowerShellRepository()
}

function Test-PowerShellPackage
{
    Write-LogInfo "`t`tTesting package"

    $nupkg = Get-CSharpNupkg

    Extract-Package $nupkg {

        param($extractFolder)

        Test-PowerShellPackageDefinition $config $extractFolder
        Test-PowerShellPackageContents $config $extractFolder
    }

    Test-PowerShellPackageInstalls
}

function Test-PowerShellPackageDefinition($config, $extractFolder)
{
    Write-LogInfo "`t`t`tValidating package definition"

    $psd1Path = Join-Path $extractFolder "PrtgAPI.psd1"

    if($config.IsCore -and $config.Configuration -eq "Release")
    {
        Test-Psd1RootModule $config $psd1Path

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
    }

    $psd1 = Import-PowerShellDataFile $psd1Path

    $version = GetVersion $config.IsCore

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

function Test-Psd1RootModule($config, $psd1Path)
{
    if($config.IsCore -and $config.Configuration -eq "Release")
    {
        $contents = (gc $psd1Path) -Join "`n"

        $expected = "RootModule = if(`$PSEdition -eq 'Core')`n{`n    'coreclr\PrtgAPI.PowerShell.dll'`n}`nelse # Desktop`n{`n    'fullclr\PrtgAPI.PowerShell.dll'`n}"

        if(!$contents.Contains($expected))
        {
            throw "'$psd1Path' did not contain correect RootModule for Release build"
        }
    }
}

function Test-PowerShellPackageContents($config, $extractFolder)
{
    $required = @(
        "Functions\New-Credential.ps1"
        "package\*"
        "_rels\*"
        "PrtgAPI.nuspec"
        "about_ChannelSettings.help.txt"
        "about_ObjectSettings.help.txt"
        "about_PrtgAPI.help.txt"
        "about_SensorParameters.help.txt"
        "about_SensorSettings.help.txt"
        "PrtgAPI.Format.ps1xml"
        "PrtgAPI.Types.ps1xml"
        "PrtgAPI.psd1"
        "PrtgAPI.psm1"
        "[Content_Types].xml"
    )

    if($config.IsCore -and $config.Configuration -eq "Release")
    {
        $required += @(
            "coreclr\PrtgAPI.dll"
            "coreclr\PrtgAPI.PowerShell.dll"
            "fullclr\PrtgAPI.dll"
            "fullclr\PrtgAPI.PowerShell.dll"
        )
    }
    else
    {
        $required += @(
            "PrtgAPI.dll"
            "PrtgAPI.PowerShell.dll"
        )
    }

    $debugVersion = Get-DebugTargetFramework

    if($debugVersion -like "net4*" -or $config.Configuration -eq "Release" -or !$config.IsCore)
    {
        $required += @(
            "PrtgAPI.PowerShell.dll-Help.xml"
        )
    }

    Test-PackageContents $extractFolder $required
}

function Test-PowerShellPackageInstalls
{
    Write-LogInfo "`t`t`tInstalling Package"

    if($config.Configuration -eq "Release" -and $config.IsCore)
    {
        Test-PowerShellPackageInstallsHidden "Desktop"
        Test-PowerShellPackageInstallsHidden "Core"
    }
    else
    {
        Test-PowerShellPackageInstallsHidden $PSEdition
    }
}

function Test-PowerShellPackageInstallsHidden($edition, $config)
{
    if([string]::IsNullOrEmpty($edition))
    {
        $edition = "Desktop"
    }

    Write-LogInfo "`t`t`t`tTesting package installs on $edition"

    Hide-Module $edition {

        param($edition)

        if(!(Install-EditionPackage $edition PrtgAPI -Source (PackageManager -RepoName) -AllowClobber)) # TShell has a Get-Device cmdlet
        {
            throw "PrtgAPI did not install properly"
        }

        Write-LogInfo "`t`t`t`t`tTesting Package cmdlets"

        try
        {
            Test-PowerShellPackageInstallsInternal $edition
        }
        finally
        {
            Write-LogInfo "`t`t`t`t`tUninstalling Package"

            if(!(Uninstall-EditionPackage $edition PrtgAPI))
            {
                throw "PrtgAPI did not uninstall properly"
            }
        }
    }
}

function Test-PowerShellPackageInstallsInternal($edition, $module = "PrtgAPI")
{
    $exe = Get-PowerShellExecutable $edition

    Write-LogInfo "`t`t`t`t`t`tValidating '$exe' cmdlet output"

    $resultCmdlet =   (& $exe -command "&{ import-module '$module'; try { Get-Sensor } catch [exception] { `$_.exception.message }}")
    $resultFunction = (& $exe -command "&{ import-module '$module'; (New-Credential a b).ToString() }")

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

function Get-PowerShellExecutable($edition)
{
    if($edition -eq "Core")
    {
        return "pwsh.exe"
    }
    else
    {
        return "powershell.exe"
    }
}

function Get-EditionModule
{
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Edition
    )

    $command = "Get-Module PrtgAPI -ListAvailable"

    if($PSEdition -eq $Edition)
    {
        return Invoke-Expression $command | Select Path,Version
    }
    else
    {
        $response = Invoke-Edition $Edition "$command | foreach { `$_.Path + '|' + `$_.Version }"

        foreach($line in $response)
        {
            $split = $line.Split('|')

            [PSCustomObject]@{
                Path = $split[0]
                Version = $split[1]
            }
        }
    }
}

function Install-EditionPackage
{
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Edition,

        [Parameter(Mandatory = $true, Position = 1)]
        [string]$Name,

        [Parameter()]
        [string]$Source,

        [Parameter()]
        [switch]$AllowClobber
    )

    $command = "Install-Package $Name -Source $Source -AllowClobber:([bool]'$AllowClobber')"

    if($PSEdition -eq $Edition)
    {
        return Invoke-Expression $command
    }
    else
    {
        $response = Invoke-Edition $Edition "$command | foreach { `$_.Name + '|' + `$_.Version }"

        foreach($line in $response)
        {
            $split = $line.Split('|')

            [PSCustomObject]@{
                Name = $split[0]
                Version = $split[1]
            }
        }
    }
}

function Uninstall-EditionPackage
{
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$Edition,

        [Parameter(Mandatory = $true, Position = 1)]
        [string]$Name
    )

    $command = "Uninstall-Package $Name"

    if($PSEdition -eq $Edition)
    {
        return Invoke-Expression $command
    }
    else
    {
        $response = Invoke-Edition $Edition "$command | foreach { `$_.Name + '|' + `$_.Version }"

        foreach($line in $response)
        {
            $split = $line.Split('|')

            [PSCustomObject]@{
                Name = $split[0]
                Version = $split[1]
            }
        }
    }
}

function Invoke-Edition($edition, $command)
{
    if($PSEdition -eq $edition)
    {
        throw "Cannot invoke command '$command' in edition 'edition': edition is the same as the currently running process"
    }
    else
    {
        $exe = Get-PowerShellExecutable $edition

        $response = & $exe -Command $command

        if($LASTEXITCODE -ne 0)
        {
            throw "'$edition' invocation failed with exit code '$LASTEXITCODE': $response"
        }

        return $response
    }
}

function Hide-Module($edition, $script)
{
    $hidden = $false

    $module = Get-EditionModule $edition

    try
    {
        if($module)
        {
            $hidden = $true

            Write-LogInfo "`t`t`t`t`tRenaming module info files"

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

            Write-LogInfo "`t`t`t`t`tRenaming module directories"

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

        Write-LogInfo "`t`t`t`t`tInvoking script"

        & $script $edition
    }
    finally
    {
        if($hidden)
        {
            Write-LogInfo "`t`t`t`t`tRestoring module directories"

            foreach($m in $module)
            {
                $path = (split-path (Get-ModuleFolder $m) -parent) + "\PrtgAPI_bak"

                # Check if we haven't already renamed the folder as part of a previous module
                if(Test-Path $path)
                {
                    Rename-Item $path "PrtgAPI"
                }
            }

            Write-LogInfo "`t`t`t`t`tRestoring module info files"

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
    $path = $module.Path -replace "PrtgAPI.psd1",""

    $versionFolder = "$($module.Version)\"

    if($path.EndsWith($versionFolder))
    {
        $path = $path.Substring(0, $path.Length - $versionFolder.Length)
    }

    return $path
}

#endregion
#region Redist

function Test-RedistributablePackage($config)
{
    Write-LogInfo "`t`tProcessing Redistributable package"

    $zipPath = Join-Path (PackageManager -RepoLocation) "PrtgAPI.zip"

    Extract-Package (gi $zipPath) {

        param($extractFolder)

        $psd1Path = Join-Path $extractFolder "PrtgAPI.psd1"

        Test-Psd1RootModule $config $psd1Path
        Test-RedistributablePackageContents $config $extractFolder
        Test-RedistributableModuleInstalls $config $extractFolder
    }
}

function Test-RedistributablePackageContents($config, $extractFolder)
{
    $required = @(
        "Functions\New-Credential.ps1"
        "about_ChannelSettings.help.txt"
        "about_ObjectSettings.help.txt"
        "about_PrtgAPI.help.txt"
        "about_SensorParameters.help.txt"
        "about_SensorSettings.help.txt"
        "PrtgAPI.cmd"
        "PrtgAPI.Format.ps1xml"
        "PrtgAPI.Types.ps1xml"
        "PrtgAPI.psd1"
        "PrtgAPI.psm1"
        "PrtgAPI.sh"
    )

    if($config.IsCore)
    {
        if($config.Configuration -eq "Release")
        {
            # Add all the .NET Core Release specific files

            $required += @(
                "fullclr\PrtgAPI.dll"
                "fullclr\PrtgAPI.pdb"
                "fullclr\PrtgAPI.xml"

                "fullclr\PrtgAPI.PowerShell.dll"
                "fullclr\PrtgAPI.PowerShell.pdb"
                "fullclr\PrtgAPI.PowerShell.xml"

                "coreclr\PrtgAPI.deps.json"
                "coreclr\PrtgAPI.dll"
                "coreclr\PrtgAPI.pdb"
                "coreclr\PrtgAPI.xml"

                "coreclr\PrtgAPI.PowerShell.deps.json"
                "coreclr\PrtgAPI.PowerShell.dll"
                "coreclr\PrtgAPI.PowerShell.pdb"
                "coreclr\PrtgAPI.PowerShell.xml"
            )
        }
        else
        {
            # Add all the .NET Core Debug specific files

            $required += @(
                "PrtgAPI.PowerShell.deps.json"
            )
        }
    }

    if($config.Configuration -eq "Debug" -or !$config.IsCore)
    {
        # Add all files common to .NET Core/.NET Framework debug builds

        $required += @(
            "PrtgAPI.dll"
            "PrtgAPI.pdb"
            "PrtgAPI.xml"
            
            "PrtgAPI.PowerShell.dll"
            "PrtgAPI.PowerShell.pdb"
            "PrtgAPI.PowerShell.xml"
        )
    }

    $debugVersion = Get-DebugTargetFramework

    if($debugVersion -like "net4*" -or $config.Configuration -eq "Release" -or !$config.IsCore)
    {
        $required += @(
            "PrtgAPI.PowerShell.dll-Help.xml"
        )
    }

    Test-PackageContents $extractFolder $required
}

function Test-RedistributableModuleInstalls($config, $extractFolder)
{
    if($config.IsCore)
    {
        Test-PowerShellPackageInstallsInternal "Desktop" $extractFolder
        Test-PowerShellPackageInstallsInternal "Core" $extractFolder
    }
    else
    {
        Test-PowerShellPackageInstallsInternal "Desktop" $extractFolder
    }
}

#endregion

function Move-AppveyorPackages($config, $suffix)
{
   if($env:APPVEYOR)
   {
        Write-LogInfo "`t`t`tMoving Appveyor artifacts"
        
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
    gci -recurse (PackageManager -RepoLocation)|remove-item -Recurse -Force
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

function Test-PackageContents($folder, $required)
{
    Write-LogInfo "`t`t`tValidating package contents"

    $pathWithoutTrailingSlash = $folder.TrimEnd("\", "/")

    $existing = gci $folder -Recurse|foreach {
        [PSCustomObject]@{
            Name = $_.fullname.substring($pathWithoutTrailingSlash.length + 1)
            IsFolder = $_.PSIsContainer
        }
    }

    $found = @()
    $illegal = @()

    foreach($item in $existing)
    {
        if($item.IsFolder)
        {
            # Do we have a folder that contains a wildcard that matches this folder? (e.g. packages\* covers packages\foo)
            $match = $required | where { $item.Name -like $_ }

            if(!$match)
            {
                # There isn't a wildcard that covers this folder, but if there are actually any items contained under this folder
                # then transitively this folder is allowed

                $match = $required | where { $_ -like "$($item.Name)\*" }

                # If there is a match, we don't care - we don't whitelist empty folders, so we'll leave it up to the file processing block
                # to decide whether the required files have been found or not
                if(!$match)
                {
                    $illegal += $item.Name
                }
            }
            else
            {
                # Add our wildcard folder (e.g. packages\*)
                $found += $match
            }
        }
        else
        {
            # If there isnt a required item that case insensitively matches a file that appears
            # to exist, then that file must be "extra" and is therefore considered illegal
            $match = $required | where { $_ -eq $item.Name }

            if(!$match)
            {
                # We don't have a direct matchm however maybe we have a folder that contains a wildcard
                # that matches this file (e.g. packages\* covers packages\foo.txt)
                $match = $required | where { $item.Name -like $_ }
            }

            if(!$match)
            {
                $illegal += $item.Name
            }
            else
            {
                $found += $match
            }
        }
    }

    if($illegal)
    {
        $str = ($illegal | Sort-Object | foreach { "'$_'" }) -join "`n"
        throw "Package contained illegal items:`n$str"
    }

    $missing = $required | where { $_ -notin $found }

    if($missing)
    {
        $str = ($missing | Sort-Object | foreach { "'$_'" }) -join "`n"
        throw "Package is missing required items:`n$str"
    }
}