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
    private bool overcrowdWarningShown = false;
    private float timeRemaining = 0f;

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

        // Allow the overcrowding warning to appear again next time it happens.
        if (creatureCount <= 2)
        {
            overcrowdWarningShown = false;
        }

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
            if (!overcrowdWarningShown)
            {
                UIManager.Instance.DisplayWarningMessage(
                    "Too many creatures in the breeding room! Remove "
                    + (creatureCount - 2)
                    + " creature(s)."
                );
                overcrowdWarningShown = true;
            }
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

        yield return StartCoroutine(RunTimer(breedingTime));

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

        OnBabyReady?.Invoke(babyData, this.transform);

        UIManager.Instance.DisplayWarningMessage("A new creature has been born!");

        isBreeding = false;

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isOnCooldown = true;
        Debug.Log($"Breeding room on cooldown for {breedingCooldown} seconds.");

        yield return StartCoroutine(RunTimer(breedingCooldown));

        isOnCooldown = false;
        Debug.Log("Breeding room cooldown complete. Ready to breed again.");
    }

    // Shared by the breeding phase and the cooldown phase. Counts timeRemaining
    // down to zero so ToString() can show a live countdown in the info panel.
    private IEnumerator RunTimer(float duration)
    {
        timeRemaining = duration;

        while (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        timeRemaining = 0f;
    }

    public override string ToString()
    {
        int creatureCount = GetRoomCount();

        if (creatureCount > 2)
        {
            return "Too many creatures! Remove " + (creatureCount - 2) + " to breed.";
        }

        if (isBreeding)
        {
            return "Breeding in progress...\n" + Mathf.CeilToInt(timeRemaining) + "s remaining";
        }

        if (isOnCooldown)
        {
            return "Breeding room is on cooldown.\nReady in " + Mathf.CeilToInt(timeRemaining) + "s";
        }

        return "Place exactly 2 creatures here to breed them.";
    }
}