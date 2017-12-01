. $PSScriptRoot\ObjectProperty.ps1

function It {
    [CmdletBinding(DefaultParameterSetName = 'Normal')]
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$name,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock] $script,

        [Parameter(Mandatory = $true)]
        [System.Collections.IDictionary[]] $TestCases
    )

    Pester\It $name {

        try
        {
            foreach($test in $TestCases)
            {
                LogTestName "Running test '$($name): $($test["name"])'"

                & $script @test
            }
        }
        catch [exception]
        {
            LogTestDetail ($_.Exception.Message -replace "`n"," ") $true
            throw
        }
    }
}
