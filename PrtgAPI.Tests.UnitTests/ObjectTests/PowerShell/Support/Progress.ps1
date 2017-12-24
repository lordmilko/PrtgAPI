. $PSScriptRoot\UnitTest.ps1

function Clear-Progress {

    $val = [PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress.ProgressQueue]::ProgressSnapshots

    if($val -ne $null)
    {
        $val.Clear()
    }    
}

function Describe($name, $script)
{
    Pester\Describe $name {

        BeforeAll {    InitializeUnitTestModules }
        AfterEach {
            [PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress.ProgressQueue]::RecordQueue.Clear()
            Clear-Progress
        }

        & $script
    }
}

function Get-Progress {
    return [PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.Progress.ProgressQueue]::Dequeue()
}

function Validate($list)    {

    foreach($progress in $list)
    {
        Get-Progress | Should Be $progress
    }

    try
    {
        { $result = Get-Progress; throw "`n`nProgress Queue contains more records than expected. Next record is:`n`n$result`n`n" } | Should Throw "Queue empty"
    }
    catch [exception]
    {
        Clear-Progress
        throw
    }
}

function InitializeClient {
    [PrtgAPI.Tests.UnitTests.InfrastructureTests.Support.MockProgressWriter]::Bind()

    SetMultiTypeResponse

    Enable-PrtgProgress
}

function RunCustomCount($hashtable, $action)
{
    $dictionary = GetCustomCountDictionary $hashtable

    $oldClient = Get-PrtgClient

    $newClient = [PrtgAPI.Tests.UnitTests.ObjectTests.BaseTest]::Initialize_Client((New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestResponses.MultiTypeResponse -ArgumentList $dictionary))

    try
    {
        SetPrtgClient $newClient

        & $action
    }
    catch
    {
        throw
    }
    finally
    {
        SetPrtgClient $oldClient
    }
}

function It
{
    param(
        [Parameter(Mandatory = $true, Position = 0)]
        [string]$name,

        [Parameter(Mandatory = $true, Position = 1)]
        [ScriptBlock] $script,

        [Parameter(Mandatory = $false)]
        [System.Collections.IDictionary[]] $TestCases
    )

    if($filter -eq $null -or $name -like $filter)
    {
        if($ignoreNotImplemented)
        {
            $throw = $script.Ast.FindAll({ $true }, $true) | select extent | where { $_.Extent.Text -ceq "throw" }

            if($throw -ne $null)
            {
                return
            }
        }

        if($TestCases -ne $null)
        {
            Pester\It $name $script -TestCases $TestCases
        }
        else
        {
            Pester\It $name $script
        }
    }
}

function Gen($activity, $description, $percentage, $operation)
{
    $builder = New-Object "System.Text.StringBuilder"

    [void]$builder.Append("$activity`n")
    [void]$builder.Append("    $description")

    if($percentage -ne $null)
    {
        [void]$builder.Append("`n    $(CreateProgressbar $percentage)")
    }

    if($operation -ne $null)
    {
        [void]$builder.Append("`n    $operation")
    }

    return $builder.ToString()
}

function Gen1($activity, $description, $percentage, $operation)
{
    return (Gen $activity $description $percentage $operation) + "`n"
}

function Gen2($activity, $description, $percentage, $operation)
{
    $str = Gen $activity $description $percentage $operation

    return IndentGen $str 1
}

function Gen3($activity, $description, $percentage, $operation)
{
    $str = Gen $activity $description $percentage $operation

    return "`n" + (IndentGen $str 2)
}

function IndentGen($str, $levels)
{
    $split = $str -split "`n"

    $indented = $split | foreach { "$("    " * $levels)$_" }

    $joined = $indented -join "`n"

    return $joined
}

function CreateProgressBar($percent)
{
    $maxChars = 40

    $builder = New-Object "System.Text.StringBuilder"
    $percentBar = ""

    if($percent -ge 0)
    {
        $percentChars = [Math]::Floor($percent/100*$maxChars)

        $spaceChars = $maxChars - $percentChars

        for($j = 0; $j -lt $percentChars; $j++)
        {
            [void]$builder.Append("o")
        }

        for($j = 0; $j -lt $spaceChars; $j++)
        {
            [void]$builder.Append(" ")
        }

        $percentBar = "[$($builder.ToString())] ($percent%)"
    }

    return $percentBar
}

function TestCmdletChainWithSingle($param, $primary, $finalCmdlet)
{
    $expr = "Get-Probe -Count 10 | Select -$primary 3 -$param 2 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr
}

function TestCmdletChainWithDouble($param, $primary, $finalCmdlet)
{
    $expr = "Get-Probe -Count 10 | Select -$primary 3 | Select -$param 2 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr
}

function TestVariableChainWithSingle($param, $primary, $finalCmdlet)
{
    $probes = Get-Probe -Count 10

    $expr = "$('$probes') | Select -$primary 3 -$param 3 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr
}

function TestVariableChainWithDouble($param, $primary, $finalCmdlet)
{
    $probes = Get-Probe -Count 10

    $expr = "$('$probes') | Select -$primary 3 | Select -$param 3 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr
}

$selectFirstParams = @(
    @{name = "Last"   ; i = 1}
    @{name = "Skip"    ; i = 2}
)

$selectLastParams = @(
    @{name = "First"   ; i = 1}
    @{name = "Skip"    ; i = 2}
)

$selectSkipParams = @(
    @{name = "First"   ; i = 1}
    @{name = "Last"    ; i = 2}
)

$allSelectParams = @(
    @{name = "First"   ; i = 1}
    @{name = "Skip"    ; i = 2}
    @{name = "Last"    ; i = 3}
    @{name = "SkipLast"; i = 4}
    @{name = "Index"   ; i = 5}
)