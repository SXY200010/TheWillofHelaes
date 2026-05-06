using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using HarmonyLib;
using Kingmaker.Craft;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.ElementsSystem;
using Kingmaker.Blueprints.Classes.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class Cleric
    {
        public static void Configure()
        {
            //亵渎加值buff
            BlueprintBuff UndeadSubdomainBonusBuff =
                BuffConfigurator.New("UndeadSubdomainBonusBuff", "21157DAD-B8A7-4386-A941-7E8DB42FA8C2")
                .CopyFrom(BlueprintTool.Get<BlueprintBuff>("f185e4585bda72b479956772944ee665"), 
                component =>
                {
                    if (component.GetType() == typeof(ContextRankConfig))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("3b12a2a6898d403ebf918d9a6aeb02b0").Icon)
                .SetDescription("UndeadSubdomainBaseAbility.Description")
                .AddBuffAllSkillsBonus(descriptor: ModifierDescriptor.Profane, 
                multiplier: new ContextValue 
                {
                    m_AbilityParameter = AbilityParameterType.Level,
                    Value = 1,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.Rank
                })
                .AddBuffAbilityRollsBonus(descriptor: ModifierDescriptor.Profane,
                multiplier: new ContextValue
                {
                    m_AbilityParameter = AbilityParameterType.Level,
                    Value = 1,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.Rank
                },
                affectAllStats: true,
                stat: StatType.Unknown,
                value: 1)
                .AddContextStatBonus(descriptor: ModifierDescriptor.Profane,
                value: new ContextValue
                {
                    m_AbilityParameter = AbilityParameterType.Level,
                    Value = 1,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.Rank
                },
                restrictions: new RestrictionCalculator
                {
                    Property = new PropertyCalculator
                    {
                        Operation = PropertyCalculator.OperationType.Sum,
                        TargetType = PropertyTargetType.CurrentEntity
                    }
                },
                stat: StatType.AdditionalAttackBonus)
                .AddContextStatBonus(descriptor: ModifierDescriptor.Profane,
                value: new ContextValue
                {
                    m_AbilityParameter = AbilityParameterType.Level,
                    Value = 1,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.Rank
                },
                restrictions: new RestrictionCalculator
                {
                    Property = new PropertyCalculator
                    {
                        Operation = PropertyCalculator.OperationType.Sum,
                        TargetType = PropertyTargetType.CurrentEntity
                    }
                },
                stat: StatType.SaveWill)
                .AddContextStatBonus(descriptor: ModifierDescriptor.Profane,
                value: new ContextValue
                {
                    m_AbilityParameter = AbilityParameterType.Level,
                    Value = 1,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.Rank
                },
                restrictions: new RestrictionCalculator
                {
                    Property = new PropertyCalculator
                    {
                        Operation = PropertyCalculator.OperationType.Sum,
                        TargetType = PropertyTargetType.CurrentEntity
                    }
                },
                stat: StatType.SaveReflex)
                .AddContextStatBonus(descriptor: ModifierDescriptor.Profane,
                value: new ContextValue
                {
                    m_AbilityParameter = AbilityParameterType.Level,
                    Value = 1,
                    ValueShared = AbilitySharedValue.Damage,
                    ValueType = ContextValueType.Rank
                },
                restrictions: new RestrictionCalculator
                {
                    Property = new PropertyCalculator
                    {
                        Operation = PropertyCalculator.OperationType.Sum,
                        TargetType = PropertyTargetType.CurrentEntity
                    }
                },
                stat: StatType.SaveFortitude)
                .Configure();
            //死亡之吻添加buff
            string[] DeathKissGuids = [
                "0a970f6e41bd451b91e7c7601a57dad9",
                "3b12a2a6898d403ebf918d9a6aeb02b0",
            ];
            foreach (string guid in DeathKissGuids)
            {
                AbilityConfigurator.For(guid)
                .SetDescription("UndeadSubdomainBaseAbility.Description")
                .EditComponent<AbilityEffectRunAction>(
                    c => 
                    {
                        var list = c.Actions.Actions.ToList();
                        list.Add(new Conditional
                        {
                            ConditionsChecker = new ConditionsChecker
                            {
                                Conditions = [
                                    new ContextConditionHasFact
                                    {
                                        m_Fact = BlueprintTool.Get<BlueprintFeature>("734a29b693e9ec346ba2951b27987e33").ToReference<BlueprintUnitFactReference>()
                                    }
                                ]
                            },
                            IfTrue = new ActionList
                            {
                                Actions = [
                                    new ContextActionApplyBuff
                                    {
                                        m_Buff = UndeadSubdomainBonusBuff.ToReference<BlueprintBuffReference>(),
                                        DurationValue = new ContextDurationValue
                                        {
                                            BonusValue = new ContextValue
                                            {
                                                m_AbilityParameter = AbilityParameterType.Level,
                                                Value = 0,
                                                ValueShared = AbilitySharedValue.Damage,
                                                ValueType = ContextValueType.Rank
                                            },
                                            DiceCountValue = new ContextValue
                                            {
                                                Value = 0
                                            },
                                            DiceType = DiceType.Zero,
                                            Rate = DurationRate.Rounds
                                        }
                                    }
                                ]
                            }
                        });
                        c.Actions.Actions = [.. list];
                    }
                )
                .Configure();
            }
            //转变不死生物能力
            BlueprintAbility FatalEmbraceAbility = 
                AbilityConfigurator.New("FatalEmbraceAbility", "B0224D68-3360-4462-AD95-46864C887CF4")
                .SetDisplayName("FatalEmbraceAbility.Name")
                .SetDescription("FatalEmbraceAbility.Description")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("3b12a2a6898d403ebf918d9a6aeb02b0").Icon)
                .SetSpellComponent(
                    new SpellComponent
                    {
                        School = SpellSchool.Necromancy
                    })
                .SetRange(AbilityRange.Touch)
                .SetCanTargetFriends(true)
                .SetCanTargetEnemies(true)
                .AddContextRankConfig(
                    new ContextRankConfig
                    {
                        m_BaseValueType = ContextRankBaseValueType.CasterLevel,
                        m_Progression = ContextRankProgression.AsIs
                    })
                .AddAbilityEffectRunAction(
                    actions: new ActionList
                    {
                        Actions = 
                        [
                            new ContextActionApplyBuff
                            {
                                ToCaster = false,
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("EBCDCD92-4087-40CE-8B18-F65DFD3AE5AF").ToReference<BlueprintBuffReference>(),
                                DurationValue = new ContextDurationValue
                                {
                                    Rate = DurationRate.Rounds,
                                    BonusValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank,
                                        ValueRank = AbilityRankType.Default
                                    },
                                    DiceCountValue = new ContextValue
                                    {
                                        Value = 0
                                    },
                                    DiceType = DiceType.Zero
                                }
                            }
                        ]
                    })
                .AddAbilityResourceLogic(
                    isSpendResource: true,
                    requiredResource: BlueprintTool.Get<BlueprintAbilityResource>("e24cda463b324c1faae1dfbc083d4484"))
                .Configure();
            //死亡之拥增加能力
            string[] DeathEmbraceGuids = [
                "b0acce833384b9b428f32517163c9117",
                "518a6ce2c1d141b78f54521f6b0075b1",
            ];
            foreach (string guid in DeathEmbraceGuids)
            {
                FeatureConfigurator.For(guid)
                .AddFacts([FatalEmbraceAbility])
                .Configure();
            }
        }
    }
}
