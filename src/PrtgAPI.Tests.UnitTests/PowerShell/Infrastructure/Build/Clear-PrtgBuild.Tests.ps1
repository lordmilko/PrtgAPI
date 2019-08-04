. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

Describe "Clear-PrtgBuild" -Tag @("PowerShell", "Build") {

    $solutionRoot = Get-SolutionRoot

    It "clears core" {

        Mock-InstallDotnet -Windows
        Mock "Remove-Item" {} -ModuleName "CI"

        Mock-InvokeProcess "dotnet clean `"$(Join-PathEx $solutionRoot PrtgAPIv17.sln)`" -c Debug" {
            Clear-PrtgBuild
        }
    }

    It "clears desktop" {

        Mock Get-MSBuild {
            return "C:\msbuild.exe"
        } -ModuleName CI

        Mock "Remove-Item" {} -ModuleName "CI"

        Mock-InvokeProcess "& C:\msbuild.exe /t:clean `"$(Join-PathEx $solutionRoot PrtgAPI.sln)`" /p:Configuration=Debug" {
            Clear-PrtgBuild -Legacy
        }
    }

    It "performs a full clear" {

        $global:clearPrtgBuildMockRemovals = @()

        InModuleScope "PrtgAPI.Build" {

            Mock "Get-ChildItem" {

                param(
                    $Path,
                    $Filter,
                    [switch]$Recurse
                )

                if($Filter -eq "*.csproj")
                {
                    $gci = Get-Command Get-ChildItem -CommandType Cmdlet

                    return & $gci $Path -Filter $Filter -Recurse:$Recurse
                }

                $ch = [IO.Path]::DirectorySeparatorChar

                if($Path -like "*$($ch)bin" -or $Path -like "*$($ch)obj")
                {
                    $file = [PSCustomObject]@{
                        Name = "file.txt"
                        FullName = "$Path\foo.txt"
                        PSIsContainer = $false
                    }

                    $folder = [PSCustomObject]@{
                        Name = "bar"
                        FullName = "$Path\bar"
                        PSIsContainer = $true
                    }

                    return $file,$folder
                }

                throw "Path was $Path, Filter was $Filter"
            }

            Mock "Test-Path" {
                return $true
            }
        }

        Mock "Invoke-WebRequest" {} # Remove the verifiable mock from Mock-InstallDotnet

        $mockBlock = {
            param($Path)

            foreach($item in $Path)
            {
                $p = $item

                if($item.StartsWith("@"))
                {
                    $p = $item -replace ".+FullName=(.+?);.+",'$1'
                }

                $root = Get-SolutionRoot

                $p = $p.Replace($root, "").Trim([IO.Path]::DirectorySeparatorChar).Replace("/", "\")

                $global:clearPrtgBuildMockRemovals += $p
            }  
        }

        Mock "Remove-Item" $mockBlock -ModuleName "PrtgAPI.Build" -Verifiable
        Mock "Remove-Item" $mockBlock -ModuleName "CI" -Verifiable

        Mock "Get-ChildItem" {

            param(
                $Path,
                $Filter
            )

            if($Filter -eq "*.*nupkg")
            {
                return [PSCustomObject]@{
                    FullName = Join-Path $Path "PrtgAPI.nupkg"
                    Name = "PrtgAPI.nupkg"
                }
            }

            if($Filter -eq "*.zip")
            {
                return [PSCustomObject]@{
                    FullName = Join-Path $Path "PrtgAPI.zip"
                    Name = "PrtgAPI.zip"
                }
            }

            throw "Path was $Path, Filter was $Filter"
        } -ModuleName "CI"

        Clear-PrtgBuild -Full

        Assert-VerifiableMocks

        $expectedRemovals = @()

        $projects = @(
            "PrtgAPI"
            "PrtgAPI.PowerShell"
            "PrtgAPI.Tests.IntegrationTests"
            "PrtgAPI.Tests.UnitTests"
            "Tools\PowerShell.TestAdapter"
            "Tools\PrtgAPI.CodeGenerator"
        )

        foreach($project in $projects)
        {
            $expectedRemovals += @(
                "src\$project\bin\foo.txt"
                "src\$project\bin\bar"
                "src\$project\bin"
                "src\$project\obj\foo.txt"
                "src\$project\obj\bar"
                "src\$project\obj"
            )
        }

        $expectedRemovals += @(
            "msbuild.binlog"
            "PrtgAPI.nupkg"
            "PrtgAPI.zip"
        )

        $extraRemovals = $global:clearPrtgBuildMockRemovals | where { $_ -notin $expectedRemovals }
        $missingRemovals = $expectedRemovals | where { $_ -notin $global:clearPrtgBuildMockRemovals }

        if($extraRemovals)
        {
            throw "Found the following extra removals: $extraRemovals"
        }

        if($missingRemovals)
        {
            throw "Found the following missing removals: $missingRemovals"
        }
    }
}