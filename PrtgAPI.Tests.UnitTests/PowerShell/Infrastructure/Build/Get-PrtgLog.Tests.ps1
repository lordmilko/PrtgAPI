. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

function MockCommands
{
    InModuleScope "PrtgAPI.Build" {
        Mock "Test-Path" { $true }
        Mock "Set-Content" { }
        Mock "Start-Process" { }
        Mock "Invoke-Expression" { }
        Mock "Clear-Host" { }
    }
}

Describe "Get-PrtgLog" -Tag @("PowerShell", "Build") {
    
    Context "Base" {
        It "views integration test logs" {

            MockCommands

            Get-PrtgLog
        }

        It "views build logs" {

            MockCommands

            Get-PrtgLog -Build
        }
    }

    Context "Pattern" {
        It "filters integration test logs" {

            MockCommands

            Get-PrtgLog test
        }

        It "filters build logs" {

            MockCommands

            Get-PrtgLog test -Build
        }
    }

    Context "Clear" {
        It "clears integration test logs" {

            MockCommands

            Get-PrtgLog -Clear
        }

        It "clears build logs" {
            MockCommands

            Get-PrtgLog -Build -CLear
        }
    }

    Context "Full" {
        It "views full integration test logs" {
            MockCommands

            Get-PrtgLog -Full
        }

        It "views full build logs" {
            MockCommands

            Get-PrtgLog -Build -Full
        }

        It "clears full integration test logs" {
            MockCommands

            Get-PrtgLog -Full -Clear
        }

        It "clears full build logs" {
            MockCommands

            Get-PrtgLog -Build -Full -Clear
        }
    }

    Context "Lines" {
        It "views a specified number of lines from integration test logs" {
            MockCommands

            Get-PrtgLog -Lines 20
        }

        It "views a specified number of lines from build logs" {
            MockCommands

            Get-PrtgLog -Build -Lines 20
        }
    }

    Context "Window" {
        It "opens integration test logs in a new window" {
            MockCommands

            Get-PrtgLog -Window
        }

        It "opens build logs in a new window" {
            MockCommands

            Get-PrtgLog -Build -Window
        }
    }
}