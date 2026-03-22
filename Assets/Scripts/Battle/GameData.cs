using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace GameData.Game
{
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
        public float def;
        public float magic_def;
        public float speed;
        public bool dieAddToCount = true;
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
        Idle,
        Die,
        Attack_interval
    }
    //------------------------------------------------------
    public enum CharacterState
    {
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
        public float def;
        public float magic_def;
        public int max_atk_num;
        public int current_def_num;
        public int def_num;
    }
    [System.Serializable]
    public class CharacterAnimationData
    {
        public CharacterState state;
        [SpineAnimation] public string animationName;
        public bool loop;
    }
}