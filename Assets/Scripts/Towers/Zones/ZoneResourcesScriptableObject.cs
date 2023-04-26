using UnityEngine;

namespace Towers.Zones
{
    [CreateAssetMenu(fileName = "Zone Resources", menuName = "Zone/Create new resources", order = 0)]
    public class ZoneResourcesScriptableObject : ScriptableObject
    {
        public Texture2D mainTexture;
        public Color mainColor;
    }
}