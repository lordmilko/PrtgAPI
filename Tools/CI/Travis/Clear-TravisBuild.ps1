function Clear-TravisBuild
{
    [CmdletBinding()]
    param(
        [switch]$NuGetOnly
    )

    Clear-CIBuild $env:TRAVIS_BUILD_DIR -IsCore:$true -NuGetOnly:$NuGetOnly
}