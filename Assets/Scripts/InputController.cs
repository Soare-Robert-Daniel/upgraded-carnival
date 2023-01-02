using Map;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [SerializeField] private MapManager manager;
    [SerializeField] private Camera mainCamera;

    public void SelectRoom(InputAction.CallbackContext context)
    {
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
            Debug.Log($"Found a room {id}");
            manager.ChangeSelectedRoomWithEvent(id);
        }
    }
}