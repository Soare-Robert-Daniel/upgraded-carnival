using System;
using UnityEngine;

namespace Map.Room
{
    public enum RoomType
    {
        Empty,
        SlowRune,
        AttackRune,
        ConstantAttackFire,
        BurstAttackFire
    }

    [Serializable]
    public class RoomState
    {
        [Header("Attributes")]
        [SerializeField] private RoomType roomType;

        [SerializeField] private float fireRate;

        [Header("Visuals")]
        [SerializeField] private SymbolStateV verticalSym;

        [SerializeField] private SymbolStateH horizontalSym;

        public RoomType RoomType
        {
            get => roomType;
            set => roomType = value;
        }

        public float FireRate
        {
            get => fireRate;
            set => fireRate = value;
        }

        public SymbolStateV VerticalSym
        {
            get => verticalSym;
            set => verticalSym = value;
        }

        public SymbolStateH HorizontalSym
        {
            get => horizontalSym;
            set => horizontalSym = value;
        }

        public void LoadFromModel(RoomModel roomModel)
        {
            if (roomModel == null)
            {
                return;
            }

            roomType = roomModel.roomType;
            fireRate = roomModel.fireRate;
        }
    }
}