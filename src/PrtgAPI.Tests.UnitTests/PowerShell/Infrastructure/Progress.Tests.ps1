. $PSScriptRoot\..\..\Support\PowerShell\Progress.ps1

Describe "Test-Progress" -Tag @("PowerShell", "UnitTest") {
        
    $filter = "*"
    $ignoreNotImplemented = $false

    #region 1: Something -> Action
    
    It "1a: Table -> Action" {
        Get-Sensor -Count 1 | Pause-Object -Forever -Batch:$false

        Validate (@(
            (Gen "PRTG Sensor Search"               "Retrieving all sensors")
            (Gen "PRTG Sensor Search"               "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "Pausing PRTG Objects"             "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/1)" 100)
        ))
    }
    
    It "1b: Variable -> Action" {
        $devices = Get-Device

        $devices.Count | Should Be 2

        $devices | Pause-Object -Forever -Batch:$false

        Validate (@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
        ))
    }

    #endregion-
    #region 2: Something -> Table

    It "2a: Table -> Table" {
        Get-Probe | Get-Group

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }

    It "2b: Variable -> Table" {

        $probes = Get-Probe

        $probes.Count | Should Be 2

        $probes | Get-Sensor

        Validate (@(
            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all sensors")
        ))
    }

    #endregion
    #region 3: Something -> Action -> Table
    
    It "3a: Table -> Action -> Table" {

        Get-Device | Clone-Object 5678 | Get-Sensor

        Validate (@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
        ))
    }

    It "3b: Variable -> Action -> Table" {

        $devices = Get-Device

        $devices | Clone-Object 5678 | Get-Sensor

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")
        ))
    }

    #endregion
    #region 4: Something -> Table -> Table

    It "4a: Table -> Table -> Table" {

        Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

        Validate (@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)
        ))
    }
    
    It "4b: Variable -> Table -> Table" {
        $probes = Get-Probe

        #we need to find a way to detect if the entire chain we're piping along
        #originated with a variable. maybe look at the first progressrecord to see if its dodgy
        #or can we potentially dig back along the pipeline to the entry

        $probes.Count | Should Be 2

        $probes | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

    #endregion
    #region 5: Something -> Table -> Action
    
    It "5a: Table -> Table -> Action" {
        Get-Device | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "5b: Variable -> Table -> Action" {
        $devices = Get-Device

        $devices | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    #endregion
    #region 6: Something -> Table -> Action -> Table
    
    It "6a: Table -> Table -> Action -> Table" {
        Get-Group | Get-Device | Clone-Object 5678 | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "6b: Variable -> Table -> Action -> Table" {
        $probes = Get-Probe

        $probes | Get-Group -Count 1 | Clone-Object 5678 | Get-Device

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 5678 (1/1)" 100)

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 5678 (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Groups (Completed)" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 5678 (1/1)" 100 "Retrieving all devices")

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 5678 (1/1)" 100)

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 5678 (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Groups (Completed)" "Cloning group 'Windows Infrastructure0' (ID: 2000) to object ID 5678 (1/1)" 100 "Retrieving all devices")

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

    #endregion
    #region 7: Something -> Object

    It "7a: Table -> Object" {
        Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search"             "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "PRTG Sensor Search"             "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")
        ))
    }

    It "7b: Variable -> Object" {

        #1. why is pipes three data cmdlets together being infected by the crash here
        #2. why is injected_showchart failing to deserialize?

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2
        $sensors | Get-Channel

        Validate(@(
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
        ))
    }

    #endregion
    #region 8: Stream -> Something
    
    It "8a: Stream -> Object" {
        # Good enough for a test to Stream -> Table as well
        
        $counts = @{
            Sensors = 501
        }

        RunCustomCount $counts {
            Get-Sensor | Get-Channel
        }

        $records = @()
        $total = 501

        # Create progress records for processing each object

        for($i = 1; $i -le $total; $i++)
        {
            $percent = [Math]::Floor($i/$total*100)

            $percentBar = CreateProgressBar $percent

            $records += "PRTG Sensor Search`n" +
                        "    Processing sensor 'Volume IO _Total$($i - 1)' (ID: $(4000 + $i - 1)) ($i/$total)`n" +
                        "    $percentBar`n" +
                        "    Retrieving all channels"
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/501)" 0)

            $records

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total500' (ID: 4500) (501/501)" 100 "Retrieving all channels")
        ))
    }
    
    It "8b: Stream -> Action" {

        # Besides the initial "Detecting total number of items", there is nothing special about a streamed, non-streamed and streaming-unsupported (e.g. devices) run

        $counts = @{
            Sensors = 501
        }

        RunCustomCount $counts {
            Get-Sensor | Pause-Object -Forever -Batch:$false
        }

        $records = @()
        $total = 501

        # Create progress records for processing each object

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

            $nameSuffix = $i - 1

            $records += "Pausing PRTG Objects`n" +
                        "    Pausing sensor 'Volume IO _Total$nameSuffix' (ID: $(4000 + $i - 1)) forever ($i/$total)`n" +
                        "    $percentBar"
        }

        #todo: maybe try replace the original stream one with this

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Detecting total number of items"

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/501)" 0)

            $records

            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total500' (ID: 4500) forever (501/501)" 100)
        ))
    }

    #endregion
    #region 9: Something -> Table -> Object

    It "9a: Table -> Table -> Object" {

        $counts = @{
            Sensors = 1
        }

        RunCustomCount $counts {
            Get-Device | Get-Sensor | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "9b: Variable -> Table -> Object" {
        $probes = Get-Probe

        $counts = @{
            Sensors = 1
        }

        RunCustomCount $counts {
            $probes | Get-Sensor | Get-Channel
        }

        Validate (@(
            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

    #endregion
    #region 10: Something -> Action -> Table -> Table
    
    It "10a: Table -> Action -> Table -> Table" {
        Get-Device | Clone-Object 5678 | Get-Sensor | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
        ))
    }

    It "10b: Variable -> Action -> Table -> Table" {
        # an extension of 3b. variable -> action -> table. Confirms that we can transform our setpreviousoperation into a
        # proper progress item when required

        #RunCustomCount $counts {
            $devices = Get-Device

            $devices | Clone-Object 5678 | Get-Sensor | Get-Channel        
        #}

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")
            
            ###################################################################

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            
            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")            

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            
            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
        ))
    }
    
    #endregion
    #region 11: Variable -> Table -> Table -> Table

    It "11: Variable -> Table -> Table -> Table" {
        # Validates we can get at least two progress bars out of a variable
        $probes = Get-Probe

        $probes | Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            ###################################################################

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            ###################################################################

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

    #endregion
    #region 12: Something -> Select -First -> Something
        #region 12.1: Something -> Select -First -> Something

    It "12.1a: Table -> Select -First -> Table" {
        Get-Probe -Count 3 | Select -First 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1b: Table -> Select -First -> Action" {
        Get-Probe -Count 3 | Select -First 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                     33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' (ID: 1000) forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
        ))
    }

    It "12.1c: Variable -> Select -First -> Table" {

        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1d: Variable -> Select -First -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' (ID: 1000) forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
        ))
    }

    It "12.1e: Table -> Select -First -Wait -> Table" {
        Get-Probe -Count 3 | Select -First 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1f: Table -> Select -First -Wait -> Action" {
        Get-Probe -Count 3 | Select -First 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                     33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' (ID: 1000) forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
        ))
    }

    It "12.1g: Variable -> Select -First -Wait -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1h: Variable -> Select -First -Wait -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' (ID: 1000) forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/3)" 66)
        ))
    }

        #endregion
        #region 12.2: Something -> Select -First -> Table -> Something
    
    It "12.2a: Table -> Select -First -> Table -> Table" {
        Get-Probe -Count 3 | Select -First 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)"        33)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)"        33 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)"       33) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/2)"      50)

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)"       33) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/2)"      50 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)"       33) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device1' (ID: 3001) (2/2)"      100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/3)"       33) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)"        66)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)"        66 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)"       66) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/2)"      50)

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)"       66) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/2)"      50 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)"       66) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device1' (ID: 3001) (2/2)"      100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/3)"       66) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.11' (ID: 1001) (2/3)"        66)
        ))
    }

    It "12.2b: Table -> Select -First -> Table -> Action" {
        Get-Probe -Count 3 | Select -First 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                         33)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                         33 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                         33) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)"                        50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                         33) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                         33) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (ID: 1000) (1/3)"                         33) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)"                        50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################
            
            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search (Completed)"         "Processing probe '127.0.0.11' (ID: 1001) (2/3)"                         66)
        ))
    }

    It "12.2c: Variable -> Select -First -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"        "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)"  50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)"  100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)"  100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"        "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)"  50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)"  100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)"  100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66)
        ))
    }

    It "12.2d: Variable -> Select -First -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"        "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)"  50)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"  100)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"  100)

            ###################################################################

            (Gen "PRTG Device Search"        "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)"  50)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"  100)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"  100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66)
        ))
    }

        #endregion
        #region 12.3: Table -> Table -> Select -First -> Table -> Something

    It "12.3a: Table -> Table -> Select -First -> Table" {
        Get-Probe | Get-Group | Select -First 3 | Get-Device

        Assert-NoProgress
    }

    It "12.3b: Table -> Table -> Table -> Select -First -> Table" {
        Get-Probe | Get-Group | Get-Device | Select -First 3 | Get-Sensor

        Assert-NoProgress
    }

        #endregion
        #region 12.4: Table -> Select -First -Something -> Something

    It "12.4a: Table -> Select -First -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 -Last 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 -Skip 2 | Get-Device"
    }
    
    It "12.4b: Table -> Select -First -> Select -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 | Select -Last 2     | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 | Select -Skip 2     | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 | Select -SkipLast 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 5 | Select -Index 2,4  | Get-Device"
    }

    It "12.4c: 12.4: Table -> Select -First -Something -> Action -Batch`$false" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 -Last 2 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 -Skip 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "12.4d: 12.5: Table -> Select -First -> Select -Something -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 | Select -Last 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 | Select -Skip 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -First 5 | Select -Index 2,4  | Pause-Object -Forever -Batch:`$false"
    }

    It "12.4e: Variable -> Select -First -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 -Last 2 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 -Skip 2 | Get-Device"
    }

    It "12.4f: Variable -> Select -First -> Select -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 | Select -Last 2     | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 | Select -Skip 2     | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 | Select -SkipLast 2 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 5 | Select -Index 2,4  | Get-Device"
    }

    It "12.4g: Variable -> Select -First -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 -Last 2 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 -Skip 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "12.4h: Variable -> Select -First -> Select -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 | Select -Last 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 | Select -Skip 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -First 5 | Select -Index 2,4  | Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 12.5: Something -> Select -First -Something -> Table -> Something

    It "12.5a<i>: Table -> Select -First -<name> -> Table -> Table" -TestCases $selectFirstParams {
        param($name)

        TestCmdletChainWithSingle $name "First" "Get-Sensor"
    }

    It "12.5b<i>: Table -> Select -First -<name> -> Table -> Action" -TestCases $selectFirstParams {
        param($name)

        TestCmdletChainWithSingle $name "First" "Pause-Object -Forever -Batch:`$false"
    }

    It "12.5c<i>: Variable -> Select -First -<name> -> Table -> Table" -TestCases $selectFirstParams {
        param($name)

        TestVariableChainWithSingle $name "First" "Get-Sensor"
    }

    It "12.5d<i>: Variable -> Select -First -<name> -> Table -> Action" -TestCases $selectFirstParams {
        param($name)

        TestVariableChainWithSingle $name "First" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 12.6: Something -> Select -First -> Select -Something -> Table -> Something

    It "12.6a<i>: Table -> Select -First -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "First" "Get-Sensor"
    }

    It "12.6b<i>: Table -> Select -First -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "First" "Pause-Object -Forever -Batch:`$false"
    }

    It "12.6c<i>: Variable -> Select -First -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "First" "Get-Sensor"
    }

    It "12.6d<i>: Variable -> Select -First -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "First" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 12.7: Something -> Select

    It "12.7a: Stream -> Select -First" {

        RunCustomCount @{ Sensors = 501 } {
            Get-Sensor | Select -First 1
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (Gen "PRTG Sensor Search" "Retrieving all sensors (1/501)" 0)
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors (1/501)" 0)
        ))
    }

    It "12.7b: Variable -> Table -> Select -First" {
        $probes = Get-Probe

        $probes | Get-Group | Select -First 1

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
        ))
    }

    It "12.7c: Variable -> Action -> Select -First" {

        $devices = Get-Device -Count 5

        $devices | Clone-Object 5678 | Select -First 2

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/5)" 20)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/5)" 40)
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/5)" 40)
        ))
    }

        #endregion
    #endregion
    #region 13: Something -> Select -Last -> Something
        #region 13.1: Something -> Select -Last -> Something

    It "13.1a: Table -> Select -Last -> Table" {
        Get-Probe -Count 4 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (ID: 1000) (1/4)"       25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (ID: 1001) (2/4)"       50)
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (ID: 1003) (4/4)"       100)

            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1b: Table -> Select -Last -> Action" {
        Get-Probe -Count 4 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)
            (Gen "PRTG Probe Search (Completed)"    "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
        ))
    }

    It "13.1c: Variable -> Select -Last -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1d: Variable -> Select -Last -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
        ))
    }

    It "13.1e: Table -> Select -Last -Wait -> Table" {
        Get-Probe -Count 4 | Select -Last 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (ID: 1000) (1/4)"       25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (ID: 1001) (2/4)"       50)
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (ID: 1003) (4/4)"       100)

            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1f: Table -> Select -Last -Wait -> Action" {
        Get-Probe -Count 4 | Select -Last 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (ID: 1000) (1/4)"                     25)
            (Gen "PRTG Probe Search (Completed)"    "Processing probe '127.0.0.13' (ID: 1003) (4/4)"                     100)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
        ))
    }

    It "13.1g: Variable -> Select -Last -Wait -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (ID: 1003) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1h: Variable -> Select -Last -Wait -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (2/2)" 100)
        ))
    }

        #endregion
        #region 13.2: Something -> Select -Last -> Table -> Something

    It "13.2a: Table -> Select -Last -> Table -> Table" {
        Get-Probe -Count 3 | Select -Last 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)"             33)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"       50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"      50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"      50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"      50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"       100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"      100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"      100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"      100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100)
        ))
    }

    It "13.2b: Table -> Select -Last -> Table -> Action" {
        Get-Probe -Count 3 | Select -Last 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)"             33)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"       50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"      50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"      50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)"      50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"       100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"      100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"      100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"      100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (2/2)"       100)
        ))
    }

    It "13.2c: Variable -> Select -Last -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -Last 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100)
        ))
    }

    It "13.2d: Variable -> Select -Last -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -Last 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (2/2)" 100)
        ))
    }

        #endregion
        #region 13.3: Table -> Table -> Select -Last -> Table

    It "13.3a: Table -> Table -> Select -Last -> Table" {
        Get-Probe | Get-Group | Select -Last 4 | Get-Device

        Assert-NoProgress
    }

    It "13.3b: Table -> Table -> Table -> Select -Last -> Table" {
        Get-Probe | Get-Group | Get-Device | Select -Last 4 | Get-Sensor

        Assert-NoProgress
    }

        #endregion
        #region 13.4: Something -> Select -First -Something -> Something

    It "13.4a: Table -> Select -Last -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 2 -First 4 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 2 -Skip 2 | Get-Device"
    }

    It "13.4b: Table -> Select -Last -> Select -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -First 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -Skip 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -SkipLast 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -Index 2 | Get-Device"
    }

    It "13.4c: Table -> Select -Last -First -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 2 -First 4 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 2 -Skip 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "13.4d: Table -> Select -Last -> Select -Something -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -First 2    | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -Skip 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Last 4 | Select -Index 2    | Pause-Object -Forever -Batch:`$false"
    }

    It "13.4e: Variable -> Select -Last -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 2 -First 4 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 2 -Skip 2 | Get-Device"
    }

    It "13.4f: Variable -> Select -Last -> Select -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -First 2 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -Skip 2 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -SkipLast 2 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -Index 2 | Get-Device"
    }

    It "13.4g: Variable -> Select -Last -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 2 -First 4 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 2 -Skip 2  | Pause-Object -Forever -Batch:`$false"
    }

    It "13.4h: Variable -> Select -Last -> Select -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -First 2    | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -Skip 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Last 4 | Select -Index 2    | Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 13.5: Something -> Select -Last -Something -> Table -> Something

    It "13.5a<i>: Table -> Select -Last -<name> -> Table -> Table" -TestCases $selectLastParams {
        param($name)

        TestCmdletChainWithSingle $name "Last" "Get-Sensor"
    }

    It "13.5b<i>: Table -> Select -Last -<name> -> Table -> Action" -TestCases $selectLastParams {
        param($name)

        TestCmdletChainWithSingle $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

    It "13.5c<i>: Variable -> Select -Last -<name> -> Table -> Table" -TestCases $selectLastParams {
        param($name)

        TestVariableChainWithSingle $name "Last" "Get-Sensor"
    }

    It "13.5d<i>: Variable -> Select -Last -<name> -> Table -> Action" -TestCases $selectLastParams {
        param($name)

        TestVariableChainWithSingle $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 13.6: Something -> Select -Last -> Select -Something -> Table -> Something

    It "13.6a<i>: Table -> Select -Last -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Last" "Get-Sensor"
    }

    It "13.6b<i>: Table -> Select -Last -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

    It "13.6c<i>: Variable -> Select -Last -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Last" "Get-Sensor"
    }

    It "13.6d<i>: Variable -> Select -Last -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 13.7: Something -> Select -Last

    It "13.7a: Stream -> Select -Last" {

        RunCustomCount @{ Sensors = 501 } {
            Get-Sensor | Select -Last 1
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            ((GenerateStreamRecords 501) | Select -SkipLast 1)
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors (501/501)" 100)
        ))
    }

    It "13.7b: Variable -> Select -Last" {
        $probes = Get-Probe

        $probes | Get-Group | Select -Last 1

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }

    It "13.7c: Variable -> Action -> Select -Last" {

        $devices = Get-Device -Count 5

        $devices | Clone-Object 5678 | Select -Last 2

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/5)" 20)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/5)" 40)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) to object ID 5678 (3/5)" 60)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/5)" 80)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device4' (ID: 3004) to object ID 5678 (5/5)" 100)
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device4' (ID: 3004) to object ID 5678 (5/5)" 100)
        ))
    }

        #endregion
    #endregion
    #region 14: Something -> Select -Skip -> Something
        #region 14.1: Something -> Select -Skip -> Something

    It "14.1a: Table -> Select -Skip -> Table" {
        Get-Probe -Count 4 | Select -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1b: Table -> Select -Skip -> Action" {
        Get-Probe -Count 4 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
        ))
    }

    It "14.1c: Variable -> Select -Skip -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1d: Variable -> Select -Skip -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
        ))
    }

    It "14.1e: Table -> Select -Skip -Wait -> Table" {
        Get-Probe -Count 4 | Select -Skip 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1f: Table -> Select -Skip -Wait -> Action" {
        Get-Probe -Count 4 | Select -Skip 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
        ))
    }

    It "14.1h: Variable -> Select -Skip -Wait -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (ID: 1003) (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1g: Variable -> Select -Skip -Wait -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' (ID: 1003) forever (4/4)" 100)
        ))
    }

        #endregion
        #region 14.2: Something -> Select -Skip -> Table -> Something
   
    It "14.2a: Table -> Select -Skip -> Table -> Table" {
        Get-Probe -Count 3 | Select -Skip 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/3)"             33)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66 "Retrieving all devices")
            
            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (ID: 3000) (1/2)"            50)

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (ID: 3000) (1/2)"            50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device1' (ID: 3001) (2/2)"            100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "PRTG Device Search (Completed)"             "Processing device 'Probe Device1' (ID: 3001) (2/2)"            100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100 "Retrieving all devices")

            ###################################################################
            
            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (ID: 3000) (1/2)"            50)

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (ID: 3000) (1/2)"            50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device1' (ID: 3001) (2/2)"            100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)"            100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)"       "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100)
        ))
    }

    It "14.2b: Table -> Select -Skip -> Table -> Action" {
        Get-Probe -Count 3 | Select -Skip 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (ID: 1000) (1/3)"             33)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"            100)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/3)"             66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"            100)

            ###################################################################

            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"            100)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)"            100)

            (Gen "PRTG Probe Search (Completed)"         "Processing probe '127.0.0.12' (ID: 1002) (3/3)"             100)
        ))
    }

    It "14.2c: Variable -> Select -Skip -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -Skip 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"   66 "Retrieving all devices")
            
            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"  66) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"  66) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"  66) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"                    "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100)
        ))
    }

    It "14.2d: Variable -> Select -Skip -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -Skip 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"   66 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"  66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"  66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/3)"  66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"   100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (ID: 1002) (3/3)"  100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)"         "Processing probe '127.0.0.12' (ID: 1002) (3/3)"   100)
        ))
    }

        #endregion
        #region 14.3: Table -> Table -> Select -Skip -> Table

    It "14.3a: Table -> Table -> Select -Skip -> Table" {
        Get-Probe | Get-Group -Count 3 | Select -Skip 4 | Get-Device

        Assert-NoProgress
    }

    It "14.3b: Table -> Table -> Table -> Select -Skip -> Table" {
        Get-Probe | Get-Group | Get-Device | Select -Skip 6 | Get-Sensor

        Assert-NoProgress
    }

        #endregion
        #region 14.4: Table -> Select -Skip -Something -> Something

    It "14.4a: Table -> Select -Skip -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 -First 4 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 -Last 2 | Get-Device"
    }

    It "14.4b: Table -> Select -Skip -> Select -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -First 4 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -Last 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -SkipLast 3 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -Index 1,3 | Get-Device"
    }

    It "14.4c: Table -> Select -Skip -Something -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 -First 4 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 -Last 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "14.4d: Table -> Select -Skip -> Select -Something -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -First 4    | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -Last 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -SkipLast 3 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Skip 2 | Select -Index 1,3  | Pause-Object -Forever -Batch:`$false"
    }

    It "14.4e: Variable -> Select -Skip -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 -First 4 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 -Last 2 | Get-Device"
    }

    It "14.4f: Variable -> Select -Skip -> Select -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -First 4 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -Last 2 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -SkipLast 3 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -Index 1,3 | Get-Device"
    }

    It "14.4g: Variable -> Select -Skip -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 -First 4 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 -Last 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "14.4h: Variable -> Select -Skip -> Select -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -First 4     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -Last 2      | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10;` `$probes | Select -Skip 2 | Select -SkipLast 3 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Skip 2 | Select -Index 1,3   | Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 14.5: Something -> Select -Skip -Something -> Table -> Something

    It "14.5a<i>: Table -> Select -Skip -<name> -> Table -> Table" -TestCases $selectSkipParams {
        param($name)

        TestCmdletChainWithSingle $name "Skip" "Get-Sensor"
    }

    It "14.5b<i>: Table -> Select -Skip -<name> -> Table -> Action" -TestCases $selectSkipParams {
        param($name)

        TestCmdletChainWithSingle $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

    It "14.5c<i>: Variable -> Select -Skip -<name> -> Table -> Table" -TestCases $selectSkipParams {
        param($name)

        TestVariableChainWithSingle $name "Skip" "Get-Sensor"
    }

    It "14.5d<i>: Variable -> Select -Skip -<name> -> Table -> Action" -TestCases $selectSkipParams {
        param($name)

        TestVariableChainWithSingle $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 14.6: Something -> Select -Skip -> Select -Something -> Table -> Something

    It "14.6a<i>: Table -> Select -Skip -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Skip" "Get-Sensor"
    }

    It "14.6b<i>: Table -> Select -Skip -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

    It "14.6c<i>: Variable -> Select -Skip -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Skip" "Get-Sensor"
    }

    It "14.6d<i>: Variable -> Select -Skip -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 14.7: Something -> Select -Skip

    It "14.7a: Stream -> Select -Skip" {

        RunCustomCount @{ Sensors = 501 } {
            Get-Sensor | Select -Skip 1
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (GenerateStreamRecords 501)
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors (501/501)" 100)
        ))
    }

    It "14.7b: Variable -> Table -> Select -Skip" {
        $probes = Get-Probe

        $probes | Get-Group | Select -Skip 1

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }

    It "14.7c: Variable -> Action -> Select -Skip" {

        $devices = Get-Device -Count 5

        $devices | Clone-Object 5678 | Select -Skip 2

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/5)" 20)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/5)" 40)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) to object ID 5678 (3/5)" 60)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/5)" 80)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device4' (ID: 3004) to object ID 5678 (5/5)" 100)
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device4' (ID: 3004) to object ID 5678 (5/5)" 100)
        ))
    }

        #endregion
    #endregion
    #region 15: Something -> Select -SkipLast -> Something
        #region 15.1: Something -> Select -SkipLast -> Something

    It "15.1a: Table -> Select -SkipLast -> Table" {
        Get-Probe -Count 4 | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50  "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100 "Retrieving all devices")
        ))
    }

    It "15.1b: Table -> Select -SkipLast -> Action" {
        Get-Probe -Count 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (ID: 1000) (1/4)" 25)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
        ))
    }

    It "15.1c: Variable -> Select -SkipLast -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "15.1d: Variable -> Select -SkipLast -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
        ))
    }

        #endregion
        #region 15.2: Something -> Select -SkipLast -> Table -> Something

    It "15.2a: Table -> Select -SkipLast -> Table -> Table" {
        Get-Probe -Count 3 | Select -SkipLast 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100)
        ))
    }

    It "15.2b: Table -> Select -SkipLast -> Table -> Action" {
        Get-Probe -Count 3 | Select -SkipLast 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (ID: 1000) (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            
            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66) +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)"  100)
        ))
    }

    It "15.2c: Variable -> Select -SkipLast -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -SkipLast 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

    It "15.2d: Variable -> Select -SkipLast -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -SkipLast 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

        #endregion
        #region 15.3: Table -> Table -> Select -SkipLast -> Table

    It "15.3a: Table -> Table -> Select -SkipLast -> Table" {
        Get-Probe | Get-Group | Select -SkipLast 4 | Get-Device

        Assert-NoProgress
    }

    It "15.3b: Table -> Table -> Table -> Select -SkipLast -> Table" {
        Get-Probe | Get-Group | Get-Device | Select -SkipLast 4 | Get-Sensor

        Assert-NoProgress
    }

        #endregion
        #region 15.4: Table -> Select -SkipLast -> Select -Something -> Something

    It "15.4a: Table -> Select -SkipLast -> Select -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -First 4 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -Last 4 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -Skip 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -Index 2 | Get-Device"
    }

    It "15.4b: Table -> Select -SkipLast -> Select -Something -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -First 4 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -Last 4  | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -Skip 2  | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -SkipLast 2 | Select -Index 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "15.4c: Variable -> Select -SkipLast -> Select -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -First 4 | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -Last 4  | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -Skip 2  | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -Index 2 | Get-Device"
    }

    It "15.4d: Variable -> Select -SkipLast -> Select -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -First 4 | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -Last 4  | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -Skip 2  | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -SkipLast 2 | Select -Index 2 | Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 15.5: Something -> Select -SkipLast -> Select -Something -> Table -> Something

    It "15.5a<i>: Table -> Select -SkipLast -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "SkipLast" "Get-Sensor"
    }

    It "15.5b<i>: Table -> Select -SkipLast -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "SkipLast" "Pause-Object -Forever -Batch:`$false"
    }

    It "15.5c<i>: Variable -> Select -SkipLast -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "SkipLast" "Get-Sensor"
    }

    It "15.5d<i>: Variable -> Select -SkipLast -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "SkipLast" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 15.6: Something -> Select -SkipLast

    It "15.6a: Stream -> Select -SkipLast" {
        RunCustomCount @{ Sensors = 501 } {
            Get-Sensor | Select -SkipLast 1
        }
        
        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            ((GenerateStreamRecords 501) | Select -SkipLast 2)
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors (500/501)" 99)
        ))
    }

    It "15.6b: Variable -> Table -> Select -SkipLast" {
        $probes = Get-Probe

        $probes | Get-Group | Select -SkipLast 1

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }

    It "15.6c: Variable -> Action -> Select -SkipLast" {

        $devices = Get-Device -Count 5

        $devices | Clone-Object 5678 | Select -SkipLast 2

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/5)" 20)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/5)" 40)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) to object ID 5678 (3/5)" 60)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/5)" 80)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device4' (ID: 3004) to object ID 5678 (5/5)" 100)
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device4' (ID: 3004) to object ID 5678 (5/5)" 100)
        ))
    }

        #endregion
    #endregion
    #region 16: Something -> Select -Index -> Something
        #region 16.1: Something -> Select -Index -> Something

    It "16.1a: Table -> Select -Index -> Table" {
        Get-Probe -Count 4 | Select -Index 1,2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75 "Retrieving all devices")
        ))
    }

    It "16.1b: Table -> Select -Index -> Action" {
        Get-Probe -Count 4 | Select -Index 1,2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
        ))
    }

    It "16.1c: Variable -> Select -Index -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75  "Retrieving all devices")
        ))
    }

    It "16.1d: Variable -> Select -Index -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
        ))
    }

    It "16.1e: Table -> Select -Index -Wait -> Table" {
        Get-Probe -Count 4 | Select -Index 1,2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75 "Retrieving all devices")
        ))
    }

    It "16.1f: Table -> Select -Index -Wait -> Action" {
        Get-Probe -Count 4 | Select -Index 1,2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
        ))
    }

    It "16.1g: Variable -> Select -Index -Wait -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (ID: 1001) (2/4)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/4)" 75  "Retrieving all devices")
        ))
    }

    It "16.1h: Variable -> Select -Index -Wait -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' (ID: 1001) forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' (ID: 1002) forever (3/4)" 75)
        ))
    }

        #endregion
        #region 16.2: Something -> Select -Index -> Table -> Something

    It "16.2a: Table -> Select -Index -> Table -> Table" {
        Get-Probe -Count 5 | Select -Index 1,3 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40)
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80)
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            
            ###################################################################

            (Gen "PRTG Probe Search (Completed)"     "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80)
        ))
    }

    It "16.2b: Table -> Select -Index -> Table -> Action" {
        Get-Probe -Count 5 | Select -Index 1,3 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40)
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search"                 "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80)
            (Gen "PRTG Probe Search"                 "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Device Search"           "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search (Completed)"     "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80)
        ))
    }

    It "16.2c: Variable -> Select -Index -> Table -> Table" {
        $probes = Get-Probe -Count 5

        $probes | Select -Index 1,3 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"                  "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"                  "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)"      "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80)
        ))
    }

    It "16.2d: Variable -> Select -Index -> Table -> Action" {
        $probes = Get-Probe -Count 5

        $probes | Select -Index 1,3 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"                    "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (ID: 1001) (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"                    "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.13' (ID: 1003) (4/5)" 80)
        ))
    }

        #endregion
        #region 16.3: Table -> Table -> Select -Index -> Table

    It "16.3a: Table -> Table -> Select -Index -> Table" {
        Get-Probe | Get-Group -Count 10 | Select -Index 2 | Get-Device

        Assert-NoProgress
    }

    It "16.3b: Table -> Table -> Table -> Select -Index -> Table" {
        Get-Probe | Get-Group | Get-Device -Count 3 | Select -Index 4 | Get-Sensor

        Assert-NoProgress
    }

        #endregion
        #region 16.4: Table -> Select -Index -> Select -Something -> Something

    It "16.4a: Table -> Select -Index -> Select -Something -> Table" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -First 3 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Last 3 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Skip 2 | Get-Device"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Get-Device"
    }

    It "16.4b: Table -> Select -Index -> Select -Something -> Action" {
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -First 3    | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Last 3     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Skip 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Pause-Object -Forever -Batch:`$false"
    }

    It "16.4c: Variable -> Select -Index -> Select -Something -> Table" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -First 3    | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -Last 3     | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -Skip 2     | Get-Device"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Get-Device"
    }

    It "16.4d: Variable -> Select -Index -> Select -Something -> Action" {
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -First 3    | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -Last 3     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -Skip 2     | Pause-Object -Forever -Batch:`$false"
        Assert-NoProgress "`$probes = Get-Probe -Count 10; `$probes | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 16.5: Something -> Select -Index -> Select -Something -> Table -> Something

    It "16.5a<i>: Table -> Select -Index -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Index" "Get-Sensor"
    }

    It "16.5b<i>: Table -> Select -Index -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Index" "Pause-Object -Forever -Batch:`$false"
    }

    It "16.5c<i>: Variable -> Select -Index -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Index" "Get-Sensor"
    }

    It "16.5d<i>: Variable -> Select -Index -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Index" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 16.6: Something -> Select -SkipLast

    It "16.6a: Stream -> Select -Index" {

        RunCustomCount @{ Sensors = 501 } {
            Get-Sensor | Select -Index 0
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (Gen "PRTG Sensor Search" "Retrieving all sensors (1/501)" 0)
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors (1/501)" 0)
        ))
    }

    It "16.6b: Variable -> Table -> Select -Index" {
        $probes = Get-Probe

        $probes | Get-Group | Select -Index 0

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
        ))
    }

    It "16.6c: Variable -> Action -> Select -Index" {

        $devices = Get-Device -Count 5

        $devices | Clone-Object 5678 | Select -Index 1,3

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/5)" 20)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/5)" 40)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) to object ID 5678 (3/5)" 60)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/5)" 80)
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/5)" 80)
        ))
    }

        #endregion
    #endregion
    #region 17: Something -> Where -> Something
    
    It "17a: Table -> Where -> Table" {

        $counts = @{
            Probes = 3
        }

        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
        ))
    }
    
    It "17b: Variable -> Where -> Table" {
        $counts = @{
            Probes = 3
        }
        
        $probes = RunCustomCount $counts {
            Get-Probe
        }

        $probes.Count | Should Be 3

        $probes | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
        ))
    }

    It "17c: Table -> Where -> Action" {
        
        $counts = @{
            Probes = 3
        }

        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Pause-Object -Forever -Batch:$false
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/3)" 33)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.12' (ID: 1002) forever (3/3)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' (ID: 1002) forever (3/3)" 100)
        ))
    }

    It "17d: Table -> Where (`$false) -> Table" {

        Get-Device -Count 3 | where { $false } | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
        ))
    }

    It "17e: Table -> Where (`$false) -> Action" {
        Get-Device -Count 3 | where { $false } | Pause-Object -Forever -Batch

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
        ))
    }

    It "17f: Variable -> Table -> Where (`$false) -> Table" {

        $probes = Get-Probe -Count 3

        $probes | Get-Device | where { $false } | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
        ))
    }

    It "17g: Variable -> Table -> Where (`$false) -> Action" {

        $probes = Get-Probe -Count 3

        $probes | Get-Device | where { $false } | Pause-Object -Forever -Batch

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")
        ))
    }

    #endregion
    #region 18: Something -> Table -> Where -> Table
    
    It "18a: Table -> Table -> Where -> Table" {

        Get-Probe | Get-Group | where name -EQ "Windows Infrastructure0" | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

    It "18b: Variable -> Table -> Where -> Table" {
        $probes = Get-Probe

        $probes | Get-Group | where name -like * | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }
    
    #endregion
    #region 19: Something -> Where -> Something -> Something

    It "19a: Table -> Where -> Table -> Table" {
        $counts = @{
            Probes = 3
        }
        
        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device | Get-Sensor
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100)
        ))
    }

    It "19b: Variable -> Where -> Table -> Table" {
        $counts = @{
            Probes = 3
        }
        
        $probes = RunCustomCount $counts {
            Get-Probe
        }

        $probes.Count | Should Be 3

        $probes | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (ID: 1002) (3/3)" 100)
        ))
    }
    
    #endregion
    #region 20: Variable(1) -> Table -> Table

    It "20: Variable(1) -> Table -> Table" {

        $probe = Get-Probe -Count 1

        $probe.Count | Should Be 1

        $probe | Get-Group | Get-Device

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100)
        ))
    }

    #endregion
    #region 21: Something -> PSObject
    
    It "21a: Table -> PSObject" {
        Get-Device | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all notification trigger types")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")
        ))
    }

    It "21b: Variable -> PSObject" {
        $devices = Get-Device

        $devices | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Notification Trigger Type Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all notification trigger types")
            (Gen "PRTG Notification Trigger Type Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")
            (Gen "PRTG Notification Trigger Type Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")
        ))
    }

    #endregion
    #region 22: Something -> Table -> PSObject

    It "22a: Table -> Table -> PSObject" {
        Get-Group | Get-Device | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "22b: Variable -> Table -> PSObject" {
        $groups = Get-Group

        $groups | Get-Device | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Notification Trigger Type Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Notification Trigger Type Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }
    
    #endregion
    #region 23: Something -> Where { Variable(1) -> Table }

    It "23a: Table -> Where { Variable(1) -> Table }" {
        Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
        ))
    }

    It "23b: Variable -> Where { Variable(1) -> Table }" {
        $probes = Get-Probe

        $probes | where { $_ | Get-Sensor }

        Assert-NoProgress
    }
    
    #endregion
    #region 24: Something -> Table -> Where { Variable(1) -> Table }

    It "24a: Table -> Table -> Where { Variable(1) -> Table }" {
        Get-Probe | Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "24b: Variable -> Table -> Where { Variable(1) -> Table }" {
        $probes = Get-Probe

        $probes | Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    #endregion
    #region 25: Something -> Where { Table -> Table }

    It "25a: Table -> Where { Table -> Table }" {
        Get-Probe | where { Get-Device | Get-Sensor }

        Validate (@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
        ))
    }

    It "25b: Variable -> Where { Table -> Table }" {
        $probes = Get-Probe

        $probes | where { Get-Device | Get-Sensor }

        Validate (@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
        ))
    }
    
    #endregion
    #region 26: Something -> Where { Variable(1) -> Where { Variable(1) -> Table } }

    It "26a: Table -> Where { Variable(1) -> Where { Variable(1) -> Table } }" {
        Get-Probe | where {
            ($_ | Get-Device | where {
                ($_|Get-Sensor).Name -eq "Volume IO _Total0"
            }).Name -eq "Probe Device0"
        }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all devices")
                (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all devices")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (1/1)" 100 "Retrieving all devices")
                (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

                (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (1/1)" 100 "Retrieving all devices")
        ))
    }

    It "26b: Variable -> Where { Variable(1) -> Where { Variable -> Table } }" {
        $probes = Get-Probe

        $probes | where {
            ($_ | Get-Device | where {
                ($_|Get-Sensor).Name -eq "Volume IO _Total0"
            }).Name -eq "Probe Device0"
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
        ))
    }

    #endregion
    #region 27: Something -> Where { Variable(1) -> Table } -> Table

    It "27a: Table -> Where { Variable(1) -> Table } -> Table" {
        Get-Probe | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" } | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
                (Gen "PRTG Sensor Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
                (Gen "PRTG Sensor Search" "Processing probe '127.0.0.11' (ID: 1001) (1/1)" 100 "Retrieving all sensors")
                (Gen "PRTG Sensor Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "27b: Variable -> Where { Variable(1) -> Table } -> Table" {
        $probes = Get-Probe

        $probes | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" } | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    #endregion
    #region 28: Something -> Table -> Where { Variable(1) -> Table -> Table }
    
    It "28a: Table -> Table -> Where { Variable(1) -> Table -> Table }" {
        Get-Probe | Get-Group | where {
            ($_ | Get-Device | Get-Sensor).Name -eq "Volume IO _Total0"
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }
    
    It "28b: Variable -> Table -> Where { Variable(1) -> Table -> Table }" {
        $probes = Get-Probe
        
        $probes | Get-Group | where {
            ($_ | Get-Device | Get-Sensor).Name -eq "Volume IO _Total0"
        }

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }
    
    #endregion
    #region 29: Something -> Table -> Where { Variable(1) -> Table -> Table -> Where { Variable -> Object } }
    
    It "29a: Table -> Table -> Where { Variable(1) -> Table -> Table -> Where { Variable -> Object } }" {
        
        $counts = @{
            Sensors = 2
        }

        $sensors = RunCustomCount $counts { Get-Sensor -Count 2 }

        Get-Probe | Get-Group | where {
            $_ | Get-Device | Get-Sensor | where {
                $sensors | Get-Channel
            }
        }

        Validate (@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")

            #region Probe 1, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            #endregion Probe 1, Group 1

            ##########################################################################################

            #region Probe 1, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

            #endregion Probe 1, Group 2

            ##########################################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")

            #region Probe 2, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }

    It "29b: Variable -> Table -> Where { Variable(1) -> Table -> Table -> Where { Variable -> Object } }" {
        $counts = @{
            Sensors = 2
        }

        $sensors = RunCustomCount $counts { Get-Sensor -Count 2 }
        $probes = Get-Probe

        $probes | Get-Group | where {
            $_ | Get-Device | Get-Sensor | where {
                $sensors | Get-Channel
            }
        }

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            #region Probe 2, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            #region Probe 2, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
        ))
    }

    #endregion
    #region 30: Something -> Table -> Where { Variable(1) -> Table -> Table } -> Table

    It "30a: Table -> Table -> Where { Variable(1) -> Table -> Table } -> Table" {

        Get-Probe | Get-Device | Where {
            ($_ | Get-Sensor | Get-Channel).Name -eq "Percent Available Memory"
        } | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            #region Probe 1, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            #endregion Probe 1, Device 2

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            #region Probe 1, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)

            #endregion Probe 1, Device 2

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            #region Probe 2, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            #endregion Probe 2, Device 1

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            #region Probe 2, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)

            #endregion Probe 2, Device 2

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }
    
    It "30b: Variable -> Table -> Where { Variable(1) -> Table -> Table } -> Table" {

        #TODO: technically speaking, this functionality does not work properly. Because when showing progress
        #when piping from a variable, the last cmdlet is responsible for updating the second last cmdlet's count.
        #In this instance, the last Get-Sensor is responsible for updating the count of the number of groups that
        #have been processed. However, because there is a Where-Object in the middle, Get-Sensor is not
        #called for every group that is processed, and therefore you can't see the groups processed count increase

        $probes = Get-Probe

        $probes | Get-Device | Where {
            ($_ | Get-Sensor | Get-Channel).Name -eq "Percent Available Memory"
        } | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            #region Probe 1, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            #endregion Probe 1, Device 1

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            #region Probe 1, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)

            #endregion Probe 1, Device 2

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            #region Probe 2, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            #endregion Probe 2, Device 1

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            #region Probe 2, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }
    
    #endregion
    #region 31: Something -> Table -> Where { Variable(1) -> Action }

    It "31a: Table -> Table -> Where { Variable(1) -> Action -Batch:`$false }" {
        Get-Probe | Get-Device | Where { $_ | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    It "31b: Variable -> Table -> Where { Variable(1) -> Action:`$false }" {
        $probes = Get-Probe

        $probes | Get-Device | Where { $_ | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' (ID: 3000) forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (1/1)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "31c: Table -> Table -> Where { Variable(1) -> Action -Batch:`$true }" {
        Get-Probe | Get-Device | Where { $_ | Pause-Object -Forever -Batch:$true }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (1/1)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (1/1)" 100)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "31d: Variable -> Table -> Where { Variable(1) -> Action -Batch:`$true }" {
        $probes = Get-Probe

        $probes | Get-Device | Where { $_ | Pause-Object -Forever -Batch:$true }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (1/1)" 100)
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device0' forever (1/1)" 100)

                (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (1/1)" 100)
                (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (1/1)" 100)
                (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (1/1)" 100)
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    #endregion
    #region 32: Something -> Table -> Where { Variable(1) -> Table -> Action }
    
    It "32a: Table -> Table -> Where { Variable(1) -> Table -> Action }" {
        Get-Probe | Get-Device | Where { $_ | Get-Sensor | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "32b: Variable -> Table -> Where { Variable(1) -> Table -> Action }" {
        $probes = Get-Probe
        
        $probes | Get-Device | Where { $_ | Get-Sensor | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100)
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    #endregion
    #region 33: Something -> Table -> Where { Variable(1) -> Action -> Table }

    It "33a: Table -> Table -> Where { Variable(1) -> Action -> Table }" {
        Get-Probe | Get-Device | Where { $_ | Clone-Object 5678 | Get-Sensor }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "33b: Variable -> Table -> Where { Variable(1) -> Action -> Table }" {
        $probes = Get-Probe
        
        $probes | Get-Device | Where { $_ | Clone-Object 5678 | Get-Sensor }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    #endregion
    #region 34: Something -> Table -> Action -> Table -> Action

    It "34a: Table -> Table -> Action -> Table -> Action" {
        Get-Group | Get-Device | Clone-Object 5678 | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "34b: Variable -> Table -> Action -> Table -> Action" {
        $groups = Get-Group

        $groups | Get-Device | Clone-Object 5678 | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    #endregion
    #region 35: Something -> Table -> Action -> Action

    It "35a: Table -> Table -> Action -> Action" {
        Get-Group | Get-Device | Clone-Object 5678 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            ###################################################################

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "35b: Variable -> Table -> Action -> Action" {
        $groups = Get-Group

        $groups | Get-Device | Clone-Object 5678 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device0' (ID: 3000) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device0' (ID: 3000) (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    #endregion
    #region 36: Something -> Select -> Where -> [Table] -> Something
        #region 36.1: Something -> Select -First -> Where -> [Table] -> Something

    It "36.1a: Table -> Select -First -> Where -> Table -> Object" {

        Get-Device -Count 10 | Select -First 5 | where { $_.Id -lt 3003 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "PRTG Device Search" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50)
        ))
    }

    It "36.1b: Variable -> Select -First -> Where -> Table -> Table" {
        $devices = Get-Device -Count 10

        $devices | Select -First 5 | where { $_.Id -lt 3003 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30)
        ))
    }

    It "36.1c: Table -> Select -First -> Where -> Action" {
        Get-Device -Count 10 | Select -First 5 | where { $_.Id -lt 3003 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/10)" 30)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device2' (ID: 3002) forever (3/10)" 30)
        ))
    }

    It "36.1d: Variable -> Select -First -> Where -> Action" {

        $devices = Get-Device -Count 10

        $devices | Select -First 5 | where { $_.Id -lt 3003 } | Pause-Object -Forever -Batch:$false
        
        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/10)" 30)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device2' (ID: 3002) forever (3/10)" 30)
        ))
    }

        #endregion
        #region 36.2: Something -> Select -Last -> Where -> [Table] -> Something

    It "36.2a: Table -> Select -Last -> Where -> Table -> Object" {
        Get-Device -Count 10 | Select -Last 5 | where { $_.Id -lt 3007 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "PRTG Device Search" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device9' (ID: 3009) (10/10)" 100)

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (1/5)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (1/5)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (1/5)" 20) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40)
        ))
    }

    It "36.2b: Variable -> Select -Last -> Where -> Table -> Object" {
        $devices = Get-Device -Count 10

        $devices | Select -Last 5 | where { $_.Id -lt 3007 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (1/5)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (1/5)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (1/5)" 20) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device6' (ID: 3006) (2/5)" 40)
        ))
    }

    It "36.2c: Table -> Select -Last -> Where -> Action" {
        Get-Device -Count 10 | Select -Last 5 | where { $_.Id -lt 3007 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device9' (ID: 3009) (10/10)" 100)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device5' (ID: 3005) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device6' (ID: 3006) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device6' (ID: 3006) forever (2/5)" 40)
        ))
    }

    It "36.2d: Variable -> Select -Last -> Where -> Action" {
        $devices = Get-Device -Count 10

        $devices | Select -Last 5 | where { $_.Id -lt 3007 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device5' (ID: 3005) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device6' (ID: 3006) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device6' (ID: 3006) forever (2/5)" 40)
        ))
    }

        #endregion
        #region 36.3: Something -> Select -Skip -> Where -> [Table] -> Something

    It "36.3a: Table -> Select -Skip -> Where -> Table -> Object" {
        Get-Device -Count 10 | Select -Skip 7 | where { $_.Id -lt 3008 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "PRTG Device Search" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device5' (ID: 3005) (6/10)" 60)
            (Gen "PRTG Device Search" "Processing device 'Probe Device6' (ID: 3006) (7/10)" 70)

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80)
            (Gen "PRTG Device Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device8' (ID: 3008) (9/10)" 90)
            (Gen "PRTG Device Search" "Processing device 'Probe Device9' (ID: 3009) (10/10)" 100)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device9' (ID: 3009) (10/10)" 100)
        ))
    }

    It "36.3b: Variable -> Select -Skip -> Where -> Table -> Object" {
        $devices = Get-Device -Count 10

        $devices | Select -Skip 7 | where { $_.Id -lt 3008 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80)
        ))
    }

    It "36.3c: Table -> Select -Skip -> Where -> Action" {
        Get-Device -Count 10 | Select -Skip 7 | where { $_.Id -lt 3008 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device7' (ID: 3007) forever (8/10)" 80)
            (Gen "Pausing PRTG Objects (Completed)" "Processing device 'Probe Device9' (ID: 3009) (10/10)" 100)
        ))
    }

    It "36.3d: Variable -> Select -Skip -> Where -> Action" {
        $devices = Get-Device -Count 10

        $devices | Select -Skip 7 | where { $_.Id -lt 3008 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device7' (ID: 3007) forever (8/10)" 80)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device7' (ID: 3007) forever (8/10)" 80)
        ))
    }

        #endregion
        #region 36.4: Something -> Select -SkipLast -> Where -> [Table] -> Something

    It "36.4a: Table -> Select -SkipLast -> Where -> Table -> Object" {
        Get-Device -Count 10 | Select -SkipLast 5 | where { $_.Id -lt 3003 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Retrieving all sensors")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60 "Retrieving all sensors")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60)
        ))
    }

    It "36.4b: Variable -> Select -SkipLast -> Where -> Table -> Object" {
        $devices = Get-Device -Count 10

        $devices | Select -SkipLast 5 | where { $_.Id -lt 3003 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60)
        ))
    }

    It "36.4c: Table -> Select -SkipLast -> Where -> Action" {
        Get-Device -Count 10 | Select -SkipLast 5 | where { $_.Id -lt 3003 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
        ))
    }

    It "36.4d: Variable -> Select -SkipLast -> Where -> Action" {
        $devices = Get-Device -Count 10
        
        $devices | Select -SkipLast 5 | where { $_.Id -lt 3003 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
        ))
    }

        #endregion
        #region 36.5: Something -> Select -Index -> Where -> [Table] -> Something

    It "36.5a: Table -> Select -Index -> Where -> Table -> Object" {
        Get-Device -Count 10 | Select -Index 1,3,5,7,8 | where { $_.Id -lt 3005 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Device Search" "Processing device 'Probe Device5' (ID: 3005) (6/10)" 60)
            (Gen "PRTG Device Search" "Processing device 'Probe Device7' (ID: 3007) (8/10)" 80)
            (Gen "PRTG Device Search" "Processing device 'Probe Device8' (ID: 3008) (9/10)" 90)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device8' (ID: 3008) (9/10)" 90)
        ))
    }

    It "36.5b: Variable -> Select -Index -> Where -> Table -> Object" {
        $devices = Get-Device -Count 10

        $devices | Select -Index 1,3,5,7,8 | where { $_.Id -lt 3005 } | Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40)
        ))
    }

    It "36.5c: Table -> Select -Index -> Where -> Action" {
        Get-Device -Count 10 | Select -Index 1,3,5,7,8 | where { $_.Id -lt 3005 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/10)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device3' (ID: 3003) forever (4/10)" 40)
        ))
    }

    It "36.5d: Variable -> Select -Index -> Where -> Action" {
        $devices = Get-Device -Count 10

        $devices | Select -Index 1,3,5,7,8 | where { $_.Id -lt 3005 } | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/10)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device3' (ID: 3003) forever (4/10)" 40)
        ))
    }

        #endregion
    #endregion
    #region 37: Something -> Where -> Select -> Something
        #region 37.1: Something -> Where -> Select -First -> Something

    It "37.1a: Table -> Where -> Select -First -> Table" {

        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -First 5 | Get-Sensor

        Assert-NoProgress
    }

    It "37.1b: Variable -> Where -> Select -First -> Table" {

        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -First 5 | Get-Sensor

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/10)" 30 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/10)" 50 "Retrieving all sensors")
        ))
    }

    It "37.1c: Table -> Where -> Select -First -> Action" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -First 5 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "37.1d: Variable -> Where -> Select -First -> Action" {
        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -First 5 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/10)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device4' (ID: 3004) forever (5/10)" 50)
        ))
    }

        #endregion
        #region 37.2: Something -> Where -> Select -Last -> Something

    It "37.2a: Table -> Where -> Select -Last -> Table" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -Last 5 | Get-Sensor

        Assert-NoProgress
    }

    It "37.2b: Variable -> Where -> Select -Last -> Table" {
        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -Last 5 | Get-Sensor

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (1/5)" 20 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (2/5)" 40 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device4' (ID: 3004) (3/5)" 60 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (4/5)" 80 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (5/5)" 100 "Retrieving all sensors")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device6' (ID: 3006) (5/5)" 100 "Retrieving all sensors")
        ))
    }

    It "37.2c: Table -> Where -> Select -Last -> Action" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -Last 5 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "37.2d: Variable -> Where -> Select -Last -> Action" {
        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -Last 5 | Pause-Object -Forever -Batch:$false
        
        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device5' (ID: 3005) forever (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device6' (ID: 3006) forever (5/5)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device6' (ID: 3006) forever (5/5)" 100)
        ))
    }

        #endregion
        #region 37.3: Something -> Where -> Select -Skip -> Something

    It "37.3a: Table -> Where -> Select -Skip -> Table" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -Skip 5 | Get-Sensor

        Assert-NoProgress
    }

    It "37.3b: Variable -> Where -> Select -Skip -> Table" {

        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -Skip 5 | Get-Sensor

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (6/10)" 60 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device6' (ID: 3006) (7/10)" 70 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device6' (ID: 3006) (7/10)" 70 "Retrieving all sensors")
        ))
    }

    It "37.3c: Table -> Where -> Select -Skip -> Action" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -Skip 5 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "37.3d: Variable -> Where -> Select -Skip -> Action" {
        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -Skip 5 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device5' (ID: 3005) forever (6/10)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device6' (ID: 3006) forever (7/10)" 70)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device6' (ID: 3006) forever (7/10)" 70)
        ))
    }

        #endregion
        #region 37.4: Something -> Where -> Select -SkipLast -> Something

    It "37.4a: Table -> Where -> Select -SkipLast -> Table" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -SkipLast 5 | Get-Sensor

        Assert-NoProgress
    }

    It "37.4b: Variable -> Where -> Select -SkipLast -> Table" {

        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -SkipLast 5 | Get-Sensor

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Retrieving all sensors")
        ))
    }

    It "37.4c: Table -> Where -> Select -SkipLast -> Action" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -SkipLast 5 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "37.4d: Variable -> Where -> Select -SkipLast -> Action" {
        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -SkipLast 5 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
        ))
    }

        #endregion
        #region 37.5: Something -> Where -> Select -Index -> Something

    It "37.5a: Table -> Where -> Select -Index -> Table" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -Index 0,1,3,5 | Get-Sensor

        Assert-NoProgress
    }

    It "37.5b: Variable -> Where -> Select -Index -> Table" {

        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -Index 0,1,3,5 | Get-Sensor

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/10)" 20 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device5' (ID: 3005) (6/10)" 60 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device5' (ID: 3005) (6/10)" 60 "Retrieving all sensors")
        ))
    }

    It "37.5c: Table -> Where -> Select -Index -> Action" {
        Get-Device -Count 10 | where { $_.Id -lt 3007 } | Select -Index 0,1,3,5 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "37.5d: Variable -> Where -> Select -Index -> Action" {
        $devices = Get-Device -Count 10

        $devices | where { $_.Id -lt 3007 } | Select -Index 0,1,3,5 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device5' (ID: 3005) forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device5' (ID: 3005) forever (6/10)" 60)
        ))
    }

        #endregion
    #endregion
    #region 100: Get-SensorFactorySource
        #region 100a: Something -> Get-SensorFactorySource

    It "100a1: Sensor -> Get-SensorFactorySource" {

        Get-Sensor -Count 2 | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")
        ))
    }

    It "100a2: Variable -> Get-SensorFactorySource" {

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2

        $sensors | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")
        ))
    }

        #endregion
        #region 100b: Something -> Get-SensorFactorySource -> Channel

    It "100b1: Sensor -> Get-SensorFactorySource -> Channel" {

        Get-Sensor -Count 2 | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "100b2: Variable -> Get-SensorFactorySource -> Channel" {

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2

        $sensors | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

        #endregion
        #region 100c: Something -> Sensor -> Get-SensorFactorySource -> Channel

    It "100c1: Device -> Sensor -> Get-SensorFactorySource -> Channel" {

        Get-Device | Get-Sensor | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "100c2: Variable -> Sensor -> Get-SensorFactorySource -> Channel" {
        $devices = Get-Device

        $devices.Count | Should Be 2

        $devices | Get-Sensor | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

        #endregion
        #region 100d: Something -> Get-SensorFactorySource -> Action

    It "100d1: Sensor -> Get-SensorFactorySource -> Action -Batch:`$false" {
        Get-Sensor -Count 2 | Get-SensorFactorySource | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "100d2: Variable -> Get-SensorFactorySource -> Action -Batch:`$false" {

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2

        $sensors | Get-SensorFactorySource | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' (ID: 4001) forever (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' (ID: 4002) forever (2/2)" 100)

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "100d3: Sensor -> Get-SensorFactorySource -> Action -Batch:`$true" {
        Get-Sensor -Count 2 | Get-SensorFactorySource | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total1', 'Volume IO _Total2', 'Volume IO _Total1' and 'Volume IO _Total2' forever (4/4)" 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total1', 'Volume IO _Total2', 'Volume IO _Total1' and 'Volume IO _Total2' forever (4/4)" 100)
        ))
    }

    It "100d4: Variable -> Get-SensorFactorySource -> Action -Batch:`$true" {
        $sensors = Get-Sensor -Count 2

        $sensors | Get-SensorFactorySource | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total1', 'Volume IO _Total2', 'Volume IO _Total1' and 'Volume IO _Total2' forever (4/4)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total1', 'Volume IO _Total2', 'Volume IO _Total1' and 'Volume IO _Total2' forever (4/4)" 100)
        ))
    }

        #endregion
        #region 100e1: Something -> Get-SensorFactorySource -> Action -> Object

    It "100e1: Sensor -> Get-SensorFactorySource -> Action -> Object" {

        Get-Sensor -Count 2 | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "100e2: Variable -> Get-SensorFactorySource -> Action -> Object" {

        $sensors = Get-Sensor -Count 2
        
        $sensors | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

        #endregion
        #region 100f1: Something -> Get-SensorFactorySource -> Action -> Object -> Anything

    It "100f1: Something -> Get-SensorFactorySource -> Action -> Object -> Anything" {
        
        Get-Sensor -Count 2 | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel | Set-ChannelProperty UpperErrorLimit 100 -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "100f2: Variable -> Get-SensorFactorySource -> Action -> Object -> Anything" {
        
        $sensors = Get-Sensor -Count 2
        
        $sensors | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel | Set-ChannelProperty UpperErrorLimit 100 -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50 "Retrieving all sensor factory sensors")

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100 "Retrieving all sensor factory sensors")

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (1/2)" 50) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

        #endregion
        #region 100g: Select -First -> Get-SensorFactorySource

    It "100g1: Table -> Select -First -> Get-SensorFactorySource" {
        Get-Sensor -Count 10 | Select -First 2 | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")
        ))
    }

    It "100g2: Variable -> Select -First -> Get-SensorFactorySource" {
        $sensors = Get-Sensor -Count 10

        $sensors | Select -First 2 | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")
        ))
    }

    It "100g3: Table -> Select -First -> Get-SensorFactorySource -> Channel" {
        Get-Sensor -Count 10 | Select -First 2 | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20)
        ))
    }

    It "100g4: Variable -> Select -First -> Get-SensorFactorySource -> Channel" {
        $sensors = Get-Sensor -Count 10

        $sensors | Select -First 2 | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/10)" 10) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")
            
            ###################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/10)" 20)#>
        ))
    }

        #endregion
        #region 100h: Select -Last -> Get-SensorFactorySource

    It "100h1: Table -> Select -Last -> Get-SensorFactorySource" {
        Get-Sensor -Count 6 | Select -Last 2 | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/6)" 16)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/6)" 33)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (3/6)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total3' (ID: 4003) (4/6)" 66)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total5' (ID: 4005) (6/6)" 100)

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory sensors")
        ))
    }

    It "100h2: Variable -> Select -Last -> Get-SensorFactorySource" {
        $sensors = Get-Sensor -Count 6

        $sensors | Select -Last 2 | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory sensors")
        ))
    }

    It "100h3: Table -> Select -Last -> Get-SensorFactorySource -> Channel" {
        Get-Sensor -Count 6 | Select -Last 2 | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/6)" 16)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/6)" 33)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (3/6)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total3' (ID: 4003) (4/6)" 66)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total5' (ID: 4005) (6/6)" 100)

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100)
        ))
    }

    It "100h4: Variable -> Select -Last -> Get-SensorFactorySource -> Channel" {
        $sensors = Get-Sensor -Count 6

        $sensors | Select -Last 2 | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total4' (ID: 4004) (1/2)" 50) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            ###################################################################

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (ID: 4001) (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total2' (ID: 4002) (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total5' (ID: 4005) (2/2)" 100)
        ))
    }

        #endregion
        #region 100i: Table -> Get-SensorFactorySource -> Select -First -> Something

    It "100i1: Table -> Get-SensorFactorySource -> Select -First -> Table" {
        Get-Sensor -Count 10 | Get-SensorFactorySource | Select -First 1 | Get-Channel

        Assert-NoProgress
    }

    It "100i2: Table -> Get-SensorFactorySource -> Select -First -> Action" {
        Get-Sensor -Count 10 | Get-SensorFactorySource | Select -First 1 | Pause-Object -Forever -Batch:$false
        Get-Sensor -Count 10 | Get-SensorFactorySource | Select -First 1 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

        #endregion
    #endregion
    #region 101: Something -> Action -Batch:$true
        #region 101a: Pipeline Combinations

    It "101a1: Table -> Action -Batch:`$true" {
        Get-Sensor -Count 3 | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1' and 'Volume IO _Total2' forever (3/3)"

        Validate(@(
            (Gen "PRTG Sensor Search"         "Retrieving all sensors")
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/3)" 66)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/3)" 100)
            (Gen "Pausing PRTG Objects"       $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101a2: Table -> Action -> Action -Batch:`$true" {
        Get-Sensor -Count 3 | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0' and 'Volume IO _Total0' (3/3)"

        Validate(@(
            (Gen "PRTG Sensor Search"         "Retrieving all sensors")
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/3)" 33)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/3)" 66)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/3)" 66)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (3/3)" 100)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (ID: 4000) (3/3)" 100)
            (Gen "Resuming PRTG Objects"      $final 100)
            (Gen "Resuming PRTG Objects (Completed)" $final 100)
        ))
    }

    ###################################################################

    It "101a3: Variable -> Action -Batch:`$true" {
        $sensors = Get-Sensor -Count 3

        $sensors | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1' and 'Volume IO _Total2' forever (3/3)"

        Validate(@(
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/3)" 66)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/3)" 100)
            (Gen "Pausing PRTG Objects"       $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101a4: Variable -> Action -> Action -Batch:`$true" {
        $sensors = Get-Sensor -Count 3

        $sensors | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0' and 'Volume IO _Total0' (3/3)"

        Validate(@(
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/3)" 33)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/3)" 66)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/3)" 66)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total2' (ID: 4002) to object ID 5678 (3/3)" 100)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (ID: 4000) (3/3)" 100)
            (Gen "Resuming PRTG Objects"      $final 100)
            (Gen "Resuming PRTG Objects (Completed)"      $final 100)
        ))
    }

    ##########################################################################################

    It "101a5: Table -> Table -> Action -Batch:`$true" {
        Get-Device -Count 3 | Get-Sensor | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total0', 'Volume IO _Total1'," +
                 " 'Volume IO _Total0' and 'Volume IO _Total1' forever (6/6)"
        
        Validate(@(
            (Gen "PRTG Device Search"         "Retrieving all devices")
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search"    "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"  "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   $final 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)"   $final 100)
        ))
    }

    It "101a6: Table -> Table -> Action -> Action -Batch:`$true" {
        Get-Device -Count 3 | Get-Sensor | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0'," + 
                  " 'Volume IO _Total0' and 'Volume IO _Total0' (6/6)"

        Validate(@(
            (Gen "PRTG Device Search"         "Retrieving all devices")
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  $final 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)"  $final 100)
        ))
    }

    ###################################################################

    It "101a7: Variable -> Table -> Action -Batch:`$true" {
        $device = Get-Device -Count 3

        $device | Get-Sensor | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total0', 'Volume IO _Total1'," +
                 " 'Volume IO _Total0' and 'Volume IO _Total1' forever (6/6)"

        Validate(@(
            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects (Completed)"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects (Completed)"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   $final 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101a8: Variable -> Table -> Action -> Action -Batch:`$true" {
        $device = Get-Device -Count 3

        $device | Get-Sensor | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0'," + 
        " 'Volume IO _Total0' and 'Volume IO _Total0' (6/6)"

        Validate(@(
            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) to object ID 5678 (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) to object ID 5678 (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (ID: 4000) (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  $final 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)"  $final 100)
        ))
    }

        #endregion
        #region 101b: Progress Messages

    It "101b1: displays 9 items 'and others' with 11 objects" {

        $sensors = Get-Sensor -Count 11

        $sensors | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total2', " +
                 "'Volume IO _Total3', 'Volume IO _Total4', 'Volume IO _Total5', 'Volume IO _Total6', " +
                 "'Volume IO _Total7', 'Volume IO _Total8' and 2 others forever (11/11)"

        Validate(@(
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/11)" 9)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/11)" 18)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/11)" 27)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total3' (ID: 4003) (4/11)" 36)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total4' (ID: 4004) (5/11)" 45)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total5' (ID: 4005) (6/11)" 54)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total6' (ID: 4006) (7/11)" 63)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total7' (ID: 4007) (8/11)" 72)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total8' (ID: 4008) (9/11)" 81)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total9' (ID: 4009) (10/11)" 90)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total10' (ID: 4010) (11/11)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101b2: displays 9 items 'and others' with 11 objects of a specific type" {
        $sensors = Get-Sensor -Count 11

        $sensors | Acknowledge-Sensor -Forever

        $final = "Acknowledging sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total2', " +
                 "'Volume IO _Total3', 'Volume IO _Total4', 'Volume IO _Total5', 'Volume IO _Total6', " +
                 "'Volume IO _Total7', 'Volume IO _Total8' and 2 others forever (11/11)"

        Validate(@(
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/11)" 9)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/11)" 18)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/11)" 27)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total3' (ID: 4003) (4/11)" 36)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total4' (ID: 4004) (5/11)" 45)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total5' (ID: 4005) (6/11)" 54)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total6' (ID: 4006) (7/11)" 63)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total7' (ID: 4007) (8/11)" 72)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total8' (ID: 4008) (9/11)" 81)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total9' (ID: 4009) (10/11)" 90)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total10' (ID: 4010) (11/11)" 100)
            (Gen "Acknowledging PRTG Sensors"         $final 100)
            (Gen "Acknowledging PRTG Sensors (Completed)" $final 100)
        ))
    }

    ###################################################################

    It "101b3: displays 10 items 'and others' with 12 objects" {

        $sensors = Get-Sensor -Count 12

        $sensors | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total2', " +
                 "'Volume IO _Total3', 'Volume IO _Total4', 'Volume IO _Total5', 'Volume IO _Total6', " +
                 "'Volume IO _Total7', 'Volume IO _Total8', 'Volume IO _Total9' and 2 others forever (12/12)"

        Validate(@(
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/12)" 8)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/12)" 16)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/12)" 25)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total3' (ID: 4003) (4/12)" 33)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total4' (ID: 4004) (5/12)" 41)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total5' (ID: 4005) (6/12)" 50)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total6' (ID: 4006) (7/12)" 58)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total7' (ID: 4007) (8/12)" 66)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total8' (ID: 4008) (9/12)" 75)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total9' (ID: 4009) (10/12)" 83)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total10' (ID: 4010) (11/12)" 91)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total11' (ID: 4011) (12/12)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101b4: displays 10 items 'and others' with 12 objects of a specific type" {
        $sensors = Get-Sensor -Count 12

        $sensors | Acknowledge-Sensor -Forever

        $final = "Acknowledging sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total2', " +
                 "'Volume IO _Total3', 'Volume IO _Total4', 'Volume IO _Total5', 'Volume IO _Total6', " +
                 "'Volume IO _Total7', 'Volume IO _Total8', 'Volume IO _Total9' and 2 others forever (12/12)"

        Validate(@(
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/12)" 8)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/12)" 16)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/12)" 25)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total3' (ID: 4003) (4/12)" 33)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total4' (ID: 4004) (5/12)" 41)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total5' (ID: 4005) (6/12)" 50)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total6' (ID: 4006) (7/12)" 58)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total7' (ID: 4007) (8/12)" 66)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total8' (ID: 4008) (9/12)" 75)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total9' (ID: 4009) (10/12)" 83)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total10' (ID: 4010) (11/12)" 91)
            (Gen "Acknowledging PRTG Sensors"         "Queuing sensor 'Volume IO _Total11' (ID: 4011) (12/12)" 100)
            (Gen "Acknowledging PRTG Sensors"         $final 100)
            (Gen "Acknowledging PRTG Sensors (Completed)" $final 100)
        ))
    }

    ###################################################################

    It "101b5: processes multiple object types" {
        $sensors = Get-Sensor -Count 2
        $devices = Get-Device -Count 2
        $groups = Get-Group -Count 2

        $objects = @()
        $objects += $sensors
        $objects += $devices
        $objects += $groups

        $objects | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', devices 'Probe Device0', 'Probe Device1'" +
                 " and groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (6/6)"

        Validate(@(
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/6)" 16)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/6)" 33)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device0' (ID: 3000) (3/6)" 50)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device1' (ID: 3001) (4/6)" 66)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure0' (ID: 2000) (5/6)" 83)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure1' (ID: 2001) (6/6)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    ###################################################################

    It "101b6: processes multiple object types 'and others' with more than 10 objects" {
        $sensors = Get-Sensor -Count 4
        $devices = Get-Device -Count 7
        $groups = Get-Group -Count 4

        $objects = @()
        $objects += $sensors
        $objects += $devices
        $objects += $groups

        $objects | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total2', " +
                 "'Volume IO _Total3', devices 'Probe Device0', 'Probe Device1', 'Probe Device2', " +
                 "'Probe Device3', 'Probe Device4', 'Probe Device5' and 5 others forever (15/15)"

        Validate(@(
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/15)" 6)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/15)" 13)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total2' (ID: 4002) (3/15)" 20)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total3' (ID: 4003) (4/15)" 26)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device0' (ID: 3000) (5/15)" 33)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device1' (ID: 3001) (6/15)" 40)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device2' (ID: 3002) (7/15)" 46)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device3' (ID: 3003) (8/15)" 53)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device4' (ID: 3004) (9/15)" 60)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device5' (ID: 3005) (10/15)" 66)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device6' (ID: 3006) (11/15)" 73)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure0' (ID: 2000) (12/15)" 80)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure1' (ID: 2001) (13/15)" 86)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure2' (ID: 2002) (14/15)" 93)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure3' (ID: 2003) (15/15)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    ###################################################################

    It "101b7: displays unique channel names and groups by channel ID" {

        $sensors = Get-Sensor -Count 3

        $channels = @()
        $channels += 1..2 | foreach { $sensors[0] | Get-Channel }
        $channels += 1..2 | foreach { $sensors[1] | Get-Channel }
        $channels += $sensors[2] | Get-Channel

        $channels.Count | Should Be 5

        $channels[0].Id = 0
        $channels[0].Name = "FirstChannel"
        $channels[1].Id = 1
        $channels[1].Name = "SecondChannel"

        $channels[2].Id = 0
        $channels[2].Name = "FirstChannel"
        $channels[3].Id = 1
        $channels[3].Name = "SecondChannel"

        $channels[4].Id = 0
        $channels[4].Name = "ThirdChannel"

        $channels | Set-ChannelProperty UpperErrorLimit 70

        $final1 = "Setting channels 'FirstChannel' (Sensor IDs: 4000, 4001) and 'ThirdChannel' (Sensor ID: 4002) setting 'UpperErrorLimit' to '70' (5/5)"
        $final2 = "Setting channel 'SecondChannel' (Sensor IDs: 4000, 4001) setting 'UpperErrorLimit' to '70' (5/5)"

        Validate(@(
            (Gen "Modify PRTG Channel Settings"         "Queuing channel 'FirstChannel' (1/5)" 20)
            (Gen "Modify PRTG Channel Settings"         "Queuing channel 'SecondChannel' (2/5)" 40)
            (Gen "Modify PRTG Channel Settings"         "Queuing channel 'FirstChannel' (3/5)" 60)
            (Gen "Modify PRTG Channel Settings"         "Queuing channel 'SecondChannel' (4/5)" 80)
            (Gen "Modify PRTG Channel Settings"         "Queuing channel 'ThirdChannel' (5/5)" 100)
            (Gen "Modify PRTG Channel Settings"         $final1 100)
            (Gen "Modify PRTG Channel Settings"         $final2 100)
            (Gen "Modify PRTG Channel Settings (Completed)" $final2 100)
        ))
    }

        #endregion
        #region 101c: Cmdlet Validation

    function ValidateMultiTypeCmdlet($baseType, $objectId, $progressActivity, $names, $realMessage)
    {
        $lowerType = $basetype.ToLower()

        Validate(@(
            (Gen "PRTG $baseType Search"         "Retrieving all $($lowerType)s")
            (Gen "PRTG $baseType Search"         "Processing $lowerType '$($names[0])' (ID: $objectId) (1/2)" 50)
            (Gen "$progressActivity"             "Queuing $lowerType '$($names[0])' (ID: $objectId) (1/2)" 50)
            (Gen "$progressActivity"             "Queuing $lowerType '$($names[1])' (ID: $($objectId + 1)) (2/2)" 100)
            (Gen "$progressActivity"             "$realMessage (2/2)" 100)
            (Gen "$progressActivity (Completed)" "$realMessage (2/2)" 100)
        ))
    }

    It "101c1: Acknowledge-Sensor" {
        Get-Sensor -Count 2 | Acknowledge-Sensor -Duration 10

        $names = @("Volume IO _Total0","Volume IO _Total1")

        ValidateMultiTypeCmdlet "Sensor" 4000 "Acknowledging PRTG Sensors" $names "Acknowledging sensors '$($names[0])' and '$($names[1])' for 10 minutes"
    }

    It "101c2: Pause-Object" {
        Get-Device -Count 2 | Pause-Object -Forever

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" 3000 "Pausing PRTG Objects" $names "Pausing devices '$($names[0])' and '$($names[1])' forever"
    }

    It "101c3: Refresh-Object" {
        Get-Device -Count 2 | Refresh-Object

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" 3000 "Refreshing PRTG Objects" $names "Refreshing devices '$($names[0])' and '$($names[1])'"
    }

    It "101c4: Rename-Object" {
        Get-Device -Count 2 | Rename-Object "newName"

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" 3000 "Rename PRTG Objects" $names "Renaming devices '$($names[0])' and '$($names[1])' to 'newName'"
    }

    It "101c5: Resume-Object" {
        Get-Device -Count 2 | Resume-Object

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" 3000 "Resuming PRTG Objects" $names "Resuming devices '$($names[0])' and '$($names[1])'"
    }

    It "101c6: Set-ChannelProperty" {
        Get-Sensor -Count 1 | Get-Channel | Set-ChannelProperty LimitsEnabled $true

        Validate(@(
            (Gen "PRTG Sensor Search"         "Retrieving all sensors")
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                (Gen2 "PRTG Channel Search"    "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                (Gen2 "Modify PRTG Channel Settings" "Queuing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                (Gen2 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'LimitsEnabled' to 'True' (1/1)" 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                (Gen2 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'LimitsEnabled' to 'True' (1/1)" 100)
        ))
    }

    It "101c7: Set-ObjectProperty" {
        Get-Device -Count 2 | Set-ObjectProperty Interval 00:00:30

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" 3000 "Modify PRTG Object Settings" $names "Setting devices '$($names[0])' and '$($names[1])' setting 'Interval' to '00:00:30'"
    }

    It "101c8: Simulate-ErrorStatus" {
        Get-Sensor -Count 2 | Simulate-ErrorStatus

        $names = @("Volume IO _Total0","Volume IO _Total1")

        ValidateMultiTypeCmdlet "Sensor" 4000 "Simulating Sensor Errors" $names "Simulating errors on sensors '$($names[0])' and '$($names[1])'"
    }

        #endregion
    #endregion
    #region 102: Something -> Select -Something -> Action -Batch:$true
        #region 102.1: Something -> Select -First -> Action -Batch:$true

    It "102.1a: Table -> Select -First -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -First 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/10)" 40)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.1b: Table -> Select -First -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -First 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (2/10)" 20)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) to object ID 5678 (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (3/10)" 30)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (4/10)" 40)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device0', 'Probe Device0' and 'Probe Device0' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device0', 'Probe Device0' and 'Probe Device0' forever (4/4)" 100)
        ))
    }

    It "102.1c: Variable -> Select -First -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -First 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.1d: Variable -> Select -First -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10
        
        $devices | Select -First 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (2/10)" 20)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) to object ID 5678 (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (3/10)" 30)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) to object ID 5678 (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (4/10)" 40)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device0', 'Probe Device0' and 'Probe Device0' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device0', 'Probe Device0' and 'Probe Device0' forever (4/4)" 100)
        ))
    }

        #endregion
        #region 102.2: Something -> Select -Last -> Action -Batch:$true

    It "102.2a: Table -> Select -Last -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Last 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device9' (ID: 3009) (10/10)" 100)

            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (ID: 3006) (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (ID: 3007) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (ID: 3008) (3/4)" 75)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (ID: 3009) (4/4)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.2b: Table -> Select -Last -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Last 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

    It "102.2c: Variable -> Select -Last -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Last 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (ID: 3006) (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (ID: 3007) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (ID: 3008) (3/4)" 75)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (ID: 3009) (4/4)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.2d: Variable -> Select -Last -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Last 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

        #endregion
        #region 102.3: Something -> Select -Skip -> Action -Batch:$true

    It "102.3a: Table -> Select -Skip -> Action -Batch:`$true" {        
        Get-Device -Count 10 | Select -Skip 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (ID: 3006) (7/10)" 70)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (ID: 3007) (8/10)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (ID: 3008) (9/10)" 90)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (ID: 3009) (10/10)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.3b: Table -> Select -Skip -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Skip 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

    It "102.3c: Variable -> Select -Skip -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10
        
        $devices | Select -Skip 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (ID: 3006) (7/10)" 70)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (ID: 3007) (8/10)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (ID: 3008) (9/10)" 90)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (ID: 3009) (10/10)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.3d: Variable -> Select -Skip -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10
        
        $devices | Select -Skip 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

        #endregion
        #region 102.4: Something -> Select -SkipLast -> Action -Batch:$true

    It "102.4a: Table -> Select -SkipLast -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -SkipLast 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/10)" 10)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/4)" 25)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/4)" 50)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/4)" 75)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/10)" 40) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.4b: Table -> Select -SkipLast -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -SkipLast 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }
    
    It "102.4c: Variable -> Select -SkipLast -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -SkipLast 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/4)" 75)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/4)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.4d: Variable -> Select -SkipLast -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -SkipLast 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

        #endregion
        #region 102.5: Something -> Select -Index -> Action -Batch:$true

    It "102.5a: Table -> Select -Index -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Index 2,3,5,7 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device5' (ID: 3005) (6/10)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (ID: 3007) (8/10)" 80)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
        ))
    }

    It "102.5b: Table -> Select -Index -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Index 2,3,5,7 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

    It "102.5c: Variable -> Select -Index -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Index 2,3,5,7 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device5' (ID: 3005) (6/10)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (ID: 3007) (8/10)" 80)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
        ))
    }

    It "102.5d: Variable -> Select -Index -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Index 2,3,5,7 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Assert-NoProgress
    }

        #endregion
    #endregion
    #region 103: Select -> Action -Batch:$true Stress Tests
        #region 103.1: Something -> Select -Something -SomethingElse -> Action -Batch:$true

    It "103.1a<i>: Table -> Select -Something -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithSingle $name $primary["name"] "Cmdlet"
            }
        }
    }
    
    It "103.1b<i>: Variable -> Select -Something -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithSingle $name $primary["name"] "Variable"
            }
        }
    }

        #endregion
        #region 103.2: Something -> Table -> Select -Something -SomethingElse -> Batch:$true

    It "103.2a<i>: Table -> Table -> Select -Something -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithSingle $name $primary["name"] "CmdletChain"
            }
        }
    }

    
    It "103.2b<i>: Variable -> Table -> Select -Something -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithSingle $name $primary["name"] "VariableChain"
            }
        }
    }

        #endregion
        #region 103.3: Something -> Select -> Select -> Action -Batch:$true

    It "103.3a<i>: Table -> Select -Something -> Select -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithDouble $name $primary["name"] "Cmdlet"
            }
        }
    }

    
    It "103.3b<i>: Variable -> Select -Something -> Select -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithDouble $name $primary["name"] "Variable"
            }
        }
    }

        #endregion
        #region 103.4: Something -> Table -> Select -> Select -> Batch:$true

    It "103.4a<i>: Table -> Table -> Select -Something -> Select -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithDouble $name $primary["name"] "CmdletChain"
            }
        }
    }

    
    It "103.4b<i>: Variable -> Table -> Select -Something -> Select -<name> -> Action -Batch:`$true" -TestCases $allSelectParams {
        param($name)

        foreach($primary in $allSelectParams)
        {
            if($primary["name"] -ne $name)
            {
                TestBatchWithDouble $name $primary["name"] "VariableChain"
            }
        }
    }

        #endregion
    #endregion
    #region 104: Get-SensorTarget
        #region 104.1: Normal Tests

    It "104.1a: Table -> Get-SensorTarget" {
        Get-Device -Count 2 | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "104.1b: Variable -> Get-SensorTarget" {
        $devices = Get-Device -Count 2
        
        $devices | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.1c: Table -> Table -> Get-SensorTarget" {
        Get-Group -Count 2 | Get-Device -Count 2 | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            ###################################################################

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "104.1d: Variable -> Table -> Get-SensorTarget" {
        $groups = Get-Group -Count 2
        
        $groups | Get-Device -Count 2 | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "104.1e: Table -> Action -> Get-SensorTarget" {

        Get-Device -Count 2 | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
        ))
    }

    It "104.1f: Variable -> Action -> Get-SensorTarget" {
        $devices = Get-Device -Count 2
        
        $devices | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Probing target device (50%)")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50 "Probing target device (100%)")

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Probing target device (50%)")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Probing target device (100%)")

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.1g: Table-> Table -> Action -> Get-SensorTarget" {
        Get-Group -Count 2 | Get-Device -Count 2 | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)
        ))
    }

    It "104.1h: Variable -> Table -> Action -> Get-SensorTarget" {
        $groups = Get-Group -Count 2
        
        $groups = Get-Device -Count 2 | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            
            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) to object ID 5678 (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) to object ID 5678 (2/2)" 100)
        ))
    }

        #endregion
        #region 104.2: Select-Object

    It "104.2a: Table -> Select -First -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -First 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)
            
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40)
        ))
    }

    It "104.2b: Variable -> Select -First -> Get-SensorTarget" {
        $devices = Get-Device -Count 5
        
        $devices | Select -First 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Probing target device (100%)")
        ))
    }

    It "104.2c: Table -> Select -Last -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -Last 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100)

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (1/2)" 50 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2d: Variable -> Select -Last -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -Last 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (1/2)" 50 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2e: Table -> Select -Skip -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -Skip 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100)
        ))
    }

    It "104.2f: Variable -> Select -Skip -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -Skip 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (5/5)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2g: Table -> Select -SkipLast -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -SkipLast 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2h: Variable -> Select -SkipLast -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -SkipLast 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2i: Table -> Select -Index -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -Index 1,3 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80)
        ))
    }

    It "104.2j: Variable -> Select -Index -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -Index 1,3 | Get-SensorTarget ExeXml
        
        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (ID: 3001) (2/5)" 40 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/5)" 80 "Probing target device (100%)")
        ))
    }

        #endregion
        #region 104.3: Something -> Get-SensorTarget -> Add-Sensor -> Action

    It "104.3a: Table -> Get-SensorTarget -> Add-Sensor -> Action -Batch:`$false" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | Get-SensorTarget WmiService -Params | Add-Sensor | Pause-Object -Forever -Batch:$false
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "104.3b: Variable -> Get-SensorTarget -> Add-Sensor -> Action -Batch:`$false" {
        $device = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $device | Get-SensorTarget WmiService -Params | Add-Sensor | Pause-Object -Forever -Batch:$false
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "104.3c: Table -> Get-SensorTarget -> Add-Sensor -> Action -Batch:`$true" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | Get-SensorTarget WmiService -Params | Add-Sensor | Pause-Object -Forever -Batch:$true
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
        ))
    }

    It "104.3d: Variable -> Get-SensorTarget -> Add-Sensor -> Action -Batch:`$true" {
        $device = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $device | Get-SensorTarget WmiService -Params | Add-Sensor | Pause-Object -Forever -Batch:$true
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            ###################################################################

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen1 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)

            (Gen1 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
        ))
    }

        #endregion
        #region 104.4: Something -> Table -> Get-SensorTarget -> Add-Sensor -> Action

    It "104.4a: Table -> Table -> Get-SensorTarget -> Add-Sensor -> Action -Batch:`$false" {
        WithResponseArgs "DiffBasedResolveResponse" 3 {
            Get-Probe -Count 1 | Get-Device -Count 2 | Get-SensorTarget WmiService -Params | Add-Sensor | Pause-Object -Forever -Batch:$false
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/1)" 100)
        ))
    }

    It "104.4b: Variable -> Table -> Get-SensorTarget -> Add-Sensor -> Action -Batch:`$false" {
        $probes = Get-Probe -Count 2
        
        WithResponseArgs "DiffBasedResolveResponse" (,([int[]](1,6))) {
            $probes | Get-Device -Count 2 | Get-SensorTarget WmiService -Params | Add-Sensor | Pause-Object -Forever -Batch:$false
        }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' (ID: 1002) forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' (ID: 1003) forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)
        ))
    }

        #endregion
        #region 104.5: Variable -> Get-SensorTarget -> Select -Something -> Add-Sensor

    It "104.5a: Variable -> Get-SensorTarget -> Select -First -> Add-Sensor" {

        $devices = Get-Device -Count 3

        WithResponse "DiffBasedResolveResponse" {
            $devices | Get-SensorTarget WmiService -Parameters | Select -First 2 | Add-Sensor
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            ###################################################################

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66)
        ))
    }

    It "104.5b: Variable -> Get-SensorTarget -> Select -Last -> Add-Sensor" {
        
        $devices = Get-Device -Count 3

        WithResponse "DiffBasedResolveResponse" {
            $devices | Get-SensorTarget WmiService -Parameters | Select -Last 1 | Add-Sensor
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")

            (Gen "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device2' (1/1)" 100)
            (Gen "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device2' (1/1)" 100)
        ))
    }

    It "104.5c: Variable -> Get-SensorTarget -> Select -Skip -> Add-Sensor" {
        
        $devices = Get-Device -Count 3

        WithResponse "DiffBasedResolveResponse" {
            $devices | Get-SensorTarget WmiService -Parameters | Select -Skip 1 | Add-Sensor
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device2' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device2' (1/1)" 100)

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
        ))
    }

    It "104.5d: Variable -> Get-SensorTarget -> Select -SkipLast -> Add-Sensor" {
        $devices = Get-Device -Count 3

        WithResponse "DiffBasedResolveResponse" {
            $devices | Get-SensorTarget WmiService -Parameters | Select -SkipLast 1 | Add-Sensor
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100)
        ))
    }

    It "104.5e: Variable -> Get-SensorTarget -> Select -Index -> Add-Sensor" {
        $devices = Get-Device -Count 3

        WithResponse "DiffBasedResolveResponse" {
            $devices | Get-SensorTarget WmiService -Parameters | Select -Index 0,1 | Add-Sensor
        }

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device0' (1/1)" 100)

            ###################################################################

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Probing target device (100%)")

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen1 "PRTG WMI Service Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding sensor 'Service' to device 'Probe Device1' (1/1)" 100)

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66)
        ))
    }

        #endregion
    #endregion
    #region 105: Select -Something -> No Progress

    It "105a: Table -> Select -First -> New-SensorFactoryDefinition" {

        Get-Sensor -Count 10 | Select -First 5 | New-SensorFactoryDefinition { "Test" } 0

        Assert-NoProgress
    }

    It "105b: Table -> Select -Last -> New-SensorFactoryDefinition" {
        Get-Sensor -Count 10 | Select -Last 5 | New-SensorFactoryDefinition { "Test" } 0

        Assert-NoProgress
    }

    #endregion
    #region 106: Restart-Probe
    
    function RestartProbeCommand($script)
    {
        WithResponse "RestartProbeResponse" $script
    }

    function GenProbeRestarting {@(
        (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"      "00:02:30") # Initial
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:29")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:28")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:27")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:26")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:25")
        (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"      "00:02:25") # First disconnected
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:24")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:23")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:22")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:21")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:20")
        (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"      "00:02:20") # Second disconnected
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:19")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:18")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:17")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:16")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/2" 50 "Waiting for all probes to restart"  "00:02:15")
            
        (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart"     "00:02:15") # First reconnected
            (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart" "00:02:14")
            (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart" "00:02:13")
            (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart" "00:02:12")
            (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart" "00:02:11")
            (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart" "00:02:10")
        (Gen "Restart PRTG Probes" "Restarting all probes 2/2" 100 "Waiting for all probes to restart"     "00:02:10") # Second reconnected
        (Gen "Restart PRTG Probes (Completed)" "Restarting all probes 2/2" 100 "Waiting for all probes to restart" "00:02:10")
    )}

        #region 106.1: Restart-Probe

    It "106.1a: Restart-Probe -Wait" {

        RestartProbeCommand {
            Restart-Probe -Force -Wait
        }

        Validate(@(
            GenProbeRestarting
        ))
    }

    It "106.1b: Restart-Probe -Wait:`$false" {
        RestartProbeCommand {
            Restart-Probe -Force -Wait:$false
        }

        Assert-NoProgress
    }

        #endregion
        #region 106.2: Something -> Restart-Probe

    It "106.2a: Table -> Restart-Probe -Wait" {

        RestartProbeCommand {
            Get-Probe | Restart-Probe -Force -Wait
        }

        Validate (@(
            (Gen "PRTG Probe Search"   "Retrieving all probes")
            (Gen "PRTG Probe Search"   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (2/2)" 100)

            GenProbeRestarting
        ))
    }

    It "106.2b: Table -> Restart-Probe -Wait:`$false" {
        RestartProbeCommand {
            Get-Probe | Restart-Probe -Force -Wait:$false
        }

        Validate (@(
            (Gen "PRTG Probe Search"   "Retrieving all probes")
            (Gen "PRTG Probe Search"   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (2/2)" 100)
            (Gen "Restart PRTG Probes (Completed)" "Restarting probe '127.0.0.11' (2/2)" 100)
        ))
    }

    It "106.2c: Variable -> Restart-Probe -Wait" {

        RestartProbeCommand {
            $probes = Get-Probe

            $probes | Restart-Probe -Force -Wait
        }

        Validate(@(
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (2/2)" 100)

            GenProbeRestarting
        ))
    }

    It "106.2d: Variable -> Restart-Probe -Wait:`$false" {
        RestartProbeCommand {
            $probes = Get-Probe

            $probes | Restart-Probe -Force -Wait:$false
        }

        Validate(@(
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (2/2)" 100)
            (Gen "Restart PRTG Probes (Completed)" "Restarting probe '127.0.0.11' (2/2)" 100)
        ))
    }

        #endregion
        #region 106.3: Something -> Select -Something -> Restart-Probe

    It "106.3a: Table -> Select -First -> Restart-Probe -Wait" {
        RestartProbeCommand {
            Get-Probe | Select -First 1 | Restart-Probe -Force -Wait
        }

        Validate(@(
            (Gen "PRTG Probe Search"   "Retrieving all probes")
            (Gen "PRTG Probe Search"   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)

            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:30") # Initial
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:29")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:28")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:27")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:26")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:25")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:25") # First disconnected
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:24")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:23")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:22")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:21")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:20")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:20") # Second disconnect
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:19") # (response doesn't know
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:18") # only one probe was specified)
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:17")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:16")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:15")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:15") # First reconnect
            (Gen "Restart PRTG Probes (Completed)" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:15")
        ))
    }

    It "106.3b: Variable -> Select -First -> Restart-Probe" {
        RestartProbeCommand {
            $probes = Get-Probe

            $probes | Select -First 1 | Restart-Probe -Force -Wait
        }

        Validate(@(
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)

            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:30") # Initial
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:29")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:28")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:27")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:26")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:25")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:25") # First disconnected
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:24")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:23")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:22")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:21")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:20")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:20") # Second disconnect
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:19") # (response doesn't know
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:18") # only one probe was specified)
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:17")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:16")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:15")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:15") # First reconnect
            (Gen "Restart PRTG Probes (Completed)" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:15")
        ))
    }

    It "106.3c: Table -> Select -First -> Restart-Probe -Wait:`$false" {
        RestartProbeCommand {
            Get-Probe | Select -First 1 | Restart-Probe -Force -Wait:$false
        }

        Validate(@(
            (Gen "PRTG Probe Search"   "Retrieving all probes")
            (Gen "PRTG Probe Search"   "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.10' (1/2)" 50)
            (Gen "Restart PRTG Probes (Completed)" "Restarting probe '127.0.0.10' (1/2)" 50)
        ))
    }

    It "106.3d: Table -> Select -Last -> Restart-Probe -Wait" {
        RestartProbeCommand {
            Get-Probe | Select -Last 1 | Restart-Probe -Force -Wait
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)

            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (1/1)" 100)
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:30") # Initial
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:29")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:28")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:27")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:26")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:25")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:25") # First disconnected
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:24") # (response doesn't know
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:23") # only the second probe was
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:22") # specified)
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:21")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:20")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:20") # Second disconnect
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:19")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:18")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:17")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:16")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:15")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:15") # First reconnect
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:14") # (response doesn't know
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:13") # only the second probe was
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:12") # specified)
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:11")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:10")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:10") # Second reconnect
            (Gen "Restart PRTG Probes (Completed)" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:10")
        ))
    }

    It "106.3e: Variable -> Select -Last -> Restart-Probe -Wait" {
        RestartProbeCommand {
            $probes = Get-Probe

            $probes | Select -Last 1 | Restart-Probe -Force -Wait
        }

        Validate(@(
            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (1/1)" 100)
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:30") # Initial
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:29")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:28")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:27")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:26")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:25")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:25") # First disconnected
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:24") # (response doesn't know
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:23") # only the second probe was
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:22") # specified)
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:21")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:20")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:20") # Second disconnect
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:19")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:18")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:17")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:16")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:15")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:15") # First reconnect
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:14") # (response doesn't know
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:13") # only the second probe was
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:12") # specified)
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:11")
                (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"  "00:02:10")
            (Gen "Restart PRTG Probes" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:10") # Second reconnect
            (Gen "Restart PRTG Probes (Completed)" "Restarting all probes 1/1" 100 "Waiting for all probes to restart"      "00:02:10")
        ))
    }

    It "106.3f: Table -> Select -Last -> Restart-Probe -Wait:`$false" {
        RestartProbeCommand {
            Get-Probe | Select -Last 1 | Restart-Probe -Force -Wait:$false
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100)

            (Gen "Restart PRTG Probes" "Restarting probe '127.0.0.11' (1/1)" 100)
            (Gen "Restart PRTG Probes (Completed)" "Restarting probe '127.0.0.11' (1/1)" 100)
        ))
    }

        #endregion
    #endregion
    #region 107: Restart-PrtgCore

    It "107.1a: Restart-PrtgCore -Wait" {

        WithResponse "RestartPrtgCoreResponse" {
            Restart-PrtgCore -Force -Wait
        }

        Validate(@(
            (Gen "Restart PRTG Core" "Restarting PRTG Core" 33 "Waiting for PRTG Core Service to shutdown" "00:02:30")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 33 "Waiting for PRTG Core Service to shutdown" "00:02:29")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 33 "Waiting for PRTG Core Service to shutdown" "00:02:28")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 33 "Waiting for PRTG Core Service to shutdown" "00:02:27")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 33 "Waiting for PRTG Core Service to shutdown" "00:02:26")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 33 "Waiting for PRTG Core Service to shutdown" "00:02:25")
            (Gen "Restart PRTG Core" "Restarting PRTG Core" 66 "Waiting for PRTG Core Service to restart" "00:02:25")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 66 "Waiting for PRTG Core Service to restart" "00:02:24")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 66 "Waiting for PRTG Core Service to restart" "00:02:23")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 66 "Waiting for PRTG Core Service to restart" "00:02:22")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 66 "Waiting for PRTG Core Service to restart" "00:02:21")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 66 "Waiting for PRTG Core Service to restart" "00:02:20")
            (Gen "Restart PRTG Core" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:20")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:19")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:18")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:17")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:16")
                (Gen "Restart PRTG Core" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:15")
            (Gen "Restart PRTG Core (Completed)" "Restarting PRTG Core" 100 "Waiting for PRTG Core Server to initialize" "00:02:15")
        ))
    }

    It "107.1b: Restart-PrtgCore -Wait:`$false" {
        WithResponse "RestartPrtgCoreResponse" {
            Restart-PrtgCore -Force -Wait:$false
        }

        Assert-NoProgress
    }

    #endregion
    #region 108: PassThru
        #region 108.1: PassThru -Batch:$false
            #region 108.1.1: Something -> Action -PassThru -Batch:$false -> Table

    It "108.1.1a: Table -> Action -PassThru -Batch:`$false -> Table" {
        Get-Device | Pause-Object -Forever -PassThru -Batch:$false | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
        ))
    }

    It "108.1.1b: Variable -> Action -PassThru -Batch:`$false -> Table" {
        $devices = Get-Device

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100 "Retrieving all sensors")
        ))
    }

            #endregion
            #region 108.1.2: Something -> Action -PassThru -Batch:$false -> Table -> Table

    It "108.1.2a: Table -> Action -PassThru -Batch:`$false -> Table -> Table" {

        Get-Group | Pause-Object -Forever -Batch:$false -PassThru | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100)
        ))
    }

    It "108.1.2b: Variable -> Action -PassThru -Batch:`$false -> Table -> Table" {
        $groups = Get-Group

        $groups | Pause-Object -Forever -Batch:$false -PassThru | Get-Device | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure0' (ID: 2000) forever (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing group 'Windows Infrastructure1' (ID: 2001) forever (2/2)" 100)
        ))
    }

            #endregion
            #region 108.1.3: Something -> Action -PassThru -Batch:$false -> Action

    It "108.1.3a: Table -> Action -PassThru -Batch:`$false -> Action" {
        Get-Device | Pause-Object -Forever -PassThru -Batch:$false | Resume-Object -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
            
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
        ))
    }

    It "108.1.3b: Variable -> Action -PassThru -Batch:`$false -> Table" {
        $devices = Get-Device

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Resume-Object -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
            
            (Gen "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

            #endregion
            #region 108.1.4: Something -> Action -PassThru -Batch:$false -> Action -PassThru -Batch:$false -> Table

    It "108.1.4a: Table -> Action -PassThru -Batch:`$false -> Action -PassThru -Batch:`$false -> Table" {

        Get-Device | Pause-Object -Forever -PassThru -Batch:$false | Resume-Object -PassThru -Batch:$false | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)"  "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
        ))
    }

    It "108.1.4b: Variable -> Action -PassThru -Batch:`$false -> Action -PassThru -Batch:`$false -> Table" {
        $devices = Get-Device
        
        $devices | Pause-Object -Forever -PassThru -Batch:$false | Resume-Object -PassThru -Batch:$false | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Resuming PRTG Objects (Completed)"  "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

            #endregion
            #region 108.1.5: Something -> Action -PassThru -Batch:$false -> Action -PassThru -Batch:$false -> Action -> -PassThru -Batch:$false -> Table

    It "108.1.5a: Table -> Action -PassThru -Batch:`$false -> Action -PassThru -Batch:`$false -> Action -PassThru -Batch:`$false -> Table" {
        Get-Device | Pause-Object -Forever -PassThru -Batch:$false | Resume-Object -PassThru -Batch:$false | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$false | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)"  "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
        ))
    }

    It "108.1.5b: Variable -> Action -PassThru -Batch:`$false -> Action -PassThru -Batch:`$false -> Action -PassThru -Batch:`$false -> Table" {
        $devices = Get-Device

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Resume-Object -PassThru -Batch:$false | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$false | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device0' (ID: 3000) forever (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects"  "Pausing device 'Probe Device1' (ID: 3001) forever (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)
            (Gen "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100 "Retrieving all sensors")

            (Gen "Resuming PRTG Objects (Completed)"  "Resuming device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

            #endregion
            #region 108.1.6: Something -> Action -PassThru -Batch:$false -> Table -> Action -PassThru -Batch:$false -> Table -> Action -PassThru -Batch:$false

    It "108.1.6a: Table -> Action -PassThru -Batch:`$false -> Table -> Action -PassThru -Batch:`$false -> Table -> Action -PassThru -Batch:`$false" {

        Get-Probe | Pause-Object -Forever -PassThru -Batch:$false | Get-Group | Resume-Object -PassThru -Batch:$false | Get-Device | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50 "Retrieving all groups")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            ##########################################################################################

            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100 "Retrieving all groups")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
        ))
    }

    It "108.1.6b: Variable -> Action -PassThru -Batch:`$false -> Table -> Action -PassThru -Batch:`$false -> Table -> Action -PassThru -Batch:`$false" {
        $probes = Get-Probe
        
        $probes | Pause-Object -Forever -PassThru -Batch:$false | Get-Group | Resume-Object -PassThru -Batch:$false | Get-Device | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50 "Retrieving all groups")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.10' (ID: 1000) forever (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            ##########################################################################################

            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100 "Retrieving all groups")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device0' (ID: 3000) setting 'Interval' to '00:00:30' (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "Modify PRTG Object Settings (Completed)" "Setting object 'Probe Device1' (ID: 3001) setting 'Interval' to '00:00:30' (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' (ID: 1001) forever (2/2)" 100)
        ))
    }

            #endregion
            #region 108.1.7: Variable -> Action -PassThru -Batch:$false -> Select -Something -> Table

    It "108.1.7a: Variable -> Action -PassThru -Batch:`$false -> Select -First -> Table" {

        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -First 2 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40 "Retrieving all sensors")
        ))
    }

    It "108.1.7b: Variable -> Action -PassThru -Batch:`$false -> Select -Last -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -Last 2 | Get-Sensor
    }

    It "108.1.7c: Variable -> Action -PassThru -Batch:`$false -> Select -Skip -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -Skip 2 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100 "Retrieving all sensors")
        ))
    }
    
    It "108.1.7d: Variable -> Action -PassThru -Batch:`$false -> Select -SkipLast -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -SkipLast 2 | Get-Sensor

        #todo: this displays incorrectly because the statusdescriptions for records 2, 3 and 4 aren't updated
        #by Get-Sensor to display records 0, 1 and 2

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100 "Retrieving all sensors")
        ))
    }
    
    It "108.1.7e: Variable -> Action -PassThru -Batch:`$false -> Select -Index -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -Index 1,3 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80 "Retrieving all sensors")
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80 "Retrieving all sensors")
        ))
    }

            #endregion
            #region 108.1.8: Variable -> Action -PassThru -Batch:$false -> Select -Something -> Action

    It "108.1.8a: Variable -> Action -PassThru -PassThru -Batch:`$false -> Select -First -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -First 2 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (1/5)" 20)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/5)" 40)

            (Gen "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device1' (ID: 3001) (2/5)" 40)
        ))
    }
    
    It "108.1.8b: Variable -> Action -PassThru -Batch:`$false -> Select -Last -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -Last 2 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device3' (ID: 3003) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device4' (ID: 3004) (2/2)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device4' (ID: 3004) (2/2)" 100)
        ))
    }

    It "108.1.8c: Variable -> Action -PassThru -Batch:`$false -> Select -Skip -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -Skip 2 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device2' (ID: 3002) (3/5)" 60)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device3' (ID: 3003) (4/5)" 80)

            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device4' (ID: 3004) (5/5)" 100)

            (Gen "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device4' (ID: 3004) (5/5)" 100)
        ))
    }

    It "108.1.8d: Variable -> Action -PassThru -Batch:`$false -> Select -SkipLast -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -SkipLast 2 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device0' (ID: 3000) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device4' (ID: 3004) forever (5/5)" 100)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device2' (ID: 3002) (5/5)" 100)

            (Gen "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device2' (ID: 3002) (5/5)" 100)
        ))
    }

    It "108.1.8e: Variable -> Action -PassThru -Batch:`$false -> Select -Index -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$false | Select -Index 1,3 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' (ID: 3000) forever (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' (ID: 3001) forever (2/5)" 40)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device2' (ID: 3002) forever (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device3' (ID: 3003) forever (4/5)" 80)
            (Gen "Resuming PRTG Objects" "Resuming device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming device 'Probe Device3' (ID: 3003) (4/5)" 80)
        ))
    }

            #endregion
        #endregion
        #region 108.2: PassThru -Batch:$true
            #region 108.2.1: Something -> Action -PassThru -Batch:$true -> Table

    It "108.2.1a: Table -> Action -PassThru -Batch:`$true -> Table" {
        Get-Device | Pause-Object -Forever -PassThru -Batch:$true | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)
        ))
    }

    It "108.2.1b: Variable -> Action -PassThru -Batch:`$true -> Table" {
        $devices = Get-Device

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)
        ))
    }

            #endregion
            #region 108.2.2: Something -> Action -PassThru -Batch:$true -> Table -> Table

    It "108.2.2a: Table -> Action -PassThru -Batch:`$true -> Table -> Table" {

        Get-Group | Pause-Object -Forever -Batch:$true -PassThru | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100)
        ))
    }

    It "108.2.2b: Variable -> Action -PassThru -Batch:`$true -> Table -> Table" {
        $groups = Get-Group

        $groups | Pause-Object -Forever -Batch:$true -PassThru | Get-Device | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100 "Retrieving all devices")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing groups 'Windows Infrastructure0' and 'Windows Infrastructure1' forever (2/2)" 100)
        ))
    }

            #endregion
            #region 108.2.3: Something -> Action -PassThru -Batch:$true -> Action

    It "108.2.3a: Table -> Action -PassThru -Batch:`$true -> Action" {
        Get-Device | Pause-Object -Forever -PassThru -Batch:$true | Resume-Object -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
        ))
    }

    It "108.2.3b: Variable -> Action -PassThru -Batch:`$true -> Table" {
        $devices = Get-Device

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Resume-Object -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
        ))
    }

            #endregion
            #region 108.2.4: Something -> Action -PassThru -Batch:$true -> Action -PassThru -Batch:$true -> Table

    It "108.2.4a: Table -> Action -PassThru -Batch:`$true -> Action -PassThru -Batch:`$true -> Table" {

        Get-Device | Pause-Object -Forever -PassThru -Batch:$true | Resume-Object -PassThru -Batch:$true | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
        ))
    }

    It "108.2.4b: Variable -> Action -PassThru -Batch:`$true -> Action -PassThru -Batch:`$true -> Table" {
        $devices = Get-Device
        
        $devices | Pause-Object -Forever -PassThru -Batch:$true | Resume-Object -PassThru -Batch:$true | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
        ))
    }

            #endregion
            #region 108.2.5: Something -> Action -PassThru -Batch:$true -> Action -PassThru -Batch:$true -> Action -> -PassThru -Batch:$true -> Table

    It "108.2.5a: Table -> Action -PassThru -Batch:`$true -> Action -PassThru -Batch:`$true -> Action -PassThru -Batch:`$true -> Table" {
        Get-Device | Pause-Object -Forever -PassThru -Batch:$true | Resume-Object -PassThru -Batch:$true | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$true | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)

            (Gen "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "Modify PRTG Object Settings" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Modify PRTG Object Settings" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Modify PRTG Object Settings" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Modify PRTG Object Settings (Completed)" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100)
        ))
    }

    It "108.2.5b: Variable -> Action -PassThru -Batch:`$true -> Action -PassThru -Batch:`$true -> Action -PassThru -Batch:`$true -> Table" {
        $devices = Get-Device

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Resume-Object -PassThru -Batch:$true | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$true | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)

            (Gen "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "Modify PRTG Object Settings" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Modify PRTG Object Settings" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Modify PRTG Object Settings" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen "Modify PRTG Object Settings (Completed)" "Setting devices 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (2/2)" 100)
        ))
    }

            #endregion
            #region 108.2.6: Something -> Action -PassThru -Batch:$true -> Table -> Action -PassThru -Batch:$true -> Table -> Action -PassThru -Batch:$true

    It "108.2.6a: Table -> Action -PassThru -Batch:`$true -> Table -> Action -PassThru -Batch:`$true -> Table -> Action -PassThru -Batch:`$true" {

        Get-Probe | Pause-Object -Forever -PassThru -Batch:$true | Get-Group | Resume-Object -PassThru -Batch:$true | Get-Device | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$true

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)

            (Gen "Pausing PRTG Objects" "Queuing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing probe '127.0.0.11' (ID: 1001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            
            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            
            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects (Completed)" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                    (Gen3 "Resuming PRTG Objects" "Resuming groups 'Windows Infrastructure0', 'Windows Infrastructure1', 'Windows Infrastructure0' and 'Windows Infrastructure1' (4/4)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/4)" 25 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/4)" 25) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/4)" 25) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/4)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/4)" 50) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/4)" 50) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (3/4)" 75 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (3/4)" 75) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (3/4)" 75) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming groups 'Windows Infrastructure0', 'Windows Infrastructure1', 'Windows Infrastructure0' and 'Windows Infrastructure1' (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings" "Setting devices 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (8/8)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings (Completed)" "Setting devices 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (8/8)" 100)
        ))
    }

    It "108.2.6b: Variable -> Action -PassThru -Batch:`$true -> Table -> Action -PassThru -Batch:`$true -> Table -> Action -PassThru -Batch:`$true" {
        $probes = Get-Probe
        
        $probes | Pause-Object -Forever -PassThru -Batch:$true | Get-Group | Resume-Object -PassThru -Batch:$true | Get-Device | Set-ObjectProperty Interval 00:00:30 -PassThru -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing probe '127.0.0.11' (ID: 1001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all groups")
            
            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            ###################################################################

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all groups")
            
            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure0' (ID: 2000) (1/2)" 50)

            (Gen1 "Pausing PRTG Objects" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                    (Gen3 "Resuming PRTG Objects" "Queuing group 'Windows Infrastructure1' (ID: 2001) (2/2)" 100)

            (Gen1 "Pausing PRTG Objects (Completed)" "Pausing probes '127.0.0.10' and '127.0.0.11' forever (2/2)" 100) +
                (Gen2 "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100) +
                    (Gen3 "Resuming PRTG Objects" "Resuming groups 'Windows Infrastructure0', 'Windows Infrastructure1', 'Windows Infrastructure0' and 'Windows Infrastructure1' (4/4)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/4)" 25 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/4)" 25) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (1/4)" 25) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/4)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/4)" 50) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (2/4)" 50) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (3/4)" 75 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (3/4)" 75) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (ID: 2000) (3/4)" 75) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Resuming groups 'Windows Infrastructure0', 'Windows Infrastructure1', 'Windows Infrastructure0' and 'Windows Infrastructure1' (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings" "Setting devices 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (8/8)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (ID: 2001) (4/4)" 100) +
                (Gen2 "Modify PRTG Object Settings (Completed)" "Setting devices 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0', 'Probe Device1', 'Probe Device0' and 'Probe Device1' setting 'Interval' to '00:00:30' (8/8)" 100)
        ))
    }
    
            #endregion
            #region 108.2.7: Variable -> Action -PassThru -Batch:$true -> Select -Something -> Table
    
    It "108.2.7a: Variable -> Action -PassThru -Batch:`$true -> Select -First -> Table" {

        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -First 2 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
        ))
    }

    It "108.2.7b: Variable -> Action -PassThru -Batch:`$true -> Select -Last -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -Last 2 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (2/2)" 100 "Retrieving all sensors")
        ))
    }

    It "108.2.7c: Variable -> Action -PassThru -Batch:`$true -> Select -Skip -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -Skip 2 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device4' (ID: 3004) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device4' (ID: 3004) (1/1)" 100 "Retrieving all sensors")
        ))
    }

    It "108.2.7d: Variable -> Action -PassThru -Batch:`$true -> Select -SkipLast -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -SkipLast 2 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/3)" 33 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/3)" 66 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (ID: 3002) (3/3)" 100 "Retrieving all sensors")

            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)
        ))
    }

    It "108.2.7e: Variable -> Action -PassThru -Batch:`$true -> Select -Index -> Table" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -Index 1,3 | Get-Sensor

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device3' (ID: 3003) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (1/1)" 100 "Retrieving all sensors")
        ))
    }
    
            #endregion
            #region 108.2.8: Variable -> Action -PassThru -Batch:$true -> Select -Something -> Action
    
    It "108.2.8a: Variable -> Action -PassThru -PassThru -Batch:`$true -> Select -First -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -First 2 | Resume-Object -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device0' and 'Probe Device1' (2/2)" 100)
        ))
    }

    It "108.2.8b: Variable -> Action -PassThru -Batch:`$true -> Select -Last -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -Last 2 | Resume-Object -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (1/2)" 50)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (2/2)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device3' and 'Probe Device4' (2/2)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device3' and 'Probe Device4' (2/2)" 100)
        ))
    }

    It "108.2.8c: Variable -> Action -PassThru -Batch:`$true -> Select -Skip -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -Skip 2 | Resume-Object -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device2', 'Probe Device3' and 'Probe Device4' (3/3)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device2', 'Probe Device3' and 'Probe Device4' (3/3)" 100)
        ))
    }

    It "108.2.8d: Variable -> Action -PassThru -Batch:`$true -> Select -SkipLast -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -SkipLast 2 | Resume-Object -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (3/5)" 60)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (4/5)" 80)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (5/5)" 100)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device0', 'Probe Device1' and 'Probe Device2' (3/3)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device0', 'Probe Device1' and 'Probe Device2' (3/3)" 100)
        ))
    }

    It "108.2.8e: Variable -> Action -PassThru -Batch:`$true -> Select -Index -> Action" {
        $devices = Get-Device -Count 5

        $devices | Pause-Object -Forever -PassThru -Batch:$true | Select -Index 1,3 | Resume-Object -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/5)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (ID: 3002) (3/5)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device4' (ID: 3004) (5/5)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2', 'Probe Device3' and 'Probe Device4' forever (5/5)" 100)

            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/5)" 40)
            (Gen "Resuming PRTG Objects" "Queuing device 'Probe Device3' (ID: 3003) (4/5)" 80)
            (Gen "Resuming PRTG Objects" "Resuming devices 'Probe Device1' and 'Probe Device3' (2/2)" 100)
            (Gen "Resuming PRTG Objects (Completed)" "Resuming devices 'Probe Device1' and 'Probe Device3' (2/2)" 100)
        ))
    }

            #endregion
        #endregion
    #endregion
    #region 109: Watch

    It "109a: Displays watch progress" {

        WithResponseArgs "InfiniteLogValidatorResponse" @((Get-Date).AddMinutes(-1), "id=0&start=1") {
            Get-ObjectLog -Tail -Interval 0 | Select -First 7
        }

        Validate(@(
            (Gen "PRTG Log Watcher" "Waiting for first event")
            (Gen "PRTG Log Watcher" "Retrieving all logs (1/∞)" 0)
            (Gen "PRTG Log Watcher" "Retrieving all logs (2/∞)" 0)
            (Gen "PRTG Log Watcher" "Retrieving all logs (3/∞)" 0)
            (Gen "PRTG Log Watcher" "Retrieving all logs (4/∞)" 0)
            (Gen "PRTG Log Watcher" "Retrieving all logs (5/∞)" 0)
            (Gen "PRTG Log Watcher" "Retrieving all logs (6/∞)" 0)
            (Gen "PRTG Log Watcher" "Retrieving all logs (7/∞)" 0)
            (Gen "PRTG Log Watcher (Completed)" "Retrieving all logs (7/∞)" 0)
        ))
    }

    It "109b: Loops watch progress" {

        $total = 1101

        WithResponse "InfiniteLogResponse" {

            Get-ObjectLog -Tail -Interval 0 | Select -First $total
        }

        $progress = @()

        # Progress goes up to 1000 then resets again
        $progressLength = 1000

        for($i = 1; $i -le $total; $i++)
        {
            $p = [Math]::Floor(($i % $progressLength) / $progressLength * 100)

            $progress += (Gen "PRTG Log Watcher" "Retrieving all logs ($i/∞)" $p)
        }

        Validate(@(
            (Gen "PRTG Log Watcher" "Waiting for first event")
            $progress
            (Gen "PRTG Log Watcher (Completed)" "Retrieving all logs (1101/∞)" 10)
        ))
    }

    #endregion
    #region 110: Table Inside Action
        #region 110.1: Something -> Action (Table)

    It "110.1a: Table -> Action (Table)" {
        Get-Device -Count 2 | New-Sensor -WmiService *prtg* -Resolve:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100)
        ))
    }

    It "110.1b: Variable -> Action (Table)" {
        $devices = Get-Device -Count 2

        $devices | New-Sensor -WmiService *prtg* -Resolve:$false

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
        ))
    }

        #endregion
        #region 110.2: Something -> Action (Table) -> Table

    It "110.2a: Table -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100)
        ))
    }

    It "110.2b: Variable -> Action (Table) -> Table" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50 "Retrieving all channels")
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
        ))
    }

        #endregion
        #region 110.3: Something -> Action (Table) -> Action

    It "110.3a: Table -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
        ))
    }

    It "110.3b: Variable -> Action (Table) -> Action" {

        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
        ))
    }

        #endregion
        #region 110.4: Something -> Action (Table) -> Select -First -> Something

    It "110.4a: Table -> Action (Table) -> Select -First -> Table" {

        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -First 2 | Get-Channel
        }

        Assert-NoProgress
    }

    It "110.4b: Variable -> Action (Table) -> Select -First -> Table" {

        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -First 2 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)

            # Single progress message for retrieving channels from both sensors on the first device
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50 "Retrieving all channels")
        ))
    }

    It "110.4c: Table -> Action (Table) -> Select -First -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -First 2 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "110.4d: Variable -> Action (Table) -> Select -First -> Action" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -First 2 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 110.5: Something -> Action (Table) -> Select -Last -> Something

    It "110.5a: Table -> Action (Table) -> Select -Last -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -Last 2 | Get-Channel
        }

        Assert-NoProgress
    }

    It "110.5b: Variable -> Action (Table) -> Select -Last -> Table" {

        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -Last 2 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.5c: Table -> Action (Table) -> Select -Last -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -Last 2 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "110.5d: Variable -> Action (Table) -> Select -Last -> Action" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -Last 2 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)

            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 110.6: Something -> Action (Table) -> Skip -> Something

    It "110.6a: Table -> Action (Table) -> Select -Skip -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -Skip 2 | Get-Channel
        }

        Assert-NoProgress
    }

    It "110.6b: Variable -> Action (Table) -> Select -Skip -> Table" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -Skip 2 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.6c: Table -> Action (Table) -> Select -Skip -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -Skip 2 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "110.6d: Variable -> Action (Table) -> Select -Skip -> Action" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -Skip 2 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)

            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 110.7: Something -> Action (Table) -> Select -SkipLast -> Something

    It "110.7a: Table -> Action (Table) -> Select -SkipLast -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -SkipLast 2 | Get-Channel
        }

        Assert-NoProgress
    }

    It "110.7b: Variable -> Action (Table) -> Select -SkipLast -> Table" {

        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -SkipLast 2 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.7c: Table -> Action (Table) -> Select -SkipLast -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -SkipLast 2 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "110.7d: Variable -> Action (Table) -> Select -SkipLast -> Action" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -SkipLast 2 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)

            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 110.8: Something -> Action (Table) -> Select -Index -> Something

    It "110.8a: Table -> Action (Table) -> Select -Index -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -Index 2,3 | Get-Channel
        }

        Assert-NoProgress
    }

    It "110.8b: Variable -> Action (Table) -> Select -Index -> Table" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -Index 2,3 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.8c: Table -> Action (Table) -> Select -Index -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 2 | New-Sensor -WmiService *prtg*,netlogon | Select -Index 1,2 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "110.8d: Variable -> Action (Table) -> Select -Index -> Action" {
        $devices = Get-Device -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $devices | New-Sensor -WmiService *prtg*,netlogon | Select -Index 1,2 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device0' (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to Device 'Probe Device1' (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)

            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total3' and 'Volume IO _Total2' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total3' and 'Volume IO _Total2' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 110.9: Something -> Select -First -> Action (Table) -> Something

    It "110.9a: Table -> Select -First -> Action (Table) -> Table" {

        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -First 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/4)" 25)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25 "Retrieving all channels")

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
        ))
    }

    It "110.9b: Variable -> Select -First -> Action (Table) -> Table" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -First 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25 "Retrieving all channels") # First two sensors
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50 "Retrieving all channels") # Second two sensors
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50 "Retrieving all channels")
        ))
    }

    It "110.9c: Table -> Select -First -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -First 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/4)" 25)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
        ))
    }

    It "110.9d: Variable -> Select -First -> Action (Table) -> Action" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -First 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (1/4)" 25)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/4)" 50)

            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2', 'Volume IO _Total3', 'Volume IO _Total2' and 'Volume IO _Total3' forever (4/4)" 100)
        ))
    }

        #endregion
        #region 110.10: Something -> Select -Last -> Action (Table) -> Something

    It "110.10a: Table -> Select -Last -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -Last 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/4)" 25)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (ID: 3003) (4/4)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50 "Retrieving all channels")

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.10b: Variable -> Select -Last -> Action (Table) -> Table" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Last 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (1/2)" 50 "Retrieving all channels")

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device3' (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.10c: Table -> Select -Last -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -Last 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -Last -> Action -> Action illegal
        Assert-NoProgress
    }

    It "110.10d: Variable -> Select -Last -> Action (Table) -> Action" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Last 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -Last -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
        #region 110.11: Something -> Select -Skip -> Action (Table) -> Something

    It "110.11a: Table -> Select -Skip -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -Skip 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/4)" 25)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75 "Retrieving all channels")

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100)
        ))
    }

    It "110.11b: Variable -> Select -Skip -> Action (Table) -> Table" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Skip 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75 "Retrieving all channels")

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100 "Retrieving all channels")
        ))
    }

    It "110.11c: Table -> Select -Skip -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -Skip 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -Skip -> Action -> Action illegal
        Assert-NoProgress
    }

    It "110.11d: Variable -> Select -Skip -> Action (Table) -> Action" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Skip 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -Skip -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
        #region 110.12: Something -> Select -SkipLast -> Action (Table) -> Something

    It "110.12a: Table -> Select -SkipLast -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -SkipLast 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/4)" 25)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device0' (1/2)" 50 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/4)" 50) +
                (Gen2 "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device1' (2/2)" 100 "Retrieving all channels")
        ))
    }

    It "110.12b: Variable -> Select -SkipLast -> Action (Table) -> Table" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Skip 2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75 "Retrieving all channels")

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device3' (4/4)" 100 "Retrieving all channels")
        ))
    }

    It "110.12c: Table -> Select -SkipLast -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -SkipLast 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -SkipLast -> Action -> Action illegal
        Assert-NoProgress
    }

    It "110.12d: Variable -> Select -SkipLast -> Action (Table) -> Action" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -SkipLast 2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -SkipLast -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
        #region 110.13: Something -> Select -Index -> Action (Table) -> Something

    It "110.13a: Table -> Select -Index -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -Index 1,2 | New-Sensor -WmiService *prtg* | Get-Channel
        }
        
        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50 "Retrieving all channels")

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
        ))
    }

    It "110.13b: Variable -> Select -Index -> Action (Table) -> Table" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Index 1,2 | New-Sensor -WmiService *prtg* | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device1' (2/4)" 50 "Retrieving all channels")

            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75)
            (Gen "Adding PRTG Sensors" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding WmiService sensor to device 'Probe Device2' (3/4)" 75 "Retrieving all channels")
        ))
    }

    It "110.13c: Table -> Select -Index -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Device -Count 4 | Select -Index 1,2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -Index -> Action -> Action illegal
        Assert-NoProgress
    }

    It "110.13d: Variable -> Select -Index -> Action (Table) -> Action" {
        $devices = Get-Device -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $devices | Select -Index 1,2 | New-Sensor -WmiService *prtg* | Pause-Object -Forever
        }

        # Select -Index -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
    #endregion
    #region 111: Table Inside Action (End Processing)
        #region 111.1: Something -> Action (Table)

    It "111.1a: Table -> Action (Table)" {
        Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 -Resolve:$false

        # No New-Sensor progress because by the time we get to it "previous did not contain progress" cos it already ended
        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "111.1b: Variable -> Action (Table)" {
        $sensors = Get-Sensor -Count 2

        $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 -Resolve:$false

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
        ))
    }

        #endregion
        #region 111.2: Something -> Action (Table) -> Table

    It "111.2a: Table -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
        ))
    }

    It "111.2b: Variable -> Action (Table) -> Table" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
        ))
    }

        #endregion
        #region 111.3: Something -> Action (Table) -> Action

    It "111.3a: Table -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

    It "111.3b: Variable -> Action (Table) -> Action" {

        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)

            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 111.4: Something -> Action (Table) -> Select -First -> Something

    It "111.4a: Table -> Action (Table) -> Select -First -> Table" {

        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -First 1 | Get-Channel
        }

        Assert-NoProgress
    }

    It "111.4b: Variable -> Action (Table) -> Select -First -> Table" {

        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -First 1 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
        ))
    }

    It "111.4c: Table -> Action (Table) -> Select -First -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -First 1 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "111.4d: Variable -> Action (Table) -> Select -First -> Action" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -First 1 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)

            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' forever (1/1)" 100)
        ))
    }

        #endregion
        #region 111.5: Something -> Action (Table) -> Select -Last -> Something

    It "111.5a: Table -> Action (Table) -> Select -Last -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Last 1 | Get-Channel
        }

        Assert-NoProgress
    }

    It "111.5b: Variable -> Action (Table) -> Select -Last -> Table" {

        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Last 1 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total3' (ID: 1003) (1/1)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total3' (ID: 1003) (1/1)" 100 "Retrieving all channels")
        ))
    }

    It "111.5c: Table -> Action (Table) -> Select -Last -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Last 1 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "111.5d: Variable -> Action (Table) -> Select -Last -> Action" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Last 1 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (1/1)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' forever (1/1)" 100)
        ))
    }

        #endregion
        #region 111.6: Something -> Action (Table) -> Skip -> Something

    It "111.6a: Table -> Action (Table) -> Select -Skip -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Skip 1 | Get-Channel
        }

        Assert-NoProgress
    }

    It "111.6b: Variable -> Action (Table) -> Select -Skip -> Table" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Skip 2 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
        ))
    }

    It "111.6c: Table -> Action (Table) -> Select -Skip -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Skip 2 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "111.6d: Variable -> Action (Table) -> Select -Skip -> Action" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Skip 1 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total3' forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total3' forever (1/1)" 100)
        ))
    }

        #endregion
        #region 111.7: Something -> Action (Table) -> Select -SkipLast -> Something

    It "111.7a: Table -> Action (Table) -> Select -SkipLast -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -SkipLast 1 | Get-Channel
        }

        Assert-NoProgress
    }

    It "111.7b: Variable -> Action (Table) -> Select -SkipLast -> Table" {

        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -SkipLast 1 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
        ))
    }

    It "111.7c: Table -> Action (Table) -> Select -SkipLast -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -SkipLast 1 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "111.7d: Variable -> Action (Table) -> Select -SkipLast -> Action" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -SkipLast 1 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' forever (1/1)" 100)
        ))
    }

        #endregion
        #region 111.8: Something -> Action (Table) -> Select -Index -> Something

    It "111.8a: Table -> Action (Table) -> Select -Index -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Index 0 | Get-Channel
        }

        Assert-NoProgress
    }

    It "111.8b: Variable -> Action (Table) -> Select -Index -> Table" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Index 0 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0 "Retrieving all channels")
        ))
    }

    It "111.8c: Table -> Action (Table) -> Select -Index -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Index 0 | Pause-Object -Forever
        }

        Assert-NoProgress
    }

    It "111.8d: Variable -> Action (Table) -> Select -Index -> Action" {
        $sensors = Get-Sensor -Count 2

        WithResponse "DiffBasedResolveResponse" {
            $sensors | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Select -Index 0 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/2)" 0)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total2' forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total2' forever (1/1)" 100)
        ))
    }

        #endregion
        #region 111.9: Something -> Select -First -> Action (Table) -> Something

    It "111.9a: Table -> Select -First -> Action (Table) -> Table" {

        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -First 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/4)" 50)
        ))
    }

    It "111.9b: Variable -> Select -First -> Action (Table) -> Table" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -First 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (2/4)" 50)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (2/4)" 50 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (2/4)" 50 "Retrieving all channels")
        ))
    }

    It "111.9c: Table -> Select -First -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -First 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

    It "111.9d: Variable -> Select -First -> Action (Table) -> Action" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -First 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total2' (ID: 1002) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total3' (ID: 1003) (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensors 'Volume IO _Total2' and 'Volume IO _Total3' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 111.10: Something -> Select -Last -> Action (Table) -> Something

    It "111.10a: Table -> Select -Last -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -Last 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total3' (ID: 4003) (4/4)" 100)

            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (1/1)" 100)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (1/1)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (1/1)" 100 "Retrieving all channels")
        ))
    }

    It "111.10b: Variable -> Select -Last -> Action (Table) -> Table" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Last 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0 "Retrieving all channels")
        ))
    }

    It "111.10c: Table -> Select -Last -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -Last 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -Last -> Action -> Action illegal
        Assert-NoProgress
    }

    It "111.10d: Variable -> Select -Last -> Action (Table) -> Action" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Last 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -Last -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
        #region 111.11: Something -> Select -Skip -> Action (Table) -> Something

    It "111.11a: Table -> Select -Skip -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -Skip 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total3' (ID: 4003) (4/4)" 100)
        ))
    }

    It "111.11b: Variable -> Select -Skip -> Action (Table) -> Table" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Skip 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0 "Retrieving all channels")
        ))
    }

    It "111.11c: Table -> Select -Skip -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -Skip 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -Skip -> Action -> Action illegal
        Assert-NoProgress
    }

    It "111.11d: Variable -> Select -Skip -> Action (Table) -> Action" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Skip 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -Skip -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
        #region 111.12: Something -> Select -SkipLast -> Action (Table) -> Something

    It "111.12a: Table -> Select -SkipLast -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -SkipLast 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (ID: 4001) (2/4)" 50)

            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (1/1)" 100)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (1/1)" 100 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (1/1)" 100 "Retrieving all channels")
        ))
    }

    It "111.12b: Variable -> Select -SkipLast -> Action (Table) -> Table" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Skip 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (0/4)" 0 "Retrieving all channels")
        ))
    }

    It "111.12c: Table -> Select -SkipLast -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -SkipLast 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -SkipLast -> Action -> Action illegal
        Assert-NoProgress
    }

    It "111.12d: Variable -> Select -SkipLast -> Action (Table) -> Action" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -SkipLast 2 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -SkipLast -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
        #region 111.13: Something -> Select -Index -> Action (Table) -> Something

    It "111.13a: Table -> Select -Index -> Action (Table) -> Table" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -Index 0 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }
        
        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/4)" 25)
        ))
    }

    It "111.13b: Variable -> Select -Index -> Action (Table) -> Table" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Index 0 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Get-Channel
        }

        Validate(@(
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (1/4)" 25)
            (Gen "Adding PRTG Sensors" "Adding Factory sensor 'Test' to device ID '1001' (1/4)" 25 "Retrieving all channels")
            (Gen "Adding PRTG Sensors (Completed)" "Adding Factory sensor 'Test' to device ID '1001' (1/4)" 25 "Retrieving all channels")
        ))
    }

    It "111.13c: Table -> Select -Index -> Action (Table) -> Action" {
        WithResponseArgs "DiffBasedResolveResponse" 2 {
            Get-Sensor -Count 4 | Select -Index 0 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -Index -> Action -> Action illegal
        Assert-NoProgress
    }

    It "111.13d: Variable -> Select -Index -> Action (Table) -> Action" {
        $sensors = Get-Sensor -Count 4

        WithResponse "DiffBasedResolveResponse" {
            $sensors | Select -Index 0 | New-Sensor -Factory "Test" { $_.Device } -DestinationId 1001 | Pause-Object -Forever
        }

        # Select -Index -> Action -> Action illegal
        Assert-NoProgress
    }

        #endregion
    #endregion
    #region Sanity Checks

    It "Streams when the number of returned objects is above the threshold" {
        RunCustomCount @{ Sensors = 501 } {
            Get-Sensor
        }        

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (GenerateStreamRecords 501)
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors (501/501)" 100)
        ))
    }

    It "pipes a single grouping to a table" {
        
        $groups = Get-Device -Count 2 | group Probe

        $groups[0].Group | Get-Sensor

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
        ))
    }

    It "pipes a single grouping to an action" {
        
        $groups = Get-Device -Count 2 | group Probe

        $groups[0].Group | Pause-Object -Forever

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0' and 'Probe Device1' forever (2/2)" 100)
        ))
    }

        #region No Progress

    It "Doesn't stream when the number of returned objects is below the threshold" {
        Get-Sensor

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Detecting total number of items"

            "PRTG Sensor Search (Completed)`n" +
            "    Detecting total number of items"
        ))
    }

    It "Doesn't show progress when a variable contains only 1 object" {
        $probe = Get-Probe -Count 1

        $probe.Count | Should Be 1

        $sensors = $probe | Get-Sensor

        Assert-NoProgress
    }

    It "Doesn't show progress when using Table -> Where" {
        Get-Device | where name -EQ "Probe Device0"

        Assert-NoProgress
    }

    It "Doesn't show progress when using Table -> Where -> Other" {
        Get-Device | where name -EQ "Probe Device0" | fl
        
        Assert-NoProgress
    }
    
    It "Doesn't show progress when containing three Select-Object skip cmdlets piping to a table" {
        Get-Probe -Count 13 | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Get-Device

        Assert-NoProgress
    }

    It "Doesn't show progress when containing three Select-Object skip cmdlets piping to an action" {
        Get-Probe -Count 13 | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "Doesn't show progress when containing three Select-Object skip cmdlets piping from a variable to a table" {
        $probes = Get-Probe -Count 13

        $probes | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Get-Device

        Assert-NoProgress
    }

    It "Doesn't show progress when containing three Select-Object skip cmdlets piping from a variable to an action" {
        $probes = Get-Probe -Count 13

        $probes | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping to a table" {
        Get-Probe -Count 13 | Select -Last 10 | Select -First 7 | Select -Last 4 | Get-Device

        Assert-NoProgress
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping to an action" {
        Get-Probe -Count 13 | Select -Last 10 | Select -First 7 | Select -Last 4 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping from a variable to a table" {
        $probes = Get-Probe -Count 13

        $probes | Select -Last 10 | Select -First 7 | Select -Last 4 | Get-Device

        Assert-NoProgress
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping from a variable to an action" {
        $probes = Get-Probe -Count 13
        
        $probes | Select -Last 10 | Select -First 7 | Select -Last 4 | Pause-Object -Forever -Batch:$false

        Assert-NoProgress
    }

    It "Doesn't show progress containing three parameters over two Select-Object cmdlets" {
        Get-Probe -Count 13 | Select -First 4 -Skip 1 | Select -First 2 | Get-Device

        Assert-NoProgress
    }

        #endregion
        #region No Results

    It "Completes all progress records when no results are returned when piping from a cmdlet to a Table" {
        Get-Probe -Count 0 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search (Completed)"  "Retrieving all probes")
        ))
    }

    It "Completes all progress records when no results are returned when piping from a cmdletto an Action" {
        Get-Probe -Count 0 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search (Completed)"  "Retrieving all probes")
        ))
    }

    It "Completes all progress records when no results are returned when piping from a variable to a Table" {
        
        $probes = Get-Probe -Count 2

        $probes | Get-Device -Count 0 | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)"  "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "Completes all progress records when no results are returned when piping from a variable  to an Action" {
        $probes = Get-Probe -Count 2

        $probes | Get-Device -Count 0 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)"  "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

        #endregion
        #region Throw Completes

    It "Table -> Table (Throw) Completes" {

        WithResponseArgs "FaultyTableResponse" (GetCustomCountDictionary @{ Devices = 1 }) {
            { Get-Probe | Get-Device } | Should Throw "Requested content 'Devices' too many times"
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
        ))
    }

    It "Table -> Table -> Table (Throw) Completes" {

        WithResponseArgs "FaultyTableResponse" (GetCustomCountDictionary @{ Sensors = 1 }) {
            { Get-Probe | Get-Device | Get-Sensor } | Should Throw "Requested content 'Sensors' too many times"
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
        ))
    }

    It "Table -> Table (Throw) -> Table Completes" {
        WithResponseArgs "FaultyTableResponse" (GetCustomCountDictionary @{ Devices = 2 }) {
            { Get-Probe -Count 3 | Get-Device -Count 1 | Get-Sensor } | Should Throw "Requested content 'Devices' too many times"
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "Table -> Action (Throw) Completes" {
         WithResponseArgs "FaultyTableResponse" "Pause" {
            { Get-Sensor | Pause-Object -Forever -Batch:$false } | Should Throw "Requested function 'Pause'"
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50) # Pause-Object cleans up his cloned record
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' (ID: 4000) forever (1/2)" 50) # Get-Sensor cleans up the original record
        ))
    }

    It "Variable -> Table (Throw) Completes" {

        WithResponseArgs "FaultyTableResponse" (GetCustomCountDictionary @{ Devices = 2 }) {
            { $probes = Get-Probe -Count 3; $probes | Get-Device } | Should Throw "Requested content 'Devices' too many times"
        }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (ID: 1000) (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "Table -> Action (Throw) -Batch:`$true Completes" {
        # When an exception is thrown in the EndProcessing block it should complete
        # We force ErrorActionPreference = 'Continue' so that we can clean up our progress after the
        # ErrorRecord is written (which wouldn't happen if we were ErrorActionPreference = 'Stop')

        Get-Sensor -Count 1 | Set-ObjectProperty PrimaryChannel *foo* -ErrorAction Continue

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "Modify PRTG Object Settings" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "Modify PRTG Object Settings (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/1)")
        ))
    }

    It "Variable -> Action (Throw) -Batch:`$true Completes" {
        # When an exception is thrown in the EndProcessing block it should complete
        # We force ErrorActionPreference = 'Continue' so that we can clean up our progress after the
        # ErrorRecord is written (which wouldn't happen if we were ErrorActionPreference = 'Stop')

        $sensors = Get-Sensor -Count 2

        $sensors | Set-ObjectProperty PrimaryChannel *foo* -ErrorAction Continue

        Validate(@(
            (Gen "Modify PRTG Object Settings" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "Modify PRTG Object Settings (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)")
        ))
    }

    It "Table -> NonTerminatingException When ErrorActionPreference = 'Stop' Completes'" {
        { Get-Sensor -Count 1 | Set-ObjectProperty PrimaryChannel *foo* } | Should Throw "Channel wildcard '*foo*' does not exist on sensor ID 4000"

        Validate(@(
            (Gen "PRTG Sensor Search" "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "Modify PRTG Object Settings" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)
            (Gen "Modify PRTG Object Settings (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/1)")
        ))
    }

    It "Variable -> NonTerminatingException When ErrorActionPreference = 'Stop' Completes'" {
        $sensors = Get-Sensor -Count 2

        { $sensors | Set-ObjectProperty PrimaryChannel *foo* } | Should Throw "Channel wildcard '*foo*' does not exist on sensor ID 4000"

        Validate(@(
            (Gen "Modify PRTG Object Settings" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)
            (Gen "Modify PRTG Object Settings" "Queuing sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)
            (Gen "Modify PRTG Object Settings (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/2)")
        ))
    }

        #endregion
        
    It "Selects a property" {
        Get-Probe | Get-Device | Select Name

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (ID: 1000) (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (ID: 1001) (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "Action in First -> Second -> Action -Batch:`$true retains ownership of progress record after first seized" {
        # 1. First will always emit an object.
        # 2. Second will emit an object the first time it is called
        # 3. First time Action is called it will seize ownership of Second's object
        # 4. Second does not emit any additional objects

        # Second's ProcessRecord will open and close several times. Action should always be declared owner
        # on all subsequent calls

        WithResponse "ProgressOwnershipResponse" {
            Get-Device | Get-Sensor "Volume IO _Total0" | Pause-Object -Forever
        }

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)
        ))
    }

    #endregion
}