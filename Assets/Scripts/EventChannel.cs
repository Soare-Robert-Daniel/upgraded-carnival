using System;
using Map;
using Towers.Zones;
using UnityEngine;

[CreateAssetMenu(fileName = "Event Channel", menuName = "Utility/Event Channel", order = 0)]
public class EventChannel : ScriptableObject
{
    public Action<int, RoomController> OnZoneControllerAdded;
    public Action<int, ZoneTokenType> OnZoneTypeChanged;

    public void AddZoneController(int zoneId, RoomController roomController)
    {
        OnZoneControllerAdded?.Invoke(zoneId, roomController);
    }

    public void ChangeZoneType(int zoneId, ZoneTokenType zoneType)
    {
        OnZoneTypeChanged?.Invoke(zoneId, zoneType);
    }
}