function Simulate-Travis
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $false)]
        [string]$Configuration = "Debug"
    )

    InitializeEnvironment $Configuration

    Clear-TravisBuild

    Invoke-TravisInstall
    Invoke-TravisScript
}

function InitializeEnvironment($configuration)
{
    $env:CONFIGURATION = $configuration
    $env:TRAVIS_BUILD_DIR = $script:SolutionDir
}