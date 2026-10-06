using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace KaiChe.Gameplay.Survivor
{
    internal static class RuntimeHumanBlockoutBuilder
    {
        private const int DefaultSides = 16;
        private const int EllipsoidSlices = 18;
        private const int EllipsoidStacks = 10;

        private static readonly Dictionary<int, Material> MaterialCache =
            new Dictionary<int, Material>();

        private struct Ring
        {
            public float Y;
            public float RadiusX;
            public float RadiusZ;
            public float OffsetX;
            public float OffsetZ;

            public Ring(
                float y,
                float radiusX,
                float radiusZ,
                float offsetX = 0f,
                float offsetZ = 0f)
            {
                Y = y;
                RadiusX = radiusX;
                RadiusZ = radiusZ;
                OffsetX = offsetX;
                OffsetZ = offsetZ;
            }
        }

        public static void Build(
            Transform modelRoot,
            Material materialTemplate)
        {
            if (modelRoot == null)
                return;

            Color skin = new Color(0.76f, 0.57f, 0.46f);
            Color skinShade = new Color(0.67f, 0.48f, 0.39f);
            Color underwear = new Color(0.14f, 0.16f, 0.19f);
            Color hair = new Color(0.075f, 0.045f, 0.032f);
            Color eye = new Color(0.055f, 0.038f, 0.028f);
            Color lip = new Color(0.48f, 0.17f, 0.17f);

            // R32 proportion upgrade V2.
            // The game camera foreshortens upright characters, so the runtime
            // blockout intentionally uses a slightly smaller head and longer
            // lower body than the literal construction guide. The target is
            // the R31 adult-human silhouette at gameplay distance.

            CreateProfilePart(
                modelRoot,
                "PlayerPelvis3D",
                skin,
                new Vector3(0f, -0.25f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.27f, 0.30f, 0.20f),
                    new Ring(0.10f, 0.36f, 0.24f),
                    new Ring(-0.10f, 0.42f, 0.27f),
                    new Ring(-0.28f, 0.39f, 0.26f),
                    new Ring(-0.42f, 0.27f, 0.21f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerAbdomen3D",
                skin,
                new Vector3(0f, 0.28f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.30f, 0.30f, 0.195f),
                    new Ring(0.12f, 0.265f, 0.180f),
                    new Ring(-0.12f, 0.245f, 0.175f),
                    new Ring(-0.32f, 0.295f, 0.198f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerChest3D",
                skin,
                new Vector3(0f, 0.92f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.32f, 0.275f, 0.180f),
                    new Ring(0.18f, 0.345f, 0.215f),
                    new Ring(0.00f, 0.370f, 0.228f),
                    new Ring(-0.20f, 0.345f, 0.216f),
                    new Ring(-0.40f, 0.300f, 0.198f)
                },
                materialTemplate);

            // Base garments stay simple during blockout.
            CreateProfilePart(
                modelRoot,
                "PlayerSportsBra3D",
                underwear,
                new Vector3(0f, 0.91f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.20f, 0.350f, 0.222f),
                    new Ring(0.06f, 0.378f, 0.238f),
                    new Ring(-0.10f, 0.368f, 0.236f),
                    new Ring(-0.22f, 0.340f, 0.222f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerBrief3D",
                underwear,
                new Vector3(0f, -0.25f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.22f, 0.365f, 0.245f),
                    new Ring(0.05f, 0.425f, 0.275f),
                    new Ring(-0.12f, 0.415f, 0.272f),
                    new Ring(-0.30f, 0.350f, 0.248f)
                },
                materialTemplate);

            // Longer neck and a smaller head counter the top-down camera.
            CreateProfilePart(
                modelRoot,
                "PlayerNeck3D",
                skinShade,
                new Vector3(0f, 1.36f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.20f, 0.095f, 0.090f),
                    new Ring(0.05f, 0.105f, 0.095f),
                    new Ring(-0.15f, 0.125f, 0.108f)
                },
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHead3D",
                skin,
                new Vector3(0f, 1.79f, -0.012f),
                new Vector3(0.180f, 0.225f, 0.188f),
                Quaternion.identity,
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerEarLeft3D",
                skinShade,
                new Vector3(-0.181f, 1.79f, 0f),
                new Vector3(0.026f, 0.052f, 0.021f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEarRight3D",
                skinShade,
                new Vector3(0.181f, 1.79f, 0f),
                new Vector3(0.026f, 0.052f, 0.021f),
                Quaternion.identity,
                materialTemplate);

            // Minimal readable face landmarks only. R33-R35 own the final face.
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEyeLeft3D",
                eye,
                new Vector3(-0.066f, 1.820f, -0.190f),
                new Vector3(0.030f, 0.017f, 0.010f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEyeRight3D",
                eye,
                new Vector3(0.066f, 1.820f, -0.190f),
                new Vector3(0.030f, 0.017f, 0.010f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerNose3D",
                skinShade,
                new Vector3(0f, 1.755f, -0.201f),
                new Vector3(0.025f, 0.045f, 0.032f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerMouth3D",
                lip,
                new Vector3(0f, 1.680f, -0.190f),
                new Vector3(0.052f, 0.012f, 0.010f),
                Quaternion.identity,
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHairCap3D",
                hair,
                new Vector3(0f, 1.885f, 0.060f),
                new Vector3(0.194f, 0.170f, 0.182f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHairBack3D",
                hair,
                new Vector3(0f, 1.745f, 0.105f),
                new Vector3(0.190f, 0.205f, 0.125f),
                Quaternion.identity,
                materialTemplate);

            // Shoulder line: wider than the waist but not heroic.
            CreateEllipsoidPart(
                modelRoot,
                "PlayerShoulderLeft3D",
                skin,
                new Vector3(-0.420f, 1.105f, 0f),
                new Vector3(0.130f, 0.142f, 0.128f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerShoulderRight3D",
                skin,
                new Vector3(0.420f, 1.105f, 0f),
                new Vector3(0.130f, 0.142f, 0.128f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerUpperArmLeft3D",
                skin,
                new Vector3(-0.430f, 1.095f, 0f),
                Quaternion.Euler(0f, 0f, -8f),
                ArmUpperProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerUpperArmRight3D",
                skin,
                new Vector3(0.430f, 1.095f, 0f),
                Quaternion.Euler(0f, 0f, 8f),
                ArmUpperProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerElbowLeft3D",
                skinShade,
                new Vector3(-0.535f, 0.355f, 0f),
                new Vector3(0.100f, 0.100f, 0.100f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerElbowRight3D",
                skinShade,
                new Vector3(0.535f, 0.355f, 0f),
                new Vector3(0.100f, 0.100f, 0.100f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerForearmLeft3D",
                skin,
                new Vector3(-0.535f, 0.355f, 0f),
                Quaternion.Euler(0f, 0f, -6f),
                ForearmProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerForearmRight3D",
                skin,
                new Vector3(0.535f, 0.355f, 0f),
                Quaternion.Euler(0f, 0f, 6f),
                ForearmProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHandLeft3D",
                skinShade,
                new Vector3(-0.610f, -0.315f, -0.015f),
                new Vector3(0.098f, 0.165f, 0.072f),
                Quaternion.Euler(0f, 0f, -4f),
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHandRight3D",
                skinShade,
                new Vector3(0.610f, -0.315f, -0.015f),
                new Vector3(0.098f, 0.165f, 0.072f),
                Quaternion.Euler(0f, 0f, 4f),
                materialTemplate);

            // Long lower body is deliberate: it restores adult proportions
            // after projection through the fixed gameplay camera.
            CreateProfilePart(
                modelRoot,
                "PlayerThighLeft3D",
                skin,
                new Vector3(-0.220f, -0.555f, 0f),
                Quaternion.Euler(0f, 0f, -1.0f),
                ThighProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerThighRight3D",
                skin,
                new Vector3(0.220f, -0.555f, 0f),
                Quaternion.Euler(0f, 0f, 1.0f),
                ThighProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerKneeLeft3D",
                skinShade,
                new Vector3(-0.230f, -1.515f, -0.012f),
                new Vector3(0.112f, 0.112f, 0.108f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerKneeRight3D",
                skinShade,
                new Vector3(0.230f, -1.515f, -0.012f),
                new Vector3(0.112f, 0.112f, 0.108f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerShinLeft3D",
                skin,
                new Vector3(-0.230f, -1.520f, 0f),
                Quaternion.identity,
                ShinProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerShinRight3D",
                skin,
                new Vector3(0.230f, -1.520f, 0f),
                Quaternion.identity,
                ShinProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerFootLeft3D",
                skinShade,
                new Vector3(-0.230f, -2.360f, -0.185f),
                new Vector3(0.150f, 0.100f, 0.330f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerFootRight3D",
                skinShade,
                new Vector3(0.230f, -2.360f, -0.185f),
                new Vector3(0.150f, 0.100f, 0.330f),
                Quaternion.identity,
                materialTemplate);

            Debug.Log("KAI_CHE_R32_PROPORTION_7_5_HEAD_BASE");
            Debug.Log("KAI_CHE_R32_FULL_LIMB_SILHOUETTE_READY");
            Debug.Log("KAI_CHE_R32_PROPORTION_UPGRADE_V2_READY");
        }

        private static Ring[] ArmUpperProfile()
        {
            return new[]
            {
                new Ring(0f, 0.118f, 0.118f),
                new Ring(-0.20f, 0.128f, 0.128f),
                new Ring(-0.43f, 0.116f, 0.118f),
                new Ring(-0.67f, 0.100f, 0.103f),
                new Ring(-0.75f, 0.094f, 0.098f)
            };
        }

        private static Ring[] ForearmProfile()
        {
            return new[]
            {
                new Ring(0f, 0.100f, 0.104f),
                new Ring(-0.18f, 0.112f, 0.116f),
                new Ring(-0.40f, 0.095f, 0.098f),
                new Ring(-0.62f, 0.072f, 0.075f)
            };
        }

        private static Ring[] ThighProfile()
        {
            return new[]
            {
                new Ring(0f, 0.142f, 0.158f),
                new Ring(-0.24f, 0.175f, 0.185f),
                new Ring(-0.52f, 0.164f, 0.175f),
                new Ring(-0.80f, 0.134f, 0.148f),
                new Ring(-0.96f, 0.112f, 0.123f)
            };
        }

        private static Ring[] ShinProfile()
        {
            return new[]
            {
                new Ring(0f, 0.110f, 0.118f),
                new Ring(-0.22f, 0.130f, 0.142f, 0f, 0.012f),
                new Ring(-0.47f, 0.122f, 0.136f, 0f, 0.022f),
                new Ring(-0.70f, 0.092f, 0.104f),
                new Ring(-0.80f, 0.078f, 0.088f)
            };
        }

        private static GameObject CreateProfilePart(
            Transform parent,
            string name,
            Color color,
            Vector3 localPosition,
            Quaternion localRotation,
            Ring[] rings,
            Material materialTemplate,
            int sides = DefaultSides)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;

            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = GetSharedTintedMaterial(
                materialTemplate,
                color);

            var vertices = new List<Vector3>(rings.Length * sides + 2);
            var triangles = new List<int>((rings.Length - 1) * sides * 6 + sides * 6);

            for (int i = 0; i < rings.Length; i++)
            {
                Ring ring = rings[i];
                for (int j = 0; j < sides; j++)
                {
                    float angle = Mathf.PI * 2f * j / sides;
                    vertices.Add(
                        new Vector3(
                            ring.OffsetX + Mathf.Cos(angle) * ring.RadiusX,
                            ring.Y,
                            ring.OffsetZ + Mathf.Sin(angle) * ring.RadiusZ));
                }
            }

            for (int i = 0; i < rings.Length - 1; i++)
            {
                int top = i * sides;
                int bottom = (i + 1) * sides;

                for (int j = 0; j < sides; j++)
                {
                    int next = (j + 1) % sides;
                    int a = top + j;
                    int b = top + next;
                    int c = bottom + j;
                    int d = bottom + next;

                    triangles.Add(a);
                    triangles.Add(d);
                    triangles.Add(c);

                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(d);
                }
            }

            int topCenter = vertices.Count;
            Ring topRing = rings[0];
            vertices.Add(
                new Vector3(
                    topRing.OffsetX,
                    topRing.Y,
                    topRing.OffsetZ));

            int bottomCenter = vertices.Count;
            Ring bottomRing = rings[rings.Length - 1];
            vertices.Add(
                new Vector3(
                    bottomRing.OffsetX,
                    bottomRing.Y,
                    bottomRing.OffsetZ));

            for (int j = 0; j < sides; j++)
            {
                int next = (j + 1) % sides;

                triangles.Add(topCenter);
                triangles.Add(next);
                triangles.Add(j);

                int bottomStart = (rings.Length - 1) * sides;
                triangles.Add(bottomCenter);
                triangles.Add(bottomStart + j);
                triangles.Add(bottomStart + next);
            }

            var mesh = new Mesh
            {
                name = name + "_R32Mesh",
                hideFlags = HideFlags.DontSave
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;

            return go;
        }

        private static GameObject CreateEllipsoidPart(
            Transform parent,
            string name,
            Color color,
            Vector3 localPosition,
            Vector3 radii,
            Quaternion localRotation,
            Material materialTemplate)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;

            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = GetSharedTintedMaterial(
                materialTemplate,
                color);

            int rowWidth = EllipsoidSlices + 1;
            var vertices = new List<Vector3>(
                (EllipsoidStacks + 1) * rowWidth);
            var triangles = new List<int>(
                EllipsoidStacks * EllipsoidSlices * 6);

            for (int lat = 0; lat <= EllipsoidStacks; lat++)
            {
                float theta = Mathf.PI * lat / EllipsoidStacks;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                for (int lon = 0; lon <= EllipsoidSlices; lon++)
                {
                    float phi = Mathf.PI * 2f * lon / EllipsoidSlices;
                    vertices.Add(
                        new Vector3(
                            radii.x * sinTheta * Mathf.Cos(phi),
                            radii.y * cosTheta,
                            radii.z * sinTheta * Mathf.Sin(phi)));
                }
            }

            for (int lat = 0; lat < EllipsoidStacks; lat++)
            {
                for (int lon = 0; lon < EllipsoidSlices; lon++)
                {
                    int a = lat * rowWidth + lon;
                    int b = a + 1;
                    int c = (lat + 1) * rowWidth + lon;
                    int d = c + 1;

                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);

                    triangles.Add(b);
                    triangles.Add(d);
                    triangles.Add(c);
                }
            }

            var mesh = new Mesh
            {
                name = name + "_R32Mesh",
                hideFlags = HideFlags.DontSave
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            filter.sharedMesh = mesh;

            return go;
        }

        private static Material GetSharedTintedMaterial(
            Material template,
            Color color)
        {
            if (template == null)
                return null;

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
                name = "KaiCheR32Blockout_" + key,
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
