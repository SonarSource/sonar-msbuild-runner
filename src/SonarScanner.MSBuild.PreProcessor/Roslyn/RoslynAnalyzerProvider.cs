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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SonarScanner.MSBuild.Common;
using SonarScanner.MSBuild.PreProcessor.Roslyn.Model;

namespace SonarScanner.MSBuild.PreProcessor.Roslyn;

public class RoslynAnalyzerProvider(IAnalyzerInstaller analyzerInstaller, ILogger logger, BuildSettings teamBuildSettings, IAnalysisPropertyProvider sonarProperties, IEnumerable<SonarRule> rules, string language)
{
    public const string RulesetFileNameNormal = "Sonar-{0}.ruleset";
    public const string RulesetFileNameNone = "Sonar-{0}-none.ruleset";
    public const string CSharpLanguage = "cs";
    public const string VBNetLanguage = "vbnet";

    private const string LegacyServerPropertyPrefix = "sonaranalyzer-";
    private const string RoslynRepoPrefix = "roslyn.";

    protected readonly IAnalyzerInstaller analyzerInstaller = analyzerInstaller ?? throw new ArgumentNullException(nameof(analyzerInstaller));
    protected readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    protected readonly BuildSettings teamBuildSettings = teamBuildSettings ?? throw new ArgumentNullException(nameof(teamBuildSettings));
    protected readonly IAnalysisPropertyProvider sonarProperties = sonarProperties ?? throw new ArgumentNullException(nameof(sonarProperties));
    protected readonly string language = language ?? throw new ArgumentNullException(nameof(language));
    protected readonly IEnumerable<SonarRule> rules = rules ?? throw new ArgumentNullException(nameof(rules));

    /// <summary>
    /// Generates several files related to rulesets and roslyn analyzer assemblies.
    /// Active rules should never be empty, but depending on the server settings of repo keys, we might have no rules in the ruleset.
    /// In that case, this method returns null.
    /// </summary>
    public virtual AnalyzerSettings SetupAnalyzer() =>
        new(language, CreateRuleSet(false), CreateRuleSet(true), FetchAnalyzerPlugins(), WriteAdditionalFiles());

    private string CreateRuleSet(bool deactivateAll)
    {
        var ruleSetGenerator = new RoslynRuleSetGenerator(deactivateAll);
        var ruleSet = ruleSetGenerator.Generate(rules);
        var rulesetFilePath = Path.Combine(teamBuildSettings.SonarConfigDirectory, string.Format(deactivateAll ? RulesetFileNameNone : RulesetFileNameNormal, language));
        logger.LogDebug(Resources.RAP_UnpackingRuleset, rulesetFilePath);
        ruleSet.Save(rulesetFilePath);
        return rulesetFilePath;
    }

    private IEnumerable<string> WriteAdditionalFiles() =>
        TryWriteSonarLintXmlFile() is { } filePath ? [filePath] : [];

    private string TryWriteSonarLintXmlFile()
    {
        var dir = Path.Combine(teamBuildSettings.SonarConfigDirectory, language);
        Directory.CreateDirectory(dir);
        var sonarLintXmlPath = Path.Combine(dir, "SonarLint.xml");
        if (File.Exists(sonarLintXmlPath))
        {
            logger.LogDebug(Resources.RAP_AdditionalFileAlreadyExists, language, sonarLintXmlPath);
            return null;
        }
        else
        {
            var content = RoslynSonarLint.GenerateXml(rules.Where(x => x.IsActive), sonarProperties, language);
            logger.LogDebug(Resources.RAP_WritingAdditionalFile, sonarLintXmlPath);
            File.WriteAllText(sonarLintXmlPath, content);
            return sonarLintXmlPath;
        }
    }

    private IEnumerable<AnalyzerPlugin> FetchAnalyzerPlugins()
    {
        var partialRepoKeys = ActiveRulesPartialRepoKeys(rules.Where(x => x.IsActive));
        List<Plugin> plugins = new();
        foreach (var partialRepoKey in partialRepoKeys)
        {
            if (sonarProperties.TryGetValue($"{partialRepoKey}.pluginKey", out var pluginKey)
                && sonarProperties.TryGetValue($"{partialRepoKey}.pluginVersion", out var pluginVersion)
                && sonarProperties.TryGetValue($"{partialRepoKey}.staticResourceName", out var staticResourceName))
            {
                plugins.Add(new Plugin(pluginKey, pluginVersion, staticResourceName));
            }
            else if (!partialRepoKey.StartsWith(LegacyServerPropertyPrefix))
            {
                logger.LogInfo(Resources.RAP_NoAssembliesForRepo, partialRepoKey, language);
            }
        }
        if (plugins.Count == 0)
        {
            logger.LogInfo(Resources.RAP_NoAnalyzerPluginsSpecified, language);
            return [];
        }
        else
        {
            logger.LogInfo(Resources.RAP_ProvisioningAnalyzerAssemblies, language);
            return analyzerInstaller.InstallAssemblies(plugins);
        }
    }

    private static ICollection<string> ActiveRulesPartialRepoKeys(IEnumerable<SonarRule> rules)
    {
        var partialRepoKeys = new HashSet<string>
        {
            // Always add C# and VB.NET to have at least tokens...
            LegacyServerPropertyPrefix + "cs",
            LegacyServerPropertyPrefix + "vbnet",
        };
        // Roslyn SDK and legacy Security C# Frontend
        partialRepoKeys.UnionWith(
            rules
                .Where(x => x.RepoKey.StartsWith(RoslynRepoPrefix))
                .Select(x => x.RepoKey.Substring(RoslynRepoPrefix.Length)));
        return partialRepoKeys;
    }
}
