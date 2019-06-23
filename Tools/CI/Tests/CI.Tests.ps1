Import-Module $PSScriptRoot\..\ci.psm1

Describe "CI" {

    It "invokes a native executable" {
        $version = Invoke-Process {
            dotnet --version
        }
    }

    It "gets the PrtgAPI version" {
        $version = Get-PrtgVersion $PSScriptRoot\..\..\..\

        $version -match "\d+\.\d+\.\d+" | Should Be $true
    }

    It "gets the solution root" {
        $root = Get-SolutionRoot

        $items = gci $root -Filter *.sln

        $items.Count | Should BeGreaterThan 0
    }
}