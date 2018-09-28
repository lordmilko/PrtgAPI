. $PSScriptRoot\UnitTest.ps1

function Clear-Progress {

    $val = [PrtgAPI.Tests.UnitTests.Support.Progress.ProgressQueue]::ProgressSnapshots

    if($val -ne $null)
    {
        $val.Clear()
    }    
}

function Describe($name, $script)
{
    Pester\Describe $name {

        BeforeAll {
            InitializeUnitTestModules
            InitializeClient
        }
        AfterEach {
            [PrtgAPI.Tests.UnitTests.Support.Progress.ProgressQueue]::RecordQueue.Clear()
            Clear-Progress
        }
        AfterAll {
            UninitializeClient
        }

        & $script
    }
}

function Get-Progress {
    return [PrtgAPI.Tests.UnitTests.Support.Progress.ProgressQueue]::Dequeue()
}

function Assert-NoProgress($expr) {

    if($expr)
    {
        Invoke-Expression $expr
    }

    { Get-Progress } | Should Throw "Queue empty"
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

    $last = $list|Select -Last 1

    if(!($last.Contains("Completed")))
    {
        throw "Processed all records, however last record did not contain `"Completed`". PrtgAPI is not completing the specified chain properly"
    }
}

function InitializeClient {
    [PrtgAPI.Tests.UnitTests.MockProgressWriter]::Bind()

    SetMultiTypeResponse

    Enable-PrtgProgress
}

function UninitializeClient {
    [PrtgAPI.Tests.UnitTests.MockProgressWriter]::Unbind()

    Disable-PrtgProgress
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

function Gen($activity, $description, $percentage, $operation, $timeRemaining)
{
    $builder = New-Object "System.Text.StringBuilder"

    [void]$builder.Append("$activity`n")
    [void]$builder.Append("    $description")

    if($percentage -ne $null)
    {
        [void]$builder.Append("`n    $(CreateProgressBar $percentage)")
    }

    if($timeRemaining -ne $null)
    {
        [void]$builder.Append("`n    $timeRemaining remaining")
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

function GenerateStreamRecords($total)
{
    $records = @()

    for($i = 1; $i -le $total; $i++)
    {
        $maxChars = 40

        $percent = [Math]::Floor($i/$total*100)

        if($percent -ge 0)
        {
            $percentChars = [Math]::Floor($percent/100*$maxChars)

            $spaceChars = $maxChars - $percentChars

            $percentBar = ""

            for($j = 0; $j -lt $percentChars; $j++)
            {
                $percentBar += "o"
            }

            for($j = 0; $j -lt $spaceChars; $j++)
            {
                $percentBar += " "
            }

            $percentBar = "[$percentBar] ($percent%)"
        }

        $records += "PRTG Sensor Search`n" +
                    "    Retrieving all sensors $i/$total`n" +
                    "    $percentBar"
    }

    $records
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

function ValidateLastRecord($expr, $param, $primary)
{
    if($primary -eq "Last" -or $param -eq "Last")
    {
        Assert-NoProgress
    }
    elseif($primary -eq "SkipLast" -or $param -eq "SkipLast")
    {
        Assert-NoProgress
    }
    elseif($primary -eq "Index")
    {
        Assert-NoProgress
    }
    elseif($primary -eq "Skip" -and $param -eq "Skip")
    {
        Assert-NoProgress
    }
    else
    {
        $last = ""

        while($true)
        {
            try
            {
                $last = Get-Progress
            }
            catch
            {
                break
            }
        }

        if(!($last.Contains("Completed")))
        {
            throw "Last progress record of expression '$expr' did not contain 'Completed'"
        }
    }
}

function TryInvokeExpression($expr)
{
    try
    {
        Invoke-Expression $expr

        return $true
    }
    catch
    {
        if(!($_.Exception.Message.Contains("Parameter set cannot be resolved")))
        {
            throw
        }

        return $false
    }
}

#region Batch

function TestBatchWithSingle($param, $primary, $mode)
{
    TestBatchCore $param $primary $mode "Select -$primary 3 -$param 2"
}

function TestBatchWithDouble($param, $primary, $mode)
{
    TestBatchCore $param $primary $mode "Select -$primary 3 | Select -$param 2"
}

function TestBatchCore($param, $primary, $mode, $selectExpr)
{
    $expr = ""

    if($mode -eq "Cmdlet") {
        $expr = "Get-Probe -Count 10 | $selectExpr | Get-Device | Pause-Object -Forever"
    }
    elseif($mode -eq "Variable") {
        $probes = Get-Probe -Count 10

        $expr = "$('$probes') | $selectExpr | Get-Device | Pause-Object -Forever"
    }
    elseif($mode -eq "CmdletChain") {
        $expr = "Get-Probe | Get-Group -Count 10 | $selectExpr | Get-Device | Pause-Object -Forever"
    }
    elseif($mode -eq "VariableChain") {
        $probes = Get-Probe

        $expr = "$('$probes') | Get-Group -Count 10 | $selectExpr | Get-Device | Pause-Object -Forever"
    }
    else {
        throw "Unknown mode '$mode'"
    }

    if(TryInvokeExpression $expr)
    {
        Assert-NoProgress
    }
}

#endregion

function TestCmdletChainWithSingle($param, $primary, $finalCmdlet)
{
    $expr = "Get-Probe -Count 10 | Select -$primary 3 -$param 2 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr

    Assert-NoProgress
}

function TestCmdletChainWithDouble($param, $primary, $finalCmdlet)
{
    $expr = "Get-Probe -Count 10 | Select -$primary 3 | Select -$param 2 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr

    Assert-NoProgress
}

function TestVariableChainWithSingle($param, $primary, $finalCmdlet)
{
    $probes = Get-Probe -Count 10

    $expr = "$('$probes') | Select -$primary 3 -$param 2 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr

    Assert-NoProgress
}

function TestVariableChainWithDouble($param, $primary, $finalCmdlet)
{
    $probes = Get-Probe -Count 10

    $expr = "$('$probes') | Select -$primary 3 | Select -$param 2 | Get-Device | $finalCmdlet"

    Invoke-Expression $expr

    Assert-NoProgress
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