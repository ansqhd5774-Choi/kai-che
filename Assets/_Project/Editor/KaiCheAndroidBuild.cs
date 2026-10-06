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
        private const string RuntimeResourcesFolder = "Assets/_Project/Resources";
        private const string RuntimeMaterialPath = RuntimeResourcesFolder + "/KaiCheRuntimeLit.mat";

        public static void BuildDevelopmentApk()
        {
            Debug.Log("KAI_CHE_BUILD_STAGE:START");
            Debug.Log("KAI_CHE_BUILD_STAGE:CONFIGURE_PLAYER_SETTINGS_BEGIN");
            ConfigurePlayerSettings();
            Debug.Log("KAI_CHE_BUILD_STAGE:CONFIGURE_PLAYER_SETTINGS_OK");

            Debug.Log("KAI_CHE_BUILD_STAGE:SWITCH_ANDROID_BEGIN");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                        BuildTargetGroup.Android, BuildTarget.Android))
                    throw new InvalidOperationException("Failed to switch build target to Android.");
                Debug.Log("KAI_CHE_BUILD_STAGE:SWITCH_ANDROID_CHANGED");
            }
            else
            {
                Debug.Log("KAI_CHE_BUILD_STAGE:SWITCH_ANDROID_SKIPPED_ALREADY_ACTIVE");
            }
            Debug.Log("KAI_CHE_BUILD_STAGE:SWITCH_ANDROID_OK");

            Debug.Log("KAI_CHE_BUILD_STAGE:ENSURE_RUNTIME_MATERIAL_BEGIN");
            EnsureRuntimeMaterial();
            Debug.Log("KAI_CHE_BUILD_STAGE:ENSURE_RUNTIME_MATERIAL_OK");
            Directory.CreateDirectory("Builds/Android");

            string[] scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scenes in Build Settings.");

            Debug.Log($"KAI_CHE_BUILD_STAGE:SCENES_OK:{scenes.Length}");
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            Debug.Log("KAI_CHE_BUILD_STAGE:BUILD_PLAYER_BEGIN");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"KAI_CHE_BUILD_STAGE:BUILD_PLAYER_END:{report.summary.result}");
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception($"Android build failed: {report.summary.result}");

            Debug.Log($"KAI_CHE_ANDROID_BUILD_OK:{Path.GetFullPath(ApkPath)}");
        }

        private static void EnsureRuntimeMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath);

            if (material != null)
            {
                Debug.Log("KAI_CHE_BUILD_STAGE:RUNTIME_MATERIAL_REUSED");
                return;
            }

            if (!AssetDatabase.IsValidFolder(RuntimeResourcesFolder))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException(
                    "Built-in runtime shader was not found in the Editor.");

            material = new Material(shader);
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            else
                material.color = Color.white;

            AssetDatabase.CreateAsset(material, RuntimeMaterialPath);
            AssetDatabase.SaveAssets();
            Debug.Log("KAI_CHE_BUILD_STAGE:RUNTIME_MATERIAL_CREATED");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = VersionName;
            PlayerSettings.Android.bundleVersionCode = VersionCode;
            PlayerSettings.Android.applicationEntry =
                AndroidApplicationEntry.Activity;
            Debug.Log("KAI_CHE_R22_ANDROID_ACTIVITY_ENTRY_ACTIVE");
            PlayerSettings.stripEngineCode = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
        }
    }
}
