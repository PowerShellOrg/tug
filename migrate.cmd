@REM 
@REM   C:\prj\zyborg\PowerShell-tug\tug\src\Tug.Client>dotnet migrate --help
@REM   .NET Migrate Command
@REM   
@REM   Usage: dotnet migrate [arguments] [options]
@REM   
@REM   Arguments:
@REM     <PROJECT_JSON/GLOBAL_JSON/SOLUTION_FILE/PROJECT_DIR>  The path to one of the following:
@REM       - a project.json file to migrate.
@REM       - a global.json file, it will migrate the folders specified in global.json.
@REM       - a solution.sln file, it will migrate the projects referenced in the solution.
@REM       - a directory to migrate, it will recursively search for project.json files to migrate.
@REM   Defaults to current directory if nothing is specified.
@REM   
@REM   Options:
@REM     -h|--help                     Show help information
@REM     -t|--template-file            Base MSBuild template to use for migrated app. The default is the project included in dotnet new.
@REM     -v|--sdk-package-version      The version of the sdk package that will be referenced in the migrated app. The default is the version of the sdk in dotnet new.
@REM     -x|--xproj-file               The path to the xproj file to use. Required when there is more than one xproj in a project directory.
@REM     -s|--skip-project-references  Skip migrating project references. By default project references are migrated recursively.
@REM     -r|--report-file              Output migration report to the given file in addition to the console.
@REM     --format-report-file-json     Output migration report file as json rather than user messages.
@REM     --skip-backup                 Skip moving project.json, global.json, and *.xproj to a `backup` directory after successful migration.
@REM   

@SETLOCAL
@SET THIS_DIR=%~dp0

dotnet migrate "%THIS_DIR%global.json" --report-file "%THIS_DIR%migrate-report.out"

dotnet new sln

dotnet sln add src\Tug.Base\Tug.Base.csproj src\Tug.Server.Base\Tug.Server.Base.csproj
dotnet sln add src\Tug.Client\Tug.Client.csproj
dotnet sln add src\Tug.Server\Tug.Server.csproj
dotnet sln add src\Tug.Server.Providers.Ps5DscHandler\Tug.Server.Providers.Ps5DscHandler.csproj
dotnet sln add src\Tug.Server.FaaS.AwsLambda\Tug.Server.FaaS.AwsLambda.csproj

dotnet sln add test\Tug.UnitTesting\Tug.UnitTesting.csproj
dotnet sln add test\Tug.Ext-tests\Tug.Ext-tests.csproj
dotnet sln add test\Tug.Ext-tests-aux\Tug.Ext-tests-aux.csproj
dotnet sln add test\client\Tug.Client-tests\Tug.Client-tests.csproj
dotnet sln add test\server\Tug.Server-itests\Tug.Server-itests.csproj
dotnet sln add test\server\Tug.Server.FaaS.AwsLambda-tests\Tug.Server.FaaS.AwsLambda-tests.csproj
