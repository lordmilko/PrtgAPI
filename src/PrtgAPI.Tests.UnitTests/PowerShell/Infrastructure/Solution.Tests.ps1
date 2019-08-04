Describe "Solution" -Tag @("PowerShell", "UnitTest") {

    if(!(Get-Module -ListAvailable PSScriptAnalyzer))
    {
        Install-Package PSScriptAnalyzer -ProviderName PowerShellGet -ForceBootstrap -Force | Out-Null
    }

    It "doesn't use 'sort' alias" {

        $solution = Resolve-Path "$PSScriptRoot\..\..\..\.."

        # Invoke-ScriptAnalyzer is a very dodgy cmdlet that uses all sorts of asynchronous programming internally
        # and has the potential to muck up your entire progress (such as not being able to call [CodeCoverage]::new()
        # when calculating Appveyor coverage with -IsCore:$false). To protect against this, we execute this command
        # in an external process

        $powershell = "powershell"

        if($PSEdition -eq "Core")
        {
            $powershell = "pwsh"
        }

        $expr = @"
`$violations = Invoke-ScriptAnalyzer $solution -IncludeRule PSAvoidUsingCmdletAliases -Recurse

        `$sortViolations = `$violations | where { `$_.Extent.Text -eq \"sort\" }

        if(`$sortViolations)
        {
            `$str = (`$sortViolations|select -expand ScriptName) -join \", \"

            throw \"Found illegal usages of 'sort' in the following scripts: `$str\"
        }
"@

        $result = & $powershell -Command $expr

        if($LASTEXITCODE -ne 0)
        {
            throw $result
        }
    }
}