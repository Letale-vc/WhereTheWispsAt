﻿using System;
using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Helpers;
using SharpDX;
using System.Collections.Generic;
using System.Linq;
using Vector3N = System.Numerics.Vector3;
using Vector2N = System.Numerics.Vector2;
using ExileCore.PoEMemory.Elements;

namespace WhereTheWispsAt;

public class WhereTheWispsAt : BaseSettingsPlugin<WhereTheWispsAtSettings>
{
    public enum WispType
    {
        None,
        Yellow,
        Purple,
        Blue,
        Chests,
        LightBomb,
        Wells,
        FuelRefill,
        Altars,
        DustConverters,
        Dealer,
        Encounter,
        Shrine,
        Iron
    }

    public List<string> GoodShrines = new List<string>() { "Acceleration Shrine", "Diamond Shrine",
    "Divine Shrine","Echoing Shrine", "Covetous Shrine", "Impenetrable Shrine"};

    public WispData Wisps = new();

    public WhereTheWispsAt()
    {
        Name = "Where The Wisps At";
    }

    public override bool Initialise() => true;

    public override Job Tick()
    {

        var fuelsToRemove = Wisps.FuelRefill.Where(fuel => fuel.TryGetComponent(out Animated animated) &&
                                                           animated.BaseAnimatedObjectEntity != null
                                                           && animated.BaseAnimatedObjectEntity.TryGetComponent(
                                                               out AnimationController animationController) &&
                                                           animationController.CurrentAnimationId == 2).ToList();
        fuelsToRemove.ForEach(f => RemoveEntityFromList(f, Wisps.FuelRefill));

        var shrinesToRemove = Wisps.GoodShrines.Where(s => !s.IsTargetable).ToList();
        shrinesToRemove.ForEach(s => RemoveEntityFromList(s, Wisps.GoodShrines));

        var wellsToRemove = Wisps.Wells.Where(
                                     well => well.TryGetComponent<StateMachine>(out var stateComp) &&
                                             stateComp?.States.Any(x => x.Name == "activated" && x.Value == 1) == true
                                 )
                                 .ToList();

        wellsToRemove.ForEach(well => RemoveEntityFromList(well, Wisps.Wells));

        var altarsToRemove = Wisps.Altars.Where(
                                      altar => altar.TryGetComponent<StateMachine>(out var stateComp) &&
                                               stateComp?.States.Any(x => x.Name == "activated" && x.Value == 1) == true
                                  )
                                  .ToList();

        altarsToRemove.ForEach(altar => RemoveEntityFromList(altar, Wisps.Altars));

        var dustConvertersToRemove = Wisps.DustConverters.Where(
                                              converter => converter.TryGetComponent<StateMachine>(out var stateComp) &&
                                                           stateComp?.States.Any(
                                                               x => x.Name == "activated" && x.Value == 1
                                                           ) == true
                                          )
                                          .ToList();

        dustConvertersToRemove.ForEach(altar => RemoveEntityFromList(altar, Wisps.DustConverters));

        foreach (var chest in Wisps.Chests.Where(x => x.GetComponent<Chest>()?.IsOpened != false).ToList())
            RemoveEntityFromList(chest, Wisps.Chests);

        return null;
    }

    public override void EntityAdded(Entity entity)
    {
        var path = entity.TryGetComponent<Animated>(out var animatedComp) ? animatedComp?.BaseAnimatedObjectEntity?.Path
            : null;

        //if (entity.Metadata.Contains("Volatile"))
        //{
        //    LogMessage($"Volatile");
        //    foreach (var item in entity.Stats)
        //    {
        //        LogMessage($"{item.Key} - {item.Value}");
        //    }
        //    LogMessage($"Buffs");
        //    foreach (var item in entity.Buffs)
        //    {
        //        LogMessage($"{item.DisplayName} - {item.Name}");
        //    }
        //}

        var metadata = entity.Metadata;

        switch (metadata)
        {
            case not null when metadata.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriResource"):
                if (path != null)
                {
                    if (path.Contains("_primal"))
                    {
                        Wisps.Blue.Add(entity);
                    }
                    else if (path.Contains("_warden"))
                    {
                        Wisps.Yellow.Add(entity);
                    }
                    else if (path.Contains("_vodoo"))
                    {
                        Wisps.Purple.Add(entity);
                    }
                }

                break;
            case "Metadata/MiscellaneousObjects/Azmeri/AzmeriLightBomb":
                Wisps.LightBomb.Add(entity);
                break;
            case "Metadata/MiscellaneousObjects/Azmeri/AzmeriFuelResupply":
                Wisps.FuelRefill.Add(entity);
                break;
            case "Metadata/Terrain/Leagues/Lake/Objects/CraftingObjectRandom":
            case "Metadata/MiscellaneousObjects/Azmeri/AzmeriFlaskRefill":
                Wisps.Wells.Add(entity);
                break;
            case "Metadata/NPC/League/Affliction/GlyphsHarvestTree":
                Wisps.Encounters[entity] = "Harvest";
                break;
            case "Metadata/MiscellaneousObjects/Azmeri/AzmeriBuffEffigySmall":
            case "Metadata/MiscellaneousObjects/Azmeri/AzmeriBuffEffigyMedium":
            case "Metadata/MiscellaneousObjects/Azmeri/AzmeriBuffEffigyLarge":
                Wisps.Encounters[entity] = "Buff";
                break;
            case "Metadata/Monsters/LeagueAzmeri/VoodooKingBoss/VoodooKingBoss":
            case "Metadata/Monsters/LeagueAzmeri/VoodooKingBoss/VoodooKingBoss2":
            case "Metadata/Monsters/LeagueAzmeri/VoodooKingBoss/VoodooKingBoss3":
            case "Metadata/NPC/Ghostrider":
            case "Metadata/NPC/League/Affliction/GlyphsEtching01":
            case "Metadata/NPC/League/Affliction/GlyphsEtching02":
            case "Metadata/NPC/League/Affliction/GlyphsEtching03":
            case "Metadata/NPC/League/Affliction/GlyphsEtching04":
            case "Metadata/NPC/League/Affliction/GlyphsEtching05":
            case "Metadata/NPC/League/Affliction/GlyphsGoddessStatue":
            case "Metadata/NPC/League/Affliction/GlyphsGruthkulShrine":
            case "Metadata/NPC/League/Affliction/GlyphsKingGlyphOne":
            case "Metadata/NPC/League/Affliction/GlyphsKingGlyphTwo":
            case "Metadata/NPC/League/Affliction/GlyphsMajiProclamation01":
            case "Metadata/NPC/League/Affliction/GlyphsMajiProclamation02":
            case "Metadata/NPC/League/Affliction/GlyphsSingleStatue":
            case "Metadata/NPC/League/Affliction/GlyphsWarringSisters":
                Wisps.Encounters[entity] = metadata[(metadata.LastIndexOf('/') + 1)..];
                break;
            case "Metadata/Chests/LeagueAzmeri/OmenChest":
                Wisps.Encounters[entity] = "Omen Chest";
                break;
            //case not null when metadata.Contains("Azmeri/SacrificeAltarObjects"):
            case "Metadata/MiscellaneousObjects/Azmeri/SacrificeAltarObjects/AzmeriSacrificeAltarBear":
            case "Metadata/MiscellaneousObjects/Azmeri/SacrificeAltarObjects/AzmeriSacrificeAltarRabbit":
            case "Metadata/MiscellaneousObjects/Azmeri/SacrificeAltarObjects/AzmeriSacrificeAltarDeer":
                var name = metadata[(metadata.LastIndexOf('/') + 1)..];
                entity.SetHudComponent(name);
                Wisps.Altars.Add(entity);
                break;
            case not null when metadata.Contains("Azmeri/AzmeriDustConverter"):
                Wisps.DustConverters.Add(entity);
                break;
            case not null when metadata.Contains("Azmeri/UniqueDealer"):
                Wisps.Dealer.Add(entity);
                break;
            case not null when metadata.StartsWith("Metadata/Chests/LeagueAzmeri/"):
                Wisps.Chests.Add(entity);
                break;
            case "Metadata/Shrines/Shrine":
                entity.SetHudComponent(entity.RenderName);
                if (GoodShrines.Contains(entity.RenderName))
                {
                    Wisps.GoodShrines.Add(entity);
                }
                else
                {
                    Wisps.BadShrines.Add(entity);
                }
                break;
            case "Metadata/Terrain/Leagues/Settlers/Node/Objects/NodeTypes/CrimsonIron":
                Wisps.Irons.Add(entity);
                break;
        }
    }

    public override void EntityRemoved(Entity entity)
    {
        new[]
            {
                Wisps.Blue, Wisps.Purple, Wisps.Yellow, Wisps.LightBomb, Wisps.Wells, Wisps.FuelRefill
            }.ToList()
             .ForEach(list => RemoveEntityFromList(entity, list));

        Wisps.Encounters.Remove(entity);
    }

    private static void RemoveEntityFromList(Entity entity, List<Entity> list)
    {
        var entityToRemove = list.FirstOrDefault(wisp => wisp.Id == entity.Id);

        if (entityToRemove != null)
        {
            list.Remove(entityToRemove);
        }
    }

    public override void AreaChange(AreaInstance area) =>
        Wisps = new WispData();

    public override void Render()
    {
        if (!Settings.Enable.Value || !GameController.InGame)
        {
            return;
        }

        var inGameUi = GameController.Game.IngameState.IngameUi;

        if (!Settings.IgnoreFullscreenPanels && inGameUi.FullscreenPanels.Any(x => x.IsVisible))
        {
            return;
        }

        if (!Settings.IgnoreLargePanels && inGameUi.LargePanels.Any(x => x.IsVisible))
        {
            return;
        }

        foreach (var (list, color, size, text, type) in new[]
                 {
                     (Wisps.Yellow, Settings.YellowWisp, Settings.YellowSize.Value, null, WispType.Yellow),
                     (Wisps.Purple, Settings.PurpleWisp, Settings.PurpleSize.Value, null, WispType.Purple),
                     (Wisps.Blue, Settings.BlueWisp, Settings.BlueSize.Value, null, WispType.Blue),
                     (Wisps.Chests, Settings.ChestColor, Settings.ChestSize.Value, null, WispType.Chests),
                     (Wisps.LightBomb, Settings.LightBomb, 0, "Light Bomb", WispType.LightBomb),
                     (Wisps.Wells, Settings.Wells, 0, "Well", WispType.Wells),
                     (Wisps.FuelRefill, Settings.FuelRefill, 0, "Fuel Refill", WispType.FuelRefill),
                     (Wisps.Altars, Settings.Altars, 0, string.Empty, WispType.Altars),
                     (Wisps.DustConverters, Settings.DustConverters, 0, "Dust Converter", WispType.DustConverters),
                     (Wisps.Dealer, Settings.Dealer, 0, "! TRADER !", WispType.Dealer),
                     (Wisps.GoodShrines, Settings.BlueWisp, 0, string.Empty, WispType.Shrine),
                     //(Wisps.BadShrines, Settings.Altars, 0, string.Empty, WispType.Shrine),

                     //(Wisps.Shrines.Where(s=>s.RenderName.Contains("Covetous")&&s.IsTargetable).ToList(), Settings.Dealer, 0, $"COVETOUS", WispType.Shrine),
                      (Wisps.Irons, Settings.Altars, 0, "CRIMSON", WispType.Iron),
                 })
            DrawWisps(list, color, size, text, type);

        foreach (var (entity, text) in Wisps.Encounters)
            DrawWisps([entity], Settings.EncounterColor, 0, text, WispType.Encounter);

        foreach (var chest in Wisps.Chests)
            if (chest.DistancePlayer < Settings.ChestScreenDisplayMaxDistance &&
                chest.TryGetComponent<Render>(out var render))
            {
                Graphics.DrawBoundingBoxInWorld(
                    chest.PosNum,
                    Settings.ChestColor.Value with
                    {
                        A = (byte)Settings.ChestAlpha
                    },
                    render.BoundsNum,
                    render.RotationNum.X
                );
            }
        if (Settings.DrawRemainingFuel)
        {
            if (!GameController.IngameState.Data.IsInsideAzmeriZone)
            {
                return;
            }
            var fuelLeft = inGameUi.LeagueMechanicButtons.AzmeriElement.Data.RemainingFuelFraction;
            var fuelLeftText = $"{fuelLeft:P0}";
            var textPlacement = new Vector2(Settings.PositionX, Settings.PositionY);
            using (Graphics.SetTextScale(Settings.TextSize))
            {
                Graphics.DrawText(fuelLeftText, textPlacement, Settings.FuelColor);
            }
        }
        return;


        void DrawWisps(List<Entity> entityList, Color color, int size, string text, WispType type = WispType.None)
        {
            // Just run this once, land looks flat.
            var groundZ = entityList.FirstOrDefault()?.GridPosNum is { } gridPosNum
                ? GameController.IngameState.Data.GetTerrainHeightAt(gridPosNum) : 0;

            entityList = entityList.OrderBy(x => x.Id).ToList();


            var screenSize = new RectangleF
            {
                X = 0,
                Y = 0,
                Width = GameController.Window.GetWindowRectangleTimeCache.Size.Width,
                Height = GameController.Window.GetWindowRectangleTimeCache.Size.Height
            };

            for (var i = 0; i < entityList.Count; i++)
            {

                var specificWispTypes = new[]
                {
                    WispType.Yellow, WispType.Purple, WispType.Blue, WispType.LightBomb, WispType.FuelRefill
                };

                var actualWispTypes = new[] { WispType.Yellow, WispType.Purple, WispType.Blue,
                };


                var entityCur = entityList[i];
                var text2 = entityCur.GetHudComponent<string>();
                if (text2 != null)
                {
                    text = entityCur.GetHudComponent<string>();
                }

                if (entityCur.IsTransitioned)
                {
                    continue;
                }
                var actualSize = size;
                WispSize wispSize = new WispSize() { Size = 0 };
                if (actualWispTypes.Contains(type))
                {
                    var newSize = entityCur.GetHudComponent<WispSize>();
                    if (newSize != null)
                    {
                        wispSize.Size = newSize.Size;
                    }
                    else
                    {
                        var c = entityCur.GetComponent<Animated>()?.BaseAnimatedObjectEntity?.Metadata;

                        if (c?.Contains("sml") ?? false)
                        {
                            wispSize.Size = Settings.WispSizeSettings.SmallSize.Value;
                        }
                        else if (c?.Contains("med") ?? false)
                        {
                            wispSize.Size = Settings.WispSizeSettings.MediumSize.Value;
                        }
                        else if (c?.Contains("big") ?? false)
                        {
                            wispSize.Size = Settings.WispSizeSettings.LargeSize.Value;
                        }
                        entityCur.SetHudComponent(wispSize);
                    }
                    actualSize += wispSize.Size;
                }

                if (Settings.DrawMap && GameController.IngameState.IngameUi.Map.LargeMap.IsVisibleLocal)
                {
                    var mapPos = GameController.IngameState.Data.GetGridMapScreenPosition(
                        entityCur.PosNum.WorldToGrid()
                    );

                    if (text != null)
                    {
                        const int widthPadding = 3;
                        var boxOffset = Graphics.MeasureText(text) / 2f;
                        var textOffset = boxOffset;
                        boxOffset.X += widthPadding;
                        Graphics.DrawBox(mapPos - boxOffset, mapPos + boxOffset, Color.Black);
                        Graphics.DrawText(text, mapPos - textOffset, color);
                    }
                    else
                    {
                        if (Settings.DrawMapLines && i < entityList.Count - 1 && type != WispType.Chests)
                        {
                            if (i < entityList.Count - 1 && specificWispTypes.Contains(type))
                            {
                                var entityNext = entityList[i + 1];

                                var mapPosTo
                                    = GameController.IngameState.Data.GetGridMapScreenPosition(
                                        entityNext.PosNum.WorldToGrid()
                                    );

                                if (entityNext.Id == entityCur.Id + 1 && entityCur.Distance(entityNext) < 30 &&
                                    IsEntityWithinScreen(mapPos, screenSize, 0) &&
                                    IsEntityWithinScreen(mapPosTo, screenSize, 0))
                                {
                                    Graphics.DrawLine(mapPos, mapPosTo, Settings.MapLineSize, color);
                                }
                            }
                        }

                        Graphics.DrawCircleFilled(mapPos, actualSize, color, 8);
                        //.DrawBox(
                        //    new RectangleF(mapPos.X - actualSize / 2, mapPos.Y - actualSize / 2, actualSize, actualSize),
                        //    color,
                        //    1f
                        //);
                    }
                }

                if (!Settings.DrawWispsOnGround || !specificWispTypes.Contains(type))
                {
                    continue;
                }

                var entityPos = entityCur.PosNum;
                var entityPosScreen = RemoteMemoryObject.pTheGame.IngameState.Camera.WorldToScreen(entityPos);

                if (IsEntityWithinScreen(entityPosScreen, screenSize, 50))
                {
                    Graphics.DrawBoundingBoxInWorld(
                        entityPos with
                        {
                            Z = groundZ
                        },
                        color with
                        {
                            A = (byte)Settings.WispsOnGroundAlpha
                        },
                        new Vector3N(
                            Settings.WispsOnGroundWidth,
                            Settings.WispsOnGroundWidth,
                            Settings.WispsOnGroundHeight
                        ),
                        0f
                    );
                }
            }
        }
    }

    private static bool IsEntityWithinScreen(Vector2N entityPos, RectangleF screensize, float allowancePX)
    {
        // Check if the entity position is within the screen bounds with allowance
        var leftBound = screensize.Left - allowancePX;
        var rightBound = screensize.Right + allowancePX;
        var topBound = screensize.Top - allowancePX;
        var bottomBound = screensize.Bottom + allowancePX;

        return entityPos.X >= leftBound && entityPos.X <= rightBound && entityPos.Y >= topBound &&
               entityPos.Y <= bottomBound;
    }

    public bool HideEntity(Entity entity)
    {
        var hideEntity = GameController.PluginBridge.GetMethod<Func<Entity, bool>>("HideEntity");
        //DebugWindow.LogMsg($"Is null: {hideEntity is null}");
        return hideEntity?.Invoke(entity) ?? false;
    }

    public class WispData
    {
        public List<Entity> Purple { get; } = new();
        public List<Entity> Yellow { get; } = new();
        public List<Entity> Blue { get; } = new();
        public List<Entity> LightBomb { get; } = new();
        public List<Entity> Wells { get; } = new();
        public List<Entity> FuelRefill { get; } = new();
        public List<Entity> Altars { get; } = new();
        public List<Entity> DustConverters { get; } = new();
        public List<Entity> Dealer { get; } = new();
        public List<Entity> Chests { get; } = new();
        public Dictionary<Entity, string> Encounters { get; } = new();
        public List<Entity> GoodShrines { get; } = new();
        public List<Entity> BadShrines { get; } = new();
        public List<Entity> Irons { get; } = new();
    }
}