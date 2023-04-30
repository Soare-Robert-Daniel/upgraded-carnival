using Map;
using Mobs;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private EventChannel eventChannel;
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
        manager.OnZoneControllerNumberChanged += ManagerOnOnLevelUpdated;
    }

    private void ManagerOnOnLevelUpdated(int level)
    {
        Debug.Log($"[INPUT][CAMERA] Camera upper limit updated to {baseUpperLimit + level * increaseUpperLimitDistance}");
        cameraUpperLimit = baseUpperLimit + level * increaseUpperLimitDistance;
    }

    public void MouseSelect(InputAction.CallbackContext context)
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

        if (hit.collider.CompareTag("ZoneTowerOverlay"))
        {
            var id = hit.collider.transform.parent.gameObject.GetComponent<ZoneController>().zoneId;
            Debug.Log($"=> Found a room {id}");
            eventChannel.OnSelectedZoneChanged?.Invoke(id);
        }

        if (hit.collider.CompareTag("Mob"))
        {
            var id = hit.collider.gameObject.GetComponent<MobSimpleController>().MobId;
            Debug.Log($"=> Found a mob {id}");
            eventChannel.OnSelectedMobChanged?.Invoke(id);
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