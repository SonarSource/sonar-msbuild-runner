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

using SonarScanner.MSBuild.PreProcessor.Roslyn.Model;

namespace SonarScanner.MSBuild.PreProcessor.Roslyn;

public class RoslynAnalyzerProvider(
    IAnalyzerInstaller analyzerInstaller,
    ILogger logger,
    BuildSettings teamBuildSettings,
    IAnalysisPropertyProvider sonarProperties,
    IEnumerable<SonarRule> rules,
    string language)
{
    public const string RulesetFileNameNormal = "Sonar-{0}.ruleset";
    public const string RulesetFileNameNone = "Sonar-{0}-none.ruleset";
    public const string CSharpLanguage = "cs";
    public const string VBNetLanguage = "vbnet";

    private const string ServerPropertyFormat = "sonar.{0}.analyzer";
    private const string RoslynRepoPrefix = "roslyn."; // Used for plugins generated by SonarQube Roslyn SDK, and legacy Sonar Security C# Frontend
    private const string LegacySecurityFrontEndPropertyPrefix = "sonaranalyzer.security.cs"; // Should not be used and exists for backward compatibility with version <= 9.2
    private const string LegacyServerPropertyPrefix = "sonaranalyzer-"; // Should not be used and exists for backward compatibility with version <= 9.2.

    protected readonly IAnalysisPropertyProvider sonarProperties = sonarProperties ?? throw new ArgumentNullException(nameof(sonarProperties));

    private readonly IAnalyzerInstaller analyzerInstaller = analyzerInstaller ?? throw new ArgumentNullException(nameof(analyzerInstaller));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly BuildSettings teamBuildSettings = teamBuildSettings ?? throw new ArgumentNullException(nameof(teamBuildSettings));
    private readonly string language = language ?? throw new ArgumentNullException(nameof(language));
    private readonly IEnumerable<SonarRule> rules = rules ?? throw new ArgumentNullException(nameof(rules));
    private readonly HashSet<string> roslynPropertyKeys = new(rules.Where(x => x.IsActive && x.RepoKey.StartsWith(RoslynRepoPrefix)).Select(x => x.RepoKey.Substring(RoslynRepoPrefix.Length)));

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
        var plugins = CreatePlugins();
        if (plugins.Length > 0)
        {
            logger.LogInfo(Resources.RAP_ProvisioningAnalyzerAssemblies, language);
            return analyzerInstaller.InstallAssemblies(plugins);
        }
        else
        {
            logger.LogInfo(Resources.RAP_NoAnalyzerPluginsSpecified, language);
            return [];
        }
    }

    private Plugin[] CreatePlugins()
    {
        var candidates = new Dictionary<string, Plugin>();
        foreach (var property in sonarProperties.GetAllProperties())
        {
            if (PluginPropertyPrefix(property.Id) is { } prefix)
            {
                if (!candidates.TryGetValue(prefix, out var plugin))
                {
                    plugin = new Plugin();
                    candidates.Add(prefix, plugin);
                }
                plugin.AddProperty(property.Id, property.Value);
            }
        }
        // If both legacy and new properties are present, remove the legacy version
        if (candidates.ContainsKey(string.Format(ServerPropertyFormat, language) + ".dotnet"))
        {
            candidates.Remove(LegacyServerPropertyPrefix + language);
        }
        if (candidates.ContainsKey(string.Format(ServerPropertyFormat, language) + ".security"))
        {
            candidates.Remove(LegacySecurityFrontEndPropertyPrefix);
        }
        return candidates.Values.Where(x => x.IsValid).ToArray();
    }

    private string PluginPropertyPrefix(string propertyId)
    {
        var prefix = propertyId.Substring(0, propertyId.LastIndexOf('.'));
        return prefix.StartsWith(string.Format(ServerPropertyFormat, language))
            || roslynPropertyKeys.Contains(prefix)
            || prefix == LegacyServerPropertyPrefix + language
            ? prefix
            : null;
    }
}
