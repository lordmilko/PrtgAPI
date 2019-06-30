function Invoke-TravisTest
{
    Write-LogHeader "Executing tests"

    Invoke-CITest $env:TRAVIS_BUILD_DIR | Out-Null
}