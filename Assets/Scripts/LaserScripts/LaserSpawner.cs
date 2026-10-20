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

    [Header("Diagonal")]
    public float diagonalYOffset = -0.5f;
    public float diagonalXJitter = 0.5f;
    public float minDiagonalAngle = 30f;
    public float maxDiagonalAngle = 60f;

    private readonly List<Laser> activeLasers = new List<Laser>();
    private readonly List<LaserOrientation> enabledOrientations = new List<LaserOrientation>();
    private Coroutine spawnRoutine;

    private float alarmEndTime;
    private Coroutine alarmRoutine;

    public void ActivateFor(float duration)
    {
        alarmEndTime = Mathf.Max(alarmEndTime, Time.time + duration);
        StartSpawning();

        if (alarmRoutine == null)
            alarmRoutine = StartCoroutine(AlarmTimer());
    }

    private IEnumerator AlarmTimer()
    {
        while (Time.time < alarmEndTime)
            yield return null;

        alarmRoutine = null;
        StopSpawning();
    }

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
        if (alarmRoutine != null)
        {
            StopCoroutine(alarmRoutine);
            alarmRoutine = null;
        }
        alarmEndTime = 0f;

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
                offset.x = Random.Range(-diagonalXJitter, diagonalXJitter);
                offset.y = diagonalYOffset;

                float angle = Random.Range(minDiagonalAngle, maxDiagonalAngle);
                float side = Random.value < 0.5f ? -1f : 1f;
                rotation = Quaternion.Euler(0f, 0f, angle * side);
                break;
        }

        Laser laser = Instantiate(laserPrefab, spawnCenter.position + offset, rotation);
        laser.isMoving = true;

        activeLasers.RemoveAll(l => l == null);
        activeLasers.Add(laser);
    }
}