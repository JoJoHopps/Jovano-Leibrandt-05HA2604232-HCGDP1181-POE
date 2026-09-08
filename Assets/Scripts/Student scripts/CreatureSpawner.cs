using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private float babyScaleMultiplier = 0.75f;

    // TODO: Full spawning implementation to be completed
    // in Summative Assessment
    public void SpawnBaby(BreedingData babyData, Transform roomTransform)
    {
        // ── Null Checks ───────────────────────────────────────
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

        // ── Log Intended Spawn Behaviour ──────────────────────
        // TODO: Replace these logs with actual Instantiate call
        Debug.Log($"[CreatureSpawner] Would spawn: {prefabToSpawn.name}");
        Debug.Log($"[CreatureSpawner] At position: {roomTransform.position}");
        Debug.Log($"[CreatureSpawner] With colour: {babyData.babyColour}");

        float finalScale = babyData.babyScale * babyScaleMultiplier;
        Debug.Log($"[CreatureSpawner] With scale: {finalScale}");

        // TODO: Instantiate prefab at roomTransform.position
        // TODO: Set baby as child of roomTransform
        // TODO: Apply babyData.babyColour to baby Renderer
        // TODO: Apply finalScale to baby Transform
        // TODO: Call ApplyStats to set inherited stat values
    }

    // ApplyStats written and ready for when
    // spawning is implemented
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
                Debug.LogWarning($"CreatureSpawner: Stat '{statName}' " +
                                 $"not found on baby.");
            }
        }
    }
}