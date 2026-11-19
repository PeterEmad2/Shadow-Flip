using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Automatically repairs any null SpriteRenderer references at runtime from Resources/Sprites/
    /// and ensures every solid wall has a visible WallBorderRenderer outline across all scenes.
    /// </summary>
    public static class SpriteFallbackRepair
    {
        private static bool hookRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            if (hookRegistered)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                hookRegistered = false;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!hookRegistered)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                hookRegistered = true;
            }
            RepairCurrentScene();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RepairCurrentScene();
        }

        public static void RepairCurrentScene()
        {
            Sprite btnUp = LoadRes("Sprites/Button_Up");
            Sprite btnDown = LoadRes("Sprites/Button_Down");
            Sprite recInactive = LoadRes("Sprites/Receiver_Inactive");
            Sprite recInactiveDark = LoadRes("Sprites/Receiver_Inactive_Dark");
            Sprite recActiveGlow = LoadRes("Sprites/Receiver_Active_Glow");
            Sprite recActiveWhite = LoadRes("Sprites/Receiver_Active_White");
            Sprite emitActive = LoadRes("Sprites/Emitter_Active");
            Sprite emitIdle = LoadRes("Sprites/Emitter_Idle");
            Sprite relaySprite = LoadRes("Sprites/Relay_Crystal");
            Sprite sancSprite = LoadRes("Sprites/Lantern_Sanctuary");
            Sprite doorClosed = LoadRes("Sprites/Door_Closed");
            Sprite doorOpen = LoadRes("Sprites/Door_Open");
            Sprite leverOff = LoadRes("Sprites/Lever_Off");
            Sprite leverOn = LoadRes("Sprites/Lever_On");
            Sprite exitPortal = LoadRes("Sprites/Exit_Portal");
            Sprite wallLight = LoadRes("Sprites/Wall_Light_Long");
            Sprite wallShadow = LoadRes("Sprites/Wall_Shadow_Long");
            Sprite floorLight = LoadRes("Sprites/Floor_Light");
            Sprite floorShadow = LoadRes("Sprites/Floor_Shadow");
            Sprite[] obeliskFrames = LoadObeliskFrames();

            // 1. Floor Buttons (PressurePlates)
            foreach (PressurePlate plate in Object.FindObjectsByType<PressurePlate>(FindObjectsSortMode.None))
            {
                if (plate == null) continue;
                SpriteRenderer sr = plate.plateRenderer != null ? plate.plateRenderer : plate.GetComponent<SpriteRenderer>();
                if (plate.unpressedSprite == null) plate.unpressedSprite = btnUp;
                if (plate.pressedSprite == null) plate.pressedSprite = btnDown;
                if (sr != null)
                {
                    plate.plateRenderer = sr;
                    sr.sortingOrder = Mathf.Max(sr.sortingOrder, 1);
                    if (sr.sprite == null)
                    {
                        sr.sprite = plate.IsPressed ? plate.pressedSprite : plate.unpressedSprite;
                    }
                    sr.color = Color.white;
                }
            }

            // 2. Laser Receivers (LightSensors)
            foreach (LightSensor sensor in Object.FindObjectsByType<LightSensor>(FindObjectsSortMode.None))
            {
                if (sensor == null) continue;
                SpriteRenderer sr = sensor.indicatorRenderer != null ? sensor.indicatorRenderer : sensor.GetComponent<SpriteRenderer>();
                if (sensor.inactiveSprite == null)
                {
                    sensor.inactiveSprite = sensor.requiredWorld == WorldAffinity.ShadowOnly
                        ? (recInactiveDark != null ? recInactiveDark : recInactive)
                        : recInactive;
                }
                if (sensor.activeSprite == null)
                {
                    sensor.activeSprite = recActiveGlow != null ? recActiveGlow : recActiveWhite;
                }
                if (sr != null)
                {
                    sensor.indicatorRenderer = sr;
                    sr.sortingOrder = Mathf.Max(sr.sortingOrder, 3);
                    if (sr.sprite == null)
                    {
                        sr.sprite = sensor.IsActivated ? sensor.activeSprite : sensor.inactiveSprite;
                    }
                    if (!sensor.IsActivated)
                    {
                        sr.color = Color.white;
                    }
                }
            }

            // 3. Laser Emitters
            foreach (LightBeamEmitter emitter in Object.FindObjectsByType<LightBeamEmitter>(FindObjectsSortMode.None))
            {
                if (emitter == null) continue;
                SpriteRenderer sr = emitter.emitterRenderer != null ? emitter.emitterRenderer : emitter.GetComponent<SpriteRenderer>();
                if (emitter.inactiveEmitterSprite == null) emitter.inactiveEmitterSprite = emitIdle;

                if (obeliskFrames.Length == 13 && emitter.GetComponent<LabAnimatedProp>() == null)
                {
                    LabAnimatedProp anim = emitter.gameObject.AddComponent<LabAnimatedProp>();
                    anim.frames = obeliskFrames;
                    anim.framesPerSecond = 10f;
                    emitter.activeEmitterSprite = null;
                }
                else if (emitter.activeEmitterSprite == null && emitter.GetComponent<LabAnimatedProp>() == null)
                {
                    emitter.activeEmitterSprite = emitActive;
                }

                if (sr != null)
                {
                    emitter.emitterRenderer = sr;
                    if (sr.sprite == null) sr.sprite = emitActive != null ? emitActive : emitIdle;
                }
            }

            // 4. Beam Relays & Sanctuaries
            foreach (BeamRelayCrystal relay in Object.FindObjectsByType<BeamRelayCrystal>(FindObjectsSortMode.None))
            {
                if (relay == null) continue;
                SpriteRenderer sr = relay.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null) sr.sprite = relaySprite != null ? relaySprite : emitActive;
                if (obeliskFrames.Length == 13 && relay.GetComponent<LabAnimatedProp>() == null)
                {
                    LabAnimatedProp anim = relay.gameObject.AddComponent<LabAnimatedProp>();
                    anim.frames = obeliskFrames;
                    anim.framesPerSecond = 10f;
                }
            }

            foreach (LightSanctuary sanc in Object.FindObjectsByType<LightSanctuary>(FindObjectsSortMode.None))
            {
                if (sanc == null) continue;
                SpriteRenderer sr = sanc.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null) sr.sprite = sancSprite != null ? sancSprite : emitActive;
                if (sanc.transform.Find("TeslaBulb_Glow") == null && obeliskFrames.Length == 13 && sanc.GetComponent<LabAnimatedProp>() == null)
                {
                    LabAnimatedProp anim = sanc.gameObject.AddComponent<LabAnimatedProp>();
                    anim.frames = obeliskFrames;
                    anim.framesPerSecond = 10f;
                }
            }

            // 5. Doors, Levers, & LevelExit
            foreach (PuzzleDoor door in Object.FindObjectsByType<PuzzleDoor>(FindObjectsSortMode.None))
            {
                if (door == null) continue;
                SpriteRenderer sr = door.GetComponent<SpriteRenderer>();
                if (door.closedSprite == null) door.closedSprite = doorClosed;
                if (door.openSprite == null) door.openSprite = doorOpen;
                if (sr != null && sr.sprite == null) sr.sprite = door.IsOpen ? door.openSprite : door.closedSprite;
            }

            foreach (PuzzleLever lever in Object.FindObjectsByType<PuzzleLever>(FindObjectsSortMode.None))
            {
                if (lever == null) continue;
                SpriteRenderer sr = lever.leverRenderer != null ? lever.leverRenderer : lever.GetComponent<SpriteRenderer>();
                if (lever.leverOffSprite == null) lever.leverOffSprite = leverOff;
                if (lever.leverOnSprite == null) lever.leverOnSprite = leverOn;
                if (sr != null)
                {
                    lever.leverRenderer = sr;
                    if (sr.sprite == null) sr.sprite = lever.isActivated ? lever.leverOnSprite : lever.leverOffSprite;
                }
            }

            foreach (LevelExit exit in Object.FindObjectsByType<LevelExit>(FindObjectsSortMode.None))
            {
                if (exit == null) continue;
                SpriteRenderer sr = exit.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
                {
                    if (exitPortal != null)
                    {
                        sr.sprite = exitPortal;
                        sr.color = Color.white;
                    }
                    else
                    {
                        sr.sprite = recActiveWhite;
                    }
                }
            }

            // 6. Floors & Bordered Walls
            foreach (WorldPhaseObject phase in Object.FindObjectsByType<WorldPhaseObject>(FindObjectsSortMode.None))
            {
                if (phase == null) continue;
                SpriteRenderer sr = phase.GetComponent<SpriteRenderer>();
                BoxCollider2D box = phase.GetComponent<BoxCollider2D>();

                if (sr != null && box == null && phase.gameObject.name.Contains("Floor"))
                {
                    if (phase.affinity == WorldAffinity.Both)
                    {
                        if (phase.lightWorldSprite == null) phase.lightWorldSprite = floorLight;
                        if (phase.shadowWorldSprite == null) phase.shadowWorldSprite = floorShadow;
                    }
                    if (sr.sprite == null) sr.sprite = floorLight;
                    phase.RefreshState();
                    continue;
                }

                if (box != null && !box.isTrigger && phase.GetComponent<PushableBlock>() == null
                    && phase.GetComponent<OpticalMirror>() == null && phase.GetComponent<OpticalPrism>() == null
                    && phase.GetComponent<LightSensor>() == null && phase.GetComponent<PuzzleDoor>() == null
                    && phase.GetComponent<ColorFilter>() == null && phase.GetComponent<PortalMirror>() == null
                    && phase.GetComponent<ColorCombiner>() == null && phase.GetComponent<BeamRelayCrystal>() == null)
                {
                    if (phase.affinity == WorldAffinity.Both)
                    {
                        if (phase.lightWorldSprite == null) phase.lightWorldSprite = wallLight;
                        if (phase.shadowWorldSprite == null) phase.shadowWorldSprite = wallShadow;
                    }
                    if (sr != null && sr.sprite == null)
                    {
                        sr.sprite = phase.affinity == WorldAffinity.ShadowOnly ? wallShadow : wallLight;
                    }
                    phase.RefreshState();

                    if (phase.GetComponent<WallBorderRenderer>() == null)
                    {
                        WallBorderRenderer border = phase.gameObject.AddComponent<WallBorderRenderer>();
                        border.wallSize = box.size;
                        border.affinity = phase.affinity;
                        border.RefreshBorders(force: true);
                    }
                }
            }
        }

        private static Sprite LoadRes(string path) => Resources.Load<Sprite>(path);

        private static Sprite[] LoadObeliskFrames()
        {
            List<Sprite> frames = new List<Sprite>(13);
            for (int i = 1; i <= 13; i++)
            {
                Sprite s = Resources.Load<Sprite>($"Sprites/NewData/Obelisk_{i:00}");
                if (s != null) frames.Add(s);
            }
            return frames.ToArray();
        }
    }
}
