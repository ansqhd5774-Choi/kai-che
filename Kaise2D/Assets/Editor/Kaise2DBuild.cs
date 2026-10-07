using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Kaise2DBuild
{
    public static void BuildAndroid()
    {
        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        camGo.tag = "MainCamera";
        cam.orthographic = true;
        cam.orthographicSize = 5.2f;
        cam.backgroundColor = new Color(0.055f, 0.075f, 0.095f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGo.transform.position = new Vector3(0f, 0f, -10f);

        var player = new GameObject("Player");
        player.AddComponent<Kaise2DPlayer>();

        string scenePath = "Assets/Scenes/Main.unity";
        EditorSceneManager.SaveScene(scene, scenePath);

        PlayerSettings.companyName = "Kaise";
        PlayerSettings.productName = "Kaise 2D R1";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.kaise.reboot2d");
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.bundleVersionCode = 1;

        Directory.CreateDirectory("Builds/Android");
        string apk = Path.GetFullPath("Builds/Android/Kaise2D-R1.apk");

        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = apk,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new System.Exception("Android build failed: " + report.summary.result);

        Debug.Log("KAISE2D_BUILD_PASS " + apk);
    }
}
