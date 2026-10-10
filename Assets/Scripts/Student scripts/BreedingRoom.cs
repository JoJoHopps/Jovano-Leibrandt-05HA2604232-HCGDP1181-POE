using System;
using System.Collections;
using UnityEngine;

public class BreedingRoom : Room
{
    [SerializeField] private BreedingSystem breedingSystem;
    [SerializeField] private float breedingTime = 10f;
    [SerializeField] private float breedingCooldown = 5f;

    private bool isBreeding = false;
    private bool isOnCooldown = false;

    // FA2 UPGRADE: Observer pattern via C# event.
    // BreedingRoom no longer needs to know CreatureSpawner
    // exists. It simply announces "a baby is ready" and anyone
    // subscribed (CreatureSpawner) responds. This replaces the
    // FA1 approach of holding a direct serialized reference to
    // CreatureSpawner and calling it manually.
    public event Action<BreedingData, Transform> OnBabyReady;

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

        // FA2 FEATURE: Cooldown prevents instant repeat breeding
        if (isOnCooldown)
        {
            return;
        }

        if (creatureCount < 2)
        {
            return;
        }

        if (creatureCount > 2)
        {
            UIManager.Instance.DisplayWarningMessage(
                "Too many creatures in the breeding room! Remove "
                + (creatureCount - 2)
                + " creature(s)."
            );
            return;
        }

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

        if (GetRoomCount() != 2)
        {
            Debug.Log("Breeding cancelled. Creature count changed.");
            UIManager.Instance.DisplayWarningMessage("Breeding cancelled!");
            isBreeding = false;
            yield break;
        }

        Creature parentA = children[0].GetComponent<Creature>();
        Creature parentB = children[1].GetComponent<Creature>();

        if (parentA == null || parentB == null)
        {
            Debug.LogWarning("BreedingRoom: Could not find Creature components.");
            isBreeding = false;
            yield break;
        }

        BreedingData babyData = breedingSystem.Breed(parentA, parentB);

        Debug.Log("Breeding calculation complete.");
        Debug.Log($"Baby colour will be: {babyData.babyColour}");
        Debug.Log($"Baby scale will be: {babyData.babyScale}");
        Debug.Log($"Mutation occurred: {babyData.mutationOccurred}");

        // Announce the baby is ready. CreatureSpawner is
        // listening and will handle the actual instantiation.
        OnBabyReady?.Invoke(babyData, this.transform);

        UIManager.Instance.DisplayWarningMessage("A new creature has been born!");

        isBreeding = false;

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        Debug.Log($"Breeding room on cooldown for {breedingCooldown} seconds.");
        yield return new WaitForSeconds(breedingCooldown);
        isOnCooldown = false;
        Debug.Log("Breeding room cooldown complete. Ready to breed again.");
    }

    public override string ToString()
    {
        if (isBreeding)
        {
            return "Breeding in progress...";
        }

        if (isOnCooldown)
        {
            return "Breeding room is on cooldown.";
        }

        return "Place exactly 2 creatures here to breed them.";
    }
}