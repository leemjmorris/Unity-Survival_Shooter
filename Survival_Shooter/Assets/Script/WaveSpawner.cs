using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject hellephantPrefab;
    public GameObject zombearPrefab;
    public GameObject zombunnyPrefab;

    [Header("Spawn Settings")]
    public float spawnRadius = 20f;
    public float minDistanceFromPlayer = 10f;  // LMJ: Don't spawn too close to player
    public int maxSpawnAttempts = 30;  // LMJ: Max attempts to find valid spawn point

    [Header("Wave Settings")]
    public float waveDuration = 30f;  // LMJ: 30 seconds per wave
    public float baseSpawnInterval = 1f;  // LMJ: Base spawn rate (1 per second)

    [Header("Wave Status")]
    public int currentWave = 0;
    public int enemiesSpawnedThisWave = 0;
    public int enemiesAliveThisWave = 0;
    public float waveTimeRemaining = 0f;
    public bool waveInProgress = false;

    [Header("Events")]
    public UnityEvent<int> onWaveStart;  // LMJ: Wave number
    public UnityEvent<int> onWaveComplete;  // LMJ: Wave number
    public UnityEvent<int, int> onEnemyCountChanged;  // LMJ: Current, Total

    private List<GameObject> activeEnemies = new List<GameObject>();
    private Coroutine waveCoroutine;
    private Transform player;

    private void Start()
    {
        // LMJ: Find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // LMJ: Start first wave after delay
        StartCoroutine(StartFirstWave());
    }

    private IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(3f);  // LMJ: Initial delay
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (waveInProgress) return;

        currentWave++;
        waveInProgress = true;
        enemiesSpawnedThisWave = 0;
        enemiesAliveThisWave = 0;
        waveTimeRemaining = waveDuration;

        // LMJ: Calculate enemies for this wave
        int enemiesToSpawn = CalculateEnemiesForWave(currentWave);

        onWaveStart?.Invoke(currentWave);

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
        waveCoroutine = StartCoroutine(WaveRoutine(enemiesToSpawn));
    }

    private int CalculateEnemiesForWave(int waveNumber)
    {
        //Wave 1: 30 enemies (1 per second for 30 seconds)
        //Wave 2: 60 enemies (2 per second)
        //Wave 3: 120 enemies (4 per second)
        return 30 * (int)Mathf.Pow(2, waveNumber - 1);
    }

    private IEnumerator WaveRoutine(int totalEnemies)
    {
        float spawnTimer = 0f;
        float spawnInterval = waveDuration / totalEnemies;  // LMJ: Time between spawns

        while (waveTimeRemaining > 0f && enemiesSpawnedThisWave < totalEnemies)
        {
            waveTimeRemaining -= Time.deltaTime;
            spawnTimer += Time.deltaTime;

            // LMJ: Spawn enemy at interval
            if (spawnTimer >= spawnInterval)
            {
                SpawnRandomEnemy();
                enemiesSpawnedThisWave++;
                spawnTimer = 0f;

                onEnemyCountChanged?.Invoke(enemiesAliveThisWave, totalEnemies);
            }

            yield return null;
        }

        while (enemiesAliveThisWave > 0)
        {
            yield return new WaitForSeconds(0.5f);  // LMJ: Check every 0.5 seconds
        }

        // LMJ: Wave complete!
        CompleteWave();
    }

    private void CompleteWave()
    {
        waveInProgress = false;

        onWaveComplete?.Invoke(currentWave);

        // LMJ: Auto start next wave after delay
        StartCoroutine(NextWaveDelay());
    }

    private IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(5f);  // LMJ: 5 second break between waves
        StartNextWave();
    }

    private void SpawnRandomEnemy()
    {
        // LMJ: Choose enemy type based on wave
        GameObject enemyToSpawn = ChooseEnemyType();

        if (enemyToSpawn == null) return;

        // LMJ: Find valid spawn position on NavMesh
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition == Vector3.zero) return;

        // LMJ: Spawn enemy
        GameObject enemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
        activeEnemies.Add(enemy);
        enemiesAliveThisWave++;

        // LMJ: Track when enemy dies
        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
        if (enemyBase != null)
        {
            enemyBase.OnDeath += () => OnEnemyDeath(enemy);
        }
    }

    private GameObject ChooseEnemyType()
    {
        // LMJ: Weighted selection based on wave
        float random = Random.Range(0f, 1f);

        if (currentWave <= 2)
        {
            // LMJ: Early waves - mostly ZomBunnies
            if (random < 0.6f) return zombunnyPrefab;
            else if (random < 0.9f) return zombearPrefab;
            else return hellephantPrefab;
        }
        else if (currentWave <= 5)
        {
            // LMJ: Mid waves - balanced
            if (random < 0.4f) return zombunnyPrefab;
            else if (random < 0.7f) return zombearPrefab;
            else return hellephantPrefab;
        }
        else
        {
            // LMJ: Late waves - harder enemies
            if (random < 0.2f) return zombunnyPrefab;
            else if (random < 0.6f) return zombearPrefab;
            else return hellephantPrefab;
        }
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            // LMJ: Random position in circle
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // LMJ: Check distance from player
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(randomPosition, player.position);
                if (distanceToPlayer < minDistanceFromPlayer)
                {
                    continue;  // LMJ: Too close to player, try again
                }
            }

            // LMJ: Sample position on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;  // LMJ: Valid position found!
            }
        }

        // LMJ: Fallback to spawner position
        return transform.position;
    }

    private void OnEnemyDeath(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        enemiesAliveThisWave--;
        onEnemyCountChanged?.Invoke(enemiesAliveThisWave, CalculateEnemiesForWave(currentWave));
    }
}