function Simulate-Travis
{
    InitializeEnvironment

    Clear-TravisBuild

    Invoke-TravisInstall
    Invoke-TravisScript
}

function InitializeEnvironment
{
    $env:CONFIGURATION = "Debug"
    $env:TRAVIS_BUILD_DIR = $script:SolutionDir
}