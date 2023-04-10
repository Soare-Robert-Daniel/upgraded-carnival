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
            var runeTokens = runeDatabase.data.Where(runeToken => runeToken.mobId == mobId).ToList();
            Debug.Log("Rune tokens count: " + runeTokens.Count);
            foreach (var runeToken in runeTokens.Where(runeToken => runeToken.timestamp + runeToken.duration < Time.time))
            {
                Debug.Log("Rune expired: " + runeToken.runeType + " " + runeToken.mobId + " " + runeToken.roomId);
                runeDatabase.RemoveRuneToken(runeToken);
            }

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
                        runeTokens.Where(runeToken => runeToken.runeType == runeAction.type).ToList().ForEach(runeDatabase.RemoveRuneToken);
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
                                .ForEach(runeDatabase.RemoveRuneToken);
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
    }
}