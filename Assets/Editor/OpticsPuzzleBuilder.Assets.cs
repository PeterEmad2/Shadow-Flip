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
        internal static void ConfigureCelShadedSprites()
        {
            ConfigureSpriteFolder(CelDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(LabDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(ScifiDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(NewDataDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(PixelNewDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Images/Jimmy/Walk/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/Laboratory/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/ScifiFacility/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/NewData/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/PixelNew/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/JimmyWalk/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Images/Jimmy/", pointFilter: true, recursive: true, forceSingleMode: false);
            ConfigureSpriteFolder("Assets/New data/", pointFilter: true, recursive: true, forceSingleMode: false);
            ConfigureSpriteFolder("Assets/tiles_laboratory_LandOfPixels/", pointFilter: true, recursive: true, forceSingleMode: false);
            ConfigureSpriteFolder("Assets/Scifi_Facility_Buider_Pack_1_ver 2.0/", pointFilter: true, recursive: true, skipPreview: true, forceSingleMode: false);
            ConfigurePixelTexture("Assets/Images/cursor/Cursor.png", TextureImporterType.Cursor, ensureReadable: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigurePixelTexture(string assetPath, TextureImporterType requiredType, bool ensureReadable)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.textureType != requiredType)
            {
                importer.textureType = requiredType;
                changed = true;
            }
            if (ensureReadable && !importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }
            if (changed) importer.SaveAndReimport();
        }

        private static void ConfigureSpriteFolder(string folderPath, bool pointFilter = true, bool recursive = false, bool skipPreview = false, bool forceSingleMode = true)
        {
            if (!Directory.Exists(folderPath)) return;
            SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(folderPath, "*.png", searchOption);
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                if (assetPath.EndsWith("CelShaded_SpriteSheet.png")) continue;
                if (skipPreview && assetPath.Contains("/Preview/")) continue;
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;
                if (forceSingleMode)
                {
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }
                    if (importer.wrapMode != TextureWrapMode.Repeat)
                    {
                        importer.wrapMode = TextureWrapMode.Repeat;
                        changed = true;
                    }

                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    if (settings.spriteMeshType != SpriteMeshType.FullRect)
                    {
                        settings.spriteMeshType = SpriteMeshType.FullRect;
                        importer.SetTextureSettings(settings);
                        changed = true;
                    }
                }

                if (Mathf.Abs(importer.spritePixelsPerUnit - 100f) > 0.1f)
                {
                    importer.spritePixelsPerUnit = 100f;
                    changed = true;
                }
                if (pointFilter && importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }
                if (pointFilter && importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }
                if (pointFilter && importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }

                if (changed) importer.SaveAndReimport();
            }
        }

        internal static CelAssets LoadThemedAssets(LevelTheme theme)
        {
            CelAssets src = LoadCelAssets();
            switch (theme)
            {
                case LevelTheme.Laboratory: return WithLaboratoryTheme(src);
                case LevelTheme.ScifiFacility: return WithScifiFacilityTheme(src);
                default: return src;
            }
        }

        internal static CelAssets LoadCelAssets()
        {
            ConfigureCelShadedSprites();
            return new CelAssets
            {
                EmitterIdle = LoadSprite(CelDir + "Emitter_Idle.png"),
                EmitterActive = LoadSprite(CelDir + "Emitter_Active.png"),
                EmitterBroken = LoadSprite(CelDir + "Emitter_Broken.png"),
                PushableBlockSprite = LoadSprite(CelDir + "Emitter_Broken.png"),
                MirrorDirections = new[]
                {
                    LoadSprite(CelDir + "Mirror_45.png"),
                    LoadSprite(CelDir + "Mirror_135.png"),
                    LoadSprite(CelDir + "Mirror_225.png"),
                    LoadSprite(CelDir + "Mirror_315.png")
                },
                ReceiverInactive = LoadSprite(CelDir + "Receiver_Inactive.png"),
                ReceiverActiveWhite = LoadSprite(CelDir + "Receiver_Active_White.png"),
                ReceiverInactiveDark = LoadSprite(CelDir + "Receiver_Inactive_Dark.png"),
                ReceiverActiveGlow = LoadSprite(CelDir + "Receiver_Active_Glow.png"),
                WallLightLong = LoadSprite(CelDir + "Wall_Light_Long.png"),
                WallLightMedium = LoadSprite(CelDir + "Wall_Light_Medium.png"),
                WallLightPillar = LoadSprite(CelDir + "Wall_Light_Pillar.png"),
                WallLightRuined = LoadSprite(CelDir + "Wall_Light_Ruined.png"),
                WallShadowLong = LoadSprite(CelDir + "Wall_Shadow_Long.png"),
                WallShadowMedium = LoadSprite(CelDir + "Wall_Shadow_Medium.png"),
                WallShadowPillar = LoadSprite(CelDir + "Wall_Shadow_Pillar.png"),
                WallShadowRuined = LoadSprite(CelDir + "Wall_Shadow_Ruined.png"),
                DoorClosed = LoadSprite(CelDir + "Door_Closed.png"),
                DoorOpen = LoadSprite(CelDir + "Door_Open.png"),
                LeverOff = LoadSprite(CelDir + "Lever_Off.png"),
                LeverOn = LoadSprite(CelDir + "Lever_On.png"),
                ButtonUp = LoadSprite(CelDir + "Button_Up.png"),
                ButtonDown = LoadSprite(CelDir + "Button_Down.png"),
                FloorLight = LoadSprite(CelDir + "Floor_Light.png"),
                FloorShadow = LoadSprite(CelDir + "Floor_Shadow.png"),
                PrismCrystal = LoadSprite(CelDir + "Prism_Crystal.png"),
                UmbraMonster1 = LoadSprite(CelDir + "UmbraMonster_1.png"),
                UmbraMonster2 = LoadSprite(CelDir + "UmbraMonster_2.png"),
                MonsterPetrified = LoadSprite(CelDir + "Monster_Petrified.png"),
                PortalMirrorA = LoadSprite(CelDir + "PortalMirror_A.png"),
                PortalMirrorB = LoadSprite(CelDir + "PortalMirror_B.png"),
                CombinerCrystal = LoadSprite(CelDir + "Combiner_Crystal.png"),
                LanternSanctuary = LoadSprite(CelDir + "Lantern_Sanctuary.png"),
                EchoPedestal = LoadSprite(CelDir + "Echo_Pedestal.png"),
                RelayCrystal = LoadSprite(CelDir + "Relay_Crystal.png"),
                LaserBarrier = LoadSprite(CelDir + "Laser_Barrier.png"),
                PhasePad = LoadSprite(CelDir + "Phase_Pad.png"),
                ColorFilterLens = LoadSprite(CelDir + "ColorFilter_Lens.png"),
                ExitPortal = LoadSprite(CelDir + "Exit_Portal.png"),

                // LandOfPixels Laboratory Assets
                LabFloorLight = LoadSprite(LabDir + "Lab_Floor_Light.png"),
                LabFloorShadow = LoadSprite(LabDir + "Lab_Floor_Shadow.png"),
                LabFloorGrating = LoadSprite(LabDir + "Lab_Floor_Grating.png"),
                LabFloorHazard = LoadSprite(LabDir + "Lab_Floor_Hazard.png"),
                LabWallLightLong = LoadSprite(LabDir + "Lab_Wall_Light_Long.png"),
                LabWallLightWindow = LoadSprite(LabDir + "Lab_Wall_Light_Window.png"),
                LabWallLightPillar = LoadSprite(LabDir + "Lab_Wall_Light_Pillar.png"),
                LabWallShadowLong = LoadSprite(LabDir + "Lab_Wall_Shadow_Long.png"),
                LabWallShadowWindow = LoadSprite(LabDir + "Lab_Wall_Shadow_Window.png"),
                LabWallShadowPillar = LoadSprite(LabDir + "Lab_Wall_Shadow_Pillar.png"),
                LabWallPipes = LoadSprite(LabDir + "Lab_Wall_Pipes.png"),
                LabBlastDoorClosed = LoadSprite(LabDir + "Lab_BlastDoor_Closed.png"),
                LabBlastDoorOpen = LoadSprite(LabDir + "Lab_BlastDoor_Open.png"),
                LabBlastDoorRedClosed = LoadSprite(LabDir + "Lab_BlastDoorRed_Closed.png"),
                LabBlastDoorRedOpen = LoadSprite(LabDir + "Lab_BlastDoorRed_Open.png"),
                LabHazardDoorClosed = LoadSprite(LabDir + "Lab_HazardDoor_Closed.png"),
                LabKeypadDoorClosed = LoadSprite(LabDir + "Lab_KeypadDoor_Closed.png"),
                LabButtonUp = LoadSprite(LabDir + "Lab_Button_Up.png"),
                LabButtonDown = LoadSprite(LabDir + "Lab_Button_Down.png"),
                LabConsoleOff = LoadSprite(LabDir + "Lab_Console_Off.png"),
                LabConsoleOn = LoadSprite(LabDir + "Lab_Console_On.png"),
                LabTeslaTower = LoadSprite(LabDir + "Lab_TeslaTower.png"),
                LabBioCapsule = LoadSprite(LabDir + "Lab_BioCapsule.png"),
                LabMonitor = LoadSprite(LabDir + "Lab_Monitor.png"),
                LabServerRack = LoadSprite(LabDir + "Lab_ServerRack.png"),
                LabWorkbench = LoadSprite(LabDir + "Lab_Workbench.png"),
                LabShelves = LoadSprite(LabDir + "Lab_Shelves.png"),
                LabBulbs = new[]
                {
                    LoadSprite(LabDir + "Lab_Bulb_1.png"),
                    LoadSprite(LabDir + "Lab_Bulb_2.png"),
                    LoadSprite(LabDir + "Lab_Bulb_3.png"),
                    LoadSprite(LabDir + "Lab_Bulb_4.png")
                },
                LabLiquidUmbra = new[]
                {
                    LoadSprite(LabDir + "Lab_Liquid_Umbra_1.png"),
                    LoadSprite(LabDir + "Lab_Liquid_Umbra_2.png")
                },
                LabLiquidAcid = new[]
                {
                    LoadSprite(LabDir + "Lab_Liquid_Acid_1.png"),
                    LoadSprite(LabDir + "Lab_Liquid_Acid_2.png")
                },
                LabLiquidCryo = new[]
                {
                    LoadSprite(LabDir + "Lab_Liquid_Cryo_1.png"),
                    LoadSprite(LabDir + "Lab_Liquid_Cryo_2.png")
                },
                LabNpcMale = new[]
                {
                    LoadSprite(LabDir + "LabNPC_Male_Down.png"),
                    LoadSprite(LabDir + "LabNPC_Male_Up.png"),
                    LoadSprite(LabDir + "LabNPC_Male_Left.png"),
                    LoadSprite(LabDir + "LabNPC_Male_Right.png")
                },
                LabNpcFemale = new[]
                {
                    LoadSprite(LabDir + "LabNPC_Female_Down.png"),
                    LoadSprite(LabDir + "LabNPC_Female_Up.png"),
                    LoadSprite(LabDir + "LabNPC_Female_Left.png"),
                    LoadSprite(LabDir + "LabNPC_Female_Right.png")
                },
                LabNpcHazmat = new[]
                {
                    LoadSprite(LabDir + "LabNPC_Hazmat_Down.png"),
                    LoadSprite(LabDir + "LabNPC_Hazmat_Up.png"),
                    LoadSprite(LabDir + "LabNPC_Hazmat_Left.png"),
                    LoadSprite(LabDir + "LabNPC_Hazmat_Right.png")
                },

                // Sci-Fi Facility Builder Pack 1 Assets
                ScifiFloorLight = LoadSprite(ScifiDir + "Scifi_Floor_Light.png"),
                ScifiFloorShadow = LoadSprite(ScifiDir + "Scifi_Floor_Shadow.png"),
                ScifiFloorGrated = LoadSprite(ScifiDir + "Scifi_Floor_Grated.png"),
                ScifiWallLightLong = LoadSprite(ScifiDir + "Scifi_Wall_Light_Long.png"),
                ScifiWallLightMedium = LoadSprite(ScifiDir + "Scifi_Wall_Light_Medium.png"),
                ScifiWallLightPillar = LoadSprite(ScifiDir + "Scifi_Wall_Light_Pillar.png"),
                ScifiWallShadowLong = LoadSprite(ScifiDir + "Scifi_Wall_Shadow_Long.png"),
                ScifiWallShadowMedium = LoadSprite(ScifiDir + "Scifi_Wall_Shadow_Medium.png"),
                ScifiWallShadowPillar = LoadSprite(ScifiDir + "Scifi_Wall_Shadow_Pillar.png"),
                ScifiDoorLabClosed = LoadSprite(ScifiDir + "Scifi_Door_Lab_Closed.png"),
                ScifiDoorLabOpen = LoadSprite(ScifiDir + "Scifi_Door_Lab_Open.png"),
                ScifiDoorStdClosed = LoadSprite(ScifiDir + "Scifi_Door_Std_Closed.png"),
                ScifiDoorStdOpen = LoadSprite(ScifiDir + "Scifi_Door_Std_Open.png"),
                ScifiThresholdH = LoadSprite(ScifiDir + "Scifi_Threshold_H.png"),
                ScifiThresholdV = LoadSprite(ScifiDir + "Scifi_Threshold_V.png"),
                ScifiDoorMarking = LoadSprite(ScifiDir + "Scifi_Door_Marking.png"),
                ScifiElecBoxClosed = LoadSprite(ScifiDir + "Scifi_ElecBox_Closed.png"),
                ScifiElecBoxOpen = LoadSprite(ScifiDir + "Scifi_ElecBox_Open.png"),
                ScifiHatchButtonUp = LoadSprite(ScifiDir + "Scifi_Hatch_Button_Up.png"),
                ScifiHatchButtonDown = LoadSprite(ScifiDir + "Scifi_Hatch_Button_Down.png"),
                ScifiCratePushable = LoadSprite(ScifiDir + "Scifi_Crate_Pushable.png"),
                ScifiBenchFilled = LoadSprite(ScifiDir + "Scifi_Bench_Filled.png"),
                ScifiBenchSide = LoadSprite(ScifiDir + "Scifi_Bench_Side.png"),
                ScifiShelfFilled1 = LoadSprite(ScifiDir + "Scifi_Shelf_Filled1.png"),
                ScifiShelfFilled2 = LoadSprite(ScifiDir + "Scifi_Shelf_Filled2.png"),
                ScifiCrateGroup1 = LoadSprite(ScifiDir + "Scifi_Crate_Group1.png"),
                ScifiPipesDouble = LoadSprite(ScifiDir + "Scifi_Pipes_Double.png"),
                ScifiPipesTriple = LoadSprite(ScifiDir + "Scifi_Pipes_Triple.png"),
                ScifiAlarmOff = LoadSprite(ScifiDir + "Scifi_Alarm_Off.png"),
                ScifiAlarmOn = LoadSprite(ScifiDir + "Scifi_Alarm_On.png"),
                ScifiAlarmGlow = LoadSprite(ScifiDir + "Scifi_Alarm_Glow.png"),
                ScifiCameraL = LoadSprite(ScifiDir + "Scifi_Camera_L.png"),
                ScifiCameraR = LoadSprite(ScifiDir + "Scifi_Camera_R.png"),
                ScifiSignBiohazard = LoadSprite(ScifiDir + "Scifi_Sign_Biohazard.png"),
                ScifiSignRadiation = LoadSprite(ScifiDir + "Scifi_Sign_Radiation.png"),
                ScifiSignCaution = LoadSprite(ScifiDir + "Scifi_Sign_Caution.png"),
                ScifiSignDoNotEnter = LoadSprite(ScifiDir + "Scifi_Sign_DoNotEnter.png"),
                ScifiSignDirExit = LoadSprite(ScifiDir + "Scifi_Sign_Dir_Exit.png"),
                ScifiSignDirLab = LoadSprite(ScifiDir + "Scifi_Sign_Dir_Lab.png"),
                ScifiSignDirCtrl = LoadSprite(ScifiDir + "Scifi_Sign_Dir_Ctrl.png"),
                ScifiLabelLab = LoadSprite(ScifiDir + "Scifi_Label_Lab.png"),
                ScifiLabelCtrl = LoadSprite(ScifiDir + "Scifi_Label_Ctrl.png"),
                ScifiLabelMed = LoadSprite(ScifiDir + "Scifi_Label_Med.png"),
                ScifiLabelExit = LoadSprite(ScifiDir + "Scifi_Label_Exit.png"),
                ScifiArrowR = LoadSprite(ScifiDir + "Scifi_Arrow_R.png"),
                ScifiArrowL = LoadSprite(ScifiDir + "Scifi_Arrow_L.png"),
                ScifiArrowUp = LoadSprite(ScifiDir + "Scifi_Arrow_Up.png"),
                ScifiArrowDown = LoadSprite(ScifiDir + "Scifi_Arrow_Down.png"),
                ScifiBlood1 = LoadSprite(ScifiDir + "Scifi_Blood_1.png"),
                ScifiBlood2 = LoadSprite(ScifiDir + "Scifi_Blood_2.png"),
                ScifiHorrorDoorGlow = LoadSprite(ScifiDir + "Scifi_Horror_DoorGlow.png"),

                // New data (Animated Flying Obelisk + PixelLabIcons)
                ObeliskFrames = new[]
                {
                    LoadSprite(NewDataDir + "Obelisk_01.png"),
                    LoadSprite(NewDataDir + "Obelisk_02.png"),
                    LoadSprite(NewDataDir + "Obelisk_03.png"),
                    LoadSprite(NewDataDir + "Obelisk_04.png"),
                    LoadSprite(NewDataDir + "Obelisk_05.png"),
                    LoadSprite(NewDataDir + "Obelisk_06.png"),
                    LoadSprite(NewDataDir + "Obelisk_07.png"),
                    LoadSprite(NewDataDir + "Obelisk_08.png"),
                    LoadSprite(NewDataDir + "Obelisk_09.png"),
                    LoadSprite(NewDataDir + "Obelisk_10.png"),
                    LoadSprite(NewDataDir + "Obelisk_11.png"),
                    LoadSprite(NewDataDir + "Obelisk_12.png"),
                    LoadSprite(NewDataDir + "Obelisk_13.png")
                },
                IconAtom = LoadSprite(NewDataDir + "atom.png"),
                IconBattery = LoadSprite(NewDataDir + "battery.png"),
                IconBulb = LoadSprite(NewDataDir + "bulb.png"),
                IconFlask = LoadSprite(NewDataDir + "flask.png"),
                IconMagnet = LoadSprite(NewDataDir + "magnet.png"),
                IconTestTube = LoadSprite(NewDataDir + "test_tube.png"),

                // PixelNew expansion assets
                OneWayPolarizer = LoadSprite(PixelNewDir + "OneWay_Polarizer.png"),
                RealityAnchorOff = LoadSprite(PixelNewDir + "RealityAnchor_Off.png"),
                RealityAnchorOn = LoadSprite(PixelNewDir + "RealityAnchor_On.png"),
                MagneticRailTrack = LoadSprite(PixelNewDir + "MagneticRail_Track.png"),
                MagneticRailCart = LoadSprite(PixelNewDir + "MagneticRail_Cart.png"),
                SplitTotem = LoadSprite(PixelNewDir + "SplitTotem.png"),
                MemoryPrismIdle = LoadSprite(PixelNewDir + "MemoryPrism_Idle.png"),
                MemoryPrismCharged = LoadSprite(PixelNewDir + "MemoryPrism_Charged.png"),
                MimicMirrorIdle = LoadSprite(PixelNewDir + "MimicMirror_Idle.png"),
                MimicMirrorAwake = LoadSprite(PixelNewDir + "MimicMirror_Awake.png"),
                BeamMoth1 = LoadSprite(PixelNewDir + "BeamMoth_1.png"),
                BeamMoth2 = LoadSprite(PixelNewDir + "BeamMoth_2.png"),
                UmbraMoldTile = LoadSprite(PixelNewDir + "UmbraMold_Tile.png"),
                UmbraWarden1 = LoadSprite(PixelNewDir + "UmbraWarden_1.png"),
                UmbraWarden2 = LoadSprite(PixelNewDir + "UmbraWarden_2.png"),
                BlackHole1 = LoadSprite(PixelNewDir + "BlackHole_1.png"),
                BlackHole2 = LoadSprite(PixelNewDir + "BlackHole_2.png"),
                CapacitorIdle = LoadSprite(PixelNewDir + "Capacitor_Idle.png"),
                CapacitorActive = LoadSprite(PixelNewDir + "Capacitor_Active.png")
            };
        }

        internal static Sprite LoadSprite(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (subAssets != null)
            {
                foreach (Object obj in subAssets)
                {
                    if (obj is Sprite subSprite) return subSprite;
                }
            }

            Debug.LogError($"OpticsPuzzleBuilder: Failed to load Sprite at '{path}'.");
            return null;
        }
        internal static AudioClip LoadAudio(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
