using System.Collections.Generic;
using UnityEngine;

public class BreedingData
{
    public Color babyColour;
    public float babyScale;
    public Dictionary<string, float> statValues;
    public bool mutationOccurred;
    public GameObject inheritedPrefab;

    public BreedingData()
    {
        statValues = new Dictionary<string, float>();
        mutationOccurred = false;
    }
}