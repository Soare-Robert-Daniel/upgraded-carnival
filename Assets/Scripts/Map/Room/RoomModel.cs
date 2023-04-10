using System;
using System.Collections.Generic;
using GameEntities;
using Runes;
using UnityEngine;

namespace Map.Room
{
    public enum RoomRuneHandlingType
    {
        Keep,
        Add,
        Consume,
        Remove
    }


    [Serializable]
    public class RoomsRuneWrapper
    {

        public RuneType type;
        public RuneValue value;
        public float damage;
        public float slow;
        public float duration;
        public RoomRuneHandlingType actionType;
        public List<RuneCondition> runesConditions;

        [Serializable]
        public struct RuneCondition
        {
            public RuneType runeType;
            public int count;
        }
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

        public Texture2D mainTexture;
        public Texture2D mainTextureMask;
        public Texture2D secondaryTexture;
    }
}