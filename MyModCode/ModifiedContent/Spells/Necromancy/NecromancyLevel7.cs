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
    internal class NecromancyLevel7
    {
        public static void Configure()
        {
            BlueprintAbilityAreaEffect PlagueStormCackleFeverArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("6cae3c64989f3684bb078efcfa9021a1");
            BlueprintAbilityAreaEffect PlagueStormShakesArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("706df636208b2864aa80032b72e0aabd");
            BlueprintAbilityAreaEffect PlagueStormBlindingSicknessArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("b342e42d2ed58484c8dff9150d18f4e4");
            BlueprintAbilityAreaEffect PlagueStormMindFireArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("6fa0adacca8d00f4aaba1e8df77a318f");
            BlueprintAbilityAreaEffect PlagueStormBubonicPlagueArea = BlueprintTool.Get<BlueprintAbilityAreaEffect>("ba09d51375db5f34790184443416d84b");


            BlueprintBuff[] aggravations = new BlueprintBuff[] { ContagionAggravationBuffs.MindFireAggravation, ContagionAggravationBuffs.ShakesAggravation, ContagionAggravationBuffs.BlindingSicknessAggravation, ContagionAggravationBuffs.BrainFeverAggravation, ContagionAggravationBuffs.BubonicPlagueAggravation };
            BlueprintAbilityAreaEffect[] PlagueStorms = new BlueprintAbilityAreaEffect[] { PlagueStormMindFireArea, PlagueStormShakesArea, PlagueStormBlindingSicknessArea, PlagueStormCackleFeverArea, PlagueStormBubonicPlagueArea };

            for (int i = 0; i < PlagueStorms.Length; i++)
            {
                var plagueStorm = PlagueStorms[i];
                var aggravation = aggravations[i];

                AbilityAreaEffectConfigurator.For(plagueStorm)
                    .EditComponent<AbilityEffectRunAction>(c =>
                    {
                        var list = c.Actions.Actions.ToList();

                        list.Add (new ContextActionApplyBuff
                        {
                            m_Buff = aggravation.ToReference<BlueprintBuffReference>(),

                            DurationValue = new ContextDurationValue
                            {
                                Rate = DurationRate.Rounds,
                                DiceType = DiceType.Zero,
                                DiceCountValue = 0,
                                BonusValue = new ContextValue
                                {
                                    ValueType = ContextValueType.CasterProperty,
                                    Property = UnitProperty.Level
                                }
                            }
                        });

                        c.Actions.Actions = list.ToArray();
                    })
                    .Configure();
            }

            BlueprintAbility UmbralStrike = BlueprintTool.Get<BlueprintAbility>("474ed0aa656cc38499cc9a073d113716");

            // 暗影打击射线数：基础1根；17级2根；22级及以后3根。
            // 相比地狱烈焰射线（11级起、每4级成长），起始等级晚1级，改为每5级成长。
            var umbralStrikeProjectileCountRank =
                ContextRankConfigs
                    .CasterLevel(type: AbilityRankType.ProjectilesCount, max: 3)
                    .WithStartPlusDivStepProgression(divisor: 5, start: 12);

            AbilityConfigurator.For(UmbralStrike)
                .RemoveComponents(c => c is ContextRankConfig rank &&
                    rank.m_Type == AbilityRankType.ProjectilesCount)
                .AddContextRankConfig(umbralStrikeProjectileCountRank)
                .EditComponent<AbilityDeliverProjectile>(c =>
                {
                    // MaxProjectilesCount 只限制发射数量；必须提供足够的投射物条目。
                    // 保留暗影打击原有的射线外观，最多准备三根。
                    if (c.m_Projectiles == null || c.m_Projectiles.Length == 0)
                        throw new InvalidOperationException("UmbralStrike has no projectile blueprint.");

                    var projectile = c.m_Projectiles[0];
                    c.m_Projectiles = Enumerable.Repeat(projectile, 3).ToArray();
                    c.Type = AbilityProjectileType.Simple;
                    c.UseMaxProjectilesCount = true;
                    c.MaxProjectilesCountRank = AbilityRankType.ProjectilesCount;
                    c.DelayBetweenProjectiles = 0.4f;
                })
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    foreach (var action in c.Actions.Actions)
                    {
                        if (action is ContextActionDealDamage dealDamage)
                        {
                            // 强韧豁免成功不再使伤害减半。
                            // 不能删除 ContextActionConditionalSaved；目盲就在它的失败分支中。
                            dealDamage.HalfIfSaved = false;

                            if (dealDamage.DamageType.Type == DamageType.Energy &&
                                dealDamage.DamageType.Energy == DamageEnergyType.Cold)
                            {
                                dealDamage.DamageType = new DamageTypeDescription
                                {
                                    Type = DamageType.Energy,
                                    Energy = DamageEnergyType.Unholy
                                };
                            }
                        }
                    }
                })
                .Configure();

                BlueprintAbility BoneSpearConjure = BlueprintTool.Get<BlueprintAbility>("ca8bc7d438e6b004a87222f3c32572f2");

                var boneSpearDamageRank = ContextRankConfigs.CasterLevel();
                boneSpearDamageRank.m_Type = AbilityRankType.DamageBonus;

                AbilityConfigurator.For(BoneSpearConjure)
                    .AddContextRankConfig(boneSpearDamageRank)
                    .EditComponent<AbilityEffectRunAction>(c =>
                    {
                        foreach (var rootAction in c.Actions.Actions)
                        {
                            ActionTreeUtils.Walk(rootAction, a =>
                            {
                                if (a is ContextActionConditionalSaved saved)
                                {
                                    // 不追加，直接替换 Succeed 分支
                                    saved.Succeed = new ActionList
                                    {
                                        Actions = new GameAction[]
                                        {
                                            new ContextActionDealDamage
                                            {
                                                Value = new ContextDiceValue
                                                {
                                                    DiceType = DiceType.D6,
                                                    DiceCountValue = new ContextValue
                                                    {
                                                        ValueType = ContextValueType.Rank,
                                                        ValueRank = AbilityRankType.DamageBonus
                                                    },
                                                    BonusValue = 0
                                                },
                                                DamageType = new DamageTypeDescription
                                                {
                                                    Type = DamageType.Untyped
                                                }
                                            }
                                        }
                                    };
                                }
                            });
                        }
                    })
                    .Configure();

                BlueprintAbility FingerOfDeath = BlueprintTool.Get<BlueprintAbility>("6f1dcf6cfa92d1948a740195707c0dbe");

                AbilityConfigurator.For(FingerOfDeath)
                    .EditComponent<AbilityEffectRunAction>(c =>
                    {
                        foreach (var rootAction in c.Actions.Actions)
                        {
                            ActionTreeUtils.Walk(rootAction, a =>
                            {
                                if (a is ContextActionConditionalSaved saved)
                                {
                                    // 保存原来的失败分支：也就是原本的每施法者等级10点污邪伤害
                                    ActionList originalFailed = saved.Failed;

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
                                                        // 目标生命骰/等级 < 施法者等级
                                                        new ContextConditionCompare
                                                        {
                                                            m_Type = ContextConditionCompare.Type.Less,

                                                            CheckValue = new ContextValue
                                                            {
                                                                ValueType = ContextValueType.TargetProperty,
                                                                Property = UnitProperty.Level
                                                            },

                                                            TargetValue = new ContextValue
                                                            {
                                                                ValueType = ContextValueType.CasterProperty,
                                                                Property = UnitProperty.Level
                                                            }
                                                        }
                                                    }
                                                },

                                                // 低于施法者等级：强韧失败立即死亡
                                                IfTrue = new ActionList
                                                {
                                                    Actions = new GameAction[]
                                                    {
                                                        new ContextActionKill()
                                                    }
                                                },

                                                // 不低于施法者等级：走原来的失败分支
                                                // 也就是原本的每施法者等级10点污邪伤害
                                                IfFalse = originalFailed
                                            }
                                        }
                                    };
                                }
                            });
                        }
                    })
                    .Configure();

                BlueprintAbility RestoreUndead = BlueprintTool.Get<BlueprintAbility>("a9dbff7a630003d4eafa6c9dd203cb7e");

                AbilityConfigurator.For(RestoreUndead)
                    .AddAbilityCanTargetDead()
                    .EditComponent<AbilityEffectRunAction>(c =>
                    {
                        foreach (var action in c.Actions.Actions)
                        {
                            if (action is Conditional conditional)
                            {
                                var ifTrueActions = conditional.IfTrue.Actions.ToList();

                                // 再次配置时不要把同一个条件动作重复插入。
                                if (ifTrueActions.Any(a => a is Conditional existing &&
                                    existing.ConditionsChecker?.Conditions?.Any(condition =>
                                        condition is RestoreUndeadTargetIsDead) == true))
                                    continue;

                                var resurrect = new ContextActionResurrect
                                {
                                    // 这里不要 full restore，因为后面的 HealUnit 本来就会负责治疗
                                    FullRestore = false
                                };

                                // 复活动作自身会播放复活特效，因此只让死者执行它。
                                // 活着的亡灵仍照常执行后面的 HealUnit。
                                var resurrectDeadOnly = new Conditional
                                {
                                    ConditionsChecker = new ConditionsChecker
                                    {
                                        Operation = Operation.And,
                                        Conditions = new Condition[] { new RestoreUndeadTargetIsDead() }
                                    },
                                    IfTrue = new ActionList { Actions = new GameAction[] { resurrect } },
                                    IfFalse = new ActionList { Actions = Array.Empty<GameAction>() }
                                };

                                // 兼容先前版本已插入的无条件复活动作。
                                int oldResurrectIndex = ifTrueActions.FindIndex(a => a is ContextActionResurrect);
                                if (oldResurrectIndex >= 0)
                                {
                                    ifTrueActions[oldResurrectIndex] = resurrectDeadOnly;
                                    conditional.IfTrue.Actions = ifTrueActions.ToArray();
                                    continue;
                                }

                                // 插到 HealUnit 前面：先复活，再治疗
                                int healIndex = ifTrueActions.FindIndex(a => a is HealUnit);

                                if (healIndex >= 0)
                                    ifTrueActions.Insert(healIndex, resurrectDeadOnly);
                                else
                                    ifTrueActions.Insert(0, resurrectDeadOnly);

                                conditional.IfTrue.Actions = ifTrueActions.ToArray();
                            }
                        }
                    })
                    .EditComponent<AbilityTargetsAround>(c =>
                    {
                        c.m_IncludeDead = true;
                    })
                    .Configure();

            // 借用重伤术命中目标时的视觉特效。
            var harmFxSource = BlueprintTool.Get<BlueprintAbility>("137af566f68fd9b428e2e12da43c1482");

            var harmTargetFx = harmFxSource.ComponentsArray
                .OfType<AbilitySpawnFx>()
                .First(fx =>
                    fx.Anchor == AbilitySpawnFxAnchor.SelectedTarget &&
                    fx.Time == AbilitySpawnFxTime.OnApplyEffect);

            AbilityConfigurator.For( BlueprintTool.Get<BlueprintAbility>( "a9dbff7a630003d4eafa6c9dd203cb7e"))
                .AddComponent<AbilitySpawnFx>(fx =>
                {
                    fx.PrefabLink = harmTargetFx.PrefabLink;
                    fx.Anchor = AbilitySpawnFxAnchor.SelectedTarget;
                    fx.PositionAnchor = harmTargetFx.PositionAnchor;
                    fx.OrientationAnchor = harmTargetFx.OrientationAnchor;
                    fx.OrientationMode = harmTargetFx.OrientationMode;
                    fx.Delay = harmTargetFx.Delay;
                    fx.DestroyOnCast = harmTargetFx.DestroyOnCast;
                    fx.Time = AbilitySpawnFxTime.OnApplyEffect;
                })
                .Configure();

            BlueprintAbility SymbolOfWeakness = BlueprintTool.Get<BlueprintAbility>("8b02310b46e54de1ae9ba7161831938d");

                AbilityConfigurator.For(SymbolOfWeakness)
                    .AddToAvailableMetamagic(Metamagic.Empower)
                    .AddToAvailableMetamagic(Metamagic.Maximize)
                    .Configure();
        }
    }

    internal class RestoreUndeadTargetIsDead : ContextCondition
    {
        public override bool CheckCondition()
        {
            return Target.Unit != null && Target.Unit.Descriptor.State.IsDead;
        }

        public override string GetConditionCaption()
        {
            return "Target is dead";
        }
    }
}
