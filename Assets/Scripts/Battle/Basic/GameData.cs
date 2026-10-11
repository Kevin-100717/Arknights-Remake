using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace GameData.Game
{
    /// <summary>
    /// 官方战斗公式集合（见 wiki《游戏数据基础》）。
    /// </summary>
    public static class GameFormula
    {
        /// <summary>
        /// 攻速公式：实际攻击间隔 T = T0 / (clamp(S, 10, 600) / 100)。
        /// S 未配置（&lt;=0）时按 100 处理，等价于理论攻击间隔。
        /// </summary>
        public static float AttackInterval(float baseInterval, float attackSpeed)
        {
            float s = attackSpeed <= 0f ? 100f : Mathf.Clamp(attackSpeed, 10f, 600f);
            return baseInterval / (s / 100f);
        }

        /// <summary>
        /// 官方“就近取整”：四舍六入五成双（round-half-to-even）。
        /// </summary>
        public static float RoundHalfEven(float value)
        {
            float floor = Mathf.Floor(value);
            float frac = value - floor;
            if (frac > 0.5f) return floor + 1f;
            if (frac < 0.5f) return floor;
            return Mathf.FloorToInt(floor) % 2 == 0 ? floor : floor + 1f;
        }
    }

    [System.Serializable]
    public class SpineAnimationData
    {
        public EnemyState state;
        [SpineAnimation] public string animationName;
        public bool loop;
    }

    [System.Serializable]
    public class EnemyData
    {
        public float hp_total;
        public float hp_current;
        public Damage damage;
        public float atk_interval;
        public float attack_speed = 100;   // 官方攻速属性，参与攻击间隔计算
        public float def;
        public float magic_def;
        public float speed;
        public bool dieAddToCount = true;
        public bool haveNear = true;
        public bool haveStart = false;
        public enum EnemyType
        {
            Normal,
            Fly
        }
        public EnemyType type;
    }

    [System.Serializable]
    public class HpUIController
    {
        public Transform hpBar;
        public Transform red;
        public Transform white;
    }

    public enum EnemyState
    {
        Move,
        Attack,
        Shoot,  // 新增: 远程攻击
        Idle,
        Die,
        Attack_interval,
        EmptyIdle,
        Start
    }

    //------------------------------------------------------
    public enum CharacterState
    {
        Place,
        Start,
        Attack,
        Idle,
        Die,
        Attack_interval
    }

    public enum DamageType
    {
        Physical,
        Magic,
        Real,
        Recovery
    }

    [System.Serializable]
    public class Damage
    {
        public float damage;
        public DamageType type;
    }

    [System.Serializable]
    public class CharacterData
    {
        public float hp_total;
        public float hp_current;
        public Damage damage;
        public float atk_interval;
        public float attack_speed = 100;   // 官方攻速属性，参与攻击间隔计算
        public float def;
        public float magic_def;
        public int max_atk_num;
        public int current_def_num;
        public int def_num;
        public int respawn_time;
    }
    public enum BuildType
    {
        Highland,
        Ground
    }
    [System.Serializable]
    public class CharacterUIData
    {
        public int cost;
        public string code;
        public BuildType buildType;
        public GameObject characterPrefab;
    }
    [System.Serializable]
    public class CharacterAnimationData
    {
        public CharacterState state;
        [SpineAnimation] public string animationName;
        public bool loop;
    }
}
