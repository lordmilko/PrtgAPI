. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "Get-Sensor_IT" {
	It "has correct number of sensors" {
		$sensors = Get-Sensor
		$sensors.Count | Should Be (Settings SensorsInTestServer)
	}

	It "can filter by name" {
		$sensors = Get-Sensor ping

		$sensors.Count | Should BeGreaterThan 0

		($sensors|where Name -ne ping).Count | Should Be 0
	}

	It "can filter by starting wildcard" {
		$sensors = Get-Sensor disk*

		$sensors.Count | Should BeGreaterThan 0

		($sensors|where Name -NotLike "disk*").Count | Should Be 0
	}

	It "can filter by ending wildcard" {

		$sensors = Get-Sensor *g

		$sensors.Count | Should BeGreaterThan 0

		($sensors|where Name -NotLike "*g").Count | Should Be 0
	}

	It "can filter by wildcard contains" {
		$sensors = Get-Sensor *harddisk*

		$sensors.Count | Should BeGreaterThan 0
		
		($sensors|where name -NotLike "*harddisk*").Count | Should Be 0
	}

	It "can filter by Id" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		$sensor.Count | Should Be 1
		$sensor.Id | Should Be (Settings UpSensor)
	}

	It "can filter by tags" {
		$sensors = Get-Sensor -Tags pingsensor

		$sensors.Count | Should BeGreaterThan 0

		($sensors | where { ($_|select -expand Tags) -notcontains "pingsensor" }).Count | Should Be 0
	}

    It "can filter by multiple tags" {
        $sensors = Get-Sensor -Tags wmicpu*,wmimemory*

        ($sensors | where name -Like Memory*).Count | Should BeGreaterThan 0
        ($sensors | where name -Like "CPU Load*").Count | Should BeGreaterThan 0
    }

	It "can filter by status" {
		$sensors = Get-Sensor -Status Down,Unknown,None

		($sensors|Where {$_.Status -eq "Unknown" -or $_.Status -eq "None"}).Count | Should Be 1
		($sensors|Where Status -eq Down).Count | Should Be 1
	}

    It "can filter by multiple IDs" {
        $upSensor = Get-Sensor -Id (Settings UpSensor)
        $upSensor | Should Not BeNullOrEmpty

        $downSensor = Get-Sensor -Id (Settings DownSensor)
        $downSensor | Should Not BeNullOrEmpty

        $sensors = Get-Sensor $upSensor.Name,$downSensor.Name

        $sensors.Count | Should Be 2

        ($sensors|Where Id -EQ (Settings UpSensor)).Count | Should Be 1
        ($sensors|Where Id -EQ (Settings DownSensor)).Count | Should Be 1
    }

    It "can filter by multiple names" {
        $sensors = Get-Sensor -Id (Settings UpSensor),(Settings DownSensor)

        $sensors.Count | Should Be 2

        ($sensors|Where Id -EQ (Settings UpSensor)).Count | Should Be 1
        ($sensors|Where Id -EQ (Settings DownSensor)).Count | Should Be 1
    }

	It "can pipe from devices" {
		$device = Get-Device -Id (Settings Device)

		$sensors = $device | Get-Sensor

		$sensors.Count | Should Be (Settings SensorsInTestDevice)
	}

	It "can pipe from groups" {
		$group = Get-Group -Id (Settings Group)

		$sensors = $group | Get-Sensor

		$sensors.Count | Should Be (Settings SensorsInTestGroup)
	}

	It "can pipe from probes" {
		$probe = Get-Probe -Id (Settings Probe)

		$sensors = $probe | Get-Sensor

		$sensors.Count | Should Be (Settings SensorsInTestProbe)
	}

	It "can pipe from search filters" {
		$sensors = New-SearchFilter type equals Ping | Get-Sensor

		$sensors.Count | Should BeGreaterThan 0

		($sensors|where Type -NE Ping).Count | Should Be 0
	}
}
