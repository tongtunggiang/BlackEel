﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

namespace BE.ECS
{
    public class SpawnAllySystem : ComponentSystem
    {
        private float m_LastSpawn;
        private float m_SpawnRate;
        private Unity.Mathematics.Random m_Random;

        protected override void OnCreate()
        {
            m_SpawnRate = GameData.Instance.spawnRate;

            m_LastSpawn = Time.time - m_SpawnRate * 2;

            m_Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 1000));
        }

        protected override void OnUpdate()
        {
            if (Time.time - m_LastSpawn < m_SpawnRate)
            {
                return;
            }

            m_LastSpawn = Time.time - m_Random.NextFloat(-GameData.Instance.spawnRateNoise, GameData.Instance.spawnRateNoise);
            World.GetOrCreateSystem<SpawnPointManagementSystem>().GetRandomSpawnPointPosition(out float3 spawnPos);

            int batchCount = m_Random.NextInt(GameData.Instance.allySpawnBatchCountMin, GameData.Instance.allySpawnBatchCountMax);
            for (int i = 0; i < batchCount; i++)
            {
                float3 instanceSpawnPos = spawnPos + 
                    new float3(m_Random.NextFloat(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise), 
                    0, m_Random.NextFloat(-GameData.Instance.spawnPositionNoise, GameData.Instance.spawnPositionNoise));

                Entity prefab = GameData.Instance.AllyEntityPrefab;
                Entity instance = EntityManager.Instantiate(prefab);
                EntityManager.SetName(instance, "Ally");
                EntityManager.AddComponentData(instance, new AgentTag { });

                EntityManager.SetComponentData(instance, new Translation { Value = instanceSpawnPos });
                EntityManager.AddComponentData(instance, new MoveSpeedComponent { Value = GameData.Instance.agentMoveSpeed });

                EntityManager.AddComponentData(instance, new AttackRadiusComponent { Value = GameData.Instance.agentScanRadius });

                EntityManager.AddSharedComponentData(instance, new AllyTeamComponent());

                EntityManager.AddComponentData(instance, new HealthComponent { Value = GameData.Instance.agentInitialHealth });
                EntityManager.AddSharedComponentData(instance, new MaxHealthComponent { Value = GameData.Instance.agentInitialHealth });
            }
        }
    } }