﻿using System.Collections.Generic;
using Towers.Zones;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Config", menuName = "Utility/Global config file", order = 0)]
public class GlobalResources : ScriptableObject
{
    [Header("Zones")]
    public List<ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjects;

    [Header("Mobs")]
    [Header("Towers")]
    public string todo;


    private Dictionary<ZoneTokenType, ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjectsDictionary;

    public Dictionary<ZoneTokenType, ZoneTokenDataScriptableObject> GetZonesResources()
    {
        if (zoneTokenDataScriptableObjectsDictionary != null) return zoneTokenDataScriptableObjectsDictionary;

        zoneTokenDataScriptableObjectsDictionary = new Dictionary<ZoneTokenType, ZoneTokenDataScriptableObject>();
        foreach (var zoneTokenDataScriptableObject in zoneTokenDataScriptableObjects)
        {
            zoneTokenDataScriptableObjectsDictionary.Add(zoneTokenDataScriptableObject.zoneTokenType, zoneTokenDataScriptableObject);
        }

        return zoneTokenDataScriptableObjectsDictionary;
    }
}