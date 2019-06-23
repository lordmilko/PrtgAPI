function Clear-AppveyorBuild
{
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [switch]$IsCore = $script:APPEYOR_BUILD_CORE,

        [switch]$NuGetOnly
    )

    Write-LogHeader "Cleaning Appveyor build folder (Core: $IsCore)"

    if(!$NuGetOnly)
    {
        if($IsCore)
        {
            throw ".NET Core is not currently supported"
        }
        else
        {
            Invoke-Process { & (Get-MSBuild) /t:clean $env:APPVEYOR_BUILD_FOLDER\PrtgAPI.sln }
        }
    }

    $nupkgs = gci $env:APPVEYOR_BUILD_FOLDER -Recurse -Filter *.nupkg | where {
        !$_.FullName.StartsWith((Join-Path $env:APPVEYOR_BUILD_FOLDER "packages"))
    }
    
    $nupkgs | foreach {
        Write-LogError "`tRemoving $($_.FullName)"

        $_ | Remove-Item -Force
    }
}