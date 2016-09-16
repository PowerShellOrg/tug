# Tug Cmdlets
As outlined in the ReadMe, Tug only acts as a "web interface" between DSC nodes and your own PowerShell commands. Tug's functionality is therefore dependent on the commands you provide.

## Register Nodes
Nodes register at the start of each consistency check to authorize themselves to the pull server. The cmdket should:

```
Set-TugNodeRegistration   -AgentId <string> 
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

Tug will only call this command if the node passes a valid Authorization header. Notice that Tug itself does not use the certificate informartion passed by the node. You, however, may wish to do so. For example, after initial authorization, Azure Automation's Pull Server does rely on the client certificate information - this is handled by their Web server, not the pull server code per se.

This command isn't expected to return any output.

## Authorization Support

```
Get-TugRegistrationKeys [-AgentId <string>]
```

This command must return a collection of strings (e.g., like the output of Get-Content), each of which should be a "shared secret" registration key for your server. The AgentId parameter is optional; if included, you have the ability to return only the registration key that you have somehow associated with that node. 

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

Pass the updated JSON object (as text) to the pipeline as the output of the command.

## Requesting a Configuration or a Module
These operations are logically similar.

```
Get-TugConfigurationMOF -AgentId <string>
					    -ConfigurationName <string>
					    
Get-TugResourceModule -AgentId <string>
					  -ModuleName <string>
					  -ModuleVersion <string>
```

In both cases, your command must return an encoded octet-stream of the file data. If you are unable to return the requested data (e.g., a module does not exist), return nothing.

## Submitting a Report
Nodes configured to use a Report Server can submit reports to Tug.

```
Save-TugNodeReport -AgentId <string>
				   -ReportJSON <JSON>
```

Review Microsoft documentation regarding the structure of the ReportJSON. Tug will pass it as-is from the node, performing no interpretation or validation. You may choose to store the raw JSON, or you may choose to break it down and store it in some form of structured database for reporting purposes. It's entirely up to you. Consult the "troubleshooting and debugging" excerpt, in the References folder of the Tug project, for some details on what the reporting JSON object may contain.

It's important to understand that the JSON passed to this cmdlet is not fully standardized. That is, it will differ a bit between nodes, due to the pecifics of their configurations. It can also include error information. The native pull server stores errors apart from report data, so you will need to decide how to handle that information. For example, you might choose to email someone about errors, or even to make that a configurable part of your module.

This command is not expected to produce any output.