using BlueprintCore.Blueprints.CustomConfigurators.Classes.Spells;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruoromancerTweaks.ModifiedContent.BugFix
{
    internal class FingerOfDeath
    {
        public static void Configure()
        {
            //修改死亡一指特效
            AbilityConfigurator.For("6f1dcf6cfa92d1948a740195707c0dbe")
                .EditComponent<AbilitySpawnFx>(c =>
                {
                    c.PrefabLink = new PrefabLink
                    {
                        AssetId = "79cd62b68ac540a48b90fc622142432e"
                    };
                    c.Anchor = AbilitySpawnFxAnchor.ClickedTarget;
                })
                .Configure();
            //修改法术书，删除死亡一指（bug）
            SpellListConfigurator.For("ba0401fdeb4062f40a7aa95b6f07fe89")
                .ModifySpellsByLevel(c =>
                {
                    if (c.SpellLevel == 7)
                    {
                        c.m_Spells.Remove(
                        BlueprintTool.Get<BlueprintAbility>("e03024c8a03f454db5b78660f524757d").ToReference<BlueprintAbilityReference>()
                        );
                    }

                })
                .Configure();
        }
    }
}
