using System.Linq;
using Economy;
using Map;
using UnityEngine;

namespace GameEntities
{
    public class Mob : MonoBehaviour
    {
        public int id;
        public Vector3 target;
        public EntityStats stats;
        public Bounty bounty;
        public Vector3 movementDirection = Vector3.right;
        public float speed = 2f;

        [Header("Components")]
        [SerializeField] private MapManager manager;

        [SerializeField] private Rigidbody2D rd2D;
        [SerializeField] private HealthBarController healthBarController;

        private void Awake()
        {

        }

        private void Start()
        {
            manager.RegisterMob(this);
        }

        private void Update()
        {
            UpdateHealthBar();
        }

        #region Room Movement

        public void StartMovingInRoom(RoomController room)
        {
            transform.position = room.GetStartingPosition();
            AdjustSpeed();
        }

        public void AdjustSpeed()
        {
            var slow = Mathf.Clamp(stats.Runes.Count(x => x.Type == RuneType.Slow) * 0.1f, 0f, 0.8f);
            rd2D.velocity = movementDirection * (speed * (1f - slow));
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
            stats.Health = Mathf.Max(0, stats.Health - damage);
            UpdateHealthBar();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("ExitRoom"))
            {
                manager.SetMobRoomStatus(id, EntityRoomStatus.Exit);
            }
        }

        private void UpdateHealthBar()
        {
            healthBarController.SetPercentage(stats.RemainingHealthPercentage());
        }

        public MapManager Manager
        {
            get => manager;
            set => manager = value;
        }

        #endregion

    }
}