@SETLOCAL

@REM **********************************************************************
@REM **  Search for and replace the variable elements with your
@REM **  counterparts down below by finding the 'REPLACE:' token
@REM **********************************************************************


@REM ** REPLACE:  the name of the S3 Bucket used to deploy Lambda function
@SET UPLOAD_S3BUCKET=S3_bucket_name_for_deploy

@REM ** REPLACE:  the S3 object key prefix prepended to Lambda deploy artifacts
@SET UPLOAD_S3PREFIX=S3_key_prefix_for_deploy/

@REM ** REPLACE:  the name of the S3 Bucket used to store DSC assets
@SET TUGDSC_S3BUCKET=S3_bucket_name_for_dsc

@REM ** REPLACE:  the S3 object key prefix prepended to path of DSC assets folders
@SET TUGDSC_S3PREFIX=S3_key_prefix_for_dsc/

@REM ** REPLACE:  the name of the Lambda function (defaults to TugDscLambda)
@SET TUGDSC_LAMBDA_NAME=


@REM Assemble all the CFN template parameters
@SET TEMPLATE_PARAMS=
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;S3Bucket=%TUGDSC_S3BUCKET%
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;S3KeyPrefix=%TUGDSC_S3PREFIX%
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;TugAppSettingsS3Key=appsettings.json
@IF NOT "%TUGDSC_LAMBDA_NAME%"=="" @SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;LambdaFunctionName=%TUGDSC_LAMBDA_NAME%

@REM Skip the leading ';'
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS:~1%


dotnet lambda deploy-serverless -sb %UPLOAD_S3BUCKET% -sp %UPLOAD_S3PREFIX% -tp %TEMPLATE_PARAMS%

@REM In case everything is peachy, need to remind about this one manual step
@IF "%ERRORLEVEL%"=="0" (
    @ECHO ************************************************************
    @ECHO **  DO NOT FORGET!!!
    @ECHO **    You have to manually add the wild-card entry '*/*'
    @ECHO **    as a Binary Media Content Type to your API Gateway
    @ECHO **    API instance that routes to your Tug DSC Lambda
    @ECHO.**    (See the README.md for more details^)
    @ECHO ************************************************************
)

@ENDLOCAL
