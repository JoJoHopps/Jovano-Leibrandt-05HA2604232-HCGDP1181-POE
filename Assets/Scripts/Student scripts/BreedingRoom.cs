using System.Collections;
using UnityEngine;

public class BreedingRoom : Room
{
    [SerializeField] private BreedingSystem breedingSystem;
    [SerializeField] private CreatureSpawner creatureSpawner;
    [SerializeField] private float breedingTime = 10f;

    private bool isBreeding = false;

    public override void Start()
    {
        base.Start();
    }

    private void Update()
    {
        HandleBreeding();
    }

    private void HandleBreeding()
    {
        int creatureCount = GetRoomCount();

        // Not enough creatures
        if (creatureCount < 2)
        {
            return;
        }

        // Too many creatures
        if (creatureCount > 2)
        {
            UIManager.Instance.DisplayWarningMessage(
                "Too many creatures in the breeding room! Remove "
                + (creatureCount - 2)
                + " creature(s)."
            );
            return;
        }

        // Exactly 2 creatures and not already breeding
        if (creatureCount == 2 && !isBreeding)
        {
            StartCoroutine(BreedingCountdown());
        }
    }

    private IEnumerator BreedingCountdown()
    {
        isBreeding = true;

        Debug.Log("Breeding started. Please wait...");
        UIManager.Instance.DisplayWarningMessage("Breeding in progress...");

        yield return new WaitForSeconds(breedingTime);

        // Check creatures are still in the room after waiting
        if (GetRoomCount() != 2)
        {
            Debug.Log("Breeding cancelled. Creature count changed.");
            UIManager.Instance.DisplayWarningMessage("Breeding cancelled!");
            isBreeding = false;
            yield break;
        }

        // Get the two parents
        Creature parentA = children[0].GetComponent<Creature>();
        Creature parentB = children[1].GetComponent<Creature>();

        if (parentA == null || parentB == null)
        {
            Debug.LogWarning("BreedingRoom: Could not find Creature components.");
            isBreeding = false;
            yield break;
        }

        // Run the breeding logic and log the result
        BreedingData babyData = breedingSystem.Breed(parentA, parentB);

        Debug.Log("Breeding calculation complete.");
        Debug.Log($"Baby colour will be: {babyData.babyColour}");
        Debug.Log($"Baby scale will be: {babyData.babyScale}");
        Debug.Log($"Mutation occurred: {babyData.mutationOccurred}");

        // Pass to spawner — spawner implementation in progress
        if (creatureSpawner != null)
        {
            creatureSpawner.SpawnBaby(babyData, this.transform);
        }
        else
        {
            Debug.LogWarning("BreedingRoom: CreatureSpawner not yet assigned.");
        }

        UIManager.Instance.DisplayWarningMessage("Breeding complete! Spawning coming soon.");

        isBreeding = false;
    }

    public override string ToString()
    {
        if (isBreeding)
        {
            return "Breeding in progress...";
        }

        return "Place exactly 2 creatures here to breed them.";
    }
}