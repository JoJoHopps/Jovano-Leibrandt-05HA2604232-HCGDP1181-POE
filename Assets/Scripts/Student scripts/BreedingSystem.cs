using System.Collections.Generic;
using UnityEngine;

public class BreedingSystem : MonoBehaviour
{
    [SerializeField] private float uniqueTraitChance = 0.5f;
    [SerializeField] private float mutationChance = 0.25f;

    // Mutations are small nudges, not replacements. 0.15 = up to +/-15% of the inherited value.
    [SerializeField] private float mutationStatStrength = 0.15f;

    // Maximum amount each colour channel (0-1) can drift when a mutation occurs.
    [SerializeField] private float mutationColourShift = 0.1f;

    public BreedingData Breed(Creature parentA, Creature parentB)
    {
        BreedingData result = new BreedingData();

        List<Stat> statsA = parentA.GetStats();
        List<Stat> statsB = parentB.GetStats();
        List<string> namesA = GetStatNames(statsA);
        List<string> namesB = GetStatNames(statsB);

        List<string> babyBaseStatNames = ChoosePrefab(parentA, parentB, namesA, namesB, result);

        BlendSharedStats(statsA, statsB, namesB, result);
        ResolveUniqueStats(statsA, statsB, namesA, namesB, babyBaseStatNames, result);

        result.babyColour = BlendColour(parentA, parentB);
        result.babyScale = BlendScale(parentA, parentB);

        TryMutate(result);

        return result;
    }

    private List<string> GetStatNames(List<Stat> stats)
    {
        return stats.ConvertAll(s => s.GetTrait().Name);
    }

    // Picks which parent's prefab the baby is built from. Returns that parent's stat
    // names, because those are the stats the baby "naturally" starts with.
    private List<string> ChoosePrefab(Creature parentA, Creature parentB,
                                      List<string> namesA, List<string> namesB,
                                      BreedingData result)
    {
        bool fromParentA = Random.value < 0.5f;
        GameObject prefabSource = fromParentA
            ? parentA.GetCreaturePrefab()
            : parentB.GetCreaturePrefab();

        if (prefabSource != null)
        {
            result.inheritedPrefab = prefabSource;
        }
        else
        {
            Debug.LogWarning("BreedingSystem: One or both parents have no " +
                             "prefab assigned. Check the Creature Inspector fields.");
        }

        return fromParentA ? namesA : namesB;
    }

    // Stats present on BOTH parents: the baby takes one parent's value at random.
    private void BlendSharedStats(List<Stat> statsA, List<Stat> statsB,
                                  List<string> namesB, BreedingData result)
    {
        foreach (Stat statA in statsA)
        {
            string statName = statA.GetTrait().Name;

            if (!namesB.Contains(statName))
            {
                continue;
            }

            Stat statB = statsB.Find(s => s.GetTrait().Name == statName);
            Stat inheritedStat = (Random.value < 0.5f) ? statB : statA;

            result.sharedStatValues[statName] = inheritedStat.GetTrait().Value;
        }
    }

    // Stats present on exactly ONE parent: each gets an independent chance to
    // appear on (or fall away from) the baby.
    private void ResolveUniqueStats(List<Stat> statsA, List<Stat> statsB,
                                    List<string> namesA, List<string> namesB,
                                    List<string> babyBaseStatNames, BreedingData result)
    {
        List<Stat> allUniqueStats = new List<Stat>();
        allUniqueStats.AddRange(statsA.FindAll(s => !namesB.Contains(s.GetTrait().Name)));
        allUniqueStats.AddRange(statsB.FindAll(s => !namesA.Contains(s.GetTrait().Name)));

        foreach (Stat uniqueStat in allUniqueStats)
        {
            string statName = uniqueStat.GetTrait().Name;
            bool babyNaturallyHasThis = babyBaseStatNames.Contains(statName);
            bool inheritRoll = Random.value < uniqueTraitChance;

            if (inheritRoll && !babyNaturallyHasThis)
            {
                result.uniqueStatsToAdd.Add(uniqueStat);
            }
            else if (!inheritRoll && babyNaturallyHasThis)
            {
                result.uniqueStatsToRemove.Add(statName);
            }
        }
    }

    private Color BlendColour(Creature parentA, Creature parentB)
    {
        Renderer rendererA = parentA.GetComponentInChildren<Renderer>();
        Renderer rendererB = parentB.GetComponentInChildren<Renderer>();

        if (rendererA == null || rendererB == null)
        {
            return Color.white;
        }

        return Color.Lerp(rendererA.material.color, rendererB.material.color, 0.5f);
    }

    private float BlendScale(Creature parentA, Creature parentB)
    {
        float baseScale = (Random.value < 0.5f)
            ? parentA.transform.localScale.x
            : parentB.transform.localScale.x;

        return baseScale + Random.Range(-0.1f, 0.1f);
    }

    // Rolls for a mutation. If it fires, it nudges one stat and the colour slightly.
    private void TryMutate(BreedingData result)
    {
        if (Random.value >= mutationChance)
        {
            return;
        }

        result.mutationOccurred = true;

        MutateRandomStat(result);
        result.babyColour = NudgeColour(result.babyColour);
    }

    private void MutateRandomStat(BreedingData result)
    {
        if (result.sharedStatValues.Count == 0)
        {
            return;
        }

        List<string> statNames = new List<string>(result.sharedStatValues.Keys);
        string mutatedStatName = statNames[Random.Range(0, statNames.Count)];
        float inheritedValue = result.sharedStatValues[mutatedStatName];

        // Magnitude is at least half the strength so a mutation is always measurable,
        // never so large that it looks like full randomisation.
        float magnitude = Random.Range(mutationStatStrength * 0.5f, mutationStatStrength);
        float direction = (Random.value < 0.5f) ? -1f : 1f;

        float mutatedValue = Mathf.Max(0f, inheritedValue * (1f + magnitude * direction));
        result.sharedStatValues[mutatedStatName] = mutatedValue;

        Debug.Log($"Mutation occurred! {mutatedStatName}: {inheritedValue:F2} -> {mutatedValue:F2}");
    }

    private Color NudgeColour(Color colour)
    {
        return new Color(
            Mathf.Clamp01(colour.r + Random.Range(-mutationColourShift, mutationColourShift)),
            Mathf.Clamp01(colour.g + Random.Range(-mutationColourShift, mutationColourShift)),
            Mathf.Clamp01(colour.b + Random.Range(-mutationColourShift, mutationColourShift)),
            1f
        );
    }
}