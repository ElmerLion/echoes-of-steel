using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowingEyesSpawner : MonoBehaviour {

    [SerializeField] private List<Transform> glowingEyesSpawnPoints;
    [SerializeField] private GameObject glowingEyesPrefab;
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float distanceToAnimatronic;

    private GameObject spawnedEyes;
    private float eyeTimer;
    private float eyeTimerMax = 10f;
    private float eyeSpawnTimer;
    private float eyeSpawnTimerMax = 5f;

    private void Update() {
        if (spawnedEyes == null) {
            eyeSpawnTimer += Time.deltaTime;
            if (eyeSpawnTimer >= eyeSpawnTimerMax) {
                SpawnGlowingEyes();
                eyeSpawnTimer = 0f;
            }
        }

        if (spawnedEyes == null) return;

        Vector3 lookDir = Player.Instance.transform.position - spawnedEyes.transform.position;
        spawnedEyes.transform.rotation = Quaternion.LookRotation(lookDir);

        eyeTimer += Time.deltaTime;
        if (eyeTimer >= eyeTimerMax) {
            Destroy(spawnedEyes);
            eyeTimer = 0f;
        }

        if (Vector3.Distance(spawnedEyes.transform.position, Player.Instance.transform.position) < distanceToPlayer) {
            Destroy(spawnedEyes);
            //Debug.Log("Player moved too close to eyes!");
            eyeTimer = 0f;
        }

        foreach (Animatronic animatronic in HorrorGameManager.Instance.AllAnimatronics) {
            if (Vector3.Distance(animatronic.transform.position, spawnedEyes.transform.position) < distanceToAnimatronic) {
                Destroy(spawnedEyes);
                //Debug.Log("Animatronic moved too close to eyes!");
            }
        }
    }

    private void SpawnGlowingEyes() {
        Transform spawnPoint = GetRandomSpawnPoint();

        if (spawnPoint == null) return;

        spawnedEyes = Instantiate(glowingEyesPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    private Transform GetRandomSpawnPoint() {
        if (glowingEyesSpawnPoints.Count == 0) return null;

        List<Transform> validSpawnPoints = new List<Transform>();

        foreach (Transform spawnPoint in glowingEyesSpawnPoints) {
            if (spawnPoint == null) continue;
            if (Vector3.Distance(spawnPoint.position, Player.Instance.transform.position) < distanceToPlayer) continue;

            validSpawnPoints.Add(spawnPoint);
        }

        if (validSpawnPoints.Count == 0) return null;

        return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
    }

}
