# FaaS Tug - AWS Lambda

This package defines an implementation of Tug DSC Pull Server using a Function-as-a-Service (FaaS)
model that runs atop [AWS Lambda](https://aws.amazon.com/lambda/).

## Why FaaS?

The argument for a *serverless* implementation of Tug DSC is actually quite simple.

For starters, in the DSC Pull Mode service model, the bulk of the intelligence is actually in
the DSC client (i.e. the LCM), and the server actually functions as a very simple storage access
service and mediator.  The server only needs to handle a handful of unique messages that deal with
node registration and then access to content (configuration and modules).

The typical load and request utilization for most environments is quite small, where each node can
only check-in with the server once every 30 minutes (restrictred by the valid range of the
`RefreshFrequencyMins` setting).  Only when there are actual changes to be pulled does the server
utilization ramp up, and only for a brief period.  So a *serverless* approach is actually more
efficient because it doesn't require deploying the DSC Pull service on dedicated host.

Finally, in an environment that is built from the ground up using an infrastructure-as-code
approach, deploying DSC as FaaS first means that all nodes, even critical basic infrastructure
services like Active Directory, WSUS, network services (firewall, routing, DNS, etc.), etc.
can be managed using DSC without worrying about build dependency ordering.

## Why AWS Lambda?

With the most recent enhancements to AWS's Lambda service, all the pieces are finally there to
support a FaaS Tug implementation atop Lambda.  These include:
* support for [binary responses](https://aws.amazon.com/blogs/compute/binary-support-for-api-integrations-with-amazon-api-gateway/)
* support for [C# and the .NET platform](https://aws.amazon.com/blogs/compute/announcing-c-sharp-support-for-aws-lambda/)
* a simplified deployment and management story for Lambda *and supporting pieces* in the form of [Serverless Application Model (SAM)](https://aws.amazon.com/blogs/compute/introducing-simplified-serverless-application-deplyoment-and-management/)
* enhancements to the integration between [Lambda and API Gateway](https://aws.amazon.com/blogs/compute/easier-integration-with-aws-lambda-and-amazon-api-gateway/)

Additionally, with the introduction of native .NET Core support, AWS provides a nice
[set of tools](https://github.com/aws/aws-lambda-dotnet) for working on their platform more naturally
and includes a *nifty* feature in the form of the package
[AspNetCoreServer](https://github.com/aws/aws-lambda-dotnet/tree/master/Libraries/src/Amazon.Lambda.AspNetCoreServer)
which allows you to run ASP.NET Core Web API (now part of MVC in Core) applications and in many
cases with very little changes.  This made it very easy and fast to port over the existing Tug
ASP.NET Core application.

## Setting Up Your Local Environment

To build/deploy this package you need to create/update up to three (3) files with your local environment details:
* `deploy-serverless.cmd`
  * there is a `sample-deploy-serverless.cmd` that you can use to get you started
  * edit the parameters in this file to match your specific environment
* `sample-aws-lambda-tools-defaults.json`
  * there is a `sample-aws-lambda-tools-defaults.json` that you can use to get you started
  * review and edit this file to match your specific environment
* `severless.template`
  * defines the CloudFormation template used to deploy the Lambda function using a SAM
    deployment model
  * normally you *do not* need to modify this file, as all the variable elements are
    input as parameters in the `deploy-serverless.cmd` file, but you can adjust some
    of the settings as necessary

## Notes

* If you keep getting "Missing authentication Token" error don't forget to DEPLOY the API:
  * http://www.awslessons.com/2017/aws-api-gateway-missing-authentication-token/'
