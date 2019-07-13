#region Get-AppveyorVersion

function Get-AppveyorVersion($IsCore)
{
    $assemblyVersion = GetVersion $IsCore
    $lastBuild = Get-LastAppveyorBuild
    $lastRelease = Get-LastAppveyorNuGetVersion

    Write-LogVerbose "    Assembly version: $assemblyVersion"
    Write-LogVerbose "    Last build: $lastBuild"
    Write-LogVerbose "    Last release: $lastRelease"

    if(IsPreview $assemblyVersion $lastRelease)
    {
        $build = $env:APPVEYOR_BUILD_NUMBER

        if(IsFirstPreview $lastBuild)
        {
            $build = 1
        }
        else
        {
            $build = IncrementBuild $lastBuild
        }

        [Version]$v = $assemblyVersion

        $result = "$($v.Major).$($v.Minor).$($v.Build + 1)-preview.$build"
    }
    elseif(IsPreRelease $assemblyVersion $lastBuild $lastRelease)
    {
        $build = $env:APPVEYOR_BUILD_NUMBER

        if(IsFirstPreRelease $lastBuild)
        {
            $build = 1
        }
        else
        {
            $build = IncrementBuild $lastBuild
        }

        $result = "$assemblyVersion-build.$build"
    }
    elseif(IsFullRelease $assemblyVersion $lastRelease)
    {
        $result = $assemblyVersion
    }
    else
    {
        throw "Failed to determine the type of build"
    }

    return $result
}

function IncrementBuild($version)
{
    ([int]($version -replace ".+-.+\.(.+)",'$1')) + 1
}

function IsPreview($assemblyVersion, $lastRelease)
{
    # If this DLL has the same version as the last RELEASE, this should be a preview release
    return $assemblyVersion -eq $lastRelease
}

function IsFirstPreview($lastBuild)
{
    return !$lastBuild.Contains("preview")
}

function IsFullRelease($assemblyVersion, $lastRelease)
{
    if([string]::IsNullOrEmpty($lastRelease))
    {
        return $true
    }

    return ([Version]$assemblyVersion) -gt (CleanVersion $lastRelease)
}

function IsPreRelease($assemblyVersion, $lastBuild, $lastRelease)
{
    if([string]::IsNullOrEmpty(($lastBuild)))
    {
        return $false
    }

    if($lastBuild.Contains("preview"))
    {
        return $false
    }

    [Version]$assemblyVersion = $assemblyVersion

    if([string]::IsNullOrEmpty($lastRelease) -or $assemblyVersion -gt (CleanVersion $lastRelease))
    {
        $lastBuildClean = CleanVersion $lastBuild

        if($assemblyVersion -eq $lastBuildClean)
        {
            # We're the same assembly version as the last build which hasn't
            # been released yet. Therefore we are a pre-release
            return $true
        }
    }

    return $false
}

function CleanVersion($version)
{
    return [Version]($version -replace "-build.+","")
}

function IsFirstPreRelease($lastBuild)
{
    return !$lastBuild.Contains("build")
}

function Get-LastAppveyorBuild
{
    if($env:APPVEYOR)
    {
        $history = Invoke-AppveyorRequest "history?recordsNumber=10"

        $version = ($history.builds|where version -NotLike "Build*" | select -first 1).version

        return $version
    }
    else
    {
        return $null
    }
}

function Get-LastAppveyorNuGetVersion
{
    if($env:APPVEYOR)
    {
        $deployments = Get-AppveyorDeployment

        $lastNuGet = $deployments|Sort-Object datetime -Descending|where Name -eq "NuGet"|select -first 1

        return $lastNuGet.Version
    }
    else
    {
        return $null
    }
}

function Get-AppveyorDeployment
{
    $response = Invoke-AppveyorRequest "deployments"
    
    $deployments = @()

    foreach($d in $response.deployments)
    {
        $deployments += [PSCustomObject]@{
            DateTIme = [DateTime]$d.started
            Version = $d.build.version
            Name = $d.environment.name -replace "(.+?)( .+)",'$1'
        }
    }

    return $deployments
}

function Reset-BuildVersion
{
    # We are 1, so the next one will be 2
    Invoke-AppveyorAction "settings/build-number" @{ nextBuildNumber = 2 }
}

function Invoke-AppveyorRequest
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        $Query = $null
    )

    return Invoke-AppveyorRequestInternal $Query Get
}

function Invoke-AppveyorAction
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        $Query = $null,

        [Parameter(Mandatory = $false, Position = 1)]
        $Body = $null
    )

    $result = Invoke-AppveyorRequestInternal $Query "Put" $Body
}

function Invoke-AppveyorRequestInternal
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        $Query = $null,

        [Parameter(Mandatory = $false, Position = 1)]
        $Method,

        [Parameter(Mandatory = $false, Position = 2)]
        $Body
    )

    if(!$env:APPVEYOR_API_TOKEN)
    {
        throw "Cannot invoke Appveyor API: environment variable APPVEYOR_API_TOKEN is not defined"
    }

    $projectURI = "https://ci.appveyor.com/api/projects/$env:APPVEYOR_ACCOUNT_NAME/$env:APPVEYOR_PROJECT_SLUG"

    if($Query)
    {
        $resourceURI = "$projectURI/$Query"
    }
    else
    {
        $resourceURI = $projectURI
    }

    $headers = @{
        "Content-type" = "application/json"
        "Authorization" = "Bearer $env:APPVEYOR_API_TOKEN"
    }

    $restArgs = @{
        Method = $Method
        Uri = $resourceURI
        Headers = $headers
    }

    if($Body)
    {
        $restArgs.Body = $Body | ConvertTo-Json
    }

    $result = Invoke-RestMethod @restArgs

    return $result
}

#endregion
#region Set-AppveyorVersion

function Set-AppveyorVersion
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        [string]$BuildFolder = $env:APPVEYOR_BUILD_FOLDER,

         [Parameter(Mandatory = $false)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE
    )

    try
    {
        Write-LogInfo "Calculating version"
        $version = Get-AppveyorVersion $IsCore

        Write-LogInfo "Setting AppVeyor build to version '$version'"

        if($env:APPVEYOR)
        {
            Update-AppVeyorBuild -Version $version
        }
        else
        {
            $env:APPVEYOR_BUILD_VERSION = $version
        }
    }
    catch
    {
        if(!$psISE)
        {
            $host.SetShouldExit(1)
        }
        
        throw
    }
}

#endregion