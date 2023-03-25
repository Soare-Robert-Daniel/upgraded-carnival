using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace GameEntities
{
    /**
     * This class is responsible for controlling all mobs controllers in the game.
     * Animation, visual, and audio are handled here. Check MobsSystem for logic.
     */
    [Serializable]
    public class MobsControllerSystem
    {
        [SerializeField] private int currentCapacity;
        [SerializeField] private Mob[] mobsController;
        [SerializeField] private Vector3[] mobsCurrentPositions;

        private NativeArray<Vector3> mobsCurrentPositionsNativeArray;

        // Job System
        private NativeArray<Vector3> mobsNewPositions;

        public MobsControllerSystem(int initialCapacity = 0)
        {
            mobsController = new Mob[initialCapacity];
            mobsCurrentPositions = new Vector3[initialCapacity];
            currentCapacity = 0;
        }

        public void AddMobController(Mob mobController, Vector3 startingPosition)
        {
            var newSlot = currentCapacity;
            if (currentCapacity >= mobsController.Length)
            {
                ResizeStorage(newSlot + 1);
            }
            else
            {
                currentCapacity++;
            }

            mobsController[newSlot] = mobController;
            mobsCurrentPositions[newSlot] = startingPosition;
        }

        public void UpdateMobsNextPosition(float[] speed, float deltaTime)
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                mobsCurrentPositions[i] += speed[i] * deltaTime * Vector3.right;
            }
        }

        public void UpdateMobControllersHealth(float[] health)
        {
            if (currentCapacity != health.Length)
                throw new Exception($"Mob indexes array length does not match health array length: {currentCapacity} != {health.Length}");

            for (var i = 0; i < currentCapacity; i++)
            {
                mobsController[i].UpdateHealthBar(health[i] / 100f); // TODO: Refactor magic number
            }
        }

        public void UpdateMobControllerHealth(int mobIndex, float health)
        {
            if (mobIndex >= currentCapacity)
                throw new Exception($"Mob index is out of bounds: {mobIndex} >= {currentCapacity}");

            mobsController[mobIndex].UpdateHealthBar(health / 100f); // TODO: Refactor magic number
        }

        public void SelfUpdatePositionsFromJobResult()
        {
            for (var i = 0; i < currentCapacity; i++)
            {
                mobsController[i].transform.position = mobsNewPositions[i];
            }
        }

        public void UpdateMobControllersPositions(Vector3[] positions)
        {
            if (positions.Length != currentCapacity)
                throw new Exception($"Positions array length does not match current capacity: {positions.Length} != {currentCapacity}");

            for (var i = 0; i < currentCapacity; i++)
            {
                mobsController[i].transform.position = positions[i];
            }
        }

        public void UpdateMobControllersPositions()
        {
            if (mobsCurrentPositions.Length < currentCapacity)
                throw new Exception(
                    $"Positions array length is lower than current mobs capacity: {mobsCurrentPositions.Length} < {currentCapacity}");

            for (var i = 0; i < currentCapacity; i++)
            {
                mobsController[i].transform.position = mobsCurrentPositions[i];
            }
        }

        public Vector3[] GetMobControllersPositions()
        {
            var positions = new Vector3[currentCapacity];
            for (var i = 0; i < currentCapacity; i++)
            {
                positions[i] = mobsController[i].transform.position;
            }

            return positions;
        }

        public void SetMobPosition(int mobIndex, Vector3 position)
        {
            if (mobIndex >= currentCapacity)
                throw new Exception($"Mob index is out of bounds: {mobIndex} >= {currentCapacity}");

            mobsCurrentPositions[mobIndex] = position;
        }

        public void UpdateMobPositionController(int mobIndex, Vector3 position)
        {
            if (mobIndex >= currentCapacity)
                throw new Exception($"Mob index is out of bounds: {mobIndex} >= {currentCapacity}");

            mobsController[mobIndex].transform.position = position;
        }

        public void ResizeStorage(int newCapacity)
        {
            Array.Resize(ref mobsController, newCapacity);
            Array.Resize(ref mobsCurrentPositions, newCapacity);
        }

        public int GetMobControllersCount()
        {
            return currentCapacity;
        }

        public Vector3[] GetMobControllersPositionsArray()
        {
            return mobsCurrentPositions;
        }

        #region Job System - Test

        public struct MovementJob : IJob
        {
            [ReadOnly] public NativeArray<Vector3> positions;
            [ReadOnly] public NativeArray<float> speeds;
            public NativeArray<Vector3> newPositions;
            public float deltaTime;

            public void Execute()
            {
                for (var index = 0; index < positions.Length; index++)
                {
                    newPositions[index] = positions[index] + Vector3.right * (deltaTime * speeds[index]);
                }
            }
        }

        public NativeArray<Vector3> GetMobControllersPositionsNativeArray(Allocator allocator)
        {
            return new NativeArray<Vector3>(mobsCurrentPositions, allocator);
        }

        public void Dispose()
        {
            if (mobsNewPositions.IsCreated)
            {
                mobsNewPositions.Dispose();
            }

            if (mobsCurrentPositionsNativeArray.IsCreated)
            {
                mobsCurrentPositionsNativeArray.Dispose();
            }
        }

        public MovementJob CreateMovementJob(float[] speeds, float deltaTime)
        {
            mobsNewPositions = new NativeArray<Vector3>(currentCapacity, Allocator.Persistent);
            mobsCurrentPositionsNativeArray = GetMobControllersPositionsNativeArray(Allocator.Persistent);

            var job = new MovementJob
            {
                positions = mobsCurrentPositionsNativeArray,
                speeds = new NativeArray<float>(speeds, Allocator.Persistent),
                newPositions = mobsNewPositions,
                deltaTime = deltaTime
            };

            return job;
        }

        #endregion

    }
}