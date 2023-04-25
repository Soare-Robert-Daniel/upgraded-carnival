using System;
using UnityEngine;

namespace Towers.Zones
{
    public enum ZoneTokenType
    {
        None,
        Damage,
        Slow,
        Burn,
        Freeze
    }

    public enum Trigger
    {
        TowerHit,
        TimerTick
    }

    [Serializable]
    public class ZoneToken : ICloneable
    {
        [Header("Identifiers")]
        public int id;

        public int zoneId;

        [Header("Data")]
        public ZoneTokenType zoneTokenType;

        public int rank;

        [Header("Duration")]
        public float remainingDuration;

        public object Clone()
        {
            return new ZoneToken
            {
                id = id,
                zoneId = zoneId,
                zoneTokenType = zoneTokenType,
                rank = rank,
                remainingDuration = remainingDuration
            };
        }

        public override string ToString()
        {
            return $"<ZoneToken: id={id}, zoneId={zoneId}, zoneTokenType={zoneTokenType}, rank={rank}, remainingDuration={remainingDuration}>";
        }
    }

    // Create a builder class for ZoneToken
    public class ZoneTokenBuilder
    {
        private ZoneToken zoneToken;

        public ZoneTokenBuilder()
        {
            zoneToken = new ZoneToken();
        }

        public ZoneTokenBuilder SetId(int id)
        {
            zoneToken.id = id;
            return this;
        }

        public ZoneTokenBuilder SetZoneId(int zoneId)
        {
            zoneToken.zoneId = zoneId;
            return this;
        }

        public ZoneTokenBuilder SetZoneTokenType(ZoneTokenType zoneTokenType)
        {
            zoneToken.zoneTokenType = zoneTokenType;
            return this;
        }

        public ZoneTokenBuilder SetRank(int rank)
        {
            zoneToken.rank = rank;
            return this;
        }

        public ZoneTokenBuilder SetRemainingDuration(float remainingDuration)
        {
            zoneToken.remainingDuration = remainingDuration;
            return this;
        }

        public ZoneToken Build()
        {
            return (ZoneToken)zoneToken.Clone();
        }
    }
}