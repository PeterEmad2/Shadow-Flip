using System.Collections.Generic;
using System.IO;
using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShadowFlip.EditorTools
{
    public static partial class OpticsPuzzleBuilder
    {
        internal static void CreateArenaWalls(CelAssets assets)
        {
            CreateWall("Wall_Top", new Vector3(0f, 5.95f, 0f), new Vector2(21.8f, 1.1f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Bottom", new Vector3(0f, -5.95f, 0f), new Vector2(21.8f, 1.1f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Left", new Vector3(-10.35f, 0f, 0f), new Vector2(1.1f, 13.0f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Right", new Vector3(10.35f, 0f, 0f), new Vector2(1.1f, 13.0f), assets, WorldAffinity.Both, WallTileStyle.Long);
        }

        internal static GameObject CreatePlayerForm(string name, Transform parent, Vector3 pos, string spritePath, Color fallbackColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.color = fallbackColor;
            sr.sortingOrder = 10;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.66f;

            return go;
        }

        internal static GameObject CreateWall(string name, Vector3 pos, Vector2 size, CelAssets assets, WorldAffinity affinity, WallTileStyle style = WallTileStyle.Medium)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            Sprite lightSprite = assets.WallLightLong;
            Sprite shadowSprite = assets.WallShadowLong;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = affinity == WorldAffinity.ShadowOnly ? shadowSprite : lightSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();

            bool isVertical = size.y > size.x;
            if (isVertical)
            {
                float rotZ = pos.x > 8f ? -90f : 90f;
                go.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
                Vector2 localSize = new Vector2(size.y, size.x);
                sr.size = localSize;
                col.size = localSize;
            }
            else
            {
                sr.size = size;
                col.size = size;
            }

            WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
            phase.affinity = affinity;
            if (affinity == WorldAffinity.Both)
            {
                phase.lightWorldSprite = lightSprite;
                phase.shadowWorldSprite = shadowSprite;
            }

            return go;
        }

        internal static LightBeamEmitter CreateEmitter(string name, Vector3 pos, LightColor color, WorldAffinity world, Vector2 dir, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.EmitterActive;
            sr.color = Color.Lerp(Color.white, LightColorUtility.ToUnityColor(color), 0.25f);
            sr.sortingOrder = 4;

            if (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0)
            {
                LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
                anim.frames = assets.ObeliskFrames;
                anim.framesPerSecond = 10f;
            }

            LightBeamEmitter emitter = go.AddComponent<LightBeamEmitter>();
            emitter.beamColor = color;
            emitter.activeInWorld = world;
            emitter.localEmitDirection = dir;
            emitter.emitterRenderer = sr;
            // Leave activeEmitterSprite null so LabAnimatedProp cycles all 13 Obelisk frames while active,
            // while inactiveEmitterSprite shows the dimmed offline Obelisk in the opposite realm.
            emitter.activeEmitterSprite = (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0) ? null : assets.EmitterActive;
            emitter.inactiveEmitterSprite = assets.EmitterIdle;
            return emitter;
        }

        internal static GameObject CreateIsometricMirror(string name, Vector3 pos, float startAngle, float stepDegrees, WorldAffinity affinity, CelAssets assets, bool canRotate = true, bool requireShadowToRotate = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MirrorDirections[0];
            if (requireShadowToRotate) sr.color = new Color(0.78f, 0.56f, 1f, 1f);
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.05f);

            OpticalMirror mirror = go.AddComponent<OpticalMirror>();
            mirror.isTwoSided = true;
            mirror.canRotate = canRotate;
            mirror.requireShadowWorldToRotate = requireShadowToRotate;
            mirror.rotationStepDegrees = stepDegrees;
            mirror.directionalSprites = assets.MirrorDirections;
            mirror.logicalAngle = startAngle;
            mirror.ApplyIsometricVisual();

            WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
            phase.affinity = affinity;
            return go;
        }

        internal static GameObject CreatePushableMirrorCart(string name, Vector3 pos, float startAngle, float stepDegrees, CelAssets assets, bool umbraOnly = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MirrorDirections[0];
            sr.color = umbraOnly ? new Color(0.78f, 0.56f, 1f, 1f) : new Color(0.9f, 0.95f, 1f, 1f);
            sr.sortingOrder = 5;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            OpticalMirror mirror = go.AddComponent<OpticalMirror>();
            mirror.isTwoSided = true;
            mirror.canRotate = true;
            mirror.requireShadowWorldToRotate = umbraOnly;
            mirror.rotationStepDegrees = stepDegrees;
            mirror.directionalSprites = assets.MirrorDirections;
            mirror.logicalAngle = startAngle;
            mirror.ApplyIsometricVisual();

            PushableBlock block = go.AddComponent<PushableBlock>();
            block.pushableInWorld = umbraOnly ? WorldAffinity.ShadowOnly : WorldAffinity.LightOnly;
            return go;
        }

        internal static PortalMirror CreatePortalMirror(string name, Vector3 pos, Sprite sprite, Vector2 exitDir, PortalMirror linked, bool canRotate = true)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            PortalMirror pm = go.AddComponent<PortalMirror>();
            pm.localExitDirection = exitDir;
            pm.linkedPortal = linked;
            pm.canRotateExit = canRotate;
            return pm;
        }

        internal static GameObject CreateColorCombiner(string name, Vector3 pos, LightColor a, LightColor b, LightColor output, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.CombinerCrystal;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            ColorCombiner cc = go.AddComponent<ColorCombiner>();
            cc.requiredColorA = a;
            cc.requiredColorB = b;
            cc.outputColor = output;
            cc.localOutputDirection = Vector2.right;
            cc.canRotate = false;
            return go;
        }

        internal static GameObject CreateSanctuary(string name, Vector3 pos, float radius, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LanternSanctuary;
            sr.color = new Color(1f, 0.94f, 0.72f, 1f);
            sr.sortingOrder = 3;

            if (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0)
            {
                LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
                anim.frames = assets.ObeliskFrames;
                anim.framesPerSecond = 10f;
            }

            LightSanctuary sanc = go.AddComponent<LightSanctuary>();
            sanc.isTeslaSafeZone = false;
            sanc.sanctuaryRadius = radius;
            return go;
        }

        internal static GameObject CreateEchoPedestal(string name, Vector3 pos, Sprite echoSprite, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.EchoPedestal;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.0f);
            col.isTrigger = true;

            ShadowEchoPedestal echo = go.AddComponent<ShadowEchoPedestal>();
            echo.echoSprite = echoSprite;
            return go;
        }

        internal static GameObject CreateBeamRelay(string name, Vector3 pos, BeamRelayCrystal.RelayMode mode, Vector2 outDir, bool canRotate, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.RelayCrystal;
            sr.color = new Color(0.88f, 0.76f, 1f, 1f);
            sr.sortingOrder = 4;

            if (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0)
            {
                LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
                anim.frames = assets.ObeliskFrames;
                anim.framesPerSecond = 10f;
            }

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            BeamRelayCrystal relay = go.AddComponent<BeamRelayCrystal>();
            relay.mode = mode;
            relay.localOutputDirection = outDir;
            relay.canRotate = canRotate;
            return go;
        }

        internal static GameObject CreateShadowBarrierGate(string name, Vector3 pos, Vector2 size, CelAssets assets, bool openWhenLit = true, bool stayOpenOnceLit = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x / 1.28f, size.y / 1.28f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LaserBarrier;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.1f);
            col.isTrigger = true;

            ShadowBarrierGate gate = go.AddComponent<ShadowBarrierGate>();
            gate.openWhenLit = openWhenLit;
            gate.stayOpenOnceLit = stayOpenOnceLit;
            gate.acceptAnyColor = true;
            return go;
        }

        internal static PhaseShiftTile CreatePhaseShiftTile(string name, Vector3 pos, PhaseShiftTile linkedPad, bool flipOnStep, CelAssets assets, bool allowPushableBlocks = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.PhasePad;
            sr.sortingOrder = 0;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.1f);
            col.isTrigger = true;

            PhaseShiftTile pad = go.AddComponent<PhaseShiftTile>();
            pad.linkedTeleportPad = linkedPad;
            pad.flipRealmOnStep = flipOnStep;
            pad.allowPushableBlocks = allowPushableBlocks;
            return pad;
        }

        internal static GameObject CreatePrism(string name, Vector3 pos, float zAngle, OpticalPrism.PrismMode mode, float splitAngle, bool orientAlongTransform, bool canRotate, float stepDegrees, CelAssets assets)
        {
            GameObject prismGo = new GameObject(name);
            prismGo.transform.position = pos;
            prismGo.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
            prismGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer prismSr = prismGo.AddComponent<SpriteRenderer>();
            prismSr.sprite = assets.PrismCrystal;
            prismSr.sortingOrder = 4;

            BoxCollider2D col = prismGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            OpticalPrism prism = prismGo.AddComponent<OpticalPrism>();
            prism.mode = mode;
            prism.splitAngle = splitAngle;
            prism.orientAlongTransform = orientAlongTransform;
            prism.canRotate = canRotate;
            prism.rotationStepDegrees = stepDegrees;
            return prismGo;
        }

        internal static GameObject CreateColorFilter(string name, Vector3 pos, Vector2 size, LightColor color, CelAssets assets)
        {
            GameObject filterGo = new GameObject(name);
            filterGo.transform.position = pos;

            SpriteRenderer sr = filterGo.AddComponent<SpriteRenderer>();
            if (assets.ColorFilterLens != null)
            {
                sr.sprite = assets.ColorFilterLens;
                sr.drawMode = SpriteDrawMode.Simple;
                Vector2 lensBounds = assets.ColorFilterLens.bounds.size;
                bool horizontal = size.x > size.y;
                if (horizontal)
                {
                    filterGo.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    filterGo.transform.localScale = new Vector3(size.y / lensBounds.x, size.x / lensBounds.y, 1f);
                }
                else
                {
                    filterGo.transform.localScale = new Vector3(size.x / lensBounds.x, size.y / lensBounds.y, 1f);
                }
                sr.sortingOrder = 3;

                BoxCollider2D col = filterGo.AddComponent<BoxCollider2D>();
                col.size = lensBounds;
            }
            else
            {
                sr.sprite = assets.FloorLight;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = size;
                sr.sortingOrder = 3;

                BoxCollider2D col = filterGo.AddComponent<BoxCollider2D>();
                col.size = size;
            }

            ColorFilter filter = filterGo.AddComponent<ColorFilter>();
            filter.filterColor = color;
            return filterGo;
        }

        internal static LightSensor CreateSensor(string name, Vector3 pos, LightColor color, WorldAffinity requiredWorld, CelAssets assets, bool acceptAny = false, bool isCapacitor = false, float holdDuration = 0f)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            Sprite idleSp = isCapacitor && assets.CapacitorIdle != null
                ? assets.CapacitorIdle
                : (requiredWorld == WorldAffinity.ShadowOnly ? assets.ReceiverInactiveDark : assets.ReceiverInactive);
            Sprite activeSp = isCapacitor && assets.CapacitorActive != null
                ? assets.CapacitorActive
                : assets.ReceiverActiveGlow;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = idleSp;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.05f);

            LightSensor sensor = go.AddComponent<LightSensor>();
            sensor.requiredColor = color;
            sensor.acceptAnyColor = acceptAny;
            sensor.requiredWorld = isCapacitor ? WorldAffinity.ShadowOnly : requiredWorld;
            sensor.isCapacitor = isCapacitor;
            sensor.holdDuration = holdDuration > 0f ? holdDuration : (isCapacitor ? 10.0f : 0f);
            sensor.indicatorRenderer = sr;
            sensor.inactiveSprite = idleSp;
            sensor.activeSprite = activeSp;
            return sensor;
        }

        internal static PuzzleLever CreateLever(string name, Vector3 pos, CelAssets assets, WorldAffinity usableWorld,
            OpticalMirror[] controlledMirrors = null, GameObject[] toggledObjects = null)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LeverOff;
            sr.sortingOrder = 3;

            bool isWideConsole = assets.LeverOff != null && assets.LeverOff.bounds.size.x > 1.8f;
            if (isWideConsole) go.transform.localScale = new Vector3(0.72f, 0.95f, 1f);

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = isWideConsole ? new Vector2(2.4f, 1.1f) : new Vector2(1.1f, 1.0f);
            col.isTrigger = true;

            PuzzleLever lever = go.AddComponent<PuzzleLever>();
            lever.usableInWorld = usableWorld;
            lever.leverRenderer = sr;
            lever.leverOffSprite = assets.LeverOff;
            lever.leverOnSprite = assets.LeverOn;
            lever.controlledMirrors = controlledMirrors;
            lever.toggledObjects = toggledObjects;
            return lever;
        }

        internal static PuzzleDoor CreateDoor(string name, Vector3 pos, Vector2 size, CelAssets assets,
            LightSensor[] sensors, PressurePlate[] plates, PuzzleLever[] levers, bool requireAll, bool stayOpen)
        {
            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = pos;

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = assets.DoorClosed;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            Vector2 spriteSize = doorSr.sprite != null ? (Vector2)doorSr.sprite.bounds.size : new Vector2(1.28f, 1.60f);
            doorGo.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(spriteSize.x * 0.94f, spriteSize.y * 0.94f);

            PuzzleDoor door = doorGo.AddComponent<PuzzleDoor>();
            door.requiredSensors = sensors;
            door.requiredPlates = plates;
            door.requiredLevers = levers;
            door.requireAll = requireAll;
            door.stayOpenOnceUnlocked = stayOpen;
            door.closedSprite = assets.DoorClosed;
            door.openSprite = assets.DoorOpen;
            return door;
        }

        internal static GameObject CreatePushableBlock(string name, Vector3 pos, CelAssets assets, WorldAffinity pushableInWorld = WorldAffinity.LightOnly)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.PushableBlockSprite != null ? assets.PushableBlockSprite : assets.EmitterBroken;
            if (pushableInWorld == WorldAffinity.ShadowOnly) sr.color = new Color(0.76f, 0.56f, 1f, 1f);
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.85f);

            PushableBlock block = go.AddComponent<PushableBlock>();
            block.pushableInWorld = pushableInWorld;
            return go;
        }

        internal static PressurePlate CreatePressurePlate(string name, Vector3 pos, CelAssets assets, WorldAffinity form, bool latch = false)
        {
            GameObject plateGo = new GameObject(name);
            plateGo.transform.position = pos;
            plateGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer plateSr = plateGo.AddComponent<SpriteRenderer>();
            plateSr.sprite = assets.ButtonUp;
            plateSr.sortingOrder = 1;

            bool isSmallButton = assets.ButtonUp != null && assets.ButtonUp.bounds.size.x < 1.0f;
            if (isSmallButton) plateGo.transform.localScale = new Vector3(1.45f, 1.45f, 1f);

            BoxCollider2D col = plateGo.AddComponent<BoxCollider2D>();
            col.size = isSmallButton ? new Vector2(0.8f, 0.8f) : new Vector2(1.15f, 1.15f);
            col.isTrigger = true;

            PressurePlate plate = plateGo.AddComponent<PressurePlate>();
            plate.acceptedForm = form;
            plate.latchOncePressed = latch;
            plate.plateRenderer = plateSr;
            plate.unpressedSprite = assets.ButtonUp;
            plate.pressedSprite = assets.ButtonDown;
            return plate;
        }

        internal static CelAssets WithLaboratoryTheme(CelAssets src)
        {
            CelAssets lab = src;
            lab.FloorLight = src.LabFloorLight != null ? src.LabFloorLight : src.FloorLight;
            lab.FloorShadow = src.LabFloorShadow != null ? src.LabFloorShadow : src.FloorShadow;
            lab.WallLightLong = src.LabWallLightLong != null ? src.LabWallLightLong : src.WallLightLong;
            lab.WallLightMedium = src.LabWallLightWindow != null ? src.LabWallLightWindow : src.WallLightMedium;
            lab.WallLightPillar = src.LabWallLightPillar != null ? src.LabWallLightPillar : src.WallLightPillar;
            lab.WallLightRuined = src.LabWallPipes != null ? src.LabWallPipes : src.WallLightRuined;
            lab.WallShadowLong = src.LabWallShadowLong != null ? src.LabWallShadowLong : src.WallShadowLong;
            lab.WallShadowMedium = src.LabWallShadowWindow != null ? src.LabWallShadowWindow : src.WallShadowMedium;
            lab.WallShadowPillar = src.LabWallShadowPillar != null ? src.LabWallShadowPillar : src.WallShadowPillar;
            lab.WallShadowRuined = src.LabWallPipes != null ? src.LabWallPipes : src.WallShadowRuined;
            lab.ButtonUp = src.LabButtonUp != null ? src.LabButtonUp : src.ButtonUp;
            lab.ButtonDown = src.LabButtonDown != null ? src.LabButtonDown : src.ButtonDown;
            lab.LeverOff = src.LabConsoleOff != null ? src.LabConsoleOff : src.LeverOff;
            lab.LeverOn = src.LabConsoleOn != null ? src.LabConsoleOn : src.LeverOn;
            return lab;
        }

        internal static CelAssets WithScifiFacilityTheme(CelAssets src)
        {
            CelAssets fac = src;
            fac.FloorLight = src.ScifiFloorLight != null ? src.ScifiFloorLight : src.FloorLight;
            fac.FloorShadow = src.ScifiFloorShadow != null ? src.ScifiFloorShadow : src.FloorShadow;
            fac.WallLightLong = src.ScifiWallLightLong != null ? src.ScifiWallLightLong : src.WallLightLong;
            fac.WallLightMedium = src.ScifiWallLightMedium != null ? src.ScifiWallLightMedium : src.WallLightMedium;
            fac.WallLightPillar = src.ScifiWallLightPillar != null ? src.ScifiWallLightPillar : src.WallLightPillar;
            fac.WallShadowLong = src.ScifiWallShadowLong != null ? src.ScifiWallShadowLong : src.WallShadowLong;
            fac.WallShadowMedium = src.ScifiWallShadowMedium != null ? src.ScifiWallShadowMedium : src.WallShadowMedium;
            fac.WallShadowPillar = src.ScifiWallShadowPillar != null ? src.ScifiWallShadowPillar : src.WallShadowPillar;
            fac.DoorClosed = src.ScifiDoorLabClosed != null ? src.ScifiDoorLabClosed : src.DoorClosed;
            fac.DoorOpen = src.ScifiDoorLabOpen != null ? src.ScifiDoorLabOpen : src.DoorOpen;
            fac.LeverOff = src.ScifiElecBoxClosed != null ? src.ScifiElecBoxClosed : src.LeverOff;
            fac.LeverOn = src.ScifiElecBoxOpen != null ? src.ScifiElecBoxOpen : src.LeverOn;
            fac.ButtonUp = src.ScifiHatchButtonUp != null ? src.ScifiHatchButtonUp : src.ButtonUp;
            fac.ButtonDown = src.ScifiHatchButtonDown != null ? src.ScifiHatchButtonDown : src.ButtonDown;
            fac.PushableBlockSprite = src.ScifiCratePushable != null ? src.ScifiCratePushable : src.PushableBlockSprite;
            return fac;
        }

        internal static PuzzleDoor CreateScifiDoor(
            string name, Vector3 pos, Vector2 size, CelAssets assets,
            LightSensor[] sensors, PressurePlate[] plates, PuzzleLever[] levers,
            bool requireAll, bool stayOpen, bool isLabDoor)
        {
            Sprite closedSprite = isLabDoor ? assets.ScifiDoorLabClosed : assets.ScifiDoorStdClosed;
            Sprite openSprite = isLabDoor ? assets.ScifiDoorLabOpen : assets.ScifiDoorStdOpen;
            if (closedSprite == null || openSprite == null)
            {
                return CreateDoor(name, pos, size, assets, sensors, plates, levers, requireAll, stayOpen);
            }

            if (assets.ScifiThresholdV != null)
            {
                CreateLabProp(name + "_Threshold", pos, assets.ScifiThresholdV, 0.9f, -14);
            }
            if (assets.ScifiDoorMarking != null)
            {
                CreateLabProp(name + "_Marking", pos + new Vector3(0f, -size.y * 0.45f, 0f), assets.ScifiDoorMarking, 0.6f, -14);
            }

            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = pos;
            doorGo.transform.localScale = new Vector3(size.x / 1.11f, size.y / 1.39f, 1f);

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = closedSprite;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.32f);

            PuzzleDoor door = doorGo.AddComponent<PuzzleDoor>();
            door.requiredSensors = sensors;
            door.requiredPlates = plates;
            door.requiredLevers = levers;
            door.requireAll = requireAll;
            door.stayOpenOnceUnlocked = stayOpen;
            door.closedSprite = closedSprite;
            door.openSprite = openSprite;
            return door;
        }

        internal static GameObject CreateScifiAlarmBeacon(string name, Vector3 pos, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.ScifiAlarmOff;
            sr.sortingOrder = 4;

            SpriteRenderer glowSr = null;
            if (assets.ScifiAlarmGlow != null)
            {
                GameObject glowGo = new GameObject("AlarmGlow");
                glowGo.transform.SetParent(go.transform, false);
                glowGo.transform.localPosition = Vector3.zero;
                glowGo.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                glowSr = glowGo.AddComponent<SpriteRenderer>();
                glowSr.sprite = assets.ScifiAlarmGlow;
                glowSr.sortingOrder = 15;
                glowSr.enabled = false;
            }

            ScifiAlarmBeacon beacon = go.AddComponent<ScifiAlarmBeacon>();
            beacon.beaconRenderer = sr;
            beacon.glowRenderer = glowSr;
            beacon.offSprite = assets.ScifiAlarmOff;
            beacon.onSprite = assets.ScifiAlarmOn;
            return go;
        }

        internal static GameObject CreateShadowHorrorDecal(string name, Vector3 pos, Sprite sprite, float scale, int sortingOrder)
        {
            if (sprite == null) return null;
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
            phase.affinity = WorldAffinity.ShadowOnly;
            phase.disableRenderersInsteadOfFade = true;
            return go;
        }

        internal static PuzzleDoor CreateLabBlastDoor(
            string name, Vector3 pos, Vector2 size, CelAssets assets,
            LightSensor[] sensors, PressurePlate[] plates, PuzzleLever[] levers,
            bool requireAll, bool stayOpen, bool vertical, bool redVariant)
        {
            Sprite closedSprite = redVariant ? assets.LabBlastDoorRedClosed : assets.LabBlastDoorClosed;
            Sprite openSprite = redVariant ? assets.LabBlastDoorRedOpen : assets.LabBlastDoorOpen;
            if (closedSprite == null || openSprite == null)
            {
                return CreateDoor(name, pos, size, assets, sensors, plates, levers, requireAll, stayOpen);
            }

            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = pos;
            if (vertical)
            {
                doorGo.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                doorGo.transform.localScale = new Vector3(size.y / 2.58f, size.x / 1.32f, 1f);
            }
            else
            {
                doorGo.transform.localScale = new Vector3(size.x / 2.58f, size.y / 1.32f, 1f);
            }

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = closedSprite;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.48f, 1.25f);

            PuzzleDoor door = doorGo.AddComponent<PuzzleDoor>();
            door.requiredSensors = sensors;
            door.requiredPlates = plates;
            door.requiredLevers = levers;
            door.requireAll = requireAll;
            door.stayOpenOnceUnlocked = stayOpen;
            door.closedSprite = closedSprite;
            door.openSprite = openSprite;
            return door;
        }

        internal static GameObject CreateLabSanctuary(string name, Vector3 pos, float radius, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.5f, 1.15f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LabTeslaTower != null ? assets.LabTeslaTower : assets.LanternSanctuary;
            sr.sortingOrder = 3;

            if (assets.LabBulbs != null && assets.LabBulbs.Length > 0)
            {
                GameObject bulbGo = new GameObject("TeslaBulb_Glow");
                bulbGo.transform.SetParent(go.transform, false);
                bulbGo.transform.localPosition = new Vector3(0f, 0.72f, 0f);
                bulbGo.transform.localScale = new Vector3(0.55f, 0.62f, 1f);
                SpriteRenderer bulbSr = bulbGo.AddComponent<SpriteRenderer>();
                bulbSr.sprite = assets.LabBulbs[0];
                bulbSr.sortingOrder = 5;
                LabAnimatedProp anim = bulbGo.AddComponent<LabAnimatedProp>();
                anim.frames = assets.LabBulbs;
                anim.framesPerSecond = 6f;
            }

            LightSanctuary sanc = go.AddComponent<LightSanctuary>();
            sanc.isTeslaSafeZone = true;
            sanc.sanctuaryRadius = radius;
            return go;
        }

        internal static LabNPCController CreateLabNPC(
            string name, Vector3 pos, string displayName, string[] dialogueLines,
            Sprite[] lightDirs, Sprite[] shadowDirs, CelAssets assets,
            WorldAffinity usableWorld = WorldAffinity.Both)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.92f, 0.92f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = (lightDirs != null && lightDirs.Length > 0) ? lightDirs[0] : null;
            sr.sortingOrder = 8;

            CircleCollider2D triggerCol = go.AddComponent<CircleCollider2D>();
            triggerCol.radius = 0.85f;
            triggerCol.isTrigger = true;

            LabNPCController npc = go.AddComponent<LabNPCController>();
            npc.npcName = displayName;
            npc.dialogueLines = dialogueLines;
            npc.usableInWorld = usableWorld;
            if (lightDirs != null && lightDirs.Length >= 4)
            {
                npc.spriteDown = lightDirs[0];
                npc.spriteUp = lightDirs[1];
                npc.spriteLeft = lightDirs[2];
                npc.spriteRight = lightDirs[3];
            }
            if (shadowDirs != null && shadowDirs.Length >= 4)
            {
                npc.shadowSpriteDown = shadowDirs[0];
                npc.shadowSpriteUp = shadowDirs[1];
                npc.shadowSpriteLeft = shadowDirs[2];
                npc.shadowSpriteRight = shadowDirs[3];
            }
            return npc;
        }

        internal static GameObject CreateLabProp(string name, Vector3 pos, Sprite sprite, float scale, int sortingOrder, bool hasCollider = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            if (hasCollider && sprite != null)
            {
                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(sprite.bounds.size.x * 0.85f, sprite.bounds.size.y * 0.72f);
            }
            return go;
        }

        internal static GameObject CreateAnimatedLabProp(string name, Vector3 pos, Sprite[] frames, float scale, int sortingOrder, float fps)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = (frames != null && frames.Length > 0) ? frames[0] : null;
            sr.sortingOrder = sortingOrder;

            LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
            anim.frames = frames;
            anim.framesPerSecond = fps;
            return go;
        }

        internal static GameObject CreateFloorAccent(string name, Vector3 pos, Vector2 size, Sprite sprite, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        internal static GameObject CreateOneWayPolarizer(string name, Vector3 pos, Vector2 allowedDir, bool invertColor, bool canRotate, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.1f, 0.9f, 1f);
            Vector2 d = allowedDir.sqrMagnitude > 0.001f ? allowedDir.normalized : Vector2.right;
            go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.OneWayPolarizer != null ? assets.OneWayPolarizer : assets.ColorFilterLens;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.15f);

            OneWayPolarizer p = go.AddComponent<OneWayPolarizer>();
            p.allowedDirection = d;
            p.invertRealmColorOnPass = invertColor;
            p.canRotate = canRotate;
            return go;
        }

        internal static GameObject CreateMemoryPrism(string name, Vector3 pos, float holdSeconds, Vector2 outDir, bool canRotate, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MemoryPrismIdle != null ? assets.MemoryPrismIdle : assets.EchoPedestal;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            OpticalMemoryPrism mp = go.AddComponent<OpticalMemoryPrism>();
            mp.holdSeconds = holdSeconds;
            mp.useFixedOutputDir = true;
            mp.localOutputDirection = outDir;
            mp.canRotate = canRotate;
            mp.idleSprite = assets.MemoryPrismIdle;
            mp.chargedSprite = assets.MemoryPrismCharged;
            return go;
        }

        internal static GameObject CreateRealityAnchor(string name, Vector3 pos, float radius, float holdDuration, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.RealityAnchorOff != null ? assets.RealityAnchorOff : assets.LanternSanctuary;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            RealityAnchorLantern ral = go.AddComponent<RealityAnchorLantern>();
            ral.anchorRadius = radius;
            ral.holdDuration = holdDuration;
            ral.offSprite = assets.RealityAnchorOff;
            ral.onSprite = assets.RealityAnchorOn;
            return go;
        }

        internal static GameObject CreateMagneticRail(string name, Vector3 terminalPos, Vector3 pointA, Vector3 pointB, LightColor reqColor, CelAssets assets)
        {
            // Track visual spans pointA -> pointB
            GameObject trackGo = new GameObject(name + "_Track");
            Vector3 mid = (pointA + pointB) * 0.5f;
            Vector3 diff = pointB - pointA;
            float dist = diff.magnitude + 1.0f;
            trackGo.transform.position = mid;
            trackGo.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
            SpriteRenderer trackSr = trackGo.AddComponent<SpriteRenderer>();
            trackSr.sprite = assets.MagneticRailTrack != null ? assets.MagneticRailTrack : assets.FloorLight;
            trackSr.drawMode = SpriteDrawMode.Tiled;
            trackSr.size = new Vector2(dist, 0.95f);
            trackSr.sortingOrder = -12;

            // Moving Cart rider
            GameObject cartGo = new GameObject(name + "_Cart");
            cartGo.transform.position = pointA;
            cartGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            SpriteRenderer cartSr = cartGo.AddComponent<SpriteRenderer>();
            cartSr.sprite = assets.MagneticRailCart != null ? assets.MagneticRailCart : assets.MirrorDirections[0];
            cartSr.sortingOrder = 4;

            // Rail Power Terminal
            GameObject termGo = new GameObject(name + "_Terminal");
            termGo.transform.position = terminalPos;
            termGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            SpriteRenderer termSr = termGo.AddComponent<SpriteRenderer>();
            termSr.sprite = assets.CombinerCrystal;
            termSr.sortingOrder = 4;
            BoxCollider2D termCol = termGo.AddComponent<BoxCollider2D>();
            termCol.size = new Vector2(1.0f, 1.0f);

            BeamMagneticRail rail = termGo.AddComponent<BeamMagneticRail>();
            rail.pointA = pointA;
            rail.pointB = pointB;
            rail.riderTransform = cartGo.transform;
            rail.requiredColor = reqColor;
            rail.moveSpeed = 2.6f;

            return cartGo;
        }

        internal static GameObject CreateMirrorTwinTotem(string name, Vector3 pos, float axisX, Sprite twinSprite, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.SplitTotem != null ? assets.SplitTotem : assets.EchoPedestal;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.0f);
            col.isTrigger = true;

            MirrorTwinTotem totem = go.AddComponent<MirrorTwinTotem>();
            totem.mirrorAxisX = axisX;
            totem.twinSprite = twinSprite;
            return go;
        }

        internal static GameObject CreateUmbraMimicMirror(string name, Vector3 pos, float startAngle, Vector3 fleeOffset, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MimicMirrorIdle != null ? assets.MimicMirrorIdle : assets.MirrorDirections[0];
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.05f);

            OpticalMirror mirror = go.AddComponent<OpticalMirror>();
            mirror.isTwoSided = true;
            mirror.canRotate = true;
            mirror.rotationStepDegrees = 90f;
            mirror.directionalSprites = assets.MirrorDirections;
            mirror.logicalAngle = startAngle;
            mirror.ApplyIsometricVisual();

            UmbraMimicMirror mimic = go.AddComponent<UmbraMimicMirror>();
            mimic.awakeSprite = assets.MimicMirrorAwake;
            mimic.fleeTargetOffset = fleeOffset;
            return go;
        }

        internal static GameObject CreateBeamEaterMoth(string name, Vector3 pos, LightColor stunColor, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.BeamMoth1 != null ? assets.BeamMoth1 : assets.UmbraMonster1;
            sr.sortingOrder = 11;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            BeamEaterMoth moth = go.AddComponent<BeamEaterMoth>();
            moth.frame1 = assets.BeamMoth1;
            moth.frame2 = assets.BeamMoth2;
            moth.stunColor = stunColor;
            return go;
        }

        internal static GameObject CreateUmbraMold(string name, Vector3 pos, float startScale, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(startScale, startScale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.UmbraMoldTile != null ? assets.UmbraMoldTile : assets.FloorShadow;
            sr.sortingOrder = -5;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.55f;
            col.isTrigger = true;

            go.AddComponent<UmbraMoldPatch>();
            return go;
        }

        internal static UmbraWardenBoss CreateUmbraWardenBoss(string name, Vector3 pos, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.UmbraWarden1 != null ? assets.UmbraWarden1 : assets.UmbraMonster1;
            sr.sortingOrder = 12;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.55f, 1.55f);

            UmbraWardenBoss boss = go.AddComponent<UmbraWardenBoss>();
            boss.frame1 = assets.UmbraWarden1;
            boss.frame2 = assets.UmbraWarden2;
            boss.petrifiedSprite = assets.MonsterPetrified;
            return boss;
        }

        internal static BlackHoleLens CreateBlackHole(
            string name, Vector3 pos, float gravityRadius, float eventHorizonRadius, float gravityStrength,
            WorldAffinity affinity, bool canAdjust, int startPreset, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.BlackHole1 != null ? assets.BlackHole1 : assets.PrismCrystal;
            sr.sortingOrder = 5;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = eventHorizonRadius / 0.85f * 0.9f;

            BlackHoleLens bh = go.AddComponent<BlackHoleLens>();
            bh.activeInWorld = affinity;
            bh.gravityRadius = gravityRadius;
            bh.eventHorizonRadius = eventHorizonRadius;
            bh.gravityStrength = gravityStrength;
            bh.canAdjustStrength = canAdjust;
            bh.currentPresetIndex = startPreset;
            bh.frame1 = assets.BlackHole1;
            bh.frame2 = assets.BlackHole2 != null ? assets.BlackHole2 : assets.BlackHole1;
            return bh;
        }

        internal static GameObject CreateExit(string name, Vector3 pos, CelAssets assets, PuzzleDoor[] requiredDoors, string nextScene)
        {
            GameObject exitGo = new GameObject(name);
            exitGo.transform.position = pos;
            exitGo.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

            SpriteRenderer exitSr = exitGo.AddComponent<SpriteRenderer>();
            bool hasCustomExit = assets.ExitPortal != null;
            exitSr.sprite = hasCustomExit ? assets.ExitPortal : assets.ReceiverActiveWhite;
            exitSr.color = hasCustomExit ? Color.white : new Color(0.45f, 1f, 0.72f, 1f);
            exitSr.sortingOrder = 1;

            BoxCollider2D col = exitGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.0f);
            col.isTrigger = true;

            LevelExit exit = exitGo.AddComponent<LevelExit>();
            exit.requiredDoors = requiredDoors;
            exit.nextSceneName = nextScene;
            return exitGo;
        }

    }
}
