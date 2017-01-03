Configuration StaticTestConfig {

    Node StaticTestConfig {
        File TempDir {
            Ensure = 'Present'
            Type = 'Directory'
            DestinationPath = 'c:\temp'
        }

        File TempFile {
            DependsOn = "[File]TempDir"
            Ensure = 'Present'
            Type = 'File'
            DestinationPath = 'c:\temp\dsc-statictestconfig-file.txt'
            Contents = 'STATIC TEST CONFIG'
        }
    }
}

StaticTestConfig

Publish-MOFToPullServer -PullServerWebConfig C:\DscService\WebSite\web.config -FullName .\StaticTestConfig\StaticTestConfig.mof -Verbose
