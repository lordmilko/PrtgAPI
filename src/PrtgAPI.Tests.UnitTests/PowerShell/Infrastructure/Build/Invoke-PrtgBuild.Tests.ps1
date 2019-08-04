. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

function Get-NuGet
{
    $root = Get-SolutionRoot

    $expected = @(
        "&"
        "C:\ProgramData\chocolatey\bin\nuget.exe"
        "restore"
        Join-Path $root 'PrtgAPI.sln'
    )

    return $expected
}

Describe "Invoke-PrtgBuild" -Tag @("PowerShell", "Build") {
    It "executes with core" {

        Mock-InstallDotnet -Windows
        Mock-StartProcess
        Mock-InstallReferenceAssemblies

        $expected1 = Get-Net452
        $expected2 = Get-Net461

        $expected3 = @(
            "dotnet"
            "build"
            Join-Path (Get-SolutionRoot) "PrtgAPIv17.sln"
            "-nologo"
            "-c"
            "Debug"
            "-p:EnableSourceLink=true"
        )

        Mock-InvokeProcess $expected1,$expected2,$expected3 {
            Invoke-PrtgBuild
        }
    }

    It "executes with desktop" {

        Mock Get-MSBuild {
            return "C:\msbuild.exe"
        } -ModuleName CI

        $root = Get-SolutionRoot

        Mock-NuGet

        $expected1 = Get-NuGet

        $expected2 = @(
            "&"
            "C:\msbuild.exe"
            Join-Path (Get-SolutionRoot) "PrtgAPI.sln"
            "/verbosity:minimal"
            "/p:Configuration=Debug"
        )

        Mock-InvokeProcess $expected1,$expected2 {
            Invoke-PrtgBuild -Legacy
        }
    }

    It "executes with core on Linux" {
        Mock-InstallDotnet -Unix

        $expected = @(
            "dotnet"
            "build"
            Join-Path (Get-SolutionRoot) "PrtgAPIv17.sln"
            "-nologo"
            "-c"
            "Debug"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgBuild
        }
    }

    It "specifies the project to build with core" {

        Mock-InstallDotnet -Windows
        Mock-StartProcess
        Mock-InstallReferenceAssemblies

        $expected1 = Get-Net452
        $expected2 = Get-Net461

        $expected3 = @(
            "dotnet"
            "build"
            Join-PathEx (Get-SourceRoot) PrtgAPI PrtgAPIv17.csproj
            "-nologo"
            "-c"
            "Debug"
            "-p:EnableSourceLink=true"
        )

        Mock-InvokeProcess $expected1,$expected2,$expected3 {
            Invoke-PrtgBuild prtgapiv17
        }
    }

    It "specifies the project to build with desktop" {

        Mock Get-MSBuild {
            return "C:\msbuild.exe"
        } -ModuleName CI

        Mock-NuGet

        $expected1 = Get-NuGet

        $expected2 = @(
            "&"
            "C:\msbuild.exe"
            Join-PathEx (Get-SourceRoot) PrtgAPI PrtgAPI.csproj
            "/verbosity:minimal"
            "/p:Configuration=Debug"
        )

        Mock-InvokeProcess $expected1,$expected2 {
            Invoke-PrtgBuild prtgapi -Legacy
        }
    }

    It "throws when more than one project is specified" {
        { Invoke-PrtgBuild *test* } | Should Throw "Can only specify one project at a time, however wildcard '*test*' matched multiple projects: PowerShell.TestAdapter, PrtgAPIv17.Tests.IntegrationTests, PrtgAPIv17.Tests.UnitTests"
    }

    It "executes MSBuild in debug mode on core" {
        
        Mock-InstallDotnet -Windows
        Mock-StartProcess
        Mock-InstallReferenceAssemblies

        $expected1 = Get-Net452
        $expected2 = Get-Net461

        $root = Get-SolutionRoot
        
        $expected4 = Join-Path $root "msbuild.binlog"

        $expected3 = @(
            "dotnet"
            "build"
            Join-Path $root "PrtgAPIv17.sln"
            "-nologo"
            "-c"
            "Debug"
            "-p:EnableSourceLink=true"
            "/bl:$expected4"
        )

        Mock-AllProcess $expected1,$expected2,$expected3,$expected4 {
            Invoke-PrtgBuild -DebugMode
        }
    }

    It "executes MSBuild in debug mode on desktop" {
        Mock Get-MSBuild {
            return "C:\msbuild.exe"
        } -ModuleName CI

        $root = Get-SolutionRoot

        Mock-NuGet

        $expected1 = Get-NuGet

        $expected3 = Join-Path $root "msbuild.binlog"

        $expected2 = @(
            "&"
            "C:\msbuild.exe"
            Join-PathEx (Get-SourceRoot) PrtgAPI PrtgAPI.csproj
            "/verbosity:minimal"
            "/p:Configuration=Debug"
            "/bl:$expected2"
        )

        Mock-AllProcess $expected1,$expected2,$expected3 {
            Invoke-PrtgBuild prtgapi -Legacy -DebugMode
        }
    }

    It "executes with Release build on core" {

        Mock-InstallDotnet -Windows
        Mock-StartProcess
        Mock-InstallReferenceAssemblies

        $expected1 = Get-Net452
        $expected2 = Get-Net461

        $expected3 = @(
            "dotnet"
            "build"
            Join-Path (Get-SolutionRoot) "PrtgAPIv17.sln"
            "-nologo"
            "-c"
            "Release"
            "-p:EnableSourceLink=true"
        )

        Mock-InvokeProcess $expected1,$expected2,$expected3 {
            Invoke-PrtgBuild -Configuration Release
        }
    }

    It "executes with Release build on desktop" {
        Mock Get-MSBuild {
            return "C:\msbuild.exe"
        } -ModuleName CI

        Mock-NuGet

        $expected1 = Get-NuGet

        $expected2 = @(
            "&"
            "C:\msbuild.exe"
            Join-Path (Get-SolutionRoot) "PrtgAPI.sln"
            "/verbosity:minimal"
            "/p:Configuration=Release"
        )

        Mock-InvokeProcess $expected1,$expected2 {
            Invoke-PrtgBuild -Legacy -Configuration Release
        }
    }

    It "processes additional arguments" {

        Mock-InstallDotnet -Windows
        Mock-StartProcess
        Mock-InstallReferenceAssemblies

        $expected1 = Get-Net452
        $expected2 = Get-Net461

        $expected3 = @(
            "dotnet"
            "build"
            Join-Path (Get-SolutionRoot) "PrtgAPIv17.sln"
            "-nologo"
            "-c"
            "Debug"
            "-p:EnableSourceLink=true"
            "first"
            "second"
        )

        Mock-InvokeProcess $expected1,$expected2,$expected3 {
            Invoke-PrtgBuild -Args "first","second"
        }
    }
}