using System.Collections.Generic;
using KaiChe.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace KaiChe.Gameplay.Survivor
{
    internal static class RuntimeBattlefieldEnvironment
    {
        private static readonly Dictionary<int, Material> MaterialCache =
            new Dictionary<int, Material>();

        public static void Build(
            Transform parent,
            Material materialTemplate,
            Vector2 townEntrancePosition)
        {
            if (parent == null)
                return;

            var root = new GameObject("R17_BattlefieldEnvironment");
            root.transform.SetParent(parent, false);

            CreatePart(
                PrimitiveType.Cube,
                root.transform,
                "GroundMass",
                new Color(0.075f, 0.070f, 0.052f),
                new Vector3(0f, -0.35f, 0f),
                new Vector3(72f, 0.70f, 72f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                root.transform,
                "MainRoad",
                new Color(0.155f, 0.132f, 0.084f),
                new Vector3(0f, 0.19f, 0f),
                new Vector3(5.15f, 0.38f, 72f),
                Quaternion.identity,
                materialTemplate);

            CreateRoadEdge(root.transform, materialTemplate, -2.68f);
            CreateRoadEdge(root.transform, materialTemplate, 2.68f);
            CreateRoadSurface(root.transform, materialTemplate);
            CreateTownGate(
                root.transform,
                materialTemplate,
                townEntrancePosition);
            CreateRuinField(root.transform, materialTemplate);
            CreateForestLine(root.transform, materialTemplate, -1);
            CreateForestLine(root.transform, materialTemplate, 1);

            Debug.Log("KAI_CHE_R17_3D_ENVIRONMENT_ACTIVE");
            Debug.Log("KAI_CHE_R20_BATTLEFIELD_GROUND_PLANE_ACTIVE");
        }

        private static void CreateRoadEdge(
            Transform parent,
            Material materialTemplate,
            float x)
        {
            CreatePart(
                PrimitiveType.Cube,
                parent,
                x < 0f ? "RoadEdgeLeft3D" : "RoadEdgeRight3D",
                new Color(0.31f, 0.24f, 0.12f),
                new Vector3(x, 0.24f, 0f),
                new Vector3(0.22f, 0.48f, 72f),
                Quaternion.identity,
                materialTemplate);
        }

        private static void CreateRoadSurface(
            Transform parent,
            Material materialTemplate)
        {
            for (int i = -18; i <= 18; i++)
            {
                float y = i * 2.25f;
                float width = i % 3 == 0 ? 4.35f : 3.85f;
                float depth = 0.12f + (Mathf.Abs(i) % 3) * 0.025f;

                CreatePart(
                    PrimitiveType.Cube,
                    parent,
                    "RoadSlab3D_" + i,
                    i % 2 == 0
                        ? new Color(0.205f, 0.170f, 0.105f)
                        : new Color(0.135f, 0.116f, 0.080f),
                    new Vector3(0f, 0.42f, y),
                    new Vector3(width, 0.075f, depth),
                    Quaternion.Euler(0f, (i % 5 - 2) * 0.45f, 0f),
                    materialTemplate);
            }
        }

        private static void CreateTownGate(
            Transform parent,
            Material materialTemplate,
            Vector2 entrance)
        {
            var gate = new GameObject("TownGate3D");
            gate.transform.SetParent(parent, false);
            gate.transform.localPosition =
                RuntimeWorldSpace.ToWorld(entrance, 1.18f);

            Color stone = new Color(0.33f, 0.27f, 0.17f);
            Color trim = new Color(0.56f, 0.43f, 0.20f);

            CreatePart(
                PrimitiveType.Cube,
                gate.transform,
                "GateLeftPillar",
                stone,
                new Vector3(-1.02f, 0f, 0f),
                new Vector3(0.38f, 2.35f, 0.62f),
                Quaternion.identity,
                materialTemplate);
            CreatePart(
                PrimitiveType.Cube,
                gate.transform,
                "GateRightPillar",
                stone,
                new Vector3(1.02f, 0f, 0f),
                new Vector3(0.38f, 2.35f, 0.62f),
                Quaternion.identity,
                materialTemplate);
            CreatePart(
                PrimitiveType.Cube,
                gate.transform,
                "GateLintel",
                trim,
                new Vector3(0f, 1.05f, -0.02f),
                new Vector3(2.45f, 0.34f, 0.70f),
                Quaternion.identity,
                materialTemplate);
            CreatePart(
                PrimitiveType.Sphere,
                gate.transform,
                "GatePortalCore",
                new Color(0.24f, 0.56f, 0.30f, 0.72f),
                new Vector3(0f, 0f, -0.16f),
                new Vector3(1.12f, 0.72f, 0.18f),
                Quaternion.identity,
                materialTemplate);
        }

        private static void CreateRuinField(
            Transform parent,
            Material materialTemplate)
        {
            for (int i = 0; i < 16; i++)
            {
                int side = i % 2 == 0 ? -1 : 1;
                float x = side * (6.8f + (i % 5) * 2.25f);
                float y = -26f + i * 3.55f;
                float height = 0.80f + (i % 4) * 0.34f;
                float width = 0.72f + (i % 3) * 0.22f;

                CreatePart(
                    PrimitiveType.Cube,
                    parent,
                    "RuinBlock3D_" + i,
                    new Color(
                        0.18f + (i % 3) * 0.018f,
                        0.17f + (i % 2) * 0.012f,
                        0.14f,
                        1f),
                    new Vector3(x, height * 0.5f, y),
                    new Vector3(width, height, 0.85f),
                    Quaternion.Euler(0f, side * (4f + (i % 4) * 2.5f), 0f),
                    materialTemplate);
            }

            for (int i = 0; i < 22; i++)
            {
                float x = Mathf.Sin(i * 2.17f) * 27f;
                float y = Mathf.Cos(i * 1.61f) * 27f;
                if (Mathf.Abs(x) < 4.2f)
                    x += x < 0f ? -5.5f : 5.5f;

                float size = 0.24f + (i % 5) * 0.08f;
                CreatePart(
                    i % 3 == 0 ? PrimitiveType.Sphere : PrimitiveType.Cube,
                    parent,
                    "FieldRock3D_" + i,
                    new Color(0.22f, 0.20f, 0.16f),
                    new Vector3(x, size * 0.41f, y),
                    new Vector3(
                        size * 1.45f,
                        size * 0.82f,
                        size * 1.05f),
                    Quaternion.Euler(
                        (i * 13f) % 25f,
                        (i * 29f) % 35f,
                        (i * 37f) % 360f),
                    materialTemplate);
            }
        }

        private static void CreateForestLine(
            Transform parent,
            Material materialTemplate,
            int side)
        {
            for (int i = 0; i < 9; i++)
            {
                float x = side * (13.5f + (i % 3) * 3.7f);
                float y = -25f + i * 6.2f + side * (i % 2) * 1.1f;
                float scale = 0.86f + (i % 4) * 0.11f;

                var tree = new GameObject(
                    (side < 0 ? "TreeLeft3D_" : "TreeRight3D_") + i);
                tree.transform.SetParent(parent, false);
                tree.transform.localPosition =
                    RuntimeWorldSpace.ToWorld(
                        new Vector2(x, y),
                        1.12f);
                tree.transform.localScale = Vector3.one * scale;

                CreatePart(
                    PrimitiveType.Cylinder,
                    tree.transform,
                    "Trunk",
                    new Color(0.19f, 0.12f, 0.065f),
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0.24f, 1.12f, 0.24f),
                    Quaternion.identity,
                    materialTemplate);
                CreatePart(
                    PrimitiveType.Sphere,
                    tree.transform,
                    "CrownLow",
                    new Color(0.085f, 0.19f, 0.075f),
                    new Vector3(0f, 0.88f, -0.18f),
                    new Vector3(1.10f, 0.82f, 0.62f),
                    Quaternion.identity,
                    materialTemplate);
                CreatePart(
                    PrimitiveType.Sphere,
                    tree.transform,
                    "CrownHigh",
                    new Color(0.11f, 0.24f, 0.09f),
                    new Vector3(0.14f * side, 1.42f, -0.08f),
                    new Vector3(0.82f, 0.72f, 0.54f),
                    Quaternion.identity,
                    materialTemplate);
            }
        }

        private static GameObject CreatePart(
            PrimitiveType type,
            Transform parent,
            string name,
            Color color,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material materialTemplate)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                if (materialTemplate != null)
                    renderer.sharedMaterial =
                        GetSharedTintedMaterial(materialTemplate, color);
            }

            return part;
        }

        private static Material GetSharedTintedMaterial(
            Material template,
            Color color)
        {
            Color32 rgba = color;
            int key = template.GetEntityId().GetHashCode();
            unchecked
            {
                key = (key * 397) ^ rgba.r;
                key = (key * 397) ^ rgba.g;
                key = (key * 397) ^ rgba.b;
                key = (key * 397) ^ rgba.a;
            }

            if (MaterialCache.TryGetValue(key, out Material cached)
                && cached != null)
                return cached;

            var material = new Material(template)
            {
                name = "KaiCheR17Environment_" + key,
                hideFlags = HideFlags.DontSave
            };

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);
            else
                material.color = color;

            MaterialCache[key] = material;
            return material;
        }
    }
}
