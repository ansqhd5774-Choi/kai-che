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

            // R32 blockout target:
            // approximately 7.3-7.5 head units from crown to sole.
            // The purpose is proportion and silhouette, not tertiary detail.
            CreateProfilePart(
                modelRoot,
                "PlayerPelvis3D",
                skin,
                new Vector3(0f, -0.13f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.24f, 0.31f, 0.21f),
                    new Ring(0.09f, 0.36f, 0.24f),
                    new Ring(-0.08f, 0.39f, 0.26f),
                    new Ring(-0.23f, 0.36f, 0.25f),
                    new Ring(-0.31f, 0.25f, 0.20f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerAbdomen3D",
                skin,
                new Vector3(0f, 0.25f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.29f, 0.31f, 0.20f),
                    new Ring(0.10f, 0.285f, 0.185f),
                    new Ring(-0.10f, 0.27f, 0.18f),
                    new Ring(-0.28f, 0.30f, 0.20f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerChest3D",
                skin,
                new Vector3(0f, 0.75f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.33f, 0.27f, 0.18f),
                    new Ring(0.19f, 0.34f, 0.215f),
                    new Ring(0.02f, 0.36f, 0.23f),
                    new Ring(-0.16f, 0.34f, 0.22f),
                    new Ring(-0.31f, 0.30f, 0.20f)
                },
                materialTemplate);

            // Opaque base garments remain simple so later equipment can replace them.
            CreateProfilePart(
                modelRoot,
                "PlayerSportsBra3D",
                underwear,
                new Vector3(0f, 0.75f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.20f, 0.345f, 0.225f),
                    new Ring(0.07f, 0.372f, 0.238f),
                    new Ring(-0.09f, 0.36f, 0.235f),
                    new Ring(-0.22f, 0.335f, 0.222f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerBrief3D",
                underwear,
                new Vector3(0f, -0.13f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.18f, 0.37f, 0.25f),
                    new Ring(0.04f, 0.402f, 0.27f),
                    new Ring(-0.11f, 0.39f, 0.266f),
                    new Ring(-0.24f, 0.34f, 0.245f)
                },
                materialTemplate);

            // Neck and head establish the head-unit used by the body blockout.
            CreateProfilePart(
                modelRoot,
                "PlayerNeck3D",
                skinShade,
                new Vector3(0f, 1.14f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.14f, 0.105f, 0.10f),
                    new Ring(0.02f, 0.115f, 0.105f),
                    new Ring(-0.12f, 0.13f, 0.115f)
                },
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHead3D",
                skin,
                new Vector3(0f, 1.51f, -0.01f),
                new Vector3(0.215f, 0.255f, 0.225f),
                Quaternion.identity,
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerEarLeft3D",
                skinShade,
                new Vector3(-0.215f, 1.51f, 0f),
                new Vector3(0.030f, 0.060f, 0.024f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEarRight3D",
                skinShade,
                new Vector3(0.215f, 1.51f, 0f),
                new Vector3(0.030f, 0.060f, 0.024f),
                Quaternion.identity,
                materialTemplate);

            // R32 keeps only readable facial landmarks; R33-R35 refine anatomy/detail.
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEyeLeft3D",
                eye,
                new Vector3(-0.080f, 1.545f, -0.226f),
                new Vector3(0.036f, 0.020f, 0.012f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEyeRight3D",
                eye,
                new Vector3(0.080f, 1.545f, -0.226f),
                new Vector3(0.036f, 0.020f, 0.012f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerNose3D",
                skinShade,
                new Vector3(0f, 1.470f, -0.238f),
                new Vector3(0.030f, 0.052f, 0.038f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerMouth3D",
                lip,
                new Vector3(0f, 1.385f, -0.226f),
                new Vector3(0.065f, 0.014f, 0.012f),
                Quaternion.identity,
                materialTemplate);

            // Short hair masses; still blockout geometry, not final hair cards/strands.
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHairCap3D",
                hair,
                new Vector3(0f, 1.610f, 0.075f),
                new Vector3(0.226f, 0.205f, 0.205f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHairBack3D",
                hair,
                new Vector3(0f, 1.465f, 0.125f),
                new Vector3(0.220f, 0.235f, 0.145f),
                Quaternion.identity,
                materialTemplate);

            // Shoulders and arms: relaxed A-pose proportions with shoulder/elbow/wrist landmarks.
            CreateEllipsoidPart(
                modelRoot,
                "PlayerShoulderLeft3D",
                skin,
                new Vector3(-0.385f, 0.965f, 0f),
                new Vector3(0.135f, 0.145f, 0.135f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerShoulderRight3D",
                skin,
                new Vector3(0.385f, 0.965f, 0f),
                new Vector3(0.135f, 0.145f, 0.135f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerUpperArmLeft3D",
                skin,
                new Vector3(-0.395f, 0.955f, 0f),
                Quaternion.Euler(0f, 0f, -10f),
                ArmUpperProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerUpperArmRight3D",
                skin,
                new Vector3(0.395f, 0.955f, 0f),
                Quaternion.Euler(0f, 0f, 10f),
                ArmUpperProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerElbowLeft3D",
                skinShade,
                new Vector3(-0.505f, 0.345f, 0f),
                new Vector3(0.105f, 0.105f, 0.105f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerElbowRight3D",
                skinShade,
                new Vector3(0.505f, 0.345f, 0f),
                new Vector3(0.105f, 0.105f, 0.105f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerForearmLeft3D",
                skin,
                new Vector3(-0.505f, 0.345f, 0f),
                Quaternion.Euler(0f, 0f, -7f),
                ForearmProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerForearmRight3D",
                skin,
                new Vector3(0.505f, 0.345f, 0f),
                Quaternion.Euler(0f, 0f, 7f),
                ForearmProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHandLeft3D",
                skinShade,
                new Vector3(-0.575f, -0.205f, -0.015f),
                new Vector3(0.085f, 0.145f, 0.065f),
                Quaternion.Euler(0f, 0f, -5f),
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHandRight3D",
                skinShade,
                new Vector3(0.575f, -0.205f, -0.015f),
                new Vector3(0.085f, 0.145f, 0.065f),
                Quaternion.Euler(0f, 0f, 5f),
                materialTemplate);

            // Legs: clear hip -> thigh -> knee -> calf -> ankle -> foot silhouette.
            CreateProfilePart(
                modelRoot,
                "PlayerThighLeft3D",
                skin,
                new Vector3(-0.205f, -0.335f, 0f),
                Quaternion.Euler(0f, 0f, -1.5f),
                ThighProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerThighRight3D",
                skin,
                new Vector3(0.205f, -0.335f, 0f),
                Quaternion.Euler(0f, 0f, 1.5f),
                ThighProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerKneeLeft3D",
                skinShade,
                new Vector3(-0.222f, -1.075f, -0.012f),
                new Vector3(0.120f, 0.120f, 0.115f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerKneeRight3D",
                skinShade,
                new Vector3(0.222f, -1.075f, -0.012f),
                new Vector3(0.120f, 0.120f, 0.115f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerShinLeft3D",
                skin,
                new Vector3(-0.222f, -1.085f, 0f),
                Quaternion.identity,
                ShinProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerShinRight3D",
                skin,
                new Vector3(0.222f, -1.085f, 0f),
                Quaternion.identity,
                ShinProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerFootLeft3D",
                skinShade,
                new Vector3(-0.222f, -1.825f, -0.155f),
                new Vector3(0.135f, 0.095f, 0.285f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerFootRight3D",
                skinShade,
                new Vector3(0.222f, -1.825f, -0.155f),
                new Vector3(0.135f, 0.095f, 0.285f),
                Quaternion.identity,
                materialTemplate);

            Debug.Log("KAI_CHE_R32_PROPORTION_7_5_HEAD_BASE");
            Debug.Log("KAI_CHE_R32_FULL_LIMB_SILHOUETTE_READY");
        }

        private static Ring[] ArmUpperProfile()
        {
            return new[]
            {
                new Ring(0f, 0.125f, 0.125f),
                new Ring(-0.17f, 0.135f, 0.135f),
                new Ring(-0.38f, 0.118f, 0.120f),
                new Ring(-0.58f, 0.102f, 0.105f),
                new Ring(-0.63f, 0.098f, 0.100f)
            };
        }

        private static Ring[] ForearmProfile()
        {
            return new[]
            {
                new Ring(0f, 0.105f, 0.108f),
                new Ring(-0.15f, 0.118f, 0.120f),
                new Ring(-0.34f, 0.100f, 0.102f),
                new Ring(-0.50f, 0.075f, 0.078f)
            };
        }

        private static Ring[] ThighProfile()
        {
            return new[]
            {
                new Ring(0f, 0.145f, 0.160f),
                new Ring(-0.18f, 0.172f, 0.180f),
                new Ring(-0.42f, 0.160f, 0.170f),
                new Ring(-0.66f, 0.130f, 0.145f),
                new Ring(-0.74f, 0.115f, 0.125f)
            };
        }

        private static Ring[] ShinProfile()
        {
            return new[]
            {
                new Ring(0f, 0.115f, 0.122f),
                new Ring(-0.18f, 0.130f, 0.140f, 0f, 0.012f),
                new Ring(-0.36f, 0.120f, 0.132f, 0f, 0.020f),
                new Ring(-0.58f, 0.095f, 0.105f),
                new Ring(-0.68f, 0.080f, 0.090f)
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
            int key = template.GetInstanceID();
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
