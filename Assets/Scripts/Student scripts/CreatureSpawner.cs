using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private float babyScaleMultiplier = 0.75f;
    [SerializeField] private Transform babySpawnPoint;
    [SerializeField] private BreedingRoom breedingRoom;

    private void OnEnable()
    {
        if (breedingRoom != null)
        {
            breedingRoom.OnBabyReady += SpawnBaby;
        }
    }

    private void OnDisable()
    {
        if (breedingRoom != null)
        {
            breedingRoom.OnBabyReady -= SpawnBaby;
        }
    }

    public void SpawnBaby(BreedingData babyData, Transform roomTransform)
    {
        if (babyData == null)
        {
            Debug.LogError("CreatureSpawner: BreedingData is null!");
            return;
        }

        GameObject prefabToSpawn = babyData.inheritedPrefab != null
            ? babyData.inheritedPrefab
            : creaturePrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError("CreatureSpawner: No prefab available to spawn!");
            return;
        }

        Vector3 spawnPosition = babySpawnPoint != null
            ? babySpawnPoint.position
            : roomTransform.position;

        GameObject baby = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        if (babySpawnPoint != null)
        {
            baby.transform.SetParent(babySpawnPoint.parent);
        }

        Renderer babyRenderer = baby.GetComponentInChildren<Renderer>();
        if (babyRenderer != null)
        {
            babyRenderer.material.color = babyData.babyColour;
        }
        else
        {
            Debug.LogWarning("CreatureSpawner: No Renderer found on baby prefab.");
        }

        float finalScale = babyData.babyScale * babyScaleMultiplier;
        baby.transform.localScale = new Vector3(finalScale, finalScale, finalScale);

        Creature babyCreature = baby.GetComponent<Creature>();
        if (babyCreature != null)
        {
            ApplySharedStats(babyCreature, babyData.sharedStatValues);
            ApplyUniqueStats(babyCreature, babyData);
        }
        else
        {
            Debug.LogWarning("CreatureSpawner: No Creature component on baby prefab.");
        }

        Debug.Log($"Baby spawned! Colour: {babyData.babyColour}, " +
                  $"Scale: {finalScale}, " +
                  $"Mutation: {babyData.mutationOccurred}");
    }

    private void ApplySharedStats(Creature baby, Dictionary<string, float> sharedStatValues)
    {
        foreach (KeyValuePair<string, float> entry in sharedStatValues)
        {
            string statName = entry.Key;
            float statValue = entry.Value;

            Stat matchingStat = baby.GetStat(statName);

            if (matchingStat != null)
            {
                matchingStat.GetTrait().Value = statValue;
            }
            else
            {
                Debug.LogWarning($"CreatureSpawner: Stat '{statName}' not found on baby.");
            }
        }
    }

    private void ApplyUniqueStats(Creature baby, BreedingData babyData)
    {
        foreach (Stat sourceStat in babyData.uniqueStatsToAdd)
        {
            string statName = sourceStat.GetTrait().Name;

            if (baby.GetStat(statName) != null)
            {
                continue; // already present somehow, avoid creating a duplicate
            }

            System.Type statType = sourceStat.GetType();
            Component addedComponent = baby.gameObject.AddComponent(statType);
            Stat newStat = addedComponent as Stat;

            if (newStat != null)
            {
                newStat.CopyFrom(sourceStat);
                baby.RegisterStat(newStat);
                Debug.Log($"CreatureSpawner: Baby gained unique trait '{statName}'.");
            }
        }

        foreach (string statName in babyData.uniqueStatsToRemove)
        {
            baby.RemoveStatByName(statName);
            Debug.Log($"CreatureSpawner: Baby lost unique trait '{statName}'.");
        }
    }
}