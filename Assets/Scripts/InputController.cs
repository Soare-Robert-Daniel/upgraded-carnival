using Map;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private MapManager manager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform cameraCursor;

    [Header("Camera Movement Settings")]
    [SerializeField] private float cameraSpeed;

    [SerializeField] private float baseUpperLimit;
    [SerializeField] private float cameraUpperLimit;
    [SerializeField] private float cameraLowerLimit;
    [SerializeField] private float increaseUpperLimitDistance;

    private void Start()
    {
        manager.OnInitUI += Init;
    }

    private void Init()
    {
        manager.OnLevelUpdated += ManagerOnOnLevelUpdated;
        cameraUpperLimit = baseUpperLimit + increaseUpperLimitDistance;
    }

    private void ManagerOnOnLevelUpdated(int level, float value)
    {
        cameraUpperLimit = baseUpperLimit + level * increaseUpperLimitDistance;
    }

    public void SelectRoom(InputAction.CallbackContext context)
    {
        if (!manager.uiState.CanSelect)
        {
            return;
        }
        var r = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Debug.Log($"Select {Mouse.current.position.ReadValue()} with {r.origin}");

        var hit = Physics2D.Raycast(r.origin, Vector2.zero, 0f);

        if (hit.collider == null)
        {
            return;
        }

        if (hit.collider.CompareTag("RoomOverlay"))
        {
            var id = hit.collider.transform.parent.gameObject.GetComponent<RoomController>().ID;
            // Debug.Log($"Found a room {id}");
            manager.ChangeSelectedRoomWithEvent(id);
        }
    }

    public void MoveCamera(InputAction.CallbackContext context)
    {
        var value = context.ReadValue<Vector2>();
        value.x = 0f;

        switch (value.y)
        {
            case > 0f when cameraCursor.position.y > cameraUpperLimit:
            case < 0f when cameraCursor.position.y < cameraLowerLimit:
                return;
            default:
                cameraCursor.Translate(value * Time.deltaTime * cameraSpeed);
                break;
        }
    }
}