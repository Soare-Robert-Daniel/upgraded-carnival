using System;
using UnityEngine;

namespace Map.Room
{
    [Serializable]
    public struct RoomState
    {
        [Header("Attributes")]
        public RoomType roomType;

        public float fireRate;

        [Header("Visuals")]
        public SymbolStateV verticalSym;

        public SymbolStateH horizontalSym;

        public void LoadFromModel(RoomModel roomModel)
        {
            if (roomModel == null)
            {
                return;
            }

            roomType = roomModel.roomType;
            fireRate = roomModel.fireRate;
            verticalSym = roomModel.verticalSym;
            horizontalSym = roomModel.horizontalSym;
        }
    }
}