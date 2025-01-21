﻿/*
 * SonarScanner for .NET
 * Copyright (C) 2016-2025 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using SonarScanner.MSBuild.Common;

namespace SonarScanner.MSBuild.PreProcessor.AnalysisConfigProcessing.Processors;

/// <summary>
/// Initializes the server and command line settings in the AnalysisConfig.
/// </summary>
public class InitializationProcessor(BuildSettings buildSettings, ProcessedArgs localSettings, Dictionary<string, string> additionalSettings, IDictionary<string, string> serverProperties)
    : AnalysisConfigProcessorBase(localSettings, serverProperties)
{
    public override void Update(AnalysisConfig config)
    {
        config.SetBuildUri(buildSettings.BuildUri);
        config.SetTfsUri(buildSettings.TfsUri);
        config.SetVsCoverageConverterToolPath(buildSettings.CoverageToolUserSuppliedPath);
        foreach (var item in additionalSettings)
        {
            config.SetConfigValue(item.Key, item.Value);
        }
        foreach (var property in ServerProperties.Where(x => !Utilities.IsSecuredServerProperty(x.Key)))
        {
            AddSetting(config.ServerSettings, property.Key, property.Value);
        }
        foreach (var property in LocalSettings.CmdLineProperties.GetAllProperties()) // Only those from command line
        {
            AddSetting(config.LocalSettings, property.Id, property.Value);
        }
        if (!string.IsNullOrEmpty(LocalSettings.Organization))
        {
            AddSetting(config.LocalSettings, SonarProperties.Organization, LocalSettings.Organization);
        }
        if (LocalSettings.PropertiesFileName is not null)
        {
            config.SetSettingsFilePath(LocalSettings.PropertiesFileName);
        }
    }
}
