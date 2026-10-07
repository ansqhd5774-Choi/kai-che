using System;
using System.IO;
using KaiChe.Prototypes.BumperSeed;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.EditorTools
{
    public static class BumperSeedR21PrototypeBuild
    {
        private const string CompanyName = "KaiChe Labs";
        private const string ProductName = "Bumper Seed R21";
        private const string PackageId = "com.kaiche.bumperseed";
        private const string VersionName = "0.21.0";
        private const int VersionCode = 21;
        private const string ScenePath = "Assets/_Project/Scenes/BumperSeedPrototype.unity";
        private const string ApkPath = "Builds/Android/BumperSeed-R21.apk";

        public static void BuildR21AndroidApk()
        {
            Debug.Log("BUMPER_SEED_R21_BUILD_START");

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                        BuildTargetGroup.Android,
                        BuildTarget.Android))
                {
                    throw new InvalidOperationException(
                        "Failed to switch active build target to Android.");
                }
            }

            ConfigurePlayerSettings();
            CreatePrototypeScene();

            Directory.CreateDirectory("Builds/Android");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("BUMPER_SEED_R21_BUILD_RESULT_" + report.summary.result);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception(
                    "Bumper Seed R21 Android build failed: " +
                    report.summary.result);
            }

            Debug.Log(
                "BUMPER_SEED_R21_BUILD_OK:" +
                Path.GetFullPath(ApkPath));
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.applicationEntry =
                AndroidApplicationEntry.Activity;
            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.Portrait;
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                PackageId);

            EditorUserBuildSettings.buildAppBundle = false;
        }

        private static void CreatePrototypeScene()
        {
            string directory = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            var root = new GameObject("BumperSeedR21Prototype");
            root.AddComponent<BumperSeedPrototype>();

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                throw new InvalidOperationException(
                    "Failed to save Bumper Seed R21 prototype scene.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("BUMPER_SEED_R21_SCENE_READY:" + ScenePath);
        }
    }
}
