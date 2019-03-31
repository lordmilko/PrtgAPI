Import-Module $PSScriptRoot\..\Build.psm1
Import-Module $PSScriptRoot\..\Appveyor.psm1 -DisableNameChecking

#region Support

function ReplaceConfig($script)
{
    $file = Get-AppveyorLocalConfigPath

    $existed = $false

    if(Test-Path $file)
    {
        $existed = $true
        $suffix = GetSuffix $file
        Rename-Item $file "$($file)$suffix" -Force
    }

    try
    {
        & $script
    }
    finally
    {
        if($existed)
        {
            if(Test-Path $file)
            {
                Remove-Item $file -Force # Remove the config that was created by this test
            }

            Rename-Item "$($file)$suffix" $file -Force
        }
    }
}

function GetSuffix($file)
{
    $suffix = "_bak"

    if(!(Test-Path "$($file)_bak"))
    {
        return $suffix
    }

    $i = 0

    while($true)
    {
        $suffix = "_bak$i"

        if(!(Test-Path "$($file)$suffix"))
        {
            return $suffix
        }

        $i++
    }
}

function CreateLocalConfig($protect = $true)
{
    $script = {
        { Get-AppveyorLocalConfig } | Should Throw "appveyor.local.json does not exist. Template has been created"

        { Get-AppveyorLocalConfig } | Should Throw "Property 'APPVEYOR_NUGET_API_KEY' of appveyor.local.json must be specified"
    }

    if($protect)
    {
        ReplaceConfig $script
    }
    else
    {
        & $script
    } 
}

function It
{
    [CmdletBinding(DefaultParameterSetName = 'Normal')]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$name,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock] $script,

        [Parameter(Mandatory = $false)]
        [System.Collections.IDictionary[]] $TestCases
    )

    Write-LogInfo "Processing test '$name'"

    Pester\It $name {
        try
        {
            & $script
        }
        catch
        {
            Write-LogError "Error: $($_.Exception.Message)"

            throw
        }
    }
}

function WithoutTestDrive($script)
{
    $drive = Get-PSDrive TestDrive -Scope Global

    $drive | Remove-PSDrive -Force
    Remove-Variable $drive.Name -Scope Global -Force

    try
    {
        & $script
    }
    finally
    {
        New-PSDrive $drive.Name -PSProvider $drive.Provider -Root $drive.Root -Scope Global
        New-Variable $drive.Name -Scope Global -Value $drive.Root
    }
}

function global:Mock-Version
{
    [CmdletBinding()]
    param(
        [ValidateNotNull()]
        $Assembly,

        $LastBuild,

        $LastRelease
    )

    Mock Get-PrtgVersion {
        $Assembly
    }.GetNewClosure()

    Mock Get-LastAppveyorBuild {
        $LastBuild
    }.GetNewClosure()

    Mock Get-LastAppveyorNuGetVersion {
        $LastRelease
    }.GetNewClosure()
}

function Simulate-Build
{
    [CmdletBinding()]
    param(
        [ValidateNotNull()]
        [Parameter(Mandatory = $true)]
        $Assembly,

        [Parameter(Mandatory = $false)]
        $LastBuild,

        [Parameter(Mandatory = $false)]
        $LastRelease,

        [ValidateNotNull()]
        [Parameter(Mandatory = $true)]
        $Expected
    )

    $global:simulateBuildArgs = $PSBoundParameters    

    InModuleScope Appveyor {

        $simulateBuildArgs = $global:simulateBuildArgs

        Simulate-Environment {
            Mock-Version -Assembly $simulateBuildArgs.Assembly -LastBuild $simulateBuildArgs.LastBuild -LastRelease $simulateBuildArgs.LastRelease

            $result = Get-AppveyorVersion

            $result | Should Be $simulateBuildArgs.Expected
        }
    }
}

#endregion

Describe "Appveyor" {
    It "simulates Appveyor" {

        WithoutTestDrive {
            Simulate-Appveyor $false
        }
    }

    Context "Version" {

        It "Release 0.1 -> Commit (p1) = Reset Counter = 0.1.1-preview.1" {

            $buildArgs = @{
                Assembly = "0.1.0"
                LastBuild = "0.1.0"
                LastRelease = "0.1.0"
                Expected = "0.1.1-preview.1"
            }

            Simulate-Build @buildArgs
        }

        It "Release 0.1 -> Commit (p1) -> Commit (p2) = 0.1.1-preview.2" {
            $buildArgs = @{
                Assembly = "0.1.0"
                LastBuild = "0.1.1-preview.1"
                LastRelease = "0.1.0"
                Expected = "0.1.1-preview.2"
            }

            Simulate-Build @buildArgs
        }

        It "Release 0.1 -> Commit (p1) -> Commit (p2) -> Set 0.1.1 = 0.1.1" {

            $buildArgs = @{
                Assembly = "0.1.1"
                LastBuild = "0.1.1-preview.2"
                LastRelease = "0.1.0"
                Expected = "0.1.1"
            }

            Simulate-Build @buildArgs
        }

        It "Release 0.1 -> Commit (p1) -> Commit (p2) -> Set 0.1.1 -> Commit (u1) = Reset Counter + 0.1.1-build.1" {
            $buildArgs = @{
                Assembly = "0.1.1"
                LastBuild = "0.1.1"
                LastRelease = "0.1.0"
                Expected = "0.1.1-build.1"
            }

            Simulate-Build @buildArgs
        }

        It "Release 0.1 -> Commit (p1) -> Commit (p2) -> Set/Release 0.1.1 -> Commit (p1) = Reset Counter + 0.1.2-preview.{build}" {
            $buildArgs = @{
                Assembly = "0.1.1"
                LastBuild = "0.1.1"
                LastRelease = "0.1.1"
                Expected = "0.1.2-preview.1"
            }

            Simulate-Build @buildArgs
        }

        It "Release 0.1 -> Release 0.2 = 0.2" {
            $buildArgs = @{
                Assembly = "0.2.0"
                LastBuild = "0.1.0"
                LastRelease = "0.1.0"
                Expected = "0.2.0"
            }

            Simulate-Build @buildArgs
        }

        It "Release 0.1 (u1) -> Release 0.2 = 0.2" {
            $buildArgs = @{
                Assembly = "0.2.0"
                LastBuild = "0.1.0-build.1"
                LastRelease = "0.1.0-build.1"
                Expected = "0.2.0"
            }

            Simulate-Build @buildArgs
        }

        It "First Build" {
            $buildArgs = @{
                Assembly = "0.1.0"
                LastBuild = $null
                LastRelease = $null
                Expected = "0.1.0"
            }

            Simulate-Build @buildArgs
        }

        It "First Release, Second Build" {
            $buildArgs = @{
                Assembly = "0.1.0"
                LastBuild = "0.1.0"
                LastRelease = $null
                Expected = "0.1.0-build.1"
            }

            Simulate-Build @buildArgs
        }
    }

    <#It "temporarily replaces an existing Appveyor config" {

        ReplaceConfig {
            CreateLocalConfig $false

            $config = gc $file | ConvertFrom-Json
            $config.APPVEYOR_NUGET_API_KEY = "test"
            $config | ConvertTo-Json | Set-Content $file

            ReplaceConfig {
                CreateLocalConfig $false

                $config = gc $file | ConvertFrom-Json
                $config.APPVEYOR_NUGET_API_KEY = "test1"
                $config | ConvertTo-Json | Set-Content $file

                $newConfig = Get-AppveyorLocalConfig

                $newConfig.APPVEYOR_NUGET_API_KEY | Should Be "test1"
                $env:APPVEYOR_NUGET_API_KEY | Should Be "test1"
            }

            $newConfig = Get-AppveyorLocalConfig

            $newConfig.APPVEYOR_NUGET_API_KEY | Should Be "test"
            $env:APPVEYOR_NUGET_API_KEY | Should Be "test"
        }
    }

    It "ensures a local Appveyor config exists" {

        CreateLocalConfig
    }

    It "creates environment variables from a local Appveyor config" {

        ReplaceConfig {
            CreateLocalConfig $false

            $file = Get-AppveyorLocalConfigPath

            $config = gc $file | ConvertFrom-Json

            $config.APPVEYOR_NUGET_API_KEY = "test"

            $config | ConvertTo-Json | Set-Content $file

            $newConfig = Get-AppveyorLocalConfig

            $newConfig.APPVEYOR_NUGET_API_KEY | Should Be "test"
            $env:APPVEYOR_NUGET_API_KEY | Should Be "test"
        }
    }#>
}