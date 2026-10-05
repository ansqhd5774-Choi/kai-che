using System.Collections.Generic;
using KaiChe.Core.Content;
using UnityEngine;
using UnityEngine.Rendering;

namespace KaiChe.Gameplay.Survivor
{
    internal static class RuntimeCombatVisualFactory
    {
        private static readonly Dictionary<int, Material> MaterialCache =
            new Dictionary<int, Material>();

        public static GameObject CreateWarrior(
            Transform parent,
            Material materialTemplate,
            float baseScale,
            out Transform weaponRoot)
        {
            Debug.Log("KAI_CHE_R15_3D_VISUALS_ACTIVE");

            var root = new GameObject("Player");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localScale = Vector3.one * baseScale;

            var modelObject = new GameObject("PlayerModelRoot");
            modelObject.transform.SetParent(root.transform, false);
            Transform modelRoot = modelObject.transform;

            CreatePart(
                PrimitiveType.Cylinder,
                root.transform,
                "PlayerShadow3D",
                new Color(0.02f, 0.018f, 0.015f, 0.78f),
                new Vector3(0f, -0.73f, 0.22f),
                new Vector3(0.82f, 0.035f, 0.42f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Capsule,
                modelRoot,
                "PlayerBody3D",
                new Color(0.18f, 0.21f, 0.24f),
                new Vector3(0f, -0.03f, 0f),
                new Vector3(0.62f, 0.74f, 0.38f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "PlayerChest3D",
                new Color(0.58f, 0.43f, 0.17f),
                new Vector3(0f, 0.05f, -0.28f),
                new Vector3(0.66f, 0.45f, 0.16f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                modelRoot,
                "PlayerHelmet3D",
                new Color(0.34f, 0.36f, 0.35f),
                new Vector3(0f, 0.72f, -0.02f),
                new Vector3(0.43f, 0.40f, 0.38f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "PlayerVisor3D",
                new Color(0.08f, 0.10f, 0.12f),
                new Vector3(0f, 0.71f, -0.38f),
                new Vector3(0.36f, 0.08f, 0.08f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "PlayerCape3D",
                new Color(0.28f, 0.06f, 0.04f),
                new Vector3(0f, -0.08f, 0.30f),
                new Vector3(0.72f, 0.92f, 0.08f),
                Quaternion.Euler(4f, 0f, 0f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                modelRoot,
                "PlayerShoulderLeft3D",
                new Color(0.42f, 0.34f, 0.20f),
                new Vector3(-0.56f, 0.24f, -0.02f),
                new Vector3(0.26f, 0.24f, 0.28f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                modelRoot,
                "PlayerShoulderRight3D",
                new Color(0.42f, 0.34f, 0.20f),
                new Vector3(0.56f, 0.24f, -0.02f),
                new Vector3(0.26f, 0.24f, 0.28f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "PlayerShield3D",
                new Color(0.14f, 0.17f, 0.18f),
                new Vector3(-0.78f, -0.05f, -0.18f),
                new Vector3(0.34f, 0.66f, 0.12f),
                Quaternion.Euler(0f, 0f, -8f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "PlayerShieldBoss3D",
                new Color(0.66f, 0.49f, 0.18f),
                new Vector3(-0.78f, 0.04f, -0.31f),
                new Vector3(0.18f, 0.18f, 0.06f),
                Quaternion.Euler(0f, 0f, 45f),
                materialTemplate);

            var weapon = new GameObject("PlayerWeapon");
            weapon.transform.SetParent(modelRoot, false);
            weaponRoot = weapon.transform;

            CreatePart(
                PrimitiveType.Cube,
                weaponRoot,
                "PlayerSwordBlade3D",
                new Color(0.84f, 0.84f, 0.75f),
                new Vector3(0.72f, 0.18f, -0.18f),
                new Vector3(0.10f, 1.08f, 0.09f),
                Quaternion.Euler(0f, 0f, -24f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                weaponRoot,
                "PlayerSwordGuard3D",
                new Color(0.66f, 0.47f, 0.16f),
                new Vector3(0.48f, -0.35f, -0.16f),
                new Vector3(0.44f, 0.09f, 0.12f),
                Quaternion.Euler(0f, 0f, -24f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Cylinder,
                weaponRoot,
                "PlayerSwordGrip3D",
                new Color(0.11f, 0.065f, 0.035f),
                new Vector3(0.37f, -0.53f, -0.14f),
                new Vector3(0.09f, 0.24f, 0.09f),
                Quaternion.Euler(0f, 0f, -24f),
                materialTemplate);

            RuntimeCombatVisualSlot slot =
                root.AddComponent<RuntimeCombatVisualSlot>();
            slot.Configure(
                "player.warrior",
                modelRoot,
                weaponRoot,
                true);

            return root;
        }

        public static GameObject CreateEnemy(
            Transform parent,
            Material materialTemplate,
            MonsterDefinition definition,
            Vector3 position)
        {
            if (definition == null)
                return null;

            string id = definition.Id;
            bool isRunner = id == ContentIds.RunnerMonster;
            bool isBrute = id == ContentIds.BruteMonster;
            bool isBoss = id == ContentIds.BossMonster;

            Color bodyColor = isRunner
                ? new Color(0.08f, 0.34f, 0.38f)
                : isBrute
                    ? new Color(0.32f, 0.25f, 0.15f)
                    : isBoss
                        ? new Color(0.28f, 0.07f, 0.42f)
                        : new Color(0.31f, 0.07f, 0.055f);

            Color armorColor = isRunner
                ? new Color(0.18f, 0.62f, 0.62f)
                : isBrute
                    ? new Color(0.56f, 0.45f, 0.24f)
                    : isBoss
                        ? new Color(0.60f, 0.16f, 0.76f)
                        : new Color(0.50f, 0.13f, 0.08f);

            Color accentColor = isRunner
                ? new Color(0.30f, 1f, 0.86f)
                : isBrute
                    ? new Color(1f, 0.74f, 0.10f)
                    : isBoss
                        ? new Color(1f, 0.34f, 0.96f)
                        : new Color(1f, 0.30f, 0.05f);

            var root = new GameObject("Enemy_" + id);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            var modelObject = new GameObject("EnemyModelRoot");
            modelObject.transform.SetParent(root.transform, false);
            Transform modelRoot = modelObject.transform;

            CreatePart(
                PrimitiveType.Cylinder,
                root.transform,
                "EnemyShadow3D",
                new Color(0.015f, 0.010f, 0.008f, 0.70f),
                new Vector3(0f, -0.72f, 0.22f),
                new Vector3(isBoss ? 0.95f : 0.70f, 0.035f, isBoss ? 0.52f : 0.36f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Capsule,
                modelRoot,
                "EnemyBody3D",
                bodyColor,
                new Vector3(0f, -0.05f, 0f),
                new Vector3(isBrute ? 0.78f : 0.62f, isBoss ? 0.92f : 0.72f, isBoss ? 0.56f : 0.42f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "EnemyChest3D",
                armorColor,
                new Vector3(0f, 0.02f, -0.34f),
                new Vector3(isBrute ? 0.82f : 0.62f, isBrute ? 0.36f : 0.42f, 0.14f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                modelRoot,
                "EnemyHead3D",
                isBoss ? new Color(0.12f, 0.03f, 0.18f) : new Color(0.12f, 0.11f, 0.10f),
                new Vector3(0f, isBoss ? 0.82f : 0.68f, -0.02f),
                new Vector3(isBoss ? 0.48f : 0.38f, isBoss ? 0.44f : 0.36f, 0.38f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Capsule,
                modelRoot,
                "EnemyArmLeft3D",
                bodyColor,
                new Vector3(-0.56f, 0.02f, -0.02f),
                new Vector3(0.18f, 0.48f, 0.18f),
                Quaternion.Euler(0f, 0f, 18f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Capsule,
                modelRoot,
                "EnemyArmRight3D",
                bodyColor,
                new Vector3(0.56f, 0.02f, -0.02f),
                new Vector3(0.18f, 0.48f, 0.18f),
                Quaternion.Euler(0f, 0f, -18f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "EnemyHornLeft3D",
                armorColor,
                new Vector3(-0.23f, isBoss ? 1.14f : 0.95f, -0.04f),
                new Vector3(0.10f, isBoss ? 0.48f : 0.34f, 0.10f),
                Quaternion.Euler(0f, 0f, 28f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Cube,
                modelRoot,
                "EnemyHornRight3D",
                armorColor,
                new Vector3(0.23f, isBoss ? 1.14f : 0.95f, -0.04f),
                new Vector3(0.10f, isBoss ? 0.48f : 0.34f, 0.10f),
                Quaternion.Euler(0f, 0f, -28f),
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                modelRoot,
                "EnemyEyeLeft3D",
                accentColor,
                new Vector3(-0.11f, isBoss ? 0.84f : 0.70f, -0.38f),
                new Vector3(0.07f, 0.055f, 0.045f),
                Quaternion.identity,
                materialTemplate);

            CreatePart(
                PrimitiveType.Sphere,
                modelRoot,
                "EnemyEyeRight3D",
                accentColor,
                new Vector3(0.11f, isBoss ? 0.84f : 0.70f, -0.38f),
                new Vector3(0.07f, 0.055f, 0.045f),
                Quaternion.identity,
                materialTemplate);

            if (isRunner)
            {
                CreatePart(
                    PrimitiveType.Cube,
                    modelRoot,
                    "EnemyRunnerMark3D",
                    accentColor,
                    new Vector3(0f, 0.15f, -0.46f),
                    new Vector3(0.10f, 0.55f, 0.06f),
                    Quaternion.identity,
                    materialTemplate);
            }
            else if (isBrute)
            {
                CreatePart(
                    PrimitiveType.Cube,
                    modelRoot,
                    "EnemyBruteArmor3D",
                    armorColor,
                    new Vector3(0f, 0.12f, -0.46f),
                    new Vector3(0.86f, 0.25f, 0.08f),
                    Quaternion.identity,
                    materialTemplate);
            }
            else if (isBoss)
            {
                CreatePart(
                    PrimitiveType.Sphere,
                    modelRoot,
                    "EnemyBossCore3D",
                    accentColor,
                    new Vector3(0f, 0.06f, -0.53f),
                    new Vector3(0.30f, 0.30f, 0.12f),
                    Quaternion.identity,
                    materialTemplate);

                CreatePart(
                    PrimitiveType.Cube,
                    modelRoot,
                    "EnemyBossCrown3D",
                    new Color(0.82f, 0.46f, 1f),
                    new Vector3(0f, 1.28f, -0.05f),
                    new Vector3(0.88f, 0.12f, 0.12f),
                    Quaternion.identity,
                    materialTemplate);
            }

            RuntimeCombatVisualSlot slot =
                root.AddComponent<RuntimeCombatVisualSlot>();
            slot.Configure(
                id,
                modelRoot,
                null,
                true);

            return root;
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
                name = "KaiCheRuntime3D_" + key,
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
