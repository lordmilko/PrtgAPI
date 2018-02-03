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
        #region 12.2: Something -> Select -First -> Table -> Something
    
    It "12.2a: Table -> Select -First -> Table -> Table" {
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

    It "12.2b: Table -> Select -First -> Table -> Action" {
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

    It "12.2c: Variable -> Select -First -> Table -> Table" {
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

    It "12.2d: Variable -> Select -First -> Table -> Action" {
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
    #endregion
    #region 13: Something -> Select -Last -> Something
        #region 13.1: Something -> Select -Last -> Something

    It "13.1a: Table -> Select -Last -> Table" {
        Get-Probe -Count 4 | Select -Last 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"              "Retrieving all probes")
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.10' (1/4)"       25)
            (Gen "PRTG Probe Search"              "Processing probe '127.0.0.11' (2/4)"       50)
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (4/4)"       100)

            (Gen "PRTG Device Search"             "Processing probe '127.0.0.12' (1/2)" 50 "Retrieving all devices")
            (Gen "PRTG Device Search"             "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
            (Gen "PRTG Device Search (Completed)" "Processing probe '127.0.0.13' (2/2)" 100 "Retrieving all devices")
        ))
    }

    It "13.1b: Table -> Select -Last -> Action" {
        Get-Probe -Count 4 | Select -Last 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                "Retrieving all probes")
            (Gen "PRTG Probe Search"                "Processing probe '127.0.0.10' (1/4)" 25)
            (Gen "PRTG Probe Search (Completed)"    "Processing probe '127.0.0.13' (4/4)" 100)

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
            (Gen "PRTG Probe Search (Completed)"  "Processing probe '127.0.0.13' (4/4)"       100)

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
            (Gen "PRTG Probe Search (Completed)"    "Processing probe '127.0.0.13' (4/4)"                     100)

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
        #region 13.2: Something -> Select -Last -> Table -> Something

    It "13.2a: Table -> Select -Last -> Table -> Table" {
        Get-Probe -Count 3 | Select -Last 2 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)"             33)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (3/3)" 100)

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

    It "13.2b: Table -> Select -Last -> Table -> Action" {
        Get-Probe -Count 3 | Select -Last 2 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search" "Retrieving all probes")
            (Gen "PRTG Probe Search" "Processing probe '127.0.0.10' (1/3)"             33)
            (Gen "PRTG Probe Search (Completed)" "Processing probe '127.0.0.12' (3/3)" 100)

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

    It "13.2c: Variable -> Select -Last -> Table -> Table" {
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

    It "13.2d: Variable -> Select -Last -> Table -> Action" {
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
        #region 14.2: Something -> Select -Skip -> Table -> Something
   
    It "14.2a: Table -> Select -Skip -> Table -> Table" {
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

    It "14.2b: Table -> Select -Skip -> Table -> Action" {
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

    It "14.2c: Variable -> Select -Skip -> Table -> Table" {
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

    It "14.2d: Variable -> Select -Skip -> Table -> Action" {
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
    #endregion
    #region 15: Something -> Select -SkipLast -> Something
        #region 15.1: Something -> Select -SkipLast -> Something

    It "15.1a: Table -> Select -SkipLast -> Table" {
        Get-Probe -Count 4 | Select -SkipLast 2 | Get-Device

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/4)" 25)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/4)" 50) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)" 50  "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/4)" 50) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/4)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")
        ))
    }

    It "15.1b: Table -> Select -SkipLast -> Action" {
        Get-Probe -Count 4 | Select -SkipLast 2 | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                     "Retrieving all probes")
            (Gen "PRTG Probe Search"                     "Processing probe '127.0.0.10' (1/4)" 25)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (2/4)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.10' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (2/4)" 50) +
                (Gen2 "Pausing PRTG Objects"             "Pausing probe '127.0.0.11' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"        "Processing probe '127.0.0.11' (2/4)" 50) +
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
        #region 15.2: Something -> Select -SkipLast -> Table -> Something

    It "15.2a: Table -> Select -SkipLast -> Table -> Table" {
        Get-Probe -Count 3 | Select -SkipLast 1 | Get-Device | Get-Sensor

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/3)" 33)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device0' (1/2)" 50 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search"         "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "PRTG Sensor Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Retrieving all sensors")

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)"  100)
        ))
    }

    It "15.2b: Table -> Select -SkipLast -> Table -> Action" {
        Get-Probe -Count 3 | Select -SkipLast 1 | Get-Device | Pause-Object -Forever -Batch:$false

        Validate(@(
            (Gen "PRTG Probe Search"                   "Retrieving all probes")
            (Gen "PRTG Probe Search"                   "Processing probe '127.0.0.10' (1/3)" 33)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.10' (1/2)"  50) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)

            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100 "Retrieving all devices")

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device0' forever (1/2)" 50)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects"       "Pausing device 'Probe Device1' forever (2/2)" 100)

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search"             "Processing probe '127.0.0.11' (2/2)"  100) +
                    (Gen3 "Pausing PRTG Objects (Completed)" "Pausing device 'Probe Device1' forever (2/2)" 100)
            
            ###################################################################

            (Gen1 "PRTG Probe Search (Completed)"      "Processing probe '127.0.0.11' (2/3)" 66) +
                (Gen2 "PRTG Device Search (Completed)" "Processing probe '127.0.0.11' (2/2)"  100)
        ))
    }

    It "15.2c: Variable -> Select -SkipLast -> Table -> Table" {
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

    It "15.2d: Variable -> Select -SkipLast -> Table -> Action" {
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
        #region 16.2: Something -> Select -Index -> Table -> Something

    It "16.2a: Table -> Select -Index -> Table -> Table" {
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

    It "16.2b: Table -> Select -Index -> Table -> Action" {
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

    It "16.2c: Variable -> Select -Index -> Table -> Table" {
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

    It "16.2d: Variable -> Select -Index -> Table -> Action" {
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
    #region 101: Something -> Action -Batch:$true
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
    #region 102: Something -> Select -Something -> Action -Batch:$true
        #region 102.1: Something -> Select -First -> Action -Batch:$true

    It "102.1a: Table -> Select -First -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -First 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/10)" 10)

            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (4/10)" 40)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.1b: Table -> Select -First -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -First 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/10)" 10)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (1/10)" 10)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (2/10)" 20)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (3/10)" 30)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (4/10)" 40)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device0', 'Probe Device0' and 'Probe Device0' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device0', 'Probe Device0' and 'Probe Device0' forever (4/4)" 100)
        ))
    }

    It "102.1c: Variable -> Select -First -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -First 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.1d: Variable -> Select -First -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10
        
        $devices | Select -First 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/10)" 10)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (1/10)" 10)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/10)" 20)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (2/10)" 20)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device2' (ID: 3002) (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (3/10)" 30)

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device3' (ID: 3003) (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (4/10)" 40)

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
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/10)" 10)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device5' (6/10)" 60)

            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (3/4)" 75)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (4/4)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.2b: Table -> Select -Last -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Last 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "102.2c: Variable -> Select -Last -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Last 4 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (3/4)" 75)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (4/4)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.2d: Variable -> Select -Last -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Last 4 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 102.3: Something -> Select -Skip -> Action -Batch:$true

    It "102.3a: Table -> Select -Skip -> Action -Batch:`$true" {        
        Get-Device -Count 10 | Select -Skip 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/10)" 10)

            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (7/10)" 70)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (8/10)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (9/10)" 90)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (10/10)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.3b: Table -> Select -Skip -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Skip 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "102.3c: Variable -> Select -Skip -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10
        
        $devices | Select -Skip 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device6' (7/10)" 70)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (8/10)" 80)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device8' (9/10)" 90)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device9' (10/10)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device6', 'Probe Device7', 'Probe Device8' and 'Probe Device9' forever (4/4)" 100)
        ))
    }

    It "102.3d: Variable -> Select -Skip -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10
        
        $devices | Select -Skip 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 102.4: Something -> Select -SkipLast -> Action -Batch:$true

    It "102.4a: Table -> Select -SkipLast -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -SkipLast 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search (Completed)" "Retrieving all devices") +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device0' (1/4)" 25)

            (Gen1 "PRTG Device Search (Completed)" "Retrieving all devices") +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device1' (2/4)" 50)

            (Gen1 "PRTG Device Search (Completed)" "Retrieving all devices") +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device2' (3/4)" 75)

            (Gen1 "PRTG Device Search (Completed)" "Retrieving all devices") +
                (Gen2 "Pausing PRTG Objects" "Queuing device 'Probe Device3' (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Retrieving all devices") +
                (Gen2 "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)

            (Gen1 "PRTG Device Search (Completed)" "Retrieving all devices") +
                (Gen2 "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.4b: Table -> Select -SkipLast -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -SkipLast 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }
    
    It "102.4c: Variable -> Select -SkipLast -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -SkipLast 6 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device0' (1/4)" 25)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device1' (2/4)" 50)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (3/4)" 75)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (4/4)" 100)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device0', 'Probe Device1', 'Probe Device2' and 'Probe Device3' forever (4/4)" 100)
        ))
    }

    It "102.4d: Variable -> Select -SkipLast -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -SkipLast 6 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

        #endregion
        #region 102.5: Something -> Select -Index -> Action -Batch:$true

    It "102.5a: Table -> Select -Index -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Index 2,3,5,7 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device5' (6/10)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (8/10)" 80)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
        ))
    }

    It "102.5b: Table -> Select -Index -> Action -> Action -Batch:`$true" {
        Get-Device -Count 10 | Select -Index 2,3,5,7 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "102.5c: Variable -> Select -Index -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Index 2,3,5,7 | Pause-Object -Forever -Batch:$true

        Validate(@(
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device2' (3/10)" 30)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device3' (4/10)" 40)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device5' (6/10)" 60)
            (Gen "Pausing PRTG Objects" "Queuing device 'Probe Device7' (8/10)" 80)

            (Gen "Pausing PRTG Objects" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
            (Gen "Pausing PRTG Objects (Completed)" "Pausing devices 'Probe Device2', 'Probe Device3', 'Probe Device5' and 'Probe Device7' forever (4/4)" 100)
        ))
    }

    It "102.5d: Variable -> Select -Index -> Action -> Action -Batch:`$true" {
        $devices = Get-Device -Count 10

        $devices | Select -Index 2,3,5,7 | Clone-Object 5678 | Pause-Object -Forever -Batch:$true

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "blah" {
        throw "we can maybe do this automatically, and just verify the last cmdlet ends in 'completed'"
        throw "in fact, we should probably modify the existing testcmdlet function to do that little check too"
        throw "why is 3a failing in ise"
        throw "need to make prtgcmdlet.processrecord have a null progress manager or something when progress is disabled (cant be null though cos we'll immediately get a null reference exception)'"
        throw "need some tests with restartprobe"
        throw "have the existing testbatchcmdlet methods also do a second test with a where-object in the middle and see how it goes"
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

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)
        ))
    }

    It "104.1b: Variable -> Get-SensorTarget" {
        $devices = Get-Device -Count 2
        
        $devices | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device0' (1/2)" 50 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG WMI Service Search" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.1c: Table -> Table -> Get-SensorTarget" {
        Get-Group -Count 2 | Get-Device -Count 2 | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)

            ###################################################################

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device1' (2/2)" 100) +
                    (Gen3 "PRTG WMI Service Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "104.1d: Variable -> Table -> Get-SensorTarget" {
        $groups = Get-Group -Count 2
        
        $groups | Get-Device -Count 2 | Get-SensorTarget WmiService

        Validate(@(
            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (1/2)" 50 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (1/2)" 50 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (100%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (100%)")

            ##########################################################################################

            (Gen "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (1/2)" 50 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device0' (1/2)" 50 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (100%)")

            (Gen1 "PRTG Device Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG WMI Service Search (Completed)" "Processing device 'Probe Device1' (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG Device Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "104.1e: Table -> Action -> Get-SensorTarget" {

        Get-Device -Count 2 | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

    It "104.1f: Variable -> Action -> Get-SensorTarget" {
        $devices = Get-Device -Count 2
        
        $devices | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (50%)")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50 "Probing target device (100%)")

            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (50%)")
            (Gen "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.1g: Table-> Table -> Action -> Get-SensorTarget" {
        Get-Group -Count 2 | Get-Device -Count 2 | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Group Search" "Retrieving all groups")
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure0' (1/2)" 50) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            ##########################################################################################

            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100)
            (Gen "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100 "Retrieving all devices")

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                    (Gen3 "PRTG Exe/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen1 "PRTG Group Search" "Processing group 'Windows Infrastructure1' (2/2)" 100) +
                (Gen2 "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)

            (Gen "PRTG Group Search (Completed)" "Processing group 'Windows Infrastructure1' (2/2)" 100)
        ))
    }

    It "104.1h: Variable -> Table -> Action -> Get-SensorTarget" {
        $groups = Get-Group -Count 2
        
        $groups = Get-Device -Count 2 | Clone-Object 5678 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/2)" 50)
            
            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device0' (ID: 3000) (1/2)" 50) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "Cloning PRTG Devices" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "Cloning PRTG Devices (Completed)" "Cloning device 'Probe Device1' (ID: 3001) (2/2)" 100)
        ))
    }

        #endregion
        #region 104.2: Select-Object

    It "104.2a: Table -> Select -First -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -First 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/5)" 20) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/5)" 20) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device0' (1/5)" 20) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)
            
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device1' (2/5)" 40)
        ))
    }

    It "104.2b: Variable -> Select -First -> Get-SensorTarget" {
        $devices = Get-Device -Count 5
        
        $devices | Select -First 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (1/5)" 20 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (1/5)" 20 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/5)" 40 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/5)" 40 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device1' (2/5)" 40 "Probing target device (100%)")
        ))
    }

    It "104.2c: Table -> Select -Last -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -Last 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/5)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40)
            (Gen "PRTG Device Search" "Processing device 'Probe Device2' (3/5)" 60)
            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (5/5)" 100)

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (1/2)" 50 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device4' (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2d: Variable -> Select -Last -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -Last 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (1/2)" 50 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (1/2)" 50 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (2/2)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (2/2)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device4' (2/2)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2e: Table -> Select -Skip -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -Skip 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/5)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device4' (5/5)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device4' (5/5)" 100) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device4' (5/5)" 100) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device4' (5/5)" 100)
        ))
    }

    It "104.2f: Variable -> Select -Skip -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -Skip 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (3/5)" 60 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (3/5)" 60 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (4/5)" 80 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (4/5)" 80 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (5/5)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device4' (5/5)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device4' (5/5)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2g: Table -> Select -SkipLast -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -SkipLast 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")
            (Gen "PRTG Device Search" "Processing device 'Probe Device0' (1/5)" 20)
            (Gen "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40)

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (1/3)" 33 "Probing target device (50%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (1/3)" 33 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/3)" 66 "Probing target device (50%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/3)" 66 "Probing target device (100%)")

            ###################################################################

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (3/3)" 100 "Probing target device (50%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (3/3)" 100 "Probing target device (100%)")

            (Gen1 "PRTG Device Search (Completed)" "Processing device 'Probe Device2' (3/5)" 60) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2h: Variable -> Select -SkipLast -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -SkipLast 2 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (1/3)" 33 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device0' (1/3)" 33 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/3)" 66 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/3)" 66 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (3/3)" 100 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device2' (3/3)" 100 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device2' (3/3)" 100 "Probing target device (100%)")
        ))
    }

    It "104.2i: Table -> Select -Index -> Get-SensorTarget" {
        Get-Device -Count 5 | Select -Index 1,3 | Get-SensorTarget ExeXml

        Validate(@(
            (Gen "PRTG Device Search" "Retrieving all devices")

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device1' (2/5)" 40) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            ###################################################################

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (50%)" 50)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search" "Probing target device (100%)" 100)

            (Gen1 "PRTG Device Search" "Processing device 'Probe Device3' (4/5)" 80) +
                (Gen2 "PRTG EXE/Script File Search (Completed)" "Probing target device (100%)" 100)

            (Gen "PRTG Device Search (Completed)" "Processing device 'Probe Device3' (4/5)" 80)
        ))
    }

    It "104.2j: Variable -> Select -Index -> Get-SensorTarget" {
        $devices = Get-Device -Count 5

        $devices | Select -Index 1,3 | Get-SensorTarget ExeXml
        
        Validate(@(
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/5)" 40 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device1' (2/5)" 40 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (4/5)" 80 "Probing target device (50%)")
            (Gen "PRTG EXE/Script File Search" "Processing device 'Probe Device3' (4/5)" 80 "Probing target device (100%)")

            (Gen "PRTG EXE/Script File Search (Completed)" "Processing device 'Probe Device3' (4/5)" 80 "Probing target device (100%)")
        ))
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
            (Gen "PRTG Sensor Search (Completed)" "Retrieving all sensors 501/501" 100)
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
    #endregion
}