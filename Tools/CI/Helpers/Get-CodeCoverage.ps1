$opencover = "C:\ProgramData\chocolatey\bin\OpenCover.Console.exe"
$vstest = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
$powershellAdapter = "$env:temp\PSToolsExtracted\"
$opencoverOutput = "$env:temp\opencover.xml"
$powershellAdapterDownload = "https://github.com/adamdriscoll/poshtools/releases/download/july-maintenance/PowerShellTools.14.0.vsix"

function Get-CodeCoverage
{
    [CmdletBinding()]
    param(
        [string]$BuildFolder = $env:APPVEYOR_BUILD_FOLDER,
        [string]$Configuration = $env:CONFIGURATION,
        [switch]$TestOnly,
        [switch]$IsCore
    )

    if(!(gcm "opencover.console" -ErrorAction SilentlyContinue))
    {
        Invoke-Process { cinst opencover.portable --confirm --no-progress }
    }
    else
    {
        & $opencover -version
    }

    if(Test-Path $opencoverOutput)
    {
        Remove-Item $opencoverOutput -Force
    }

    Get-PSCodeCoverage $BuildFolder $Configuration -TestOnly:$TestOnly
    Get-CSharpCodeCoverage $BuildFolder $Configuration -TestOnly:$TestOnly -IsCore:$IsCore
}

function Get-CSharpCodeCoverage
{
    [CmdletBinding()]
    param(
        [string]$BuildFolder = $env:APPVEYOR_BUILD_FOLDER,
        [string]$Configuration = $env:CONFIGURATION,
        [switch]$TestOnly,
        [switch]$IsCore
    )

    Write-LogHeader "`tCalculating C# Coverage"

    $vstestParams = "/TestCaseFilter:TestCategory!=SlowCoverage&TestCategory!=SkipCI \`"$BuildFolder\PrtgAPI.Tests.UnitTests\bin\$Configuration\PrtgAPI.Tests.UnitTests.dll\`""

    if($TestOnly)
    {
        if($IsCore)
        {
            throw "TestOnly for IsCore is not implemented"
        }

        # Replace the cmd escaped quotes with PowerShell escaped quotes, and then add an additional quote at the end of the TestCaseFilter to separate the arguments.
        # Trim any quotes from the end of the string, since PowerShell will add its own quote for us
        $vstestParams = ($vstestParams -replace "\\`"","`" `"").Trim("`" `"")

        Write-LogInfo "`t`tExecuting $vstest $vstestParams"
        Invoke-Process { & $vstest $vstestParams } -Host
    }
    else
    {
        if($IsCore)
        {
            throw "Waiting on OpenCover to support portable pdbs"
        }
        else
        {
            $opencoverParams = (Get-OpenCoverParams $vstestParams) + "-mergeoutput"

            Write-LogInfo "`t`tExecuting $opencover $opencoverParams"
            Invoke-Process { & $opencover @opencoverParams } -Host
        }
    }
}

function Get-PSCodeCoverage
{
    [CmdletBinding()]
    param(
        [string]$BuildFolder = $env:APPVEYOR_BUILD_FOLDER,
        [string]$Configuration = $env:CONFIGURATION,
        [Switch]$TestOnly
    )

    Write-LogHeader "`tCalculating PowerShell Coverage"

    $tests = gci $BuildFolder\PrtgAPI.Tests.UnitTests\PowerShell -Recurse -Filter *.Tests.ps1|foreach {"\`"$($_.FullName)\`""}

    if($tests.Count -eq 0)
    {
        throw "Couldn't find any PowerShell tests"
    }
    else
    {
        Write-LogInfo "`t`tFound $($tests.Count) PowerShell tests"
    }

    $testsStr = ($tests -join " ")
    
    if(!(Test-Path "$powershellAdapter\PowerShellTools.TestAdapter.dll"))
    {
        $downloadPath = "$env:temp\PowerShellTools.14.0.vsix"
        $zip = ($downloadPath -replace ".vsix",".zip")

        $localTools = "$BuildFolder\PrtgAPI.Tests.IntegrationTests\Tools\PowerShellTools.14.0.vsix" 

        if(Test-Path $localTools)
        {
            Write-LogInfo "`tCopying PowerShell Tools for Visual Studio from project"
            Copy-Item $localTools $downloadPath
        }
        else
        {
            Write-LogInfo "`tDownloading PowerShell Tools for Visual Studio"
            Invoke-WebRequest $powershellAdapterDownload -OutFile $downloadPath
        }

        Write-LogInfo "`tExtracting PowerShell Tools for Visual Studio"
        Move-Item $downloadPath $zip -Force

        Expand-Archive $zip $powershellAdapter -Force
    }

    $vstestParams = "/TestAdapterPath:$powershellAdapter"

    if($TestOnly)
    {
        $testsStr = ($testsStr -replace "\\`"","`"").Trim("`"")

        $vstestParamsFinal = @(
            "$testsStr"
            $vstestParams
        )

        Invoke-Process { & $vstest ("$testsStr " + "`" `"$vstestParams") } -Host
    }
    else
    {
        $opencoverParams = Get-OpenCoverParams "$vstestParams $testsStr"

        Write-LogInfo "`tExecuting $opencover $opencoverParams"
        Invoke-Process { & $opencover @opencoverParams } -Host
    }
}

function Get-OpenCoverParams($arguments)
{
    $opencoverParams = @(
        "-target:$vstest"
        "-targetargs:$arguments"
        "-output:$opencoverOutput"
        "-register:path32"
        "-filter:+`"[PrtgAPI*]* -[PrtgAPI.Tests*]*`""
        "-excludebyattribute:System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute"
        "-hideskipped:attribute"
    )

    return $opencoverParams
}

function New-CoverageReport
{
    [CmdletBinding()]
    param(
        [string]$Types = "Html",
        [string]$TargetDir = "$env:temp\report"
    )

    Write-LogHeader "Generating a coverage report"

    if(!(gcm "reportgenerator" -ErrorAction SilentlyContinue))
    {
        Invoke-Process { cinst reportgenerator.portable --confirm --no-progress | Out-Null }
    }

    Invoke-Process { & "reportgenerator" -reports:$opencoverOutput -reporttypes:$Types "-targetdir:$TargetDir" | Out-Null }
}

function Get-LineCoverage
{
    New-CoverageReport CsvSummary

    $csv = Import-Csv $env:temp\report\Summary.csv -Delimiter ';' -Header "Property","Value"

    $val = ($csv|where property -eq "Line coverage:"|select -expand value).Trim("%")

    return [double]$val
}

Export-ModuleMember New-CoverageReport,Get-LineCoverage