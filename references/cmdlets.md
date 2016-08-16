# Tug Cmdlets
As outlined in the ReadMe, Tug only acts as a "web interface" between DSC nodes and your own PowerShell commands. Tug's functionality is therefore dependent on the commands you provide.

## Authorize Nodes
Nodes register at the start of each consistency check to authorize themselves to the pull server.

```
Test-TugNodeAuthorization -AgentId <string> 
						  -SharedSecret <string> 
						  -ConfigurationNames <string[]>
						  -CertFriendlyName <string>
						  -CertIssuer <string>
						  -CertNotAfter <string>
						  -CertNotBefore <string>
						  -CertSubject <string>
						  -CertPublicKey <string>
						  -CertThumbprint <string>
						  -CertVersion <string>
```

The SharedSecret string will be the result of a SHA-256 cryptographic hash. This hash may have been created from:
* A predefined RegistrationKey, in the event of a new node registration
* A node's existing certificate, which you must maintain on file

You will therefore need to potentially check both sources. It is recommended that you first see if the AgentId is known to you, since that will be a quick lookup and a faster operation. If the AgentId is not known, then you will need to check any registration keys that you are configured to use.

This command must return either $True or $False. Note that:
* CertNotAfter and CertNotBefore will be strings, but should be cast and treated as dates.
* CertVersion will be a string, but should be cast and treated as an integer.
* If the agent is authorized, your command must persist the configuration names and certificate information. Even if the agent already has certificate information on file, you should update it with any new information provided.

Nominally, nodes register independently for pulling configurations and for reporting; Tug provides both sets of functionality. So your command should do whatever provisioning is needed to also receive reports from the node. 

Implementation notes:
* You will need to provide a means for the server operator to maintain registration keys, including creating new ones and deleting old ones. This can be as simple as a configuration file on disk at a predetermined location.

## Node Check-In
Nodes check-in at the start of each consistency check to see if they have the latest configuration (MOF) file. The nature of this check-in depends on whether the node is configured to use a single MOF, or to use multiple partial MOFs. In the case of a single-MOF check-in, you have the option to tell the node to retrieve an all-new configuration, and to then provide a configuration other than the one the node may have been expecting. This enables a server-based, centralized means of "feeding" a new MOF to a node, and the node will treat whatever it is given as authoritative.

In the case of a partial-MOF check-in, the node will actually indicate which configuration names it is expecting to receive. You still have the ability to tell the node that its current MOFs are outdated, and to force it to pull new ones. 

```
Get-TugNodeConfigurationStatus -AgentId <string>
							   -Configurations <JSON>
```

The -Configurations parameter will receive a JSON object. This will be structured as follows:

```
{
	Configurations: [
		{ ConfigurationName: "string",
		  Checksum: "string",
		  Status: "string" },
		{ ConfigurationName: "string",
		  Checksum: "string",
		  Status: "string" },
	]
}
```

The Configurations array may contain one or more child objects. Each child object will have a ConfigurationName property (which may be empty), a Checksum property (which may be empty), and a Status property (which will always be empty). In the event the ConfigurationName property is empty, there will always be only one child object. Your command must look up whatever configuration the node is known to be using (based on data provided during registration), and match the checksum of that MOF to the checksum provided. If they match, set the Status property to "Ok". If they do not match, or if for some other reason you want the node to pull a new MOF, set the Status property to "GetConfiguration".

If multiple child objects are included, then the ConfigurationName properties will contain the actual name of the configuration MOF that the node is checking on. Again, verify the checksums match and set status to "Ok", or set to "GetConfiguration" to force the node to re-pull that MOF. 

Pass the updated JSON object to the pipeline as the output of the command.

## Requesting a Configuration or a Module
These operations are logically similar.

```
Get-TugConfigurationMOF -AgentId <string>
					    -ConfigurationName <string>
					    
Get-TugResourceModule -AgentId <string>
					  -ModuleName <string>
					  -ModuleVersion <string>
```

In both cases, your commands should provide an absolute local file path for the content in question, which must either be a plain-text MOF file for configurations, or a properly named ZIP file for modules. Tug will return the data as an application/octet-stream to the node. Tug will not delete the files.

In the future, it is desired for Tug to be able to accept an octet-stream directly from your command, so that there is no need for the data to be written to disk.

## Submitting a Report
Nodes configured to use a Report Server can submit reports to Tug.

```
Save-TugNodeReport -AgentId <string>
				   -ReportJSON <JSON>
```

Review Microsoft documentation regarding the structure of the ReportJSON. Tug will pass it as-is from the node, performing no interpretation or validation. You may choose to store the raw JSON, or you may choose to break it down and store it in some form of structured database for reporting purposes. It's entirely up to you. Consult the "troubleshooting and debugging" excerpt, in the References folder of the Tug project, for some details on what the reporting JSON object may contain.