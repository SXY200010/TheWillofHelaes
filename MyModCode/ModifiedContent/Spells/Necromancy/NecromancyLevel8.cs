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
    internal class NecromancyLevel8
    {
        private const string DeathClutchBleedConst1d6BuffGuid = "286E456D8EC44BA2AFE8D84E38575D72";
        public static void Configure()
        {
            BlueprintAbility DeathClutch = BlueprintTool.Get<BlueprintAbility>("c3d2294a6740bc147870fff652f3ced5");
            BlueprintBuff Stunned = BlueprintTool.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");
            BlueprintBuff Dazed = BlueprintTool.Get<BlueprintBuff>("9934fedff1b14994ea90205d189c8759");
            BlueprintBuff BleedConst1d4Buff = BlueprintTool.Get<BlueprintBuff>("f80de2a32fc2a7141b23ec29bc36f395");

            // 使用一个独立的 Rank 保存本次施法的施法者等级
            var deathClutchCasterLevelRank = ContextRankConfigs.CasterLevel();
            deathClutchCasterLevelRank.m_Type = AbilityRankType.StatBonus;


            // 复制原版 1D4 体质流血 Buff，并改成 1D6。
            // 不直接修改原 Buff，避免其他引用 BleedConst1d4Buff 的效果一起变成 1D6。
            BlueprintBuff DeathClutchBleedConst1d6Buff = BuffConfigurator.New("DeathClutchBleedConst1d6Buff",DeathClutchBleedConst1d6BuffGuid)
                .CopyFrom(BleedConst1d4Buff)
                // 部分版本的体质持续伤害使用这个组件
                .EditComponents<BuffPoisonStatDamage>(
                    c =>
                    {
                        c.Value = new DiceFormula
                        {
                            m_Dice = DiceType.D6,
                            m_Rolls = 1
                        };
                    },
                    _ => true
                )

                // 兼容使用行动树执行体质流失的版本
                .EditComponents<AddFactContextActions>(
                    c =>
                    {
                        if (c.NewRound?.Actions == null)
                        {
                            return;
                        }

                        foreach (var rootAction in c.NewRound.Actions)
                        {
                            ActionTreeUtils.Walk(rootAction, action =>
                            {
                                if (action is DealStatDamage statDamage &&
                                    statDamage.Stat == StatType.Constitution)
                                {
                                    statDamage.DamageDice = new DiceFormula
                                    {
                                        m_Dice = DiceType.D6,
                                        m_Rolls = 1
                                    };
                                }

                                if (action is ContextActionDealDamage dealDamage)
                                {
                                    dealDamage.Value = new ContextDiceValue
                                    {
                                        DiceType = DiceType.D6,
                                        DiceCountValue = 1,
                                        BonusValue = 0
                                    };
                                }
                            });
                        }
                    },
                    _ => true
                )
                .Configure();


            AbilityConfigurator.For(DeathClutch)
                .AddContextRankConfig(deathClutchCasterLevelRank)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    // 死亡之握只有一个顶层的豁免结果判断
                    ContextActionConditionalSaved saved =
                        c.Actions.Actions
                            .OfType<ContextActionConditionalSaved>()
                            .FirstOrDefault();

                    if (saved == null)
                    {
                        return;
                    }

                    // 取出原版的体质吸取动作。
                    // 这样能够保留原动作中“体质”“Drain”等现有设置，
                    // 只把骰子从 1D4 改成 1D6。
                    Conditional originalConditional =
                        saved.Failed.Actions
                            .OfType<Conditional>()
                            .FirstOrDefault();

                    ContextActionDealDamage constitutionDrain =
                        originalConditional?
                            .IfFalse.Actions
                            .OfType<ContextActionDealDamage>()
                            .FirstOrDefault();

                    if (constitutionDrain == null)
                    {
                        return;
                    }

                    constitutionDrain.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.D6,
                        DiceCountValue = 1,
                        BonusValue = 0
                    };


                    /*
                     * 强韧豁免失败：
                     *
                     * 施法者等级 > 目标等级（目标生命骰低于施法者等级）
                     *     → 立即杀死目标
                     *
                     * 否则
                     *     → 1D6 体质吸取
                     *     → 1D6 体质流血
                     *     → 每施法者等级 1 分钟震慑
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
                                            // 严格大于，因此目标等级等于施法者等级时不会死亡
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

                                IfTrue = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionKill()
                                    }
                                },

                                IfFalse = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        // 一次性 1D6 体质吸取
                                        constitutionDrain,

                                        // 每轮 1D6 体质流失
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = DeathClutchBleedConst1d6Buff
                                                .ToReference<BlueprintBuffReference>(),

                                            Permanent = true,
                                            ToCaster = false
                                        },

                                        // 每施法者等级 1 分钟震慑
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = Stunned
                                                .ToReference<BlueprintBuffReference>(),

                                            ToCaster = false,

                                            DurationValue = new ContextDurationValue
                                            {
                                                Rate = DurationRate.Minutes,
                                                DiceType = DiceType.Zero,
                                                DiceCountValue = 0,

                                                BonusValue = new ContextValue
                                                {
                                                    ValueType = ContextValueType.Rank,
                                                    ValueRank = AbilityRankType.StatBonus
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };


                    /*
                     * 强韧豁免成功：
                     * 不再进入生命骰判断，直接恍惚 1D4 轮。
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

            BlueprintAbility BestowCurseGreater = BlueprintTool.Get<BlueprintAbility>("6101d0f0720927e4ca413de7b3c4b7e5");

            BlueprintAbility BestowCurseGreaterDeteriorationCast = BlueprintTool.Get<BlueprintAbility>("54606d540f5d3684d9f7d6e2e2be9b63");
            BlueprintAbility BestowCurseGreaterFeebleBodyCast = BlueprintTool.Get<BlueprintAbility>("292d630a5abae64499bb18057aaa24b4");
            BlueprintAbility BestowCurseGreaterIdiocyCast = BlueprintTool.Get<BlueprintAbility>("e0212142d2a426f43926edd4202996bb");
            BlueprintAbility BestowCurseGreaterWeaknessCast = BlueprintTool.Get<BlueprintAbility>("1168f36fac0bad64f965928206df7b86");

            BlueprintAbility[] bestowCursesGreater = new BlueprintAbility[] { BestowCurseGreaterDeteriorationCast, BestowCurseGreaterFeebleBodyCast, BestowCurseGreaterIdiocyCast, BestowCurseGreaterWeaknessCast };



            BlueprintAbility HorridWilting = BlueprintTool.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526");



            BlueprintAbility Soulreaver = BlueprintTool.Get<BlueprintAbility>("b4afacd337dac4a40a769a567c038ab7");



            BlueprintAbility DomainOfTheHungryFlesh = BlueprintTool.Get<BlueprintAbility>("0d820abda7693a9418546a47eea62ea2");
            BlueprintAbilityAreaEffect DomainOfTheHungryFleshArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("6df8ed6ff8ac4974aba16eca1b7b5cbe");
        }
    }
}
