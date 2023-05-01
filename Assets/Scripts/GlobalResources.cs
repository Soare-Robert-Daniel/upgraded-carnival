using System.Collections.Generic;
using Mobs;
using Towers;
using Towers.Zones;
using UnityEngine;

[CreateAssetMenu(fileName = "Global Config", menuName = "Utility/Global config file", order = 0)]
public class GlobalResources : ScriptableObject
{
    [Header("Zones")]
    public List<ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjects;

    [Header("Mobs")]
    public List<MobDataScriptableObject> mobDataScriptableObjects;

    [Header("Towers")]
    public List<TowerDataScriptableObject> towerDataScriptableObjects;

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