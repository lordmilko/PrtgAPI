. $PSScriptRoot\Support\Standalone.ps1

function CreateData($name1, $name2, $val1, $val2)
{
    if($name1 -eq $null)
    {
        $name1 = "Percent Available Memory"
    }

    if($name2 -eq $null)
    {
        $name2 = "Available Memory"
    }

    if($val1 -eq $null)
    {
        $val1 = "<1 %"
    }

    if($val2 -eq $null)
    {
        $val2 = "< 1 MByte"
    }

    $channel1 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryChannelItem -ArgumentList @("0", $name1, $val1, "0")
    $channel2 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryChannelItem -ArgumentList @("1", $name2, $val2, "0")

    $item2 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", @($channel1, $channel2), "100 %", "0000010000")

    return $item2
}

function SetSensorHistoryResponse
{
    $item1 = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", $null, "100 %", "0000010000")

    $item2 = CreateData

    SetResponseAndClientWithArguments "SensorHistoryResponse" @($item2, $item1)
}

function ValidateChannels($file, $labels, $properties)
{
    $lines = gc $file.FullName
    $matchingLines = ($lines -match ".+Custom\d.+").Trim()
    $matchingLines.Count | Should Be 8

    foreach($line in $matchingLines)
    {
        if($line.Contains("Label"))
        {
            if(!($labels | where { "<Label>$_</Label>" -eq $line }))
            {
                throw "Label '$line' is not considered valid"
            }
        }
        else
        {
            if(!($properties | where { "<PropertyName>$_</PropertyName>" -eq $line }))
            {
                throw "PropertyName '$line' is not considered valid"
            }
        }
    }
}

Describe "Get-SensorHistory" {

    SetSensorHistoryResponse
    
    $sensor = Run Sensor { Get-Sensor }

    $format = "yyyy-MM-dd-HH-mm-ss"

    It "creates a custom format" {
        $path = "$env:temp\PrtgAPIFormats"

        $sensor | Get-SensorHistory

        (gci $path).Count | Should BeGreaterThan 0
    }

    It "specifies a custum timespan and average" {

        $sensor | Get-SensorHistory -StartDate (get-date).AddHours(-1) -EndDate (get-date).AddHours(-3) -Average 300
    }

    It "processes a ValueLookup" {
        try
        {
            $channel = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryChannelItem -ArgumentList @("0", "Backup State", "Success", "Success")
            $item = New-Object PrtgAPI.Tests.UnitTests.ObjectTests.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", $channel, "100 %", "0000010000")

            SetResponseAndClientWithArguments "SensorHistoryResponse" $item

            $history = $sensor | Get-SensorHistory

            $history.BackupState | Should Be "Success"
        }
        finally
        {
            SetSensorHistoryResponse
        }
    }
    
    It "outputs all records when over the stream threshold" {

        $items = 1..501 | foreach { CreateData }

        $items.Count | Should Be 501

        SetResponseAndClientWithArguments "SensorHistoryResponse" $items

        $history = $sensor | Get-SensorHistory

        $history.Count | Should Be 501
    }

    It "streams when an end date is specified" {

        $start = Get-Date
        $end = $start.AddDays(-1)

        SetAddressValidatorResponse @(
            "api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=0&"
            "api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=500&"
        )

        $items = $sensor | Get-SensorHistory -StartDate $start -EndDate $end

        $items.Count | Should Be 2
    }

    It "retrieves the specified number of records when a count is specified" {

        $start = Get-Date
        $end = $start.AddHours(-1)

        SetAddressValidatorResponse "historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=4&sortby=-datetime"

        $items = $sensor | Get-SensorHistory -Count 4 -StartDate $start

        $items.Count | Should Be 4
    }

    It "retrieves the specified number of records when an end date and a count is specified" {

        $start = Get-Date
        $end = $start.AddDays(-1)

        SetAddressValidatorResponse @(
            "api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=0&sortby=-datetime&"
            "api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=500&sortby=-datetime&"
        )

        $items = @($sensor | Get-SensorHistory -StartDate $start -EndDate $end -Count 1)

        $items.Count | Should Be 1
    }

    It "replaces impure formats" {
        $dir = "$env:temp\PrtgAPIFormats"
        $pre = gci $dir

        # Use a random name to allow running test multiple times within the same process
        $random = Get-Random

        $name1 = "Custom1_$random"
        $name2 = "Custom2_$random"

        # Register a format with a channel whose value is missing
        $item = CreateData $name1 $name2 "" ""
        SetResponseAndClientWithArguments "SensorHistoryResponse" $item
        $sensor | Get-SensorHistory

        # Validate that the format was created
        $post = gci $dir
        $post.Count | Should Be ($pre.Count + 1)

        $new = $post | Select -Last 1

        # Get the created format and validate its content
        ValidateChannels $new @($name1, $name2) @($name1, $name2)

        # Attempt to update the format with a new record on the channel that does have a unit
        $item = CreateData $name1 $name2 "1 #" ""
        SetResponseAndClientWithArguments "SensorHistoryResponse" $item
        $sensor | Get-SensorHistory

        # Validate the format was updated
        ValidateChannels $new @("$name1(#)", $name2) @($name1, $name2)
    }
}pqp