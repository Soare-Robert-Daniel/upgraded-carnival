using System;
using System.Collections.Generic;
using System.Linq;
using GameEntities;
using Map.Room;
using UnityEngine;

namespace Runes
{
    [Serializable]
    public struct RuneValue : ICloneable
    {
        private float damage;
        private float slow;

        public object Clone()
        {
            return new RuneValue
            {
                damage = damage,
                slow = slow
            };
        }
    }

    [Serializable]
    public struct RuneToken : ICloneable
    {
        public int id;
        public int mobId;
        public int roomId;
        public float duration;
        public float timestamp;
        public RuneType runeType;
        public RuneValue runeValue;

        public object Clone()
        {
            return new RuneToken
            {
                id = id,
                mobId = mobId,
                roomId = roomId,
                duration = duration,
                timestamp = timestamp,
                runeType = runeType,
                runeValue = (RuneValue)runeValue.Clone()
            };
        }
    }

    [Serializable]
    public class RuneTokensBuilder
    {
        public static int idCounter = 0;
        private Dictionary<RuneType, RuneToken> runeTokensBlueprints;

        public RuneTokensBuilder()
        {
            runeTokensBlueprints = new Dictionary<RuneType, RuneToken>();
        }

        public RuneTokensBuilder(RoomModel roomModel) : this(roomModel.runesAction)
        {
        }

        public RuneTokensBuilder(IEnumerable<RoomsRuneWrapper> runesAction)
        {
            runeTokensBlueprints = new Dictionary<RuneType, RuneToken>();
            foreach (var runeAction in runesAction)
            {
                var runeType = runeAction.type;
                var runeToken = new RuneToken
                {
                    duration = runeAction.duration,
                    runeType = runeType,
                    runeValue = runeAction.value
                };
                runeTokensBlueprints.Add(runeType, runeToken);
            }
        }

        public void AddRuneToken(RuneToken runeToken)
        {
            runeTokensBlueprints.Add(runeToken.runeType, runeToken);
        }

        public RuneToken Build(RuneType runeType, int mobId, int roomId)
        {
            var runeToken = (RuneToken)runeTokensBlueprints[runeType].Clone();
            runeToken.id = GenerateId(runeToken);
            runeToken.mobId = mobId;
            runeToken.roomId = roomId;
            runeToken.timestamp = Time.time;
            return runeToken;
        }

        public List<RuneToken> BuildAll(int mobId, int roomId)
        {
            return runeTokensBlueprints.Values.Select(runeToken => Build(runeToken.runeType, mobId, roomId)).ToList();
        }

        public static int GenerateId(RuneToken runeToken)
        {
            runeToken.id = idCounter++;
            return runeToken.id;
        }
    }
}