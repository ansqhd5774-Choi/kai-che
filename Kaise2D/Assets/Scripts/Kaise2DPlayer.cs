using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Kaise2DPlayer : MonoBehaviour
{
    enum Facing { Down, DownLeft, Left, UpLeft, Up, UpRight, Right, DownRight }
    enum AnimState { Idle, Walk, Run, Fireball, Damaged, Death }

    SpriteRenderer sr;
    readonly Dictionary<string, Sprite[]> clips = new Dictionary<string, Sprite[]>();
    Facing facing = Facing.Down;
    AnimState state = AnimState.Idle;
    float frameClock;
    int frameIndex;
    Vector2 guiMove;
    bool dead;

    const float MoveSpeed = 3.1f;
    const float RunSpeed = 5.0f;
    const float Cell = 256f;
    const float Ppu = 100f;

    void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 10;
        transform.localScale = Vector3.one * 1.18f;

        foreach (AnimState s in Enum.GetValues(typeof(AnimState)))
            foreach (Facing f in Enum.GetValues(typeof(Facing)))
                LoadClip(s, f);

        sr.sprite = GetClip(AnimState.Idle, Facing.Down)[0];
        CreateGroundMarkers();
    }

    void LoadClip(AnimState s, Facing f)
    {
        string resource = "Hormelz/GirlSample/" + Folder(s) + "/" + FileName(s, f);
        Texture2D tex = Resources.Load<Texture2D>(resource);
        if (tex == null) return;

        int count = Mathf.Max(1, Mathf.FloorToInt(tex.width / Cell));
        Sprite[] frames = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            Rect r = new Rect(i * Cell, 0, Cell, Mathf.Min(Cell, tex.height));
            frames[i] = Sprite.Create(tex, r, new Vector2(0.5f, 0.14f), Ppu, 0, SpriteMeshType.FullRect);
            frames[i].name = s + "_" + f + "_" + i;
        }
        clips[Key(s, f)] = frames;
    }

    void Update()
    {
        if (dead)
        {
            TickAnimation();
            return;
        }

        Vector2 input = guiMove;
        guiMove = Vector2.zero;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) input.x -= 1;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) input.x += 1;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) input.y += 1;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) input.y -= 1;
        input = Vector2.ClampMagnitude(input, 1f);

        if (Input.GetKeyDown(KeyCode.Space)) PlayOneShot(AnimState.Fireball);
        if (Input.GetKeyDown(KeyCode.H)) PlayOneShot(AnimState.Damaged);
        if (Input.GetKeyDown(KeyCode.K)) Kill();

        bool locked = state == AnimState.Fireball || state == AnimState.Damaged;
        if (!locked && input.sqrMagnitude > 0.01f)
        {
            facing = Quantize(input);
            bool run = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            SetState(run ? AnimState.Run : AnimState.Walk);
            transform.position += (Vector3)(input.normalized * (run ? RunSpeed : MoveSpeed) * Time.deltaTime);
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, -8.2f, 8.2f);
            p.y = Mathf.Clamp(p.y, -4.1f, 4.1f);
            transform.position = p;
        }
        else if (!locked && input.sqrMagnitude <= 0.01f)
        {
            SetState(AnimState.Idle);
        }

        TickAnimation();
    }

    void TickAnimation()
    {
        Sprite[] c = GetClip(state, facing);
        if (c.Length == 0) return;
        frameClock += Time.deltaTime;
        float fps = state == AnimState.Idle ? 6f : state == AnimState.Walk ? 9f : state == AnimState.Run ? 12f : 11f;
        if (frameClock < 1f / fps) return;
        frameClock = 0f;
        frameIndex++;

        bool oneShot = state == AnimState.Fireball || state == AnimState.Damaged || state == AnimState.Death;
        if (frameIndex >= c.Length)
        {
            if (state == AnimState.Death) frameIndex = c.Length - 1;
            else if (oneShot) { state = AnimState.Idle; frameIndex = 0; }
            else frameIndex = 0;
        }
        sr.sprite = c[Mathf.Clamp(frameIndex, 0, c.Length - 1)];
    }

    void SetState(AnimState next)
    {
        if (state == next) return;
        state = next;
        frameIndex = 0;
        frameClock = 0f;
    }

    void PlayOneShot(AnimState s)
    {
        if (dead) return;
        SetState(s);
    }

    void Kill()
    {
        dead = true;
        SetState(AnimState.Death);
    }

    Sprite[] GetClip(AnimState s, Facing f)
    {
        if (clips.TryGetValue(Key(s, f), out Sprite[] c) && c.Length > 0) return c;
        if (clips.TryGetValue(Key(AnimState.Idle, f), out c) && c.Length > 0) return c;
        return Array.Empty<Sprite>();
    }

    static string Key(AnimState s, Facing f) => s + ":" + f;
    static string Folder(AnimState s) => s.ToString();

    static string FileName(AnimState s, Facing f)
    {
        string d = f.ToString();
        switch (s)
        {
            case AnimState.Idle: return "GirlSample_ReadyIdle_" + d;
            case AnimState.Walk: return "GirlSample_Walk_" + d;
            case AnimState.Run: return "GirlSample_Run_" + d;
            case AnimState.Fireball: return "GirlSample_Fireball_" + d;
            case AnimState.Damaged: return "GirlSample_Damaged_" + d;
            case AnimState.Death: return "GirlSample_Death_" + d;
            default: return "";
        }
    }

    static Facing Quantize(Vector2 v)
    {
        float a = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        if (a < 0) a += 360f;
        int sector = Mathf.RoundToInt(a / 45f) & 7;
        switch (sector)
        {
            case 0: return Facing.Right;
            case 1: return Facing.UpRight;
            case 2: return Facing.Up;
            case 3: return Facing.UpLeft;
            case 4: return Facing.Left;
            case 5: return Facing.DownLeft;
            case 6: return Facing.Down;
            default: return Facing.DownRight;
        }
    }

    void OnGUI()
    {
        float u = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        u = Mathf.Max(0.75f, u);
        float b = 62f * u;
        float gap = 6f * u;
        float x = 28f * u;
        float y = Screen.height - (b * 3f + gap * 2f + 28f * u);

        GUIStyle dir = new GUIStyle(GUI.skin.button) { fontSize = Mathf.RoundToInt(22f * u) };
        Vector2 m = Vector2.zero;
        if (GUI.RepeatButton(new Rect(x + b + gap, y, b, b), "▲", dir)) m += Vector2.up;
        if (GUI.RepeatButton(new Rect(x, y + b + gap, b, b), "◀", dir)) m += Vector2.left;
        if (GUI.RepeatButton(new Rect(x + b + gap, y + b + gap, b, b), "●", dir)) { }
        if (GUI.RepeatButton(new Rect(x + (b + gap) * 2f, y + b + gap, b, b), "▶", dir)) m += Vector2.right;
        if (GUI.RepeatButton(new Rect(x + b + gap, y + (b + gap) * 2f, b, b), "▼", dir)) m += Vector2.down;
        if (GUI.RepeatButton(new Rect(x, y, b, b), "↖", dir)) m += new Vector2(-1, 1);
        if (GUI.RepeatButton(new Rect(x + (b + gap) * 2f, y, b, b), "↗", dir)) m += new Vector2(1, 1);
        if (GUI.RepeatButton(new Rect(x, y + (b + gap) * 2f, b, b), "↙", dir)) m += new Vector2(-1, -1);
        if (GUI.RepeatButton(new Rect(x + (b + gap) * 2f, y + (b + gap) * 2f, b, b), "↘", dir)) m += new Vector2(1, -1);
        guiMove += Vector2.ClampMagnitude(m, 1f);

        float ax = Screen.width - 110f * u;
        float ay = Screen.height - 250f * u;
        if (GUI.Button(new Rect(ax, ay, 84f * u, 60f * u), "마법")) PlayOneShot(AnimState.Fireball);
        if (GUI.Button(new Rect(ax, ay + 68f * u, 84f * u, 50f * u), "피격")) PlayOneShot(AnimState.Damaged);
        if (GUI.Button(new Rect(ax, ay + 126f * u, 84f * u, 50f * u), "사망")) Kill();

        GUI.Label(new Rect(18f * u, 16f * u, 500f * u, 32f * u), "KAISE 2D R1 · 8-DIRECTION CHARACTER TEST");
        GUI.Label(new Rect(18f * u, 45f * u, 600f * u, 30f * u), "방향: " + facing + "   상태: " + state);
    }

    void CreateGroundMarkers()
    {
        Texture2D t = new Texture2D(1, 1);
        t.SetPixel(0, 0, Color.white);
        t.Apply();
        Sprite s = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1f);

        for (int i = 0; i < 18; i++)
        {
            GameObject g = new GameObject("GroundMarker" + i);
            SpriteRenderer r = g.AddComponent<SpriteRenderer>();
            r.sprite = s;
            r.color = (i % 2 == 0) ? new Color(0.18f,0.22f,0.27f,1f) : new Color(0.14f,0.18f,0.22f,1f);
            r.sortingOrder = -5;
            g.transform.localScale = new Vector3(1.8f, 0.04f, 1f);
            g.transform.position = new Vector3(-8f + (i % 9) * 2f, -3.5f + (i / 9) * 7f, 0f);
        }
    }
}
