using UnityEngine;

namespace Mobs
{
    public class MobSimpleController : MonoBehaviour
    {
        [SerializeField] private int mobId;

        public int MobId => mobId;

        public void SetMobId(int id)
        {
            mobId = id;
        }
    }
}