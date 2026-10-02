using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KaiChe.EditorTools
{
    public static class KaiCheProjectBootstrap
    {
        private const string ScenesFolder = "Assets/_Project/Scenes";

        [MenuItem("KaiChe/Setup/Create Initial Scenes")]
        public static void CreateInitialScenes()
        {
            EnsureScenesFolder();

            CreateScene("Boot", false);
            CreateScene("Main", false);
            CreateScene("Game", true);
            CreateScene("Town", true);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{ScenesFolder}/Boot.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Main.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Game.unity", true),
                new EditorBuildSettingsScene($"{ScenesFolder}/Town.unity", true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("KAI_CHE_BOOTSTRAP_OK");
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
                AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        }

        private static void CreateScene(string name, bool addLight)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            new GameObject($"{name}Root");

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 1f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.04f, 0.06f);
            cameraObject.AddComponent<AudioListener>();

            if (addLight)
            {
                var lightObject = new GameObject("Directional Light");
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{name}.unity");
        }
    }
}
