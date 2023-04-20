using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;
using Map.Room;
using UnityEngine;

namespace Runes
{
    [Serializable]
    public class RunesHandlerForRoom
    {

        [SerializeField] private RuneTokensBuilder runeTokensBuilder;
        public List<RoomsRuneWrapper> runesActions;
        private Dictionary<RuneType, Dictionary<RuneType, int>> runesConditions;

        public RunesHandlerForRoom()
        {
            runeTokensBuilder = new RuneTokensBuilder();
            runesConditions = new Dictionary<RuneType, Dictionary<RuneType, int>>();
            runesActions = new List<RoomsRuneWrapper>();
        }

        public RunesHandlerForRoom(RoomModel roomModel)
        {
            runeTokensBuilder = new RuneTokensBuilder(roomModel);

            runesConditions = new Dictionary<RuneType, Dictionary<RuneType, int>>();

            // Extract runes conditions from room model.
            foreach (var runeAction in roomModel.runesAction)
            {
                var runeType = runeAction.type;
                var runeConditions =
                    runeAction.runesConditions.ToDictionary(runeCondition => runeCondition.runeType, runeCondition => runeCondition.count);
                runesConditions.Add(runeType, runeConditions);
            }

            runesActions = roomModel.runesAction;
        }

        public void UpdateDatabase(RuneDatabase runeDatabase, int roomId, int mobId)
        {
            // Eliminate expired tokens.
            Debug.Log("Current time: " + Time.time + "s");
            Debug.Log("Room id: " + roomId + " Mob id: " + mobId);
            var hasTokens = runeDatabase.mobsRunesCollection.TryGetValue(mobId, out var runeTokens);
            if (!hasTokens) return;

            Debug.Log("Rune tokens count: " + runeTokens.Count);

            // Remove expired tokens.
            runeTokens
                .Where(runeToken => runeToken.timestamp + runeToken.duration < Time.time).ToList()
                .ForEach(runeDatabase.RemoveTokenFrom);

            // Add new tokens.
            runesActions.ForEach(runeAction =>
            {
                if (!runeAction.runesConditions.All(runeCondition =>
                        runeDatabase.mobsRunesCount[mobId][runeCondition.runeType] >= runeCondition.count)) return;
                switch (runeAction.actionType)
                {
                    case RoomRuneHandlingType.Add:
                    {
                        var runeToken = runeTokensBuilder.Build(runeAction.type, mobId, roomId);
                        runeDatabase.AddRuneToken(runeToken);
                        break;
                    }
                    case RoomRuneHandlingType.Remove:
                        // Remove all tokens of this type.
                        runeTokens
                            .Where(runeToken => runeToken.runeType == runeAction.type).ToList()
                            .ForEach(runeDatabase.RemoveTokenFrom);
                        break;
                    case RoomRuneHandlingType.Consume:
                    {
                        // Calculate how many tokens to remove.
                        var tokensToRemove = runeAction.runesConditions.Min(runeCondition =>
                            runeDatabase.mobsRunesCount[mobId][runeCondition.runeType] / runeCondition.count);
                        // Remove tokens from database of each type in runeAction.runesConditions.
                        runeAction.runesConditions.ForEach(runeCondition =>
                        {
                            var runeType = runeCondition.runeType;
                            runeTokens
                                .Where(runeToken => runeToken.runeType == runeType)
                                .Take(tokensToRemove)
                                .ToList()
                                .ForEach(runeDatabase.RemoveTokenFrom);
                        });
                        break;
                    }
                    case RoomRuneHandlingType.Keep:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            });
        }

        public QueryResultRunProcessing QueryRunProcessing(RuneDatabase runeDatabase, int mobId, RunesProcessors runesProcessors)
        {
            var hasTokens = runeDatabase.mobsRunesCollection.TryGetValue(mobId, out var runeTokens);
            return !hasTokens ? new QueryResultRunProcessing() : runesProcessors.BuildQuery(runeTokens);
        }

        public struct QueryResultRunProcessing
        {
            public float damage;
            public float slow;

            public QueryResultRunProcessing AddDamage(float damage)
            {
                this.damage += damage;
                return this;
            }

            public QueryResultRunProcessing AddSlow(float slow)
            {
                this.slow += slow;
                return this;
            }

            public QueryResultRunProcessing Add(QueryResultRunProcessing other)
            {
                damage += other.damage;
                slow += other.slow;
                return this;
            }

            public QueryResultRunProcessing Multiply(float multiplier)
            {
                damage *= multiplier;
                slow *= multiplier;
                return this;
            }

            public QueryResultRunProcessing MultiplyDamage(float multiplier)
            {
                damage *= multiplier;
                return this;
            }

            public QueryResultRunProcessing MultiplySlow(float multiplier)
            {
                slow *= multiplier;
                return this;
            }

            public QueryResultRunProcessing Clone()
            {
                return new QueryResultRunProcessing
                {
                    damage = damage,
                    slow = slow
                };
            }
        }
    }
}