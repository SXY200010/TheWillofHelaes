using BlueprintCore.Blueprints.Configurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Buffs;
using BlueprintCore.Utils;
using BlueprintCore.Utils.Types;
using CruoromancerTweaks.ModifiedContent.Classes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruoromancerTweaks.ModifiedContent.Feats
{
    internal class DhampirRace
    {
        public static void Configure()
        {
            //移除饮血者限制、增加恢复血池点数
            AbilityConfigurator.For("be35623d2c561c649b98a1794216e9f9")
                .RemoveComponents(
                predicate: c => c is AbilityTargetHasOneOfConditionsOrHP
                )
                .EditComponent<AbilityEffectRunAction>(
                c =>
                {
                    var list = c.Actions.Actions.ToList();
                    list.Add(
                        new ContextActionOnContextCaster
                        {
                            Actions = new ActionList
                            {
                                Actions = [
                                    new ContextRestoreResource
                                    {
                                        m_Resource = BlueprintTool.Get<BlueprintAbilityResource>("d776a80421be411e8d3b62fc956b80d2").ToReference<BlueprintAbilityResourceReference>(),
                                        ContextValueRestoration = true,
                                        Value = new ContextValue
                                        {
                                            Value = 1,
                                            Property = UnitProperty.Level
                                        }
                                    }
                                ]
                            },
                            TargetAsCaster = true
                        }
                    );
                    
                    c.Actions.Actions = [.. list];
                })
                .SetDescription("BloodDrinker.Description")
                .Configure(delayed: true);

            //移除饮血者描述
            FeatureConfigurator.For("96983d50aca1d214e8adc57a39b41c25")
                .SetDescription("BloodDrinker.Description")
                .Configure(delayed: true);
            BuffConfigurator.For("796b2c438fbb3414eb2cd041b625e8af")
                .SetDescription("BloodDrinker.Description")
                .Configure(delayed: true);
            //增加Buff施法者等级+1
            BlueprintBuff DhampirResonanceBuffLv4 = 
                BuffConfigurator.New("DhampirResonanceBuffLv4", "E180EB5E-8F4A-42EF-BE48-FBCB19261256")
                .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .AddIncreaseSpellSchoolCasterLevel(
                    bonusLevel: 1,
                    school: SpellSchool.Necromancy,
                    descriptor: ModifierDescriptor.Racial
                )
                .Configure(delayed: true);
            BlueprintBuff DhampirResonanceBuffLv8 =
                BuffConfigurator.New("DhampirResonanceBuffLv8", "C1EF7B8C-5E64-4E6C-82D4-45D54EC4A7FF")
                .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .AddIncreaseSpellSchoolCasterLevel(
                    bonusLevel: 1,
                    school: SpellSchool.Necromancy,
                    descriptor: ModifierDescriptor.Racial
                )
                .Configure(delayed: true);
            BlueprintBuff DhampirResonanceBuffLv12 =
                BuffConfigurator.New("DhampirResonanceBuffLv12", "582D513F-6BCF-4036-AF63-F31A48871771")
                .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .AddIncreaseSpellSchoolCasterLevel(
                    bonusLevel: 1,
                    school: SpellSchool.Necromancy,
                    descriptor: ModifierDescriptor.Racial
                )
                .Configure(delayed: true);
            BlueprintBuff DhampirResonanceBuffLv16 =
                BuffConfigurator.New("DhampirResonanceBuffLv16", "2DCCD255-ACDB-4388-B39E-B9F8356861D6")
                .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .AddIncreaseSpellSchoolCasterLevel(
                    bonusLevel: 1,
                    school: SpellSchool.Necromancy,
                    descriptor: ModifierDescriptor.Racial
                )
                .Configure(delayed: true);
            BlueprintBuff DhampirResonanceBuffLv20 =
                BuffConfigurator.New("DhampirResonanceBuffLv20", "DE12DBE0-ACC1-4B61-AB94-38F0BAAC2382")
                .SetFlags(BlueprintBuff.Flags.HiddenInUi)
                .AddIncreaseSpellSchoolCasterLevel(
                    bonusLevel: 1,
                    school: SpellSchool.Necromancy,
                    descriptor: ModifierDescriptor.Racial
                )
                .Configure(delayed: true);
            //增加能力吸血鬼共鸣
            BlueprintAbility DhampirResonanceAbility = 
                AbilityConfigurator.New("DhampirResonance", "47B00733-6F50-4B5A-85CD-91F505C06455")
                .SetDisplayName("DhampirResonance.Name")
                .SetDescription("DhampirResonanceAbility.Description")
                .SetType(AbilityType.Supernatural)
                .SetRange(AbilityRange.Personal)
                .SetIcon(BlueprintTool.Get<BlueprintFeature>("703c488da4a34dc883dd7787e8d7ed51").Icon)
                .AddComponent<AbilityEffectRunAction>(c=>
                {
                    c.Actions = new ActionList
                    {
                        Actions =
                        [
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Operation = Operation.And,
                                    Conditions =
                                    [
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
                                                Value = 4
                                            }
                                        }
                                    ]
                                },
                                IfTrue = new ActionList
                                {
                                    Actions =
                                    [
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = DhampirResonanceBuffLv4.ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        }
                                    ]
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Operation = Operation.And,
                                    Conditions = new Condition[]
                                    {
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
                                                Value = 8
                                            }
                                        }
                                    }
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = DhampirResonanceBuffLv8.ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        }
                                    }
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Operation = Operation.And,
                                    Conditions = new Condition[]
                                    {
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
                                                Value = 12
                                            }
                                        }
                                    }
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = DhampirResonanceBuffLv12.ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        }
                                    }
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Operation = Operation.And,
                                    Conditions = new Condition[]
                                    {
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
                                                Value = 16
                                            }
                                        }
                                    }
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = DhampirResonanceBuffLv16.ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        }
                                    }
                                }
                            },
                            new Conditional
                            {
                                ConditionsChecker = new ConditionsChecker
                                {
                                    Operation = Operation.And,
                                    Conditions = new Condition[]
                                    {
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
                                                Value = 20
                                            }
                                        }
                                    }
                                },
                                IfTrue = new ActionList
                                {
                                    Actions = new GameAction[]
                                    {
                                        new ContextActionApplyBuff
                                        {
                                            m_Buff = DhampirResonanceBuffLv20.ToReference<BlueprintBuffReference>(),
                                            Permanent = true
                                        }
                                    }
                                }
                            },
                        ]
                    };
                })
                .Configure(delayed: true);
            //增加特长吸血鬼共鸣（不可选）
            BlueprintFeature DhampirResonanceFeature = 
            FeatureConfigurator.New("DhampirResonanceFeature", "A822CF12-D0C8-41F2-A741-E0307E21D7EA")
                .SetDisplayName("DhampirResonance.Name")
                .SetDescription("DhampirResonance.Description")
                .SetGroups(FeatureGroup.Racial)
                .SetIcon(BlueprintTool.Get<BlueprintFeature>("703c488da4a34dc883dd7787e8d7ed51").Icon)
                .AddFacts(new List<Blueprint<BlueprintUnitFactReference>>
                {
                    DhampirResonanceAbility
                })
                .Configure(delayed: true);
            //增加种族特性
            RaceConfigurator.For("64e8b7d5f1ae91d45bbf1e56a3fdff01")
                .AddToFeatures(DhampirResonanceFeature)
                .Configure(delayed: true);
        }
    }
}
