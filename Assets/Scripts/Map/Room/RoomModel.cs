using System;
using System.Collections.Generic;
using GameEntities;
using UnityEngine;

namespace Map.Room
{
    public enum RoomRuneHandlingType
    {
        Keep,
        Add,
        Remove
    }

    [Serializable]
    public class RoomsRuneWrapper
    {
        public RuneType type;
        public float damage;
        public float duration;
        public RoomRuneHandlingType actionType;
    }

    [CreateAssetMenu(fileName = "Room Model", menuName = "Rooms/Create Room Model", order = 0)]
    public class RoomModel : ScriptableObject
    {
        [Header("Selectors")]
        public RoomType roomType;

        [Header("Room Entity")]
        public string roomName;

        public float fireRate;
        public float fireDuration;

        [Header("Economy")]
        public float price;

        [Header("Damage")]
        public float baseDamage;

        public List<RoomsRuneWrapper> runesAction;

        [Header("Assets")]
        public Sprite sprite;
    }
}