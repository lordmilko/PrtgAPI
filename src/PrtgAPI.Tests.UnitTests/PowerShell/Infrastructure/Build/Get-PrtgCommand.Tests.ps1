. $PSScriptRoot\..\..\..\Support\PowerShell\Build.ps1

Describe "Get-PrtgCommand" -Tag @("PowerShell", "Build") {
    It "retrieves all commands" {
        $commands = Get-PrtgCommand

        $expectedCommands = @(
            "Clear-PrtgBuild"
            "Invoke-PrtgBuild"
            "Get-PrtgCoverage"
            "New-PrtgPackage"
            "Simulate-PrtgCI"            
            "Get-PrtgCommand"
            "Get-PrtgHelp"
            "Get-PrtgTestResult"
            "Invoke-PrtgTest"
            "Get-PrtgLog"
            "Install-PrtgDependency"
            "Invoke-PrtgAnalyzer"
            "Start-PrtgAPI"
            "Get-PrtgVersion"
            "Set-PrtgVersion"
            "Update-PrtgVersion"
        )

        if($commands.Count -ne $expectedCommands.Count)
        {
            $additionalCommands = $commands|select -expand name | where { $_ -notin $expectedCommands }

            throw "Found additional unexpected commands: $($additionalCommands -join ",")"
        }

        for($i = 0; $i -lt $expectedCommands.Count; $i++)
        {
            $commands[$i].Name | Should Be $expectedCommands[$i]

            if($commands[$i].Description.Contains("["))
            {
                throw "Command '$($commands[$i].Name)' is missing documentation"
            }
        }
    }

    It "filters specified commands" {
        $commands = Get-PrtgCommand *invoke*

        $commands.Count | Should Be 3

        $commands[0].Name | Should Be "Invoke-PrtgBuild"
        $commands[1].Name | Should Be "Invoke-PrtgTest"
        $commands[2].Name | Should Be "Invoke-PrtgAnalyzer"
    }

    It "lists all commands properly in PrtgAPI.Build.psd1" {

        $root = Get-SolutionRoot

        $psd1Folder = Join-Path $root "build\PrtgAPI.Build"
        $psd1Path = Join-Path $psd1Folder "PrtgAPI.Build.psd1"
        $buildFunctions = Join-Path $psd1Folder "Functions"

        $psd1 = [PSCustomObject](Import-PowerShellDataFile $psd1Path)

        $expectedAliases = @("Simulate-PrtgCI")
        $actualAliases = $psd1.AliasesToExport

        # Functions

        $fileFunctions = gci $buildFunctions | select -ExpandProperty BaseName

        $excluded = @(
            "Initialize-BuildEnvironment"
            "Simulate-PrtgCI"
        )

        $expectedFunctions = @(
            $fileFunctions | where { $_ -notin $excluded }
            "Test-PrtgCI"
        )

        $actualFunctions = $psd1.FunctionsToExport

        $missingFunctions = $expectedFunctions | where { $_ -notin $actualFunctions }
        $extraFunctions = $actualFunctions | where { $_ -notin $expectedFunctions }

        if($missingFunctions)
        {
            $str = $missingFunctions -join ", "

            throw "PrtgAPI.Build.psd1\FunctionsToExport is missing $($missingFunctions.Count) functions: $str"
        }

        if($extraFunctions)
        {
            $str = $extraFunctions -join ", "

            throw "PrtgAPI.Build.psd1\FunctionsToExport contains $($extraFunctions.Count) extra functions: $str"
        }

        # Aliases

        $missingAliases = $expectedAliases | where { $_ -notin $actualAliases }
        $extraAliases = $actualAliases | where { $_ -notin $expectedAliases }

        if($missingAliases)
        {
            $str = $missingAliases -join ", "

            throw "PrtgAPI.Build.psd1\AliasesToExport is missing $($missingAliases.Count) aliases: $str"
        }

        if($extraAliases)
        {
            $str = $extraAliases -join ", "

            throw "PrtgAPI.Build.psd1\AliasesToExport contains $($extraAliases.Count) extra aliases: $str"
        }
    }
}