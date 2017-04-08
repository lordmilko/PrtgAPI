. $PSScriptRoot\Support\IntegrationTestSafe.ps1

Describe "IT_Connect-PrtgServer" {
	It "can retry request" {

		Connect-PrtgServer (Settings ServerWithProto) (New-Credential prtgadmin prtgadmin) -Force -RetryCount 3
		$server = (Settings Server)
		$credential = (New-Credential (Settings WindowsUsername) (Settings WindowsPassword))

		<#Invoke-Command -ComputerName $server -Credential (New-Credential (Settings WindowsUsername) (Settings WindowsPassword)) -ScriptBlock {
			Stop-Service "PRTGCoreService"
		}#>

		$service = gwmi win32_service -ComputerName $server -Credential $credential -filter "name='PRTGCoreService'"

		$service.StopService()

		try
		{
            $output = [string]::Join("`n",(&{try { Get-Sensor 3>&1 | %{$_.Message} } catch [exception] { }}))

            $expected = "'Get-Sensor' timed out: Unable to connect to the remote server. Retries remaining: 3`n" +
                "'Get-Sensor' timed out: Unable to connect to the remote server. Retries remaining: 2`n" +
                "'Get-Sensor' timed out: Unable to connect to the remote server. Retries remaining: 1"

			$output | Should Be $expected

            { Get-Sensor | Get-Channel } | Should Throw "Unable to connect to the remote server"
		}
		finally
		{
			$service.StartService()
            Sleep 20
		}

		#gwmi win32_service -ComputerName $server -Credential (New-Credential (Settings WindowsUsername) (Settings WindowsPassword))
	}
}