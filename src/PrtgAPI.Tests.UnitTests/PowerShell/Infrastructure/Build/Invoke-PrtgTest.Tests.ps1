. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

function MockInvokePester($action)
{
    InModuleScope CI {
        Mock Invoke-Pester {

            $root = Get-SolutionRoot

            $PassThru | Should be $true
            $OutputFile | Should BeLike (Join-PathEx $root src PrtgAPI.Tests.UnitTests TestResults "PrtgAPI_PowerShell_*.xml")
            $OutputFormat | Should Be "NUnitXml"
            $Script | Should Be (Join-PathEx $root src PrtgAPI.Tests.UnitTests PowerShell)

        } -Verifiable
    }

    & $action

    Assert-VerifiableMocks
}

Describe "Invoke-PrtgTest" -Tag @("PowerShell", "Build") {

    It "executes C# with core" {

        Mock-InstallDotnet -Windows

        $sourceRoot = Get-SourceRoot

        $expected = @(
            "& `"dotnet`""
            "test"
            Join-PathEx $sourceRoot PrtgAPI.Tests.UnitTests PrtgAPIv17.Tests.UnitTests.csproj
            "-nologo"
            "--no-restore"
            "--no-build"
            "--verbosity:n"
            "-c"
            "Debug"
            "--logger"
            "trx;LogFileName=PrtgAPI_C#.trx"
            "--filter"
            "FullyQualifiedName~dynamic"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest *dynamic* -Type C# -Build
        }
    }

    It "executes C# with desktop" {

        Mock Get-VSTest {
            return "C:\vstest.console.exe"
        } -ModuleName CI

        Mock "Test-CIIsWindows" {
            return $true
        }

        $sourceRoot = Get-SourceRoot

        $expected = @(
            "& C:\vstest.console.exe"
            Join-PathEx $sourceRoot PrtgAPI.Tests.UnitTests bin Debug PrtgAPI.Tests.UnitTests.dll
            "/logger:trx;LogFileName=PrtgAPI_C#.trx"
            "/TestCaseFilter:FullyQualifiedName~dynamic"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest *dynamic* -Type C# -Legacy -Build
        }
    }

    It "executes PowerShell with core" {

        MockInvokePester {
            Invoke-PrtgTest -Type PowerShell -Build
        }
    }

    It "executes PowerShell with desktop" {
        MockInvokePester {
            Invoke-PrtgTest -Type PowerShell -Legacy -Build
        }
    }

    It "executes with Release build" {

        Mock-InstallDotnet -Windows

        $sourceRoot = Get-SourceRoot

        $expected = @(
            "& `"dotnet`""
            "test"
            Join-PathEx $sourceRoot PrtgAPI.Tests.UnitTests PrtgAPIv17.Tests.UnitTests.csproj
            "-nologo"
            "--no-restore"
            "--no-build"
            "--verbosity:n"
            "-c"
            "Release"
            "--logger"
            "trx;LogFileName=PrtgAPI_C#.trx"
            "--filter"
            "FullyQualifiedName~dynamic"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest *dynamic* -Type C# -c Release -Build
        }
    }

    It "specifies multiple C# limits" {

        Mock-InstallDotnet -Windows

        $sourceRoot = Get-SourceRoot

        $expected = @(
            "& `"dotnet`""
            "test"
            Join-PathEx $sourceRoot PrtgAPI.Tests.UnitTests PrtgAPIv17.Tests.UnitTests.csproj
            "-nologo"
            "--no-restore"
            "--no-build"
            "--verbosity:n"
            "-c"
            "Debug"
            "--logger"
            "trx;LogFileName=PrtgAPI_C#.trx"
            "--filter"
            "FullyQualifiedName~dynamic|FullyQualifiedName~potato"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest *dynamic*,*potato* -Type C# -Build
        }
    }

    It "specifies multiple PowerShell limits" {

        InModuleScope CI {
            Mock Invoke-Pester {

                param($TestName)

                $TestName -join "," | Should Be "*dynamic*,*potato*"
            }
        }

        Invoke-PrtgTest *dynamic*,*potato* -Type PowerShell -Build
    }

    It "executes c# integration tests" {
        
        Mock-InstallDotnet -Windows

        $sourceRoot = Get-SourceRoot
        
        $expected = @(
            "& `"dotnet`""
            "test"
            Join-PathEx $sourceRoot PrtgAPI.Tests.IntegrationTests PrtgAPIv17.Tests.IntegrationTests.csproj
            "-nologo"
            "--no-restore"
            "--no-build"
            "--verbosity:n"
            "-c"
            "Debug"
            "--logger"
            "trx;LogFileName=PrtgAPI_C#.trx"
            "--filter"
            "FullyQualifiedName~dynamic"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest *dynamic* -Type C# -Integration -Build
        }
    }

    It "specifies C# tags" {

        Mock-InstallDotnet -Windows

        $sourceRoot = Get-SourceRoot

        $expected = @(
            "& `"dotnet`""
            "test"
            Join-PathEx $sourceRoot PrtgAPI.Tests.UnitTests PrtgAPIv17.Tests.UnitTests.csproj
            "-nologo"
            "--no-restore"
            "--no-build"
            "--verbosity:n"
            "-c"
            "Debug"
            "--logger"
            "trx;LogFileName=PrtgAPI_C#.trx"
            "--filter"
            "TestCategory=UnitTest|TestCategory=SkipCoverage"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest -Tag UnitTest,SkipCoverage -Type C# -Build
        }
    }

    It "specifies PowerShell tags" {

        InModuleScope CI {
            Mock Invoke-Pester {

                $root = Get-SolutionRoot

                $PassThru | Should be $true
                $OutputFile | Should BeLike (Join-PathEx $root src PrtgAPI.Tests.UnitTests TestResults "PrtgAPI_PowerShell_*.xml")
                $OutputFormat | Should Be "NUnitXml"
                $Script | Should Be (Join-PathEx $root src PrtgAPI.Tests.UnitTests PowerShell)
                $Tag[0] | Should Be "UnitTest"
                $Tag[1] | Should Be "IntegrationTest"

            } -Verifiable
        }

        Invoke-PrtgTest -Tag UnitTest,IntegrationTest -Type PowerShell -Build

        Assert-VerifiableMocks
    }

    It "specifies C# names and tags" {

        Mock-InstallDotnet -Windows

        $sourceRoot = Get-SourceRoot

        $expected = @(
            "& `"dotnet`""
            "test"
            Join-PathEx $sourceRoot PrtgAPI.Tests.UnitTests PrtgAPIv17.Tests.UnitTests.csproj
            "-nologo"
            "--no-restore"
            "--no-build"
            "--verbosity:n"
            "-c"
            "Debug"
            "--logger"
            "trx;LogFileName=PrtgAPI_C#.trx"
            "--filter"
            "(FullyQualifiedName~dynamic|FullyQualifiedName~potato)&(TestCategory=UnitTest|TestCategory=SkipCoverage)"
        )

        Mock-InvokeProcess $expected {
            Invoke-PrtgTest *dynamic*,*potato* -Tag UnitTest,SkipCoverage -Type C# -Build
        }
    }
}