using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.Configurators.UnitLogic.ActivatableAbilities;
using BlueprintCore.Blueprints.CustomConfigurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using CruoromancerTweaks.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Enums.Damage;
using Kingmaker.ResourceLinks;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Utility;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.EntitySystem.Properties.BaseGetter.PropertyContextAccessor;

namespace CruoromancerTweaks.ModifiedContent.Spells.Necromancy
{
    internal class NecromancyLevel9
    {
        public static void Configure()
        {
            BlueprintAbility WailOfBanshee = BlueprintTool.Get<BlueprintAbility>("b24583190f36a8442b212e45226c54fc");
            BlueprintBuff Dazed = BlueprintTool.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");

            // 单独保存施法者等级，用于和目标生命骰比较
            var wailCasterLevelRank = ContextRankConfigs.CasterLevel();
            wailCasterLevelRank.m_Type = AbilityRankType.StatBonus;

            AbilityConfigurator.For(WailOfBanshee)
                // 实际寻找目标的范围：40 → 60 英尺
                .EditComponent<AbilityTargetsAround>(c =>
                {
                    c.m_Radius = 60.Feet();
                })

                // 保存施法者等级
                .AddContextRankConfig(wailCasterLevelRank)

                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    ContextActionConditionalSaved saved = null;

                    // 女妖之嚎的豁免判断套在：
                    // Conditional → SavingThrow → ConditionalSaved
                    // 所以直接用现有 ActionTreeUtils 往下找
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (saved == null &&
                                action is ContextActionConditionalSaved candidate)
                            {
                                saved = candidate;
                            }
                        });
                    }

                    if (saved == null)
                    {
                        return;
                    }

                    // 取出原版：
                    // 每施法者等级 10 点污邪伤害
                    // 不重新创建，直接沿用原版 Action
                    ContextActionDealDamage originalDamage =
                        saved.Failed.Actions
                            .OfType<ContextActionDealDamage>()
                            .FirstOrDefault();

                    if (originalDamage == null)
                    {
                        return;
                    }

                    /*
                     * 强韧豁免失败：
                     *
                     * 目标生命骰 < 施法者等级
                     *     → 立即死亡
                     *
                     * 目标生命骰 >= 施法者等级
                     *     → 原版每施法者等级 10 点伤害
                     */
                    saved.Failed = new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Operation = Operation.And,
                                    Conditions = new Condition[]
                                    {
                                        new ContextConditionCompare
                                        {
                                            // 施法者等级严格大于目标等级
                                            m_Type = ContextConditionCompare.Type.Greater,

                                            CheckValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.Rank,
                                                ValueRank = AbilityRankType.StatBonus
                                            },

                                            TargetValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.TargetProperty,
                                                Property = UnitProperty.Level
                                            }
                                        }
                                    }
                                },

                                // CL > 目标生命骰：死亡
                                IfTrue = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionKill()
                                    }
                                },

                                // CL <= 目标生命骰：10 × CL 伤害
                                IfFalse = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        originalDamage
                                    }
                                }
                            }
                        }
                    };

                    /*
                     * 强韧豁免成功：
                     * 无视生命骰，恍惚 1D4 轮
                     */
                    saved.Succeed = new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new ContextActionApplyBuff
                            {
                                m_Buff = Dazed
                                    .ToReference<BlueprintBuffReference>(),

                                ToCaster = false,

                                DurationValue = new ContextDurationValue
                                {
                                    Rate = DurationRate.Rounds,
                                    DiceType = DiceType.D4,
                                    DiceCountValue = 1,
                                    BonusValue = 0
                                }
                            }
                        }
                    };
                })
                .Configure();

            BlueprintAbility NegativeEruption = BlueprintTool.Get<BlueprintAbility>("5c377ad96e3fc4f4d9b74cba9d38f4f8");
            BlueprintFeature NegativeEnergyAffinity = BlueprintTool.Get<BlueprintFeature>("d5ee498e19722854198439629c1841a5");

            AbilityConfigurator.For(NegativeEruption)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (action is not Conditional conditional)
                                return;

                            // 找到截图中：
                            // ContextConditionHasFact
                            // Fact = NegativeEnergyAffinity
                            bool isNegativeEnergyAffinityBranch =
                                conditional.ConditionsChecker?.Conditions?.Any(condition =>
                                    condition is ContextConditionHasFact hasFact &&
                                    hasFact.m_Fact != null &&
                                    hasFact.m_Fact.Guid == NegativeEnergyAffinity.AssetGuid &&
                                    !hasFact.Not
                                ) == true;

                            if (!isNegativeEnergyAffinityBranch)
                                return;

                            // 原本这里是：
                            // HealTarget
                            // ApplyBuff
                            //
                            // 现在追加：
                            // RemoveHarmfulBuffs
                            conditional.IfTrue.Actions =
                                conditional.IfTrue.Actions
                                    .Append<GameAction>(
                                        new ContextActionRemoveHarmfulBuffs()
                                    )
                                    .ToArray();
                        });
                    }
                })
                .Configure();
        }
    }
}
