. $PSScriptRoot\Support\Progress.ps1

Describe "Test-Progress" -Tag @("PowerShell", "UnitTest") {
    
    InitializeClient
    
    $filter = $null

    function It($name, $script)
    {
        if($filter -eq $null -or $name -like $filter)
        {
            Pester\It $name $script
        }
    }

    #region 1: Something -> Action
    
    It "1a: Table -> Action" {
        Get-Sensor -Count 1 | Pause-Object -Forever

        Validate (@(
            "PRTG Sensor Search`n" +
            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing sensor 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Pausing PRTG Objects`n" +
            "    Pausing sensor 'Volume IO _Total0' forever (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Pausing PRTG Objects (Completed)`n" +
            "    Pausing sensor 'Volume IO _Total0' forever (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    It "1b: Variable -> Action" {
        $devices = Get-Device

        $devices.Count | Should Be 2

        $devices | Pause-Object -Forever

        Validate (@(
            "Pausing PRTG Objects`n" +
            "    Pausing device 'Probe Device0' forever (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Pausing PRTG Objects`n" +
            "    Pausing device 'Probe Device1' forever (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Pausing PRTG Objects (Completed)`n" +
            "    Pausing device 'Probe Device1' forever (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 2: Something -> Table

    It "2a: Table -> Table" {
        Get-Probe | Get-Group

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"
        ))
    }

    It "2b: Variable -> Table" {

        $probes = Get-Probe

        $probes.Count | Should Be 2

        $probes | Get-Sensor

        Validate (@(
            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"
        ))
    }

    #endregion
    #region 3: Something -> Action -> Table
    
    It "3a: Table -> Action -> Table" {

        Get-Device | Clone-Device 5678 | Get-Sensor

        Validate (@(
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "3b: Variable -> Action -> Table" {

        $devices = Get-Device

        $devices | Clone-Device 5678 | Get-Sensor

        Validate(@(
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"
        ))
    }

    #endregion
    #region 4: Something -> Table -> Table

    It "4a: Table -> Table -> Table" {

        Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

        Validate (@(
            "PRTG Group Search`n" +
            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search (Completed)`n" +
            "        Processing device 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing group 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
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
            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 5: Something -> Table -> Action
    
    It "5a: Table -> Table -> Action" {
        Get-Device | Get-Sensor | Pause-Object -Forever

        Validate(@(
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "5b: Variable -> Table -> Action" {
        $devices = Get-Device

        $devices | Get-Sensor | Pause-Object -Forever

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Processing all devices 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 6: Something -> Table -> Action -> Table
    
    It "6a: Table -> Table -> Action -> Table" {
        Get-Group | Get-Device | Clone-Device 5678 | Get-Sensor

        Validate(@(
            "PRTG Group Search`n" +
            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Devices (Completed)`n" +
            "        Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Devices`n" +
            "        Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Devices (Completed)`n" +
            "        Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "6b: Variable -> Table -> Action -> Table" {
        $probes = Get-Probe

        $probes | Get-Group -Count 1 | Clone-Group 5678 | Get-Device

        Validate(@(
            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Groups`n" +
            "        Cloning group 'Windows Infrastructure0' (ID: 2211) (1/1)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Groups`n" +
            "        Cloning group 'Windows Infrastructure0' (ID: 2211) (1/1)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Cloning PRTG Groups (Completed)`n" +
            "        Cloning group 'Windows Infrastructure0' (ID: 2211) (1/1)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Groups`n" +
            "        Cloning group 'Windows Infrastructure0' (ID: 2211) (1/1)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Groups`n" +
            "        Cloning group 'Windows Infrastructure0' (ID: 2211) (1/1)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Cloning PRTG Groups (Completed)`n" +
            "        Cloning group 'Windows Infrastructure0' (ID: 2211) (1/1)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 7: Something -> Object

    It "7a: Table -> Object" {
        Get-Sensor -Count 1 | Get-Channel

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing sensor 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing sensor 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing sensor 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"
        ))
    }

    It "7b: Variable -> Object" {

        #1. why is pipes three data cmdlets together being infected by the crash here
        #2. why is injected_showchart failing to deserialize?

        $result = Run "Sensor" {

            $obj1 = GetItem
            $obj2 = GetItem

            WithItems ($obj1, $obj2) {
                Get-Sensor -Count 2
            }
        }

        $result.Count | Should Be 2
        $result | Get-Channel

        Validate(@(
            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"
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
                        "    Processing sensor $i/$total`n" +
                        "    $percentBar`n" +
                        "    Retrieving all channels"
        }

        Validate(@(
            "PRTG Sensor Search`n" +
            "    Detecting total number of items"

            "PRTG Sensor Search`n" +
            "    Processing sensor 1/501`n" +
            "    [                                        ] (0%)"

            $records

            "PRTG Sensor Search (Completed)`n" +
            "    Processing sensor 501/501`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            "    Retrieving all channels"
        ))
    }
    
    It "8b: Stream -> Action" {

        # Besides the initial "Detecting total number of items", there is nothing special about a streamed, non-streamed and streaming-unsupported (e.g. devices) run

        $counts = @{
            Sensors = 501
        }

        RunCustomCount $counts {
            Get-Sensor | Pause-Object -Forever
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

            if($i -gt 1)
            {
                $records += "Pausing PRTG Objects`n" +
                            "    Processing sensor $i/$total`n" +
                            "    $percentBar"
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

            "PRTG Sensor Search`n" +
            "    Processing sensor 1/501`n" +
            "    [                                        ] (0%)"

            $records

            "Pausing PRTG Objects (Completed)`n" +
            "    Pausing sensor 'Volume IO _Total0' forever (501/501)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
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
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing sensor 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing sensor 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
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
            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 10: Something -> Action -> Table -> Table
    
    It "10a: Table -> Action -> Table -> Table" {
        Get-Device | Clone-Device 5678 | Get-Sensor | Get-Channel

        Validate(@(
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "10b: Variable -> Action -> Table -> Table" {
        # an extension of 3b. variable -> action -> table. Confirms that we can transform our setpreviousoperation into a
        # proper progress item when required

        #BUG----------------we should have TWO sensors in our progress, but we're only showing 1???

        #RunCustomCount $counts {
            $devices = Get-Device

            $devices | Clone-Device 5678 | Get-Sensor | Get-Channel        
        #}

        Validate(@(
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"
            
            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################
            
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################
            
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/2)`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################
            
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################
            
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################
            
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################
            
            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing sensor 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (2/2)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    #endregion
    #region 11: Variable -> Table -> Table -> Table

    It "11: Variable -> Table -> Table -> Table" {
        # Validates we can get at least two progress bars out of a variable
        $probes = Get-Probe

        $probes | Get-Group -Count 1 | Get-Device -Count 1 | Get-Sensor

        Validate(@(
            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    PRTG Device Search`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    PRTG Device Search`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        PRTG Sensor Search`n" +
            "            Processing all devices 1/1`n" +
            "            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "            Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    PRTG Device Search`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        PRTG Sensor Search (Completed)`n" +
            "            Processing all devices 1/1`n" +
            "            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "            Retrieving all sensors"

            ###################################################################


            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    PRTG Device Search (Completed)`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Device Search`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Device Search`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        PRTG Sensor Search`n" +
            "            Processing all devices 1/1`n" +
            "            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "            Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Device Search`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        PRTG Sensor Search (Completed)`n" +
            "            Processing all devices 1/1`n" +
            "            [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "            Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Device Search (Completed)`n" +
            "        Processing all groups 1/1`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 12: Table -> Filter -> Something

    It "12a: Table -> Filter -> Table" {
        Get-Probe | Select-Object -First 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "12b: Table -> Filter -> Action" {
        Get-Probe | Select-Object -First 2 | Pause-Object -Forever

        { Get-Progress } | Should Throw "Queue empty"
    }

    #endregion
    #region 13: Variable -> Filter -> Something

    It "13a: Variable -> Filter -> Table" {
        $probes = Get-Probe

        $probes | Select-Object -First 2 | Get-Device

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "13b: Variable -> Filter -> Action" {
        $probes = Get-Probe

        $probes | Select-Object -First 2 | Pause-Object -Forever

        { Get-Progress } | Should Throw "Queue empty"
    }

    #endregion
    #region 14: Table -> Filter -> Table -> Something

    It "14a: Table -> Filter -> Table -> Table" {
        Get-Probe | Select-Object -First 2 | Get-Device | Get-Sensor

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "14b: Table -> Filter -> Table -> Action" {
        Get-Probe | Select-Object -First 2 | Get-Device | Pause-Object -Forever

        { Get-Progress } | Should Throw "Queue empty"
    }

    #endregion
    #region 15: Variable -> Filter -> Table -> Something

    It "15a: Variable -> Filter -> Table -> Table" {
        $probes = Get-Probe

        $probes | Select-Object -First 2 | Get-Device | Get-Sensor

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "15b: Variable -> Filter -> Table -> Action" {
        $probes = Get-Probe

        $probes | Select-Object -First 2 | Get-Device | Pause-Object -Forever

        { Get-Progress } | Should Throw "Queue empty"
    }
    
    #endregion
    #region 16: Something -> Where -> Something
    
    It "16a: Table -> Where -> Table" {

        $counts = @{
            ProbeNode = 3
        }

        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device
        }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/3`n" +
            "    [oooooooooooooooooooooooooo              ] (66%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }
    
    It "16b: Variable -> Where -> Table" {
        $counts = @{
            ProbeNode = 3
        }
        
        $probes = RunCustomCount $counts {
            Get-Probe
        }

        $probes.Count | Should Be 3

        $probes | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }

    It "16c: Table -> Where -> Action" {
        
        $counts = @{
            ProbeNode = 3
        }

        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Pause-Object -Forever
        }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)"

            ###################################################################

            "Pausing PRTG Objects`n" +
            "    Pausing probe '127.0.0.10' forever (1/3)`n" +
            "    [ooooooooooooo                           ] (33%)"

            ###################################################################

            "Pausing PRTG Objects`n" +
            "    Processing probe 2/3`n" +
            "    [oooooooooooooooooooooooooo              ] (66%)"

            ###################################################################

            "Pausing PRTG Objects`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Pausing PRTG Objects`n" +
            "    Pausing probe '127.0.0.12' forever (3/3)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Pausing PRTG Objects (Completed)`n" +
            "    Pausing probe '127.0.0.12' forever (3/3)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    #endregion
    #region 17: Something -> Table -> Where -> Table
    
    It "17a: Table -> Table -> Where -> Table" {

        Get-Probe | Get-Group | where name -EQ "Windows Infrastructure0" | Get-Sensor

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search (Completed)`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search (Completed)`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "17b: Variable -> Table -> Where -> Table" {
        $probes = Get-Probe

        $probes | Get-Group | where name -like * | Get-Sensor

        Validate(@(
            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    #endregion
    #region 18: Something -> Where -> Something -> Something

    It "18a: Table -> Where -> Table -> Table" {
        $counts = @{
            ProbeNode = 3
        }
        
        RunCustomCount $counts {
            Get-Probe | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device | Get-Sensor
        }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Device Search (Completed)`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/3`n" +
            "    [oooooooooooooooooooooooooo              ] (66%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search (Completed)`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "18b: Variable -> Where -> Table -> Table" {
        $counts = @{
            ProbeNode = 3
        }
        
        $probes = RunCustomCount $counts {
            Get-Probe
        }

        $probes.Count | Should Be 3

        $probes | where { $_.name -EQ "127.0.0.10" -or $_.name -eq "127.0.0.12" } | Get-Device | Get-Sensor

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 1/3`n" +
            "    [ooooooooooooo                           ] (33%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 3/3`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    #endregion
    #region 19: Variable(1) -> Table -> Table

    It "19: Variable(1) -> Table -> Table" {

        $probe = Get-Probe -Count 1

        $probe.Count | Should Be 1

        $probe | Get-Group | Get-Device

        Validate(@(

            "PRTG Group Search`n" +
            "    Processing all probes 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing all groups 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search (Completed)`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all devices"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #the next record should be a prtg device search

            #i think that if we have 1 variable and we're piping to multiple that deserves progress. maybe have PipeFromVariable detect we're also pipetocmdlet
        ))
    }

    #endregion
    #region 20: Something -> PSObject
    
    It "20a: Table -> PSObject" {
        Get-Device | Get-Trigger -Types

        Validate(@(
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all notification trigger types"
        ))
    }

    It "20b: Variable -> PSObject" {
        $devices = Get-Device

        $devices | Get-Trigger -Types

        Validate(@(
            "PRTG Notification Trigger Type Search`n" +
            "    Processing all devices 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all notification trigger types"

            ###################################################################

            "PRTG Notification Trigger Type Search`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all notification trigger types"

            ###################################################################

            "PRTG Notification Trigger Type Search (Completed)`n" +
            "    Processing all devices 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all notification trigger types"
        ))
    }

    #endregion
    #region 21: Something -> Table -> PSObject

    It "21a: Table -> Table -> PSObject" {
        Get-Group | Get-Device | Get-Trigger -Types

        Validate(@(
            "PRTG Group Search`n" +
            "    Retrieving all groups"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Device Search (Completed)`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Device Search (Completed)`n" +
            "        Processing device 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing group 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }

    It "21b: Variable -> Table -> PSObject" {
        $groups = Get-Group

        $groups | Get-Device | Get-Trigger -Types

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all groups 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Notification Trigger Type Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Notification Trigger Type Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Notification Trigger Type Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Notification Trigger Type Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Notification Trigger Type Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Notification Trigger Type Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all notification trigger types"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    #endregion
    #region 22: Something -> Where { Variable(1) -> Table }

    It "22a: Table -> Where { Variable(1) -> Table }" {
        Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "22b: Variable -> Where { Variable(1) -> Table }" {
        $probes = Get-Probe

        $probes | where { $_ | Get-Sensor }

        { Get-Progress } | Should Throw "Queue empty"
    }
    
    #endregion
    #region 23: Something -> Table -> Where { Variable(1) -> Table }

    It "23a: Table -> Table -> Where { Variable(1) -> Table }" {
        Get-Probe | Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }

    It "23b: Variable -> Table -> Where { Variable(1) -> Table }" {
        $probes = Get-Probe

        $probes | Get-Device | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" }

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }
    
    #endregion
    #region 24: Something -> Where { Table -> Table }

    It "24a: Table -> Where { Table -> Table }" {
        Get-Probe | where { Get-Device | Get-Sensor }

        Validate (@(
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"
        ))
    }

    It "24b: Variable -> Where { Table -> Table }" {
        $probes = Get-Probe

        $probes | where { Get-Device | Get-Sensor }

        Validate (@(
            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing device 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"
        ))
    }
    
    #endregion
    #region 25: Something -> Where { Variable -> Where { Variable(1) -> Table } }

    It "25a: Table -> Where { Variable -> Where { Variable(1) -> Table } }" {
        Get-Probe | where {
            ($_ | Get-Device | where {
                ($_|Get-Sensor).Name -eq "Volume IO _Total0"
            }).Name -eq "Probe Device0"
        }

        { Get-Progress } | Should Throw "Queue empty"
    }

    It "25b: Variable -> Where { Variable -> Where { Variable -> Table } }" {
        $probes = Get-Probe

        $probes | where {
            ($_ | Get-Device | where {
                ($_|Get-Sensor).Name -eq "Volume IO _Total0"
            }).Name -eq "Probe Device0"
        }

        { Get-Progress } | Should Throw "Queue empty"
    }

    #endregion
    #region 26: Something -> Where { Variable(1) -> Table } -> Table

    It "26a: Table -> Where { Variable(1) -> Table } -> Table" {
        Get-Probe | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" } | Get-Device

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }

    It "26b: Variable -> Where { Variable(1) -> Table } -> Table" {
        $probes = Get-Probe

        $probes | where { ($_ | Get-Sensor).Name -eq "Volume IO _Total0" } | Get-Device

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }
    
    #endregion
    #region 27: Something -> Table -> Where { Variable(1) -> Table -> Table }
    
    It "27a: Table -> Table -> Where { Variable(1) -> Table -> Table }" {
        Get-Probe | Get-Group | where {
            ($_ | Get-Device | Get-Sensor).Name -eq "Volume IO _Total0"
        }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"
        ))
    }
    
    It "27b: Variable -> Table -> Where { Variable(1) -> Table -> Table }" {
        $probes = Get-Probe
        
        $probes | Get-Group | where {
            ($_ | Get-Device | Get-Sensor).Name -eq "Volume IO _Total0"
        }

        Validate(@(
            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    Retrieving all groups"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all groups"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all groups"
        ))
    }
    
    #endregion
    #region 28: Something -> Table -> Where { Variable(1) -> Table -> Table -> Where { Variable -> Object } }
    
    It "28a: Table -> Table -> Where { Variable(1) -> Table -> Table -> Where { Variable -> Object } }" {
        
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
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +
            
            "    Retrieving all groups"

            ##########################################################################################

            #region Probe 1, Group 1

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 1, Group 1

            ##########################################################################################

            #region Probe 1, Group 2

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 1, Group 2

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all groups"

            ##########################################################################################

            #region Probe 2, Group 1

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 2

            ##########################################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all groups"

            ##########################################################################################
        ))
    }

    It "28b: Variable -> Table -> Where { Variable(1) -> Table -> Table -> Where { Variable -> Object } }" {
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
            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ##########################################################################################

            #region Probe 2, Group 1

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 2

            ##########################################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ##########################################################################################

            #region Probe 2, Group 1

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 1

            ##########################################################################################

            #region Probe 2, Group 2

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            ###################################################################

            "PRTG Channel Search (Completed)`n" +
            "    Processing all sensors 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all channels"

            #################################################################################################################

            "PRTG Device Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all devices 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 2

            ##########################################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"
        ))
    }

    #endregion
    #region 29: Something -> Table | Where { Variable(1) -> Table -> Table } -> Table

    It "29a: Table -> Table | Where { Variable(1) -> Table -> Table } -> Table" {

        Get-Probe | Get-Group | Where {
            ($_ | Get-Sensor | Get-Channel).Name -eq "Percent Available Memory"
        } | Get-Sensor

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ##########################################################################################

            #region Probe 1, Group 1

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 1, Group 2

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ##########################################################################################

            #region Probe 1, Group 2

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 1, Group 2

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Group Search (Completed)`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ##########################################################################################

            #region Probe 2, Group 1

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 1

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ##########################################################################################
            
            #region Probe 2, Group 2

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 2

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Group Search (Completed)`n" +
            "        Processing group 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    It "29b: Variable -> Table | Where { Variable(1) -> Table -> Table } -> Table" {

        #TODO: technically speaking, this functionality does not work properly. Because when showing progress
        #when piping from a variable, the last cmdlet is responsible for updating the second last cmdlet's count.
        #In this instance, the last Get-Sensor is responsible for updating the count of the number of groups that
        #have been processed. However, because there is a Where-Object in the middle, Get-Sensor is not
        #called for every group that is processed, and therefore you can't see the groups processed count increase

        $probes = Get-Probe

        $probes | Get-Group | Where {
            ($_ | Get-Sensor | Get-Channel).Name -eq "Percent Available Memory"
        } | Get-Sensor

        Validate(@(
            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all groups"

            ##########################################################################################

            #region Probe 1, Group 1

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 1, Group 1

            ##########################################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            ##########################################################################################

            #region Probe 1, Group 2

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 1, Group 2

            ##########################################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all groups"

            ##########################################################################################

            #region Probe 2, Group 1

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 1

            ##########################################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all sensors"

            
            ##########################################################################################

            #region Probe 2, Group 2

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 1/2`n" +
            "        [oooooooooooooooooooo                    ] (50%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    PRTG Channel Search (Completed)`n" +
            "        Processing all sensors 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all channels"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all groups 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            #endregion Probe 2, Group 2

            ##########################################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    PRTG Sensor Search (Completed)`n" +
            "        Processing all groups 2/2`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "        Retrieving all sensors"

            ###################################################################

            "PRTG Group Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
        ))
    }
    
    #endregion
    #region 30: Something -> Table -> Where { Variable(1) -> Action }

    It "30a: Table -> Table | Where { Variable(1) -> Action }" {
        Get-Probe | Get-Device | Where { $_ | Pause-Object -Forever }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }
    
    It "30b: Variable -> Table | Where { Variable(1) -> Action }" {
        $probes = Get-Probe

        $probes | Get-Device | Where { $_ | Pause-Object -Forever }

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ###################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))

    }

    #endregion
    #region 31: Something -> Table -> Where { Variable(1) -> Table -> Action }
    
    It "31a: Table -> Table -> Where { Variable(1) -> Table -> Action }" {
        Get-Probe | Get-Device | Where { $_ | Get-Sensor | Pause-Object -Forever }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }

    It "31b: Variable -> Table -> Where { Variable(1) -> Table -> Action }" {
        $probes = Get-Probe
        
        $probes | Get-Device | Where { $_ | Get-Sensor | Pause-Object -Forever }

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total0' forever (1/2)`n" +
            "        [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Pausing PRTG Objects (Completed)`n" +
            "        Pausing sensor 'Volume IO _Total1' forever (2/2)`n" +
            "        [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "PRTG Sensor Search (Completed)`n" +
            "    Processing all devices 1/1`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ##########################################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }

    #endregion
    #region 32: Something -> Table -> Where { Variable(1) -> Action -> Table }

    It "32a: Table -> Table -> Where { Variable(1) -> Action -> Table }" {
        Get-Probe | Get-Device | Where { $_ | Clone-Device 5678 | Get-Sensor }

        Validate(@(
            "PRTG Probe Search`n" +
            "    Retrieving all probes"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)"

            ###################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all sensors"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ##########################################################################################

            "PRTG Probe Search`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all sensors"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ##########################################################################################

            "PRTG Probe Search (Completed)`n" +
            "    Processing probe 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }

    It "32b: Variable -> Table -> Where { Variable(1) -> Action -> Table }" {
        $probes = Get-Probe
        
        $probes | Get-Device | Where { $_ | Clone-Device 5678 | Get-Sensor }

        Validate(@(
            "PRTG Device Search`n" +
            "    Processing all probes 1/2`n" +
            "    [oooooooooooooooooooo                    ] (50%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all sensors"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ##########################################################################################

            "PRTG Device Search`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device0' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +
            
            "    Retrieving all sensors"

            ##########################################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"

            ###################################################################

            "Cloning PRTG Devices`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ###################################################################

            "Cloning PRTG Devices (Completed)`n" +
            "    Cloning device 'Probe Device1' (ID: 40) (1/1)`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all sensors"

            ##########################################################################################

            "PRTG Device Search (Completed)`n" +
            "    Processing all probes 2/2`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)`n" +

            "    Retrieving all devices"
        ))
    }
    
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

            "PRTG Sensor Search (Completed)`n" +
            "    Retrieving all sensors 501/501`n" +
            "    [oooooooooooooooooooooooooooooooooooooooo] (100%)"
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
    
    #endregion
}