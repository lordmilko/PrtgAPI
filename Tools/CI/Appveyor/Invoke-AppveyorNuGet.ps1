$ProgressPreference = "SilentlyContinue"
$ErrorActionPreference = "Stop"

$repoName = "TempRepository"
$repoLocation = "$env:TEMP\$repoName"
$packageLocation = "$env:TEMP\TempPackages"

function Invoke-AppveyorNuGet
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
        PowerShellOutputDir   = GetPowerShellOutputDir $IsCore
        IsCore = $IsCore
    }

    Process-CSharpPackage $config
    Process-PowerShellPackage $config
}

function GetPowerShellOutputDir($IsCore)
{
    $base = "$env:APPVEYOR_BUILD_FOLDER\PrtgAPI.PowerShell\bin\$env:CONFIGURATION\"

    if($IsCore)
    {
        # Get the lowest .NET Framework folder
        $candidates = gci "$base\net4*"

        $fullName = $candidates | select -First 1 -Expand FullName

        return "$fullName\PrtgAPI"
    }
    else
    {
        return "$base\PrtgAPI"
    }
}

#region PackageSource

function Install-CSharpPackageSource
{
    $packageArgs = @(
        "CSharp"
        { Get-PackageSource }
        { Register-PackageSource -Name $repoName -Location $repoLocation -ProviderName "NuGet" -Trusted }
        { Unregister-PackageSource -Name $repoName -Location $repoLocation -ProviderName "NuGet" -Force -ErrorAction SilentlyContinue }
    )
    
    Install-GenericPackageSource @packageArgs
}

function Uninstall-CSharpPackageSource
{
    Uninstall-GenericPackageSource "CSharp" { Unregister-PackageSource -Name $repoName -Location $repoLocation -ProviderName "NuGet" -Force }
}

#endregion
#region PSRepository

function Install-TempPSRepository
{
    $packageArgs = @(
        "PowerShell",
        { Get-PSRepository },
        { Register-PSRepository -Name $repoName -SourceLocation $repoLocation -PublishLocation $repoLocation -InstallationPolicy Trusted },
        { Unregister-PSRepository $repoName }
    )
    
    Install-GenericPackageSource @packageArgs
}

function Uninstall-TempPSRepository
{
    Uninstall-GenericPackageSource "PowerShell" { Unregister-PSRepository $repoName }
}

#endregion
#region Generic Package Source

function Install-GenericPackageSource($language, $exists, $register, $unregister)
{
    Write-LogInfo "`t`tInstalling temp $language repository"

    if(Test-Path $repoLocation)
    {
        Write-LogError "`t`t`tRemoving repository folder left over from previous run..."

        Remove-Item $repoLocation -Recurse -Force
    }

    Write-LogInfo "`t`t`tCreating repository folder"
    New-Item -ItemType Directory $repoLocation | Out-Null

    if((& $exists) | where name -eq $repoName)
    {
        Write-LogError "`t`t`tRemoving repository left over from previous run..."
        & $unregister
    }

    Write-LogInfo "`t`t`tRegistering temp repository"
    & $register | Out-Null
}

function Uninstall-GenericPackageSource($language, $unregister)
{
    Write-LogInfo "`t`tUninstalling temp $language repository"

    Write-LogInfo "`t`t`tUnregistering temp repository"
    & $unregister

    Write-LogInfo "`t`t`tRemoving temp repository folder"
    Remove-Item $repoLocation -Recurse -Force
}

#endregion
#region C#

function Process-CSharpPackage($config)
{
    Write-LogSubHeader "`tProcessing C# package"

    Install-CSharpPackageSource

    New-CSharpPackage $config
    Test-CSharpPackage $config

    Move-Packages

    Uninstall-CSharpPackageSource
}

function New-CSharpPackage($config)
{
    Write-LogInfo "`t`tBuilding package"

    $result = Invoke-Process {

        if($config.IsCore)
        {
            throw ".NET Core is not currently supported"
        }
        else
        {
            $nugetArgs = @(
                "pack"
                "$($config.CSharpProjectRoot)\PrtgAPI.csproj"
                "-Exclude"
                "**/*.tt;**/Resources/*.txt;*PrtgClient.Methods.xml"
                "-outputdirectory"
                "$repoLocation"
                "-NoPackageAnalysis"
                "-symbols"
                "-version"
            )

            if($env:APPVEYOR)
            {
                $nugetArgs += ($env:APPVEYOR_BUILD_VERSION -replace "-.+")
            }
            else
            {
                $nugetArgs += (Get-PrtgVersion)
            }

            nuget @nugetArgs
        }
    }

    if($result)
    {
        Write-LogInfo $result
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
    $installPath = "$packageLocation\$packageName"

    if(IsNuGetPackageInstalled $installPath)
    {
        Write-LogInfo "`t`t`t`t'$packageName' is already installed. Uninstalling package"
        Uninstall-CSharpPackageInternal
    }

    Install-CSharpPackageInternal
    Test-CSharpPackageInstallInternal
    Uninstall-CSharpPackageInternal
}

function Get-CSharpNupkg
{
    $nupkg = @(gci $repoLocation -Filter *.nupkg|where name -NotLike "*.symbols.nupkg")

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
    return (Get-Package PrtgAPI -Destination $packageLocation -ErrorAction SilentlyContinue) -or (Test-Path $installPath)
}

function Install-CSharpPackageInternal
{
    Write-LogInfo "`t`t`t`tInstalling package from $repoName"

    $result = Install-Package PrtgAPI -Source $repoName -ProviderName NuGet -Destination $packageLocation | Out-Null

    if(!(Test-Path $installPath))
    {
        throw "Package did not install successfully"
    }
}

function Test-CSharpPackageInstallInternal
{
    $folders = gci "$packageLocation\PrtgAPI.$(Get-PrtgVersion)\lib\net4*"

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
    Get-Package PrtgAPI -Provider NuGet -Destination $packageLocation | Uninstall-Package

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

    Install-TempPSRepository

    if($env:APPVEYOR)
    {
        Update-ModuleManifest "$($config.PowerShellOutputDir)\PrtgAPI.psd1"
    }    

    New-PowerShellPackage $config

    Test-PowerShellPackage

    Move-Packages "_PowerShell"

    Uninstall-TempPSRepository
}

function New-PowerShellPackage($config)
{
    WithTempCopy $config.PowerShellOutputDir {
        param($tempPath)

        gci "$($tempPath)\*" -Include *.cmd,*.pdb | Remove-Item -Force

        Write-LogInfo "`t`tPublishing module to $repoName"

        Publish-Module -Path $tempPath -Repository $repoName -WarningAction SilentlyContinue
    }
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
        if(!(Install-Package PrtgAPI -Source $repoName -AllowClobber)) # TShell has a Get-Device cmdlet
        {
            throw "PrtgAPI did not install properly"
        }

        Write-LogInfo "`t`t`t`tTesting Package cmdlets"

        $resultCmdlet =   (powershell -command '&{ import-module PrtgAPI; try { Get-Sensor } catch [exception] { $_.exception.message }}')
        $resultFunction = (powershell -command '&{ import-module PrtgAPI; (New-Credential a b).ToString() }')

        Write-LogInfo "`t`t`t`tUninstalling Package"
    
        if(!(Uninstall-Package PrtgAPI))
        {
            throw "PrtgAPI did not uninstall properly"
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

function Hide-Module($name, $script)
{
    $hidden = $false

    $module = Get-Module $name -ListAvailable

    try
    {
        if($module)
        {
            $hidden = $true

            foreach($m in $module)
            {
                $path = Get-ModuleFolder $m

                # Check if we haven't already renamed the folder as part of a previous module
                if(Test-Path $path)
                {
                    Rename-Item $path "PrtgAPI_bak"
                }
            }
        }

        & $script
    }
    finally
    {
        if($hidden)
        {
            foreach($m in $module)
            {
                $path = (split-path (Get-ModuleFolder $m) -parent) + "\PrtgAPI_bak"

                # Check if we haven't already renamed the folder as part of a previous module
                if(Test-Path $path)
                {
                    Rename-Item $path "PrtgAPI"
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

function WithTempCopy($folderName, $script)
{
    $tempPath = Join-Path $repoLocation "TempOutput\$(Split-Path $folderName -Leaf)"

    Copy-Item -Path $folderName -Destination $tempPath -Recurse -Force

    try
    {
        & $script $tempPath
    }
    finally
    {
        Remove-Item "$repoLocation\TempOutput" -Recurse -Force
    }
}

function Move-Packages($suffix)
{
   if($env:APPVEYOR)
    {
        $pkgs = Get-ChildItem $repoLocation -Filter *.nupkg
        
        foreach($pkg in $pkgs)
        {
            $newName = "$($pkg.BaseName)$suffix$($pkg.Extension)"
            $newPath = "$env:APPVEYOR_BUILD_FOLDER\$newName"

            Write-LogInfo "`t`t`t`tMoving package $($pkg.Name) to $newPath"
            Move-Item $pkg.Fullname $newPath
        }
    }
    else
    {
        Write-LogInfo "`t`t`t`tClearing repo (not running under Appveyor)"
        Clear-Repo
    } 
}

function Clear-Repo
{
    gci -recurse $repoLocation|remove-item -Recurse -Force
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

#todo: for c# we also need to manually compare all the fields in the resulting nuspec vs the nuspecs we've been getting
#in our released nupkgs to make sure all the fields are filled in correctly