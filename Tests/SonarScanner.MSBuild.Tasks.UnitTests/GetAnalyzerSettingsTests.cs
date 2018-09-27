﻿/*
 * SonarScanner for MSBuild
 * Copyright (C) 2016-2018 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
using System.IO;
using FluentAssertions;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarScanner.MSBuild.Common;
using TestUtilities;

namespace SonarScanner.MSBuild.Tasks.UnitTests
{
    [TestClass]
    public class GetAnalyzerSettingsTests
    {
        public TestContext TestContext { get; set; }

        #region Tests

        [TestMethod]
        public void GetAnalyzerSettings_MissingConfigDir_NoError()
        {
            // Arrange
            var testSubject = new GetAnalyzerSettings
            {
                AnalysisConfigDir = "c:\\missing"
            };

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            CheckNoAnalyzerSettings(testSubject);
        }

        [TestMethod]
        public void GetAnalyzerSettings_MissingConfigFile_NoError()
        {
            // Arrange
            var testSubject = new GetAnalyzerSettings
            {
                AnalysisConfigDir = TestContext.DeploymentDirectory
            };

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            CheckNoAnalyzerSettings(testSubject);
        }

        [TestMethod]
        public void GetAnalyzerSettings_ConfigExistsButNoAnalyzerSettings_NoError()
        {
            // Arrange
            var testSubject = CreateConfiguredTestSubject(new AnalysisConfig(), "anyLanguage", TestContext);

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            CheckNoAnalyzerSettings(testSubject);
        }

        [TestMethod]
        public void GetAnalyzerSettings_ConfigExists_Legacy_SettingsOverwritten()
        {
            // Arrange
            var expectedAnalyzers = new string[] { "c:\\analyzer1.DLL", "c:\\analyzer2.dll" };

            // SONARMSBRU-216: non-assembly files should be filtered out
            var filesInConfig = new List<string>(expectedAnalyzers)
            {
                "c:\\not_an_assembly.exe",
                "c:\\not_an_assembly.zip",
                "c:\\not_an_assembly.txt",
                "c:\\not_an_assembly.dll.foo",
                "c:\\not_an_assembly.winmd"
            };

            var config = new AnalysisConfig
            {
                SonarQubeVersion = "7.3",
                ServerSettings = new AnalysisProperties
                {
                    // Setting should be ignored
                    new Property { Id = "sonar.cs.roslyn.ignoreIssues", Value = "true" }
                },
                AnalyzersSettings = new List<AnalyzerSettings>
                {
                    new AnalyzerSettings
                    {
                        Language = "cs",
                        RuleSetFilePath = "f:\\yyy.ruleset",
                        AnalyzerAssemblyPaths = filesInConfig,
                        AdditionalFilePaths = new List<string> { "c:\\add1.txt", "d:\\add2.txt", "e:\\subdir\\add3.txt" }
                    },

                    new AnalyzerSettings
                    {
                        Language = "cobol",
                        RuleSetFilePath = "f:\\xxx.ruleset",
                        AnalyzerAssemblyPaths = filesInConfig,
                        AdditionalFilePaths = new List<string> { "c:\\cobol.\\add1.txt", "d:\\cobol\\add2.txt" }
                    }
                }
            };

            var testSubject = CreateConfiguredTestSubject(config, "cs", TestContext);
            testSubject.OriginalAdditionalFiles = new string[]
            {
                "original.should.be.preserved.txt",
                "original.should.be.replaced\\add2.txt",
                "e://foo//should.be.replaced//add3.txt"
            };

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            testSubject.RuleSetFilePath.Should().Be("f:\\yyy.ruleset");
            testSubject.AnalyzerFilePaths.Should().BeEquivalentTo(expectedAnalyzers);
            testSubject.AdditionalFilePaths.Should().BeEquivalentTo("c:\\add1.txt", "d:\\add2.txt",
                "e:\\subdir\\add3.txt", "original.should.be.preserved.txt");
        }

        [TestMethod]
        public void GetAnalyzerSettings_ConfigExists_NewBehaviour_SettingsMerged()
        {
            // Expecting both the additional files and the analyzers to be merged

            // Arrange
            // SONARMSBRU-216: non-assembly files should be filtered out
            var filesInConfig = new List<string>
            {
                "c:\\config\\analyzer1.DLL",
                "c:\\config\\analyzer2.dll",
                "c:\\not_an_assembly.exe",
                "c:\\not_an_assembly.zip",
                "c:\\not_an_assembly.txt",
                "c:\\not_an_assembly.dll.foo",
                "c:\\not_an_assembly.winmd"
            };

            var config = new AnalysisConfig
            {
                SonarQubeVersion = "7.4",
                ServerSettings = new AnalysisProperties
                                {
                    new Property { Id = "sonar.cs.roslyn.ignoreIssues", Value = "false" }
                },
                AnalyzersSettings = new List<AnalyzerSettings>
                {
                    new AnalyzerSettings
                    {
                        Language = "cs",
                        RuleSetFilePath = "f:\\yyy.ruleset",
                        AnalyzerAssemblyPaths = filesInConfig,
                        AdditionalFilePaths = new List<string> { "c:\\config\\add1.txt", "d:\\config\\add2.txt" }
                    },

                    new AnalyzerSettings
                    {
                        Language = "cobol",
                        RuleSetFilePath = "f:\\xxx.ruleset",
                        AnalyzerAssemblyPaths = new List<string>(),
                        AdditionalFilePaths = new List<string> { "c:\\cobol.\\add1.txt", "d:\\cobol\\add2.txt" }
                    }

                }
            };

            var testSubject = CreateConfiguredTestSubject(config, "cs", TestContext);

            testSubject.OriginalAnalyzers = new string[]
            {
                "c:\\original.should.be.removed\\analyzer1.DLL",
                "f:\\original.should.be.preserved\\analyzer3.dll"
            };

            testSubject.OriginalAdditionalFiles = new string[]
            {
                "original.should.be.preserved.txt",
                "original.should.be.replaced\\add2.txt"
            };

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            testSubject.RuleSetFilePath.Should().Be("f:\\yyy.ruleset");

            testSubject.AnalyzerFilePaths.Should().BeEquivalentTo(
                "c:\\config\\analyzer1.DLL",
                "c:\\config\\analyzer2.dll",
                "f:\\original.should.be.preserved\\analyzer3.dll");

            testSubject.AdditionalFilePaths.Should().BeEquivalentTo(
                "c:\\config\\add1.txt",
                "d:\\config\\add2.txt",
                "original.should.be.preserved.txt");
        }

        [TestMethod]
        public void ShouldMerge_OldServerVersion_ReturnsFalse()
        {
            // The "importAllValue" setting should be ignored for old server versions
            var logger = CheckShouldMerge("7.3.1", "cs", ignoreExternalIssues: "true", expected: false);
            logger.AssertInfoMessageExists("External issues are not supported on this version of SonarQube. SQv7.4+ is required.");

            logger = CheckShouldMerge("6.7.0", "vbnet", ignoreExternalIssues: "true", expected: false);
            logger.AssertInfoMessageExists("External issues are not supported on this version of SonarQube. SQv7.4+ is required.");
        }

        [TestMethod]
        public void ShouldMerge_Multiples_NewServer_NoSetting_ReturnsTrue()
        {
            // Should default to false i.e. override, don't merge
            var logger = CheckShouldMerge("7.4.0.0", "cs", ignoreExternalIssues: null /* not set */, expected: true);
            logger.AssertDebugLogged("sonar.cs.roslyn.ignoreIssues=false");

            logger = CheckShouldMerge("7.4.0.0", "vbnet", ignoreExternalIssues: null /* not set */, expected: true);
            logger.AssertDebugLogged("sonar.vbnet.roslyn.ignoreIssues=false");
        }

        [TestMethod]
        public void ShouldMerge_NewServerVersion_SettingIsTrue_ReturnsFalse()
        {
            var logger = CheckShouldMerge("8.9", "cs", ignoreExternalIssues: "true", expected: false);
            logger.AssertDebugLogged("sonar.cs.roslyn.ignoreIssues=true");

            logger = CheckShouldMerge("7.4", "vbnet", ignoreExternalIssues: "true", expected: false);
            logger.AssertDebugLogged("sonar.vbnet.roslyn.ignoreIssues=true");
        }

        [TestMethod]
        public void ShouldMerge_NewServerVersion_SettingIsFalse_ReturnsTrue()
        {
            var logger = CheckShouldMerge("7.4", "cs", ignoreExternalIssues: "false", expected: true);
            logger.AssertDebugLogged("sonar.cs.roslyn.ignoreIssues=false");

            logger = CheckShouldMerge("7.7", "vbnet", ignoreExternalIssues: "false", expected: true);
            logger.AssertDebugLogged("sonar.vbnet.roslyn.ignoreIssues=false");
        }

        [TestMethod]
        public void ShouldMerge_NewServerVersion_InvalidSetting_NoError_ReturnsFalse()
        {
            var logger = CheckShouldMerge("7.4", "cs", ignoreExternalIssues: "not a boolean value", expected: false);
            logger.AssertSingleWarningExists("Invalid value for 'sonar.cs.roslyn.ignoreIssues'. Expecting 'true' or 'false'. Actual: 'not a boolean value'. External issues will not be imported.");

            logger = CheckShouldMerge("7.7", "vbnet", ignoreExternalIssues: "not a boolean value", expected: false);
            logger.AssertSingleWarningExists("Invalid value for 'sonar.vbnet.roslyn.ignoreIssues'. Expecting 'true' or 'false'. Actual: 'not a boolean value'. External issues will not be imported.");
        }

        private static TestLogger CheckShouldMerge(string serverVersion, string language, string ignoreExternalIssues, bool expected)
        {
            // Should default to true i.e. don't override, merge
            var logger = new TestLogger();
            var config = new AnalysisConfig
            {
                SonarQubeVersion = serverVersion
            };
            if (ignoreExternalIssues != null)
            {
                config.ServerSettings = new AnalysisProperties
                {
                    new Property { Id = $"sonar.{language}.roslyn.ignoreIssues", Value = ignoreExternalIssues }
                };
            }

            var result = GetAnalyzerSettings.ShouldMergeAnalysisSettings(language, config, logger);

            result.Should().Be(expected);
            return logger;
        }

        [TestMethod]
        public void MergeRulesets_NoOriginalRuleset_FirstGeneratedRulsetUsed()
        {
            // Arrange
            var config = new AnalysisConfig
            {
                SonarQubeVersion = "7.4",
                AnalyzersSettings = new List<AnalyzerSettings>
                {
                    new AnalyzerSettings
                    {
                        Language = "xxx",
                        RuleSetFilePath = "firstGeneratedRuleset.txt"
                    }
                }
            };

            var testSubject = CreateConfiguredTestSubject(config, "xxx", TestContext);
            testSubject.OriginalRulesetFilePath = null;

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            testSubject.RuleSetFilePath.Should().Be("firstGeneratedRuleset.txt");
        }

        [TestMethod]
        public void MergeRulesets_OriginalRulesetSpecified_RelativePath_SecondGeneratedRulsetUsed()
        {
            // Arrange
            var config = new AnalysisConfig
            {
                SonarQubeVersion = "7.4",
                ServerSettings = new AnalysisProperties
                {
                    new Property { Id = "sonar.xxx.roslyn.ignoreIssues", Value = "false" }
                },
                AnalyzersSettings = new List<AnalyzerSettings>
                {
                    new AnalyzerSettings
                    {
                        Language = "xxx",
                        RuleSetFilePath = "firstGeneratedRuleset.txt"
                    }
                }
            };

            var testSubject = CreateConfiguredTestSubject(config, "xxx", TestContext);
            testSubject.CurrentProjectDirectoryPath = "c:\\solution.folder\\project.folder";
            testSubject.OriginalRulesetFilePath = ".\\..\\originalRuleset.txt";
            testSubject.ProjectSpecificConfigDirectory = testSubject.AnalysisConfigDir;

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            CheckMergedRulesetFile(testSubject, "c:\\solution.folder\\originalRuleset.txt",
                "firstGeneratedRuleset.txt");
        }

        [TestMethod]
        public void MergeRulesets_OriginalRulesetSpecified_AbsolutePath_SecondGeneratedRulsetUsed()
        {
            // Arrange
            var config = new AnalysisConfig
            {
                SonarQubeVersion = "7.4",
                ServerSettings = new AnalysisProperties
                {
                    new Property { Id = "sonar.xxx.roslyn.ignoreIssues", Value = "false" }
                },
                AnalyzersSettings = new List<AnalyzerSettings>
                {
                    new AnalyzerSettings
                    {
                        Language = "xxx",
                        RuleSetFilePath = "firstGeneratedRuleset.txt"
                    }
                }
            };

            var testSubject = CreateConfiguredTestSubject(config, "xxx", TestContext);
            testSubject.CurrentProjectDirectoryPath = "c:\\nonexistent.project.path";
            testSubject.OriginalRulesetFilePath = "e:\\sub1\\originalRuleset.txt";
            testSubject.ProjectSpecificConfigDirectory = testSubject.AnalysisConfigDir;

            // Act
            ExecuteAndCheckSuccess(testSubject);

            // Assert
            CheckMergedRulesetFile(testSubject, "e:\\sub1\\originalRuleset.txt",
                "firstGeneratedRuleset.txt");
        }

        #endregion Tests

        #region Private methods

        private static GetAnalyzerSettings CreateConfiguredTestSubject(AnalysisConfig config, string language, TestContext testContext)
        {
            var testDir = TestUtils.CreateTestSpecificFolder(testContext);
            var testSubject = new GetAnalyzerSettings();
            testSubject.Language = language;

            var fullPath = Path.Combine(testDir, FileConstants.ConfigFileName);
            config.Save(fullPath);

            testSubject.AnalysisConfigDir = testDir;
            return testSubject;
        }

        #endregion

        #region Checks methods

        private static void ExecuteAndCheckSuccess(Task task)
        {
            var dummyEngine = new DummyBuildEngine();
            task.BuildEngine = dummyEngine;

            var taskSucess = task.Execute();
            taskSucess.Should().BeTrue("Expecting the task to succeed");
            dummyEngine.AssertNoErrors();
            dummyEngine.AssertNoWarnings();
        }

        private static void CheckNoAnalyzerSettings(GetAnalyzerSettings executedTask)
        {
            executedTask.RuleSetFilePath.Should().BeNull();
            executedTask.AdditionalFilePaths.Should().BeNull();
            executedTask.AnalyzerFilePaths.Should().BeNull();
        }

        private static void CheckMergedRulesetFile(GetAnalyzerSettings executedTask,
            string originalRulesetFullPath, string firstGeneratedRulesetFilePath)
        {
            var expectedMergedRulesetFilePath = RuleSetAssertions.CheckMergedRulesetFile(
                executedTask.ProjectSpecificConfigDirectory,
                originalRulesetFullPath,
                firstGeneratedRulesetFilePath);

            executedTask.RuleSetFilePath.Should().Be(expectedMergedRulesetFilePath);
        }

        #endregion Checks methods
    }
}
