. $PSScriptRoot\Support\IntegrationTestSafe.ps1

function RetrieveProperties([ScriptBlock]$getObjects)
{
	$properties = (& $getObjects) | Get-ObjectProperty

	$properties.Count | Should BeGreaterThan 1
}

Describe "Get-ObjectProperty_IT" {
	It "retrieves all sensor properties" {
		RetrieveProperties { Get-Sensor }
	}

	It "retrieves all device properties" {
		RetrieveProperties { Get-Device }
	}

	It "retrieves all group properties" {
		RetrieveProperties { Get-Group }
	}

	It "retrieves all probe properties" {
		RetrieveProperties { Get-Group }
	}

	It "retrieves an raw property" {
		$device = Get-Device -Id (Settings Device)

		$property = $device | Get-ObjectProperty -RawProperty "name"

		$property | Should Be (Settings DeviceName)
	}

	It "retrieves an raw property with a trailing underscore" {
		$device = Get-Device -Id (Settings Device)

		$property = $device | Get-ObjectProperty -RawProperty "name_"

		$property | Should Be (Settings DeviceName)
	}

	It "throws retrieving a raw inheritance flag" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		{ $sensor | Get-ObjectProperty -RawProperty "accessgroup" } | Should Throw "A value for property 'accessgroup' could not be found"
	}

	It "retrieves an invalid unsupported property" {
		$device = Get-Device -Id (Settings Device)

		{ $device | Get-ObjectProperty -RawProperty "banana" } | Should Throw "A value for property 'banana' could not be found"
	}
}