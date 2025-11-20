using MainScene.Types;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] private AbilityEffect[] abilities;
    [SerializeField] private float minTimeForSpawn = 3f;
    [SerializeField] private float maxTimeForSpawn = 7f;
    [SerializeField] private int maxAbilitiesInScene = 3;
    [SerializeField] private Boundary abilityBoundery;
    [SerializeField] private Transform boundaryHolder;

    private List<AbilityEffect> activeAbilities;
    private float currentTime=0f;
    private float nextTimeToSpawn = 0f;

    private void Awake()
    {
        nextTimeToSpawn = NextTimeToSpawn();
        activeAbilities = new List<AbilityEffect>();
        abilityBoundery = new Boundary(boundaryHolder.GetChild(0).position.y,
               boundaryHolder.GetChild(1).position.y,
               boundaryHolder.GetChild(2).position.x,
               boundaryHolder.GetChild(3).position.x);
    }

    private void Update()
    {
        if (activeAbilities.Count >= 3) return;

        if (currentTime<nextTimeToSpawn)
        {
        currentTime += Time.deltaTime;
        }
        else
        {
            currentTime = 0;
            nextTimeToSpawn = NextTimeToSpawn();
            SpawnAbility();
        }
    }
    private float NextTimeToSpawn()
    {
        return Random.Range(minTimeForSpawn, maxTimeForSpawn);
    }
    private void SpawnAbility()
    {
        Vector3 spawnPos = new Vector3(Random.Range(abilityBoundery.Left,abilityBoundery.Right), Random.Range(abilityBoundery.Down,abilityBoundery.Up),1);

        var newAbility = Instantiate(
            abilities[Random.Range(0, abilities.Length)],
            spawnPos,
            Quaternion.identity
        );

        newAbility.transform.SetParent(transform);
        activeAbilities.Add(newAbility);
    }

    public void RemoveAbility(AbilityEffect ability)
    {
        activeAbilities.Remove(ability);
    }
}
