using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAbberation : MonoBehaviour {

    public static ChromaticAbberation Instance { get; private set; }

    [SerializeField] private Volume postProcessingVolume;

    private ChromaticAberration chromaticAberration;
    private Coroutine activeEffectCoroutine = null;  

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (postProcessingVolume.profile.TryGet(out chromaticAberration)) {
            chromaticAberration.intensity.value = 0f;  
        } else {
            Debug.LogWarning("No Chromatic Aberration effect found in the post-processing profile!");
        }
    }

    public void SetChromaticAberration(float intensity, float duration) {
        if (chromaticAberration == null) return;


        if (activeEffectCoroutine != null) {
            StopCoroutine(activeEffectCoroutine);
        }


        activeEffectCoroutine = StartCoroutine(ChromaticAberrationEffect(intensity, duration));
    }


    private IEnumerator ChromaticAberrationEffect(float intensity, float duration) {
        SetChromaticAberration(intensity);

        yield return new WaitForSeconds(duration); 

        StartCoroutine(FadeOutChromaticAberration());  
    }

    public void SetChromaticAberration(float intensity) {
        if (chromaticAberration != null) {
            if (activeEffectCoroutine != null) {
                StopCoroutine(activeEffectCoroutine);
            }

            if (intensity == 0) {
                StartCoroutine(FadeOutChromaticAberration());
                return;
            }
            chromaticAberration.intensity.value = Mathf.Clamp(intensity, 0f, 1f);
        }
    }


    private IEnumerator FadeOutChromaticAberration() {
        float timer = 0f;
        float duration = 2f;  
        float startIntensity = chromaticAberration.intensity.value;

        while (timer < duration) {
            timer += Time.deltaTime;
            float t = timer / duration;
            chromaticAberration.intensity.value = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        chromaticAberration.intensity.value = 0f;  
    }
}
