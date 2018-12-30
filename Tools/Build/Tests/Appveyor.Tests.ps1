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

#endregion

Describe "Appveyor" {
    It "simulates Appveyor" {

        WithoutTestDrive {
            Simulate-Appveyor $false
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