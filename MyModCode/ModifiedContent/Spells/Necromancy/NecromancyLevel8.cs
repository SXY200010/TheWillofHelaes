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
        private const string HorridWiltingDelayedBuffGuid = "56AF1855B7E54EB09C7207E1D2816A69";
        private const string GreaterHorrifiedThisRoundBuffGuid = "A4C0E7D2B9F64B52A1F1C7E9D0A2B311";
        private const string GreaterHorrifyingCurseBuffGuid = "D6F3B24C8A4E49F7A8123D0C5B7E9F21";
        private const string GreaterHorrifyingCurseAbilityGuid = "F19B7A2D4C834E6BA0D5C1E8F2A94763";
        private static readonly string DeathClutchDescription = "DeathClutch.Description";
        private static readonly string BestowCurseGreaterDescription = "BestowCurseGreater.Description";
        private static readonly string GreaterHorrifyingCurseName = "GreaterHorrifyingCurse.Name";
        private static readonly string GreaterHorrifyingCurseDescription = "GreaterHorrifyingCurse.Description";
        private static readonly string HorridWiltingDescription = "HorridWilting.Description";
        private static readonly string SoulreaverDescription = "Soulreaver.Description";
        private static readonly string DomainOfTheHungryFleshDescription = "DomainOfTheHungryFlesh.Description";
        public static void Configure()
        {
            BlueprintAbility DeathClutch = BlueprintTool.Get<BlueprintAbility>("c3d2294a6740bc147870fff652f3ced5");
            BlueprintBuff Stunned = BlueprintTool.Get<BlueprintBuff>("09d39b38bb7c6014394b6daced9bacd3");
            BlueprintBuff Staggered = BlueprintTool.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3");
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
                .SetDescription(DeathClutchDescription)
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

                                            DurationValue = new ContextDurationValue
                                            {
                                                DiceType = DiceType.Zero,
                                                DiceCountValue = 0,
                                                BonusValue = 0
                                            },

                                            Permanent = true,
                                            ToCaster = false
                                        },  
                                        
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
                                m_Buff = Staggered
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
            BlueprintAbility BestowCurseGreaterDeterioration = BlueprintTool.Get<BlueprintAbility>("71196d7e6d6645247a058a3c3c9bb5fd");
            BlueprintAbility BestowCurseGreaterFeebleBody = BlueprintTool.Get<BlueprintAbility>("c74a7dfebd7b1004a80f7e59689dfadd");
            BlueprintAbility BestowCurseGreaterIdiocy = BlueprintTool.Get<BlueprintAbility>("f7739a453e2138b46978e9098a29b3fb");
            BlueprintAbility BestowCurseGreaterWeakness = BlueprintTool.Get<BlueprintAbility>("abb2d42dd9219eb41848ec56a8726d58");

            BlueprintBuff BestowCurseGreaterDeteriorationBuff = BlueprintTool.Get<BlueprintBuff>("8f8835d083f31c547a39ebc26ae42159");
            BlueprintBuff BestowCurseGreaterFeebleBodyBuff = BlueprintTool.Get<BlueprintBuff>("28c9db77dfb1aa54a94e8a7413b1840a");
            BlueprintBuff BestowCurseGreaterIdiocyBuff = BlueprintTool.Get<BlueprintBuff>("493dcc29a21abd94d9adb579e1f40318");
            BlueprintBuff BestowCurseGreaterWeaknessBuff = BlueprintTool.Get<BlueprintBuff>("0493a9d25687d7e4682e250ae3ccb187");

            BlueprintAbility[] bestowCursesGreater = new BlueprintAbility[] { BestowCurseGreaterDeterioration, BestowCurseGreaterFeebleBody, BestowCurseGreaterIdiocy, BestowCurseGreaterWeakness };

            BlueprintBuff[] bestowCurseGreaterBuffs = new BlueprintBuff[] { BestowCurseGreaterDeteriorationBuff, BestowCurseGreaterFeebleBodyBuff, BestowCurseGreaterIdiocyBuff, BestowCurseGreaterWeaknessBuff };

            BlueprintBuff greaterHorrifiedThisRoundBuff = BuffConfigurator.New("GreaterHorrifiedThisRoundBuff", GreaterHorrifiedThisRoundBuffGuid)
                .AddCondition(Kingmaker.UnitLogic.UnitCondition.Frightened)
                .AddCondition(Kingmaker.UnitLogic.UnitCondition.MovementBan)
                .Configure();

            BlueprintBuff greaterHorrifyingCurseBuff = BuffConfigurator.New("GreaterHorrifyingCurseBuff", GreaterHorrifyingCurseBuffGuid)
                .SetIcon(BestowCurseGreater.Icon)
                .SetDisplayName(GreaterHorrifyingCurseName)
                .SetDescription(GreaterHorrifyingCurseDescription)
                .SetStacking(StackingType.Stack)
                .AddFactContextActions(
                    newRound: new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new ContextActionSavingThrow
                            {
                                Type = SavingThrowType.Will,

                                Actions = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionConditionalSaved
                                        {
                                            Succeed = new ActionList
                                            {
                                                Actions = Array.Empty<GameAction>()
                                            },

                                            Failed = new ActionList
                                            {
                                                Actions = new GameAction[]
                                                {
                                                    new ContextActionApplyBuff
                                                    {
                                                        m_Buff = greaterHorrifiedThisRoundBuff
                                                            .ToReference<BlueprintBuffReference>(),

                                                        DurationValue = new ContextDurationValue
                                                        {
                                                            Rate = DurationRate.Rounds,
                                                            DiceType = DiceType.Zero,
                                                            DiceCountValue = 0,
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
                    }
                )
                .Configure();

            BlueprintAbility BestowCurseGreaterHorrifying = AbilityConfigurator.New("GreaterHorrifyingCurse", GreaterHorrifyingCurseAbilityGuid)
                .CopyFrom(BestowCurseGreaterFeebleBody, typeof(SpellComponent), typeof(SpellDescriptorComponent), typeof(AbilitySpawnFx))
                .SetDisplayName(GreaterHorrifyingCurseName)
                .SetDescription(GreaterHorrifyingCurseDescription)
                .SetRange(AbilityRange.Close)
                .AddComponent<AbilityEffectRunAction>(c =>
                {
                    c.Actions = new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new ContextActionConditionalSaved
                            {
                                Succeed = new ActionList
                                {
                                    Actions = Array.Empty<GameAction>()
                                },

                                Failed = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = greaterHorrifyingCurseBuff
                                                .ToReference<BlueprintBuffReference>(),

                                            DurationValue = new ContextDurationValue
                                            {
                                                DiceType = DiceType.Zero,
                                                DiceCountValue = 0,
                                                BonusValue = 0
                                            },

                                            Permanent = true
                                        }
                                    }
                                }
                            }
                        }
                    };

                    c.SavingThrowType = SavingThrowType.Will;
                })
                .Configure();

            var greaterCurseVariants = new List<Blueprint<BlueprintAbilityReference>>
                {
                    BestowCurseGreaterDeterioration,
                    BestowCurseGreaterFeebleBody,
                    BestowCurseGreaterIdiocy,
                    BestowCurseGreaterWeakness,
                    BestowCurseGreaterHorrifying
                };

            AbilityConfigurator.For(BestowCurseGreater)
                .SetRange(AbilityRange.Close)
                .SetDescription(BestowCurseGreaterDescription)
                .AddAbilityVariants(greaterCurseVariants, mergeBehavior: ComponentMerge.Replace)
                .Configure();

            foreach (var curse in bestowCursesGreater)
            {
                AbilityConfigurator.For(curse)
                    .SetRange(AbilityRange.Close)
                    .RemoveComponents(c => c is AbilityDeliverTouch)
                    .Configure();
            }

            foreach (var curseBuff in bestowCurseGreaterBuffs)
            {
                BuffConfigurator.For(curseBuff)
                    .SetStacking(StackingType.Stack)
                    .EditComponents<AddStatBonus>(
                        c =>
                        {
                            c.Descriptor = ModifierDescriptor.UntypedStackable;
                        },
                        _ => true
                    )
                    .EditComponents<AddContextStatBonus>(
                        c =>
                        {
                            c.Descriptor = ModifierDescriptor.UntypedStackable;
                        },
                        _ => true
                    )
                    .Configure();
            }

            BlueprintAbility HorridWilting = BlueprintTool.Get<BlueprintAbility>("08323922485f7e246acb3d2276515526");

            // 豁免失败后标记目标，在下一轮结算一次额外伤害。
            BlueprintBuff horridWiltingDelayedBuff =
                BuffConfigurator.New("HorridWiltingDelayedBuff", HorridWiltingDelayedBuffGuid)
                    .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                    .SetStacking(StackingType.Stack)
                    .AddContextRankConfig(ContextRankConfigs.CasterLevel())
                    .AddFactContextActions(newRound: new ActionList
                    {
                        Actions = new GameAction[]
                        {
                            new ContextActionDealDamage
                            {
                                DamageType = new DamageTypeDescription
                                {
                                    Type = DamageType.Untyped
                                },
                                Value = new ContextDiceValue
                                {
                                    DiceType = DiceType.D6,
                                    DiceCountValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank,
                                        ValueRank = AbilityRankType.Default
                                    },
                                    BonusValue = 0
                                },
                                ReadPreRolledFromSharedValue = false
                            },
                            new ContextActionRemoveSelf()
                        }
                    })
                    .Configure();

            AbilityConfigurator.For(HorridWilting)
                .SetDescription(HorridWiltingDescription)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (action is ContextActionDealDamage damage)
                            {
                                // 普通目标：D6 → D8
                                if (damage.Value.DiceType == DiceType.D6)
                                {
                                    damage.Value.DiceType = DiceType.D8;
                                }
                                // 植物 / 水元素：D8 → D10
                                else if (damage.Value.DiceType == DiceType.D8)
                                {
                                    damage.Value.DiceType = DiceType.D10;
                                }

                                // 修复强效 / 极效不参与实际伤害结算
                                damage.ReadPreRolledFromSharedValue = false;
                            }
                        });
                    }

                    // 使用 AbilityEffectRunAction 已进行的豁免，不依赖原树含有 ConditionalSaved。
                    c.SavingThrowType = SavingThrowType.Fortitude;
                    var actions = c.Actions.Actions.ToList();
                    actions.Add(new ContextActionConditionalSaved
                    {
                        Succeed = new ActionList { Actions = Array.Empty<GameAction>() },
                        Failed = new ActionList
                        {
                            Actions = new GameAction[]
                            {
                                new ContextActionApplyBuff
                                {
                                    m_Buff = horridWiltingDelayedBuff.ToReference<BlueprintBuffReference>(),
                                    DurationValue = new ContextDurationValue
                                    {
                                        Rate = DurationRate.Rounds,
                                        DiceType = DiceType.Zero,
                                        DiceCountValue = 0,
                                        BonusValue = 2
                                    },
                                    ToCaster = false,
                                    AsChild = false,
                                    IsFromSpell = true
                                }
                            }
                        }
                    });
                    c.Actions.Actions = actions.ToArray();
                })
                .Configure();

            BlueprintAbility Soulreaver = BlueprintTool.Get<BlueprintAbility>("b4afacd337dac4a40a769a567c038ab7");

            AbilityConfigurator.For(Soulreaver)
                .SetDescription(SoulreaverDescription)
                // 伤害骰上限：20 → 25
                .EditComponents<ContextRankConfig>(
                    c =>
                    {
                        c.m_Max = 25;
                    },
                    c => c.m_Type == AbilityRankType.Default
                )

                // 伤害：每施法者等级 D6 → D8
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, action =>
                        {
                            if (action is ContextActionDealDamage damage &&
                                damage.m_Type == ContextActionDealDamage.Type.Damage)
                            {
                                damage.Value.DiceType = DiceType.D8;

                                // 保留原本的 Rank.Default 作为骰数
                                // 即最终为 min(CL, 25)D8

                                // 修复强效 / 极效不参与伤害结算的问题
                                damage.ReadPreRolledFromSharedValue = false;
                            }
                        });
                    }

                    // 单独追加失败分支；原版直接伤害仍按自己的 HalfIfSaved 结算。
                    c.SavingThrowType = SavingThrowType.Fortitude;
                    var actions = c.Actions.Actions.ToList();
                    actions.Add(new ContextActionConditionalSaved
                    {
                        Succeed = new ActionList { Actions = Array.Empty<GameAction>() },
                        // 由 BlueprintCore 构造完整的能量吸取动作，避免遗漏专用字段。
                        Failed = ActionsBuilder.New()
                            .DealDamagePermanentNegativeLevels(new ContextDiceValue
                            {
                                DiceType = DiceType.D4,
                                DiceCountValue = 1,
                                BonusValue = 0
                            }, halfIfSaved: false)
                            .Build()
                    });
                    c.Actions.Actions = actions.ToArray();
                })

                // 范围：25 英尺 → 30 英尺
                .EditComponent<AbilityTargetsAround>(
                    c => c.m_Radius = 30.Feet()
                )
                .Configure();

            BlueprintAbility DomainOfTheHungryFlesh = BlueprintTool.Get<BlueprintAbility>("0d820abda7693a9418546a47eea62ea2");
            BlueprintAbilityAreaEffect DomainOfTheHungryFleshArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("6df8ed6ff8ac4974aba16eca1b7b5cbe");

            // 不同区域分别维护Buff实例，豁免减值不重复叠加。
            var hungryFleshSavePenalty = BuffConfigurator.New("HungryFleshSavePenaltyBuff", "D38D8D64EE1B4B9B9E8F548CBB9A5CFD")
                .SetIcon(DomainOfTheHungryFlesh.Icon)
                .SetStacking(StackingType.Stack)
                .AddComponent<AddStatBonus>(c =>
                {
                    c.Stat = StatType.SaveFortitude;
                    c.Value = -4;
                    c.Descriptor = ModifierDescriptor.UntypedStackable;
                })
                .AddComponent<AddStatBonus>(c =>
                {
                    c.Stat = StatType.SaveReflex;
                    c.Value = -4;
                    c.Descriptor = ModifierDescriptor.UntypedStackable;
                })
                .AddComponent<AddStatBonus>(c =>
                {
                    c.Stat = StatType.SaveWill;
                    c.Value = -4;
                    c.Descriptor = ModifierDescriptor.UntypedStackable;
                })
                .Configure();

            AbilityAreaEffectConfigurator.For(DomainOfTheHungryFleshArea)
                .AddComponent<AbilityAreaEffectBuff>(c =>
                {
                    c.m_Buff = hungryFleshSavePenalty.ToReference<BlueprintBuffReference>();
                    c.Condition = new ConditionsChecker
                    {
                        Operation = Operation.And,
                        Conditions = Array.Empty<Condition>()
                    };

                })
                .EditComponent<AbilityAreaEffectRunAction>(c =>
                {
                    var actions = c.Round.Actions.ToList();

                    var originalDamage = actions
                        .OfType<ContextActionDealDamage>()
                        .FirstOrDefault();

                    if (originalDamage == null)
                        return;

                    int index = actions.IndexOf(originalDamage);

                    // 原版：
                    // 2D6 + Rank.Default
                    //
                    // 改为第一段：
                    // 2D6
                    originalDamage.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.D6,

                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Simple,
                            Value = 2
                        },

                        BonusValue = new ContextValue
                        {
                            ValueType = ContextValueType.Simple,
                            Value = 0
                        }
                    };

                    // 第二段：
                    // 每施法者等级 1D4
                    var casterLevelDamage = new ContextActionDealDamage
                    {
                        DamageType = originalDamage.DamageType,

                        Value = new ContextDiceValue
                        {
                            DiceType = DiceType.D4,

                            DiceCountValue = new ContextValue
                            {
                                ValueType = ContextValueType.Rank,
                                ValueRank = AbilityRankType.Default
                            },

                            BonusValue = new ContextValue
                            {
                                ValueType = ContextValueType.Simple,
                                Value = 0
                            }
                        },

                        ReadPreRolledFromSharedValue = false
                    };

                    // 紧接在原伤害后面
                    actions.Insert(index + 1, casterLevelDamage);

                    c.Round.Actions = actions.ToArray();
                })
                .Configure();
        }
    }
}
