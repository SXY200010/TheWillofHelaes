using BlueprintCore.Blueprints.Configurators;
using BlueprintCore.Blueprints.Configurators.UnitLogic.ActivatableAbilities;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Craft;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Utility;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class DirgeBard
    {
        public static void Configure()
        {
            //坟场奥秘增加法术
            FeatureConfigurator.For("cb8892ce2e28bfe459e86b4da7840b70")
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("450af0402422b0b4980d9c2175869612"),
                spellLevel:1)
                .Configure();
            FeatureConfigurator.For("c7d5adc54f48666489934eddcc87b098")
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("08cb5f4c3b2695e44971bf5c45205df0"),
                spellLevel: 2)
                .Configure();
            BlueprintFeature DirgeBardSecretsOfTheGraveAnimateDeadLesserFeature =
            FeatureConfigurator.New("DirgeBardSecretsOfTheGraveAnimateDeadLesser", "B52568C5-B01F-446C-AD96-5405E8406B0C")
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("57fcf8016cf04da4a8b33d2add14de7e"),
                spellLevel: 3)
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("8eead52509987034ea9025d60cc05985"),
                spellLevel: 3)
                .SetHideInCharacterSheetAndLevelUp(true)
                .SetHideInUI(true)
                .SetIsClassFeature(true)
                .Configure();
            FeatureConfigurator.For("8557fd70d47149c47b8162117604a776")
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("4b76d32feb089ad4499c3a1ce8e1ac27"),
                spellLevel: 4)
                .Configure();
            FeatureConfigurator.For("f3b2bffb507e1124ab54362431db9827")
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("1cde0691195feae45bab5b83ea3f221e"),
                spellLevel: 5)
                .Configure();
            FeatureConfigurator.For("02474ed9b759247499386eeaec1c304a")
                .AddKnownSpell(
                characterClass: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                spell: BlueprintTool.Get<BlueprintAbility>("a9a52760290591844a96d0109e30e04d"),
                spellLevel: 6)
                .Configure();
            FeatureConfigurator.For("ff657fbdd7576f647a24fbe25f684e18")
                .SetDescription("DirgeBardSecretsOfTheGraveFeature.Description")
                .RemoveComponents(
                predicate: c =>{
                    if (c.GetType().Equals(typeof(AddFeatureOnClassLevel)))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .AddFeatureOnClassLevel(
                clazz: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                feature: BlueprintTool.Get<BlueprintFeature>("cb8892ce2e28bfe459e86b4da7840b70"),
                level: 2)
                .AddFeatureOnClassLevel(
                clazz: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                feature: BlueprintTool.Get<BlueprintFeature>("c7d5adc54f48666489934eddcc87b098"),
                level: 6)
                .AddFeatureOnClassLevel(
                clazz: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                feature: DirgeBardSecretsOfTheGraveAnimateDeadLesserFeature,
                level: 8)
                .AddFeatureOnClassLevel(
                clazz: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                feature: BlueprintTool.Get<BlueprintFeature>("8557fd70d47149c47b8162117604a776"),
                level: 10)
                .AddFeatureOnClassLevel(
                clazz: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                feature: BlueprintTool.Get<BlueprintFeature>("f3b2bffb507e1124ab54362431db9827"),
                level: 14)
                .AddFeatureOnClassLevel(
                clazz: BlueprintTool.Get<BlueprintCharacterClass>("772c83a25e2268e448e841dcd548235f"),
                feature: BlueprintTool.Get<BlueprintFeature>("02474ed9b759247499386eeaec1c304a"),
                level: 17)
                .Configure();
            FeatureConfigurator.For("d90802b1a74ae004aac439036263825a")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("d2aeac47450c76347aebbc02e4f463e0").Icon)
                .Configure();
            //尸骸之舞每轮亡者增援
            BlueprintAbility DirgeBardAnimateDeadReinforcement =
                AbilityConfigurator.New("DirgeBardAnimateDeadReinforcement", "4ECD2605-200E-4145-BE9B-CC076B943995")
                .CopyFrom(BlueprintTool.Get<BlueprintAbility>("37ac433e7814af44daf9f8144ae29892"))
                .SetDisplayName("DirgeBardAnimateDeadReinforcement.Name")
                .AddSpellComponent(
                    school: SpellSchool.Necromancy)
                .AddAbilityEffectRunAction(
                    actions: new ActionList
                    {
                        Actions = [
                            new ContextActionSpawnMonster{
                                CountValue = new ContextDiceValue{
                                    BonusValue = new ContextValue{
                                        Value = 1
                                    },
                                    DiceCountValue = new ContextValue{
                                        Value = 0
                                    },
                                    DiceType = DiceType.Zero
                                },
                                DurationValue = new ContextDurationValue{
                                    BonusValue = new ContextValue{
                                        Value = 999
                                    },
                                    DiceCountValue = new ContextValue{
                                        Value = 0
                                    },
                                    DiceType = DiceType.Zero,
                                    Rate = DurationRate.Days
                                },
                                m_Blueprint = BlueprintTool.Get<BlueprintUnit>("53e228ba7fe18104c93dc4b7294a1b30").ToReference<BlueprintUnitReference>(),
                                m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                                AfterSpawn = new ActionList{
                                    Actions = [
                                        new ContextActionApplyBuff{
                                            m_Buff = BlueprintTool.Get<BlueprintBuff>("50d51854cf6a3434d96a87d050e1d09a").ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        },
                                        new ContextActionApplyBuff{
                                            m_Buff = BlueprintTool.Get<BlueprintBuff>("c6a53420cd2353e47a6e1b8d8df7af86").ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        },
                                        new Conditional{
                                            ConditionsChecker = new ConditionsChecker{
                                                Conditions = [new ContextConditionCasterHasFact{
                                                    m_Fact = BlueprintTool.Get<BlueprintUnitFact>("d45a818a74b842f3912adc22419b2760").ToReference<BlueprintUnitFactReference>()
                                                }],
                                                Operation = Operation.And
                                            },
                                            IfTrue = new ActionList{
                                                Actions = [new ContextActionApplyBuff{
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("c6a53420cd2353e47a6e1b8d8df7af86").ToReference<BlueprintBuffReference>(),
                                                        Permanent = true
                                                }]
                                            }
                                        }
                                    ]
                                }
                            }
                        ]
                    })
                .Configure();
            //尸骸之舞增幅buff
            BlueprintBuff DirgeBardDanceOfTheDeadBuff =
                BuffConfigurator.New("DirgeBardDanceOfTheDeadBuff", "0DC6CED7-4024-46BC-848F-EAE178DBCAD1")
                .AddContextStatBonus(
                    stat: StatType.AdditionalAttackBonus,
                    value: new ContextValue
                    {
                        Value = 3
                    },
                    descriptor: ModifierDescriptor.Competence)
                .AddContextStatBonus(
                    stat: StatType.AdditionalDamage,
                    value: new ContextValue
                    {
                        Value = 3
                    },
                    descriptor: ModifierDescriptor.Competence)
                .Configure();
            BlueprintBuff DirgeBardDanceOfTheDeadLv15Buff =
                BuffConfigurator.New("DirgeBardDanceOfTheDeadLv15Buff", "753932AB-5843-4A68-912E-0930F1A08207")
                .AddContextStatBonus(
                    stat: StatType.AdditionalAttackBonus,
                    value: new ContextValue
                    {
                        Value = 4
                    },
                    descriptor: ModifierDescriptor.Competence)
                .AddContextStatBonus(
                    stat: StatType.AdditionalDamage,
                    value: new ContextValue
                    {
                        Value = 4
                    },
                    descriptor: ModifierDescriptor.Competence)
                .Configure();
            BlueprintBuff DirgeBardDanceOfTheDeadLv20Buff =
                BuffConfigurator.New("DirgeBardDanceOfTheDeadLv20Buff", "3BEF71D6-CE23-40CE-BF76-67B00932A7DC")
                .AddContextStatBonus(
                    stat: StatType.AdditionalAttackBonus,
                    value: new ContextValue
                    {
                        Value = 5
                    },
                    descriptor: ModifierDescriptor.Competence)
                .AddContextStatBonus(
                    stat: StatType.AdditionalDamage,
                    value: new ContextValue
                    {
                        Value = 5
                    },
                    descriptor: ModifierDescriptor.Competence)
                .Configure();
            //尸骸之舞范围效果
            BlueprintAbilityAreaEffect DirgeBardDanceOfTheDeadAreaEffect =
                AbilityAreaEffectConfigurator.New("DirgeBardDanceOfTheDeadAreaEffect", "E2466ADE-39B8-45E2-97C7-C6BF92490430")
                .AddAbilityAreaEffectRunAction(
                    unitEnter: new ActionList{
                        Actions = [
                            new Conditional{
                                ConditionsChecker = new ConditionsChecker{
                                    Conditions = [
                                        new ContextConditionHasFact{
                                            m_Fact = BlueprintTool.Get<BlueprintFeature>("9e201f3ef56a4c76a55b24b0ab53262c").ToReference<BlueprintUnitFactReference>()
                                        }
                                    ]
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = [
                                        new ContextActionApplyBuff{
                                            m_Buff = DirgeBardDanceOfTheDeadBuff.ToReference<BlueprintBuffReference>(),
                                            AsChild = true,
                                            Permanent = true
                                        },
                                        // Lv >= 15
                                        new Conditional{
                                            ConditionsChecker = new ConditionsChecker{
                                                Conditions = [
                                                    new ContextConditionCompare{
                                                        m_Type = ContextConditionCompare.Type.GreaterOrEqual,
                                                        CheckValue = new ContextValue{
                                                            ValueType = ContextValueType.CasterProperty,
                                                            Property = UnitProperty.Level
                                                        },
                                                        TargetValue = new ContextValue{
                                                            ValueType = ContextValueType.Simple,
                                                            Value = 15
                                                        }
                                                    }
                                                ]
                                            },
                                            IfTrue = new ActionList{
                                                Actions = [
                                                    new ContextActionApplyBuff{
                                                        m_Buff = DirgeBardDanceOfTheDeadLv15Buff.ToReference<BlueprintBuffReference>(),
                                                        AsChild = true,
                                                        Permanent = true
                                                    },
                                                    new ContextActionApplyBuff{
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("85af9f0c5d29e5e4fa2e75ca70442487").ToReference<BlueprintBuffReference>(),
                                                        AsChild = true,
                                                        Permanent = true
                                                    },
                                                    new ContextActionApplyBuff{
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("035ed56eb973f0e469a288ff5991c9ff").ToReference<BlueprintBuffReference>(),
                                                        AsChild = true,
                                                        Permanent = true
                                                    }
                                                ]
                                            }
                                        },
                                        // Lv >= 20
                                        new Conditional{
                                            ConditionsChecker = new ConditionsChecker{
                                                Conditions = [
                                                    new ContextConditionCompare{
                                                        m_Type = ContextConditionCompare.Type.GreaterOrEqual,
                                                        CheckValue = new ContextValue{
                                                            ValueType = ContextValueType.CasterProperty,
                                                            Property = UnitProperty.Level
                                                        },
                                                        TargetValue = new ContextValue{
                                                            ValueType = ContextValueType.Simple,
                                                            Value = 20
                                                        }
                                                    }
                                                ]
                                            },
                                            IfTrue = new ActionList{
                                                Actions = [
                                                    new ContextActionApplyBuff{
                                                        m_Buff = DirgeBardDanceOfTheDeadLv20Buff.ToReference<BlueprintBuffReference>(),
                                                        AsChild = true,
                                                        Permanent = true
                                                    },
                                                    new ContextActionApplyBuff{
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("b8da3ec045ec04845a126948e1f4fc1a").ToReference<BlueprintBuffReference>(),
                                                        AsChild = true,
                                                        Permanent = true
                                                    },
                                                    new ContextActionApplyBuff{
                                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("17113651fa8b40c2a7bbf9721462e646").ToReference<BlueprintBuffReference>(),
                                                        AsChild = true,
                                                        Permanent = true
                                                    }
                                                ]
                                            }
                                        }
                                    ]
                                }
                            }
                        ]
                    },
                    unitExit: new ActionList
                    {
                        Actions = [
                            new ContextActionRemoveBuff{
                                m_Buff = DirgeBardDanceOfTheDeadBuff.ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff{
                                m_Buff = DirgeBardDanceOfTheDeadLv15Buff.ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff{
                                m_Buff = DirgeBardDanceOfTheDeadLv20Buff.ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff{
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("85af9f0c5d29e5e4fa2e75ca70442487").ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff{
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("035ed56eb973f0e469a288ff5991c9ff").ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff{
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("b8da3ec045ec04845a126948e1f4fc1a").ToReference<BlueprintBuffReference>()
                            },
                            new ContextActionRemoveBuff{
                                m_Buff = BlueprintTool.Get<BlueprintBuff>("17113651fa8b40c2a7bbf9721462e646").ToReference<BlueprintBuffReference>()
                            }
                        ]
                    })
                .SetShape(AreaEffectShape.Cylinder)
                .SetSize(new Feet
                {
                    m_Value = 30
                })
                .Configure();
            BuffConfigurator.For("be20c77faa6c05f4387e52985e3358fc")
                .EditComponent<AddFactContextActions>(c =>
                {
                    c.NewRound = new ActionList
                    {
                        Actions = [
                            new ContextActionCastSpell{
                                m_Spell = DirgeBardAnimateDeadReinforcement.ToReference<BlueprintAbilityReference>()
                            }
                        ]
                    };
                })
                .AddAreaEffect(areaEffect: DirgeBardDanceOfTheDeadAreaEffect)
                .Configure();
        }
    }
}
