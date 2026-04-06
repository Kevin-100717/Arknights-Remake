using GameData.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletController))]
public class PaoDan : MonoBehaviour
{
    [Header("抛物线参数")]
    public float maxHeight = 5f;        // 抛物线最高点高度

    private BulletController bulletController;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float totalDistance;
    private float startTime;
    private Vector3 lastPosition; // 用于计算飞行方向

    void Start()
    {
        bulletController = GetComponent<BulletController>();
        if (bulletController == null)
        {
            Debug.LogError("PaoDan 组件必须附加到拥有 BulletController 脚本的游戏对象上！", this);
            return;
        }

        // 设置不使用默认移动
        bulletController.useMove = false;

        // 记录起始位置和目标位置
        startPosition = transform.position;
        if (bulletController.target != null)
        {
            targetPosition = bulletController.target.transform.position;
        }
        else
        {
            targetPosition = bulletController.dist;
        }

        // 计算总距离
        totalDistance = Vector3.Distance(startPosition, targetPosition);

        // 记录开始时间和初始位置
        startTime = Time.time;
        lastPosition = transform.position;
    }

    void Update()
    {
        // 保存当前位置用于计算方向
        Vector3 currentPosition = transform.position;

        // 计算已经飞行的距离
        float timeElapsed = Time.time - startTime;
        float distanceTraveled = bulletController.speed * timeElapsed;

        // 计算飞行进度 (0到1)
        float progress = totalDistance > 0 ? distanceTraveled / totalDistance : 0;
        progress = Mathf.Clamp01(progress);

        // 计算水平方向的位置（直线插值）
        Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, progress);

        // 计算Z轴方向的抛物线偏移（Z向上为负，所以用负值）
        // 使用二次函数 z = -4 * maxHeight * x * (1 - x) 产生抛物线轨迹
        float heightOffset = -4 * maxHeight * progress * (1 - progress);

        // 设置当前位置（Z轴抛物线，向上为负）
        Vector3 newPosition = new Vector3(
            horizontalPosition.x,
            horizontalPosition.y,
            horizontalPosition.z + heightOffset
        );

        transform.position = newPosition;

        // 调整朝向始终朝向飞行方向
        Vector3 direction = newPosition - lastPosition;
        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        // 更新上一帧位置
        lastPosition = newPosition;

        // 检查是否到达目标
        if (progress >= 1.0f || Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            // 到达目标，执行命中逻辑
            HitTarget();
        }
    }

    /// <summary>
    /// 炮弹命中目标时的处理
    /// </summary>
    private void HitTarget()
    {
        // 让BulletController处理伤害和销毁
        if (bulletController != null)
        {
            // 手动触发命中检测
            if (bulletController.target != null)
            {
                // 如果有目标，直接对目标造成伤害
                if (!bulletController.target.isDead && bulletController.target.state != CharacterState.Die)
                {
                    bulletController.target.TakeDamage(bulletController.damage);
                }
            }
            else
            {
                // 如果没有目标，检查是否接近目标点
                if (Vector3.Distance(transform.position, bulletController.dist) < 0.2f)
                {
                    // 这里可以添加对位置伤害的处理（如果有需要的话）
                }
            }

            // 销毁炮弹
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.yellow;

        // 绘制抛物线轨迹预览
        if (bulletController != null)
        {
            Vector3 start = transform.position;
            Vector3 end = bulletController.target != null ?
                         bulletController.target.transform.position :
                         bulletController.dist;

            // 绘制多个点来显示抛物线
            int segments = 20;
            Vector3 previousPoint = start;

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector3 horizontalPos = Vector3.Lerp(start, end, t);
                float heightOffset = -4 * maxHeight * t * (1 - t);
                Vector3 currentPoint = new Vector3(
                    horizontalPos.x,
                    horizontalPos.y,
                    horizontalPos.z + heightOffset
                );

                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}
