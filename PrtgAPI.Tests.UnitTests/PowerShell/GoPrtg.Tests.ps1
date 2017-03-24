. $PSScriptRoot\Support\Init.ps1

function InstallInEmptyProfile($baseExpected)
{
	New-Item -ItemType File $Profile

	Install-GoPrtgAlias

	$content = gc $Profile -Raw

	$expected = $baseExpected

	$content | Should BeLike $expected

	{ GoPrtg } | Should Throw "Already connected to server prtg.example.com. To override please specify -Force"
}

function InstallInProfileWithContent($baseExpected, $multiLine)
{
	New-Item -ItemType File $Profile

	Add-Content $Profile "Write-Host 'test1'"

	if($multiLine)
	{
		Add-Content $Profile "Write-Host 'test2'"
	}

	Install-GoPrtgAlias

	$content = gc $Profile -Raw

	$expected = "Write-Host 'test1'`r`n$baseExpected"

	if($multiLine)
	{
		$expected = "Write-Host 'test1'`r`nWrite-Host 'test2'`r`n$baseExpected"
	}

	$content | Should BeLike $expected

	{ GoPrtg } | Should Throw "Already connected to server prtg.example.com. To override please specify -Force"
}

function GoPrtgBeforeAll
{
	if(!$Profile)
	{
		$Global:Profile = "$env:username\Documents\WindowsPowerShell\Microsoft.PowerShell_profile.ps1"
	}

	InitializeModules "PrtgAPI.Tests.UnitTests" $PSScriptRoot
}

function GoPrtgBeforeEach
{
	if(Test-Path $Profile)
	{
		if(Test-Path "$Profile.tmp")
		{
			throw "Cannot create temp profile; $Profile.tmp already exists"
		}
		else
		{
			mv $Profile "$Profile.tmp"
		}
	}

	if(Test-Path $Profile)
	{
		throw "Could not rename PowerShell Profile"
	}

	Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
}

function GoPrtgAfterEach
{
	Remove-Item $Profile -Force
	
	if(Test-Path "$Profile.tmp")
	{
		mv "$Profile.tmp" $Profile
	}

	Disconnect-PrtgServer
}

Describe "Install-GoPrtgAlias" {

	$baseExpected = "function __goPrtgConnectServer { Connect-PrtgServer prtg.example.com (New-Object System.Management.Automation.PSCredential -ArgumentList username, (ConvertTo-SecureString *)) -PassHash }`r`nNew-Alias GoPrtg __goPrtgConnectServer`r`n"

	BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

	It "installs correctly in new profile" {

		Install-GoPrtgAlias

		$content = gc $Profile -Raw

		$expected = $baseExpected

		$content | Should BeLike $expected

		{ GoPrtg } | Should Throw "Already connected to server prtg.example.com. To override please specify -Force"
	}

	It "installs correctly in empty profile" {
		InstallInEmptyProfile $baseExpected
	}

	It "installs correctly in profile with content and new line" {
		InstallInProfileWithContent $baseExpected $false
	}

	It "installs correctly in profile with content and no new line" {
		New-Item -ItemType File $Profile

		Add-Content $Profile "Write-Host 'test'" -NoNewline

		Install-GoPrtgAlias

		$content = gc $Profile -Raw

		$expected = "Write-Host 'test'`r`n$baseExpected"

		$content | Should BeLike $expected

		{ GoPrtg } | Should Throw "Already connected to server prtg.example.com. To override please specify -Force"
	}
}

Describe "Uninstall-GoPrtgAlias" {

	$baseExpected = "function __goPrtgConnectServer { Connect-PrtgServer prtg.example.com (New-Object System.Management.Automation.PSCredential -ArgumentList username, (ConvertTo-SecureString *)) -PassHash }`r`nNew-Alias GoPrtg __goPrtgConnectServer`r`n"

	BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

	It "uninstalls correctly in empty profile" {
		InstallInEmptyProfile $baseExpected

		Uninstall-GoPrtgAlias

		$content = gc $Profile -Raw

		$content | Should BeNullOrEmpty
	}

	It "uninstalls correctly in profile with single existing line" {
		InstallInProfileWithContent $baseExpected $false

		Uninstall-GoPrtgAlias

		$content = gc $Profile -Raw

		$content | Should Be "Write-Host 'test1'`r`n"
	}

	It "uninstalls correctly in profile with multiple existing lines" {
		InstallInProfileWithContent $baseExpected $true

		Uninstall-GoPrtgAlias

		$content = gc $Profile -Raw

		$content | Should Be "Write-Host 'test1'`r`nWrite-Host 'test2'`r`n"
	}

	It "uninstalls correctly in profile in between lines" {
		InstallInProfileWithContent $baseExpected $true

		Add-Content $Profile "Write-Host 'test3'"

		Uninstall-GoPrtgAlias

		$content = gc $Profile -Raw

		$content | Should Be "Write-Host 'test1'`r`nWrite-Host 'test2'`r`nWrite-Host 'test3'`r`n"
	}
}