using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Interaction")]
    public float interactDistance = 5f;
    public LayerMask interactLayerMask = ~0;   // 필요하면 "Clue" 레이어만 선택해서 쓰기

    private float xRotation = 0f;
    private CharacterController controller;
    public Transform playerCameraTransform;
    public Camera playerCamera;

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
                var clue = hit.collider.GetComponentInParent<ClueMeta>();
                if (clue != null && clue.isFind == false)
                {
                    InGameManager.Instance.FindGetClue(clue);
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
