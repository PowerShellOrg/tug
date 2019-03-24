// PowerShell.org Tug DSC Pull Server
// Copyright (c) The DevOps Collective, Inc.  All rights reserved.
// Licensed under the MIT license.  See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;

namespace TugDSC.Client.CLIApp.Configuration
{
    public class CommandLine
    {


        public static readonly string ASMNAME =
                typeof(CommandLine).GetTypeInfo().Assembly.GetName().Name;
        public static readonly string VERSION =
                typeof(CommandLine).GetTypeInfo().Assembly.GetName().Version.ToString();

        CommandLineApplication _root;

        public CommandLine(string[] args = null)
        {
            if (args != null)
            {
                Init();
                Execute(args);
            }
        }

        public bool ShowingFeedback
        { get; set; }

        public string ConfigFile
        { get; set; } = Program.APP_CONFIG_FILENAME;

        public string ConfigEnvPrefix
        { get; set; } = Program.APP_CONFIG_ENV_PREFIX;

        /// <summary>
        /// An optional set of configuration settings values already prepared
        /// in a form that can be passed into the CommandLine Configuration Provider
        /// (i.e. a string array where each entry is prefixed with a leading '/').
        /// </summary>
        public string[] ConfigValues
        { get; set; }

        public Action OnRegisterAgent
        { get; set; }

        public Action OnGetAction
        { get; set; }

        public Action OnGetConfiguration
        { get; set; }

        public Action OnGetActionAndConfiguration
        { get; set; }

        public Action OnGetModule
        { get; set; }

        public Action OnSendReport
        { get; set; }

        public Action OnGetReports
        { get; set; }

        public CommandLine Init()
        {
            _root = new CommandLineApplication();

            _root.FullName = ASMNAME;
            _root.Description = "Command-line interface driver to the Tug DSC test client.";
            
            // Define commands and modes of operations
            var runCommand = _root.Command("run", cl =>
            {
                cl.HelpOption("-h|--help");
                cl.OnExecute((Func<int>)(() =>
                {
                    throw new NotImplementedException();
                }));
            });

            var registerAgentCommand = _root.Command("register-agent", cl =>
            {
                cl.Description = "Register a node agent with a"
                        + " configuration repository server";
                cl.HelpOption("-h|--help");
                cl.OnExecute(() =>
                {
                    if (OnRegisterAgent != null)
                        OnRegisterAgent();
                    return 0;
                });
            });

            var getActionCommand = _root.Command("get-action", cl =>
            {
                cl.Description = "Retrieves the current retrieval status of a set of"
                        + " configuration names from the configuration repository server";
                cl.HelpOption("-h|--help");
                var getConfigurationOption = cl.Option("-g|--get-configurations",
                        "Flag indicates to retrieve any outdated configurations"
                                + " received in the response",
                        CommandOptionType.NoValue);
                cl.OnExecute(() =>
                {
                    if (getConfigurationOption.HasValue())
                    {
                        if (OnGetActionAndConfiguration != null)
                            OnGetActionAndConfiguration();
                    }
                    else
                    {
                        if (OnGetAction != null)
                            OnGetAction();
                    }
                    
                    return 0; 
                });
            });

            var getConfigurationCommand = _root.Command("get-configuration", cl =>
            {
                cl.Description = "Retrieves the MOF for a given configuration name"
                        + " or from the result of a status check"
                        + " from the configuration repository server";
                cl.HelpOption("-h|--help");
                cl.OnExecute(() =>
                {
                    if (OnGetConfiguration != null)
                        OnGetConfiguration();
                    return 0;
                });
            });

            var getModuleCommand = _root.Command("get-module", cl =>
            {
                cl.Description = "Retrieves a module for a given name and version"
                        + " from the resource repository server";
                cl.HelpOption("-h|--help");
                var modulesOption = cl.Option("-m|--module <module=version>",
                        "Specifies the module and version to request, can be specified multiple times",
                        CommandOptionType.MultipleValue);
                cl.OnExecute(() =>
                {
                    if (OnGetModule != null)
                        OnGetModule();
                    return 0;
                });
            });

            var sendReport = _root.Command("send-report", cl =>
            {
                cl.Description = "Sends node state report to the reporting server";
                cl.HelpOption("-h|--help");
                cl.OnExecute((Func<int>)(() =>
                {
                    throw new NotImplementedException();
                }));
            });

            var getReports = _root.Command("get-reports", cl =>
            {
                cl.Description = "Fetches existing reports from the reporting server";
                cl.HelpOption("-h|--help");
                cl.OnExecute((Func<int>)(() =>
                {
                    throw new NotImplementedException();
                }));
            });

            // Define global options
            CommandOption _helpOption = _root.HelpOption("-h|--help|-?");
            CommandOption _versionOption = _root.VersionOption("-v|--version",
                    "v" + VERSION,
                    "Tug.Client CLI version " + VERSION);

            CommandOption _configFileOption = _root.Option("-c|--config <configFile>",
                    "Override the default configuration file"
                            + $" ({Program.APP_CONFIG_FILENAME})",
                    CommandOptionType.SingleValue);

            CommandOption _configEnvOption = _root.Option("-e|--envprefix <configEnvPrefix>",
                    "Override the default ENV variable configuration prefix"
                            + $" ({Program.APP_CONFIG_ENV_PREFIX})",
                    CommandOptionType.SingleValue);

            CommandOption _configValuesOption = _root.Option("-s|--set-value <key=value>",
                    "Override/set configuration values",
                    CommandOptionType.MultipleValue);

            // Define execute behavior
            _root.OnExecute(() =>
            {
                ShowingFeedback = _root.IsShowingInformation;

                if (_configFileOption.HasValue())
                    ConfigFile = _configFileOption.Value();

                if (_configEnvOption.HasValue())
                    ConfigEnvPrefix = _configEnvOption.Value();

                if (_configValuesOption.HasValue())
                    ConfigValues = _configValuesOption.Values.Select(x => "/" + x).ToArray();

                return 0;
            });
            
            return this;
        }

        public CommandLine Execute(string[] args)
        {
            _root.Execute(args);
            return this;
        }
    }
}