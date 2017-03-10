# FaaS Tug - AWS Lambda


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
