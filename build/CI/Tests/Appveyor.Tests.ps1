Import-Module $PSScriptRoot\..\ci.psm1 -Scope Local
Import-Module $PSScriptRoot\..\Appveyor.psm1 -DisableNameChecking -Scope Local

$skipBuildModule = $true
. $PSScriptRoot\..\..\..\src\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

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

function global:Mock-Version
{
    [CmdletBinding()]
    param(
        [ValidateNotNull()]
        $Assembly,

        $LastBuild,

        $LastRelease
    )

    Mock Get-CIVersion {
        [PSCustomObject]@{
            File = [Version]$Assembly
        }
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

function TestPackageContents
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [bool]$IsCore,

        [Parameter(Mandatory = $true, Position = 1)]
        $Contents,

        [Parameter(Mandatory = $false, Position = 2)]
        $Configuration = "Debug",

        [Parameter(Mandatory = $false)]
        [ValidateSet("CSharp", "PowerShell", "Redist")]
        [string]$Type = "CSharp"
    )

    $global:prtgPackageContents = $Contents

    if($Type -eq "CSharp")
    {
        Mock "New-AppveyorPackage" {

            param($IsCore)

            $config = [PSCustomObject]@{
                IsCore = $IsCore
                Configuration = $env:CONFIGURATION
            }

            Test-CSharpPackageContents $config "C:\FakeExtractFolder"
        } -Verifiable -ModuleName "Appveyor"
    }
    elseif($Type -eq "PowerShell")
    {
        Mock "New-AppveyorPackage" {

            param($IsCore)

            $config = [PSCustomObject]@{
                IsCore = $IsCore
                Configuration = $env:CONFIGURATION
            }

            Test-PowerShellPackageContents $config "C:\FakeExtractFolder"
        } -Verifiable -ModuleName "Appveyor"
    }
    elseif($Type -eq "Redist")
    {
        Mock "New-AppveyorPackage" {

            param($IsCore)

            $config = [PSCustomObject]@{
                IsCore = $IsCore
                Configuration = $env:CONFIGURATION
            }

            Test-RedistributablePackageContents $config "C:\FakeExtractFolder"
        } -Verifiable -ModuleName "Appveyor"
    }
    else
    {
        throw "Don't know how to handle type '$Type'"
    }

    InModuleScope "Appveyor" {

        Mock "Get-ChildItem" {

            param($Path)

            if($Path -eq "C:\FakeExtractFolder")
            {
                foreach($content in $global:prtgPackageContents)
                {
                    $joined = Join-Path $Path $content.Path

                    if($content.Type -eq "File")
                    {
                        [System.IO.FileInfo]$joined | Add-Member PSIsContainer $false -PassThru
                    }
                    elseif($content.Type -eq "Folder")
                    {
                        [System.IO.DirectoryInfo]$joined | Add-Member PSIsContainer $true -PassThru
                    }
                    else
                    {
                        throw "Unknown type specified in content '$content'"
                    }
                }
            }
            else
            {
                throw "Unknown path '$Path' was specified"
            }            
        } -Verifiable
    }

    try
    {
        Simulate-Environment {

            $env:CONFIGURATION = $Configuration

            Invoke-AppveyorBeforeTest -IsCore:$IsCore
        }
    }
    finally
    {
        $global:prtgPackageContents = $null
    }

    Assert-VerifiableMocks
}

#endregion

Describe "Appveyor" {
    It "simulates Appveyor" {

        WithoutTestDrive {
            Simulate-Appveyor -IsCore
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
    
    Context "C# NuGet" {

        It "has all files for .NET Framework" {
            TestPackageContents $false @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.dll"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.xml"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "lib"}
                @{Type = "Folder"; Path = "lib\net452"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            )
        }

        It "has all files for .NET Core" {

            $target = Get-DebugTargetFramework

            TestPackageContents $true @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "lib\$target\PrtgAPI.dll"}
                @{Type = "File"; Path = "lib\$target\PrtgAPI.xml"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "LICENSE"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "lib"}
                @{Type = "Folder"; Path = "lib\$target"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            )
        }

        It "has all files for .NET Core for Release" {
            TestPackageContents $true @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.dll"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.xml"}
                @{Type = "File"; Path = "lib\netstandard2.0\PrtgAPI.dll"}
                @{Type = "File"; Path = "lib\netstandard2.0\PrtgAPI.xml"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "LICENSE"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "lib"}
                @{Type = "Folder"; Path = "lib\net452"}
                @{Type = "Folder"; Path = "lib\netstandard2.0"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            ) "Release"
        }

        It "has no contents" {

            $missing = @(
                "'[Content_Types].xml'"
                "'_rels\*'"
                "'lib\net452\PrtgAPI.dll'"
                "'lib\net452\PrtgAPI.xml'"
                "'package\*'"
                "'PrtgAPI.nuspec'"
            )

            { TestPackageContents $false @() } | Should Throw "Package is missing required items:`n$($missing -join "`n")"
        }

        It "is missing one file" {
            { TestPackageContents $false @(
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.dll"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.xml"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "lib"}
                @{Type = "Folder"; Path = "lib\net452"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            ) } | Should Throw "Package is missing required items:`n'[Content_Types].xml'"
        }

        It "is missing a wildcard folder" {
            { TestPackageContents $false @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.dll"}
                @{Type = "File"; Path = "lib\net452\PrtgAPI.xml"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "lib"}
                @{Type = "Folder"; Path = "lib\net452"}
            ) } | Should Throw "Package is missing required items:`n'package\*'"
        }
    }

    Context "PowerShell NuGet" {

        It "has all files for .NET Framework" {
            TestPackageContents $false @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "PrtgAPI.dll"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "Functions"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            ) -Type PowerShell
        }

        It "has all files for .NET Core for Release" {
            TestPackageContents $true @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.dll"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.dll"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "Functions"}
                @{Type = "Folder"; Path = "fullclr"}
                @{Type = "Folder"; Path = "coreclr"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            ) "Release" -Type PowerShell
        }

        It "has no contents" {

            $missing = @(
                "'[Content_Types].xml'"
                "'_rels\*'"
                "'about_ChannelSettings.help.txt'"
                "'about_ObjectSettings.help.txt'"
                "'about_PrtgAPI.help.txt'"
                "'about_SensorParameters.help.txt'"
                "'about_SensorSettings.help.txt'"
                "'Functions\New-Credential.ps1'"
                "'package\*'"
                "'PrtgAPI.dll'"
                "'PrtgAPI.Format.ps1xml'"
                "'PrtgAPI.nuspec'"
                "'PrtgAPI.PowerShell.dll'"
                "'PrtgAPI.PowerShell.dll-Help.xml'"
                "'PrtgAPI.psd1'"
                "'PrtgAPI.psm1'"
            )

            { TestPackageContents $false @() -Type PowerShell } | Should Throw "Package is missing required items:`n$($missing -join "`n")"
        }

        It "is missing one file" {
            { TestPackageContents $false @(
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "PrtgAPI.dll"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "package\foo\bar.txt"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "Functions"}
                @{Type = "Folder"; Path = "package"}
                @{Type = "Folder"; Path = "package\foo"}
            ) -Type PowerShell } | Should Throw "Package is missing required items:`n'[Content_Types].xml'"
        }

        It "is missing a wildcard folder" {
            { TestPackageContents $false @(
                @{Type = "File"; Path = "[Content_Types].xml"}
                @{Type = "File"; Path = "_rels\blah.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "PrtgAPI.dll"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.nuspec"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}   
                @{Type = "Folder"; Path = "_rels"}
                @{Type = "Folder"; Path = "Functions"}
            ) -Type PowerShell } | Should Throw "Package is missing required items:`n'package\*'"
        }
    }

    Context "Redistributable" {
        It "has all files for .NET Framework for Debug" {
            TestPackageContents $false @(
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "PrtgAPI.cmd"}
                @{Type = "File"; Path = "PrtgAPI.dll"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.pdb"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.pdb"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "File"; Path = "PrtgAPI.sh"}
                @{Type = "File"; Path = "PrtgAPI.xml"}
                @{Type = "Folder"; Path = "Functions"}
            ) -Type Redist
        }

        It "has all files for .NET Framework for Release" {
            TestPackageContents $false @(
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "PrtgAPI.cmd"}
                @{Type = "File"; Path = "PrtgAPI.dll"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.pdb"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.pdb"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "File"; Path = "PrtgAPI.sh"}
                @{Type = "File"; Path = "PrtgAPI.xml"}
                @{Type = "Folder"; Path = "Functions"}
            ) "Release" -Type Redist
        }

        It "has all files for .NET Core for Debug" {
            TestPackageContents $true @(
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "PrtgAPI.cmd"}
                @{Type = "File"; Path = "PrtgAPI.dll"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.pdb"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.deps.json"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.pdb"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "File"; Path = "PrtgAPI.sh"}
                @{Type = "File"; Path = "PrtgAPI.xml"}
                @{Type = "Folder"; Path = "Functions"}
            ) -Type Redist
        }

        It "has all files for .NET Core for Release" {
            TestPackageContents $true @(
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_ObjectSettings.help.txt"}
                @{Type = "File"; Path = "about_ChannelSettings.help.txt"}
                @{Type = "File"; Path = "about_PrtgAPI.help.txt"}
                @{Type = "File"; Path = "about_SensorParameters.help.txt"}
                @{Type = "File"; Path = "about_SensorSettings.help.txt"}
                @{Type = "File"; Path = "Functions\New-Credential.ps1"}
                @{Type = "File"; Path = "PrtgAPI.cmd"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.deps.json"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.dll"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.pdb"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.xml"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.PowerShell.deps.json"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.PowerShell.pdb"}
                @{Type = "File"; Path = "coreclr\PrtgAPI.PowerShell.xml"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.dll"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.pdb"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.xml"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.PowerShell.dll"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.PowerShell.pdb"}
                @{Type = "File"; Path = "fullclr\PrtgAPI.PowerShell.xml"}
                @{Type = "File"; Path = "PrtgAPI.Format.ps1xml"}
                @{Type = "File"; Path = "PrtgAPI.PowerShell.dll-Help.xml"}
                @{Type = "File"; Path = "PrtgAPI.psd1"}
                @{Type = "File"; Path = "PrtgAPI.psm1"}
                @{Type = "File"; Path = "PrtgAPI.sh"}
                @{Type = "Folder"; Path = "coreclr"}
                @{Type = "Folder"; Path = "fullclr"}
                @{Type = "Folder"; Path = "Functions"}
            ) "Release" -Type Redist
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