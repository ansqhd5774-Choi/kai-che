using UnityEngine;

namespace KaiChe.Prototypes.BumperSeed
{
    public sealed class BumperSeedR21TrajectoryGuide : MonoBehaviour
    {
        private readonly LineRenderer[] _lines = new LineRenderer[3];
        private float _visibleUntil;

        public void Initialize()
        {
            Material material = CreateLineMaterial();

            for (int i = 0; i < _lines.Length; i++)
            {
                GameObject lineObject = new GameObject("R21_Guide_" + i);
                lineObject.transform.SetParent(transform, false);

                LineRenderer line = lineObject.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.widthMultiplier = 0.065f;
                line.numCapVertices = 4;
                line.alignment = LineAlignment.View;
                line.material = material;
                line.enabled = false;

                _lines[i] = line;
            }
        }

        public void ShowChoices(
            Vector2 origin,
            Vector2[] directions,
            float duration)
        {
            if (directions == null || directions.Length < 3)
                return;

            _visibleUntil = Time.unscaledTime + duration;

            for (int i = 0; i < 3; i++)
            {
                LineRenderer line = _lines[i];
                if (line == null)
                    continue;

                Color color = ChoiceColor(i, 0.42f);
                line.startColor = color;
                line.endColor = new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.05f);

                Vector3 start = new Vector3(origin.x, origin.y, -0.05f);
                Vector2 end2 = origin + directions[i].normalized * 2.65f;
                Vector3 end = new Vector3(end2.x, end2.y, -0.05f);

                line.SetPosition(0, start);
                line.SetPosition(1, end);
                line.enabled = true;
            }
        }

        public void HighlightChoice(int choice, float duration)
        {
            _visibleUntil = Mathf.Max(
                _visibleUntil,
                Time.unscaledTime + duration);

            for (int i = 0; i < 3; i++)
            {
                LineRenderer line = _lines[i];
                if (line == null)
                    continue;

                float alpha = i == choice ? 0.95f : 0.12f;
                Color color = ChoiceColor(i, alpha);

                line.startColor = color;
                line.endColor = new Color(
                    color.r,
                    color.g,
                    color.b,
                    i == choice ? 0.25f : 0.02f);
            }
        }

        private void Update()
        {
            if (Time.unscaledTime <= _visibleUntil)
                return;

            for (int i = 0; i < _lines.Length; i++)
            {
                if (_lines[i] != null)
                    _lines[i].enabled = false;
            }
        }

        private static Color ChoiceColor(int choice, float alpha)
        {
            switch (choice)
            {
                case 0:
                    return new Color(0.20f, 0.75f, 1.00f, alpha);
                case 2:
                    return new Color(1.00f, 0.45f, 0.80f, alpha);
                default:
                    return new Color(1.00f, 0.95f, 0.50f, alpha);
            }
        }

        private static Material CreateLineMaterial()
        {
            Material template = Resources.Load<Material>("KaiCheRuntimeLit");
            if (template != null)
                return new Material(template);

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new System.InvalidOperationException(
                    "No compatible shader found for R21 trajectory guide.");

            return new Material(shader);
        }
    }
}
