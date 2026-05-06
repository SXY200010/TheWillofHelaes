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
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
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
    internal class NecromancyLevel5
    {
        private static readonly string BoneExplosionDescription = "BoneExplosion.Description";
        private static readonly string WavesOfFatigueDescription = "WavesOfFatigue.Description";
        public static void Configure()
        {
            BlueprintAbility BoneExplosion = BlueprintTool.Get<BlueprintAbility>("cc51242ff01192b49a2e25adf096e2d0");
            BlueprintAbility WrackingRay = BlueprintTool.Get<BlueprintAbility>("1cde0691195feae45bab5b83ea3f221e");

            BlueprintBuff Fatigued = BlueprintTool.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");
            BlueprintBuff Exhausted = BlueprintTool.Get<BlueprintBuff>("46d1b9cc3d0fd36469a471b047d773a2");

            AbilityConfigurator.For(BoneExplosion)
                .SetDescription(BoneExplosionDescription)
                .EditComponents<ContextRankConfig>(
                    c =>
                    {
                        if (c.m_Type == AbilityRankType.DamageBonus)
                        {
                            c.m_StepLevel = 8;
                        }
                    },
                    c => c.m_Type == AbilityRankType.DamageBonus
                )
                .EditComponent<ContextCalculateSharedValue>(c =>
                {
                    c.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.D8,
                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank
                        },
                        BonusValue = 0
                    };
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, a =>
                        {
                            if (a is ContextActionConditionalSaved saved)
                            {
                                for (int i = 0; i < saved.Succeed.Actions.Length; i++)
                                {
                                    if (saved.Succeed.Actions[i] is ContextActionDealDamage dealDamage)
                                    {
                                        dealDamage.Value = new ContextDiceValue()
                                        {
                                            DiceType = DiceType.D8,
                                            DiceCountValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.Rank
                                            },
                                            BonusValue = 0
                                        };
                                    }
                                }

                                saved.Succeed.Actions = saved.Succeed.Actions
                                    .Append<GameAction>(new ContextActionApplyBuff
                                    {
                                        m_Buff = Fatigued.ToReference<BlueprintBuffReference>(),
                                        Permanent = true,
                                        DurationValue = new ContextDurationValue
                                        {
                                            Rate = DurationRate.Rounds,
                                            DiceType = DiceType.Zero,
                                            DiceCountValue = 0,
                                            BonusValue = 0
                                        },
                                        ToCaster = false
                                    })
                                    .ToArray();

                                saved.Failed.Actions = saved.Failed.Actions
                                    .Append<GameAction>(new ContextActionApplyBuff
                                    {
                                        m_Buff = Exhausted.ToReference<BlueprintBuffReference>(),
                                        Permanent = true,
                                        DurationValue = new ContextDurationValue
                                        {
                                            Rate = DurationRate.Rounds,
                                            DiceType = DiceType.Zero,
                                            DiceCountValue = 0,
                                            BonusValue = 0
                                        },
                                        ToCaster = false
                                    })
                                    .ToArray();
                            }
                        });
                    }

                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, a =>
                        {
                            if (a is ContextActionChangeSharedValue change)
                            {
                                change.MultiplyValue = 6;
                            }
                        });
                    }
                })
                .EditComponent<AbilityTargetsAround>(
                        c => c.m_Radius = 30.Feet()
                )
                .Configure();

            AbilityConfigurator.For(WrackingRay)
            .EditComponent<AbilityEffectRunAction>(c =>
            {
                foreach (var rootAction in c.Actions.Actions)
                {
                    ActionTreeUtils.Walk(rootAction, a =>
                    {
                        if (a is ContextActionDealDamage dealDamage)
                        {
                            dealDamage.MinHPAfterDamage = -999;
                        }
                    });
                }
            })
            .Configure();

            BlueprintAbility WavesOfFatigue = BlueprintTool.Get<BlueprintAbility>("8878d0c46dfbd564e9d5756349d5e439");
            AbilityConfigurator.For(WavesOfFatigue)
                .SetDescription(WavesOfFatigueDescription)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    Conditional targetCond = null;
                    GameAction savedSpawnFx = null;

                    foreach (var rootAction in c.Actions.Actions)
                    {
                        ActionTreeUtils.Walk(rootAction, a =>
                        {
                            if (targetCond != null) return;

                            if (a is not Conditional cond)
                                return;

                            bool hasFatiguedNotCondition =
                                cond.ConditionsChecker?.Conditions != null &&
                                cond.ConditionsChecker.Conditions
                                    .OfType<ContextConditionHasFact>()
                                    .Any(hasFact =>
                                        hasFact.m_Fact != null &&
                                        hasFact.m_Fact.Guid == Fatigued.AssetGuid &&
                                        hasFact.Not);

                            bool ifTrueHasFatiguedApply =
                                cond.IfTrue?.Actions != null &&
                                cond.IfTrue.Actions
                                    .OfType<ContextActionApplyBuff>()
                                    .Any(apply =>
                                        apply.m_Buff != null &&
                                        apply.m_Buff.Guid == Fatigued.AssetGuid);

                            if (!hasFatiguedNotCondition || !ifTrueHasFatiguedApply)
                                return;

                            targetCond = cond;

                            savedSpawnFx = cond.IfTrue?.Actions?
                                .FirstOrDefault(a => a is ContextActionSpawnFx);
                        });
                    }

                    if (targetCond == null)
                        return;

                    // 第二步：遍历结束后再改
                    targetCond.ConditionsChecker.Conditions =
                        targetCond.ConditionsChecker.Conditions
                            .Where(cond =>
                                !(cond is ContextConditionHasFact hasFact
                                  && hasFact.m_Fact != null
                                  && hasFact.m_Fact.Guid == Fatigued.AssetGuid
                                  && hasFact.Not))
                            .ToArray();

                    var innerCond = new Conditional
                    {
                        ConditionsChecker = new ConditionsChecker
                        {
                            Operation = Operation.And,
                            Conditions = new Condition[]
                            {
                                new ContextConditionHasFact
                                {
                                    m_Fact = Fatigued.ToReference<BlueprintUnitFactReference>(),
                                    Not = false
                                }
                            }
                        },
                        IfTrue = new ActionList
                        {
                            Actions = new GameAction[]
                            {
                                new ContextActionApplyBuff
                                {
                                    m_Buff = Exhausted.ToReference<BlueprintBuffReference>(),
                                    Permanent = true,
                                    DurationValue = new ContextDurationValue
                                    {
                                        Rate = DurationRate.Rounds,
                                        DiceType = DiceType.Zero,
                                        DiceCountValue = 0,
                                        BonusValue = 0
                                    }
                                }
                            }
                        },
                        IfFalse = new ActionList
                        {
                            Actions = new GameAction[]
                            {
                                new ContextActionApplyBuff
                                {
                                    m_Buff = Fatigued.ToReference<BlueprintBuffReference>(),
                                    Permanent = true,
                                    DurationValue = new ContextDurationValue
                                    {
                                        Rate = DurationRate.Rounds,
                                        DiceType = DiceType.Zero,
                                        DiceCountValue = 0,
                                        BonusValue = 0
                                    }
                                }
                            }
                        }
                    };

                    if (savedSpawnFx != null)
                    {
                        targetCond.IfTrue.Actions = new GameAction[]
                        {
                            innerCond,
                            savedSpawnFx
                        };
                    }
                    else
                    {
                        targetCond.IfTrue.Actions = new GameAction[]
                        {
                             innerCond
                        };
                    }
                })
                .Configure();
        }
    }
}
