. $PSScriptRoot\Support\IntegrationTest.ps1

function TestScanningInterval($expectedString, $value)
{
	$sensor = Get-Sensor -Id (Settings UpSensor)

	$initialInterval = $($sensor | Get-ObjectProperty).Interval
	$initialInterval | Should Not Be $expectedString

	$sensor | Set-ObjectProperty interval $value

	$newInterval = $($sensor | Get-ObjectProperty).Interval

	$newInterval | Should Be $expectedString
}

Describe "Set-ObjectProperty_IT" {
	
	It "sets a raw property" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		$initialTimeout = ($sensor | Get-ObjectProperty).Timeout
		$initialTimeout | Should Not Be 7

		$sensor | Set-ObjectProperty -RawProperty "timeout_" -RawValue "7" -Force

		$newTimeout = ($sensor | Get-ObjectProperty).Timeout

		$newTimeout | Should Be 7
	}

	It "sets a scanning interval from an enum"       { TestScanningInterval "01:00:00" OneHour }

	It "sets a scanning interval from a TimeSpan"    { TestScanningInterval "00:10:00" ([TimeSpan]"00:10") }

	It "sets a scanning interval from an integer"    { TestScanningInterval "00:05:00" 300 }

	It "can compare a ScanningInterval with a ScanningInterval" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		$initialInterval = $($sensor | Get-ObjectProperty).Interval

		$sensor | Set-ObjectProperty "Interval" $initialInterval

		$newInterval = $($sensor | Get-ObjectProperty).Interval

		$initialInterval | Should Be $newInterval
	}

	It "sets a custom scanning interval"             { TestScanningInterval "00:00:10" (Settings CustomInterval) }

	It "sets a custom unsupported scanning interval" { TestScanningInterval "00:00:05" (Settings CustomUnsupportedInterval) }

	It "throws setting an empty value on a required property" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		$message = "failed due to the following: Sensor Name: Required field, not defined. The object has not been changed"

		{ $sensor | Set-ObjectProperty Name $null } | Should Throw $message
	}

	It "setting an invalid scanning interval sets the nearest valid interval" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		$initialInterval = $($sensor | Get-ObjectProperty).Interval
		$initialInterval | Should Not Be "00:00:55"

		$sensor | Set-ObjectProperty interval "00:00:55"

		$newInterval = $($sensor | Get-ObjectProperty).Interval

		$newInterval | Should Be "00:01:00"
	}
}

Describe "Set-ObjectProperty_Sensors_IT" {

	$sensor = $null

	function GetValue($property, $expected)
	{
		LogTestDetail "Processing property $property"

		$sensor | Assert-True -Message "Sensor was not initialized"
		$expected | Should Not BeNullOrEmpty

		$initialSettings = $sensor | Get-ObjectProperty

		$initialSettings.$property | Should Be $expected
	}

	function SetValue($property, $value)
	{
		LogTestDetail "Processing property $property"

		$sensor | Assert-True -Message "Sensor was not initialized"

		$initialSettings = $sensor | Get-ObjectProperty

		$sensor | Set-ObjectProperty $property $value

		$newSettings = $sensor | Get-ObjectProperty

		$newSettings.$property | Assert-NotEqual $initialSettings.$property -Message "Expected initial and new value to be different, but they were both '<actual>'"
		$newSettings.$property | Should Not BeNullOrEmpty

		$newSettings.$property | Should Be $value
	}

	function SetChild($property, $value, $dependentProperty, $dependentValue)
	{
		LogTestDetail "Processing property $property"

		$sensor | Assert-True -Message "Sensor was not initialized"

		$initialSettings = $sensor | Get-ObjectProperty
		$initialValue = $initialSettings.$property
		$initialDependent = $initialSettings.$dependentProperty

		$sensor | Set-ObjectProperty $property $value

		$newSettings = $sensor | Get-ObjectProperty
		$newValue = $newSettings.$property
		$newDependent = $newSettings.$dependentProperty

		$newValue | Assert-NotEqual $initialValue -Message "Expected initial and new value to be different, but they were both '<actual>'"
		$newDependent | Assert-NotEqual $initialDependent -Message "Expected initial and new dependent to be different, but they were both '<actual>'"
		$newValue | Should Not BeNullOrEmpty

		$newValue | Assert-Equal $value
		$newDependent | Assert-Equal $dependentValue

		$sensor | Set-ObjectProperty $dependentProperty $initialDependent
	}

	It "Basic Sensor Settings" {

		$sensor = Get-Sensor -Id (Settings UpSensor)

		SetValue "Name"       "TestName"
		GetValue "ParentTags" (Settings ParentTags)
		#SetValue "Tags"       "TestTag"       (Settings BLAH) #is this going to set or overwrite? we need the user to be able to indicate which one they want. maybe have an add-channelproperty?
		SetValue "Priority"   5
	}

	It "Ping Settings" {
		
		$sensor = Get-Sensor Ping

		SetValue "Timeout"     3
		SetValue "PingPacketSize"  33
		SetValue "PingMode"        "SinglePing"
		SetValue "PingCount"       6
		SetValue "PingDelay"       6
		SetValue "AutoAcknowledge" $true
	}

	It "Sensor Display" {
		#if we're going to use Channel as the type for PrimaryChannel, we need to be able to handle the case where the primary channel is downtime

		$sensor = Get-Sensor -Id (Settings UpSensor)

		#SetValue "PrimaryChannel" BLAH
		SetValue "GraphType" "Stacked"
		#SetValue "StackUnit" BLAH #how do we get a list of valid units? what happens if you set an invalid one? maybe do channel.unit?
	}

	It "Scanning Interval" {

		$sensor = Get-Sensor -Id (Settings UpSensor)

		SetValue "InheritInterval"   $true
		SetChild "Interval"          "00:01:00"         "InheritInterval" $false
		SetChild "IntervalErrorMode" OneWarningThenDown "InheritInterval" $false
	}

	It "Schedules, Dependencies and Maintenance Window" {
		$sensor = Get-Sensor -Id (Settings PausedByDependencySensor)

		SetValue "InheritDependency" $true
		#SetChild "Schedule" BLAH
		#SetChild "MaintenanceEnabled" $true
		#SetChild MaintenanceStart
		#SetChild MaintenanceEnd
		#SetChild DependencyType Object #todo: will this not work if i havent also specified the object to use at the same time?
		#should we maybe create a dependency attribute between the two? and would the same be true vice versa? (so when you set it to master,
		#the dependencyvalue goes away? check how its meant to work with fiddler)
		#SetChild Dependency (Settings DownSensor)
		#SetChild DependencyDelay 3
	}

	It "Access Rights" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		SetValue "InheritAccess" $false
		#todo: actually modifying access rights
	}

	It "Debug Options" {
		$sensor = Get-Sensor -Id (Settings WarningSensor)

		SetValue "DebugMode" "WriteToDisk"
	}

	It "WMI Alternative Query" {
		$sensor = Get-Sensor -Id (Settings WarningSensor)

		SetValue "WmiMode" "Alternative"
	}

	It "WMI Remote Ping Configuration" {
		$sensor = Get-Sensor -Id (Settings WmiRemotePing)

		SetValue "Target" "8.8.8.8"
		SetValue "Timeout" "200"
		SetValue "PingRemotePacketSize" "1000"
	}

	It "HTTP Specific" {
		$sensor = Get-Sensor -Id (Settings UpSensor)

		SetValue "Timeout" "500"
		SetValue "Url" "https://"
		SetValue "HttpRequestMethod" "POST"
		#GetValue "SNI" "blah"
		SetValue "UseSNIFromUrl" $true
	}

	It "Sensor Settings (EXE/XML)" {
		$sensor = Get-Sensor -Id (Settings ExeXml)

		#GetValue "ExeName" "blah"
		SetValue "ExeParameters" "test parameters `"with test quotes`""
		SetValue "SetEnvironmentVariables" $true
		SetValue "UseWindowsAuthentication" $true
		SetValue "Mutex" "Mutex1"
		SetValue "EnableChangeTriggers" $true
		#GetValue "ExeValueType" #todo: need to test that setting readonly on exevaluetype still retrieves the value
		SetValue "DebugMode" "WriteToDiskWhenError"
	}

	It "todo" {

	###timeout
		#timeout has a maximum value which is different depending on the context. what happens if you specify more than it. does the response give a clue that it failed?

	###settings
		#todo: do it on devices, etc as well. we need to change our ObjectProperty xmldoc refs to refer to the base class <- wtf is this about. maybe check a backup of this document to see what text was above it

		#add a link to about_sensorsettings on setobjectproperty and getobjectproperty, and we also need to have links to about_the other types
		#and then we also need to link from these about pages back to get- and set- objectproperty

		#add a todo item to go through the device, group and probe settings and rename each setting properly. also, are these types internal right now?

		#complete documentation of getobjectproperty and setobjectproperty

		#document all sensorsettings, enums and objectsettings

	###miscellaneous
		#maybe we should make priority a fancy class? then we can say tostring is numeric
		##todo: pretty much ALL PrtgClient methods need to validate their inputs arent null or empty

	###setchannelproperty
		#what happens if you try and pass null or string.emtpy to setobjectproperty (normal or channel). it should fail! (but maybe some values DO consider nothing an empty value
		#setobjectproperty is now sorted...need to check how it works in setchannelproperty
		#we should probably implement ALL the same sorts of tests we have on set-objectproperty for set-channelproperty
	    #need to list what valid enum values are, just like setobjectproperty doesrcal

	###progress
		#todo: maybe we SHOULD show progress when theres a cmdlet in the midde. e.g. $a|get-channel 'free bytes'|where lowererrorlimit -ne $null|set-channellproperty lowerwarninglimit $null
				
		throw "need to rewrite the get/set object settings bit of the readme"
	}
}