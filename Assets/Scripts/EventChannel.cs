using System;
using Map;
using Towers.Zones;
using UnityEngine;

[CreateAssetMenu(fileName = "Event Channel", menuName = "Utility/Event Channel", order = 0)]
public class EventChannel : ScriptableObject
{
    public Action<int> OnMobEliminated;
    public Action<int> OnMobReachedEnd;
    public Action<int> OnSelectedMobChanged;
    public Action<int> OnSelectedZoneChanged;
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

    public void EliminateMob(int mobId)
    {
        OnMobEliminated?.Invoke(mobId);
    }

    public void MobReachedEnd(int mobId)
    {
        OnMobReachedEnd?.Invoke(mobId);
    }
}