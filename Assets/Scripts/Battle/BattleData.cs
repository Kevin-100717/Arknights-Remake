using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameData.MapData
{
    public class BattleData
    {
        [JsonProperty("options")]
        public OptionsEntity Options { get; set; }

        [JsonProperty("levelId")]
        public object LevelId { get; set; }

        [JsonProperty("mapId")]
        public object MapId { get; set; }

        [JsonProperty("bgmEvent")]
        public string BgmEvent { get; set; }

        [JsonProperty("environmentSe")]
        public object EnvironmentSe { get; set; }

        [JsonProperty("mapData")]
        public MapDataEntity MapData { get; set; }

        [JsonProperty("tilesDisallowToLocate")]
        public List<object> TilesDisallowToLocate { get; set; }

        [JsonProperty("runes")]
        public List<RuneEntity> Runes { get; set; }

        [JsonProperty("optionalRunes")]
        public object OptionalRunes { get; set; }

        [JsonProperty("globalBuffs")]
        public List<object> GlobalBuffs { get; set; }

        [JsonProperty("routes")]
        public List<RouteEntity> Routes { get; set; }

        [JsonProperty("extraRoutes")]
        public List<object> ExtraRoutes { get; set; }

        [JsonProperty("enemies")]
        public List<object> Enemies { get; set; }

        [JsonProperty("enemyDbRefs")]
        public List<EnemyDbRefEntity> EnemyDbRefs { get; set; }

        [JsonProperty("waves")]
        public List<WaveEntity> Waves { get; set; }

        [JsonProperty("branches")]
        public BranchesEntity Branches { get; set; }

        [JsonProperty("predefines")]
        public PredefinesEntity Predefines { get; set; }

        [JsonProperty("hardPredefines")]
        public object HardPredefines { get; set; }

        [JsonProperty("excludeCharIdList")]
        public object ExcludeCharIdList { get; set; }

        [JsonProperty("randomSeed")]
        public long RandomSeed { get; set; }

        [JsonProperty("operaConfig")]
        public object OperaConfig { get; set; }

        [JsonProperty("cameraPlugin")]
        public object CameraPlugin { get; set; }
    }

    public class OptionsEntity
    {
        [JsonProperty("characterLimit")]
        public long CharacterLimit { get; set; }

        [JsonProperty("maxLifePoint")]
        public long MaxLifePoint { get; set; }

        [JsonProperty("initialCost")]
        public long InitialCost { get; set; }

        [JsonProperty("maxCost")]
        public long MaxCost { get; set; }

        [JsonProperty("costIncreaseTime")]
        public long CostIncreaseTime { get; set; }

        [JsonProperty("moveMultiplier")]
        public double MoveMultiplier { get; set; }

        [JsonProperty("steeringEnabled")]
        public bool SteeringEnabled { get; set; }

        [JsonProperty("isTrainingLevel")]
        public bool IsTrainingLevel { get; set; }

        [JsonProperty("isHardTrainingLevel")]
        public bool IsHardTrainingLevel { get; set; }

        [JsonProperty("isPredefinedCardsSelectable")]
        public bool IsPredefinedCardsSelectable { get; set; }

        [JsonProperty("maxPlayTime")]
        public long MaxPlayTime { get; set; }

        [JsonProperty("functionDisableMask")]
        public string FunctionDisableMask { get; set; }

        [JsonProperty("configBlackBoard")]
        public object ConfigBlackBoard { get; set; }
    }

    public class MapDataEntity
    {
        [JsonProperty("map")]
        public List<List<long>> Map { get; set; }

        [JsonProperty("tiles")]
        public List<TileEntity> Tiles { get; set; }

        [JsonProperty("blockEdges")]
        public object BlockEdges { get; set; }

        [JsonProperty("tags")]
        public object Tags { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; }

        [JsonProperty("layerRects")]
        public object LayerRects { get; set; }
    }

    public class TileEntity
    {
        [JsonProperty("tileKey")]
        public string TileKey { get; set; }

        [JsonProperty("heightType")]
        public string HeightType { get; set; }

        [JsonProperty("buildableType")]
        public string BuildableType { get; set; }

        [JsonProperty("passableMask")]
        public string PassableMask { get; set; }

        [JsonProperty("playerSideMask")]
        public string PlayerSideMask { get; set; }

        [JsonProperty("blackboard")]
        public object Blackboard { get; set; }

        [JsonProperty("effects")]
        public object Effects { get; set; }
    }

    public class RuneEntity
    {
        [JsonProperty("difficultyMask")]
        public string DifficultyMask { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("professionMask")]
        public long ProfessionMask { get; set; }

        [JsonProperty("buildableMask")]
        public string BuildableMask { get; set; }

        [JsonProperty("blackboard")]
        public List<BlackboardEntity> Blackboard { get; set; }
    }

    public class BlackboardEntity
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("valueStr")]
        public object ValueStr { get; set; }
    }

    public class RouteEntity
    {
        [JsonProperty("motionMode")]
        public string MotionMode { get; set; }

        [JsonProperty("startPosition")]
        public PositionEntity StartPosition { get; set; }

        [JsonProperty("endPosition")]
        public PositionEntity EndPosition { get; set; }

        [JsonProperty("spawnRandomRange")]
        public SpawnRandomRangeEntity SpawnRandomRange { get; set; }

        [JsonProperty("spawnOffset")]
        public SpawnOffsetEntity SpawnOffset { get; set; }

        [JsonProperty("checkpoints")]
        public List<CheckpointEntity> Checkpoints { get; set; }

        [JsonProperty("allowDiagonalMove")]
        public bool AllowDiagonalMove { get; set; }

        [JsonProperty("visitEveryTileCenter")]
        public bool VisitEveryTileCenter { get; set; }

        [JsonProperty("visitEveryNodeCenter")]
        public bool VisitEveryNodeCenter { get; set; }

        [JsonProperty("visitEveryCheckPoint")]
        public bool VisitEveryCheckPoint { get; set; }
    }

    public class PositionEntity
    {
        [JsonProperty("row")]
        public long Row { get; set; }

        [JsonProperty("col")]
        public long Col { get; set; }
    }

    public class SpawnRandomRangeEntity
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    public class SpawnOffsetEntity
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public class CheckpointEntity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("position")]
        public PositionEntity Position { get; set; }

        [JsonProperty("reachOffset")]
        public ReachOffsetEntity ReachOffset { get; set; }

        [JsonProperty("randomizeReachOffset")]
        public bool RandomizeReachOffset { get; set; }

        [JsonProperty("reachDistance")]
        public long ReachDistance { get; set; }
    }

    public class ReachOffsetEntity
    {
        [JsonProperty("x")]
        public long X { get; set; }

        [JsonProperty("y")]
        public long Y { get; set; }
    }

    public class EnemyDbRefEntity
    {
        [JsonProperty("useDb")]
        public bool UseDb { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("overwrittenData")]
        public object OverwrittenData { get; set; }
    }

    public class WaveEntity
    {
        [JsonProperty("preDelay")]
        public long PreDelay { get; set; }

        [JsonProperty("postDelay")]
        public long PostDelay { get; set; }

        [JsonProperty("maxTimeWaitingForNextWave")]
        public long MaxTimeWaitingForNextWave { get; set; }

        [JsonProperty("fragments")]
        public List<FragmentEntity> Fragments { get; set; }

        [JsonProperty("advancedWaveTag")]
        public object AdvancedWaveTag { get; set; }
    }

    public class FragmentEntity
    {
        [JsonProperty("preDelay")]
        public long PreDelay { get; set; }

        [JsonProperty("actions")]
        public List<ActionEntity> Actions { get; set; }
    }

    public class ActionEntity
    {
        [JsonProperty("actionType")]
        public string ActionType { get; set; }

        [JsonProperty("managedByScheduler")]
        public bool ManagedByScheduler { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("preDelay")]
        public long PreDelay { get; set; }

        [JsonProperty("interval")]
        public long Interval { get; set; }

        [JsonProperty("routeIndex")]
        public long RouteIndex { get; set; }

        [JsonProperty("blockFragment")]
        public bool BlockFragment { get; set; }

        [JsonProperty("autoPreviewRoute")]
        public bool AutoPreviewRoute { get; set; }

        [JsonProperty("autoDisplayEnemyInfo")]
        public bool AutoDisplayEnemyInfo { get; set; }

        [JsonProperty("isUnharmfulAndAlwaysCountAsKilled")]
        public bool IsUnharmfulAndAlwaysCountAsKilled { get; set; }

        [JsonProperty("hiddenGroup")]
        public object HiddenGroup { get; set; }

        [JsonProperty("randomSpawnGroupKey")]
        public object RandomSpawnGroupKey { get; set; }

        [JsonProperty("randomSpawnGroupPackKey")]
        public object RandomSpawnGroupPackKey { get; set; }

        [JsonProperty("randomType")]
        public string RandomType { get; set; }

        [JsonProperty("refreshType")]
        public string RefreshType { get; set; }

        [JsonProperty("weight")]
        public long Weight { get; set; }

        [JsonProperty("dontBlockWave")]
        public bool DontBlockWave { get; set; }

        [JsonProperty("forceBlockWaveInBranch")]
        public bool ForceBlockWaveInBranch { get; set; }
    }

    public class BranchesEntity
    {
        [JsonProperty("faust_ballis")]
        public FaustBallisEntity FaustBallis { get; set; }
    }

    public class FaustBallisEntity
    {
        [JsonProperty("phases")]
        public List<PhaseEntity> Phases { get; set; }
    }

    public class PhaseEntity
    {
        [JsonProperty("preDelay")]
        public long PreDelay { get; set; }

        [JsonProperty("actions")]
        public List<ActionEntity> Actions { get; set; }
    }

    public class PredefinesEntity
    {
        [JsonProperty("characterInsts")]
        public List<object> CharacterInsts { get; set; }

        [JsonProperty("tokenInsts")]
        public List<TokenInstEntity> TokenInsts { get; set; }

        [JsonProperty("characterCards")]
        public List<object> CharacterCards { get; set; }

        [JsonProperty("tokenCards")]
        public List<object> TokenCards { get; set; }
    }

    public class TokenInstEntity
    {
        [JsonProperty("position")]
        public PositionEntity Position { get; set; }

        [JsonProperty("direction")]
        public string Direction { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }

        [JsonProperty("uniEquipIds")]
        public object UniEquipIds { get; set; }

        [JsonProperty("showSpIllust")]
        public bool ShowSpIllust { get; set; }

        [JsonProperty("inst")]
        public InstEntity Inst { get; set; }

        [JsonProperty("skillIndex")]
        public long SkillIndex { get; set; }

        [JsonProperty("mainSkillLvl")]
        public long MainSkillLvl { get; set; }

        [JsonProperty("skinId")]
        public object SkinId { get; set; }

        [JsonProperty("tmplId")]
        public object TmplId { get; set; }

        [JsonProperty("overrideSkillBlackboard")]
        public object OverrideSkillBlackboard { get; set; }
    }

    public class InstEntity
    {
        [JsonProperty("characterKey")]
        public string CharacterKey { get; set; }

        [JsonProperty("level")]
        public long Level { get; set; }

        [JsonProperty("phase")]
        public string Phase { get; set; }

        [JsonProperty("favorPoint")]
        public long FavorPoint { get; set; }

        [JsonProperty("potentialRank")]
        public long PotentialRank { get; set; }
    }
}
