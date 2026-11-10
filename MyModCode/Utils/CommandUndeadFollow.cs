using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility;
using System;
using System.Linq;
using UnityEngine;

namespace CruoromancerTweaks.Utils
{
    /// <summary>
    /// 让仍受“号令死灵”控制的亡灵在同一地图内自动跟随施法者。
    /// Harmony 补丁会由 Main.Load() 中现有的 harmony.PatchAll() 自动加载。
    /// </summary>
    internal static class CommandUndeadFollow
    {
        private const string CommandUndeadBuffGuid =
            "7cd727ddd4cc4be498720e45f0c1f6f4";

        private const string CommandUndeadIntelligentBuffGuid =
            "07f4f8d2000a91c459c23c7fff8c74fb";

        // 每隔0.35秒更新一次目标位置，避免每帧扫描和重复下命令。
        private const float UpdateIntervalSeconds = 0.35f;

        // 超过4米才开始追赶，避免站在主人附近时不断抖动。
        private const float StartFollowingDistanceMeters = 4.0f;

        // 移动到距主人约2尺处停止，不与主人完全重叠。
        private static readonly float StopDistanceMeters = 2.Feet().Meters;

        private static readonly LogWrapper Logger =
            LogWrapper.Get("CommandUndeadFollow");

        private static float s_NextUpdateTime;

        /// <summary>
        /// 获取对该单位施加号令死灵的队伍成员。
        /// Buff不存在、已经失效或施法者不在当前队伍时返回null。
        /// </summary>
        private static UnitEntityData GetController(UnitEntityData unit)
        {
            if (unit == null || unit.Buffs == null)
            {
                return null;
            }

            foreach (var buff in unit.Buffs)
            {
                var blueprint = buff.Blueprint;
                if (blueprint == null)
                {
                    continue;
                }

                string guid = blueprint.AssetGuid.ToString();
                bool isCommandUndeadBuff =
                    string.Equals(
                        guid,
                        CommandUndeadBuffGuid,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        guid,
                        CommandUndeadIntelligentBuffGuid,
                        StringComparison.OrdinalIgnoreCase);

                if (!isCommandUndeadBuff)
                {
                    continue;
                }

                UnitEntityData caster = buff.Context?.MaybeCaster;
                if (caster != null &&
                    caster.IsInGame &&
                    IsCurrentPartyMember(caster))
                {
                    return caster;
                }
            }

            return null;
        }

        private static bool IsCurrentPartyMember(UnitEntityData unit)
        {
            if (unit == null)
            {
                return false;
            }

            if (unit.IsMainCharacter)
            {
                return true;
            }

            var player = Game.Instance?.Player;
            return player != null &&
                   (player.Party.Contains(unit) ||
                    player.PartyAndPets.Contains(unit));
        }

        private static bool CanFollow(UnitEntityData unit)
        {
            if (unit == null || !unit.IsInGame)
            {
                return false;
            }

            if (unit.Descriptor?.State?.IsDead == true)
            {
                return false;
            }

            // 号令死灵有效时目标应属于玩家阵营。
            // Buff结束并恢复敌对后，立即停止给它下达跟随命令。
            if (!unit.IsPlayerFaction)
            {
                return false;
            }

            // 正式队员和宠物已经由游戏自己的编队移动逻辑负责。
            if (IsCurrentPartyMember(unit))
            {
                return false;
            }

            return GetController(unit) != null;
        }

        private static void UpdateFollowers()
        {
            var game = Game.Instance;
            var player = game?.Player;
            var mainState = game?.LoadedAreaState?.MainState;

            if (player == null || mainState == null)
            {
                return;
            }

            // 战斗中不覆盖单位的接敌、攻击和施法移动命令。
            if (player.IsInCombat)
            {
                return;
            }

            var controlledUndead = mainState.AllEntityData
                .OfType<UnitEntityData>()
                .Where(CanFollow)
                .ToList();

            float startDistanceSquared =
                StartFollowingDistanceMeters * StartFollowingDistanceMeters;

            foreach (UnitEntityData unit in controlledUndead)
            {
                UnitEntityData controller = GetController(unit);
                if (controller == null)
                {
                    continue;
                }

                Vector3 difference = unit.Position - controller.Position;
                if (difference.sqrMagnitude <= startDistanceSquared)
                {
                    continue;
                }

                // Run只替换Move槽位，不会占用标准动作；每次刷新主人当前位置，
                // 因此主人持续走动时，被控制亡灵也会持续追随。
                unit.Commands.Run(
                    new UnitMoveTo(
                        controller.Position,
                        StopDistanceMeters));
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Tick))]
        private static class Game_Tick_Patch
        {
            private static void Postfix()
            {
                if (!Main.Enabled)
                {
                    return;
                }

                float currentTime = Time.realtimeSinceStartup;
                if (currentTime < s_NextUpdateTime)
                {
                    return;
                }

                s_NextUpdateTime = currentTime + UpdateIntervalSeconds;

                try
                {
                    UpdateFollowers();
                }
                catch (Exception exception)
                {
                    Logger.Error(
                        "Failed to update Command Undead followers.",
                        exception);
                }
            }
        }
    }
}
