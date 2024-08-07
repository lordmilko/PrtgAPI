#
# Module manifest for module 'PrtgAPI'
#
# Generated by: lordmilko
#
# Generated on: 13/11/2016
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'PrtgAPI.PowerShell.dll'

# Version number of this module.
ModuleVersion = '0.9.20'

# Supported PSEditions
CompatiblePSEditions = @('Desktop', 'Core')

# ID used to uniquely identify this module
GUID = '81d4380f-31ff-42c7-9d64-1678dc5cd978'

# Author of this module
Author = 'lordmilko'

# Company or vendor of this module
CompanyName = 'Unknown'

# Copyright statement for this module
Copyright = '(c) 2015 lordmilko. All rights reserved.'

# Description of the functionality provided by this module
Description = 'C#/PowerShell interface for PRTG Network Monitor'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.1'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
TypesToProcess = @('PrtgAPI.Types.ps1xml')

# Format files (.ps1xml) to be loaded when importing this module
FormatsToProcess = @('PrtgAPI.Format.ps1xml')

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = @('PrtgAPI.psm1')

# Functions to export from this module
FunctionsToExport = '*'

# Cmdlets to export from this module
CmdletsToExport = '*'

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module
AliasesToExport = '*'

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

        # Tags applied to this module. These help with module discovery in online galleries.
        Tags = @('PSModule','PSEdition_Desktop','PSEdition_Core','Windows','Linux','MacOS','Prtg','Sensor','Device','Group','Probe','Channel','Notification','Action','Trigger','Remove','Pause','Resume','Check','Acknowledge','PowerShell','Setting','Property')

        # A URL to the license for this module.
        LicenseUri = 'https://raw.githubusercontent.com/lordmilko/PrtgAPI/master/LICENSE'

        # A URL to the main website for this project.
        ProjectUri = 'https://github.com/lordmilko/PrtgAPI'

        # A URL to an icon representing this module.
        # IconUri = ''

        # ReleaseNotes of this module
        ReleaseNotes = 'Release Notes: https://github.com/lordmilko/PrtgAPI/releases/tag/v0.9.20

---

PrtgAPI is a C#/PowerShell library that abstracts away the complexity of interfacing with the PRTG Network Monitor HTTP API.

PrtgAPI implements comprehensive type system providing an extensive array of functionality, enabling you to develop powerful applications for managing your network.

All cmdlets in PrtgAPI support some level of piping, allowing you to directly chain multiple cmdlets together to develop powerful one liners.

Operations supported by PrtgAPI include enumerating channels, sensors, devices, groups and probes, modifying object properties, creating brand new sensors, devices, groups and notification triggers, pausing, unpausing and acknowledging objects, renaming and removing items, and more.

PrtgAPI includes full Cmdlet Comment/XmlDoc documentation. Detailed information on any cmdlet can be found within PowerShell by running Get-Help <cmdlet> or Get-Help <cmdlet> -Full

For examples and usage scenarios, please see the Project Site.'

    } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
