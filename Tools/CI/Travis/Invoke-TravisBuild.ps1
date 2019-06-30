function Invoke-TravisBuild
{
    Write-LogHeader "Building PrtgAPI"

    Invoke-CIBuild $env:TRAVIS_BUILD_DIR -IsCore:$true
}