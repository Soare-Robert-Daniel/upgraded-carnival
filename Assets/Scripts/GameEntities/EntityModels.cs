﻿using System.Collections.Generic;
using UnityEngine;

namespace GameEntities
{
    [CreateAssetMenu(fileName = "Mobs Model List", menuName = "Mob/Create Mob Model List", order = 0)]
    public class EntityModels : ScriptableObject
    {
        [SerializeField] protected List<EntityModel> list;

        public List<EntityModel> List => list;
    }
}