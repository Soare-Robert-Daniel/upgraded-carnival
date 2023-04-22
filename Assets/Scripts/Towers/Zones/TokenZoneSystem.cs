﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Towers.Zones
{
    public class TokenZoneSystem
    {
        private Dictionary<int, List<ZoneToken>> mobsTokens;
        private Dictionary<int, ZoneTokenType> zones;
        private Dictionary<ZoneTokenType, TokenData> zoneTokenData;

        public TokenZoneSystem()
        {
            zoneTokenData = new Dictionary<ZoneTokenType, TokenData>();
            mobsTokens = new Dictionary<int, List<ZoneToken>>();
            zones = new Dictionary<int, ZoneTokenType>();
        }

        public TokenZoneSystem(List<ZoneTokenDataScriptableObject> zoneTokenDataScriptableObjects)
        {
            mobsTokens = new Dictionary<int, List<ZoneToken>>();
            zoneTokenData = new Dictionary<ZoneTokenType, TokenData>();
            zones = new Dictionary<int, ZoneTokenType>();
            foreach (var zoneTokenDataScriptableObject in zoneTokenDataScriptableObjects)
            {
                zoneTokenData.Add(zoneTokenDataScriptableObject.zoneTokenType, new TokenData(zoneTokenDataScriptableObject));
            }
        }

        public void AddOrUpdateToken(int mobId, ZoneToken token)
        {
            if (!mobsTokens.ContainsKey(mobId))
            {
                mobsTokens.Add(mobId, new List<ZoneToken>());
            }

            var mobTokens = mobsTokens[mobId];

            var existingToken = mobTokens.FirstOrDefault(t => t.zoneTokenType == token.zoneTokenType);
            if (existingToken.id == 0)
            {
                mobTokens.Add(token);
            }
            else
            {
                existingToken.rank += token.rank;
                existingToken.remainingDuration = token.remainingDuration;
            }
        }

        public void AddOrUpdateZone(int zoneId, ZoneTokenType zoneType)
        {
            if (!zones.ContainsKey(zoneId))
            {
                zones.Add(zoneId, zoneType);
            }
            else
            {
                zones[zoneId] = zoneType;
            }
        }

        public void CleanUpTokens()
        {
            foreach (var (mobId, tokens) in mobsTokens)
            {
                var tokensToRemove = tokens.Where(t => t.remainingDuration <= 0).ToList();
                foreach (var tokenToRemove in tokensToRemove)
                {
                    tokens.Remove(tokenToRemove);
                }

                if (tokens.Count == 0)
                {
                    RemoveMobTokens(mobId);
                }
            }
        }

        public void ChangeZoneType(int zoneId, ZoneTokenType zoneType)
        {
            Debug.Log($"Changing zone type to {zoneType} for zone {zoneId}");
            if (zones.ContainsKey(zoneId))
            {
                Debug.Log($"Zone {zoneId} found");
                zones[zoneId] = zoneType;
            }
        }

        public void RemoveMobTokens(int mobId)
        {
            mobsTokens.Remove(mobId);
        }

        public class TokenData
        {
            public ZoneTokenDataScriptableObject data;

            public int maxRank;
            public Dictionary<(ZoneTokenType, int), TokenRankData> ranksData;

            public TokenData(ZoneTokenDataScriptableObject data)
            {
                this.data = data;
                maxRank = data.ranks.Select(tokenRankData => tokenRankData.rank).Prepend(0).Max();
                ranksData = data.ranks.ToDictionary(tokenRankData => (data.zoneTokenType, tokenRankData.rank));
            }

            public List<ZoneTokenBuilder> Transform(List<ZoneToken> currentRanks)
            {
                var tokenBuilders = new List<ZoneTokenBuilder>();

                foreach (var transformation in data.transformations)
                {
                    // Check if data from currentRank fully matches data from transformation.from

                    var from = transformation.from;
                    var to = transformation.to;

                    var fromMatches = !(from rankTypePair in @from
                        let rank = rankTypePair.rank
                        let type = rankTypePair.tokenType
                        let currentRank = currentRanks.FirstOrDefault(t => t.zoneTokenType == type)
                        where currentRank.rank != rank
                        select rank).Any();

                    if (!fromMatches) continue;

                    // If it matches, add new tokens to list
                    tokenBuilders.AddRange(from rankTypePair in to
                        let rank = rankTypePair.rank
                        let type = rankTypePair.tokenType
                        select new ZoneTokenBuilder().SetRank(rank)
                            .SetZoneTokenType(type)
                            .SetRemainingDuration(ranksData[(type, rank)].totalDuration));

                    // Subtract the ranks from currentRanks
                    foreach (var rankTypePair in from)
                    {
                        var rank = rankTypePair.rank;
                        var type = rankTypePair.tokenType;

                        var currentRank = currentRanks.FirstOrDefault(t => t.zoneTokenType == type);
                        currentRank.rank -= rank;
                    }
                }

                return tokenBuilders;
            }
        }
    }
}