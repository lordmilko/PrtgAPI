. $PSScriptRoot\Support\Init.ps1

#region Support

function InstallInEmptyProfile($baseExpected)
{
    New-Item $Profile -Type File -Force

    Install-GoPrtgServer

	$content = gc $Profile -Raw

	$content | Should BeLike $baseExpected
}

function InstallInProfileWithContent($baseExpected, $multiLine)
{
    New-Item $Profile -Type File -Force

    Add-Content $Profile "Write-Host `"hello`""

    if($multiLine)
    {
		Add-Content $Profile "Write-Host `"what what?`""
    }

    Install-GoPrtgServer

	$content = gc $Profile -Raw

	$expected = "Write-Host `"hello`"`r`n$baseExpected"

    if($multiLine)
    {
		$expected = "Write-Host `"hello`"`r`nWrite-Host `"what what?`"`r`n$baseExpected"
    }

	$content | Should BeLike $expected
}

function InstallMultipleInProfile
{
    Install-GoPrtgServer

	try
	{
		Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

		Install-GoPrtgServer
	}
	finally
	{
		Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
	}

	$content = gc $Profile -Raw

	$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	$expected += "function __goPrtgGetServers {@(`r`n"
	$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",`r`n"
	$expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"`r`n"
	$expected += ")}`r`n`r`n"
	$expected += "############################ End GoPrtg Servers ############################`r`n"

	$expected = $expected.Replace("``", "````")

	$content | Should BeLike $expected
}

function InstallMultipleWithAlias
{
    Install-GoPrtgServer prod

	try
	{
		Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

		Install-GoPrtgServer dev
	}
	finally
	{
		Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
	}

	$content = gc $Profile -Raw

	$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	$expected += "function __goPrtgGetServers {@(`r`n"
	$expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",`r`n"
	$expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"`r`n"
	$expected += ")}`r`n`r`n"
	$expected += "############################ End GoPrtg Servers ############################`r`n"

	$expected = $expected.Replace("``", "````")

	$content | Should BeLike $expected
}

#endregion
#region Init

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

	if(Get-Command __goPrtgGetServers -ErrorAction SilentlyContinue)
	{
		Remove-Item Function:\__goPrtgGetServers
	}

	if(Test-Path $Profile)
	{
		throw "Could not rename PowerShell Profile"
	}

	Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
}

function GoPrtgAfterEach
{
	if(Test-Path $Profile)
	{
		Remove-Item $Profile -Force
	}

	if(Test-Path "$Profile.tmp")
	{
		mv "$Profile.tmp" $Profile
	}

	Disconnect-PrtgServer
}

#endregion

Describe "Install-GoPrtgServer" {

    $baseExpected = ("########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n" + 
                    "function __goPrtgGetServers {@(`r`n    `"```"prtg.example.com```",,```"username```",```"*```"`"`r`n)}`r`n`r`n" + 
                    "############################ End GoPrtg Servers ############################`r`n").Replace("``","````")

	BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

	It "installs correctly in new profile" {
		Install-GoPrtgServer

		$content = gc $Profile -Raw

		$content | Should BeLike $baseExpected
	}

    It "installs correctly in existing empty profile" {
        InstallInEmptyProfile $baseExpected
    }

    It "installs correctly in profile with content in it" {
        InstallInProfileWithContent $baseExpected
    }

	It "installs correctly in profile with content in it without trailing newline" {
		Add-Content $Profile "Write-Host `"hello`"" -NoNewline

		Install-GoPrtgServer

		$expected = "Write-Host `"hello`"`r`n$baseExpected"

		$content = gc $Profile -Raw

		$content | Should BeLike $expected
	}

	It "installs multiple servers in new profile" {
		InstallMultipleInProfile
	}

    It "installs multiple servers in existing empty profile" {
		New-Item $Profile -Type File -Force

        InstallMultipleInProfile
    }

    It "installs multiple servers correctly in profile with content in it" {

        New-Item $Profile -Type File -Force

        Add-Content $Profile "Write-Host `"hello`""

        Install-GoPrtgServer

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		$content = gc $Profile -Raw

        $expected = "Write-Host `"hello`"`r`n"
		$expected += "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",`r`n"
		$expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
    }

    It "installs multiple servers correctly in profile adding content, server, content, then another server" {

        New-Item $Profile -Type File -Force

        Add-Content $Profile "Write-Host `"hello`""

        Install-GoPrtgServer

        Add-Content $Profile "Write-Host `"goodbye`""

        try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

        $content = gc $Profile -Raw

        $expected = "Write-Host `"hello`"`r`n"
		$expected += "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",`r`n"
		$expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"
        $expected += "Write-Host `"goodbye`"`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
    }

    It "installs with alias" {
        Install-GoPrtgServer prod

	    $content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	    $expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

	    $content | Should BeLike $expected
    }

	It "installs with a null alias" {
		Install-GoPrtgServer $null

	    $content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	    $expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

	    $content | Should BeLike $expected
	}

	It "installs with an empty string alias" {
		Install-GoPrtgServer ""

	    $content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	    $expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

	    $content | Should BeLike $expected
	}

    It "installs multiple with alias" {
        InstallMultipleWithAlias
    }

    It "installs multiple entries for a server with different usernames and aliases" {
		Install-GoPrtgServer prod

		try
	    {
		    Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

		    Install-GoPrtgServer dev
	    }
	    finally
	    {
		    Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
	    }

		$contents = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",`r`n"
		$expected += "    `"```"prtg.example.com```",```"dev```",```"username2```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$contents | Should BeLike $expected
    }

	It "throws when username for server exists already" {

		$client = Get-PrtgClient

		Install-GoPrtgServer
		
		{ Install-GoPrtgServer } | Should Throw "Cannot add server '$($client.Server)': a record for user '$($client.UserName)' already exists."
	}

	#todo: update-goprtgserver should allow specifying the server to update explicitly

	It "throws when server exists with alias but new record for same server is missing alias" {

		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

			$client = Get-PrtgClient

			{ Install-GoPrtgServer } | Should Throw "Cannot add server '$($client.Server)': an alias must be specified to differentiate this connection from an existing connection with the same server address."
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}
	}

	It "throws when server exists already without alias on existing record" {

		Install-GoPrtgServer

		try
		{
			Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

			$client = Get-PrtgClient

			{ Install-GoPrtgServer } | Should Throw "Cannot add server '$($client.Server)': a record for server already exists without an alias. Please update the alias of this record with Set-GoPrtgAlias and try again."
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		$client = Get-PrtgClient
	}

	It "throws when alias already exists" {
		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			{ Install-GoPrtgServer prod } | Should Throw "Cannot add server 'prtg.example2.com' with alias 'prod': a record for this alias already exists. For more information see 'Get-GoPrtgServer prod'"
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}
	}

	It "throws installing a duplicate username/server combination even with a different alias" {
		Install-GoPrtgServer prod
		{ Install-GoPrtgServer dev } | Should Throw "Cannot add server 'prtg.example.com': a record for user 'username' already exists. To update the alias of this record use Set-GoPrtgAlias. To reinstall this record, first uninstall with Uninstall-GoPrtgServer and then re-run Install-GoPrtgServer."
	}

	It "throws when getServers function is missing" {
		Install-GoPrtgServer

		# Simulate the function being missing from the file

		Remove-Item Function:\__goPrtgGetServers

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			{ Install-GoPrtgServer dev } | Should Throw "GoPrtg header and footer are present in PowerShell profile, however __goPrtgGetServers function was not loaded into the current session. Please verify the function has not been corrupted or remove the GoPrtg header and footer and re-run Install-GoPrtgServer."
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}
	}

	It "throws when GoPrtg start block is missing" {
		Install-GoPrtgServer

        $contents = gc $Profile

		$newContents = $contents | where {$_ -ne "########################### Start GoPrtg Servers ###########################"}

		Set-Content $Profile $newContents

		{ Install-GoPrtgServer } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
	}

	It "throws when GoPrtg end block is missing" {
        Install-GoPrtgServer

        $contents = gc $Profile

		$newContents = $contents | where {$_ -ne "############################ End GoPrtg Servers ############################"}

		Set-Content $Profile $newContents

		{ Install-GoPrtgServer } | Should Throw "GoPrtg Servers end line '############################ End GoPrtg Servers ############################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
	}
}

Describe "Uninstall-GoPrtgServer" {

    $baseExpected = ("########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n" +
					"function __goPrtgGetServers {@(`r`n    `"```"prtg.example.com```",,```"username```",```"*```"`"`r`n)}`r`n" +
					"`r`n############################ End GoPrtg Servers ############################`r`n").Replace("``", "````")

    BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

    It "uninstalls correctly in empty profile" {
        InstallInEmptyProfile $baseExpected

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should BeNullOrEmpty
    }

    It "uninstalls correctly in profile with single existing line" {
        InstallInProfileWithContent $baseExpected $false

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

		$content | Should Be "Write-Host `"hello`"`r`n"
    }

    It "uninstalls correctly in profile with multiple existing lines" {
        InstallInProfileWithContent $baseExpected $true

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

		$content | Should Be "Write-Host `"hello`"`r`nWrite-Host `"`what what?`"`r`n"
    }

    It "uninstalls correctly in profile between lines" {
        InstallInProfileWithContent $baseExpected $true

        Add-Content $Profile "Write-Host `"test1`""

        Uninstall-GoPrtgServer

        $content = gc $Profile -Raw

        $content | Should Be "Write-Host `"hello`"`r`nWrite-Host `"`what what?`"`r`nWrite-Host `"test1`"`r`n"
    }

    It "uninstalls single entry when server specified" {
        InstallMultipleInProfile

        Uninstall-GoPrtgServer prtg.example.com

        $content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

        $content | Should BeLike $expected
    }

    It "uninstalls matching entries when wildcard server specified" {
        
        Install-GoPrtgServer

        try
	    {
		    Connect-PrtgServer prtg3.example.com (New-Credential username3 12345678) -PassHash -Force

		    Install-GoPrtgServer

            Connect-PrtgServer prtg.example2.com (New-Credential username2 87654321) -PassHash -Force

		    Install-GoPrtgServer
	    }
	    finally
	    {
		    Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
	    }

        $content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	    $expected += "function __goPrtgGetServers {@(`r`n"
	    $expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`",`r`n"
	    $expected += "    `"```"prtg3.example.com```",,```"username3```",```"*```"`",`r`n"
        $expected += "    `"```"prtg.example2.com```",,```"username2```",```"*```"`"`r`n"
	    $expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

	    $content | Should BeLike $expected

        Uninstall-GoPrtgServer prtg.*

        $finalContent = gc $Profile -Raw

		$finalExpected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
	    $finalExpected += "function __goPrtgGetServers {@(`r`n"
        $finalExpected += "    `"```"prtg3.example.com```",,```"username3```",```"*```"`"`r`n"
	    $finalExpected += ")}`r`n`r`n"
		$finalExpected += "############################ End GoPrtg Servers ############################`r`n"

		$finalExpected = $finalExpected.Replace("``", "````")

	    $finalContent | Should BeLike $finalExpected
    }

    It "uninstalls everything when last wildcard server specified" {
        Install-GoPrtgServer

        Uninstall-GoPrtgServer prtg.*

        $contents = gc $Profile -Raw

        $contents | Should BeNullOrEmpty
    }
	
    It "uninstalls when alias specified" {
        InstallMultipleWithAlias

        Uninstall-GoPrtgServer prod

        $contents = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
        $expected += "function __goPrtgGetServers {@(`r`n"
	    $expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"`r`n"
	    $expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

        $contents | Should BeLike $expected
    }

    It "uninstalls all when -Force specified" {

		InstallMultipleWithAlias

		Uninstall-GoPrtgServer -Force

		$contents = gc $Profile -Raw

		$contents | Should BeNullOrEmpty
    }

	It "removes getServers function from global scope" {
		Install-GoPrtgServer

		$servers = __goPrtgGetServers
		$servers | Should Not BeNullOrEmpty

		Uninstall-GoPrtgServer

		{ __goPrtgGetServers } | Should Throw "The term '__goPrtgGetServers' is not recognized as the name of a cmdlet"
	}

	It "removes the global function" {
		Install-GoPrtgServer
		
		Uninstall-GoPrtgServer

		{ __goPrtgGetServers } | Should Throw "The term '__goPrtgGetServers' is not recognized as the name of a cmdlet"
	}

	It "updates the global function" {
		Install-GoPrtgServer

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Uninstall-GoPrtgServer prtg.example.com

		Get-GoPrtgServer | Should Be "@{[!]=[ ]; Server=prtg.example2.com; Alias=; UserName=username2}"
	}

    It "throws when server specified that doesn't exist" {
        InstallMultipleInProfile

        { Uninstall-GoPrtgServer banana } | Should Throw "'banana' is not a valid server name or alias. To view all saved servers, run Get-GoPrtgServer"
    }

    It "throws uninstalling when not installed" {
        New-Item -Type File $Profile -Force
        { Uninstall-GoPrtgServer } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer"
    }

	It "throws uninstalling when profile doesn't exist" {
		{ Uninstall-GoPrtgServer } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
	}

    It "throws uninstalling with multiple entries and server not specified" {
		InstallMultipleInProfile

		{ Uninstall-GoPrtgServer } | Should Throw "Cannot remove servers; server name or alias must be specified when multiple entries exist. To remove all servers, specify -Force"
    }

	It "throws when getServers function is missing" {
		Install-GoPrtgServer

		Remove-Item Function:\__goPrtgGetServers

		{ Uninstall-GoPrtgServer } | Should Throw "GoPrtg header and footer are present in PowerShell profile, however __goPrtgGetServers function was not loaded into the current session. Please verify the function has not been corrupted or remove the GoPrtg header and footer and re-run Install-GoPrtgServer."
	}

	It "throws when GoPrtg start block is missing" {
		Install-GoPrtgServer

		$contents = gc $Profile

		$newContents = $contents | where {$_ -ne "########################### Start GoPrtg Servers ###########################"}

		Set-Content $Profile $newContents

		{ Uninstall-GoPrtgServer } | Should Throw "GoPrtg Servers start line '########################### Start GoPrtg Servers ###########################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
	}

	It "throws when GoPrtg end block is missing" {
		Install-GoPrtgServer

		$contents = gc $Profile

		$newContents = $contents | where {$_ -ne "############################ End GoPrtg Servers ############################"}

		Set-Content $Profile $newContents

		{ Uninstall-GoPrtgServer } | Should Throw "GoPrtg Servers end line '############################ End GoPrtg Servers ############################' has been removed from PowerShell profile. Please reinstate line or remove all lines pertaining to GoPrtg from your profile."
	}
}

Describe "Get-GoPrtgServer" {

	BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

	It "can retrieve servers" {
		#todo: allow specifying the servers you want to get-goprtgserver, alias or server wildcard

		Install-GoPrtgServer dev

		$servers = Get-GoPrtgServer

		$servers | Should Not BeNullOrEmpty

		$servers.Server | Should Be prtg.example.com
		$servers.Alias | Should Be dev
		$servers.UserName | Should Be username
	}

	It "sets empty aliases to null" {
		Install-GoPrtgServer

		$server = Get-GoPrtgServer

		$server.Alias | Should Be $null
	}

	It "can retrieve servers by server name" {
		InstallMultipleInProfile

		$server = Get-GoPrtgServer prtg.example2.com

		$server.Server | Should Be prtg.example2.com
	}

	It "can retrieve servers by server wildcard" {
		InstallMultipleInProfile

		try
		{
			Connect-PrtgServer prtg2.example.com (New-Credential username3 12345678) -PassHash -Force

			Install-GoPrtgServer
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		$servers = Get-GoPrtgServer prtg.*

		$servers.Count | Should Be 2
		$servers[0].Server | Should Be prtg.example.com
		$servers[1].Server | Should Be prtg.example2.com
	}

	It "can retrieve servers by alias" {
		InstallMultipleWithAlias

		$server = Get-GoPrtgServer dev

		$server.Server | Should Be prtg.example2.com
		$server.Alias | Should Be dev
	}

	It "warns when GoPrtg isn't installed" {
		Get-GoPrtgServer | Should Be "`nGoPrtg is not installed. Run Install-GoPrtgServer first to install a GoPrtg server.`n"
	}

	It "warns when profile doesn't exist" {
		Get-GoPrtgServer | Should Be "`nGoPrtg is not installed. Run Install-GoPrtgServer first to install a GoPrtg server.`n"
	}
}

Describe "Connect-GoPrtgServer" {

	BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

	It "can connect to default server" {
		Install-GoPrtgServer

		Disconnect-PrtgServer

		Connect-GoPrtgServer | Should Be "`nConnected to prtg.example.com as username`n"
	}

	It "can connect to alias when single server exists" {
		Install-GoPrtgServer test

		Disconnect-PrtgServer

		Connect-GoPrtgServer test | Should Be "`nConnected to prtg.example.com as username`n"
	}

	It "can connect to specified server when single server exists" {
		Install-GoPrtgServer

		Disconnect-PrtgServer

		Connect-GoPrtgServer prtg.example.com | Should Be "`nConnected to prtg.example.com as username`n"
	}

	It "can connected to specified server when multiple servers exist" {
		Install-GoPrtgServer

		Disconnect-PrtgServer

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Connect-GoPrtgServer prtg.example2.com | Should Be "`nConnected to prtg.example2.com as username2`n"
	}

	It "can connect to specified alias when multiple servers exist" {
		Install-GoPrtgServer test1

		Disconnect-PrtgServer

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer test2
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Disconnect-PrtgServer

		Connect-GoPrtgServer test1 | Should Be "`nConnected to prtg.example.com as username`n"

		Disconnect-PrtgServer

		Connect-GoPrtgServer test2 | Should Be "`nConnected to prtg.example2.com as username2`n"
	}

	It "authenticates with the original server details" {
		Install-GoPrtgServer

		Disconnect-PrtgServer

		Connect-GoPrtgServer

		$client = Get-PrtgClient

		$client.Server | Should Be "prtg.example.com"
		$client.UserName | Should Be "username"
		$client.PassHash | Should Be "passhash"
	}

	It "warns when server is already connected" {
        Install-GoPrtgServer

        Connect-GoPrtgServer | Should Be "`nAlready connected to prtg.example.com as username`n"
	}

	It "warns when GoPrtg is not installed" {
		New-Item $Profile -Type File -Force

		Connect-GoPrtgServer | Should Be "`nNo GoPrtg servers are installed. Please install a server first using Install-GoPrtgServer`n"
	}

	It "warns when profile doesn't exist" {
		Connect-GoPrtgServer | Should Be "`nNo GoPrtg servers are installed. Please install a server first using Install-GoPrtgServer`n"
	}

	It "throws when multiple servers match the server expression" {
        Install-GoPrtgServer

		Disconnect-PrtgServer

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer

			Connect-PrtgServer test.example3.com (New-Credential username3 12345678) -PassHash -Force

			Install-GoPrtgServer
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

        $response = Connect-GoPrtgServer p*

        $response | Should Be @(
            "`nAmbiguous server specified. The following servers matched the specified server name or alias",
            "@{[!]=[*]; Server=prtg.example.com; Alias=; UserName=username}",
            "@{[!]=[ ]; Server=prtg.example2.com; Alias=; UserName=username2}"
        )
	}
}

Describe "Set-GoPrtgAlias" {
	BeforeAll { GoPrtgBeforeAll	}

	BeforeEach { GoPrtgBeforeEach }
	AfterEach { GoPrtgAfterEach }

	It "sets an alias on a record that doesn't have one" {
		Install-GoPrtgServer

		Set-GoPrtgAlias test

		$content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "sets an alias on a record that does have one" {
		Install-GoPrtgServer prod

		Set-GoPrtgAlias dev

		$content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"dev```",```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "sets an alias on the first server when multiple servers are installed" {
		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer dev
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Disconnect-PrtgServer

		Connect-GoPrtgServer prod

		Set-GoPrtgAlias test

		$content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`",`r`n"
		$expected += "    `"```"prtg.example2.com```",```"dev```",```"username2```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "sets an alias on the last server when multiple servers are installed" {
		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer dev
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Disconnect-PrtgServer

		Connect-GoPrtgServer dev

		Set-GoPrtgAlias test

		$content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"prod```",```"username```",```"*```"`",`r`n"
		$expected += "    `"```"prtg.example2.com```",```"test```",```"username2```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "sets an alias when profile has content before function" {
		Add-Content $Profile "Write-Host `"hello`""

		Install-GoPrtgServer prod

		Set-GoPrtgAlias test

		$content = gc $Profile -Raw

		$expected = "Write-Host `"hello`"`r`n"
		$expected += "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "sets an alias when profile has content before and after function" {
		Add-Content $Profile "Write-Host `"hello`""

		Install-GoPrtgServer prod

		Add-Content $Profile "Write-Host `"goodbye`""

		Set-GoPrtgAlias test

		$content = gc $Profile -Raw

		$expected = "Write-Host `"hello`"`r`n"
		$expected += "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",```"test```",```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"
		$expected += "Write-Host `"goodbye`"`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "removes an alias on a record that has one" {
		Install-GoPrtgServer prod

		Set-GoPrtgAlias $null

		$content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "removes an alias on a record that doesn't have one" {
		Install-GoPrtgServer

		Set-GoPrtgAlias $null

		$content = gc $Profile -Raw

		$expected = "########################### Start GoPrtg Servers ###########################`r`nImport-Module PrtgAPI`r`n"
		$expected += "function __goPrtgGetServers {@(`r`n"
		$expected += "    `"```"prtg.example.com```",,```"username```",```"*```"`"`r`n"
		$expected += ")}`r`n`r`n"
		$expected += "############################ End GoPrtg Servers ############################`r`n"

		$expected = $expected.Replace("``", "````")

		$content | Should BeLike $expected
	}

	It "updates the global function" {
		Install-GoPrtgServer prod
		Set-GoPrtgAlias dev

		Get-GoPrtgServer dev | Should Be "@{[!]=[*]; Server=prtg.example.com; Alias=dev; UserName=username}"
	}

	It "throws setting a duplicate alias" {
		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer dev
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Connect-GoPrtgServer dev

		{ Set-GoPrtgAlias prod } | Should Throw "Cannot set alias for server 'prtg.example2.com': a record with alias 'prod' already exists. For more information see Get-GoPrtgServer."
	}

	It "throws removing an alias on a duplicate server" {
		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example.com (New-Credential username2 12345678) -PassHash -Force

			Install-GoPrtgServer dev
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}

		Connect-GoPrtgServer dev

		{ Set-GoPrtgAlias } | Should Throw "Cannot remove alias of server: multiple entries for server 'prtg.example.com' are stored within GoPrtg. To remove this alias uninstall all other entries for this server. For more information see Get-GoPrtgServer."
	}

	It "throws when not connected to a PRTG Server" {
		Install-GoPrtgServer

		Disconnect-PrtgServer

		{ Set-GoPrtgAlias } | Should Throw "You are not connected to a PRTG Server. Please connect first using GoPrtg [<server>]."
	}

	It "throws when GoPrtg is not installed" {
		New-Item $Profile -Type File -Force

		{ Set-GoPrtgAlias } | Should Throw "GoPrtg is not installed. Run Install-GoPrtgServer <alias> to install a server with the specified alias."
	}

	It "throws when profile doesn't exist" {
		{ Set-GoPrtgAlias } | Should Throw "No GoPrtg servers are installed. To install a GoPrtg server, run Install-GoPrtgServer."
	}

	It "throws when the connected PRTG Server is not a GoPrtg server" {
		Install-GoPrtgServer prod

		try
		{
			Connect-PrtgServer prtg.example2.com (New-Credential username2 12345678) -PassHash -Force

			{ Set-GoPrtgAlias } | Should Throw "Server 'prtg.example2.com' is not a valid GoPrtg server. To install this server, run Install-GoPrtgServer [<alias>]"
		}
		finally
		{
			Connect-PrtgServer prtg.example.com (New-Credential username passhash) -PassHash -Force
		}
	}

	<#
	it sets an alias on a record that doesnt have one
on one that does have one
sets an alias to nothing
prevents setting an alias to nothing in scenarios that would result in valid situations we normally
dont allow when doing install-goprtgserver <look those up>
throws when not connected to a prtg server
throws when the goprtgserver is not installed
throws when its been installed twice with the same name using different aliases
-should we maybe prevent that with install-goprtgserver even if they are using different aliases?
	#>
}