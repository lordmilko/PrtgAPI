function Clear-TravisBuild
{
    [CmdletBinding()]
    param(
        [switch]$NuGetOnly
    )

    $clearArgs = @{
        BuildFolder = $env:TRAVIS_BUILD_DIR
        Configuration = $env:CONFIGURATION
        IsCore = $true
        NuGetOnly = $NuGetOnly
    }

    Clear-CIBuild @clearArgs
}