. $PSScriptRoot\..\..\..\..\PrtgAPI.Tests.UnitTests\Support\PowerShell\BuildCore.ps1

$ErrorActionPreference = "Stop"

Describe "Set-PrtgVersion_IT" -Tag @("PowerShell", "Build_IT") {
    It "sets version on core" -Skip:(SkipBuildTest) {
        $originalVersion = Get-PrtgVersion

        try
        {
            Set-PrtgVersion 1.2.3

            $newVersion = Get-PrtgVersion

            $newVersion.Package | Should Be "1.2.3"
        }
        finally
        {
            Set-PrtgVersion $originalVersion.File
        }
    }

    It "sets version on desktop" -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
        $originalVersion = Get-PrtgVersion -Legacy

        try
        {
            Set-PrtgVersion 1.2.3 -Legacy

            $newVersion = Get-PrtgVersion -Legacy

            $newVersion.Package | Should Be "1.2.3"
        }
        finally
        {
            Set-PrtgVersion $originalVersion.File -Legacy
        }
    }

    Context "CI" {
        It "doesn't add a build number when this is the first build" -Skip:(SkipBuildTest) {
            $originalVersion = Get-PrtgVersion

            try
            {
                Set-CIVersion -CIBuild "0.2.4"

                $result = Get-PrtgVersion

                $result.File | Should Not Be "0.2.4"
                $result.Info | Should Be "0.2.4"
            }
            finally
            {
                Set-PrtgVersion $originalVersion.File
            }
        }

        It "adds a build number when this is the second build or later with Core" -Skip:(SkipBuildTest) {
            $originalVersion = Get-PrtgVersion

            try
            {
                Set-CIVersion -CIBuild "1.2.4-build.1"

                $result = Get-PrtgVersion

                # Modify the existing File Version and Info Version
                $result.File | Should Be "1.2.4.1"
                $result.Info | Should Be "1.2.4.1"
            }
            finally
            {
                Set-PrtgVersion $originalVersion.File
            }
        }

        It "sets the info version when this is a preview build with Core" -Skip:(SkipBuildTest) {
            $originalVersion = Get-PrtgVersion

            try
            {
                Set-CIVersion -CIBuild "1.2.4-preview.1"

                $result = Get-PrtgVersion

                $result.Info | Should Be "1.2.4-preview.1"
            }
            finally
            {
                Set-PrtgVersion $originalVersion.File
            }
        }

        It "adds a build number when this is the second build or later with Desktop" -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
            $originalVersion = Get-PrtgVersion -Legacy

            try
            {
                Set-CIVersion -CIBuild "1.2.4-build.1" -IsCore:$false

                $result = Get-PrtgVersion -Legacy

                # Modify the existing File Version and Info Version
                $result.File | Should Be "1.2.4.1"
                $result.Info | Should Be "1.2.4.1"
            }
            finally
            {
                Set-PrtgVersion $originalVersion.File -Legacy
            }
        }

        It "sets the info version when this is a preview build with Desktop" -Skip:(!(Test-IsWindows) -or (SkipBuildTest)) {
            $originalVersion = Get-PrtgVersion -Legacy

            try
            {
                Set-CIVersion -CIBuild "1.2.4-preview.1" -IsCore:$false

                $result = Get-PrtgVersion -Legacy

                $result.Info | Should Be "1.2.4-preview.1"
            }
            finally
            {
                Set-PrtgVersion $originalVersion.File -Legacy
            }
        }
    }
}