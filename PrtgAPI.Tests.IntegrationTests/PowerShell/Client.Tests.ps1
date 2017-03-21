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
			Get-Sensor | Get-Channel
		}
		finally
		{
			$service.StartService()
		}

		#gwmi win32_service -ComputerName $server -Credential (New-Credential (Settings WindowsUsername) (Settings WindowsPassword))
	}
}