using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Character Movement Settings")]
    public float walkSpeed = 5.0f;
    public float jumpForce = 8.0f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = 1;

    [Header("View Rotation Settings")]
    public float mouseSensitivity = 2.0f;
    public float lookXLimit = 80.0f;

    // Input system and component references
    private PlayerControls inputActions;
    private Rigidbody rb;
    
    // Store composite input values manually
    private Vector2 currentMovementInput = Vector2.zero;
    private Vector2 currentLookInput = Vector2.zero;
    
    // Movement and rotation variables
    private float rotationX = 0; // 摄像机上下旋转
    private float rotationY = 0; // 摄像机左右旋转
    private bool isGrounded = false;

    // 摄像机引用
    public Transform playerCamera;
    public Transform playerHoldCart;

    private void Awake()
    {
        // Get component references
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // Initialize input systems
        inputActions = new PlayerControls();
        
        // Register input callback functions
        inputActions.GamePlay.Move.performed += OnMove;
        inputActions.GamePlay.Move.canceled += OnMove;
        inputActions.GamePlay.Jump.performed += OnJump;
        inputActions.GamePlay.Look.performed += OnLook;   
        inputActions.GamePlay.Look.canceled += OnLookCanceled;    
    }

    private void Start()
    {
        // 初始化摄像机旋转角度
        if (playerCamera != null)
        {
            rotationY = playerCamera.localEulerAngles.y;
            rotationX = playerCamera.localEulerAngles.x;
            // 调整角度范围到 -180 到 180
            if (rotationX > 180) rotationX -= 360;
        }
        
        //Debug.Log("角色旋转锁定模式：只旋转摄像机，不旋转角色");
    }

    private void Update()
    {
        CheckGrounded();
        HandleRotation();
        
        // 按F1显示调试信息
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"鼠标输入: X={currentLookInput.x:F2}, Y={currentLookInput.y:F2}");
            Debug.Log($"摄像机旋转: X={rotationX:F1}, Y={rotationY:F1}");
            Debug.Log($"移动输入: {currentMovementInput}");
            Debug.Log($"在地面上: {isGrounded}");
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void CheckGrounded()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.01f;
        
        // 使用简单的射线检测
        RaycastHit hit;
        bool hasHit = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer);
        
        // 如果没检测到，尝试从角色边缘检测
        if (!hasHit)
        {
            Vector3[] offsets = new Vector3[] {
                new Vector3(0.2f, 0, 0.2f),
                new Vector3(-0.2f, 0, 0.2f),
                new Vector3(0.2f, 0, -0.2f),
                new Vector3(-0.2f, 0, -0.2f)
            };
            
            foreach (Vector3 offset in offsets)
            {
                if (Physics.Raycast(rayStart + offset, Vector3.down, out hit, groundCheckDistance + 0.1f, groundLayer))
                {
                    hasHit = true;
                    break;
                }
            }
        }
        
        isGrounded = hasHit;
        
        // 可视化调试
        Debug.DrawRay(rayStart, Vector3.down * (groundCheckDistance + 0.1f), isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// 处理移动 - 基于摄像机朝向
    /// </summary>
    private void HandleMovement()
    {
        float targetSpeed = walkSpeed;
        
        if (playerCamera != null && currentMovementInput.magnitude > 0.1f)
        {
            // 基于摄像机朝向计算移动方向
            Vector3 cameraForward = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(playerCamera.right, Vector3.up).normalized;
            
            Vector3 horizontalMovement = (cameraForward * currentMovementInput.y + cameraRight * currentMovementInput.x).normalized;
            
            Vector3 targetVelocity = horizontalMovement * targetSpeed;
            targetVelocity.y = rb.linearVelocity.y;
            rb.linearVelocity = new Vector3(targetVelocity.z, rb.linearVelocity.y, -targetVelocity.x);
        }
        else
        {
            // 没有输入时保持Y轴速度
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    /// <summary>
    /// 处理视角旋转 - 只旋转摄像机，不旋转角色
    /// </summary>
    private void HandleRotation()
    {
        if (currentLookInput.magnitude > 0.01f && playerCamera != null)
        {
            // 左右旋转（Y轴）- 只旋转摄像机
            rotationY += currentLookInput.x * mouseSensitivity;
            
            // 上下旋转（X轴）- 只旋转摄像机
            rotationX -= currentLookInput.y * mouseSensitivity;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            
            // 应用旋转到摄像机（同时包含左右和上下）
            playerCamera.localRotation = Quaternion.Euler(0, rotationY, -rotationX);
        }
    }

    // Input event callback functions
    private void OnMove(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        currentLookInput = context.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext context)
    {
        currentLookInput = Vector2.zero;
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //Debug.Log("跳跃执行! 在地面上: " + isGrounded);
        }
        else if (context.performed)
        {
            //Debug.Log("跳跃失败: 不在地面上");
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 初始化摄像机旋转
        if (playerCamera != null)
        {
            rotationY = playerCamera.localEulerAngles.y;
            Vector3 cameraEuler = playerCamera.localEulerAngles;
            rotationX = cameraEuler.x;
            if (rotationX > 180) rotationX -= 360;
        }
    }   

    private void OnDisable()
    {
        inputActions.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.GamePlay.Move.performed -= OnMove;
            inputActions.GamePlay.Move.canceled -= OnMove;
            inputActions.GamePlay.Look.performed -= OnLook;
            inputActions.GamePlay.Look.canceled -= OnLookCanceled;
            inputActions.GamePlay.Jump.performed -= OnJump;
            inputActions?.Dispose();
        }
    }

    // 在 Scene 视图中可视化
    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            // 绘制摄像机朝向
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(playerCamera.position, playerCamera.position + playerCamera.forward * 2f);
            
            // 绘制移动方向（基于摄像机）
            Gizmos.color = Color.green;
            Vector3 cameraForward = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up).normalized;
            Gizmos.DrawLine(transform.position, transform.position + cameraForward * 1.5f);
        }
        
        // 绘制地面检测
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * 0.01f;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * (groundCheckDistance + 0.1f));
    }
}