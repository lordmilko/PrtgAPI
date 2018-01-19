. $PSScriptRoot\Support\Progress.ps1

Describe "Test-Progress" -Tag @("PowerShell", "UnitTest") {
    
    InitializeClient
    
    $filter = "*"
    $ignoreNotImplemented = $false

    #region 1: Something -> Action
    
    It "1a: Table -> Action" {
        Get-Sensor -Count 1 | Pause-Object -Forever -Batch:$false

        Validate (@(
            (Gen "PRTG Sensor Search"               "Retrieving all sensors")
            (Gen "PRTG Sensor Search"               "Processing sensor 'Volume IO _Total0' (1/1)" 100)
            (Gen "Pausing PRTG Objects"             "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)
        ))
    }
    
    It "1b: Variable -> Action" {
        $devices = Get-Device

        $devices.Count | Should Be 2

        $devices | Pause-Object -Forever -Batch:$false

        Validate (@(
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)
        ))
    }

    #endregion
    #region 2: Something -> Table

    It "2a: Table -> Table" {
        Get-Probe | Get-Group

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
        ))
    }

    It "2b: Variable -> Table" {

        $probes = Get-Probe

        $probes.Count | Should Be 2

        $probes | Get-Sensor

        Validate (@(
            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all sensors")
        ))
    }

    #endregion
    #region 3: Something -> Action -> Table
    
    It "3a: Table -> Action -> Table" {

        Get-Device | Clone-Object 5678 | Get-Sensor

        Validate (@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "3b: Variable -> Action -> Table" {

        $devices = Get-Device

        $devices | Clone-Object 5678 | Get-Sensor

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")
        ))
    }

    #endregion
    #region 4: Something -> Table -> Table

    It "4a: Table -> Table -> Table" {

        Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

        Validate (@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/1)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/1)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)
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
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

    #endregion
    #region 5: Something -> Table -> Action
    
    It "5a: Table -> Table -> Action" {
        Get-Device | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)
        ))
    }

    It "5b: Variable -> Table -> Action" {
        $devices = Get-Device

        $devices | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)
        ))
    }

    #endregion
    #region 6: Something -> Table -> Action -> Table
    
    It "6a: Table -> Table -> Action -> Table" {
        Get-Group | Get-Device | Clone-Object 5678 | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "6b: Variable -> Table -> Action -> Table" {
        $probes = Get-Probe

        $probes | Get-Group -Count 1 | Clone-Object 5678 | Get-Device

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "Cloning PRTG Groups (Completed)" "Cloning group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100)

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "Cloning PRTG Groups" "Cloning group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "Cloning PRTG Groups (Completed)" "Cloning group 'Windows Infrastructure0' (ID: 2000) (1/1)" 100 "Retrieving all devices")

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

    #endregion
    #region 7: Something -> Object

    It "7a: Table -> Object" {
        Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search"             "Processing sensor 'Volume IO _Total0' (1/1)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")
        ))
    }

    It "7b: Variable -> Object" {

        #1. why is pipes three data cmdlets together being infected by the crash here
        #2. why is injected_showchart failing to deserialize?

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2
        $sensors | Get-Channel

        Validate(@(
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
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

            $c = $i

            while($c -gt 500)
            {
                $c -= 500
            }

            $records += "PRTG Sensor Search`n" +
                        "    Processing sensor 'Volume IO _Total$($c - 1)' ($i/$total)`n" +
                        "    $percentBar`n" +
                        "    Retrieving all channels"
        }

        Validate(@(
            (Gen "PRTG Sensor Search" "Detecting total number of items")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/501)" 0)

            $records

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (501/501)" 100 "Retrieving all channels")
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

            if($i -ge 501)
            {
                $nameSuffix -= 500
            }

            $records += "Pausing PRTG Objects`n" +
                        "    Pausing sensor 'Volume IO _Total$nameSuffix' forever ($i/$total)`n" +
                        "    $percentBar"
        }

        #todo: maybe try replace the original stream one with this

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Detecting total number of items"

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/501)" 0)

            $records

            (Gen "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (501/501)" 100)
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
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)
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
            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

    #endregion
    #region 10: Something -> Action -> Table -> Table
    
    It "10a: Table -> Action -> Table -> Table" {
        Get-Device | Clone-Object 5678 | Get-Sensor | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "10b: Variable -> Action -> Table -> Table" {
        # an extension of 3b. variable -> action -> table. Confirms that we can transform our setpreviousoperation into a
        # proper progress item when required

        #BUG----------------we should have TWO sensors in our progress, but we're only showing 1???

        #RunCustomCount $counts {
            $devices = Get-Device

            $devices | Clone-Object 5678 | Get-Sensor | Get-Channel        
        #}

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")
            
            ###################################################################

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            
            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")            

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            
            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }
    
    #endregion
    #region 11: Variable -> Table -> Table -> Table

    It "11: Variable -> Table -> Table -> Table" {
        # Validates we can get at least two progress bars out of a variable
        $probes = Get-Probe

        $probes | Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            ###################################################################

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            ###################################################################

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

    #endregion
    #region 12: Something -> Select -First -> Something
        #region 12.1: Something -> Select -First -> Something

    It "12.1a: Table -> Select -First -> Table" {
        Get-Probe -Count 3 | Select -First 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)" 33)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1b: Table -> Select -First -> Action" {
        Get-Probe -Count 3 | Select -First 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/3)"                     33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/3)" 66)
        ))
    }

    It "12.1c: Variable -> Select -First -> Table" {

        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1d: Variable -> Select -First -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/3)" 66)
        ))
    }

    It "12.1e: Table -> Select -First -Wait -> Table" {
        Get-Probe -Count 3 | Select -First 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)" 33)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1f: Table -> Select -First -Wait -> Action" {
        Get-Probe -Count 3 | Select -First 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/3)"                     33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/3)" 66)
        ))
    }

    It "12.1g: Variable -> Select -First -Wait -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
        ))
    }

    It "12.1h: Variable -> Select -First -Wait -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/3)" 33)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/3)" 66)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/3)" 66)
        ))
    }

        #endregion
        #region 12.2: Table -> Select -First -Something -> Table

    It "12.2a: Table -> Select -First -Last -> Table" {
        Get-Probe -Count 10 | Select -First 4 -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.18' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "12.2b: Table -> Select -First -Skip -> Table" {
        Get-Probe -Count 10 | Select -First 4 -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

        #endregion
        #region 12.3: Table -> Select -First -> Select -Something -> Table

    It "12.3a: Table -> Select -First -> Select -Last -> Table" {
        Get-Probe -Count 10 | Select -First 4 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "12.3b: Table -> Select -First -> Select -Skip -> Table" {
        Get-Probe -Count 10 | Select -First 4 | Select -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
        ))
    }

    It "12.3c: Table -> Select -First -> Select -SkipLast -> Table" {
        Get-Probe -Count 10 | Select -First 4 | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/10)"     10)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (2/10)"     20)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/10)"     20) + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/8)" 12 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/10)"     20) + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/8)" 25 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/10)"     20) + 
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/8)" 25 "Retrieving all devices")
        ))
    }

    It "12.3d: Table -> Select -First -> Select -Index -> Table" {
        Get-Probe -Count 10 | Select -First 5 | Select -Index 2,4 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
        ))
    }

        #endregion
        #region 12.4: Table -> Select -First -Something -> Action

    It "12.4a: Table -> Select -First -Last -> Action" {
        Get-Probe -Count 10 | Select -First 4 -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/10)" 10)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/10)" 20)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/10)" 40)

            ###################################################################

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.18' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.19' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.19' forever (2/2)" 100)
        ))
    }

    It "12.4b: Table -> Select -First -Skip -> Action" {
        Get-Probe -Count 10 | Select -First 4 -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/10)"                     10)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (6/10)" 60)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

        #endregion
        #region 12.5: Table -> Select -First -> Select -Something -> Action

    It "12.5a: Table -> Select -First -> Select -Last -> Action" {
        Get-Probe -Count 10 | Select -First 4 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/10)" 10)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (2/2)" 100)
        ))
    }

    It "12.5b: Table -> Select -First -> Select -Skip -> Action" {
        Get-Probe -Count 10 | Select -First 4 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/10)"                     10)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)

            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/10)" 40)
        ))
    }

    It "12.5c: Table -> Select -First -> Select -SkipLast -> Action" {
        Get-Probe -Count 10 | Select -First 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (1/10)"     10)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (2/10)"     20) + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/8)" 12)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (2/10)"     20) + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/8)" 25)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (2/10)"     20) + 
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/8)" 25)
        ))
    }

    It "12.5d: Table -> Select -First -> Select -Index -> Action" {
        Get-Probe -Count 10 | Select -First 5 | Select -Index 2,4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.14' forever (5/10)" 50)
        ))
    }

        #endregion
        #region 12.6: Variable -> Select -First -Something -> Table

    It "12.6a: Variable -> Select -First -Last -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/10)" 10 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/10)" 20 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.18' (9/10)" 90 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.19' (10/10)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.19' (10/10)" 100 "Retrieving all devices")
        ))
    }

    It "12.6b: Variable -> Select -First -Skip -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

        #endregion
        #region 12.7: Variable -> Select -First -> Select -Something -> Table

    It "12.7a: Variable -> Select -First -> Select -Last -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "12.7b: Variable -> Select -First -> Select -Skip -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 | Select -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
        ))
    }

    It "12.7c: Variable -> Select -First -> Select -SkipLast -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (1/8)" 12 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (2/8)" 25 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/8)" 25 "Retrieving all devices")
        ))
    }

    It "12.7d: Variable -> Select -First -> Select -Index -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 5 | Select -Index 2,4 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
        ))
    }

        #endregion
        #region 12.8: Variable -> Select -First -Something -> Action

    It "12.8a: Variable -> Select -First -Last -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/10)" 10)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/10)" 20)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.18' forever (9/10)" 90)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.19' forever (10/10)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.19' forever (10/10)" 100)
        ))
    }

    It "12.8b: Variable -> Select -First -Skip -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

        #endregion
        #region 12.9: Variable -> Select -First -> Select -Something -> Action

    It "12.9a: Variable -> Select -First -> Select -Last -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (2/2)" 100)
        ))
    }

    It "12.9b: Variable -> Select -First -> Select -Skip -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/10)" 40)
        ))
    }

    It "12.9c: Variable -> Select -First -> Select -SkipLast -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/2)" 100)
        ))
    }

    It "12.9d: Variable -> Select -First -> Select -Index -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -First 5 | Select -Index 2,4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.14' forever (5/10)" 50)
        ))
    }

        #endregion
        #region 12.10: Something -> Select -First -> Table -> Something
    
    It "12.10a: Table -> Select -First -> Table -> Table" {
        Get-Probe -Count 3 | Select -First 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)"        33)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)"        33 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)"       33) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (1/2)"      50)

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)"       33) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (1/2)"      50 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)"       33) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device1' (2/2)"      100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.10' (1/3)"       33) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)"        66)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)"        66 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)"       66) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (1/2)"      50)

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)"       66) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device0' (1/2)"      50 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)"       66) +
                (Gen2 "PRTG Device Search"        "Processing device 'Probe Device1' (2/2)"      100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/3)"       66) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.11' (2/3)"        66)
        ))
    }

    It "12.10b: Table -> Select -First -> Table -> Action" {
        Get-Probe -Count 3 | Select -First 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (1/3)"                         33)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (1/3)"                         33 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (1/3)"                         33) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (1/2)"                        50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (1/3)"                         33) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (1/3)"                         33) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.10' (1/3)"                         33) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (2/3)"                         66)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (2/3)"                         66 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"                         66) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (1/2)"                        50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"                         66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"                         66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################
            
            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"                         66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search (Completed)"         "Processing probe '127.0.0.11' (2/3)"                         66)
        ))
    }

    It "12.10c: Variable -> Select -First -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"        "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)"  50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)"  100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)"  100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"        "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)"  50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)"  100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)"  100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/3)" 66)
        ))
    }

    It "12.10d: Variable -> Select -First -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -First 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"        "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)"  50)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)"  100)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)"  100)

            ###################################################################

            (Gen "PRTG Device Search"        "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)"  50)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)"  100)

            (Gen1 "PRTG Device Search"       "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)"  100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/3)" 66)
        ))
    }

        #endregion
        #region 12.11: Something -> Select -First -Something -> Table -> Something

    It "12.11a<i>: Table -> Select -First -<name> -> Table -> Table" -TestCases $selectFirstParams {
        param($name)

        TestCmdletChainWithSingle $name "First" "Get-Sensor"
    }

    It "12.11b<i>: Table -> Select -First -<name> -> Table -> Action" -TestCases $selectFirstParams {
        param($name)

        TestCmdletChainWithSingle $name "First" "Pause-Object -Forever -Batch:`$false"
    }

    It "12.11c<i>: Variable -> Select -First -<name> -> Table -> Table" -TestCases $selectFirstParams {
        param($name)

        TestVariableChainWithSingle $name "First" "Get-Sensor"
    }

    It "12.11d<i>: Variable -> Select -First -<name> -> Table -> Action" -TestCases $selectFirstParams {
        param($name)

        TestVariableChainWithSingle $name "First" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 12.12: Something -> Select -First -> Select -Something -> Table -> Something

    It "12.12a<i>: Table -> Select -First -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "First" "Get-Sensor"
    }

    It "12.12b<i>: Table -> Select -First -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "First" "Pause-Object -Forever -Batch:`$false"
    }

    It "12.12c<i>: Variable -> Select -First -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "First" "Get-Sensor"
    }

    It "12.12d<i>: Variable -> Select -First -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "First" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
    #endregion
    #region 13: Something -> Select -Last -> Something
        #region 13.1: Something -> Select -Last -> Something

    It "13.1a: Table -> Select -Last -> Table" {
        Get-Probe -Count 4 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (1/4)"       25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (2/4)"       50)
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.11' (2/4)"       50)

            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1b: Table -> Select -Last -> Action" {
        Get-Probe -Count 4 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/4)"                     25)
            (Gen "PRTG Probe Search (Completed)"    "Processing probe '127.0.0.11' (2/4)"                     50)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (2/2)" 100)
        ))
    }

    It "13.1c: Variable -> Select -Last -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1d: Variable -> Select -Last -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (2/2)" 100)
        ))
    }

    It "13.1e: Table -> Select -Last -Wait -> Table" {
        Get-Probe -Count 4 | Select -Last 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (1/4)"       25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (2/4)"       50)
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.11' (2/4)"       50)

            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1f: Table -> Select -Last -Wait -> Action" {
        Get-Probe -Count 4 | Select -Last 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/4)"                     25)
            (Gen "PRTG Probe Search (Completed)"    "Processing probe '127.0.0.11' (2/4)"                     50)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (2/2)" 100)
        ))
    }

    It "13.1g: Variable -> Select -Last -Wait -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1h: Variable -> Select -Last -Wait -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Last 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 13.2: Table -> Select -Last -Something -> Table

    It "13.2a: Table -> Select -Last -First -> Table" {
        Get-Probe -Count 10 | Select -Last 2 -First 4 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.18' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.2b: Table -> Select -Last -Skip -> Table" {
        Get-Probe -Count 10 | Select -Last 2 -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.14' (5/10)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.15' (6/10)" 60)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.16' (7/10)" 70)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.17' (8/10)" 80)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.17' (8/10)" 80)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.16' (1/4)" 25 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
        ))
    }

        #endregion
        #region 13.3: Table -> Select -Last -> Select -Something -> Table

    It "13.3a: Table -> Select -Last -> Select -First -> Table" {
        Get-Probe -Count 10 | Select -Last 4 | Select -First 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.3b: Table -> Select -Last -> Select -Skip -> Table" {
        Get-Probe -Count 10 | Select -Last 4 | Select -Skip 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.3c: Table -> Select -Last -> Select -SkipLast -> Table" {
        Get-Probe -Count 10 | Select -Last 4 | Select -SkipLast 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.3d: Table -> Select -Last -> Select -Index -> Table" {
        Get-Probe -Count 10 | Select -Last 4 | Select -Index 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 13.4: Table -> Select -Last -Something -> Action

    It "13.4a: Table -> Select -Last -First -> Action" {
        Get-Probe -Count 10 | Select -Last 2 -First 4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/10)" 10)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/10)" 20)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/10)" 40)

            ###################################################################

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.18' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.19' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.19' forever (2/2)" 100)
        ))
    }

    It "13.4b: Table -> Select -Last -Skip -> Action" {
        Get-Probe -Count 10 | Select -Last 2 -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.17' (8/10)" 80)

            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.16' forever (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.17' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.17' forever (2/4)" 50)
        ))
    }

        #endregion
        #region 13.5: Table -> Select -Last -> Select -Something -> Action

    It "13.5a: Table -> Select -Last -> Select -First -> Action" {
        Get-Probe -Count 10 | Select -Last 4 | Select -First 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.5b: Table -> Select -Last -> Select -Skip -> Action" {
        Get-Probe -Count 10 | Select -Last 4 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.5c: Table -> Select -Last -> Select -SkipLast -> Action" {
        Get-Probe -Count 10 | Select -Last 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.5d: Table -> Select -Last -> Select -Index -> Action" {
        Get-Probe -Count 10 | Select -Last 4 | Select -Index 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 13.6: Variable -> Select -Last -Something -> Table

    It "13.6a: Variable -> Select -Last -First -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 2 -First 4 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/10)" 10 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/10)" 20 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.18' (9/10)" 90 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.19' (10/10)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.19' (10/10)" 100 "Retrieving all devices")
        ))
    }

    It "13.6b: Variable -> Select -Last -Skip -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 2 -Skip 2 | Get-Device
        
        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.16' (1/4)" 25 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
        ))
    }

        #endregion
        #region 13.7: Variable -> Select -Last -> Select -Something -> Table

    It "13.7a: Variable -> Select -Last -> Select -First -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -First 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.7b: Variable -> Select -Last -> Select -Skip -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -Skip 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.7c: Variable -> Select -Last -> Select -SkipLast -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -SkipLast 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.7d: Variable -> Select -Last -> Select -Index -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -Index 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 13.8: Variable -> Select -Last -Something -> Action

    It "13.8a: Variable -> Select -Last -First -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 2 -First 4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/10)" 10)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/10)" 20)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.18' forever (9/10)" 90)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.19' forever (10/10)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.19' forever (10/10)" 100)
        ))
    }

    It "13.8b: Variable -> Select -Last -Skip -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 2 -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.16' forever (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.17' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.17' forever (2/4)" 50)
        ))
    }

        #endregion
        #region 13.9: Variable -> Select -Last -> Select -Something -> Action

    It "13.9a: Variable -> Select -Last -> Select -First -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -First 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.9b: Variable -> Select -Last -> Select -Skip -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.9c: Variable -> Select -Last -> Select -SkipLast -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13.9d: Variable -> Select -Last -> Select -Index -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Last 4 | Select -Index 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 13.10: Something -> Select -Last -> Table -> Something

    It "13.10a: Table -> Select -Last -> Table -> Table" {
        Get-Probe -Count 3 | Select -Last 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)"             33)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.10' (1/3)" 33)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"       50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"      50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"      50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"      50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"       100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"      100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"      100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"      100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (2/2)" 100)
        ))
    }

    It "13.10b: Table -> Select -Last -> Table -> Action" {
        Get-Probe -Count 3 | Select -Last 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)"             33)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.10' (1/3)" 33)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"       50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"      50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"      50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)"      50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"       100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"      100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"      100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)"      100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (2/2)"       100)
        ))
    }

    It "13.10c: Variable -> Select -Last -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -Last 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (2/2)" 100)
        ))
    }

    It "13.10d: Variable -> Select -Last -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -Last 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (2/2)" 100)
        ))
    }

        #endregion
        #region 13.11: Something -> Select -Last -Something -> Table -> Something

    It "13.11a<i>: Table -> Select -Last -<name> -> Table -> Table" -TestCases $selectLastParams {
        param($name)

        TestCmdletChainWithSingle $name "Last" "Get-Sensor"
    }

    It "13.11b<i>: Table -> Select -Last -<name> -> Table -> Action" -TestCases $selectLastParams {
        param($name)

        TestCmdletChainWithSingle $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

    It "13.11c<i>: Variable -> Select -Last -<name> -> Table -> Table" -TestCases $selectLastParams {
        param($name)

        TestVariableChainWithSingle $name "Last" "Get-Sensor"
    }

    It "13.11d<i>: Variable -> Select -Last -<name> -> Table -> Action" -TestCases $selectLastParams {
        param($name)

        TestVariableChainWithSingle $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 13.12: Something -> Select -Last -> Select -Something -> Table -> Something

    It "13.12a<i>: Table -> Select -Last -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Last" "Get-Sensor"
    }

    It "13.12b<i>: Table -> Select -Last -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

    It "13.12c<i>: Variable -> Select -Last -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Last" "Get-Sensor"
    }

    It "13.12d<i>: Variable -> Select -Last -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Last" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
    #endregion
    #region 14: Something -> Select -Skip -> Something
        #region 14.1: Something -> Select -Skip -> Something

    It "14.1a: Table -> Select -Skip -> Table" {
        Get-Probe -Count 4 | Select -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (1/4)" 25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (2/4)" 50)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (3/4)" 75)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1b: Table -> Select -Skip -> Action" {
        Get-Probe -Count 4 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/4)" 25)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/4)" 100)
        ))
    }

    It "14.1c: Variable -> Select -Skip -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1d: Variable -> Select -Skip -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/4)" 100)
        ))
    }

    It "14.1e: Table -> Select -Skip -Wait -> Table" {
        Get-Probe -Count 4 | Select -Skip 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (1/4)" 25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (2/4)" 50)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (3/4)" 75)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.12' (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1f: Table -> Select -Skip -Wait -> Action" {
        Get-Probe -Count 4 | Select -Skip 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/4)" 25)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/4)" 100)
        ))
    }

    It "14.1h: Variable -> Select -Skip -Wait -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (4/4)" 100 "Retrieving all devices")
        ))
    }

    It "14.1g: Variable -> Select -Skip -Wait -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Skip 2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.13' forever (4/4)" 100)
        ))
    }

        #endregion
        #region 14.2: Table -> Select -Skip -Something -> Table

    It "14.2a: Table -> Select -Skip -First -> Table" {
        Get-Probe -Count 10 | Select -Skip 2 -First 4 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

    It "14.2b: Table -> Select -Skip -Last -> Table" {
        Get-Probe -Count 10 | Select -Skip 2 -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.13' (4/10)" 40)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.14' (5/10)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.15' (6/10)" 60)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.16' (7/10)" 70)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.17' (8/10)" 80)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.17' (8/10)" 80)

            (Gen "PRTG Device Search" "Processing probe '127.0.0.16' (1/4)" 25 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
        ))
    }

        #endregion
        #region 14.3: Table -> Select -Skip -> Select -Something -> Table

    It "14.3a: Table -> Select -Skip -> Select -First -> Table" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -First 4 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)"       "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

    It "14.3b: Table -> Select -Skip -> Select -Last -> Table" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (2/10)" 20)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (3/10)" 30)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.13' (4/10)" 40)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.14' (5/10)" 50)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.15' (6/10)" 60)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.16' (7/10)" 70)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.17' (8/10)" 80)
            (Gen "PRTG Probe Search (Completed)"       "Processing probe '127.0.0.17' (8/10)" 80)

            (Gen "PRTG Device Search"                  "Processing probe '127.0.0.18' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"                  "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)"      "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "14.3c: Table -> Select -Skip -> Select -SkipLast -> Table" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -SkipLast 3 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.12' (1/5)" 20 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.13' (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.14' (3/5)" 60 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.15' (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") + 
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.16' (5/5)" 100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") + 
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.16' (5/5)" 100 "Retrieving all devices")
        ))
    }

    It "14.3d: Table -> Select -Skip -> Select -Index -> Table" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -Index 1,3 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.13' (4/10)" 40)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)"       "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

        #endregion
        #region 14.4: Table -> Select -Skip -Something -> Action

    It "14.4a: Table -> Select -Skip -First -> Action" {
        Get-Probe -Count 10 | Select -Skip 2 -First 4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/10)"                     10)

            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

    It "14.4b: Table -> Select -Skip -Last -> Action" {
        Get-Probe -Count 10 | Select -Skip 2 -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.17' (8/10)" 80)

            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.16' forever (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.17' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.17' forever (2/4)" 50)
        ))
    }

        #endregion
        #region 14.5: Table -> Select -Skip -> Select -Something -> Action

    It "14.5a: Table -> Select -Skip -> Select -First -> Action" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -First 4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)"    "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

    It "14.5b: Table -> Select -Skip -> Select -Last -> Action" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/10)" 10)
            (Gen "PRTG Probe Search (Completed)"       "Processing probe '127.0.0.17' (8/10)" 80)

            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.18' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.19' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)"    "Pausing probe '127.0.0.19' forever (2/2)" 100)
        ))
    }

    It "14.5c: Table -> Select -Skip -> Select -SkipLast -> Action" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -SkipLast 3 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/5)" 20)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/5)" 40)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (3/5)" 60)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (4/5)" 80)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") + 
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.16' forever (5/5)" 100)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") + 
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.16' forever (5/5)" 100)
        ))
    }

    It "14.5d: Table -> Select -Skip -> Select -Index -> Action" {
        Get-Probe -Count 10 | Select -Skip 2 | Select -Index 1,3 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"                "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)"    "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

        #endregion
        #region 14.6: Variable -> Select -Skip -Something -> Table

    It "14.6a: Variable -> Select -Skip -First -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 -First 4 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

    It "14.6b: Variable -> Select -Skip -Last -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 -Last 2 | Get-Device
        
        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.16' (1/4)" 25 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.17' (2/4)" 50 "Retrieving all devices")
        ))
    }

        #endregion
        #region 14.7: Variable -> Select -Skip -> Select -Something -> Table

    It "14.7a: Variable -> Select -Skip -> Select -First -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -First 4 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (3/10)" 30 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.13' (4/10)" 40 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.14' (5/10)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.15' (6/10)" 60 "Retrieving all devices")
        ))
    }

    It "14.7b: Variable -> Select -Skip -> Select -Last -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.18' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.19' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "14.7c: Variable -> Select -Skip -> Select -SkipLast -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -SkipLast 3 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/5)" 20  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/5)" 40  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.14' (3/5)" 60  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.15' (4/5)" 80  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.16' (5/5)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.16' (5/5)" 100 "Retrieving all devices")
        ))
    }

    It "14.7d: Variable -> Select -Skip -> Select -Index -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -Index 1,3 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (4/10)" 40  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.15' (6/10)" 60  "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.15' (6/10)" 60  "Retrieving all devices")
        ))
    }

        #endregion
        #region 14.8: Variable -> Select -Skip -Something -> Action

    It "14.8a: Variable -> Select -Skip -First -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 -First 4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

    It "14.8b: Variable -> Select -Skip -Last -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.16' forever (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.17' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.17' forever (2/4)" 50)
        ))
    }

        #endregion
        #region 14.9: Variable -> Select -Skip -> Select -Something -> Action

    It "14.9a: Variable -> Select -Skip -> Select -First -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -First 4 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/10)" 30)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (5/10)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

    It "14.9b: Variable -> Select -Skip -> Select -Last -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.18' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.19' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.19' forever (2/2)" 100)
        ))
    }

    It "14.9c: Variable -> Select -Skip -> Select -SkipLast -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -SkipLast 3 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (1/5)" 20)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (2/5)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.14' forever (3/5)" 60)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (4/5)" 80)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.16' forever (5/5)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.16' forever (5/5)" 100)
        ))
    }

    It "14.9d: Variable -> Select -Skip -> Select -Index -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Skip 2 | Select -Index 1,3 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.13' forever (4/10)" 40)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.15' forever (6/10)" 60)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.15' forever (6/10)" 60)
        ))
    }

        #endregion
        #region 14.10: Something -> Select -Skip -> Table -> Something
   
    It "14.10a: Table -> Select -Skip -> Table -> Table" {
        Get-Probe -Count 3 | Select -Skip 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/3)"             33)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (2/3)"             66)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.11' (2/3)"             66 "Retrieving all devices")
            
            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (1/2)"            50)

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (1/2)"            50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device1' (2/2)"            100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "PRTG Device Search (Completed)"             "Processing device 'Probe Device1' (2/2)"            100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (3/3)"             100)
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.12' (3/3)"             100 "Retrieving all devices")

            ###################################################################
            
            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (1/2)"            50)

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device0' (1/2)"            50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "PRTG Device Search"             "Processing device 'Probe Device1' (2/2)"            100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                  "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)"            100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)"       "Processing probe '127.0.0.12' (3/3)"             100)
        ))
    }

    It "14.10b: Table -> Select -Skip -> Table -> Action" {
        Get-Probe -Count 3 | Select -Skip 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (1/3)"             33)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (2/3)"             66)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.11' (2/3)"             66 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)"            100)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.11' (2/3)"             66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)"            100)

            ###################################################################

            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.12' (3/3)"             100)
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.12' (3/3)"             100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "PRTG Device Search"               "Processing device 'Probe Device0' (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)"            50)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)"            100)

            (Gen1 "PRTG Probe Search"                    "Processing probe '127.0.0.12' (3/3)"             100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)"            100)

            (Gen "PRTG Probe Search (Completed)"         "Processing probe '127.0.0.12' (3/3)"             100)
        ))
    }

    It "14.10c: Variable -> Select -Skip -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -Skip 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"   66 "Retrieving all devices")
            
            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"  66) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"  66) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"  66) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"                    "Processing probe '127.0.0.12' (3/3)"  100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"  100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"  100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"  100) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.12' (3/3)"  100)
        ))
    }

    It "14.10d: Variable -> Select -Skip -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -Skip 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"   66 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"  66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"  66) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/3)"  66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"   100 "Retrieving all devices")

            ###################################################################

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"  100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"  100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.12' (3/3)"  100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)"         "Processing probe '127.0.0.12' (3/3)"   100)
        ))
    }

        #endregion
        #region 14.11: Something -> Select -Last -Something -> Table -> Something

    It "14.11a<i>: Table -> Select -Skip -<name> -> Table -> Table" -TestCases $selectSkipParams {
        param($name)

        TestCmdletChainWithSingle $name "Skip" "Get-Sensor"
    }

    It "14.11b<i>: Table -> Select -Skip -<name> -> Table -> Action" -TestCases $selectSkipParams {
        param($name)

        TestCmdletChainWithSingle $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

    It "14.11c<i>: Variable -> Select -Skip -<name> -> Table -> Table" -TestCases $selectSkipParams {
        param($name)

        TestVariableChainWithSingle $name "Skip" "Get-Sensor"
    }

    It "14.11d<i>: Variable -> Select -Skip -<name> -> Table -> Action" -TestCases $selectSkipParams {
        param($name)

        TestVariableChainWithSingle $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
        #region 14.12: Something -> Select -Last -> Select -Something -> Table -> Something

    It "14.12a<i>: Table -> Select -Skip -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Skip" "Get-Sensor"
    }

    It "14.12b<i>: Table -> Select -Skip -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

    It "14.12c<i>: Variable -> Select -Skip -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Skip" "Get-Sensor"
    }

    It "14.12d<i>: Variable -> Select -Skip -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Skip" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
    #endregion
    #region 15: Something -> Select -SkipLast -> Something
        #region 15.1: Something -> Select -SkipLast -> Something

    It "15.1a: Table -> Select -SkipLast -> Table" {
        Get-Probe -Count 4 | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)" 50  "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")
        ))
    }

    It "15.1b: Table -> Select -SkipLast -> Action" {
        Get-Probe -Count 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") +
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") +
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"        "Retrieving all probes") +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/2)" 100)
        ))
    }

    It "15.1c: Variable -> Select -SkipLast -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "15.1d: Variable -> Select -SkipLast -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/2)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/2)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.11' forever (2/2)" 100)
        ))
    }

        #endregion
        #region 15.2: Table -> Select -SkipLast -> Select -Something -> Table

    It "15.2a: Table -> Select -SkipLast -> Select -First -> Table" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -First 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.2b: Table -> Select -SkipLast -> Select -Last -> Table" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -Last 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.2c: Table -> Select -SkipLast -> Select -Skip -> Table" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -Skip 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.2d: Table -> Select -SkipLast -> Select -Index -> Table" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -Index 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 15.3: Table -> Select -SkipLast -> Select -Something -> Action

    It "15.3a: Table -> Select -SkipLast -> Select -First -> Action" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -First 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.3b: Table -> Select -SkipLast -> Select -Last -> Action" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -Last 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.3c: Table -> Select -SkipLast -> Select -Skip -> Action" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.3d: Table -> Select -SkipLast -> Select -Index -> Action" {
        Get-Probe -Count 10 | Select -SkipLast 2 | Select -Index 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 15.4: Variable -> Select -SkipLast -> Select -Something -> Table

    It "15.4a: Variable -> Select -SkipLast -> Select -First -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -First 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.4b: Variable -> Select -SkipLast -> Select -Last -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -Last 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.4c: Variable -> Select -SkipLast -> Select -Skip -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -Skip 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.4d: Variable -> Select -SkipLast -> Select -Index -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -Index 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 15.5: Variable -> Select -SkipLast -> Select -Something -> Action

    It "15.5a: Variable -> Select -SkipLast -> Select -First -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -First 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.5b: Variable -> Select -SkipLast -> Select -Last -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -Last 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.5c: Variable -> Select -SkipLast -> Select -Skip -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15.5d: Variable -> Select -SkipLast -> Select -Index -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -SkipLast 2 | Select -Index 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 15.6: Something -> Select -SkipLast -> Table -> Something

    It "15.6a: Table -> Select -SkipLast -> Table -> Table" {
        Get-Probe -Count 3 | Select -SkipLast 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)"  100)
        ))
    }

    It "15.6b: Table -> Select -SkipLast -> Table -> Action" {
        Get-Probe -Count 3 | Select -SkipLast 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)
            
            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Retrieving all probes") +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)"  100)
        ))
    }

    It "15.6c: Variable -> Select -SkipLast -> Table -> Table" {
        $probes = Get-Probe -Count 3

        $probes | Select -SkipLast 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search"               "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)"   "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

    It "15.6d: Variable -> Select -SkipLast -> Table -> Action" {
        $probes = Get-Probe -Count 3

        $probes | Select -SkipLast 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

        #endregion
        #region 15.7: Something -> Select -SkipLast -> Select -Something -> Table -> Something

    It "15.7a<i>: Table -> Select -SkipLast -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "SkipLast" "Get-Sensor"
    }

    It "15.7b<i>: Table -> Select -SkipLast -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "SkipLast" "Pause-Object -Forever -Batch:`$false"
    }

    It "15.7c<i>: Variable -> Select -SkipLast -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "SkipLast" "Get-Sensor"
    }

    It "15.7d<i>: Variable -> Select -SkipLast -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "SkipLast" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
    #endregion
    #region 16: Something -> Select -Index -> Something
        #region 16.1: Something -> Select -Index -> Something

    It "16.1a: Table -> Select -Index -> Table" {
        Get-Probe -Count 4 | Select -Index 1,2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/4)" 50)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.12' (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (3/4)" 75 "Retrieving all devices")
        ))
    }

    It "16.1b: Table -> Select -Index -> Action" {
        Get-Probe -Count 4 | Select -Index 1,2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' forever (3/4)" 75)
        ))
    }

    It "16.1c: Variable -> Select -Index -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (2/4)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (3/4)" 75  "Retrieving all devices")
        ))
    }

    It "16.1d: Variable -> Select -Index -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' forever (3/4)" 75)
        ))
    }

    It "16.1e: Table -> Select -Index -Wait -> Table" {
        Get-Probe -Count 4 | Select -Index 1,2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"             "Retrieving all probes")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/4)" 50)
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.11' (2/4)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search"             "Processing probe '127.0.0.12' (3/4)" 75 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (3/4)" 75 "Retrieving all devices")
        ))
    }

    It "16.1f: Table -> Select -Index -Wait -> Action" {
        Get-Probe -Count 4 | Select -Index 1,2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' forever (3/4)" 75)
        ))
    }

    It "16.1g: Variable -> Select -Index -Wait -> Table" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 -Wait | Get-Device

        Validate(@(
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.11' (2/4)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (3/4)" 75  "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (3/4)" 75  "Retrieving all devices")
        ))
    }

    It "16.1h: Variable -> Select -Index -Wait -> Action" {
        $probes = Get-Probe -Count 4

        $probes | Select -Index 1,2 -Wait | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/4)" 50)
            (Gen "Pausing PRTG Objects"             "Pausing probe '127.0.0.12' forever (3/4)" 75)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' forever (3/4)" 75)
        ))
    }

        #endregion
        #region 16.2: Table -> Select -Index -> Select -Something -> Table

    It "16.2a: Table -> Select -Index -> Select -First -> Table" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -First 3 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.2b: Table -> Select -Index -> Select -Last -> Table" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Last 3 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.2c: Table -> Select -Index -> Select -Skip -> Table" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Skip 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.2d: Table -> Select -Index -> Select -SkipLast -> Table" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 16.3: Table -> Select -Index -> Select -Something -> Action

    It "16.3a: Table -> Select -Index -> Select -First -> Action" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -First 3 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.3b: Table -> Select -Index -> Select -Last -> Action" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Last 3 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.3c: Table -> Select -Index -> Select -Skip -> Action" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -Skip 2 | Pause-Object -Forever -Batch:$false
    }

    It "16.3d: Table -> Select -Index -> Select -SkipLast -> Action" {
        Get-Probe -Count 10 | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 16.4: Variable -> Select -Index -> Select -Something -> Table

    It "16.4a: Variable -> Select -Index -> Select -First -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -First 3 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.4b: Variable -> Select -Index -> Select -Last -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -Last 3 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.4c: Variable -> Select -Index -> Select -Skip -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -Skip 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.4d: Variable -> Select -Index -> Select -SkipLast -> Table" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 16.5: Variable -> Select -Index -> Select -Something -> Action

    It "16.5a: Variable -> Select -Index -> Select -First -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -First 3 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.5b: Variable -> Select -Index -> Select -Last -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -Last 3 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.5c: Variable -> Select -Index -> Select -Skip -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -Skip 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "16.5d: Variable -> Select -Index -> Select -SkipLast -> Action" {
        $probes = Get-Probe -Count 10

        $probes | Select -Index 1,2,5,7,9 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 16.6: Something -> Select -Index -> Table -> Something

    It "16.6a: Table -> Select -Index -> Table -> Table" {
        Get-Probe -Count 5 | Select -Index 1,3 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40)
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80)
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            
            ###################################################################

            (Gen "PRTG Probe Search (Completed)"     "Processing probe '127.0.0.13' (4/5)" 80)
        ))
    }

    It "16.6b: Table -> Select -Index -> Table -> Action" {
        Get-Probe -Count 5 | Select -Index 1,3 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40)
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Device Search"                "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search"                 "Processing probe '127.0.0.13' (4/5)" 80)
            (Gen "PRTG Probe Search"                 "Processing probe '127.0.0.13' (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Device Search"           "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"         "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search"                "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Probe Search (Completed)"     "Processing probe '127.0.0.13' (4/5)" 80)
        ))
    }

    It "16.6c: Variable -> Select -Index -> Table -> Table" {
        $probes = Get-Probe -Count 5

        $probes | Select -Index 1,3 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search"                  "Processing probe '127.0.0.11' (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search"                  "Processing probe '127.0.0.13' (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Sensor Search"             "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"                 "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen "PRTG Device Search (Completed)"      "Processing probe '127.0.0.13' (4/5)" 80)
        ))
    }

    It "16.6d: Variable -> Select -Index -> Table -> Action" {
        $probes = Get-Probe -Count 5

        $probes | Select -Index 1,3 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"                    "Processing probe '127.0.0.11' (2/5)" 40 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.11' (2/5)" 40) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"                    "Processing probe '127.0.0.13' (4/5)" 80 "Retrieving all devices")

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects"             "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Device Search"                   "Processing probe '127.0.0.13' (4/5)" 80) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)"        "Processing probe '127.0.0.13' (4/5)" 80)
        ))
    }

        #endregion
        #region 16.7: Something -> Select -Index -> Select -Something -> Table -> Something

    It "16.7a<i>: Table -> Select -Index -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Index" "Get-Sensor"
    }

    It "16.7b<i>: Table -> Select -Index -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestCmdletChainWithDouble $name "Index" "Pause-Object -Forever -Batch:`$false"
    }

    It "16.7c<i>: Variable -> Select -Index -> Select -<name> -> Table -> Table" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Index" "Get-Sensor"
    }

    It "16.7d<i>: Variable -> Select -Index -> Select -<name> -> Table -> Action" -TestCases $allSelectParams {
        param($name)

        TestVariableChainWithDouble $name "Index" "Pause-Object -Forever -Batch:`$false"
    }

        #endregion
    #endregion
    #region 17: Something -> Where -> Something
    
    It "17a: Table -> Where -> Table" {

        $counts = @{
            ProbeNode = 3
        }

        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/3)" 66 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (3/3)" 100 "Retrieving all devices")
        ))
    }
    
    It "17b: Variable -> Where -> Table" {
        $counts = @{
            ProbeNode = 3
        }
        
        $probes = RunCustomCount $counts {
            Get-Probe
        }

        $probes.Count | Should Be 3

        $probes | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (3/3)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (3/3)" 100 "Retrieving all devices")
        ))
    }

    It "17c: Table -> Where -> Action" {
        
        $counts = @{
            ProbeNode = 3
        }

        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Pause-Object -Forever -Batch:$false
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.10' forever (1/3)" 33)
            (Gen "Pausing PRTG Objects" "Pausing probe '127.0.0.12' forever (3/3)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing probe '127.0.0.12' forever (3/3)" 100)
        ))
    }

    #endregion
    #region 18: Something -> Table -> Where -> Table
    
    It "18a: Table -> Table -> Where -> Table" {

        Get-Probe | Get-Group | where name -EQ "Windows Infrastructure0" | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }

    It "18b: Variable -> Table -> Where -> Table" {
        $probes = Get-Probe

        $probes | Get-Group | where name -like * | Get-Sensor

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }
    
    #endregion
    #region 19: Something -> Where -> Something -> Something

    It "19a: Table -> Where -> Table -> Table" {
        $counts = @{
            ProbeNode = 3
        }
        
        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device | Get-Sensor
        }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/3)" 66)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (3/3)" 100)
        ))
    }

    It "19b: Variable -> Where -> Table -> Table" {
        $counts = @{
            ProbeNode = 3
        }
        
        $probes = RunCustomCount $counts {
            Get-Probe
        }

        $probes.Count | Should Be 3

        $probes | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/3)" 33 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.12' (3/3)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.12' (3/3)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.12' (3/3)" 100)
        ))
    }
    
    #endregion
    #region 20: Variable(1) -> Table -> Table

    It "20: Variable(1) -> Table -> Table" {

        $probe = Get-Probe -Count 1

        $probe.Count | Should Be 1

        $probe | Get-Group | Get-Device

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (1/1)" 100 "Retrieving all groups")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/1)" 100) +
                (Gen2 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing probe '127.0.0.10' (1/1)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.10' (1/1)" 100)
        ))
    }

    #endregion
    #region 21: Something -> PSObject
    
    It "21a: Table -> PSObject" {
        Get-Device | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all notification trigger types")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")
        ))
    }

    It "21b: Variable -> PSObject" {
        $devices = Get-Device

        $devices | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Notification Trigger Type Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all notification trigger types")
            (Gen "PRTG Notification Trigger Type Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")
            (Gen "PRTG Notification Trigger Type Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")
        ))
    }

    #endregion
    #region 22: Something -> Table -> PSObject

    It "22a: Table -> Table -> PSObject" {
        Get-Group | Get-Device | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "22b: Variable -> Table -> PSObject" {
        $groups = Get-Group

        $groups | Get-Device | Get-Trigger -Types

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Notification Trigger Type Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Notification Trigger Type Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Notification Trigger Type Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all notification trigger types")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }
    
    #endregion
    #region 23: Something -> Where { Variable(1) -> Table }

    It "23a: Table -> Where { Variable(1) -> Table }" {
        Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "23b: Variable -> Where { Variable(1) -> Table }" {
        $probes = Get-Probe

        $probes | where { $_ | Get-Sensor }

        { Get-Progress } | Should Throw "Queue empty"
    }
    
    #endregion
    #region 24: Something -> Table -> Where { Variable(1) -> Table }

    It "24a: Table -> Table -> Where { Variable(1) -> Table }" {
        Get-Probe | Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "24b: Variable -> Table -> Where { Variable(1) -> Table }" {
        $probes = Get-Probe

        $probes | Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    #endregion
    #region 25: Something -> Where { Table -> Table }

    It "25a: Table -> Where { Table -> Table }" {
        Get-Probe | where { Get-Device | Get-Sensor }

        Validate (@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
        ))
    }

    It "25b: Variable -> Where { Table -> Table }" {
        $probes = Get-Probe

        $probes | where { Get-Device | Get-Sensor }

        Validate (@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")
        ))
    }
    
    #endregion
    #region 26: Something -> Where { Variable -> Where { Variable(1) -> Table } }

    It "26a: Table -> Where { Variable -> Where { Variable(1) -> Table } }" {
        Get-Probe | where {
            ($_ | Get-Device | where {
                ($_|Get-Sensor).Name -eq "Volume IO _Total0"
            }).Name -eq "Probe Device0"
        }

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "26b: Variable -> Where { Variable -> Where { Variable -> Table } }" {
        $probes = Get-Probe

        $probes | where {
            ($_ | Get-Device | where {
                ($_|Get-Sensor).Name -eq "Volume IO _Total0"
            }).Name -eq "Probe Device0"
        }

        { Get-Progress } | Should Throw "Queue empty"
    }

    #endregion
    #region 27: Something -> Where { Variable(1) -> Table } -> Table

    It "27a: Table -> Where { Variable(1) -> Table } -> Table" {
        Get-Probe | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" } | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "27b: Variable -> Where { Variable(1) -> Table } -> Table" {
        $probes = Get-Probe

        $probes | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" } | Get-Device

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
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
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
        ))
    }
    
    It "28b: Variable -> Table -> Where { Variable(1) -> Table -> Table }" {
        $probes = Get-Probe
        
        $probes | Get-Group | where {
            ($_ | Get-Device | Get-Sensor).Name -eq "Volume IO _Total0"
        }

        Validate(@(
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)
            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
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
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")

            #region Probe 1, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            #endregion Probe 1, Group 1

            ##########################################################################################

            #region Probe 1, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)

            #endregion Probe 1, Group 2

            ##########################################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")

            #region Probe 2, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
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
            (Gen "PRTG Group Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all groups")
            #region Probe 2, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen "PRTG Group Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
            #region Probe 2, Group 1

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure0' (1/1)" 100)

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")
            (Gen "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")
            (Gen "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (1/1)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen "PRTG Group Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all groups")
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
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            #region Probe 1, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            #endregion Probe 1, Device 2

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            #region Probe 1, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)

            #endregion Probe 1, Device 2

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            #region Probe 2, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            #endregion Probe 2, Device 1

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            #region Probe 2, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)

            #endregion Probe 2, Device 2

            ##########################################################################################

            (Gen1 "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
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
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            #region Probe 1, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            #endregion Probe 1, Device 1

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            #region Probe 1, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)

            #endregion Probe 1, Device 2

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            #region Probe 2, Device 1

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            #endregion Probe 2, Device 1

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            #region Probe 2, Device 2

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)

            #endregion Probe 2, Group 2

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100)
        ))
    }
    
    #endregion
    #region 31: Something -> Table -> Where { Variable(1) -> Action }

    It "31a: Table -> Table -> Where { Variable(1) -> Action }" {
        Get-Probe | Get-Device | Where { $_ | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    It "31b: Variable -> Table -> Where { Variable(1) -> Action }" {
        $probes = Get-Probe

        $probes | Get-Device | Where { $_ | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))

    }

    #endregion
    #region 32: Something -> Table -> Where { Variable(1) -> Table -> Action }
    
    It "32a: Table -> Table -> Where { Variable(1) -> Table -> Action }" {
        Get-Probe | Get-Device | Where { $_ | Get-Sensor | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "32b: Variable -> Table -> Where { Variable(1) -> Table -> Action }" {
        $probes = Get-Probe
        
        $probes | Get-Device | Where { $_ | Get-Sensor | Pause-Object -Forever -Batch:$false }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device0' (1/1)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (1/1)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (1/1)" 100)
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    #endregion
    #region 33: Something -> Table -> Where { Variable(1) -> Action -> Table }

    It "33a: Table -> Table -> Where { Variable(1) -> Action -> Table }" {
        Get-Probe | Get-Device | Where { $_ | Clone-Object 5678 | Get-Sensor }

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50)
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "33b: Variable -> Table -> Where { Variable(1) -> Action -> Table }" {
        $probes = Get-Probe
        
        $probes | Get-Device | Where { $_ | Clone-Object 5678 | Get-Sensor }

        Validate(@(
            (Gen "PRTG Device Search" "Processing probe '127.0.0.10' (1/2)" 50 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device0' (ID: 3000) (1/1)" 100 "Retrieving all sensors")

            ##########################################################################################

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (1/1)" 100 "Retrieving all sensors")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }
    
    #endregion
    #region 34: Something -> Table -> Action -> Table -> Action

    It "34a: Table -> Table -> Action -> Table -> Action" {
        Get-Group | Get-Device | Clone-Object 5678 | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "34b: Variable -> Table -> Action -> Table -> Action" {
        $groups = Get-Group

        $groups | Get-Device | Clone-Object 5678 | Get-Sensor | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/2)" 50)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    #endregion
    #region 35: Something -> Table -> Action -> Action

    It "35a: Table -> Table -> Action -> Action" {
        Get-Group | Get-Device | Clone-Object 5678 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (2/2)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "35b: Variable -> Table -> Action -> Action" {
        $groups = Get-Group

        $groups | Get-Device | Clone-Object 5678 | Resume-Object -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Processing device 'Probe Device0' (2/2)" 100)

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects" "Processing device 'Probe Device0' (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Processing device 'Probe Device0' (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    #endregion
    #region 100: Get-SensorFactorySource
        #region 100a: Something -> Get-SensorFactorySource

    It "100a1: Sensor -> Get-SensorFactorySource" {

        Get-Sensor -Count 2 | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")
        ))
    }

    It "100a2: Variable -> Get-SensorFactorySource" {

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2

        $sensors | Get-SensorFactorySource

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")
            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")
        ))
    }

        #endregion
        #region 100b: Something -> Get-SensorFactorySource -> Channel

    It "100b1: Sensor -> Get-SensorFactorySource -> Channel" {

        Get-Sensor -Count 2 | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

    It "100b2: Variable -> Get-SensorFactorySource -> Channel" {

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2

        $sensors | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

        #endregion
        #region 100c: Something -> Sensor -> Get-SensorFactorySource -> Channel

    It "100c1: Device -> Sensor -> Get-SensorFactorySource -> Channel" {

        Get-Device | Get-Sensor | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ##########################################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)
        ))
    }

    It "100c2: Variable -> Sensor -> Get-SensorFactorySource -> Channel" {
        $devices = Get-Device

        $devices.Count | Should Be 2

        $devices | Get-Sensor | Get-SensorFactorySource | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                    (Gen3 "PRTG Channel Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)
        ))
    }

        #endregion
        #region 100d: Something -> Get-SensorFactorySource -> Action

    It "100d1: Sensor -> Get-SensorFactorySource -> Action" {
        Get-Sensor -Count 2 | Get-SensorFactorySource | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

    It "100d2: Variable -> Get-SensorFactorySource -> Action" {

        $sensors = Get-Sensor -Count 2

        $sensors.Count | Should Be 2

        $sensors | Get-SensorFactorySource | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing sensor 'Volume IO _Total0' forever (1/1)" 100)

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

        #endregion
        #region 100e1: Something -> Get-SensorFactorySource -> Action -> Object

    It "100e1: Sensor -> Get-SensorFactorySource -> Action -> Object" {

        Get-Sensor -Count 2 | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

    It "100e2: Variable -> Get-SensorFactorySource -> Action -> Object" {

        $sensors = Get-Sensor -Count 2
        
        $sensors | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

        #endregion
        #region 100f1: Something -> Get-SensorFactorySource -> Action -> Object -> Anything

    It "100f1: Something -> Get-SensorFactorySource -> Action -> Object -> Anything" {
        
        Get-Sensor -Count 2 | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel | Set-ChannelProperty UpperErrorLimit 100 -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Search"             "Retrieving all sensors")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "PRTG Sensor Factory Sensor Search" "Processing sensor factory sensor 'Volume IO _Total0' (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

    It "100f2: Variable -> Get-SensorFactorySource -> Action -> Object -> Anything" {
        
        $sensors = Get-Sensor -Count 2
        
        $sensors | Get-SensorFactorySource | Clone-Object 5678 | Get-Channel | Set-ChannelProperty UpperErrorLimit 100 -Batch:$false

        Validate(@(
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory properties")
            (Gen "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100 "Retrieving all sensor factory sensors")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "PRTG Channel Search" "Processing channel 'Percent Available Memory' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100) +
                    (Gen3 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'UpperErrorLimit' to '100' (1/1)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Factory Sensor Search" "Processing sensor 'Volume IO _Total1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Sensors (Completed)" "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/1)" 100)

            (Gen "PRTG Sensor Factory Sensor Search (Completed)" "Processing sensor 'Volume IO _Total1' (2/2)" 100)
        ))
    }

        #endregion
    #endregion
    #region 101: Multi Operation Cmdlets
        #region 101a: Pipeline Combinations

    It "101a1: Table -> Action -Batch:`$true" {
        Get-Sensor -Count 3 | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1' and 'Volume IO _Total2' forever (3/3)"

        Validate(@(
            (Gen "PRTG Sensor Search"         "Retrieving all sensors")
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (1/3)" 33)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total0' (1/3)" 33)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total1' (2/3)" 66)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total2' (3/3)" 100)
            (Gen "Pausing PRTG Objects"       $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101a2: Table -> Action -> Action -Batch:`$true" {
        Get-Sensor -Count 3 | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0' and 'Volume IO _Total0' (3/3)"

        Validate(@(
            (Gen "PRTG Sensor Search"         "Retrieving all sensors")
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (1/3)" 33)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (1/3)" 33)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/3)" 66)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (2/3)" 66)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total2' (ID: 4002) (3/3)" 100)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (3/3)" 100)
            (Gen "Resuming PRTG Objects"      $final 100)
            (Gen "Resuming PRTG Objects (Completed)"      $final 100)
        ))
    }

    ###################################################################

    It "101a3: Variable -> Action -Batch:`$true" {
        $sensors = Get-Sensor -Count 3

        $sensors | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1' and 'Volume IO _Total2' forever (3/3)"

        Validate(@(
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total0' (1/3)" 33)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total1' (2/3)" 66)
            (Gen "Pausing PRTG Objects"       "Queuing sensor 'Volume IO _Total2' (3/3)" 100)
            (Gen "Pausing PRTG Objects"       $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101a4: Variable -> Action -> Action -Batch:`$true" {
        $sensors = Get-Sensor -Count 3

        $sensors | Clone-Object 1234 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0' and 'Volume IO _Total0' (3/3)"

        Validate(@(
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/3)" 33)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (1/3)" 33)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/3)" 66)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (2/3)" 66)
            (Gen "Cloning PRTG Sensors"       "Cloning sensor 'Volume IO _Total2' (ID: 4002) (3/3)" 100)
            (Gen "Resuming PRTG Objects"      "Queuing sensor 'Volume IO _Total0' (3/3)" 100)
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
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search"    "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"  "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen1 "PRTG Device Search"        "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   $final 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)"   $final 100)
        ))
    }

    It "101a6: Table -> Table -> Action -> Action -Batch:`$true" {
        Get-Device -Count 3 | Get-Sensor | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0'," + 
                  " 'Volume IO _Total0' and 'Volume IO _Total0' (6/6)"

        Validate(@(
            (Gen "PRTG Device Search"         "Retrieving all devices")
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100)
            (Gen "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "PRTG Sensor Search"     "Processing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Device Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  $final 100)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
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
            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device0' (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Pausing PRTG Objects (Completed)"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device1' (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Pausing PRTG Objects (Completed)"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device2' (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   "Queuing sensor 'Volume IO _Total1' (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects"   $final 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101a8: Variable -> Table -> Action -> Action -Batch:`$true" {
        $device = Get-Device -Count 3

        $device | Get-Sensor | Clone-Object 5678 | Resume-Object

        $final = "Resuming sensors 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0', 'Volume IO _Total0'," + 
        " 'Volume IO _Total0' and 'Volume IO _Total0' (6/6)"

        Validate(@(
            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device0' (1/3)" 33 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/3)" 33) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device1' (2/3)" 66 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/3)" 66) +
                (Gen2 "Resuming PRTG Objects (Completed)" "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Sensor Search"          "Processing device 'Probe Device2' (3/3)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total0' (ID: 4000) (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (1/2)" 50)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Cloning PRTG Sensors"   "Cloning sensor 'Volume IO _Total1' (ID: 4001) (2/2)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  "Queuing sensor 'Volume IO _Total0' (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
                (Gen2 "Resuming PRTG Objects"  $final 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100) +
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
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (1/11)" 9)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (2/11)" 18)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total2' (3/11)" 27)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total3' (4/11)" 36)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total4' (5/11)" 45)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total5' (6/11)" 54)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total6' (7/11)" 63)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total7' (8/11)" 72)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total8' (9/11)" 81)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total9' (10/11)" 90)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total10' (11/11)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101b2: displays 10 items 'and others' with 12 objects" {

        $sensors = Get-Sensor -Count 12

        $sensors | Pause-Object -Forever

        $final = "Pausing sensors 'Volume IO _Total0', 'Volume IO _Total1', 'Volume IO _Total2', " +
                 "'Volume IO _Total3', 'Volume IO _Total4', 'Volume IO _Total5', 'Volume IO _Total6', " +
                 "'Volume IO _Total7', 'Volume IO _Total8', 'Volume IO _Total9' and 2 others forever (12/12)"

        Validate(@(
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (1/12)" 8)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (2/12)" 16)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total2' (3/12)" 25)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total3' (4/12)" 33)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total4' (5/12)" 41)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total5' (6/12)" 50)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total6' (7/12)" 58)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total7' (8/12)" 66)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total8' (9/12)" 75)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total9' (10/12)" 83)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total10' (11/12)" 91)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total11' (12/12)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101b3: processes multiple object types" {
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
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (1/6)" 16)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (2/6)" 33)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device0' (3/6)" 50)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device1' (4/6)" 66)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure0' (5/6)" 83)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure1' (6/6)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101b4: processes multiple object types 'and others' with more than 10 objects" {
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
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total0' (1/15)" 6)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total1' (2/15)" 13)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total2' (3/15)" 20)
            (Gen "Pausing PRTG Objects"         "Queuing sensor 'Volume IO _Total3' (4/15)" 26)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device0' (5/15)" 33)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device1' (6/15)" 40)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device2' (7/15)" 46)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device3' (8/15)" 53)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device4' (9/15)" 60)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device5' (10/15)" 66)
            (Gen "Pausing PRTG Objects"         "Queuing device 'Probe Device6' (11/15)" 73)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure0' (12/15)" 80)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure1' (13/15)" 86)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure2' (14/15)" 93)
            (Gen "Pausing PRTG Objects"         "Queuing group 'Windows Infrastructure3' (15/15)" 100)
            (Gen "Pausing PRTG Objects"         $final 100)
            (Gen "Pausing PRTG Objects (Completed)" $final 100)
        ))
    }

    It "101b5: displays unique channel names and groups by channel ID" {

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

    function ValidateMultiTypeCmdlet($baseType, $progressActivity, $names, $realMessage)
    {
        $lowerType = $basetype.ToLower()

        Validate(@(
            (Gen "PRTG $baseType Search"         "Retrieving all $($lowerType)s")
            (Gen "PRTG $baseType Search"         "Processing $lowerType '$($names[0])' (1/2)" 50)
            (Gen "$progressActivity"             "Queuing $lowerType '$($names[0])' (1/2)" 50)
            (Gen "$progressActivity"             "Queuing $lowerType '$($names[1])' (2/2)" 100)
            (Gen "$progressActivity"             "$realMessage (2/2)" 100)
            (Gen "$progressActivity (Completed)" "$realMessage (2/2)" 100)
        ))
    }

    It "101c1: Acknowledge-Sensor" {
        Get-Sensor -Count 2 | Acknowledge-Sensor -Duration 10

        $names = @("Volume IO _Total0","Volume IO _Total1")

        ValidateMultiTypeCmdlet "Sensor" "Acknowledge PRTG Sensors" $names "Acknowledging sensors '$($names[0])' and '$($names[1])' for 10 minutes"
    }

    It "101c2: Pause-Object" {
        Get-Device -Count 2 | Pause-Object -Forever

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" "Pausing PRTG Objects" $names "Pausing devices '$($names[0])' and '$($names[1])' forever"
    }

    It "101c3: Refresh-Object" {
        Get-Device -Count 2 | Refresh-Object

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" "Refreshing PRTG Objects" $names "Refreshing devices '$($names[0])' and '$($names[1])'"
    }

    It "101c4: Rename-Object" {
        Get-Device -Count 2 | Rename-Object "newName"

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" "Rename PRTG Objects" $names "Renaming devices '$($names[0])' and '$($names[1])' to 'newName'"
    }

    It "101c5: Resume-Object" {
        Get-Device -Count 2 | Resume-Object

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" "Resuming PRTG Objects" $names "Resuming devices '$($names[0])' and '$($names[1])'"
    }

    It "101c6: Set-ChannelProperty" {
        Get-Sensor -Count 1 | Get-Channel | Set-ChannelProperty LimitsEnabled $true

        Validate(@(
            (Gen "PRTG Sensor Search"         "Retrieving all sensors")
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (1/1)" 100)
            (Gen "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (1/1)" 100 "Retrieving all channels")

            (Gen1 "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (1/1)" 100) +
                (Gen2 "PRTG Channel Search"    "Processing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search"         "Processing sensor 'Volume IO _Total0' (1/1)" 100) +
                (Gen2 "Modify PRTG Channel Settings" "Queuing channel 'Percent Available Memory' (1/1)" 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100) +
                (Gen2 "Modify PRTG Channel Settings" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'LimitsEnabled' to 'True' (1/1)" 100)

            (Gen1 "PRTG Sensor Search (Completed)" "Processing sensor 'Volume IO _Total0' (1/1)" 100) +
                (Gen2 "Modify PRTG Channel Settings (Completed)" "Setting channel 'Percent Available Memory' (Sensor ID: 4000) setting 'LimitsEnabled' to 'True' (1/1)" 100)
        ))
    }

    It "101c7: Set-ObjectProperty" {
        Get-Device -Count 2 | Set-ObjectProperty Interval 00:00:30

        $names = @("Probe Device0", "Probe Device1")

        ValidateMultiTypeCmdlet "Device" "Modify PRTG Object Settings" $names "Setting devices '$($names[0])' and '$($names[1])' setting 'Interval' to '00:00:30'"
    }

    It "101c8: Simulate-ErrorStatus" {
        Get-Sensor -Count 2 | Simulate-ErrorStatus

        $names = @("Volume IO _Total0","Volume IO _Total1")

        ValidateMultiTypeCmdlet "Sensor" "Simulating Sensor Errors" $names "Simulating errors on sensors '$($names[0])' and '$($names[1])'"
    }

        #endregion
    #endregion
    #region Sanity Checks    
    
    It "Streams when the number of returned objects is above the threshold" {
        Run "Sensor" {

            $objs = @()

            for($i = 0; $i -lt 501; $i++)
            {
                $objs += GetItem
            }

            WithItems ($objs) {
                $result = Get-Sensor
                $result.Count | Should Be 501
            }
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

            $records += "PRTG Sensor Search`n" +
                        "    Retrieving all sensors $i/$total`n" +
                        "    $percentBar"
        }

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Detecting total number of items"

            $records

            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors 501/501" 100)
        ))
    }

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

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress when using Table -> Where" {
        Get-Device | where name -EQ "Probe Device0"

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress when using Table -> Where -> Other" {
        Get-Device | where name -EQ "Probe Device0" | fl
        
        { Get-Progress } | Should Throw "Queue empty"
    }
    
    It "Doesn't show progress when containing three Select-Object skip cmdlets piping to a table" {
        Get-Probe -Count 13 | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress when containing three Select-Object skip cmdlets piping to an action" {
        Get-Probe -Count 13 | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress when containing three Select-Object skip cmdlets piping from a variable to a table" {
        $probes = Get-Probe -Count 13

        $probes | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress when containing three Select-Object skip cmdlets piping from a variable to an action" {
        $probes = Get-Probe -Count 13

        $probes | Select -SkipLast 2 | Select -Skip 3 | Select -SkipLast 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping to a table" {
        Get-Probe -Count 13 | Select -Last 10 | Select -First 7 | Select -Last 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping to an action" {
        Get-Probe -Count 13 | Select -Last 10 | Select -First 7 | Select -Last 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping from a variable to a table" {
        $probes = Get-Probe -Count 13

        $probes | Select -Last 10 | Select -First 7 | Select -Last 4 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress containing three Select-Object cmdlets starting and ending with last piping from a variable to an action" {
        $probes = Get-Probe -Count 13
        
        $probes | Select -Last 10 | Select -First 7 | Select -Last 4 | Pause-Object -Forever -Batch:$false

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "Doesn't show progress containing three parameters over two Select-Object cmdlets" {
        Get-Probe -Count 13 | Select -First 4 -Skip 1 | Select -First 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

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
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.10' (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)"  "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "Completes all progress records when no results are returned when piping from a variable  to an Action" {
        $probes = Get-Probe -Count 2

        $probes | Get-Device -Count 0 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.10' (1/2)" 50  "Retrieving all devices")
            (Gen "PRTG Device Search"              "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)"  "Processing probe '127.0.0.11' (2/2)" 100 "Retrieving all devices")
        ))
    }

    #endregion
}