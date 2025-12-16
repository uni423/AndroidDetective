using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Gravity")]
    public float gravity = -9.81f;       // 중력 가속도
    public float groundedGravity = -2f;  // 바닥 붙여주는 약한 힘

    [Header("Interaction")]
    public float interactDistance = 5f;
    public LayerMask interactLayerMask = ~0;   // 필요하면 "Clue" 레이어만 선택해서 쓰기

    private float xRotation = 0f;
    private CharacterController controller;
    public Transform playerCameraTransform;
    public Camera playerCamera;

    private Vector3 verticalVelocity;

    private Outline currentOutline;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (InGameManager.Instance.m_InGameStep == InGameStep.Playing)
        {
            MouseLook();
            Move();

            HandleCenterRayInteraction();
        }
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 카메라 상하 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 플레이어 좌우 회전
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal"); // A,D
        float z = Input.GetAxis("Vertical");   // W,S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 2) 중력 / 바닥 처리
        if (controller.isGrounded && verticalVelocity.y < 0)
        {
            // 완전 0으로 두면 살짝 떠 있는 느낌이라
            // 조금 음수로 눌러주면 계단/경사에 잘 붙어다님
            verticalVelocity.y = groundedGravity;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    /// <summary>
    /// 카메라 정면 기준 Ray로 Outline + Clue 클릭 처리
    /// </summary>
    void HandleCenterRayInteraction()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            // 1) Outline 처리
            var outline = hit.collider.GetComponentInParent<Outline>();

            if (outline != currentOutline)
            {
                // 이전 거 꺼주고
                if (currentOutline != null)
                    currentOutline.DisableOutline();

                currentOutline = outline;

                // 새로 바라본 애 켜주고
                if (currentOutline != null)
                    currentOutline.EnableOutline();
            }

            // 2) 클릭 처리 (좌클릭 한 번)
            if (Input.GetMouseButtonDown(0))
            {
                // (1) Clue 먼저 체크
                var clue = hit.collider.GetComponentInParent<ClueMeta>();
                if (clue != null && clue.isFind == false)
                {
                    InGameManager.Instance.FindGetClue(clue);
                    return;
                }

                // (2) NPC Suspect 체크
                var suspect = hit.collider.GetComponentInParent<NpcSuspectMeta>();
                if (suspect != null)
                {
                    InGameManager.Instance.NPCChatStart(suspect.data);
                    return;
                }
            }
        }
        else
        {
            // 아무 것도 안 보고 있으면 Outline 끄기
            if (currentOutline != null)
            {
                currentOutline.DisableOutline();
                currentOutline = null;
            }
        }
    }
}
