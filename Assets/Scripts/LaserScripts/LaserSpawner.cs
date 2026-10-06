using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    public enum LaserOrientation { Horizontal, Vertical, Diagonal }

    public Laser laserPrefab;
    public Transform spawnCenter;

    [Header("Timing")]
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 2f;

    [Header("Orientations")]
    public bool spawnHorizontal = true;
    public bool spawnVertical = true;
    public bool spawnDiagonal;

    [Header("Random Offset From Center")]
    public float xRange = 5f;
    public float yRange = 3f;

    private readonly List<Laser> activeLasers = new List<Laser>();
    private readonly List<LaserOrientation> enabledOrientations = new List<LaserOrientation>();
    private Coroutine spawnRoutine;

    public void StartSpawning()
    {
        if (spawnRoutine != null) return;
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null) return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public void ClearLasers()
    {
        foreach (Laser laser in activeLasers)
        {
            if (laser != null)
                Destroy(laser.gameObject);
        }

        activeLasers.Clear();
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnLaser();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void SpawnLaser()
    {
        enabledOrientations.Clear();
        if (spawnHorizontal) enabledOrientations.Add(LaserOrientation.Horizontal);
        if (spawnVertical) enabledOrientations.Add(LaserOrientation.Vertical);
        if (spawnDiagonal) enabledOrientations.Add(LaserOrientation.Diagonal);

        if (enabledOrientations.Count == 0) return;

        LaserOrientation orientation = enabledOrientations[Random.Range(0, enabledOrientations.Count)];

        Vector3 offset = Vector3.zero;
        Quaternion rotation;

        switch (orientation)
        {
            case LaserOrientation.Horizontal:
                offset.y = Random.Range(-yRange, yRange);
                rotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case LaserOrientation.Vertical:
                offset.x = Random.Range(-xRange, xRange);
                rotation = Quaternion.identity;
                break;

            default:
                offset.x = Random.Range(-xRange, xRange);
                offset.y = Random.Range(-yRange, yRange);
                rotation = Quaternion.Euler(0f, 0f, Random.value < 0.5f ? 45f : -45f);
                break;
        }

        Laser laser = Instantiate(laserPrefab, spawnCenter.position + offset, rotation);
        laser.isMoving = true;

        activeLasers.RemoveAll(l => l == null);
        activeLasers.Add(laser);
    }
}