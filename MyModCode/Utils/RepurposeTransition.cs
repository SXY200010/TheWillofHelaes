using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View;
using System;
using System.Linq;

namespace CruoromancerTweaks.Utils
{
    /// <summary>
    /// Preserve friendly undead raised by the Lich spell Repurpose across area transitions.
    /// This does not change Command Undead or the same-area follow behavior.
    /// Main.Load's harmony.PatchAll() installs these patches automatically.
    /// </summary>
    internal static class RepurposeTransition
    {
        private static readonly LogWrapper Logger = LogWrapper.Get("RepurposeTransition");

        private static bool IsCurrentPartyMember(UnitEntityData unit)
        {
            if (unit == null) return false;
            if (unit.IsMainCharacter) return true;

            var player = Game.Instance?.Player;
            if (player != null &&
                (player.Party.Contains(unit) || player.PartyAndPets.Contains(unit)))
                return true;

            var pet = unit.Get<UnitPartPet>();
            return pet?.Master != null && pet.Master != unit &&
                   IsCurrentPartyMember(pet.Master);
        }

        private static UnitEntityData GetRepurposeCaster(UnitEntityData unit)
        {
            if (unit?.Buffs == null) return null;

            bool hasRepurposeBuff = false;

            foreach (var buff in unit.Buffs)
            {
                // The raised minion keeps a Repurpose-named buff. Match its
                // blueprint name, not Command Undead's two unrelated GUIDs.
                var name = buff.Blueprint?.name;
                if (string.IsNullOrEmpty(name) ||
                    name.IndexOf("Repurpose", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                hasRepurposeBuff = true;

                var caster = buff.Context?.MaybeCaster;
                if (caster != null && IsCurrentPartyMember(caster))
                    return caster;
            }

            // The reanimation may not preserve a caster context on its final
            // buff. For a friendly Repurpose minion, use the party leader as
            // the placement anchor in that case.
            if (hasRepurposeBuff)
            {
                var leader = Game.Instance?.Player?.MainCharacter.Value;
                if (leader != null && IsCurrentPartyMember(leader))
                    return leader;
            }

            return null;
        }

        private static bool IsRepurposedMinion(UnitEntityData unit)
        {
            return unit != null &&
                   unit.Descriptor?.State?.IsDead != true &&
                   unit.IsPlayerFaction &&
                   !IsCurrentPartyMember(unit) &&
                   GetRepurposeCaster(unit) != null;
        }

        private static void MoveWithoutDisposal(
            SceneEntitiesState source, SceneEntitiesState destination, UnitEntityData unit)
        {
            if (source == null || destination == null || unit == null ||
                source == destination) return;

            source.AllEntityData.Remove(unit);
            if (!destination.AllEntityData.Any(e => e.UniqueId == unit.UniqueId))
                destination.AddEntityData(unit);
        }

        [HarmonyPatch(typeof(Game), nameof(Game.HandleAreaBeginUnloading))]
        private static class BeforeAreaUnload
        {
            private static void Prefix(bool forDispose)
            {
                if (!Main.Enabled || forDispose) return;

                try
                {
                    var source = Game.Instance?.LoadedAreaState?.MainState;
                    var destination = Game.Instance?.Player?.CrossSceneState;
                    if (source == null || destination == null) return;

                    foreach (var unit in source.AllEntityData.OfType<UnitEntityData>()
                                 .Where(IsRepurposedMinion).ToList())
                    {
                        MoveWithoutDisposal(source, destination, unit);
                        unit.ClearDestroyMark();
                        Logger.Info($"Preserved Repurpose minion: {unit.CharacterName}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to preserve Repurpose minions.", ex);
                }
            }
        }

        [HarmonyPatch(typeof(EntityDataBase), nameof(EntityDataBase.IsInGame),
            MethodType.Setter)]
        private static class KeepCrossSceneUnitActive
        {
            private static bool Prefix(EntityDataBase __instance, bool value)
            {
                if (!Main.Enabled || value ||
                    !(__instance is UnitEntityData unit) ||
                    !IsRepurposedMinion(unit)) return true;

                var crossScene = Game.Instance?.Player?.CrossSceneState;
                return crossScene == null || unit.HoldingState != crossScene;
            }
        }

        [HarmonyPatch(typeof(AreaEnterPoint), nameof(AreaEnterPoint.PositionCharacters))]
        private static class PositionRepurposedMinions
        {
            private static void Prefix()
            {
                if (!Main.Enabled) return;

                try
                {
                    var source = Game.Instance?.Player?.CrossSceneState;
                    var destination = Game.Instance?.LoadedAreaState?.MainState;
                    if (source == null || destination == null) return;

                    var units = source.AllEntityData.OfType<UnitEntityData>()
                        .Where(IsRepurposedMinion).ToList();
                    foreach (var unit in units)
                    {
                        MoveWithoutDisposal(source, destination, unit);
                        unit.ClearDestroyMark();
                    }

                    if (units.Count > 0)
                        Game.Instance.Player.InvalidateCharacterLists();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to restore Repurpose minions.", ex);
                }
            }

            private static void Postfix()
            {
                if (!Main.Enabled) return;

                try
                {
                    var state = Game.Instance?.LoadedAreaState?.MainState;
                    if (state == null) return;

                    foreach (var unit in state.AllEntityData.OfType<UnitEntityData>()
                                 .Where(IsRepurposedMinion).ToList())
                    {
                        var caster = GetRepurposeCaster(unit);
                        if (caster == null) continue;

                        unit.ClearDestroyMark();
                        unit.Commands.InterruptAll(true);
                        unit.View?.StopMoving();
                        unit.Translocate(caster.Position, caster.Orientation);
                        unit.IsInGame = true;
                        unit.View?.UpdateViewActive();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to position Repurpose minions.", ex);
                }
            }
        }

        [HarmonyPatch(typeof(AreaEnterPoint),
            nameof(AreaEnterPoint.ShouldMoveCharacterOnAreaEnterPoint))]
        private static class SkipPartyFormation
        {
            private static bool Prefix(UnitEntityData character, ref bool __result)
            {
                if (!Main.Enabled || !IsRepurposedMinion(character)) return true;

                __result = false;
                return false;
            }
        }
    }
}
