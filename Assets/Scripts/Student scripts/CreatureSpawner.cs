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

        // FA2 BUG FIX: In FA1 the baby was parented directly to
        // the BreedingRoom, which caused the room to immediately
        // detect 3 creatures (2 parents + 1 baby) and incorrectly
        // trigger the "too many creatures" warning, permanently
        // blocking further breeding. Spawning the baby at a
        // separate point outside the room resolves this.
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
            ApplyStats(babyCreature, babyData.statValues);
        }
        else
        {
            Debug.LogWarning("CreatureSpawner: No Creature component on baby prefab.");
        }

        Debug.Log($"Baby spawned! Colour: {babyData.babyColour}, " +
                  $"Scale: {finalScale}, " +
                  $"Mutation: {babyData.mutationOccurred}");
    }

    private void ApplyStats(Creature baby, Dictionary<string, float> statValues)
    {
        foreach (KeyValuePair<string, float> entry in statValues)
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
}