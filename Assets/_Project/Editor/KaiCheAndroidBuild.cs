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
            ConfigurePlayerSettings();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Failed to switch build target to Android.");

            EnsureRuntimeMaterial();
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

        private static void EnsureRuntimeMaterial()
        {
            if (!AssetDatabase.IsValidFolder(RuntimeResourcesFolder))
                AssetDatabase.CreateFolder("Assets/_Project", "Resources");

            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("Built-in runtime shader was not found in the Editor.");

            Material material = AssetDatabase.LoadAssetAtPath<Material>(RuntimeMaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, RuntimeMaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", Color.white);
            else
                material.color = Color.white;

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
