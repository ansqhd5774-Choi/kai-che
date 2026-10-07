using System.Collections;
using UnityEngine;

namespace KaiChe.Prototypes.BumperSeed
{
    public sealed class BumperSeedR20Feedback : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private Camera _camera;
        private Vector3 _cameraHome;
        private AudioSource _audioSource;
        private AudioClip _impactClip;
        private AudioClip _bumperClip;
        private AudioClip _targetClip;

        private float _shakeUntil;
        private float _shakeMagnitude;
        private float _flashUntil;
        private float _flashDuration;
        private float _flashPeakAlpha;
        private Color _flashColor = Color.white;

        public void Initialize(Camera camera)
        {
            _camera = camera;
            if (_camera != null)
                _cameraHome = _camera.transform.localPosition;

            GameObject audioHost = _camera != null ? _camera.gameObject : gameObject;
            _audioSource = audioHost.GetComponent<AudioSource>();
            if (_audioSource == null)
                _audioSource = audioHost.AddComponent<AudioSource>();

            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 1f;

            _impactClip = CreateToneClip("R20_Impact", 0.055f, 240f, 125f, 0.30f);
            _bumperClip = CreateToneClip("R20_BumperCreate", 0.095f, 410f, 250f, 0.34f);
            _targetClip = CreateToneClip("R20_TargetSuccess", 0.16f, 620f, 980f, 0.36f);
        }

        public void PlayWallImpact(Vector2 worldPoint)
        {
            if (_audioSource != null && _impactClip != null)
                _audioSource.PlayOneShot(_impactClip, 0.58f);

            StartShake(0.045f, 0.035f);
            StartFlash(new Color(0.45f, 0.78f, 1f), 0.055f, 0.075f);
        }

        public void PlayBumperCreated(Transform bumper)
        {
            if (_audioSource != null && _bumperClip != null)
                _audioSource.PlayOneShot(_bumperClip, 0.72f);

            StartShake(0.075f, 0.06f);
            StartFlash(new Color(0.20f, 0.72f, 0.95f), 0.08f, 0.11f);

            if (bumper != null)
                StartCoroutine(PulseTransform(bumper, 1.24f, 0.13f));
        }

        public void PlayTargetSuccess(Transform target)
        {
            if (_audioSource != null && _targetClip != null)
                _audioSource.PlayOneShot(_targetClip, 0.82f);

            StartShake(0.11f, 0.09f);
            StartFlash(new Color(0.35f, 1f, 0.48f), 0.12f, 0.15f);

            if (target != null)
                StartCoroutine(PulseTransform(target, 1.30f, 0.18f));
        }

        private void LateUpdate()
        {
            if (_camera == null)
                return;

            if (Time.unscaledTime < _shakeUntil)
            {
                Vector2 offset = Random.insideUnitCircle * _shakeMagnitude;
                _camera.transform.localPosition =
                    _cameraHome + new Vector3(offset.x, offset.y, 0f);
            }
            else
            {
                _camera.transform.localPosition = _cameraHome;
                _shakeMagnitude = 0f;
            }
        }

        private void OnGUI()
        {
            if (Time.unscaledTime >= _flashUntil || _flashDuration <= 0f)
                return;

            float remaining = _flashUntil - Time.unscaledTime;
            float normalized = Mathf.Clamp01(remaining / _flashDuration);
            float alpha = _flashPeakAlpha * normalized;

            Color previous = GUI.color;
            GUI.color = new Color(
                _flashColor.r,
                _flashColor.g,
                _flashColor.b,
                alpha);

            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                Texture2D.whiteTexture);

            GUI.color = previous;
        }

        private void StartShake(float duration, float magnitude)
        {
            float until = Time.unscaledTime + duration;
            if (until > _shakeUntil)
                _shakeUntil = until;

            _shakeMagnitude = Mathf.Max(_shakeMagnitude, magnitude);
        }

        private void StartFlash(Color color, float duration, float peakAlpha)
        {
            _flashColor = color;
            _flashDuration = duration;
            _flashPeakAlpha = peakAlpha;
            _flashUntil = Time.unscaledTime + duration;
        }

        private static IEnumerator PulseTransform(
            Transform target,
            float peakScale,
            float duration)
        {
            if (target == null)
                yield break;

            Vector3 original = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (target == null)
                    yield break;

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pulse = Mathf.Sin(t * Mathf.PI);
                float scale = Mathf.Lerp(1f, peakScale, pulse);
                target.localScale = original * scale;
                yield return null;
            }

            if (target != null)
                target.localScale = original;
        }

        private static AudioClip CreateToneClip(
            string name,
            float duration,
            float startFrequency,
            float endFrequency,
            float amplitude)
        {
            int sampleCount = Mathf.Max(
                1,
                Mathf.CeilToInt(duration * SampleRate));

            float[] samples = new float[sampleCount];
            float phase = 0f;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = sampleCount <= 1
                    ? 1f
                    : i / (float)(sampleCount - 1);

                float frequency = Mathf.Lerp(startFrequency, endFrequency, t);
                phase += (Mathf.PI * 2f * frequency) / SampleRate;

                float envelope = 1f - t;
                envelope *= envelope;

                samples[i] =
                    Mathf.Sin(phase) *
                    amplitude *
                    envelope;
            }

            AudioClip clip = AudioClip.Create(
                name,
                sampleCount,
                1,
                SampleRate,
                false);

            clip.SetData(samples, 0);
            return clip;
        }
    }
}
