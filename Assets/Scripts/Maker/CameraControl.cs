using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public float moveSpeed = 5f;        // 移动速度
    public float heightLimitMin = -10f; // Y轴最低高度
    public float heightLimitMax = 30f;  // Y轴最高高度
    public float depthLimitMin = -20f;  // Z轴最近距离
    public float depthLimitMax = 20f;   // Z轴最远距离
    public float scrollSensitivity = 2f; // 滚轮敏感度

    private Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");  // A/D键
        float vertical = Input.GetAxis("Vertical");      // W/S键
        float scroll = Input.GetAxis("Mouse ScrollWheel"); // 鼠标滚轮

        // A/D键控制X轴移动（左右）
        Vector3 newPosition = transform.position;
        newPosition.x += horizontal * moveSpeed * Time.deltaTime;

        // W/S键控制Y轴移动（上下）
        newPosition.y += vertical * moveSpeed * Time.deltaTime;
        newPosition.y = Mathf.Clamp(newPosition.y, heightLimitMin, heightLimitMax);

        // 鼠标滚轮控制Z轴移动（前后）
        newPosition.z += scroll * scrollSensitivity;
        newPosition.z = Mathf.Clamp(newPosition.z, depthLimitMin, depthLimitMax);

        // 更新相机位置
        transform.position = newPosition;
    }
}
