using System.Collections.Generic;
using KaiChe.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace KaiChe.Gameplay.Adventure
{
    internal static class RuntimeTownEnvironment
    {
        private static readonly Dictionary<int, Material> MaterialCache =
            new Dictionary<int, Material>();

        public static void Build(
            Transform parent,
            Material materialTemplate,
            Vector2 innPosition,
            Vector2 apothecaryPosition,
            Vector2 workshopPosition,
            Vector2 hallPosition,
            Vector2 fieldGatePosition)
        {
            if (parent == null)
                return;

            var root = new GameObject("R18_TownEnvironment");
            root.transform.SetParent(parent, false);

            CreatePart(
                PrimitiveType.Cube,
                root.transform,
                "TownGround3D",
                new Color(0.15f, 0.17f, 0.11f),
                new Vector3(0f, -0.35f, 0f),
                new Vector3(14.5f, 0.70f, 17.5f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                root.transform,
                "TownPlaza3D",
                new Color(0.31f, 0.28f, 0.20f),
                new Vector3(0f, 0.17f, 0f),
                new Vector3(6.8f, 0.34f, 5.5f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                root.transform,
                "FieldRoad3D",
                new Color(0.30f, 0.25f, 0.17f),
                new Vector3(0f, 0.31f, 4.45f),
                new Vector3(2.2f, 0.18f, 4.5f),
                Quaternion.identity,
                materialTemplate);

            CreateBuilding(
                root.transform,
                materialTemplate,
                "Inn3D",
                innPosition,
                new Color(0.48f, 0.25f, 0.12f),
                new Color(0.25f, 0.10f, 0.055f));
            CreateBuilding(
                root.transform,
                materialTemplate,
                "Apothecary3D",
                apothecaryPosition,
                new Color(0.18f, 0.36f, 0.21f),
                new Color(0.07f, 0.18f, 0.09f));
            CreateBuilding(
                root.transform,
                materialTemplate,
                "Workshop3D",
                workshopPosition,
                new Color(0.29f, 0.29f, 0.31f),
                new Color(0.12f, 0.12f, 0.13f));
            CreateBuilding(
                root.transform,
                materialTemplate,
                "Hall3D",
                hallPosition,
                new Color(0.36f, 0.27f, 0.15f),
                new Color(0.18f, 0.11f, 0.05f));

            CreateNpc(
                root.transform,
                materialTemplate,
                "Innkeeper3D",
                innPosition + new Vector2(0f, -1.25f),
                new Color(0.70f, 0.52f, 0.28f));
            CreateNpc(
                root.transform,
                materialTemplate,
                "ApothecaryNpc3D",
                apothecaryPosition + new Vector2(0f, -1.25f),
                new Color(0.34f, 0.62f, 0.36f));

            CreateGate(
                root.transform,
                materialTemplate,
                fieldGatePosition);
            CreateTownProps(root.transform, materialTemplate);

            Debug.Log("KAI_CHE_R18_3D_TOWN_ACTIVE");
            Debug.Log("KAI_CHE_R20_TOWN_GROUND_PLANE_ACTIVE");
        }

        private static void CreateBuilding(
            Transform parent,
            Material materialTemplate,
            string name,
            Vector2 position,
            Color wallColor,
            Color roofColor)
        {
            var building = new GameObject(name);
            building.transform.SetParent(parent, false);
            building.transform.localPosition =
                RuntimeWorldSpace.ToWorld(position, 0.81f);

            CreatePart(
                PrimitiveType.Cube,
                building.transform,
                name + "_Body",
                wallColor,
                Vector3.zero,
                new Vector3(2.25f, 1.62f, 0.78f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                building.transform,
                name + "_Roof",
                roofColor,
                new Vector3(0f, 0.97f, -0.02f),
                new Vector3(2.65f, 0.40f, 0.92f),
                Quaternion.Euler(0f, 0f, 2f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                building.transform,
                name + "_Door",
                new Color(0.13f, 0.075f, 0.035f),
                new Vector3(0f, -0.45f, -0.43f),
                new Vector3(0.48f, 0.70f, 0.10f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                building.transform,
                name + "_WindowLeft",
                new Color(0.78f, 0.68f, 0.30f),
                new Vector3(-0.68f, 0.18f, -0.44f),
                new Vector3(0.30f, 0.30f, 0.08f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                building.transform,
                name + "_WindowRight",
                new Color(0.78f, 0.68f, 0.30f),
                new Vector3(0.68f, 0.18f, -0.44f),
                new Vector3(0.30f, 0.30f, 0.08f),
                Quaternion.identity,
                materialTemplate);
        }

        private static void CreateNpc(
            Transform parent,
            Material materialTemplate,
            string name,
            Vector2 position,
            Color bodyColor)
        {
            var npc = new GameObject(name);
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition =
                RuntimeWorldSpace.ToWorld(position, 0.54f);

            CreatePart(
                PrimitiveType.Capsule,
                npc.transform,
                "Body",
                bodyColor,
                Vector3.zero,
                new Vector3(0.36f, 0.54f, 0.28f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                npc.transform,
                "Head",
                new Color(0.74f, 0.62f, 0.48f),
                new Vector3(0f, 0.53f, -0.03f),
                new Vector3(0.30f, 0.30f, 0.26f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                npc.transform,
                "Apron",
                new Color(
                    Mathf.Clamp01(bodyColor.r * 0.62f),
                    Mathf.Clamp01(bodyColor.g * 0.62f),
                    Mathf.Clamp01(bodyColor.b * 0.62f)),
                new Vector3(0f, -0.05f, -0.27f),
                new Vector3(0.28f, 0.42f, 0.08f),
                Quaternion.identity,
                materialTemplate);
        }

        private static void CreateGate(
            Transform parent,
            Material materialTemplate,
            Vector2 position)
        {
            var gate = new GameObject("FieldGate3D");
            gate.transform.SetParent(parent, false);
            gate.transform.localPosition =
                RuntimeWorldSpace.ToWorld(position, 1.03f);

            Color wood = new Color(0.40f, 0.29f, 0.14f);
            Color trim = new Color(0.62f, 0.46f, 0.20f);

            CreatePart(
                PrimitiveType.Cube,
                gate.transform,
                "GateLeft",
                wood,
                new Vector3(-1.02f, 0f, 0f),
                new Vector3(0.30f, 2.05f, 0.56f),
                Quaternion.identity,
                materialTemplate);
            CreatePart(
                PrimitiveType.Cube,
                gate.transform,
                "GateRight",
                wood,
                new Vector3(1.02f, 0f, 0f),
                new Vector3(0.30f, 2.05f, 0.56f),
                Quaternion.identity,
                materialTemplate);
            CreatePart(
                PrimitiveType.Cube,
                gate.transform,
                "GateTop",
                trim,
                new Vector3(0f, 0.92f, -0.02f),
                new Vector3(2.35f, 0.30f, 0.66f),
                Quaternion.identity,
                materialTemplate);
        }

        private static void CreateTownProps(
            Transform parent,
            Material materialTemplate)
        {
            Vector2[] lampPositions =
            {
                new Vector2(-2.25f, 3.0f),
                new Vector2(2.25f, 3.0f),
                new Vector2(-2.25f, -3.2f),
                new Vector2(2.25f, -3.2f)
            };

            for (int i = 0; i < lampPositions.Length; i++)
            {
                Vector2 position = lampPositions[i];
                var lamp = new GameObject("TownLamp3D_" + i);
                lamp.transform.SetParent(parent, false);
                lamp.transform.localPosition =
                    RuntimeWorldSpace.ToWorld(position, 0.92f);

                CreatePart(
                    PrimitiveType.Cylinder,
                    lamp.transform,
                    "Post",
                    new Color(0.14f, 0.10f, 0.06f),
                    Vector3.zero,
                    new Vector3(0.12f, 0.92f, 0.12f),
                    Quaternion.identity,
                    materialTemplate);

                CreatePart(
                    PrimitiveType.Sphere,
                    lamp.transform,
                    "Light",
                    new Color(1f, 0.72f, 0.22f),
                    new Vector3(0f, 0.86f, -0.04f),
                    new Vector3(0.25f, 0.25f, 0.18f),
                    Quaternion.identity,
                    materialTemplate);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = side * (5.35f - i * 0.22f);
                    float y = -4.8f + i * 4.6f;

                    var tree = new GameObject(
                        (side < 0 ? "TownTreeLeft3D_" : "TownTreeRight3D_")
                        + i);
                    tree.transform.SetParent(parent, false);
                    tree.transform.localPosition =
                        RuntimeWorldSpace.ToWorld(
                            new Vector2(x, y),
                            0.95f);

                    CreatePart(
                        PrimitiveType.Cylinder,
                        tree.transform,
                        "Trunk",
                        new Color(0.18f, 0.11f, 0.055f),
                        Vector3.zero,
                        new Vector3(0.22f, 0.95f, 0.22f),
                        Quaternion.identity,
                        materialTemplate);

                    CreatePart(
                        PrimitiveType.Sphere,
                        tree.transform,
                        "Crown",
                        new Color(0.08f, 0.22f, 0.075f),
                        new Vector3(0f, 0.92f, -0.08f),
                        new Vector3(0.82f, 0.72f, 0.58f),
                        Quaternion.identity,
                        materialTemplate);
                }
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
            part.transform.localScale = localScale;
            part.transform.localRotation = localRotation;

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
                name = "KaiCheR18Town_" + key,
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
