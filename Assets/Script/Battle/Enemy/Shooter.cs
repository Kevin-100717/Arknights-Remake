using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Reflection;

public class Shooter : MonoBehaviour 
{
    public Enemy enemyController;
    private float AttackInterval;
    private Char attackTarget;
    private EnemyState lastState;
    [Range(0.0f,1.0f)]public float scale = 1;
    [Header("攻击范围半径")]
    [Header("!!! 在Enemy中设置Shoot -> 射击动画")]
    [Range(0.1f,20.0f)]public float attackRange = 2.0f;
    private void Start(){
        enemyController = GetComponent<Enemy>();
        enemyController.skeletonAnimation.state.Complete += delegate{
            if(enemyController.state == EnemyState.Shoot){
                enemyController.state = lastState; // 直接恢复枚举值
            }
        };
    }

    private void Update(){
        AttackInterval -= Time.deltaTime;
        if(enemyController.attackTarget == null && AttackInterval < 0){
            AttackInterval = enemyController.damageInterval;
            foreach(GameObject c in GameObject.FindGameObjectsWithTag("char")){
                if(enemyController.ObjectIsAvailable(c.GetComponent<Char>()) && Vector3.Distance(c.transform.position,transform.position) <= attackRange){
                    lastState = enemyController.state; // 直接保存枚举值
                    enemyController.state = EnemyState.Shoot;
                    attackTarget = c.GetComponent<Char>();
                    break;
                }
            }
        }
    }
    public void ShootChar(){
        if(enemyController.state != EnemyState.Shoot) return;
        float dmg = enemyController.damage.damage;
        Damage damage = new Damage()
        {
            damage = dmg * scale,
            dt = Damage.DamageType.Physics
        };
        attackTarget.TakeDamage(damage);

    }
}