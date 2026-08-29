using BlueprintCore.Actions.Builder;
using BlueprintCore.Actions.Builder.ContextEx;
using BlueprintCore.Blueprints.Configurators;
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
    internal class NecromancyLevel10
    {
        public static void Configure()
        {
            BlueprintAbility DeathRealm = BlueprintTool.Get<BlueprintAbility>("16c3fe873538010459a8a3fd61304825");

            AbilityConfigurator.For(DeathRealm)
                // 塑能系 → 死灵系
                .EditComponent<SpellComponent>(c =>
                {
                    c.School = SpellSchool.Necromancy;
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    // 截图中负向等级是根动作列表里的 ContextActionDealDamage。
                    // 不递归搜索，避免修改强韧豁免成功时的伤害。
                    var negativeLevelAction = c.Actions.Actions
                        .OfType<ContextActionDealDamage>()
                        .FirstOrDefault();

                    if (negativeLevelAction != null)
                    {
                        negativeLevelAction.Value = new ContextDiceValue
                        {
                            DiceType = DiceType.D4,
                            DiceCountValue = 2,
                            BonusValue = 0
                        };
                    }
                })
                .Configure();

            BlueprintAbility PitOfDespair = BlueprintTool.Get<BlueprintAbility>("cc74245ba989480488925214dd925100");
            BlueprintAbilityAreaEffect PitOfDespairArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("b905a3c987f22cb49a246f0ab211f34c");
            BlueprintBuff PitOfDespairImmunity = BlueprintTool.Get<BlueprintBuff>("ab6c12dbb08530843aade6df0ba0101d");
            BlueprintUnit SummonedNightcrawlerLich = BlueprintTool.Get<BlueprintUnit>("2158490973a84fe38291089163cb4fa4");
            BlueprintFeature UndeadType = BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33");

            AbilityConfigurator.For(PitOfDespair)
                // 学派改为死灵学派
                .EditComponent<SpellComponent>(c =>
                {
                    c.School = SpellSchool.Necromancy;
                })

                // 40英尺范围，并且只显示敌方目标
                .EditComponent<AbilityAoERadius>(c =>
                {
                    c.m_Radius = 40.Feet();
                    c.m_TargetType =
                        Kingmaker.UnitLogic.Abilities.Components.TargetType.Enemy;
                })

                // 陷坑和巨虫夜影持续时间改为每施法者等级分钟
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var action in c.Actions.Actions)
                    {
                        if (action is ContextActionSpawnAreaEffect spawnArea)
                        {
                            spawnArea.DurationValue.Rate = DurationRate.Minutes;
                        }

                        if (action is ContextActionSpawnMonster spawnMonster)
                        {
                            spawnMonster.DurationValue.Rate = DurationRate.Minutes;
                        }
                    }
                })
                .Configure();


            AbilityAreaEffectConfigurator.For(PitOfDespairArea)
                // 实际坑范围增加至40英尺
                .SetSize(40.Feet())

                // 自带甄选：区域只影响敌人
                .SetTargetType(BlueprintAbilityAreaEffect.TargetType.Enemy)

                // 坠落时受到20D6钝击伤害
                .EditComponent<AreaEffectPit>(c =>
                {
                    var originalActions =
                        c.OnFallAction?.Actions ?? Array.Empty<GameAction>();

                    var fallDamage = new ContextActionDealDamage
                    {
                        DamageType = DamageTypes.Physical(
                            form: PhysicalDamageForm.Bludgeoning
                        ),
                        Value = new ContextDiceValue
                        {
                            DiceType = DiceType.D6,
                            DiceCountValue = 20,
                            BonusValue = 0
                        },
                        IsAoE = false,
                        IgnoreCritical = true
                    };

                    c.OnFallAction = new ActionList
                    {
                        Actions = originalActions
                            .Append<GameAction>(fallDamage)
                            .ToArray()
                    };
                })
                .Configure();


            BuffConfigurator.For(PitOfDespairImmunity)
                .EditComponent<AddFactContextActions>(c =>
                {
                    // 原版已经是每轮执行一次。
                    // 将其中的负能量伤害明确设为每施法者等级1D6。
                    bool damageChanged = false;

                    foreach (var rootAction in c.NewRound.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (!damageChanged &&
                                action is ContextActionDealDamage dealDamage)
                            {
                                dealDamage.Value = new ContextDiceValue
                                {
                                    DiceType = DiceType.D6,
                                    DiceCountValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank
                                    },
                                    BonusValue = 0
                                };

                                dealDamage.IsAoE = true;
                                damageChanged = true;
                            }
                        });
                    }
                })
                .Configure();

            UnitConfigurator.For(SummonedNightcrawlerLich)
                .AddToAddFacts(UndeadType)
                .Configure();

            BlueprintAbility DoomOfServitude = BlueprintTool.Get<BlueprintAbility>( "24067ba8e0e69a14e83ff397826f6c6d");

            BlueprintBuff DoomOfServitudeBuff =
                BlueprintTool.Get<BlueprintBuff>(
                    "b7043130aeffe094285c8c74cd0ff576"
                );


            // 施法时造成的伤害
            AbilityConfigurator.For(DoomOfServitude)
                .EditComponents<ContextRankConfig>(
                    c =>
                    {
                        // DamageDice Rank = 施法者等级 × 6
                        c.m_Progression = ContextRankProgression.MultiplyByModifier;
                        c.m_StepLevel = 6;
                    },
                    c => c.m_Type == AbilityRankType.DamageDice
                )
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (action is ContextActionDealDamage dealDamage)
                            {
                                dealDamage.Value = new ContextDiceValue
                                {
                                    DiceType = DiceType.Zero,
                                    DiceCountValue = 0,
                                    BonusValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank,
                                        ValueRank = AbilityRankType.DamageDice
                                    }
                                };
                            }
                        });
                    }
                })
                .Configure();


            // Buff每轮造成的伤害
            BuffConfigurator.For(DoomOfServitudeBuff)
                .EditComponents<ContextRankConfig>(
                    c =>
                    {
                        // Default Rank = 施法者等级 × 6
                        c.m_Progression = ContextRankProgression.MultiplyByModifier;
                        c.m_StepLevel = 6;
                    },
                    c => c.m_Type == AbilityRankType.Default
                )
                .EditComponent<AddFactContextActions>(c =>
                {
                    foreach (var rootAction in c.NewRound.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (action is ContextActionDealDamage dealDamage)
                            {
                                dealDamage.Value = new ContextDiceValue
                                {
                                    DiceType = DiceType.Zero,
                                    DiceCountValue = 0,
                                    BonusValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank,
                                        ValueRank = AbilityRankType.Default
                                    }
                                };
                            }
                        });
                    }
                })
                .Configure();

            BlueprintAbility AbsoluteDeath = BlueprintTool.Get<BlueprintAbility>( "7d721be6d74f07f4d952ec8d6f8f44a0");
            BlueprintBuff GreaterHorrifiedThisRoundBuff = BlueprintTool.Get<BlueprintBuff>( "A4C0E7D2B9F64B52A1F1C7E9D0A2B311");


            // 复制高等恐怖诅咒的原地恐惧Buff。
            // 该Buff包含Frightened和MovementBan。
            BlueprintBuff AbsoluteDeathHorrifiedBuff =
                BuffConfigurator.New(
                    "AbsoluteDeathHorrifiedBuff",
                    "6C5E0A9D12B44A849F1C7D3E8B20A671"
                )
                .CopyFrom(GreaterHorrifiedThisRoundBuff)
                .OnConfigure(buff =>
                {
                    // 不显示Buff UI
                    buff.m_Icon = null;
                })
                .Configure();


            AbilityConfigurator.For(AbsoluteDeath)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    ContextActionConditionalSaved willSaved =
                        c.Actions.Actions
                            .OfType<ContextActionConditionalSaved>()
                            .FirstOrDefault();

                    if (willSaved == null)
                    {
                        return;
                    }

                    // 保留原版意志豁免失败时的死亡动作。
                    ActionList originalWillFailedActions = willSaved.Failed;

                    // 取出原版意志豁免成功时的伤害动作。
                    // 该动作已经是每施法者等级10点污邪伤害。
                    ContextActionDealDamage originalDamage =
                        willSaved.Succeed.Actions
                            .OfType<ContextActionDealDamage>()
                            .FirstOrDefault();

                    if (originalDamage == null)
                    {
                        return;
                    }

                    /*
                     * 意志豁免失败：
                     * 直接执行原版死亡动作。
                     */
                    willSaved.Failed = originalWillFailedActions;

                    /*
                     * 意志豁免成功：
                     * 额外进行一次强韧豁免。
                     *
                     * 强韧失败：立即死亡。
                     * 强韧成功：每施法者等级10点伤害，并原地恐惧1轮。
                     */
                    willSaved.Succeed = new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new ContextActionSavingThrow
                            {
                                Type = SavingThrowType.Fortitude,

                                Actions = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionConditionalSaved
                                        {
                                            Failed = new ActionList
                                            {
                                                Actions = new GameAction[]
                                                {
                                                    new ContextActionKill()
                                                }
                                            },

                                            Succeed = new ActionList
                                            {
                                                Actions = new GameAction[]
                                                {
                                                    originalDamage,

                                                    new ContextActionApplyBuff
                                                    {
                                                        m_Buff =
                                                            AbsoluteDeathHorrifiedBuff
                                                                .ToReference<
                                                                    BlueprintBuffReference>(),

                                                        ToCaster = false,

                                                        DurationValue =
                                                            new ContextDurationValue
                                                            {
                                                                Rate =
                                                                    DurationRate.Rounds,

                                                                DiceType =
                                                                    DiceType.Zero,

                                                                DiceCountValue = 0,

                                                                // 固定持续1轮
                                                                BonusValue = 1
                                                            }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    };

                    // 最外层豁免仍然是意志豁免。
                    c.SavingThrowType = SavingThrowType.Will;
                })
                .Configure();
        }
    }
}
