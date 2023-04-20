using System.Collections.Generic;

namespace Runes
{
    /**
     * This class is used to keep track of the duration of dynamic runes.
     * A dynamic rune is a rune that has an "onGoing" effect on the mob.
     * For example, a rune that slows the mob by 50% for 5 seconds. Or rune that deals 10 damage per second for 5 seconds.
     *
     * No dynamic rune will be implemented in the first version of the game. But the code is already here for reference.
     */
    public class RuneDuration
    {
        public const float ExpirationThreshold = 0.02f;
        public int capacity;
        public float[] durations;
        public SortedSet<(int, int)> expiredIds;
        public Stack<int> freeSlots;

        public (int, int)[] ids;

        public RuneDuration(int capacity)
        {
            this.capacity = capacity;
            durations = new float[capacity];
            ids = new (int, int)[capacity];
            expiredIds = new SortedSet<(int, int)>();
            freeSlots = new Stack<int>();

            for (var i = 0; i < capacity; i++)
            {
                freeSlots.Push(i);
            }
        }

        public void Add(int tokenId, int ownerId, float duration)
        {
            if (freeSlots.Count == 0)
            {
                if (!TryFindAnAlmostExpiredRune())
                {
                    return;
                }
            }
            ;
            var slot = freeSlots.Pop();
            durations[slot] = duration;
            ids[slot] = (tokenId, ownerId);
        }

        public void Update(float deltaTime)
        {
            for (var i = 0; i < capacity; i++)
            {
                durations[i] -= deltaTime;
                if (!(durations[i] <= 0)) continue;
                expiredIds.Add(ids[i]);
                freeSlots.Push(i);
            }
        }

        public bool TryFindAnAlmostExpiredRune()
        {
            for (var i = 0; i < capacity; i++)
            {
                if (!(durations[i] <= ExpirationThreshold)) continue;
                expiredIds.Add(ids[i]);
                freeSlots.Push(i);
            }

            return freeSlots.Count > 0;
        }

        public void Reset()
        {
            freeSlots.Clear();
            for (var i = 0; i < capacity; i++)
            {
                freeSlots.Push(i);
            }
        }
    }
}