using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectAegis.Build
{
    public static class BuildScript
    {
        // Build configuration
        private const string BUILD_PATH = "Builds/Android";
        private const string APP_NAME = "ProjectAegisDroneDominion";
        private const string BUNDLE_ID = "com.projectaegis.dronedominion";
        private const string VERSION = "1.0.0";
        private const int VERSION_CODE = 1;
        
        // Keystore configuration (set in Unity Editor)
        private const string KEYSTORE_PATH = "Build/Android/keystore/projectaegis.keystore";
        private const string KEYSTORE_PASSWORD = "YOUR_KEYSTORE_PASSWORD";
        private const string KEY_ALIAS = "projectaegis";
        private const string KEY_PASSWORD = "YOUR_KEY_PASSWORD";

        [MenuItem("Project Aegis/Build/Android/Build AAB Release", false, 1)]
        public static void BuildAndroidAAB()
        {
            Debug.Log("Starting Android AAB Release Build...");
            
            // Configure build settings
            ConfigureAndroidSettings();
            ConfigureIL2CPP();
            ConfigureKeystore();
            
            // Set build path
            string buildPath = Path.Combine(BUILD_PATH, $"{APP_NAME}_{VERSION}.aab");
            
            // Create build options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None // Release build
            };
            
            // Build
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;
            
            // Report results
            switch (summary.result)
            {
                case BuildResult.Succeeded:
                    Debug.Log($"Build succeeded! Size: {summary.totalSize / 1024 / 1024} MB");
                    Debug.Log($"Output: {buildPath}");
                    EditorUtility.RevealInFinder(buildPath);
                    break;
                    
                case BuildResult.Failed:
                    Debug.LogError("Build failed!");
                    break;
                    
                case BuildResult.Cancelled:
                    Debug.LogWarning("Build cancelled!");
                    break;
            }
        }

        [MenuItem("Project Aegis/Build/Android/Build APK Debug", false, 2)]
        public static void BuildAndroidAPKDebug()
        {
            Debug.Log("Starting Android APK Debug Build...");
            
            ConfigureAndroidSettings();
            ConfigureIL2CPP();
            
            // Disable keystore for debug
            PlayerSettings.Android.useCustomKeystore = false;
            
            string buildPath = Path.Combine(BUILD_PATH, $"{APP_NAME}_DEBUG_{VERSION}.apk");
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
            
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Debug build succeeded! Size: {summary.totalSize / 1024 / 1024} MB");
                EditorUtility.RevealInFinder(buildPath);
            }
        }

        [MenuItem("Project Aegis/Build/Android/Configure Settings", false, 3)]
        public static void ConfigureAndroidSettings()
        {
            // Switch to Android platform
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            
            // Basic settings
            PlayerSettings.companyName = "Project Aegis Studios";
            PlayerSettings.productName = "Project Aegis: Drone Dominion";
            PlayerSettings.bundleVersion = VERSION;
            PlayerSettings.Android.bundleVersionCode = VERSION_CODE;
            
            // Package name
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BUNDLE_ID);
            
            // Build settings
            EditorUserBuildSettings.buildAppBundle = true; // AAB format
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            
            // Target API levels
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel34; // Android 14
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26; // Android 8.0
            
            // Architecture
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Scripting backend
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            
            // IL2CPP specific settings
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.SetIl2CppCodeGeneration(BuildTargetGroup.Android, Il2CppCodeGeneration.OptimizeSize);
            
            // Strip engine code
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);
            
            // Other optimizations
            PlayerSettings.Android.useAPKExpansionFiles = false;
            PlayerSettings.Android.forceInternetPermission = true;
            
            Debug.Log("Android settings configured!");
        }

        private static void ConfigureIL2CPP()
        {
            // Ensure IL2CPP is selected
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            
            // C++ Compiler Configuration
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            
            // Code Generation
            PlayerSettings.SetIl2CppCodeGeneration(BuildTargetGroup.Android, Il2CppCodeGeneration.OptimizeSize);
            
            // Additional IL2CPP arguments
            PlayerSettings.SetAdditionalIl2CppArgs("--emit-source-mapping");
            
            Debug.Log("IL2CPP configured for Release build!");
        }

        private static void ConfigureKeystore()
        {
            string fullKeystorePath = Path.Combine(Application.dataPath, "..", KEYSTORE_PATH);
            
            if (File.Exists(fullKeystorePath))
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.Android.keystoreName = fullKeystorePath;
                PlayerSettings.Android.keystorePass = KEYSTORE_PASSWORD;
                PlayerSettings.Android.keyaliasName = KEY_ALIAS;
                PlayerSettings.Android.keyaliasPass = KEY_PASSWORD;
                
                Debug.Log($"Keystore configured: {fullKeystorePath}");
            }
            else
            {
                Debug.LogWarning($"Keystore not found at: {fullKeystorePath}");
                Debug.LogWarning("Building with debug keystore...");
                PlayerSettings.Android.useCustomKeystore = false;
            }
        }

        private static string[] GetBuildScenes()
        {
            // Get all enabled scenes from build settings
            var scenes = new System.Collections.Generic.List<string>();
            
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            
            if (scenes.Count == 0)
            {
                Debug.LogError("No scenes enabled in Build Settings!");
                throw new InvalidOperationException("No scenes in build settings");
            }
            
            return scenes.ToArray();
        }

        [MenuItem("Project Aegis/Build/Android/Validate Build", false, 4)]
        public static void ValidateBuild()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== Android Build Validation ===");
            report.AppendLine();
            
            // Platform
            report.AppendLine($"Platform: {EditorUserBuildSettings.activeBuildTarget}");
            report.AppendLine($"Build App Bundle: {EditorUserBuildSettings.buildAppBundle}");
            report.AppendLine();
            
            // Player Settings
            report.AppendLine("Player Settings:");
            report.AppendLine($"  Bundle ID: {PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)}");
            report.AppendLine($"  Version: {PlayerSettings.bundleVersion}");
            report.AppendLine($"  Version Code: {PlayerSettings.Android.bundleVersionCode}");
            report.AppendLine();
            
            // Scripting
            report.AppendLine("Scripting:");
            report.AppendLine($"  Backend: {PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android)}");
            report.AppendLine($"  IL2CPP Config: {PlayerSettings.GetIl2CppCompilerConfiguration(BuildTargetGroup.Android)}");
            report.AppendLine();
            
            // Architecture
            report.AppendLine("Architecture:");
            report.AppendLine($"  Target: {PlayerSettings.Android.targetArchitectures}");
            report.AppendLine();
            
            // SDK
            report.AppendLine("SDK Versions:");
            report.AppendLine($"  Min SDK: {PlayerSettings.Android.minSdkVersion}");
            report.AppendLine($"  Target SDK: {PlayerSettings.Android.targetSdkVersion}");
            report.AppendLine();
            
            // Keystore
            report.AppendLine("Keystore:");
            report.AppendLine($"  Using Custom: {PlayerSettings.Android.useCustomKeystore}");
            if (PlayerSettings.Android.useCustomKeystore)
            {
                report.AppendLine($"  Path: {PlayerSettings.Android.keystoreName}");
                report.AppendLine($"  Alias: {PlayerSettings.Android.keyaliasName}");
            }
            report.AppendLine();
            
            // Scenes
            report.AppendLine("Scenes in Build:");
            foreach (var scene in GetBuildScenes())
            {
                report.AppendLine($"  - {scene}");
            }
            
            Debug.Log(report.ToString());
        }
    }
}
