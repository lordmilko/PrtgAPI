function Start-PrtgTest
{
    [CmdletBinding()]
    param (
        [Parameter(Mandatory=$false)]
        [Switch]$Integration = $false,

        [Parameter(Mandatory=$false)]
        [string]$Configuration = "Debug"
    )

    $projectRoot = GetProjectRoot
    $powershellAdapter = GetPowerShellTestAdapter $projectRoot
    $tests = GetTests $projectRoot $Configuration

    ExecuteTests $powershellAdapter $tests
}

function GetPowerShellTestAdapter($projectRoot)
{
    $powershellAdapter = "$env:temp\PSToolsExtracted\"

    if(!(Test-Path "$powershellAdapter\PowerShellTools.TestAdapter.dll"))
    {
        $file = "PowerShellTools.14.0.vsix"

        $vsix = "$projectRoot\Tools\PowerShell\$file"

        $zip = "$env:temp\" + ($file -replace "vsix","zip")

        Copy-Item $vsix $zip -Force

        Expand-Archive $zip $powershellAdapter
    }

    return $powershellAdapter
}

function GetTests($projectRoot, $configuration)
{
    $testProject = "PrtgAPI.Tests.UnitTests"

    if($Integration)
    {
        $testProject = "PrtgAPI.Tests.IntegrationTests"
    }

    $testRoot = "$projectRoot$testProject"

    Write-Host "Executing tests from $testRoot\"

    $testDll = "`"$testRoot\bin\$configuration\$testProject.dll`""

    if($Integration)
    {
        $testDll = "`"$testRoot\bin\$configuration\PrtgAPI.Tests\$testProject.dll`""
    }

    # Get PowerShell tests

    $powershellTests = gci $testRoot -Recurse -Filter *.Tests.ps1|foreach {"`"$($_.FullName)`""}

    if($powershellTests.Count -eq 0)
    {
        throw "Couldn't find any PowerShell tests"
    }
    else
    {
        Write-Host "Found $($powershellTests.Count) PowerShell tests"
    }

    Write-Host "Using Test DLL $testDll"

    $allTests = @($testDll) + $powershellTests

    return $allTests
}

function ExecuteTests($powershellAdapter, $tests)
{
    $vstest = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

    Write-Host -ForegroundColor Cyan "Executing tests"

    & $vstest "/TestAdapterPath:$powershellAdapter" $tests

    # 0 on success, 0 on failure
    Write-Host "Last exit code was $LASTEXITCODE"
}