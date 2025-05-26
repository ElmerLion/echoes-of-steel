using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AliveMainMenu : MonoBehaviour {

    [SerializeField] private GameObject blinkingLight;
    [SerializeField] private List<GameObject> animatronics;
    [SerializeField] private List<Animator> animatronicAnimators;
    [SerializeField] private List<Transform> standPositions;

    private bool isFlickering;
    private bool flashLights;

    private void Start() {
        //FlashLightsContinuously();
    }

    public void FlickLights() {
        if (!isFlickering) {
            StartCoroutine(Flicker());
        }
    }

    private IEnumerator Flicker() {
        isFlickering = true;
        foreach (Transform child in blinkingLight.transform) {
            bool newState = !child.gameObject.activeSelf;

            if (newState) {
                //ShowAnimatronics();
            }

            child.gameObject.SetActive(newState);
        }
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 3f));

        isFlickering = false;
    }

    private void Update() {
        if (flashLights && !isFlickering) {
            FlickLights();

            /*int random = Random.Range(0, 2);

            if (!blinkingLight.transform.GetChild(0).gameObject.activeSelf && random == 1) {
                MoveAnimatronics();
                foreach (GameObject animatronic in animatronics) {
                    animatronic.gameObject.SetActive(false);
                }
            }*/
        }
    }

    public void FlashLightsContinuously() {
        flashLights = true;
        FlickLights();
    }

    public void StopFlashingLights() {
        flashLights = false;
    }

    public void MoveAnimatronics() {
        List<Transform> availablePositions = new List<Transform>(standPositions);

        foreach (GameObject animatronic in animatronics) {
            Transform positionTransform = availablePositions[Random.Range(0, availablePositions.Count)];
            animatronic.transform.position = positionTransform.position;
            availablePositions.Remove(positionTransform);

            Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.position - animatronic.transform.position, Vector3.up);
            rotation.x = 0;
            rotation.z = 0;
            animatronic.transform.rotation = rotation;

            
        }
    }

    private void ShowAnimatronics() {
        foreach (GameObject animatronic in animatronics) {

            int randomAnimation = Random.Range(0, 2);
            bool idle = randomAnimation == 1;
            bool sit = randomAnimation == 0;

            Animator animator = animatronicAnimators[animatronics.IndexOf(animatronic)];

            animatronic.gameObject.SetActive(true);

            animator.SetBool("Idle", idle);
            animator.SetBool("Sit", sit);

        }


    }

}
