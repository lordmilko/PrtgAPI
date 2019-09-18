. $PSScriptRoot\..\..\Support\PowerShell\Standalone.ps1

function CreateData
{
    param(
        [Parameter(Mandatory = $false, Position = 0)]
        $Name1 = "Percent Available Memory",

        [Parameter(Mandatory = $false, Position = 1)]
        $Name2 = "Available Memory",

        [Parameter(Mandatory = $false, Position = 2)]
        $DisplayValue1 = "<1 %",

        [Parameter(Mandatory = $false, Position = 3)]
        $DisplayValue2 = "< 1 MByte",

        [Parameter(Mandatory = $false, Position = 4)]
        $Value1 = "0",

        [Parameter(Mandatory = $false, Position = 5)]
        $Value2 = "0"
    )

    $channel1 = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorHistoryChannelItem -ArgumentList @("0", $Name1, $DisplayValue1, $Value1)
    $channel2 = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorHistoryChannelItem -ArgumentList @("1", $Name2, $DisplayValue2, $Value2)

    $item2 = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", @($channel1, $channel2), "100 %", "0000010000")

    return $item2
}

function SetSensorHistoryResponse
{
    $item1 = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", $null, "100 %", "0000010000")

    $item2 = CreateData

    SetResponseAndClientWithArguments "SensorHistoryResponse" @($item2, $item1)
}

function ValidateChannels($file, $labels, $properties)
{
    $lines = gc $file.FullName
    $matchingLines = ($lines -match ".+Custom\d.+") | foreach { $_.Trim() }
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

function GetTableColumnHeader($obj, $propertyName)
{
    $properties = @($obj.PSObject.Properties)

    $propertyIndex = $null

    for($i = 0; $i -lt $properties.Length; $i++)
    {
        if($properties[$i].Name -eq $propertyName)
        {
            $propertyIndex = $i
            break
        }
    }

    if(!$propertyIndex)
    {
        throw "A property '$propertyName' could not be found on object '$obj'"
    }

    $typeName = $obj.PSObject.TypeNames[0]

    $format = Get-FormatData $typeName

    $headers = $format.FormatViewDefinition[0].Control.Headers

    $header = $headers[$propertyIndex]

    return $header.Label
}

Describe "Get-SensorHistory" -Tag @("PowerShell", "UnitTest") {

    SetSensorHistoryResponse
    
    $sensor = Run Sensor { Get-Sensor }

    $temp = [IO.Path]::GetTempPath()

    $format = "yyyy-MM-dd-HH-mm-ss"

    It "creates a custom format" {
        $path = Join-Path $temp "PrtgAPIFormats"

        $sensor | Get-SensorHistory

        (gci $path).Count | Should BeGreaterThan 0
    }

    It "specifies a custum timespan and average" {

        $sensor | Get-SensorHistory -StartDate (get-date).AddHours(-1) -EndDate (get-date).AddHours(-3) -Average 300
    }

    It "processes a ValueLookup" {
        try
        {
            $channel = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorHistoryChannelItem -ArgumentList @("0", "Backup State", "Success", "1")
            $item = New-Object PrtgAPI.Tests.UnitTests.Support.TestItems.SensorHistoryItem -ArgumentList @("22/10/2017 3:19:54 PM", "43030.1804871528", $channel, "100 %", "0000010000")

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
            [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=0")
            [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=500")
        )

        $items = $sensor | Get-SensorHistory -StartDate $start -EndDate $end

        $items.Count | Should Be 2
    }

    It "ignores display values that have multiple spaces" {
    
        $data = CreateData -Name1 "PercentAvailableMemory1" -DisplayValue1 "9 a 5 b" -Value1 "32700"

        SetResponseAndClientWithArguments "SensorHistoryResponse" $data

        $history = $sensor | Get-SensorHistory

        $timeSpanLabel = GetTableColumnHeader $history "PercentAvailableMemory1"
        $regularLabel = GetTableColumnHeader $history "AvailableMemory"

        $timeSpanLabel | Should Be "PercentAvailableMemory1"
        $regularLabel | Should Be "AvailableMemory(MByte)"

        $history.PercentAvailableMemory1 | Should Be "9 a 5 b"
    }

    It "converts display values that look like TimeSpans" {
        $data = CreateData -Name1 "PercentAvailableMemory2" -DisplayValue1 "9 h 5 m" -Value1 "32700"

        SetResponseAndClientWithArguments "SensorHistoryResponse" $data

        $history = $sensor | Get-SensorHistory

        $timeSpanLabel = GetTableColumnHeader $history "PercentAvailableMemory2"
        $regularLabel = GetTableColumnHeader $history "AvailableMemory"

        $timeSpanLabel | Should Be "PercentAvailableMemory2"
        $regularLabel | Should Be "AvailableMemory(MByte)"

        $history."PercentAvailableMemory2" | Should Be "09:05:00"
    }

    It "ignores display values that look like TimeSpans but also contain unknown units" {
        $data = CreateData -Name1 "PercentAvailableMemory3" -DisplayValue1 "9 d 5 h 3 t" -Value1 "32700"

        SetResponseAndClientWithArguments "SensorHistoryResponse" $data

        $history = $sensor | Get-SensorHistory

        $timeSpanLabel = GetTableColumnHeader $history "PercentAvailableMemory3"
        $regularLabel = GetTableColumnHeader $history "AvailableMemory"

        $timeSpanLabel | Should Be "PercentAvailableMemory3"
        $regularLabel | Should Be "AvailableMemory(MByte)"

        $history.PercentAvailableMemory3 | Should Be "9 d 5 h 3 t"
    }

    It "displays raw values" {

        $data = CreateData -Name1 "PercentAvailableMemory4" -DisplayValue1 "9 h 5 m" -Value1 "32700"

        SetResponseAndClientWithArguments "SensorHistoryResponse" $data

        $normal = $sensor | Get-SensorHistory

        $normalUnitLabel = GetTableColumnHeader $normal "AvailableMemory"
        $normalUnitLabel | Should Be "AvailableMemory(MByte)"

        $normal.PercentAvailableMemory4 | Should Be "09:05:00"
        $normal.PSObject.TypeNames[0] | Should Match "PrtgAPI.DynamicFormatPSObject\d+"

        $raw = $sensor | Get-SensorHistory -Raw

        $rawUnitLabel = GetTableColumnHeader $raw "AvailableMemory"
        $rawUnitLabel | Should Be "AvailableMemory"

        $raw.PercentAvailableMemory4 | Should Be 32700
        $raw.PSObject.TypeNames[0] | Should Match "PrtgAPI.DynamicFormatPSObject\d+"

        $normal.PSObject.TypeNames[0] | Should Not Be $raw.PSObject.TypeNames[0]
    }

    It "doesn't convert display values into TimeSpans when the display and raw value aren't roughly equivalent" {
        $data = CreateData -Name1 "PercentAvailableMemory5" -DisplayValue1 "9 h 5 m" -Value1 "132700"

        SetResponseAndClientWithArguments "SensorHistoryResponse" $data

        $history = $sensor | Get-SensorHistory

        $history.PercentAvailableMemory5 | Should Be "9 h 5 m"
    }

    Context "Count" {
        It "retrieves the specified number of records when a count is specified" {

            $start = Get-Date
            $end = $start.AddHours(-1).AddMinutes(-4)

            SetAddressValidatorResponse "historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=4&sortby=-datetime"

            $items = $sensor | Get-SensorHistory -Count 4 -StartDate $start

            $items.Count | Should Be 4
        }

        It "retrieves the specified number of records when an end date and a count is specified" {

            $start = Get-Date
            $end = $start.AddDays(-1)

            SetAddressValidatorResponse @(
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=0&sortby=-datetime")
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=500")
            )

            $items = @($sensor | Get-SensorHistory -StartDate $start -EndDate $end -Count 1)

            $items.Count | Should Be 1
        }
        
        It "adjusts the missing end date when a large count is specified" {

            $start = (Get-Date)
            $end = $start.AddHours(-3)

            $s = Run Sensor { Get-Sensor }
            $s.Interval | Should Be "00:01:00"

            SetAddressValidatorResponse @(
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=120&sortby=-datetime")
            )

            $s | Get-SensorHistory -Count 120
        }

        It "doesn't adjust the specified end date when a large count is specified" {
            
            $start = (Get-Date)
            $end = $start.AddHours(-1)
            
            $s = Run Sensor { Get-Sensor }
            $s.Interval | Should Be "00:01:00"

            SetAddressValidatorResponse @(
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=0&sortby=-datetime")
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=500")
            )

            $s | Get-SensorHistory -Count 120 -StartDate $start -EndDate $end
        }
        
        It "adjusts the end date based on an interval of 24 hours" {
            $start = (Get-Date)
            $end = $start.AddDays(-20).AddHours(-1)
            
            SetAddressValidatorResponse @(
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=20&sortby=-datetime")
            )

            $s = Run Sensor { Get-Sensor }
            $s.Interval = "01:00:00:00"
            $s.Interval | Should Be "1.00:00:00"

            $s | Get-SensorHistory -Count 20 -StartDate $start
        }
        
        It "adjusts the missing end date when a sensor ID is specified" {

            $start = (Get-Date)
            $end = $start.AddHours(-2).AddMinutes(-10)
            
            SetAddressValidatorResponse @(
                [Request]::Sensors("filter_objid=4000", [Request]::DefaultObjectFlags)
                [Request]::Get("api/historicdata.xml?id=4000&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=70&sortby=-datetime")
            )

            Get-SensorHistory -Id 4000 -Count 70 -StartDate $start
        }
        
        It "adjusts the missing end date when an average is specified" {

            $start = (Get-Date)
            $end = $start.AddHours(-7).AddMinutes(-40)
            
            SetAddressValidatorResponse @(
                [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=300&count=80&sortby=-datetime")
            )

            $s = Run Sensor { Get-Sensor }
            $s.Interval | Should Be "00:01:00"

            $s | Get-SensorHistory -Count 80 -Average 300 -StartDate $start
        }

        It "doesn't resolve the sensor ID when an average is specified" {

            $start = (Get-Date)
            $end = $start.AddHours(-6).AddMinutes(-50)

            SetAddressValidatorResponse @(
                [Request]::Get("api/historicdata.xml?id=4000&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=300&count=70&sortby=-datetime")
            )

            Get-SensorHistory -Id 4000 -Count 70 -Average 300
        }
    }
    
    It "executes when performing two stage formatting" {
        $start = Get-Date
        $end = $start.AddDays(-1)

        $response = SetAddressValidatorResponse @(
            [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&count=0&sortby=-datetime")
            [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=500")
            [Request]::Get("api/historicdata.xml?id=2203&edate=$($start.ToString($format))&sdate=$($end.ToString($format))&avg=0&sortby=-datetime&count=500&start=500")
        )

        $response.FixedCountOverride = 1000

        $items = @($sensor | Get-SensorHistory -StartDate $start -EndDate $end -Count 1000)

        $items.Count | Should Be 1000
    }

    It "replaces impure formats" {
        $dir = Join-Path $temp "PrtgAPIFormats"
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
        $post = gci $dir | Sort-Object LastWriteTime
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
    
    It "regenerates deleted formats" {
        SetMultiTypeResponse

        Get-SensorHistory -Id 1001

        $formats = Join-Path $temp "PrtgAPIFormats"

        $dir = gci $formats

        $dir.Count | Should BeGreaterThan 0

        $dir | Remove-Item -Force

        $dir = gci $formats

        $dir.Count | Should Be 0

        Get-SensorHistory -Id 1001

        $dir = gci $formats

        $dir.Count | Should BeGreaterThan 1
    }
}