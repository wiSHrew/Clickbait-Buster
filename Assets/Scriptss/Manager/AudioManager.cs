using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sound effect IDs. Call from anywhere with:  AudioManager.Play(Sfx.VerdictCorrect);
/// </summary>
public enum Sfx
{
    ButtonClick,
    ButtonHover,
    ObjectClick,      // clicking 3D objects in the office (monitor, tablet...)
    ResearchSelect,   // Monitor 2 research-point buttons
    TabletOpen,
    TabletClose,
    VerdictSubmit,    // any verdict pressed
    VerdictCorrect,
    VerdictWrong,
    VerdictWait,
    NewAd,            // next ad appears
    NightStart,
    NightComplete,
    EndingGood,
    EndingNeutral,
    EndingBad
}

/// <summary>
/// Persistent audio controller (survives scene changes).
/// - Per-scene looping music + ambience with crossfades (matches your additive SceneFader).
/// - Same clip in consecutive scenes keeps playing without restarting.
/// - Automatic click/hover sounds for every UI Button (including ones spawned at runtime).
/// - Master volume from PlayerPrefs "Volume" (same key your gamesettings script saves).
///
/// SETUP: build one GameObject with this script, save it as a prefab at
/// Assets/Resources/AudioManager.prefab. It is auto-created at game start in
/// ANY scene, so you can press Play from office/monitor scenes too.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SceneAudio
    {
        [Tooltip("Scene name, e.g. main menu / settings / office / monitor 1 / monitor 2 (case-insensitive)")]
        public string sceneName;
        public AudioClip music;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        public AudioClip ambience;
        [Range(0f, 1f)] public float ambienceVolume = 0.4f;
    }

    [System.Serializable]
    public class SfxEntry
    {
        public Sfx type;
        [Tooltip("One or more clips. If several, one is picked at random.")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.3f)] public float pitchVariation = 0.05f;
    }

    [Header("Per-scene audio")]
    [SerializeField] private List<SceneAudio> scenes = new List<SceneAudio>();
    [SerializeField] private float crossfadeSeconds = 1.0f;

    [Header("Sound effects")]
    [SerializeField] private List<SfxEntry> sfx = new List<SfxEntry>();
    [SerializeField] private int sfxVoices = 8;

    [Header("UI sounds")]
    [SerializeField] private bool autoButtonSounds = true;
    [Tooltip("Play the click sound on EVERY left click, even if no UI Button is detected. Use this if your menus/cursor aren't standard UI Buttons.")]
    [SerializeField] private bool anyLeftClickPlaysSound = false;

    [Header("Debug")]
    [Tooltip("Logs every Play() call and lets you press T to test the ButtonClick sound.")]
    [SerializeField] private bool debugLogs = true;

    // ---------- internals ----------
    private class Channel
    {
        public AudioSource[] src = new AudioSource[2];
        public int active;
        public AudioClip clip;
        public Coroutine routine;
    }

    private Channel music = new Channel();
    private Channel ambience = new Channel();
    private AudioSource[] sfxPool;
    private int sfxNext;

    private Dictionary<string, SceneAudio> sceneMap = new Dictionary<string, SceneAudio>();
    private Dictionary<Sfx, SfxEntry> sfxMap = new Dictionary<Sfx, SfxEntry>();

    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
    private PointerEventData pointerData;
    private Button lastHovered;

    // ---------- startup ----------
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        AudioManager prefab = Resources.Load<AudioManager>("AudioManager");
        if (prefab != null) Instantiate(prefab);
        else Debug.LogWarning("AudioManager prefab not found at Resources/AudioManager. Audio disabled.");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSources();
        BuildMaps();
        ApplyVolume();

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void Start()
    {
        ApplySceneAudio(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void BuildSources()
    {
        for (int i = 0; i < 2; i++)
        {
            music.src[i] = MakeSource("Music" + i, true);
            ambience.src[i] = MakeSource("Ambience" + i, true);
        }

        sfxPool = new AudioSource[Mathf.Max(1, sfxVoices)];
        for (int i = 0; i < sfxPool.Length; i++)
        {
            sfxPool[i] = MakeSource("Sfx" + i, false);
            sfxPool[i].volume = 1f;   // MakeSource starts at 0 (for music fades); SFX must be audible
        }
    }

    private AudioSource MakeSource(string n, bool loop)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform);
        var s = go.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f;   // 2D
        s.volume = 0f;
        return s;
    }

    private void BuildMaps()
    {
        sceneMap.Clear();
        foreach (var s in scenes)
            if (!string.IsNullOrEmpty(s.sceneName)) sceneMap[Normalize(s.sceneName)] = s;

        sfxMap.Clear();
        foreach (var e in sfx) sfxMap[e.type] = e;
    }

    private static string Normalize(string n) => n.Trim().ToLowerInvariant();

    // ---------- volume ----------
    /// <summary>Reads PlayerPrefs "Volume" (0-1) and applies it globally. Call after saving settings.</summary>
    public static void ApplyVolume()
    {
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("Volume", 0.8f));
    }

    /// <summary>Live preview while dragging the settings slider (does not save).</summary>
    public static void SetVolumePreview(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }

    // ---------- scene music ----------
    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplySceneAudio(newScene.name);
    }

    private void ApplySceneAudio(string sceneName)
    {
        sceneMap.TryGetValue(Normalize(sceneName), out SceneAudio data);

        SetLoop(music, data != null ? data.music : null, data != null ? data.musicVolume : 0f);
        SetLoop(ambience, data != null ? data.ambience : null, data != null ? data.ambienceVolume : 0f);
    }

    /// <summary>Crossfades a looping channel to a new clip (or silence if clip is null).</summary>
    private void SetLoop(Channel ch, AudioClip clip, float volume)
    {
        AudioSource current = ch.src[ch.active];

        // Same clip as before: don't restart, just adjust volume.
        if (clip != null && clip == ch.clip && current.isPlaying)
        {
            StartRoutine(ch, FadeVolume(current, volume, crossfadeSeconds));
            return;
        }

        ch.clip = clip;

        if (clip == null)
        {
            StartRoutine(ch, FadeOutAndStop(current, crossfadeSeconds));
            return;
        }

        int next = 1 - ch.active;
        AudioSource incoming = ch.src[next];
        incoming.clip = clip;
        incoming.volume = 0f;
        incoming.Play();
        ch.active = next;
        StartRoutine(ch, Crossfade(current, incoming, volume, crossfadeSeconds));
    }

    private void StartRoutine(Channel ch, IEnumerator r)
    {
        if (ch.routine != null) StopCoroutine(ch.routine);
        ch.routine = StartCoroutine(r);
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float toVol, float dur)
    {
        float fromStart = from.volume, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            from.volume = Mathf.Lerp(fromStart, 0f, k);
            to.volume = Mathf.Lerp(0f, toVol, k);
            yield return null;
        }
        from.volume = 0f;
        from.Stop();
        to.volume = toVol;
    }

    private IEnumerator FadeVolume(AudioSource s, float target, float dur)
    {
        float start = s.volume, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            s.volume = Mathf.Lerp(start, target, Mathf.Clamp01(t / dur));
            yield return null;
        }
        s.volume = target;
    }

    private IEnumerator FadeOutAndStop(AudioSource s, float dur)
    {
        yield return FadeVolume(s, 0f, dur);
        s.Stop();
    }

    // ---------- sound effects ----------
    /// <summary>Null-safe: does nothing if the manager or clip is missing.</summary>
    public static void Play(Sfx type)
    {
        if (Instance != null) Instance.PlayInternal(type);
    }

    private void PlayInternal(Sfx type)
    {
        if (!sfxMap.TryGetValue(type, out SfxEntry e))
        {
            if (debugLogs) Debug.LogWarning("[Audio] No Sfx entry for " + type);
            return;
        }
        if (e.clips == null || e.clips.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("[Audio] " + type + " has no clips");
            return;
        }

        AudioClip clip = e.clips[Random.Range(0, e.clips.Length)];
        if (clip == null)
        {
            if (debugLogs) Debug.LogWarning("[Audio] " + type + " clip slot is empty");
            return;
        }
        if (debugLogs) Debug.Log("[Audio] Playing " + type + " (" + clip.name + ") listenerVolume=" + AudioListener.volume);

        AudioSource s = sfxPool[sfxNext];
        sfxNext = (sfxNext + 1) % sfxPool.Length;

        s.pitch = 1f + Random.Range(-e.pitchVariation, e.pitchVariation);
        s.PlayOneShot(clip, e.volume);
    }

    // ---------- automatic UI button sounds ----------
    private bool warnedNoEventSystem;

    private void Update()
    {
        if (debugLogs && Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("[Audio] T pressed - testing ButtonClick");
            Play(Sfx.ButtonClick);
        }

        if (!autoButtonSounds || Mouse.current == null) return;

        bool pressed = Mouse.current.leftButton.wasPressedThisFrame;

        if (EventSystem.current == null)
        {
            if (pressed && anyLeftClickPlaysSound) Play(Sfx.ButtonClick);
            if (debugLogs && !warnedNoEventSystem)
            {
                warnedNoEventSystem = true;
                Debug.LogWarning("[Audio] No EventSystem in this scene, so UI buttons can't be detected.");
            }
            return;
        }

        Button under = GetButtonUnderPointer();

        // Hover: fires once when the pointer moves onto a new interactable button.
        if (under != lastHovered)
        {
            lastHovered = under;
            if (under != null) Play(Sfx.ButtonHover);
        }

        if (!pressed) return;

        if (debugLogs)
        {
            string top = raycastResults.Count > 0 ? raycastResults[0].gameObject.name : "nothing";
            Debug.Log("[Audio] Left click. UI hits: " + raycastResults.Count + ", top: " + top +
                      ", Button found: " + (under != null));
        }

        if (under != null || anyLeftClickPlaysSound) Play(Sfx.ButtonClick);
    }

    private Button GetButtonUnderPointer()
    {
        if (pointerData == null) pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Mouse.current.position.ReadValue();

        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        if (raycastResults.Count == 0) return null;

        Button b = raycastResults[0].gameObject.GetComponentInParent<Button>();
        return (b != null && b.interactable && b.isActiveAndEnabled) ? b : null;
    }
}