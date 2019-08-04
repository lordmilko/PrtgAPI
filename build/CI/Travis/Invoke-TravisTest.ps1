function Invoke-TravisTest
{
    Write-LogHeader "Executing tests"

    $additionalArgs = $null

    if($PSEdition -eq "Core" -and $IsWindows)
    {
        if(!(gmo pester))
        {
            ipmo pester
        }

        if((gmo pester).Version.Major -lt 4)
        {
            $additionalArgs = @{ExcludeTag = "Build"}
        }
    }

    $result = Invoke-CIPowerShellTest $env:TRAVIS_BUILD_DIR $additionalArgs -IsCore:$true

    if($result.FailedCount -gt 0)
    {
        throw "$($result.FailedCount) Pester tests failed"
    }

    $csharpArgs = @(
        "--filter"
        "TestCategory!=SkipCI"
    )

    Invoke-CICSharpTest $env:TRAVIS_BUILD_DIR $csharpArgs $env:CONFIGURATION -IsCore:$true
}