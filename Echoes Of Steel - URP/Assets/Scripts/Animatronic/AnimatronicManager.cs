using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Jobs;
using Steamworks;

public class AnimatronicManager : MonoBehaviour {

    public static AnimatronicManager Instance { get; private set; }

    // Handle waking up the right animatronics
    // Keep track of all the active animatronics
    // Spawn animatronics in the correct randomized places

    public enum Stage {
        Start,
        Factory,
        Mine,
        Ending,
    }

    [SerializeField] private List<AnimatronicStage> stageAnimatronics;

    public List<Animatronic> ActiveAnimatronics { get; private set; }
    private List<Animatronic> sleepingAnimatronics;
    private float notSpottedTimerMax = 600f;

    public Stage CurrentStage { get; private set; }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        ActiveAnimatronics = new List<Animatronic>();
        sleepingAnimatronics = new List<Animatronic>();

        foreach (AnimatronicStage stage in stageAnimatronics) {
            foreach (Animatronic animatronic in stage.animatronics) {
                sleepingAnimatronics.Add(animatronic);
            }
        }

        CurrentStage = Stage.Start;
        SpawnAnimatronics();
        SetStage(Stage.Factory);
    }

    private void Update() {
        Animatronic.NotSpottedTimer += Time.deltaTime;
        if (Animatronic.NotSpottedTimer >= notSpottedTimerMax) {
            HorrorGameManager.TryUnlockAchievement("NOT_SPOTTED");
        }
    }

    private void SpawnAnimatronics() {
        foreach (AnimatronicStage animatronicStage in stageAnimatronics) {
            if (animatronicStage.stage == Stage.Ending) continue;

            foreach (Animatronic animatronic in animatronicStage.animatronics) {
                Transform spawnPoint = GetRandomSpawnPoint(animatronicStage.stage);

                animatronic.GetComponent<NavMeshAgent>().enabled = false;
                animatronic.transform.position = spawnPoint.position;
                animatronic.GetComponent<NavMeshAgent>().enabled = true;
                animatronic.gameObject.SetActive(true);
            }
        }
    }

    private Transform GetRandomSpawnPoint(Stage activeStage) {
        foreach (AnimatronicStage animatronicStage in stageAnimatronics) {
            if (animatronicStage.stage == activeStage) {
                Transform spawnPoint = animatronicStage.spawnPoints[Random.Range(0, animatronicStage.spawnPoints.Count)];
                
                animatronicStage.spawnPoints.Remove(spawnPoint);
                return spawnPoint;
            }
        }

        return null;
    }

    public void SetStage(Stage newStage) {
        CurrentStage = newStage;

        List<Animatronic> activeAnimatronicsCopy = new List<Animatronic>(ActiveAnimatronics);
        foreach (Animatronic animatronic in activeAnimatronicsCopy) {
            animatronic.PowerDown();
            ActiveAnimatronics.Remove(animatronic);
            sleepingAnimatronics.Add(animatronic);
        }

        foreach (AnimatronicStage stage in stageAnimatronics) {
            if (stage.stage == CurrentStage) {
                foreach (Animatronic animatronic in stage.animatronics) {
                    animatronic.gameObject.SetActive(true);
                    ActiveAnimatronics.Add(animatronic);
                    sleepingAnimatronics.Remove(animatronic);
                    animatronic.SetIsWakingUp(true);
                }
            }
        }
    }


        [System.Serializable]
    public class AnimatronicStage {
        public Stage stage;
        public List<Animatronic> animatronics;
        public List<Transform> spawnPoints;
    }
}
