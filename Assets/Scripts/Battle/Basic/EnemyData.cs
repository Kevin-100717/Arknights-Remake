using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameData.EnemyData
{
    public class EnemyData
    {
        [JsonProperty("enemies")]
        public List<EnemiesEntity> Enemies { get; set; }

    }

    public class EnemiesEntity
    {
        [JsonProperty("Key")]
        public string Key { get; set; }
        [JsonProperty("Value")]
        public List<ValueEntity> Value { get; set; }

    }

    public class ValueEntity
    {
        [JsonProperty("level")]
        public long Level { get; set; }
        [JsonProperty("enemyData")]
        public EnemyDataEntity EnemyData { get; set; }

    }

    public class EnemyDataEntity
    {
        [JsonProperty("name")]
        public NameEntity Name { get; set; }
        [JsonProperty("description")]
        public DescriptionEntity Description { get; set; }
        [JsonProperty("prefabKey")]
        public PrefabKeyEntity PrefabKey { get; set; }
        [JsonProperty("attributes")]
        public AttributesEntity Attributes { get; set; }
        [JsonProperty("applyWay")]
        public ApplyWayEntity ApplyWay { get; set; }
        [JsonProperty("motion")]
        public MotionEntity Motion { get; set; }
        [JsonProperty("enemyTags")]
        public EnemyTagsEntity EnemyTags { get; set; }
        [JsonProperty("lifePointReduce")]
        public LifePointReduceEntity LifePointReduce { get; set; }
        [JsonProperty("levelType")]
        public LevelTypeEntity LevelType { get; set; }
        [JsonProperty("rangeRadius")]
        public RangeRadiusEntity RangeRadius { get; set; }
        [JsonProperty("numOfExtraDrops")]
        public NumOfExtraDropsEntity NumOfExtraDrops { get; set; }
        [JsonProperty("viewRadius")]
        public ViewRadiusEntity ViewRadius { get; set; }
        [JsonProperty("notCountInTotal")]
        public NotCountInTotalEntity NotCountInTotal { get; set; }
        [JsonProperty("talentBlackboard")]
        public Object TalentBlackboard { get; set; }
        [JsonProperty("skills")]
        public List<SkillsEntity> Skills { get; set; }
        [JsonProperty("spData")]
        public Object SpData { get; set; }

    }

    public class NameEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public string MValue { get; set; }

    }

    public class DescriptionEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public string MValue { get; set; }

    }

    public class PrefabKeyEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public string MValue { get; set; }

    }

    public class AttributesEntity
    {
        [JsonProperty("maxHp")]
        public MaxHpEntity MaxHp { get; set; }
        [JsonProperty("atk")]
        public AtkEntity Atk { get; set; }
        [JsonProperty("def")]
        public DefEntity Def { get; set; }
        [JsonProperty("magicResistance")]
        public MagicResistanceEntity MagicResistance { get; set; }
        [JsonProperty("cost")]
        public CostEntity Cost { get; set; }
        [JsonProperty("blockCnt")]
        public BlockCntEntity BlockCnt { get; set; }
        [JsonProperty("moveSpeed")]
        public MoveSpeedEntity MoveSpeed { get; set; }
        [JsonProperty("attackSpeed")]
        public AttackSpeedEntity AttackSpeed { get; set; }
        [JsonProperty("baseAttackTime")]
        public BaseAttackTimeEntity BaseAttackTime { get; set; }
        [JsonProperty("respawnTime")]
        public RespawnTimeEntity RespawnTime { get; set; }
        [JsonProperty("hpRecoveryPerSec")]
        public HpRecoveryPerSecEntity HpRecoveryPerSec { get; set; }
        [JsonProperty("spRecoveryPerSec")]
        public SpRecoveryPerSecEntity SpRecoveryPerSec { get; set; }
        [JsonProperty("maxDeployCount")]
        public MaxDeployCountEntity MaxDeployCount { get; set; }
        [JsonProperty("massLevel")]
        public MassLevelEntity MassLevel { get; set; }
        [JsonProperty("baseForceLevel")]
        public BaseForceLevelEntity BaseForceLevel { get; set; }
        [JsonProperty("tauntLevel")]
        public TauntLevelEntity TauntLevel { get; set; }
        [JsonProperty("epDamageResistance")]
        public EpDamageResistanceEntity EpDamageResistance { get; set; }
        [JsonProperty("epResistance")]
        public EpResistanceEntity EpResistance { get; set; }
        [JsonProperty("damageHitratePhysical")]
        public DamageHitratePhysicalEntity DamageHitratePhysical { get; set; }
        [JsonProperty("damageHitrateMagical")]
        public DamageHitrateMagicalEntity DamageHitrateMagical { get; set; }
        [JsonProperty("stunImmune")]
        public StunImmuneEntity StunImmune { get; set; }
        [JsonProperty("silenceImmune")]
        public SilenceImmuneEntity SilenceImmune { get; set; }
        [JsonProperty("sleepImmune")]
        public SleepImmuneEntity SleepImmune { get; set; }
        [JsonProperty("frozenImmune")]
        public FrozenImmuneEntity FrozenImmune { get; set; }
        [JsonProperty("levitateImmune")]
        public LevitateImmuneEntity LevitateImmune { get; set; }
        [JsonProperty("disarmedCombatImmune")]
        public DisarmedCombatImmuneEntity DisarmedCombatImmune { get; set; }
        [JsonProperty("fearedImmune")]
        public FearedImmuneEntity FearedImmune { get; set; }
        [JsonProperty("palsyImmune")]
        public PalsyImmuneEntity PalsyImmune { get; set; }

    }

    public class MaxHpEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class AtkEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class DefEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class MagicResistanceEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class CostEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class BlockCntEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class MoveSpeedEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class AttackSpeedEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class BaseAttackTimeEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public double MValue { get; set; }

    }

    public class RespawnTimeEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class HpRecoveryPerSecEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class SpRecoveryPerSecEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class MaxDeployCountEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class MassLevelEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class BaseForceLevelEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class TauntLevelEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class EpDamageResistanceEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class EpResistanceEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class DamageHitratePhysicalEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class DamageHitrateMagicalEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class StunImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class SilenceImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class SleepImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class FrozenImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class LevitateImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class DisarmedCombatImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class FearedImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class PalsyImmuneEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class ApplyWayEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public string MValue { get; set; }

    }

    public class MotionEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public string MValue { get; set; }

    }

    public class EnemyTagsEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public List<string> MValue { get; set; }

    }

    public class LifePointReduceEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class LevelTypeEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public string MValue { get; set; }

    }

    public class RangeRadiusEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class NumOfExtraDropsEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class ViewRadiusEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public long MValue { get; set; }

    }

    public class NotCountInTotalEntity
    {
        [JsonProperty("m_defined")]
        public bool MDefined { get; set; }
        [JsonProperty("m_value")]
        public bool MValue { get; set; }

    }

    public class SkillsEntity
    {
        [JsonProperty("prefabKey")]
        public string PrefabKey { get; set; }
        [JsonProperty("priority")]
        public long Priority { get; set; }
        [JsonProperty("cooldown")]
        public long Cooldown { get; set; }
        [JsonProperty("initCooldown")]
        public long InitCooldown { get; set; }
        [JsonProperty("spCost")]
        public long SpCost { get; set; }
        [JsonProperty("blackboard")]
        public List<BlackboardEntity> Blackboard { get; set; }

    }

    public class BlackboardEntity
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("value")]
        public double Value { get; set; }
        [JsonProperty("valueStr")]
        public Object ValueStr { get; set; }

    }

}
