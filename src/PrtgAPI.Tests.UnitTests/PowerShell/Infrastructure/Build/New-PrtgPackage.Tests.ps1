. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

$testCases = @(
    @{name = "Debug"}
    @{name = "Release"}
)

Describe "New-PrtgPackage" -Tag @("PowerShell", "Build") {
    It "creates a C# package on core for <name>" -TestCases $testCases {

        param($name)

        Mock-InstallDotnet -Windows

        $root = Get-SourceRoot
        $tempRepository = Join-Path ([IO.Path]::GetTempPath()) "TempRepository"

        Mock "Get-PrtgVersion" {
            return [PSCustomObject]@{
                Package = "1.2.3"
            }
        } -ModuleName PrtgAPI.Build

        $expected = @(
            "&"
            "dotnet"
            "pack"
            Join-PathEx $root PrtgAPI PrtgAPIv17.csproj
            "--include-symbols"
            "--no-restore"
            "--no-build"
            "-c"
            "$name"
            "--output"
            $tempRepository
            "/nologo"
            "-p:EnableSourceLink=true;SymbolPackageFormat=snupkg"
        )

        Mock-InvokeProcess $expected {
            New-PrtgPackage -Type C# -Configuration $name
        }
    }

    It "creates a C# package on desktop for <name>" -TestCases $testCases {

        param($name)

        Mock Test-CIIsWindows {
            return $true
        } -ModuleName CI

        Mock-NuGet

        $root = Get-SourceRoot
        $tempRepository = Join-Path ([IO.Path]::GetTempPath()) "TempRepository"

        Mock "Get-PrtgVersion" {
            return [PSCustomObject]@{
                Package = "1.2.3"
            }
        } -ModuleName PrtgAPI.Build

        $expected = @(
            "&"
            "nuget"
            "pack"
            Join-PathEx $root PrtgAPI PrtgAPI.csproj
            "-Exclude"
            "**/*.tt;**/Resources/*.txt;*PrtgClient.Methods.xml;**/*.json"
            "-outputdirectory"
            $tempRepository
            "-NoPackageAnalysis"
            "-symbols"
            "-version 1.2.3"
            "-properties"
            "Configuration=$name"
        )

        Mock-InvokeProcess $expected {
            New-PrtgPackage -Type C# -Legacy -Configuration $name
        }
    }

    It "creates a PowerShell package on core for <name>" -TestCases $testCases {

        param($name)

        Mock-InstallDotnet -Windows

        InModuleScope "CI" {

            $empty = {}

            Mock "Publish-ModuleEx" $empty
            Mock "Get-PSRepositoryEx" $empty
            Mock "Register-PSRepositoryEx" $empty
            Mock "Unregister-PSRepositoryEx" $empty
            Mock "Remove-Item" $empty
            Mock "Move-Item" $empty
            Mock "New-Item" $empty
            Mock "Get-Item" $empty
            Mock "Update-RootModule" $empty
            Mock "Invoke-Expression" $empty -ParameterFilter { $Command -like "*EncodedCommand*" }
            Mock "Test-Path" {
                return $true
            }
            Mock "Get-ChildItem" {
                return [System.IO.DirectoryInfo](Join-PathEx (global:FSRoot) PrtgAPI bin Release)
            }
            Mock "Copy-Item" $empty
            Mock "Resolve-Path" {
                return [PSCustomObject]@{
                    Path = Join-PathEx (global:FSRoot) PrtgAPI bin Release netstandard2.0
                }
            }
        }

        New-PrtgPackage -Type PowerShell -Configuration $name
    }

    It "creates a PowerShell package on desktop for <name>" -TestCases $testCases {

        param($name)

        InModuleScope "CI" {

            $empty = {}

            Mock "Publish-ModuleEx" $empty
            Mock "Get-PSRepositoryEx" $empty
            Mock "Register-PSRepositoryEx" $empty
            Mock "Unregister-PSRepositoryEx" $empty
            Mock "Remove-Item" $empty
            Mock "Move-Item" $empty
            Mock "New-Item" $empty
            Mock "Get-Item" $empty
            Mock "Invoke-Expression" $empty -ParameterFilter { $Command -like "*EncodedCommand*" }
            Mock "Test-Path" {
                return $true
            }
            Mock "Get-ChildItem" {
                return [System.IO.DirectoryInfo](Join-PathEx (global:FSRoot) PrtgAPI bin Release)
            }
            Mock "Copy-Item" $empty
        }

        New-PrtgPackage -Type PowerShell -Legacy -Configuration $name
    }

    It "creates a redistributable package on core for <name>" -TestCases $testCases {
        param($name)

        InModuleScope "CI" {

            $empty = {}

            Mock "Publish-ModuleEx" $empty
            Mock "Get-PSRepositoryEx" $empty
            Mock "Register-PSRepositoryEx" $empty
            Mock "Unregister-PSRepositoryEx" $empty
            Mock "Remove-Item" $empty
            Mock "Move-Item" $empty
            Mock "New-Item" $empty
            Mock "Get-Item" $empty
            Mock "Update-RootModule" $empty
            Mock "Invoke-Expression" $empty -ParameterFilter { $Command -like "*EncodedCommand*" }
            Mock "Test-Path" {
                return $true
            }
            Mock "Get-ChildItem" {
                return [System.IO.DirectoryInfo](Join-PathEx (global:FSRoot) PrtgAPI bin Release)
            }
            Mock "Copy-Item" $empty
            Mock "Resolve-Path" {
                return [PSCustomObject]@{
                    Path = Join-PathEx (global:FSRoot) PrtgAPI bin Release netstandard2.0
                }
            }
            Mock "Compress-Archive" $empty
        }

        New-PrtgPackage -Type Redist -Configuration $name
    }

    It "creates a redistributable package on desktop for <name>" -TestCases $testCases {
        param($name)

        InModuleScope "CI" {

            $empty = {}

            Mock "Publish-ModuleEx" $empty
            Mock "Get-PSRepositoryEx" $empty
            Mock "Register-PSRepositoryEx" $empty
            Mock "Unregister-PSRepositoryEx" $empty
            Mock "Remove-Item" $empty
            Mock "Move-Item" $empty
            Mock "New-Item" $empty
            Mock "Get-Item" $empty
            Mock "Update-RootModule" $empty
            Mock "Invoke-Expression" $empty -ParameterFilter { $Command -like "*EncodedCommand*" }
            Mock "Test-Path" {
                return $true
            }
            Mock "Get-ChildItem" {
                return [System.IO.DirectoryInfo](Join-PathEx (global:FSRoot) PrtgAPI bin Release)
            }
            Mock "Copy-Item" $empty
            Mock "Resolve-Path" {
                return [PSCustomObject]@{
                    Path = Join-PathEx (global:FSRoot) PrtgAPI bin Release netstandard2.0
                }
            }
            Mock "Compress-Archive" $empty
        }

        New-PrtgPackage -Type Redist -Legacy -Configuration $name
    }
}