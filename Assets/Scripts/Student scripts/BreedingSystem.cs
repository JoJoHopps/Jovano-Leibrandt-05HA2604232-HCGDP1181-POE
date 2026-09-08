using System.Collections.Generic;
using UnityEngine;

public class BreedingSystem : MonoBehaviour
{
    [SerializeField] private float mutationChance = 0.25f;
    [SerializeField] private float mutationMinValue = 0f;
    [SerializeField] private float mutationMaxValue = 100f;

    public BreedingData Breed(Creature parentA, Creature parentB)
    {
        BreedingData result = new BreedingData();

        // ── Stat Inheritance ──────────────────────────────────
        List<Stat> parentAStats = parentA.GetStats();
        List<Stat> parentBStats = parentB.GetStats();

        for (int i = 0; i < parentAStats.Count; i++)
        {
            Stat inheritedStat = (Random.value < 0.5f)
                ? parentAStats[i]
                : parentBStats[i];

            string statName = inheritedStat.GetTrait().Name;
            float statValue = inheritedStat.GetTrait().Value;

            result.statValues[statName] = statValue;
        }

        // ── Mutation ──────────────────────────────────────────
        if (Random.value < mutationChance)
        {
            result.mutationOccurred = true;

            List<string> statNames = new List<string>(result.statValues.Keys);
            int randomIndex = Random.Range(0, statNames.Count);
            string mutatedStatName = statNames[randomIndex];

            float mutatedValue = Random.Range(mutationMinValue, mutationMaxValue);
            result.statValues[mutatedStatName] = mutatedValue;

            Debug.Log($"Mutation occurred! {mutatedStatName} mutated to {mutatedValue}");
        }

        // ── Colour Inheritance ────────────────────────────────
        Renderer rendererA = parentA.GetComponentInChildren<Renderer>();
        Renderer rendererB = parentB.GetComponentInChildren<Renderer>();

        if (rendererA != null && rendererB != null)
        {
            Color colourA = rendererA.material.color;
            Color colourB = rendererB.material.color;

            result.babyColour = Color.Lerp(colourA, colourB, 0.5f);

            if (result.mutationOccurred)
            {
                result.babyColour = new Color(
                    Random.value,
                    Random.value,
                    Random.value,
                    1f
                );
            }
        }

        // ── Scale Inheritance ─────────────────────────────────
        float scaleA = parentA.transform.localScale.x;
        float scaleB = parentB.transform.localScale.x;

        result.babyScale = (Random.value < 0.5f) ? scaleA : scaleB;
        result.babyScale += Random.Range(-0.1f, 0.1f);

        // ── Prefab Inheritance ────────────────────────────────
        GameObject inheritedPrefab = (Random.value < 0.5f)
            ? parentA.GetCreaturePrefab()
            : parentB.GetCreaturePrefab();

        if (inheritedPrefab != null)
        {
            result.inheritedPrefab = inheritedPrefab;
        }
        else
        {
            Debug.LogWarning("BreedingSystem: One or both parents have no " +
                             "prefab assigned. Check the Creature Inspector fields.");
        }

        return result;
    }
}