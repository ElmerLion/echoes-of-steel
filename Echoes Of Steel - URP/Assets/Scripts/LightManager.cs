using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour {

    public static LightManager Instance { get; private set; }

    public enum LightType {
        Factory,
        Mine
    }

    [SerializeField] private List<LightStage> stages;

    private void Awake() {
        Instance = this;
    }

    private void Start() {

        foreach (LightStage stage in stages) {
            if (stage.enableOnStart) {
                TurnOnLights(stage.lightParents, stage.singleLights);
            }
            if (stage.startFlashingOnStart) {
                FlashLightsContinuously(stage);
            } 
            if (!stage.enableOnStart && !stage.startFlashingOnStart) {
                TurnOffLights(stage.lightParents, stage.singleLights);
            }
        }
    }

    public void TurnOffLights(List<GameObject> lightParents, List<Light> singleLights) {
        foreach (Light light in singleLights) {
            light.enabled = false;
        }
        foreach (GameObject lightParent in lightParents) {
            foreach (Transform child in lightParent.transform) {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void TurnOnLights(LightType lightType) {
        LightStage lightStage = GetLightStageObject(lightType);

        TurnOnLights(lightStage.lightParents, lightStage.singleLights);
    }

    public void TurnOnLights(List<GameObject> lightParents, List<Light> singleLights) {
        foreach (Light light in singleLights) {
            light.enabled = true;
        }
        foreach (GameObject lightParent in lightParents) {
            foreach (Transform child in lightParent.transform) {
                child.gameObject.SetActive(true);
            }
        }
    }

    public void ToggleLights(List<GameObject> lightParents, List<Light> singleLights) {
        foreach (var light in singleLights) {
            light.enabled = !light.enabled;
        }
        foreach (GameObject lightParent in lightParents) {
            foreach (Transform child in lightParent.transform) {
                child.gameObject.SetActive(!child.gameObject.activeSelf);

            }

        }
    }

    public void FlickLights(LightStage lightStage) {
        if (!lightStage.isFlickering) { 
            StartCoroutine(Flicker(lightStage));
        }
    }

    private IEnumerator Flicker(LightStage lightStage) {
        lightStage.isFlickering = true;

        foreach (Light light in lightStage.singleLights) {
            light.enabled = !light.enabled;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.5f));
        }

        foreach (GameObject lightParent in lightStage.lightParents) {
            foreach (Transform child in lightParent.transform) {
                child.gameObject.SetActive(!child.gameObject.activeSelf);
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 3f));
        }

        lightStage.isFlickering = false; 
    }

    private void Update() {
        foreach (LightStage stage in stages) {
            if (stage.flashLights && !stage.isFlickering) {
                FlickLights(stage);
            }
        }
    }

    public void FlashLightsContinuously(LightType lightType) {
        LightStage lightStage = GetLightStageObject(lightType);
        FlashLightsContinuously(lightStage);
    }

    public void FlashLightsContinuously(LightStage lightStage) {
        lightStage.flashLights = true;
        FlickLights(lightStage);
    }

    public void StopFlashingLights(LightType lightType) {
        LightStage lightStage = GetLightStageObject(lightType);
        StopFlashingLights(lightStage);
    }

    public void StopFlashingLights(LightStage lightStage) {
        lightStage.flashLights = false;
    }

    public LightStage GetLightStageObject(LightType lightType) {
        foreach (LightStage stage in stages) {
            if (stage.lightType == lightType) {
                return stage;
            }
        }
        return null;
    }


    [System.Serializable]
    public class LightStage {
        public LightType lightType;
        public bool startFlashingOnStart;
        public bool enableOnStart;
        public List<GameObject> lightParents;
        public List<Light> singleLights;

        [HideInInspector] public bool flashLights = false;
        [HideInInspector] public bool isFlickering = false;
    }


}
