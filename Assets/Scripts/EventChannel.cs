using System;
using Map;
using Towers.Zones;
using UnityEngine;

[CreateAssetMenu(fileName = "Event Channel", menuName = "Utility/Event Channel", order = 0)]
public class EventChannel : ScriptableObject
{
    public Action<ZoneController> OnZoneControllerAdded;
    public Action<int, ZoneTokenType> OnZoneTypeChanged;

    public void AddZoneController(ZoneController zone)
    {
        OnZoneControllerAdded?.Invoke(zone);
    }

    public void ChangeZoneType(int zoneId, ZoneTokenType zoneType)
    {
        OnZoneTypeChanged?.Invoke(zoneId, zoneType);
    }
}