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


@REM Assemble all the CFN template parameters
@SET TEMPLATE_PARAMS=
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;S3Bucket=%TUGDSC_S3BUCKET%
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;S3KeyPrefix=%TUGDSC_S3PREFIX%
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS%;TugAppSettingsS3Key=appsettings.json
@SET TEMPLATE_PARAMS=%TEMPLATE_PARAMS:~1%


dotnet lambda deploy-serverless -sb %UPLOAD_S3BUCKET% -sp %UPLOAD_S3PREFIX% -tp %TEMPLATE_PARAMS%

@ENDLOCAL
