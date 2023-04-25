using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Towers.Zones
{
    public class TokenZoneSystem
    {
        private Dictionary<int, List<ZoneToken>> mobsTokens;
        private int nextTokenId;
        private Dictionary<int, ZoneTokenType> zones;
        private Dictionary<ZoneTokenType, TokenData> zoneTokenData;

        public TokenZoneSystem()
        {
            zoneTokenData = new Dictionary<ZoneTokenType, TokenData>();
            mobsTokens = new Dictionary<int, List<ZoneToken>>();
            zones = new Dictionary<int, ZoneTokenType>();
            nextTokenId = 0;
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

        public Dictionary<int, List<ZoneToken>> MobsTokens => mobsTokens;

        public void AddOrUpdateTokens(int mobId, int zoneId)
        {
            if (!mobsTokens.ContainsKey(mobId))
            {
                mobsTokens.Add(mobId, new List<ZoneToken>());
            }

            if (!zones.ContainsKey(zoneId))
            {
                return;
            }

            var mobTokens = mobsTokens[mobId];
            var zoneType = zones[zoneId];

            // TODO: Might be better to have a default token type
            // if (zoneType == ZoneTokenType.None)
            //     return;

            var tokensToAddBuilders = zoneTokenData[zoneType].Transform(mobTokens);

            foreach (var updatedToken in tokensToAddBuilders
                         .Select(tokenBuilder =>
                             tokenBuilder
                                 .SetId(GenerateTokenId())
                                 .SetZoneId(zoneId)
                                 .Build()))
            {
                AddOrUpdateToken(mobId, updatedToken);
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

            if (existingToken == null)
            {
                mobTokens.Add(token);
                Debug.Log($"Adding token <{token.id}> to mob {mobId}. Rank: {token.rank}, duration: {token.remainingDuration}");
            }
            else
            {
                existingToken.rank += token.rank;
                existingToken.remainingDuration = token.remainingDuration;

                Debug.Log(
                    $"Updating token <{token.id}> to mob {mobId}. New rank: {existingToken.rank}, new duration: {existingToken.remainingDuration} for id <{existingToken.id}>");
            }
        }

        public void AddOrUpdateZone(int zoneId, ZoneTokenType zoneType)
        {
            Debug.Log($"Adding zone [{zoneId}] with type {zoneType}");
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
            if (!mobsTokens.ContainsKey(mobId))
            {
                return;
            }

            Debug.Log(
                $"Removing tokens for mob {mobId}. Tokens count: {mobsTokens[mobId].Count}. Printing tokens: {string.Join(", ", mobsTokens[mobId])}");
            mobsTokens.Remove(mobId);
        }

        public int GenerateTokenId()
        {
            nextTokenId = (nextTokenId + 1) % int.MaxValue;
            return nextTokenId;
        }

        public bool TryGetMobTokens(int mobId, out List<ZoneToken> tokens)
        {
            return mobsTokens.TryGetValue(mobId, out tokens);
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