using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.EntitySystem.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Blueprints.Classes;
using BlueprintCore.Blueprints.CustomConfigurators;

namespace CruoromancerTweaks.ModifiedContent.Classes
{
    internal class Alchemist
    {
        public static void Configure()
        {
            //召唤僵尸战士能力资源
            BlueprintAbilityResource SummonZombieAbilityResource =
            AbilityResourceConfigurator.New("SummonZombieAbilityResource", "51496BBF-171F-4717-B239-E15EC8F6AD2D")
            .SetIcon(BlueprintTool.Get<BlueprintAbility>("57fcf8016cf04da4a8b33d2add14de7e").Icon)
            .SetMax(1)
            .SetMin(0)
            .SetUseMax(false)
            .SetMaxAmount(
                new BlueprintAbilityResource.Amount
                {
                    BaseValue = 1
                }
            )
            .Configure();
            //召唤僵尸战士能力资源特性
            BlueprintFeature SummonZombieAbilityResourceFeature =
                FeatureConfigurator.New("SummonZombieAbilityResourceFeature", "0D651278-BFAC-4051-A7F2-1E89C5835C12")
                .SetHideInUI(true)
                .SetHideInCharacterSheetAndLevelUp(true)
                .AddAbilityResources(
                    resource: SummonZombieAbilityResource,
                    restoreAmount: true)
                .Configure();
            //召唤僵尸战士能力
            BlueprintAbility SummonZombieAbility =
                AbilityConfigurator.New("SummonZombieAbility", "312A67A9-89C3-4C54-8F59-F1A452037876")
                .SetType(AbilityType.SpellLike)
                .SetRange(AbilityRange.Personal)
                .SetDisplayName("SummonZombieAbility.Name")
                .SetDescription("SummonZombieAbility.Description")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("57fcf8016cf04da4a8b33d2add14de7e").Icon)
                .AddAbilityEffectRunAction(
                    actions: new ActionList
                    {
                        Actions = [new ContextActionSpawnMonster 
                        {
                            m_Blueprint = BlueprintTool.Get<BlueprintUnit>("73b5669f19bd4ad2820ced314deb7361").ToReference<BlueprintUnitReference>(),
                            m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                            CountValue = new ContextDiceValue
                            {
                                BonusValue = 1,
                                DiceType = DiceType.Zero,
                                DiceCountValue = new ContextValue
                                {
                                    Value = 0
                                }
                            },
                            DurationValue = new ContextDurationValue
                            {
                                BonusValue = new ContextValue
                                {
                                    m_AbilityParameter = AbilityParameterType.Level,
                                    Property = UnitProperty.Level,
                                    PropertyName = ContextPropertyName.Value1,
                                    ValueRank = AbilityRankType.Default,
                                    ValueShared = AbilitySharedValue.Damage,
                                    ValueType = ContextValueType.CasterProperty
                                },
                                DiceCountValue = new ContextValue{
                                    Value = 0
                                },
                                DiceType = DiceType.Zero,
                                Rate = DurationRate.TenMinutes
                            },
                            AfterSpawn = new ActionList
                            {
                                Actions = [
                                    new ContextActionApplyBuff
                                    {
                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("6fcdf014694b2b542a867763b4369cb3").ToReference<BlueprintBuffReference>(),
                                        Permanent = true
                                    },
                                    new Conditional
                                    {
                                        ConditionsChecker = new ConditionsChecker
                                        {
                                            Conditions = [
                                                new ContextConditionCasterHasFact
                                                {
                                                    m_Fact = BlueprintTool.Get<BlueprintFeature>("d45a818a74b842f3912adc22419b2760").ToReference<BlueprintUnitFactReference>()
                                                }
                                            ]
                                        },
                                        IfTrue = new ActionList{
                                            Actions = [
                                                new ContextActionApplyBuff 
                                                {
                                                    m_Buff = BlueprintTool.Get<BlueprintBuff>("621c0d208a6e4ef4b3e2c0e50421082a").ToReference<BlueprintBuffReference>(),
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
                                                new ContextConditionCasterHasFact
                                                {
                                                    m_Fact = BlueprintTool.Get<BlueprintFeature>("38155ca9e4055bb48a89240a2055dcc3").ToReference<BlueprintUnitFactReference>()
                                                }
                                            ]
                                        },
                                        IfTrue = new ActionList{
                                            Actions = [
                                                new ContextActionApplyBuff
                                                {
                                                    m_Buff = BlueprintTool.Get<BlueprintBuff>("169d03bbccdbdc542ae1a57d83673d80").ToReference<BlueprintBuffReference>(),
                                                    Permanent = true
                                                }
                                            ]
                                        }
                                    },
                                    new ContextActionRemoveBuff
                                    {
                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3").ToReference<BlueprintBuffReference>()
                                    }
                                ]
                            }
                        }]
                    }
                )
                .AddAbilityResourceLogic(
                    amount: 1,
                    isSpendResource: true,
                    requiredResource: SummonZombieAbilityResource
                )
                .SetType(AbilityType.Spell)
                .SetSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Summoning)
                .Configure();
            //简易赋生添加召唤僵尸战士
            FeatureConfigurator.For("d45a818a74b842f3912adc22419b2760")
                .AddFacts([
                    SummonZombieAbilityResourceFeature,
                    SummonZombieAbility
                ])
                .Configure();

            //召唤食尸鬼狩猎大师能力资源
            BlueprintAbilityResource SummonGhoulHuntMasterAbilityResource =
            AbilityResourceConfigurator.New("SummonGhoulHuntMasterAbilityResource", "B4166342-37D4-4967-8C6E-3A14132605FF")
            .SetIcon(BlueprintTool.Get<BlueprintAbility>("76a11b460be25e44ca85904d6806e5a3").Icon)
            .SetMax(1)
            .SetMin(0)
            .SetUseMax(false)
            .SetMaxAmount(
                new BlueprintAbilityResource.Amount
                {
                    BaseValue = 1
                }
            )
            .Configure();
            //召唤食尸鬼狩猎大师能力资源特性
            BlueprintFeature SummonGhoulHuntMasterAbilityResourceFeature =
                FeatureConfigurator.New("SummonGhoulHuntMasterAbilityResourceFeature", "1CFF0989-F583-419B-9515-AF50F74AE518")
                .SetHideInUI(true)
                .SetHideInCharacterSheetAndLevelUp(true)
                .AddAbilityResources(
                    resource: SummonGhoulHuntMasterAbilityResource,
                    restoreAmount: true)
                .Configure();
            //召唤食尸鬼狩猎大师能力
            BlueprintAbility SummonGhoulHuntMasterAbility =
                AbilityConfigurator.New("SummonGhoulHuntMasterAbility", "8A2FE81F-A7BE-44DF-BF54-1D3C401C9BBE")
                .SetType(AbilityType.SpellLike)
                .SetRange(AbilityRange.Personal)
                .SetDisplayName("SummonGhoulHuntMasterAbility.Name")
                .SetDescription("SummonGhoulHuntMasterAbility.Description")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("76a11b460be25e44ca85904d6806e5a3").Icon)
                .AddAbilityEffectRunAction(
                    actions: new ActionList
                    {
                        Actions = [new ContextActionSpawnMonster
                        {
                            m_Blueprint = BlueprintTool.Get<BlueprintUnit>("f648b655fc6c48de8c88656cbb9856ac").ToReference<BlueprintUnitReference>(),
                            m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                            CountValue = new ContextDiceValue
                            {
                                BonusValue = 1,
                                DiceType = DiceType.Zero,
                                DiceCountValue = new ContextValue
                                {
                                    Value = 0
                                }
                            },
                            DurationValue = new ContextDurationValue
                            {
                                BonusValue = new ContextValue
                                {
                                    m_AbilityParameter = AbilityParameterType.Level,
                                    Property = UnitProperty.Level,
                                    PropertyName = ContextPropertyName.Value1,
                                    ValueRank = AbilityRankType.Default,
                                    ValueShared = AbilitySharedValue.Damage,
                                    ValueType = ContextValueType.CasterProperty
                                },
                                DiceCountValue = new ContextValue{
                                    Value = 0
                                },
                                DiceType = DiceType.Zero,
                                Rate = DurationRate.TenMinutes
                            },
                            AfterSpawn = new ActionList
                            {
                                Actions = [
                                    new ContextActionApplyBuff
                                    {
                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("6fcdf014694b2b542a867763b4369cb3").ToReference<BlueprintBuffReference>(),
                                        Permanent = true
                                    },
                                    new Conditional
                                    {
                                        ConditionsChecker = new ConditionsChecker
                                        {
                                            Conditions = [
                                                new ContextConditionCasterHasFact
                                                {
                                                    m_Fact = BlueprintTool.Get<BlueprintFeature>("d45a818a74b842f3912adc22419b2760").ToReference<BlueprintUnitFactReference>()
                                                }
                                            ]
                                        },
                                        IfTrue = new ActionList{
                                            Actions = [
                                                new ContextActionApplyBuff
                                                {
                                                    m_Buff = BlueprintTool.Get<BlueprintBuff>("621c0d208a6e4ef4b3e2c0e50421082a").ToReference<BlueprintBuffReference>(),
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
                                                new ContextConditionCasterHasFact
                                                {
                                                    m_Fact = BlueprintTool.Get<BlueprintFeature>("38155ca9e4055bb48a89240a2055dcc3").ToReference<BlueprintUnitFactReference>()
                                                }
                                            ]
                                        },
                                        IfTrue = new ActionList{
                                            Actions = [
                                                new ContextActionApplyBuff
                                                {
                                                    m_Buff = BlueprintTool.Get<BlueprintBuff>("169d03bbccdbdc542ae1a57d83673d80").ToReference<BlueprintBuffReference>(),
                                                    Permanent = true
                                                }
                                            ]
                                        }
                                    },
                                    new ContextActionRemoveBuff
                                    {
                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3").ToReference<BlueprintBuffReference>()
                                    }
                                ]
                            }
                        }]
                    }
                )
                .AddAbilityResourceLogic(
                    amount: 1,
                    isSpendResource: true,
                    requiredResource: SummonGhoulHuntMasterAbilityResource
                )
                .SetType(AbilityType.Spell)
                .SetSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Summoning)
                .Configure();
            //精通赋生添加召唤食尸鬼狩猎大师
            FeatureConfigurator.For("855ad33f36d3421da7293d8cfd4a02ef")
                .AddFacts([
                    SummonGhoulHuntMasterAbilityResourceFeature,
                    SummonGhoulHuntMasterAbility
                ])
                .Configure();

            //召唤血腥骨兽能力资源
            BlueprintAbilityResource SummonBloodyBonesServantAbilityResource =
            AbilityResourceConfigurator.New("SummonBloodyBonesServantAbilityResource", "21D89D6D-3C19-4681-879E-63BE93015DF9")
            .SetIcon(BlueprintTool.Get<BlueprintAbility>("8ba9b6e4df4c46a597154e2b8e7e6e4a").Icon)
            .SetMax(1)
            .SetMin(0)
            .SetUseMax(false)
            .SetMaxAmount(
                new BlueprintAbilityResource.Amount
                {
                    BaseValue = 1
                }
            )
            .Configure();
            //召唤血腥骨兽能力资源特性
            BlueprintFeature SummonBloodyBonesServantAbilityResourceFeature =
                FeatureConfigurator.New("SummonBloodyBonesServantAbilityResourceFeature", "D4ACB8F8-759C-43C0-AE9E-69AB51430928")
                .SetHideInUI(true)
                .SetHideInCharacterSheetAndLevelUp(true)
                .AddAbilityResources(
                    resource: SummonBloodyBonesServantAbilityResource,
                    restoreAmount: true)
                .Configure();
            //召唤血腥骨兽能力
            BlueprintAbility SummonBloodyBonesServantAbility =
                AbilityConfigurator.New("SummonBloodyBonesServantAbility", "14F9DAEF-5BA1-4E1F-AF72-0A69E0CA993F")
                .SetType(AbilityType.SpellLike)
                .SetRange(AbilityRange.Personal)
                .SetDisplayName("SummonBloodyBonesServantAbility.Name")
                .SetDescription("SummonBloodyBonesServantAbility.Description")
                .SetIcon(BlueprintTool.Get<BlueprintAbility>("8ba9b6e4df4c46a597154e2b8e7e6e4a").Icon)
                .AddAbilityEffectRunAction(
                    actions: new ActionList
                    {
                        Actions = [new ContextActionSpawnMonster
                        {
                            m_Blueprint = BlueprintTool.Get<BlueprintUnit>("439f19b76e2144a19250c18c3c580971").ToReference<BlueprintUnitReference>(),
                            m_SummonPool = BlueprintTool.Get<BlueprintSummonPool>("d94c93e7240f10e41ae41db4c83d1cbe").ToReference<BlueprintSummonPoolReference>(),
                            CountValue = new ContextDiceValue
                            {
                                BonusValue = 1,
                                DiceType = DiceType.Zero,
                                DiceCountValue = new ContextValue
                                {
                                    Value = 0
                                }
                            },
                            DurationValue = new ContextDurationValue
                            {
                                BonusValue = new ContextValue
                                {
                                    m_AbilityParameter = AbilityParameterType.Level,
                                    Property = UnitProperty.Level,
                                    PropertyName = ContextPropertyName.Value1,
                                    ValueRank = AbilityRankType.Default,
                                    ValueShared = AbilitySharedValue.Damage,
                                    ValueType = ContextValueType.CasterProperty
                                },
                                DiceCountValue = new ContextValue{
                                    Value = 0
                                },
                                DiceType = DiceType.Zero,
                                Rate = DurationRate.TenMinutes
                            },
                            AfterSpawn = new ActionList
                            {
                                Actions = [
                                    new ContextActionApplyBuff
                                    {
                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("6fcdf014694b2b542a867763b4369cb3").ToReference<BlueprintBuffReference>(),
                                        Permanent = true
                                    },
                                    new Conditional
                                    {
                                        ConditionsChecker = new ConditionsChecker
                                        {
                                            Conditions = [
                                                new ContextConditionCasterHasFact
                                                {
                                                    m_Fact = BlueprintTool.Get<BlueprintFeature>("d45a818a74b842f3912adc22419b2760").ToReference<BlueprintUnitFactReference>()
                                                }
                                            ]
                                        },
                                        IfTrue = new ActionList{
                                            Actions = [
                                                new ContextActionApplyBuff
                                                {
                                                    m_Buff = BlueprintTool.Get<BlueprintBuff>("621c0d208a6e4ef4b3e2c0e50421082a").ToReference<BlueprintBuffReference>(),
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
                                                new ContextConditionCasterHasFact
                                                {
                                                    m_Fact = BlueprintTool.Get<BlueprintFeature>("38155ca9e4055bb48a89240a2055dcc3").ToReference<BlueprintUnitFactReference>()
                                                }
                                            ]
                                        },
                                        IfTrue = new ActionList{
                                            Actions = [
                                                new ContextActionApplyBuff
                                                {
                                                    m_Buff = BlueprintTool.Get<BlueprintBuff>("169d03bbccdbdc542ae1a57d83673d80").ToReference<BlueprintBuffReference>(),
                                                    Permanent = true
                                                }
                                            ]
                                        }
                                    },
                                    new ContextActionRemoveBuff
                                    {
                                        m_Buff = BlueprintTool.Get<BlueprintBuff>("df3950af5a783bd4d91ab73eb8fa0fd3").ToReference<BlueprintBuffReference>()
                                    }
                                ]
                            }
                        }]
                    }
                )
                .AddAbilityResourceLogic(
                    amount: 1,
                    isSpendResource: true,
                    requiredResource: SummonBloodyBonesServantAbilityResource
                )
                .SetType(AbilityType.Spell)
                .SetSpellDescriptor(Kingmaker.Blueprints.Classes.Spells.SpellDescriptor.Summoning)
                .Configure();
            //高等赋生添加召唤血腥骨兽
            FeatureConfigurator.For("d538d0cfbd3a4f16bd2b1682c57e06f8")
                .AddFacts([
                    SummonBloodyBonesServantAbilityResourceFeature,
                    SummonBloodyBonesServantAbility
                ])
                .Configure();
        }
    }
}
