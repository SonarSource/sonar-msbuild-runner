﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2022 SonarSource SA
 * mailto: info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Threading.Tasks;
#if NETFRAMEWORK || NETCOREAPP2_1
using SonarScanner.MSBuild.AnalysisWarning;
#endif
using SonarScanner.MSBuild.Common;

namespace SonarScanner.MSBuild
{
    public static class Program
    {
        private const int ErrorCode = 1;
        private const int SuccessCode = 0;

        private static async Task<int> Main(string[] args)
            => await Execute(args);

        private static async Task<int> Execute(string[] args)
        {
            var logger = new ConsoleLogger(includeTimestamp: false);
            return await Execute(args, logger);
        }

        public static async Task<int> Execute(string[] args, ILogger logger)
        {
            Utilities.LogAssemblyVersion(logger, Resources.AssemblyDescription);
#if NET46
            logger.LogInfo("Using the .NET Framework version of the Scanner for MSBuild");
#else
            logger.LogInfo("Using the .NET Core version of the Scanner for MSBuild");
#endif

            logger.SuspendOutput();

            if (ArgumentProcessor.IsHelp(args))
            {
                logger.LogInfo(string.Empty);
                logger.LogInfo("Usage: ");
                logger.LogInfo(string.Empty);
                logger.LogInfo(
                    @"  {0} [begin|end] /key:project_key [/name:project_name] [/version:project_version] [/s:settings_file] [/d:sonar.login=token] [/d:sonar.{{property_name}}=value]",
                    AppDomain.CurrentDomain.FriendlyName);
                logger.LogInfo(string.Empty);
                logger.LogInfo("  - When executing the begin phase, at least the project key and the authentication token must be defined.");
                logger.LogInfo("  - The authentication token should be provided through 'sonar.login' parameter in both 'BEGIN' and 'END' steps. It should be the only provided parameter during the 'END' step.");
                logger.LogInfo("  - A settings file can be used to define properties. If no settings file path is given, the file SonarQube.Analysis.xml in the installation directory will be used.");
                logger.LogInfo("  - Other properties can dynamically be defined with '/d:'. For example, '/d:sonar.verbose=true'. See 'Useful links for full list of available properties.'");
                logger.LogInfo("\nUseful links:");
                logger.LogInfo("  - Available properties for SonarQube: https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/");
                logger.LogInfo("  - Available properties for SonarCloud: https://docs.sonarcloud.io/advanced-setup/ci-based-analysis/sonarscanner-for-net/");
                logger.LogInfo("  - Full list of Analysis Properties that can be specified with '/d:' : https://docs.sonarqube.org/latest/analysis/analysis-parameters/");
                logger.LogInfo("  - Generate a token for analysis on SonarQube: https://docs.sonarqube.org/latest/user-guide/user-token/");
                logger.LogInfo("  - Generate a token for analysis on SonarCloud: https://docs.sonarcloud.io/advanced-setup/user-accounts/");
                logger.ResumeOutput();
                return SuccessCode;
            }

            try
            {
                if (!ArgumentProcessor.TryProcessArgs(args, logger, out IBootstrapperSettings settings))
                {
                    logger.ResumeOutput();
                    // The argument processor will have logged errors
                    Environment.ExitCode = ErrorCode;
                    return ErrorCode;
                }

                var processorFactory = new DefaultProcessorFactory(logger);
#if NETFRAMEWORK

                var bootstrapper = new NetFrameworkBootstrapperClass(processorFactory, settings, logger);

#elif NETCOREAPP2_1

                var bootstrapper = new NetCore21BootstrapperClass(processorFactory, settings, logger);

#else

                var bootstrapper = new BootstrapperClass(processorFactory, settings, logger);

#endif

                var exitCode = await bootstrapper.Execute();
                Environment.ExitCode = exitCode;
                return exitCode;
            }
            finally
            {
#if DEBUG
                DEBUG_DumpLoadedAssemblies(logger);
#endif
            }
        }

#if DEBUG
        private static void DEBUG_DumpLoadedAssemblies(ILogger logger)
        {
            try
            {
                logger.IncludeTimestamp = false;
                logger.LogDebug(string.Empty);
                logger.LogDebug("**************************************************************");
                logger.LogDebug("*** Loaded assemblies");
                logger.LogDebug(string.Empty);

                // Note: the information is dumped in a format that can be cut and pasted into a CSV file
                logger.LogDebug("Name,Version, Culture,Public Key,Location");
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var location = asm.IsDynamic ? "{dynamically generated}" : asm.Location;
                    logger.LogDebug($"{asm.FullName},{location}");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error dumping assembly information: {ex.ToString()}");
            }
        }
#endif

    }
}
