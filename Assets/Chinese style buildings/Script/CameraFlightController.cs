using UnityEngine;

public class CameraFlightController : MonoBehaviour
{
    // 飞行速度
    public float moveSpeed = 10.0f;
    public float rotationSpeed = 720.0f; // 每秒旋转度数

    // 相机控制
    public float cameraPitchLimit = 80.0f; // 相机上下旋转的限制角度
    private float cameraPitch = 0.0f; // 相机的俯仰角

    void Start()
    {
        // 锁定光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 获取输入
        float moveForward = Input.GetAxis("Vertical");
        float moveStrafe = Input.GetAxis("Horizontal");
        float moveUp = Input.GetAxis("Mouse ScrollWheel"); // 使用鼠标滚轮控制上下移动

        // 计算移动方向
        Vector3 moveDirection = new Vector3(moveStrafe, moveUp, moveForward);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed * Time.deltaTime;

        // 移动摄像机
        transform.position += moveDirection;

        // 计算旋转
        float rotationX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // 旋转摄像机（俯仰角限制）
        cameraPitch -= rotationY;
        cameraPitch = Mathf.Clamp(cameraPitch, -cameraPitchLimit, cameraPitchLimit);
        transform.rotation = Quaternion.Euler(cameraPitch, transform.eulerAngles.y + rotationX, 0);
    }
}
