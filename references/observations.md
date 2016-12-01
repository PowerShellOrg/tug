# Observations of the xDscWebService PullServer (v2)

Here are some observations that I found when testing against a running xDscWebService PullServer

## References

Here are some useful site/doc references:
* Configuring LCM:
  * https://msdn.microsoft.com/en-us/powershell/dsc/metaconfig
* Setting up a pull client using configuration names:
  * https://msdn.microsoft.com/en-us/powershell/dsc/pullclientconfignames
* Setting up a pull client using configuration ID:
  * https://msdn.microsoft.com/en-us/powershell/dsc/pullclientconfigid

## Observations

* When using ConfigurationNames:
  * you *must* use RegistrationKeys (since Config Names are guessable)
  * you *should* specify at least one `ConfigurationName` when configuring the LCM,
    otherwise the pull server always shows the node is update-to-date (i.e. `OK`)
  * if you use multiple `ConfigurationName`s then you *must* also specify
    `PartialConfiguration` blocks in the configs

* When using ConfigurationID:
  * LCM will issue v1.x calls to the PullServer even though it will claim
    `ProtocolVersion` = 2.0 in the request headers
  * There is no complement to the v2 `RegisterDscAcgent` in v1.x setup
    * When issuing `Set-DscLocalConfigurationManager` to enable local LCM config
      for a v1 (ConfigurationID) setup, there is no inial call from node to server
    * When issuing `Set-DscLocalConfigurationManager` to enable local LCM config
      for a v2 (ConfigurationNames) setup, the node issues a `RegisterDscAgent`
      call to the server and provides the list of config names as well as a bunch
      of node meta data (IP Addresses (all), hostname, and node certificate)

