# README - `TugDSC.Client.CLIApp-tests`

Please note, this tests project is ***not*** a collection of Unit Tests -- it is
a collection of Integration Tests that use a unit testing framework (MSTest).

This project contains a series of tests that exercise the TugDSC Client library
against a target DSC Pull Mode Server, such as the *classic* DSC Pull Server v2
which would verify its client-side behavior and interoperability with a DSC
pull server.

Alternatively, it can be run against a new DSC Pull Server, such as TugDSC to
verify the server-side behavior of the target pull server with a typical DSC
client.

We do both in order to verify protocol interoperability.  First we verify the
TugDSC client behaves as expected, then we use it to test against the TugDSC
server to verify that it behaves as expected.

This test project supports several configuration settings that drive how some
of the tests operate and how they perform certain validations:

* `agent_id` - can be used to override the Agent ID that is used by the tests;
  the default is to just use this static, predictable ID all the time:
  `12345678-0000-0000-0000-000000000001`.

* `server_url` - specifies the target DSC Pull Server endpoint URL; this is
  how we specify which server will be interoperated against; the default is
  `http://DSC-SERVER1.tugnet:8080/PSDSCPullServer.svc/` which *assumes* that
  a DSN name of `DSC-SERVER1.tugnet` will resolve to the target server *and*
  that the full server endpoint includes the `PSDCSPullServer.svc` path
  component which is typical for the *classic* DSC Pull Server v2.

* `reg_key` - the registration key that will be used to authorize access
  to the target DSC Pull Server by the running test client.  This does have
  a default value which is a pre-defined, static value that is compatible with
  various integration test scenarios defined by this project.

* `proxy_url` - an optional URL endpoint that should be used as a WebProxy;
  their is no default value, which means proxying is disabled.  You can use
  this to go through a proxy service in a controlled network, or using a tool
  such as Fiddler which allows you to inspect the traffic for troubleshooting
  or analysis.

* `adjust_for_wmf_50` - a boolean flag which defaults to `true` which is used
  to control various behaviors and validations when interoperating with the
  DSC Pull Service that ships with WMF50 vs WMF51.  This server changes some of
  the expected results, such as expected datetime format strings and disabling
  support for reporting with *additional data*.  More details can be found at:
  [GitHub PowerShell #2921](https://github.com/PowerShell/PowerShell/issues/2921).

## Running Tests with Settings

When running the integration tests, you can use the standard `dotnet test` invocation
and it will run the tests with the above configuration settings set to their defaults.

You can adjust any of those settings by defining environment variables of the form:

> `TSTCFG_`*<config_name>*=*<config_value>*

For example, to define a specific target DSC Pull Server endpoint you would define
the following environment variable:

```powershell
    TSTCFG_server_url=https://my-dsc-server.example.com/MyDSCPath.svc/
```

### Running Tests with Test Running Script: `run-test.ps1`

Alternatively, you can use the test-running script `run-test.ps` which is a PS6-compatible
script (*should* be runnable on non-Windows environments under PS Core) and allows you to
specify any of the above configuration settings a CLI switches.  Additionally, it also
supports some pre-defined configuration combinations for various test scenarios by using
the `-TestRunConfig` parameter and one of the pre-defined parameter arguments.

Use the typical Get-Help path/to/run-test.ps1 to get details on CLI usage.

You can cycle through the list of predefined configuration combinations as per usual.
