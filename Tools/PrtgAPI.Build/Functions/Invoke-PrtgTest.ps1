<#
.SYNOPSIS
Executes tests on a PrtgAPI build.

.DESCRIPTION
The Invoke-PrtgTest cmdlet executes tests on previously generated builds of PrtgAPI. By default both C# and PowerShell tests will be executed against the last Debug build. Tests can be limited to a specific platform by specifying a value to the -Type parameter, and can also be limited to those whose name matches a specified wildcard expression via the -Name parameter.

Tests executed by Invoke-PrtgTest are automatically logged in the TRX format (C#) and NUnitXml format (PowerShell) under the PrtgAPI.Tests.UnitTests\TestResults folder of the PrtgAPI solution. Test results in this directory can be evaluated and filtered after the fact using the Get-PrtgTestResult cmdlet. Note that upon compiling a new build of PrtgAPI.Tests.UnitTests, all items in this test results folder will automatically be deleted.

.PARAMETER Name
Wildcard used to specify tests to execute. If no value is specified, all tests will be executed.

.PARAMETER Type
Type of tests to execute. If no value is specified, both C# and PowerShell tests will be executed.

.PARAMETER Legacy
Specifies whether to test PrtgAPI using the .NET Core CLI or legacy .NET Framework tooling.

.PARAMETER Configuration
Build configuration to test. If no value is specified, the last Debug build will be tested.

.PARAMETER Integration
Specifies to run integration tests instead of unit tests.

.PARAMETER Tag
Specifies tags or test categories to execute. If a Name is specified as well, these two categories will be filtered using logical AND.

.PARAMETER Build
Specifies that PrtgAPI.Build tests should be included. If -Tag contains "Build" this parameter will also be activated.

.EXAMPLE
C:\> Invoke-PrtgTest
Executes all unit tests on the last PrtgAPI build.

.EXAMPLE
C:\> Invoke-PrtgTest *dynamic*
Executes all tests whose name contains the word "dynamic".

.EXAMPLE
C:\> Invoke-PrtgTest -Type PowerShell
Executes all PowerShell tests only.

.EXAMPLE
C:\> Invoke-PrtgTest -Configuration Release
Executes tests on the Release build of PrtgAPI.

C:\> Invoke-PrtgTest -Integration
Invoke all integration tests on the last PrtgAPI build.

.LINK
Invoke-PrtgBuild
Get-PrtgTestResult
#>
function Invoke-PrtgTest
{
    [CmdletBinding()]
    param(
        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory = $false, Position = 0)]
        [string[]]$Name,

        [Parameter(Mandatory = $false)]
        [ValidateSet('C#', 'PowerShell')]
        [string[]]$Type,

        [ValidateScript({
            if($_ -and !(Test-IsWindows)) {
                throw "Parameter is only supported on Windows."
            }
            return $true
        })]
        [Parameter(Mandatory = $false)]
        [switch]$Legacy,

        [Parameter(Mandatory = $false)]
        [Configuration]$Configuration = "Debug",

        [Parameter(Mandatory = $false)]
        [switch]$Integration,

        [ValidateNotNullOrEmpty()]
        [Parameter(Mandatory = $false)]
        [string[]]$Tag,

        [Parameter(Mandatory = $false)]
        [switch]$Build
    )

    $testArgs = @{
        Name = $Name
        Type = $Type
        IsCore = -not $Legacy
        Configuration = $Configuration
        Integration = $Integration
        Tags = $Tag
        Build = $build
    }

    InvokeCSharpTest @testArgs
    InvokePowerShellTest @testArgs
}

function InvokeCSharpTest($name, $type, $isCore, $configuration, $integration, $tags)
{
    if($type | HasType "C#")
    {
        $additionalArgs = @()

        $additionalArgs += GetLoggerArgs $isCore
        $additionalArgs += GetLoggerFilters $name $tags $isCore

        $testArgs = @{
            BuildFolder = Get-SolutionRoot
            AdditionalArgs = $additionalArgs
            Configuration = $configuration
            IsCore = $isCore
            Integration = $integration
        }

        $projectDir = Join-Path (Get-SolutionRoot) (Get-TestProject $isCore $integration).Directory

        try
        {
            # Legacy vstest.console stores the test results in the TestResults folder under the current directory.
            # Change into the project directory whole we execute vstest to ensure the results get stored
            # in the right folder
            Push-Location $projectDir

            Invoke-CICSharpTest @testArgs -Verbose
        }
        finally
        {
            Pop-Location
        }
    }
}

function InvokePowerShellTest($name, $type, $isCore, $configuration, $integration, $tags, $build)
{
    if($type | HasType "PowerShell")
    {
        $projectDir = Join-Path (Get-SolutionRoot) (Get-TestProject $isCore $integration).Directory
        $testResultsDir = Join-Path $projectDir "TestResults"

        if(!(Test-Path $testResultsDir))
        {
            New-Item $testResultsDir -ItemType Directory | Out-Null
        }

        $dateTime = (Get-Date).ToString("yyyy-MM-dd_HH-mm-ss-fff")

        $additionalArgs = @{
            OutputFile = Join-Path $testResultsDir "PrtgAPI_PowerShell_$dateTime.xml"
            OutputFormat = "NUnitXml"
        }

        if($name -ne $null)
        {
            $additionalArgs.TestName = $name
        }

        if($null -ne $tags)
        {
            $additionalArgs.Tag = $tags
        }

        if(!$build -and $tag -notcontains "Build")
        {
            $additionalArgs.ExcludeTag = "Build"
        }

        $testArgs = @{
            BuildFolder = Get-SolutionRoot
            AdditionalArgs = $additionalArgs
            IsCore = $isCore
            Integration = $integration
        }

        Invoke-CIPowerShellTest @testArgs | Out-Null
    }
}

function GetLoggerArgs($IsCore)
{
    $loggerTarget = "trx;LogFileName=PrtgAPI_C#.trx"

    if($IsCore)
    {
        return @(
            "--logger"
            $loggerTarget
        )
    }
    else
    {
        return "/logger:$loggerTarget"
    }
}

function GetLoggerFilters($name, $tags, $IsCore)
{
    $filter = $null

    if($name)
    {
        $filter = ($name | foreach { "FullyQualifiedName~$($_.Trim('*'))" }) -join "|"
    }

    if($tags)
    {
        $tagsFilter = ($tags | foreach { "TestCategory=$($_)" }) -join "|"

        if($filter)
        {
            $filter = "($filter)&($tagsFilter)"
        }
        else
        {
            $filter = $tagsFilter
        }
    }

    if($filter)
    {
        if($IsCore)
        {
            return @(
                "--filter"
                $filter
            )
        }
        else
        {
            return "/TestCaseFilter:$filter"
        }
    }
}