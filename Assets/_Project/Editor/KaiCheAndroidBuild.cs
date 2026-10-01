using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KaiChe.EditorTools
{
    public static class KaiCheAndroidBuild
    {
        private const string CompanyName = "KaiChe";
        private const string ProductName = "KaiChe";
        private const string PackageId = "com.kaiche.rpg";
        private const string VersionName = "0.1.0";
        private const int VersionCode = 1;
        private const string ApkPath = "Builds/Android/KaiChe-dev.apk";

        public static void BuildDevelopmentApk()
        {
            ConfigurePlayerSettings();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Failed to switch build target to Android.");

            Directory.CreateDirectory("Builds/Android");

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes in Build Settings.");

            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Android build failed: {report.summary.result}");

            Debug.Log($"KAI_CHE_ANDROID_BUILD_OK:{Path.GetFullPath(ApkPath)}");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
        }
    }
}
