using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour {

    [Header("Flashlight Arrow")]
    [SerializeField] private GameObject flashlightArrow;
    [SerializeField] private Flashlight flashLight;

    public enum Stage {
        PickupFlashlight,
        End
    }

    //private Stage currentStage = Stage.PickupFlashlight;

    private void Start() {
        flashlightArrow.SetActive(true);

        flashLight.OnItemInteracted += () => {
            flashlightArrow.SetActive(false);
            //currentStage = Stage.End;
        };
    }

}
