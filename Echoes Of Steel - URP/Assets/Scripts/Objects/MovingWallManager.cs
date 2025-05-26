using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingWallManager : MonoBehaviour {

    [SerializeField] private List<GameObject> movingWallList;
    [SerializeField] private float yMovedPosition = 10.2f;

    private GameObject movedWall;
    private List<Vector3> originalPositions;

    private float moveWallTimer;
    private float moveWallTimerMax;
    private float shakeCameraDistance = 35f;

    private void Start() {
        originalPositions = new List<Vector3>();

        foreach (GameObject movingWall in movingWallList) {
            originalPositions.Add(movingWall.transform.localPosition);
        }
    }

    private void Update() {
        moveWallTimer += Time.deltaTime;

        if (moveWallTimer >= moveWallTimerMax) {
            MoveWall();
            moveWallTimer = 0f;
            moveWallTimerMax = Random.Range(20f, 30f);
        }

        foreach (Animatronic animatronic in HorrorGameManager.Instance.AllAnimatronics) {
            if (Vector3.Distance(animatronic.transform.position, movedWall.transform.position) < 4f) {
                MoveWall();
            }
        }
    }

    private void MoveWall() {
        if (movedWall != null) {
            movedWall.transform.localPosition = originalPositions[movingWallList.IndexOf(movedWall)];
            AudioManager.Instance.PlaySound(AudioManager.Sound.WallMoved, movedWall.transform.position);
            if (Vector3.Distance(movedWall.transform.position, Player.Instance.transform.position) < shakeCameraDistance) {
                CameraShake.Instance.ShakeCamera(1f, .3f, 0.25f);
            }
        }

        int randomWallIndex = Random.Range(0, movingWallList.Count);
        movedWall = movingWallList[randomWallIndex];

        Vector3 newPosition = new Vector3(movedWall.transform.localPosition.x, yMovedPosition, movedWall.transform.localPosition.z);
        movedWall.transform.localPosition = newPosition;

        AudioManager.Instance.PlaySound(AudioManager.Sound.WallMoved, newPosition);

        if (Vector3.Distance(newPosition, Player.Instance.transform.position) < shakeCameraDistance) {
            CameraShake.Instance.ShakeCamera(1f, .3f, 0.25f);
        }
    }
    
}
