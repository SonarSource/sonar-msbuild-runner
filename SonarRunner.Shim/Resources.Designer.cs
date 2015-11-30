﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SonarRunner.Shim {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SonarRunner.Shim.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to sonar-project.properties files are not understood by the SonarQube Scanner for MSBuild. Remove those files from the following folders: {0}.
        /// </summary>
        public static string ERR_ConflictingSonarProjectProperties {
            get {
                return ResourceManager.GetString("ERR_ConflictingSonarProjectProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred loading the analysis config file. Please check that it is a valid file and try again. Error: {0}.
        /// </summary>
        public static string ERR_ErrorLoadingConfigFile {
            get {
                return ResourceManager.GetString("ERR_ErrorLoadingConfigFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expected to be called with one argument: the full path to the analysis config file.
        /// </summary>
        public static string ERR_InvalidCommandLineArgs {
            get {
                return ResourceManager.GetString("ERR_InvalidCommandLineArgs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No ProjectInfo.xml files were found. Possible causes: 1. The project has not been built - the &lt;end&gt; step was called right after the &lt;begin&gt;step, without a build step in between 2. An unsupported version of MSBuild has been used to build the project. Currently MSBuild 12 and 14 are supported 3. The build step has been launched from a different working folder. .
        /// </summary>
        public static string ERR_NoProjectInfoFilesFound {
            get {
                return ResourceManager.GetString("ERR_NoProjectInfoFilesFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No analysable projects were found but some duplicate project IDs were found. Possible cause: you are building multiple configurations (e.g. DEBUG|x86 and RELEASE|x64) at the same time, which is not supported by the SonarQube integration. Please build and analyse each configuration individually..
        /// </summary>
        public static string ERR_NoValidButDuplicateProjects {
            get {
                return ResourceManager.GetString("ERR_NoValidButDuplicateProjects", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No analysable projects were found. SonarQube analysis will not be performed. Check the build summary report for details..
        /// </summary>
        public static string ERR_NoValidProjectInfoFiles {
            get {
                return ResourceManager.GetString("ERR_NoValidProjectInfoFiles", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The SonarQube Scanner did not complete successfully.
        /// </summary>
        public static string ERR_SonarRunnerExecutionFailed {
            get {
                return ResourceManager.GetString("ERR_SonarRunnerExecutionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling the SonarQube Scanner....
        /// </summary>
        public static string MSG_CallingSonarRunner {
            get {
                return ResourceManager.GetString("MSG_CallingSonarRunner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generating SonarQube project properties file to {0}.
        /// </summary>
        public static string MSG_GeneratingProjectProperties {
            get {
                return ResourceManager.GetString("MSG_GeneratingProjectProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Analysis property is already correctly set: {0}={1}.
        /// </summary>
        public static string MSG_MandatorySettingIsCorrectlySpecified {
            get {
                return ResourceManager.GetString("MSG_MandatorySettingIsCorrectlySpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The project does not contain any files that can analyzed by SonarQube. Project file: {0}.
        /// </summary>
        public static string MSG_NoFilesToAnalyze {
            get {
                return ResourceManager.GetString("MSG_NoFilesToAnalyze", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The exclude flag has been set so the project will not be analyzed by SonarQube. Project file: {0}.
        /// </summary>
        public static string MSG_ProjectIsExcluded {
            get {
                return ResourceManager.GetString("MSG_ProjectIsExcluded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generation of the sonar-properties file failed. Unable to complete SonarQube analysis..
        /// </summary>
        public static string MSG_PropertiesGenerationFailed {
            get {
                return ResourceManager.GetString("MSG_PropertiesGenerationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Code Analysis ErrorLog file found at {0}..
        /// </summary>
        public static string MSG_SarifFileNotFound {
            get {
                return ResourceManager.GetString("MSG_SarifFileNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fixed invalid Code Analysis ErrorLog file. Please check that VS 2015 Update 1 (or later) is installed..
        /// </summary>
        public static string MSG_SarifFixSuccess {
            get {
                return ResourceManager.GetString("MSG_SarifFixSuccess", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Setting analysis property: {0}={1}.
        /// </summary>
        public static string MSG_SettingAnalysisProperty {
            get {
                return ResourceManager.GetString("MSG_SettingAnalysisProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The SonarQube Scanner has finished.
        /// </summary>
        public static string MSG_SonarRunnerCompleted {
            get {
                return ResourceManager.GetString("MSG_SonarRunnerCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The SONAR_RUNNER_HOME environment variable is not required and will be ignored..
        /// </summary>
        public static string MSG_SonarRunnerHomeIsSet {
            get {
                return ResourceManager.GetString("MSG_SonarRunnerHomeIsSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is not configured. Setting it to the default value of {1}.
        /// </summary>
        public static string MSG_SonarRunnerOptsDefaultUsed {
            get {
                return ResourceManager.GetString("MSG_SonarRunnerOptsDefaultUsed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is already set. Value: {1}.
        /// </summary>
        public static string MSG_SonarRunOptsAlreadySet {
            get {
                return ResourceManager.GetString("MSG_SonarRunOptsAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Writing processing summary to {0}.
        /// </summary>
        public static string MSG_WritingSummary {
            get {
                return ResourceManager.GetString("MSG_WritingSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Excluded projects.
        /// </summary>
        public static string REPORT_ExcludedProjectsTitle {
            get {
                return ResourceManager.GetString("REPORT_ExcludedProjectsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid projects.
        /// </summary>
        public static string REPORT_InvalidProjectsTitle {
            get {
                return ResourceManager.GetString("REPORT_InvalidProjectsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {none}.
        /// </summary>
        public static string REPORT_NoProjectsOfType {
            get {
                return ResourceManager.GetString("REPORT_NoProjectsOfType", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Product projects.
        /// </summary>
        public static string REPORT_ProductProjectsTitle {
            get {
                return ResourceManager.GetString("REPORT_ProductProjectsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Skipped projects.
        /// </summary>
        public static string REPORT_SkippedProjectsTitle {
            get {
                return ResourceManager.GetString("REPORT_SkippedProjectsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Test projects.
        /// </summary>
        public static string REPORT_TestProjectsTitle {
            get {
                return ResourceManager.GetString("REPORT_TestProjectsTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Code coverage report does not exist at the specified location. Path: {0}.
        /// </summary>
        public static string WARN_CodeCoverageReportNotFound {
            get {
                return ResourceManager.GetString("WARN_CodeCoverageReportNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate project GUID: &quot;{0}&quot;. Check that the project is only being built for a single platform/configuration and that that the project guid is unique. The project will not be analyzed by SonarQube. Project file: {1}.
        /// </summary>
        public static string WARN_DuplicateProjectGuid {
            get {
                return ResourceManager.GetString("WARN_DuplicateProjectGuid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File referenced in the project does not exist: {0}.
        /// </summary>
        public static string WARN_FileDoesNotExist {
            get {
                return ResourceManager.GetString("WARN_FileDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to File is not under the project directory and cannot currently be analysed by SonarQube. File: {0}, project: {1}.
        /// </summary>
        public static string WARN_FileIsOutsideProjectDirectory {
            get {
                return ResourceManager.GetString("WARN_FileIsOutsideProjectDirectory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to FxCop analysis report does not exist at the specified location. Path: {0}.
        /// </summary>
        public static string WARN_FxCopReportNotFound {
            get {
                return ResourceManager.GetString("WARN_FxCopReportNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The project has an invalid GUID &quot;{0}&quot;. The project will not be analyzed by SonarQube. Project file: {1}.
        /// </summary>
        public static string WARN_InvalidProjectGuid {
            get {
                return ResourceManager.GetString("WARN_InvalidProjectGuid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Overriding analysis property. Effective value: {0}={1}.
        /// </summary>
        public static string WARN_OverridingAnalysisProperty {
            get {
                return ResourceManager.GetString("WARN_OverridingAnalysisProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to fix Code Analysis ErrorLog file. Please check that VS 2015 Update 1 (or later) is installed..
        /// </summary>
        public static string WARN_SarifFixFail {
            get {
                return ResourceManager.GetString("WARN_SarifFixFail", resourceCulture);
            }
        }
    }
}
