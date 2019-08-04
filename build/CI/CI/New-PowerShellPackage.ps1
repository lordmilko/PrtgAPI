function New-PowerShellPackage
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputDir,

        [Parameter(Mandatory = $true)]
        $RepoManager,

        [Parameter(Mandatory = $true)]
        [string]$Configuration,

        [Parameter(Mandatory = $true)]
        [switch]$IsCore,

        [Parameter(Mandatory = $true)]
        [switch]$Redist,

        [Parameter(Mandatory = $false)]
        [switch]$PowerShell
    )

    $dll = Join-Path $OutputDir "PrtgAPI.PowerShell.dll"

    if(!(Test-Path $dll))
    {
        throw "Cannot build PowerShell package as PrtgAPI has not been compiled. Could not find file '$dll'."
    }

    if($Configuration -eq "Release" -and $IsCore)
    {
        # When we're building Release, instead of copying the normal folder we copied two folders up so we
        # have both the net452 and netstandard2.0 folders so we can merge them together
        $OutputDir = Join-Path $OutputDir "..\.."
    }

    $RepoManager.WithTempCopy(
        $OutputDir,
        {
            param($tempPath)

            Update-RootModule $tempPath $Configuration $IsCore

            New-RedistributablePackage $tempPath $OutputDir $Configuration $IsCore $Redist

            if($PowerShell)
            {
                $modulePath = $tempPath

                if($IsCore -and $Configuration -eq "Release")
                {
                    $modulePath = Join-Path $tempPath "net452\PrtgAPI"
                }

                New-PowerShellPackageInternal $modulePath
            }            
        }
    )
}

function Update-RootModule($tempPath, $configuration, $isCore)
{
    if($isCore -and $configuration -eq "Release")
    {
        $psd1Path = Join-Path $tempPath "net452\PrtgAPI\PrtgAPI.psd1"

        $contents = gc $psd1Path

        $newContents = $contents | foreach {
            if($_ -eq "RootModule = 'PrtgAPI.PowerShell.dll'")
            {
                return @(
                    "RootModule = if(`$PSEdition -eq 'Core')"
                    "{"
                    "    'coreclr\PrtgAPI.PowerShell.dll'"
                    "}"
                    "else # Desktop"
                    "{"
                    "    'fullclr\PrtgAPI.PowerShell.dll'"
                    "}"
                )
            }
            else
            {
                return $_
            }
        }

        $newContents | Set-Content $psd1Path
    }
}

function New-RedistributablePackage($tempPath, $outputDir, $configuration, $isCore, $redist)
{
    $packageDir = Move-PowerShellAssemblies $tempPath $outputDir $configuration $isCore

    if($redist)
    {
        $packageDir = Join-Path $packageDir "*"

        $destinationPath = Join-Path (PackageManager -RepoLocation) "PrtgAPI.zip"

        if(Test-Path $destinationPath)
        {
            Remove-Item $destinationPath -Force
        }

        try
        {
            $global:ProgressPreference = "SilentlyContinue"

            Compress-Archive $packageDir -DestinationPath $destinationPath
        }
        finally
        {
            $global:ProgressPreference = "Continue"
        }
    }
}

function Move-PowerShellAssemblies($tempPath, $outputDir, $configuration, $isCore)
{
    if($isCore -and $configuration -eq "Release")
    {
        $netstandardPrtgAPI = Join-Path $tempPath "netstandard2.0\PrtgAPI"
        $netframeworkPrtgAPI = Join-Path $tempPath "net452\PrtgAPI"

        $coreclr = Join-Path $netframeworkPrtgAPI "coreclr"
        $fullclr = Join-Path $netframeworkPrtgAPI "fullclr"

        $list = @(
            "*.dll"
            "*.json"
            "*.xml"
            "*.pdb"
        )

        $standardFiles = gci $netstandardPrtgAPI -Include $list -Exclude "*-Help.xml" -Recurse
        $netframeworkFiles = gci $netframeworkPrtgAPI -Include $list -Exclude "*-Help.xml" -Recurse

        if(!(Test-Path $coreclr))
        {
            New-Item $coreclr -ItemType Directory | Out-Null
        }

        if(!(Test-Path $fullclr))
        {
            New-Item $fullclr -ItemType Directory | Out-Null
        }

        $standardFiles | Move-Item -Destination $coreclr
        $netframeworkFiles | Move-Item -Destination $fullclr

        $prtgAPIOutputDir = Join-Path $outputDir "..\..\..\PrtgAPI\bin\Release\netstandard2.0" | Resolve-Path | select -expand path

        $deps = Join-Path $prtgAPIOutputDir "PrtgAPI.deps.json"

        $deps | Copy-Item -Destination $coreclr

        return $netframeworkPrtgAPI
    }

    return $tempPath
}

function New-PowerShellPackageInternal($modulePath)
{
    # Remove any files that are not required in the nupkg

    $list = @(
        "*.cmd"
        "*.pdb"
        "*.sh"
        "*.json"
        "PrtgAPI.xml"
        "PrtgAPI.PowerShell.xml"
    )

    gci $modulePath -Include $list -Recurse | Remove-Item -Force

    Publish-PowerShellPackage $modulePath
}

function Publish-PowerShellPackage($tempPath)
{
    Write-LogInfo "`t`tPublishing module to $(PackageManager -RepoName)"

    $expr = "try { `$global:ProgressPreference = 'SilentlyContinue'; Publish-Module -Path '$tempPath' -Repository $(PackageManager -RepoName) -WarningAction SilentlyContinue } finally { `$global:ProgressPreference = 'Continue' }"

    # PowerShell Core currently has a bug wherein attempting to execute Start-Process -Wait (which is used internally by Publish-Module)
    # doesn't work on Windows 7. Work around this by diverting to Windows PowerShell
    if($PSEdition -eq "Core" -and (Test-IsWindows))
    {
        Write-Verbose "Executing powershell -command '$expr'"
        # Clear the PSModulePath to prevent PowerShell Core specific directories contaminating Publish-Module's inner cmdlet lookups
        $command = "`$env:PSModulePath = '$env:ProgramFiles\WindowsPowerShell\Modules;$env:SystemRoot\system32\WindowsPowerShell\v1.0\Modules'; $expr"

        $bytes = [System.Text.Encoding]::Unicode.GetBytes($command)
        $encodedCommand = [Convert]::ToBase64String($bytes)

        Invoke-Expression "powershell -EncodedCommand $encodedCommand"
    }
    else
    {
        $expr = $expr -replace "Publish-Module","Publish-ModuleEx"

        Write-Verbose "Executing '$expr'"
        Invoke-Expression $expr
    }
}

function Publish-ModuleEx
{
    [CmdletBinding()]
    param(
        [string]$Path,
        [string]$Repository
    )

    Get-CallerPreference $PSCmdlet $ExecutionContext.SessionState

    Publish-Module -Path $Path -Repository $Repository -WarningAction $WarningPreference
}