using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class PowerSwitch : MonoBehaviour, IInteractable {

    [SerializeField] private GameObject powerLever;
    [SerializeField] private Quaternion powerTurnedOnRotation;

    [Header("Localization")]
    [SerializeField] private LocalizedString interactText;

    private bool hasBeenFlipped;

    public string GetInteractText() {
        return interactText.GetLocalizedString();
    }

    [Command("TurnOnPower")]
    public void Interact() {
        if (hasBeenFlipped) {
            return;
        }

        hasBeenFlipped = true;

        StartCoroutine(PowerOn());
    }

    public IEnumerator PowerOn() {
        float duration = 1f; 
        float elapsedTime = 0f;

        Vector3 initialEulerAngles = powerLever.transform.eulerAngles;
        Vector3 targetEulerAngles = powerTurnedOnRotation.eulerAngles;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration; 

            Vector3 currentEulerAngles = new Vector3(
                Mathf.LerpAngle(initialEulerAngles.x, targetEulerAngles.x, t),
                initialEulerAngles.y, 
                initialEulerAngles.z  
            );

            powerLever.transform.rotation = Quaternion.Euler(currentEulerAngles);

            yield return null; 
        }

        powerLever.transform.rotation = powerTurnedOnRotation;

        AudioManager.Instance.PlaySound(AudioManager.Sound.SwitchPower, transform.position);

        yield return new WaitForSeconds(0.3f);

        LightManager.Instance.StopFlashingLights(LightManager.LightType.Factory);
        LightManager.Instance.TurnOnLights(LightManager.LightType.Factory);

        Vector3 lightFlickerSoundPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        AudioManager.Instance.PlaySound(AudioManager.Sound.LightsFlickerOn, lightFlickerSoundPos);
        EscapeManager.Instance.PowerOn();
    }
}
