using Economy;
using Map;
using UnityEngine;

namespace GameEntities
{
    public class Mob : MonoBehaviour
    {
        public int id;
        public Vector3 target;

        public Bounty bounty;
        public Vector3 movementDirection = Vector3.right;
        public float speed = 2f;

        [Header("Components")]
        [SerializeField] private MapManager manager;

        [SerializeField] private HealthBarController healthBarController;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;


        #region Room Movement

        public void StartMovingInRoom(RoomController room)
        {
            // Debug.Log($"StartMovingInRoom: {movementDirection} | {speed}");
            // transform.position = room.GetStartingPosition();
            // AdjustSpeed();
        }

        public void AdjustSpeed()
        {
            // rd2D.velocity = movementDirection * speed;
        }

        public void AdjustSpeed(float newSpeed)
        {
            // speed = newSpeed;
            // AdjustSpeed();
        }

        public void RelocateToPosition(Vector3 position)
        {
            // transform.position = position;
            // rd2D.velocity = Vector2.zero;
        }

        #endregion

        #region Room Effects

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("ExitRoom"))
            {
                manager.SetMobRoomStatus(id, EntityRoomStatus.Exiting);
            }
        }

        public void UpdateHealthBar(float healthPercentage)
        {
            healthBarController.SetPercentage(healthPercentage);
        }

        public void UpdateBodySprite(Sprite sprite)
        {
            bodySpriteRenderer.sprite = sprite;
        }


        public MapManager Manager
        {
            get => manager;
            set => manager = value;
        }

        #endregion

    }
}