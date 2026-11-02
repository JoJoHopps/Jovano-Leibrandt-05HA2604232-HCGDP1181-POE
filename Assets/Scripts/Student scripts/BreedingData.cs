using System.Collections.Generic;
using UnityEngine;

public class BreedingData
{
    public Color babyColour;
    public float babyScale;
    public GameObject inheritedPrefab;
    public bool mutationOccurred;

    // Stats both parents share (e.g. Health, Speed) — blended values to apply
    // to stats the baby's prefab already has.
    public Dictionary<string, float> sharedStatValues;

    // Stats only one parent had, which won the inheritance roll and need to be
    // added to the baby even though its base prefab doesn't naturally have them.
    public List<Stat> uniqueStatsToAdd;

    // Stats the baby's base prefab naturally comes with, but which lost the
    // inheritance roll and need to be stripped off entirely.
    public List<string> uniqueStatsToRemove;

    public BreedingData()
    {
        sharedStatValues = new Dictionary<string, float>();
        uniqueStatsToAdd = new List<Stat>();
        uniqueStatsToRemove = new List<string>();
        mutationOccurred = false;
    }
}