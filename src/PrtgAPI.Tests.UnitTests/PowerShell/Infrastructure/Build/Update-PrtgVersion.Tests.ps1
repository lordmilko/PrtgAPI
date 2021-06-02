. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

Describe "Update-PrtgVersion" -Tag @("PowerShell", "Build") {
    It "updates all versions" {

        $global:getPrtgVersionIndex = 0

        InModuleScope "PrtgAPI.Build" {
            Mock "Get-PrtgVersion" {

                $global:getPrtgVersionIndex++

                if($global:getPrtgVersionIndex -eq 1 -or $global:getPrtgVersionIndex -eq 2)
                {
                    return [PSCustomObject]@{
                        Package = "1.2.3"
                        Assembly = "1.2.0.0"
                        File = [Version]"1.2.3.4"
                        Module = "1.2.3"
                        ModuleTag = "v1.2.3"
                        PreviousTag = "v1.2.3"
                    }
                }
                elseif($global:getPrtgVersionIndex -eq 3)
                {
                    return [PSCustomObject]@{
                        Package = "1.2.4"
                        Assembly = "1.2.0.0"
                        File = [Version]"1.2.4.4"
                        Module = "1.2.4"
                        ModuleTag = "v1.2.4"
                        PreviousTag = "v1.2.3"
                    }
                }

                throw "Don't know how to handle request for index $global:getPrtgVersionIndex"
            }

            Mock "Set-CIVersion" {
                param($Version)

                $Version | Should Be "1.2.4.4"
            }
        }

        $result = Update-PrtgVersion

        $result.Package | Should Be "1.2.3 -> 1.2.4"
        $result.Assembly | Should Be "1.2.0.0"
        $result.File | Should Be "1.2.3.4 -> 1.2.4.4"
        $result.Module | Should Be "1.2.3 -> 1.2.4"
        $result.ModuleTag | Should Be "v1.2.3 -> v1.2.4"
        $result.PreviousTag | Should Be "v1.2.3"
    }
}