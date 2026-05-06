using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.Classes.Selection;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Craft;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Enums.Damage;
using Kingmaker.UnitLogic.FactLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Kingmaker.UnitLogic.Mechanics.Properties.UnitPropertyComponent;
using Kingmaker.RuleSystem.Rules.Damage;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class OracleRevelation
    {
        public static void Configure()
        {
            //buff：唤起死亡(力量和魅力附加先知魅调)
            BlueprintBuff OracleRevelationInvokeDeathBuff =
                BuffConfigurator.New("OracleRevelationInvokeDeathBuff", "61DB1CFD-B634-480A-AD39-2BA08A75739D")
                .AddContextRankConfig(
                        ContextRankConfigs.StatBonus(stat: StatType.Charisma)
                    )
                .AddContextStatBonus(
                    stat: StatType.Strength,
                    value: new ContextValue
                    {
                        ValueType = ContextValueType.Rank
                    })
                .AddContextStatBonus(
                    stat: StatType.Charisma,
                    value: new ContextValue
                    {
                        ValueType = ContextValueType.Rank
                    })
                .Configure();
            //能力：唤起死亡
            BlueprintAbility OracleRevelationInvokeDeathAbility =
                AbilityConfigurator.New("OracleRevelationInvokeDeathAbility", "5C1A8B8B-6A6E-4E58-B358-DBE1B0DFBBAB")
                .CopyFrom(BlueprintTool.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27"),
                component =>
                {
                    if (component.GetType() == typeof(CraftInfoComponent))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .SetDisplayName("OracleRevelationInvokeDeath.Name")
                .SetDescription("OracleRevelationInvokeDeath.Description")
                .AddContextRankConfig(new ContextRankConfig
                {
                    m_BaseValueType = ContextRankBaseValueType.StatBonus,
                    m_Stat = StatType.Charisma
                })
                .AddAbilityEffectRunAction(
                    savingThrowType: SavingThrowType.Unknown,
                    actions: new ActionList
                    {
                        Actions = [
                            new ContextActionSpawnMonster
                            {
                                m_Blueprint = BlueprintTool.Get<BlueprintUnit>("53e228ba7fe18104c93dc4b7294a1b30").ToReference<BlueprintUnitReference>(),
                                m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                                CountValue = new ContextDiceValue
                                {
                                    BonusValue = new ContextValue
                                    {
                                        Value = 1
                                    },
                                    DiceCountValue = new ContextValue
                                    {
                                        Value = 0
                                    },
                                    DiceType = DiceType.Zero
                                },
                                DurationValue = new ContextDurationValue
                                {
                                    BonusValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank
                                    },
                                    DiceCountValue = new ContextValue
                                    {
                                        Value = 0
                                    },
                                    DiceType = DiceType.Zero,
                                    Rate = DurationRate.Minutes
                                },
                                AfterSpawn = new ActionList
                                {
                                    Actions = [
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = OracleRevelationInvokeDeathBuff.ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        },
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = BlueprintTool.Get<BlueprintBuff>("50d51854cf6a3434d96a87d050e1d09a").ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        }
                                    ]
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Conditions = [
                                        new ContextConditionCompare
                                        {
                                            m_Type = ContextConditionCompare.Type.GreaterOrEqual,
                                            CheckValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.CasterProperty,
                                                Property = UnitProperty.Level
                                            },
                                            TargetValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.Simple,
                                                Value = 7
                                            }
                                        }
                                    ]
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = [
                                        new ContextActionSpawnMonster
                                        {
                                            m_Blueprint = BlueprintTool.Get<BlueprintUnit>("439f19b76e2144a19250c18c3c580971").ToReference<BlueprintUnitReference>(),
                                            m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                                            CountValue = new ContextDiceValue
                                            {
                                                BonusValue = new ContextValue
                                                {
                                                    Value = 1
                                                },
                                                DiceCountValue = new ContextValue
                                                {
                                                    Value = 0
                                                },
                                                DiceType = DiceType.Zero
                                            },
                                            DurationValue = new ContextDurationValue
                                            {
                                                BonusValue = new ContextValue
                                                {
                                                    ValueType = ContextValueType.Rank
                                                },
                                                DiceCountValue = new ContextValue
                                                {
                                                    Value = 0
                                                },
                                                DiceType = DiceType.Zero,
                                                Rate = DurationRate.Minutes
                                            },
                                            AfterSpawn = new ActionList
                                            {
                                                Actions = [
                                                    new ContextActionApplyBuff
                                                    {
                                                        m_Buff = OracleRevelationInvokeDeathBuff.ToReference<BlueprintBuffReference>(),
                                                        Permanent = true
                                                    },
                                                    new ContextActionApplyBuff
                                                    {
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("6fcdf014694b2b542a867763b4369cb3").ToReference<BlueprintBuffReference>(),
                                                        Permanent = true
                                                    }
                                                ]
                                            }
                                        }
                                    ]
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Conditions = [
                                        new ContextConditionCompare
                                        {
                                            m_Type = ContextConditionCompare.Type.GreaterOrEqual,
                                            CheckValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.CasterProperty,
                                                Property = UnitProperty.Level
                                            },
                                            TargetValue = new ContextValue
                                            {
                                                ValueType = ContextValueType.Simple,
                                                Value = 15
                                            }
                                        }
                                    ]
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = [
                                        new ContextActionSpawnMonster
                                        {
                                            m_Blueprint = BlueprintTool.Get<BlueprintUnit>("a1acea7cda5e46d8b8d0549be25e0acb").ToReference<BlueprintUnitReference>(),
                                            m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                                            CountValue = new ContextDiceValue
                                            {
                                                BonusValue = new ContextValue
                                                {
                                                    Value = 1
                                                },
                                                DiceCountValue = new ContextValue
                                                {
                                                    Value = 0
                                                },
                                                DiceType = DiceType.Zero
                                            },
                                            DurationValue = new ContextDurationValue
                                            {
                                                BonusValue = new ContextValue
                                                {
                                                    ValueType = ContextValueType.Rank
                                                },
                                                DiceCountValue = new ContextValue
                                                {
                                                    Value = 0
                                                },
                                                DiceType = DiceType.Zero,
                                                Rate = DurationRate.Minutes
                                            },
                                            AfterSpawn = new ActionList
                                            {
                                                Actions = [
                                                    new ContextActionApplyBuff
                                                    {
                                                        m_Buff = OracleRevelationInvokeDeathBuff.ToReference<BlueprintBuffReference>(),
                                                        Permanent = true
                                                    },
                                                    new ContextActionApplyBuff
                                                    {
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("0dff842f06edace43baf8a2f44207045").ToReference<BlueprintBuffReference>(),
                                                        Permanent = true
                                                    }
                                                ]
                                            }
                                        }
                                    ]
                                }
                            }
                        ]
                    })
                .AddAbilityResourceLogic(
                    isSpendResource: true,
                    requiredResource: BlueprintTool.Get<BlueprintAbilityResource>("49735e955e1d14e42a238a87709a92c0"))
                .Configure();
            //启示：唤起死亡
            BlueprintFeature OracleRevelationInvokeDeathFeature =
            FeatureConfigurator.New("OracleRevelationInvokeDeathFeature", "468989A8-4CFF-4DC4-9833-45B7CBB1A142")
                .SetDisplayName("OracleRevelationInvokeDeath.Name")
                .SetDescription("OracleRevelationInvokeDeath.Description")
                .SetGroups(FeatureGroup.OracleRevelation)
                .AddPrerequisiteFeaturesFromList([
                    BlueprintTool.Get<BlueprintFeature>("067b175d3df0d1a408efd7eee2b36b9b"),
                    BlueprintTool.Get<BlueprintFeature>("511ee1f805f8d1445b64f4c8398ca938"),
                    BlueprintTool.Get<BlueprintFeature>("e083bd3fa7dfc924fb4e6ab371eda368")
                    ])
                .AddFacts([
                    OracleRevelationInvokeDeathAbility
                ])
                .AddToFeatureSelection(BlueprintTool.Get<BlueprintFeatureSelection>("60008a10ad7ad6543b1f63016741a5d2"))
                .Configure();
            //流血Nd6Buff
            BlueprintBuff BleedNd6Buff =
                BuffConfigurator.New("BleedNd6Buff", "C80F22B5-3D55-4E4B-87CF-60CE13E43A84")
                .CopyFrom(BlueprintTool.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562"),
                component =>
                {
                    if (component.GetType() != typeof(AddFactContextActions))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .AddContextRankConfig(
                    ContextRankConfigs.ClassLevel([
                            "20ce9bf8af32bee4c8557a045ab499b1",
                            "4a6eb63858244f7ea4ed950a7e798a70",
                            "09035bb5dbb714645bb08ea7db47884c",
                            "77769e43e63647c2b32298efcc577aca"
                        ]).WithDivStepProgression(5))
                .AddFactContextActions(
                    activated: new ActionList
                    {
                        Actions = [
                            new ContextActionRemoveBuff
                            {
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("5eb68bfe186d71a438d4f85579ce40c1").ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff
                            {
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("75039846c3d85d940aa96c249b97e562").ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff
                            {
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("dc9ed761b7721c64e98fab507e2a7755").ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff
                            {
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("16249b8075ab8684ca105a78a047a5ef").ToReference<BlueprintBuffReference>()
                            }
                        ]
                    },
                    newRound: new ActionList
                    {
                        Actions = [
                            new ContextActionDealDamage
                            {
                                DamageType = new DamageTypeDescription
                                {
                                    Type = DamageType.Direct
                                },
                                Value = new ContextDiceValue
                                {
                                    BonusValue = new ContextValue
                                    {
                                        Value = 1
                                    },
                                    DiceCountValue = new ContextValue
                                    {
                                        ValueType = ContextValueType.Rank
                                    },
                                    DiceType = DiceType.D6
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Conditions = [
                                        new ContextConditionIsInCombat{}
                                    ]
                                },
                                IfFalse = new ActionList
                                {
                                    Actions = [
                                        new ContextActionRemoveSelf{}
                                    ]
                                }
                            }
                        ]
                    })
                .Configure();
            //启示：血流成河
            FeatureConfigurator.New("OracleRevelationBloodBathFeature", "B60E0CF4-33ED-45D7-9023-321758A2875D")
                .SetDisplayName("OracleRevelationBloodBath.Name")
                .SetDescription("OracleRevelationBloodBath.Description")
                .SetGroups(FeatureGroup.OracleRevelation)
                .AddPrerequisiteFeaturesFromList([
                    BlueprintTool.Get<BlueprintFeature>("067b175d3df0d1a408efd7eee2b36b9b"),
                    BlueprintTool.Get<BlueprintFeature>("511ee1f805f8d1445b64f4c8398ca938"),
                    BlueprintTool.Get<BlueprintFeature>("e083bd3fa7dfc924fb4e6ab371eda368")
                    ])
                .AddPrerequisiteClassLevel(
                    characterClass: BlueprintTool.Get<BlueprintCharacterClass>("20ce9bf8af32bee4c8557a045ab499b1"),
                    level: 7)
                .AddToFeatureSelection(BlueprintTool.Get<BlueprintFeatureSelection>("60008a10ad7ad6543b1f63016741a5d2"))
                .AddComponent<AddOutgoingDamageTrigger>(c =>
                {
                    c.EnergyType = DamageEnergyType.NegativeEnergy;
                    c.Actions = new ActionList
                    {
                        Actions = [
                            new ContextActionApplyBuff
                            {
                                m_Buff = BleedNd6Buff.ToReference<BlueprintBuffReference>(),
                                Permanent = true
                            }
                        ]
                    };
                })
                .AddAutoMetamagic(
                metamagic: Kingmaker.UnitLogic.Abilities.Metamagic.Reach,
                school: Kingmaker.Blueprints.Classes.Spells.SpellSchool.Necromancy)
                .Configure();
            //一轮一次，6秒冷却
            BlueprintBuff HarmOrSalyLivingCoolDownBuff =
                BuffConfigurator.New("HarmOrSalyLivingCoolDownBuff", "3B790B30-4185-4B6B-9B43-428FE9549FDD")
                    //.SetFlags(BlueprintBuff.Flags.HiddenInUi)
                    .SetStacking(StackingType.Replace)
                    .Configure();
            //自由动作使用杀生术
            BlueprintAbility FreeSalyLivingAbility =
                AbilityConfigurator.New("FreeSalyLivingAbility", "568E545A-8180-48EE-92C6-F6E612D07DA6")
                .CopyFrom(BlueprintTool.Get<BlueprintAbility>("a6e59e74cba46a44093babf6aec250fc"),
                component =>
                {
                    return true;
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var list = c.Actions.Actions.ToList();
                    list.Add(new ContextActionApplyBuff
                    {
                        m_Buff = HarmOrSalyLivingCoolDownBuff.ToReference<BlueprintBuffReference>(),
                        DurationValue = new ContextDurationValue
                        {
                            BonusValue = new ContextValue
                            {
                                Value = 1
                            },
                            DiceCountValue = new ContextValue
                            {
                                Value = 0
                            },
                            DiceType = DiceType.Zero,
                            Rate = DurationRate.Rounds
                        },
                        ToCaster = true
                    });
                    c.Actions.Actions = list.ToArray();
                })
                .AddAbilityCasterHasNoFacts(facts: [HarmOrSalyLivingCoolDownBuff])
                .SetActionType(Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free)
                .Configure();
            //自由动作使用重伤术
            BlueprintAbility FreeHarmAbility =
                AbilityConfigurator.New("FreeHarmAbility", "0152C3D2-40CF-40DB-959D-02CA6571D3D5")
                .CopyFrom(BlueprintTool.Get<BlueprintAbility>("3da67f8b941308348b7101e7ef418f52"),
                component =>
                {
                    return true;
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var list = c.Actions.Actions.ToList();
                    list.Add(new ContextActionApplyBuff
                    {
                        m_Buff = HarmOrSalyLivingCoolDownBuff.ToReference<BlueprintBuffReference>(),
                        DurationValue = new ContextDurationValue
                        {
                            BonusValue = new ContextValue
                            {
                                Value = 1
                            },
                            DiceCountValue = new ContextValue
                            {
                                Value = 0
                            },
                            DiceType = DiceType.Zero,
                            Rate = DurationRate.Rounds
                        },
                        ToCaster = true
                    });
                    c.Actions.Actions = list.ToArray();
                })
                .AddAbilityCasterHasNoFacts(facts: [HarmOrSalyLivingCoolDownBuff])
                .Configure();
            BlueprintAbility HarmOrSalyLivingAbility =
            AbilityConfigurator.New("HarmOrSalyLivingAbility", "17BBB0A3-96E3-4B90-B020-15C74C240CBB")
                .SetDisplayName("HarmOrSalyLivingAbility.Name")
                .SetDescription("HarmOrSalyLivingAbility.Description")
                .AddAbilityVariants(
                    variants:
                    [
                        FreeSalyLivingAbility,
                        FreeHarmAbility
                    ])
                .Configure();
            //最终启示：骸骨
            FeatureConfigurator.For("d20e184293942774eac71acb48ad7f26")
                .AddFacts(facts: [HarmOrSalyLivingAbility])
                .Configure();
        }
    }
}
