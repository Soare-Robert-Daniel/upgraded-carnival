using Map;
using UnityEngine;

namespace GameEntities
{
    public class Mob : MonoBehaviour
    {
        public int id;
        public Vector3 target;
        public EntityStats stats;
        public Vector3 movementDirection = Vector3.right;
        public float speed = 2f;

        [Header("Components")]
        [SerializeField] private MapManager manager;

        [SerializeField] private Rigidbody2D rd2D;

        private void Start()
        {
            manager.RegisterMob(this);
        }

        #region Room Movement

        public void StartMovingInRoom(RoomController room)
        {
            transform.position = room.GetStartingPosition();
            rd2D.velocity = movementDirection * speed;
        }

        public void RelocateToPosition(Vector3 position)
        {
            transform.position = position;
            rd2D.velocity = Vector2.zero;
        }

        #endregion

        #region Room Effects

        public void ApplyDamage(float damage)
        {
            stats.Health -= damage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("ExitRoom"))
            {
                manager.SetMobRoomStatus(id, EntityRoomStatus.Exit);
            }
        }

        #endregion

    }
}