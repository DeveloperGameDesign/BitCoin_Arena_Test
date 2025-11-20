using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Settings")]
    [SerializeField] private AbilityEffect[] abilities;
    [SerializeField] private float minTimeForSpawn = 3f;
    [SerializeField] private float maxTimeForSpawn = 7f;
    [SerializeField] private int maxAbilitiesInScene = 3;

    private List<AbilityEffect> activeAbilities;
    private float currentTime=0f;
    private float nextTimeToSpawn = 0f;

    private void Awake()
    {
        nextTimeToSpawn = NextTimeToSpawn();
        activeAbilities = new List<AbilityEffect>();
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
        Camera cam = Camera.main;

        Vector2 min = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 max = cam.ViewportToWorldPoint(new Vector2(1, 1));

        float xPos = Random.Range(min.x, max.x);

        float yPos = Random.Range(min.y, (min.y + max.y) / 2f);

        Vector3 spawnPos = new Vector3(xPos, yPos,1);

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
