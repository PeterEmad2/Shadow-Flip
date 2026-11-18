using System.Collections.Generic;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Plays puzzle SFX, block-sliding audio, and crossfades Light/Shadow Realm background music.
    /// Automatically ensures an AudioListener, PuzzleAudioManager, and walking audio exist at runtime.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PuzzleAudioManager : MonoBehaviour
    {
        public static PuzzleAudioManager Instance { get; private set; }

        [Header("SFX Clips")]
        public AudioClip flipLightClip;
        public AudioClip flipShadowClip;
        public AudioClip flipBlockedClip;
        public AudioClip mirrorRotateClip;
        public AudioClip leverPullClip;
        public AudioClip sensorOnClip;
        public AudioClip sensorOffClip;
        public AudioClip platePressClip;
        public AudioClip buttonClickClip;
        public AudioClip doorOpenClip;
        public AudioClip levelWinClip;
        public AudioClip blockPushClip;
        public AudioClip monsterWarnClip;
        public AudioClip monsterSpawnClip;
        public AudioClip monsterCatchClip;
        public AudioClip portalWarpClip;
        public AudioClip echoRecordClip;
        public AudioClip monsterPetrifyClip;

        [Header("Realm Music")]
        public AudioClip bgmLightClip;
        public AudioClip bgmShadowClip;

        [Header("Volumes")]
        [Range(0f, 1f)] public float sfxVolume = 0.85f;
        [Range(0f, 1f)] public float bgmVolume = 0.35f;
        public float crossfadeSpeed = 3.5f;

        private AudioSource sfxSource;
        private AudioSource lightBgmSource;
        private AudioSource shadowBgmSource;
        private AudioSource blockPushSource;
        private logic cachedLogic;
        private bool lastDarkState;
        private bool isBlockPushing = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureAudioInScene()
        {
            PuzzleAudioManager mgr = FindFirstObjectByType<PuzzleAudioManager>();
            if (mgr == null)
            {
                GameObject go = new GameObject("PuzzleAudioManager");
                mgr = go.AddComponent<PuzzleAudioManager>();
            }

            if (FindFirstObjectByType<AudioListener>() == null)
            {
                Camera targetCam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
                if (targetCam != null)
                {
                    targetCam.gameObject.AddComponent<AudioListener>();
                }
                else
                {
                    mgr.gameObject.AddComponent<AudioListener>();
                }
            }

            MusicShuffler shuffler = FindFirstObjectByType<MusicShuffler>();
            if (shuffler != null)
            {
                if (shuffler.walking == null)
                {
                    AudioSource walkSrc = shuffler.GetComponent<AudioSource>();
                    if (walkSrc == null) walkSrc = shuffler.gameObject.AddComponent<AudioSource>();
                    walkSrc.playOnAwake = false;
                    walkSrc.volume = 0.55f;
                    shuffler.walking = walkSrc;
                }

                if (shuffler.songs == null || shuffler.songs.Length == 0)
                {
                    List<AudioClip> walkClips = new List<AudioClip>();
                    AudioClip w1 = Resources.Load<AudioClip>("SFX/walking_1");
                    AudioClip w2 = Resources.Load<AudioClip>("SFX/walking_2");
                    if (w1 != null) walkClips.Add(w1);
                    if (w2 != null) walkClips.Add(w2);
                    if (walkClips.Count > 0) shuffler.songs = walkClips.ToArray();
                }
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = GetComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;

            LoadFallbackClips();
            SetupLoopingSources();
        }

        void OnEnable()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (cachedLogic != null)
            {
                lastDarkState = cachedLogic.isDarkActive;
                cachedLogic.OnWorldFlipped += HandleWorldFlipped;
            }
        }

        void OnDisable()
        {
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
            if (blockPushSource != null && blockPushSource.isPlaying)
            {
                blockPushSource.Stop();
            }
        }

        void Update()
        {
            if (cachedLogic == null)
            {
                cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            }

            bool paused = Time.timeScale == 0f;
            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;

            float targetLight = (!paused && !isDark) ? bgmVolume : 0f;
            float targetShadow = (!paused && isDark) ? bgmVolume : 0f;

            if (lightBgmSource != null)
            {
                lightBgmSource.volume = Mathf.MoveTowards(lightBgmSource.volume, targetLight, Time.unscaledDeltaTime * crossfadeSpeed);
            }
            if (shadowBgmSource != null)
            {
                shadowBgmSource.volume = Mathf.MoveTowards(shadowBgmSource.volume, targetShadow, Time.unscaledDeltaTime * crossfadeSpeed);
            }

            if (paused && blockPushSource != null && blockPushSource.isPlaying)
            {
                blockPushSource.Pause();
            }
            else if (!paused && isBlockPushing && blockPushSource != null && !blockPushSource.isPlaying && blockPushSource.clip != null)
            {
                blockPushSource.Play();
            }
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            if (isDarkActive == lastDarkState) return;
            lastDarkState = isDarkActive;
            PlayClip(isDarkActive ? flipShadowClip : flipLightClip, 0.9f);
        }

        public void PlayFlipBlocked() => PlayClip(flipBlockedClip, 0.8f);
        public void PlayMirrorRotate() => PlayClip(mirrorRotateClip, 0.85f);
        public void PlayLeverPull() => PlayClip(leverPullClip, 0.9f);
        public void PlaySensorState(bool active) => PlayClip(active ? sensorOnClip : sensorOffClip, 0.75f);
        public void PlayPlatePress() => PlayClip(platePressClip, 0.85f);
        public void PlayButtonClick() => PlayClip(buttonClickClip, 0.85f);
        public void PlayDoorOpen() => PlayClip(doorOpenClip, 0.9f);
        public void PlayLevelWin() => PlayClip(levelWinClip, 1.0f);
        public void PlayMonsterWarn() => PlayClip(monsterWarnClip, 0.75f);
        public void PlayMonsterSpawn() => PlayClip(monsterSpawnClip, 0.95f);
        public void PlayMonsterCatch() => PlayClip(monsterCatchClip, 1.0f);
        public void PlayPortalWarp() => PlayClip(portalWarpClip, 0.8f);
        public void PlayEchoRecord() => PlayClip(echoRecordClip, 0.85f);
        public void PlayMonsterPetrify() => PlayClip(monsterPetrifyClip, 0.95f);

        public void SetBlockPushing(bool pushing)
        {
            if (isBlockPushing == pushing) return;
            isBlockPushing = pushing;
            if (blockPushSource == null || blockPushSource.clip == null) return;

            if (isBlockPushing && Time.timeScale > 0f)
            {
                blockPushSource.volume = sfxVolume * 0.45f;
                if (!blockPushSource.isPlaying) blockPushSource.Play();
            }
            else
            {
                blockPushSource.Stop();
            }
        }

        private void PlayClip(AudioClip clip, float scale)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * scale);
        }

        private void LoadFallbackClips()
        {
            flipLightClip = flipLightClip != null ? flipLightClip : Resources.Load<AudioClip>("SFX/flip_light");
            flipShadowClip = flipShadowClip != null ? flipShadowClip : Resources.Load<AudioClip>("SFX/flip_shadow");
            flipBlockedClip = flipBlockedClip != null ? flipBlockedClip : Resources.Load<AudioClip>("SFX/flip_blocked");
            mirrorRotateClip = mirrorRotateClip != null ? mirrorRotateClip : Resources.Load<AudioClip>("SFX/mirror_rotate");
            leverPullClip = leverPullClip != null ? leverPullClip : Resources.Load<AudioClip>("SFX/lever_pull");
            sensorOnClip = sensorOnClip != null ? sensorOnClip : Resources.Load<AudioClip>("SFX/sensor_on");
            sensorOffClip = sensorOffClip != null ? sensorOffClip : Resources.Load<AudioClip>("SFX/sensor_off");
            platePressClip = platePressClip != null ? platePressClip : Resources.Load<AudioClip>("SFX/plate_press");
            buttonClickClip = buttonClickClip != null ? buttonClickClip : Resources.Load<AudioClip>("SFX/button_click");
            doorOpenClip = doorOpenClip != null ? doorOpenClip : Resources.Load<AudioClip>("SFX/door_open");
            levelWinClip = levelWinClip != null ? levelWinClip : Resources.Load<AudioClip>("SFX/level_win");
            blockPushClip = blockPushClip != null ? blockPushClip : Resources.Load<AudioClip>("SFX/block_push");
            monsterWarnClip = monsterWarnClip != null ? monsterWarnClip : Resources.Load<AudioClip>("SFX/monster_warn");
            monsterSpawnClip = monsterSpawnClip != null ? monsterSpawnClip : Resources.Load<AudioClip>("SFX/monster_spawn");
            monsterCatchClip = monsterCatchClip != null ? monsterCatchClip : Resources.Load<AudioClip>("SFX/monster_catch");
            portalWarpClip = portalWarpClip != null ? portalWarpClip : Resources.Load<AudioClip>("SFX/portal_warp");
            echoRecordClip = echoRecordClip != null ? echoRecordClip : Resources.Load<AudioClip>("SFX/echo_record");
            monsterPetrifyClip = monsterPetrifyClip != null ? monsterPetrifyClip : Resources.Load<AudioClip>("SFX/monster_petrify");
            bgmLightClip = bgmLightClip != null ? bgmLightClip : Resources.Load<AudioClip>("SFX/bgm_light_realm");
            bgmShadowClip = bgmShadowClip != null ? bgmShadowClip : Resources.Load<AudioClip>("SFX/bgm_shadow_realm");
        }

        private void SetupLoopingSources()
        {
            lightBgmSource = CreateChildLoopSource("BGM_LightRealm", bgmLightClip, 0f);
            shadowBgmSource = CreateChildLoopSource("BGM_ShadowRealm", bgmShadowClip, 0f);
            blockPushSource = CreateChildLoopSource("SFX_BlockPush", blockPushClip, sfxVolume * 0.45f);

            if (lightBgmSource.clip != null) lightBgmSource.Play();
            if (shadowBgmSource.clip != null) shadowBgmSource.Play();
        }

        private AudioSource CreateChildLoopSource(string childName, AudioClip clip, float initialVolume)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            AudioSource src = child.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.spatialBlend = 0f;
            src.clip = clip;
            src.volume = initialVolume;
            return src;
        }
    }
}
