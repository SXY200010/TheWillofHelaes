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
    internal class NecromancyLevel6
    {
        public static void Configure()
        {
            BlueprintAbility SiphonLife = BlueprintTool.Get<BlueprintAbility>("7bd52a86498c7854ebe99bc3cfb85bfe");

            BlueprintBuff Fatigued = BlueprintTool.Get<BlueprintBuff>("e6f2fc5d73d88064583cb828801212f4");

            AbilityConfigurator.For(SiphonLife)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var rootAction in c.Actions.Actions)
                    {
                        if (rootAction is ContextActionDealDamage dealDamage)
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
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var list = c.Actions.Actions.ToList();

                    list.Add(new ContextActionApplyBuff
                    {
                        m_Buff = Fatigued.ToReference<BlueprintBuffReference>(),
                        DurationValue = new ContextDurationValue
                        {
                            Rate = DurationRate.Rounds,
                            DiceType = DiceType.Zero,
                            DiceCountValue = 0,
                            BonusValue = 0
                        },
                        Permanent = true
                    });

                    c.Actions.Actions = list.ToArray();
                })
                .Configure();

            BlueprintAbility Eyebite = BlueprintTool.Get<BlueprintAbility>("3167d30dd3c622c46b0c0cb242061642");
            BlueprintBuff EyebiteBuff = BlueprintTool.Get<BlueprintBuff>("50827f87d113b194f9fc772a47ae2b58");


            BlueprintAbility CircleOfDeath = BlueprintTool.Get<BlueprintAbility>("a89dcbbab8f40e44e920cc60636097cf");
            AbilityConfigurator.For(CircleOfDeath)
            .EditComponents<ContextCalculateSharedValue>(
                c =>
                {
                    c.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.D6,
                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.Default,
                        },
                        BonusValue = 0
                    };
                },
                c => c.ValueType != AbilitySharedValue.Heal
            )
            .AddComponent<ContextRankConfig>(c =>
            {
                c.m_Type = AbilityRankType.DamageBonus;
                c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                c.m_Progression = ContextRankProgression.AsIs;
                c.m_Max = 10;
            })
            .AddComponent<ContextCalculateSharedValue>(c =>
            {
                c.ValueType = AbilitySharedValue.Heal; 
                c.Value = new ContextDiceValue
                {
                    DiceType = DiceType.Zero,
                    DiceCountValue = 0,
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.DamageBonus,
                        Value = 9
                    }
                };
            })
            .EditComponent<AbilityEffectRunAction>(c =>
            {
                foreach (var rootAction in c.Actions.Actions)
                {
                    ActionTreeUtils.Walk(rootAction, a =>
                    {
                        if (a is Conditional cond)
                        {
                            foreach (var condition in cond.ConditionsChecker.Conditions)
                            {
                                if (condition is ContextConditionHitDice hpCond && hpCond.HitDice == 9)
                                {
                                    hpCond.SharedValue = AbilitySharedValue.Heal;
                                }
                            }
                        }
                    });
                }
            })
            .Configure();

            BlueprintAbility UndeathToDeath = BlueprintTool.Get<BlueprintAbility>("a9a52760290591844a96d0109e30e04d");
            AbilityConfigurator.For(UndeathToDeath)
            .EditComponents<ContextCalculateSharedValue>(
                c =>
                {
                    c.Value = new ContextDiceValue
                    {
                        DiceType = DiceType.D6,
                        DiceCountValue = new ContextValue
                        {
                            ValueType = ContextValueType.Rank,
                            ValueRank = AbilityRankType.Default,
                        },
                        BonusValue = 0
                    };
                },
                c => c.ValueType != AbilitySharedValue.Heal
            )
            .AddComponent<ContextRankConfig>(c =>
            {
                c.m_Type = AbilityRankType.DamageBonus;
                c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                c.m_Progression = ContextRankProgression.AsIs;
                c.m_Max = 10;
            })
            .AddComponent<ContextCalculateSharedValue>(c =>
            {
                c.ValueType = AbilitySharedValue.Heal;
                c.Value = new ContextDiceValue
                {
                    DiceType = DiceType.Zero,
                    DiceCountValue = 0,
                    BonusValue = new ContextValue
                    {
                        ValueType = ContextValueType.Rank,
                        ValueRank = AbilityRankType.DamageBonus,
                        Value = 9
                    }
                };
            })
            .EditComponent<AbilityEffectRunAction>(c =>
            {
                foreach (var rootAction in c.Actions.Actions)
                {
                    ActionTreeUtils.Walk(rootAction, a =>
                    {
                        if (a is Conditional cond)
                        {
                            foreach (var condition in cond.ConditionsChecker.Conditions)
                            {
                                if (condition is ContextConditionHitDice hpCond && hpCond.HitDice == 9)
                                {
                                    hpCond.SharedValue = AbilitySharedValue.Heal;
                                }
                            }
                        }
                    });
                }
            })
            .Configure();


            BlueprintAbility HarmDamage = BlueprintTool.Get<BlueprintAbility>("3da67f8b941308348b7101e7ef418f52");
            AbilityConfigurator.For(HarmDamage)
                .EditComponent<ContextRankConfig>(c =>
                {
                    c.m_Max = 200;
                })
                .Configure();
        }
    }
}
