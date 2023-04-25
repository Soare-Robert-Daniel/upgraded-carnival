﻿using UnityEngine;

namespace Towers
{
    [CreateAssetMenu(fileName = "Tower Resources", menuName = "Tower/ Create new resources", order = 0)]
    public class TowerResourcesScriptableObject : ScriptableObject
    {
        public string label;

        [Header("Sprites")]
        public Sprite mainSprite;
    }
}