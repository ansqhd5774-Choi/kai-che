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

            // R32 V3: 95% silhouette gate against the approved R31 reference.
            // Main correction from V2:
            // - projected crotch target ~52-53% down from crown
            // - substantially longer lower body
            // - narrower shoulder line
            // - smaller head/hair mass
            // - stronger chest -> waist -> pelvis transition

            CreateProfilePart(
                modelRoot,
                "PlayerPelvis3D",
                skin,
                new Vector3(0f, -0.30f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.24f, 0.285f, 0.195f),
                    new Ring(0.08f, 0.330f, 0.225f),
                    new Ring(-0.10f, 0.365f, 0.245f),
                    new Ring(-0.26f, 0.350f, 0.238f),
                    new Ring(-0.40f, 0.245f, 0.190f)
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
                    new Ring(0.34f, 0.285f, 0.192f),
                    new Ring(0.15f, 0.245f, 0.172f),
                    new Ring(-0.10f, 0.225f, 0.165f),
                    new Ring(-0.35f, 0.285f, 0.195f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerChest3D",
                skin,
                new Vector3(0f, 0.91f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.31f, 0.245f, 0.175f),
                    new Ring(0.16f, 0.305f, 0.205f),
                    new Ring(-0.02f, 0.330f, 0.220f),
                    new Ring(-0.23f, 0.315f, 0.212f),
                    new Ring(-0.43f, 0.280f, 0.192f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerSportsBra3D",
                underwear,
                new Vector3(0f, 0.91f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.17f, 0.310f, 0.210f),
                    new Ring(0.03f, 0.338f, 0.224f),
                    new Ring(-0.12f, 0.328f, 0.222f),
                    new Ring(-0.24f, 0.302f, 0.205f)
                },
                materialTemplate);

            // Taper the lower garment toward the leg openings so it reads as
            // briefs rather than the V2 shorts-like block.
            CreateProfilePart(
                modelRoot,
                "PlayerBrief3D",
                underwear,
                new Vector3(0f, -0.30f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.18f, 0.340f, 0.232f),
                    new Ring(0.04f, 0.372f, 0.248f),
                    new Ring(-0.11f, 0.350f, 0.238f),
                    new Ring(-0.25f, 0.245f, 0.190f)
                },
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerNeck3D",
                skinShade,
                new Vector3(0f, 1.35f, 0f),
                Quaternion.identity,
                new[]
                {
                    new Ring(0.15f, 0.087f, 0.083f),
                    new Ring(0.02f, 0.094f, 0.088f),
                    new Ring(-0.14f, 0.112f, 0.100f)
                },
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHead3D",
                skin,
                new Vector3(0f, 1.76f, -0.010f),
                new Vector3(0.160f, 0.205f, 0.170f),
                Quaternion.identity,
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerEarLeft3D",
                skinShade,
                new Vector3(-0.161f, 1.76f, 0f),
                new Vector3(0.022f, 0.047f, 0.019f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEarRight3D",
                skinShade,
                new Vector3(0.161f, 1.76f, 0f),
                new Vector3(0.022f, 0.047f, 0.019f),
                Quaternion.identity,
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerEyeLeft3D",
                eye,
                new Vector3(-0.058f, 1.790f, -0.172f),
                new Vector3(0.026f, 0.014f, 0.009f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerEyeRight3D",
                eye,
                new Vector3(0.058f, 1.790f, -0.172f),
                new Vector3(0.026f, 0.014f, 0.009f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerNose3D",
                skinShade,
                new Vector3(0f, 1.730f, -0.181f),
                new Vector3(0.022f, 0.040f, 0.029f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerMouth3D",
                lip,
                new Vector3(0f, 1.662f, -0.172f),
                new Vector3(0.046f, 0.010f, 0.009f),
                Quaternion.identity,
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHairCap3D",
                hair,
                new Vector3(0f, 1.850f, 0.050f),
                new Vector3(0.173f, 0.155f, 0.165f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHairBack3D",
                hair,
                new Vector3(0f, 1.720f, 0.092f),
                new Vector3(0.168f, 0.185f, 0.112f),
                Quaternion.identity,
                materialTemplate);

            // Narrower shoulder line: V2 shoulder bulbs were too dominant.
            CreateEllipsoidPart(
                modelRoot,
                "PlayerShoulderLeft3D",
                skin,
                new Vector3(-0.330f, 1.075f, 0f),
                new Vector3(0.090f, 0.115f, 0.100f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerShoulderRight3D",
                skin,
                new Vector3(0.330f, 1.075f, 0f),
                new Vector3(0.090f, 0.115f, 0.100f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerUpperArmLeft3D",
                skin,
                new Vector3(-0.350f, 1.045f, 0f),
                Quaternion.Euler(0f, 0f, -5f),
                ArmUpperProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerUpperArmRight3D",
                skin,
                new Vector3(0.350f, 1.045f, 0f),
                Quaternion.Euler(0f, 0f, 5f),
                ArmUpperProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerElbowLeft3D",
                skinShade,
                new Vector3(-0.405f, 0.305f, 0f),
                new Vector3(0.085f, 0.090f, 0.086f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerElbowRight3D",
                skinShade,
                new Vector3(0.405f, 0.305f, 0f),
                new Vector3(0.085f, 0.090f, 0.086f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerForearmLeft3D",
                skin,
                new Vector3(-0.405f, 0.305f, 0f),
                Quaternion.Euler(0f, 0f, -4f),
                ForearmProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerForearmRight3D",
                skin,
                new Vector3(0.405f, 0.305f, 0f),
                Quaternion.Euler(0f, 0f, 4f),
                ForearmProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerHandLeft3D",
                skinShade,
                new Vector3(-0.455f, -0.720f, -0.015f),
                new Vector3(0.080f, 0.165f, 0.060f),
                Quaternion.Euler(0f, 0f, -3f),
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerHandRight3D",
                skinShade,
                new Vector3(0.455f, -0.720f, -0.015f),
                new Vector3(0.080f, 0.165f, 0.060f),
                Quaternion.Euler(0f, 0f, 3f),
                materialTemplate);

            // Lower body is intentionally long in world space to compensate for
            // perspective foreshortening in the fixed gameplay camera.
            CreateProfilePart(
                modelRoot,
                "PlayerThighLeft3D",
                skin,
                new Vector3(-0.185f, -0.670f, 0f),
                Quaternion.Euler(0f, 0f, -0.7f),
                ThighProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerThighRight3D",
                skin,
                new Vector3(0.185f, -0.670f, 0f),
                Quaternion.Euler(0f, 0f, 0.7f),
                ThighProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerKneeLeft3D",
                skinShade,
                new Vector3(-0.195f, -2.020f, -0.012f),
                new Vector3(0.095f, 0.102f, 0.094f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerKneeRight3D",
                skinShade,
                new Vector3(0.195f, -2.020f, -0.012f),
                new Vector3(0.095f, 0.102f, 0.094f),
                Quaternion.identity,
                materialTemplate);

            CreateProfilePart(
                modelRoot,
                "PlayerShinLeft3D",
                skin,
                new Vector3(-0.195f, -2.025f, 0f),
                Quaternion.identity,
                ShinProfile(),
                materialTemplate);
            CreateProfilePart(
                modelRoot,
                "PlayerShinRight3D",
                skin,
                new Vector3(0.195f, -2.025f, 0f),
                Quaternion.identity,
                ShinProfile(),
                materialTemplate);

            CreateEllipsoidPart(
                modelRoot,
                "PlayerFootLeft3D",
                skinShade,
                new Vector3(-0.195f, -3.475f, -0.205f),
                new Vector3(0.125f, 0.125f, 0.300f),
                Quaternion.identity,
                materialTemplate);
            CreateEllipsoidPart(
                modelRoot,
                "PlayerFootRight3D",
                skinShade,
                new Vector3(0.195f, -3.475f, -0.205f),
                new Vector3(0.125f, 0.125f, 0.300f),
                Quaternion.identity,
                materialTemplate);

            Debug.Log("KAI_CHE_R32_PROPORTION_7_5_HEAD_BASE");
            Debug.Log("KAI_CHE_R32_FULL_LIMB_SILHOUETTE_READY");
            Debug.Log("KAI_CHE_R32_PROPORTION_UPGRADE_V3_95_GATE_READY");
        }

        private static Ring[] ArmUpperProfile()
        {
            return new[]
            {
                new Ring(0f, 0.095f, 0.100f),
                new Ring(-0.22f, 0.108f, 0.112f),
                new Ring(-0.48f, 0.100f, 0.104f),
                new Ring(-0.72f, 0.085f, 0.090f),
                new Ring(-0.78f, 0.080f, 0.085f)
            };
        }

        private static Ring[] ForearmProfile()
        {
            return new[]
            {
                new Ring(0f, 0.086f, 0.090f),
                new Ring(-0.24f, 0.098f, 0.102f),
                new Ring(-0.52f, 0.085f, 0.090f),
                new Ring(-0.82f, 0.064f, 0.068f)
            };
        }

        private static Ring[] ThighProfile()
        {
            return new[]
            {
                new Ring(0f, 0.130f, 0.148f),
                new Ring(-0.30f, 0.158f, 0.172f),
                new Ring(-0.68f, 0.150f, 0.166f),
                new Ring(-1.05f, 0.120f, 0.138f),
                new Ring(-1.35f, 0.098f, 0.112f)
            };
        }

        private static Ring[] ShinProfile()
        {
            return new[]
            {
                new Ring(0f, 0.098f, 0.108f),
                new Ring(-0.30f, 0.116f, 0.132f, 0f, 0.012f),
                new Ring(-0.68f, 0.110f, 0.126f, 0f, 0.020f),
                new Ring(-1.05f, 0.082f, 0.096f),
                new Ring(-1.34f, 0.068f, 0.080f)
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
