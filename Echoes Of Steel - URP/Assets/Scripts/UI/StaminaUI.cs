using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour {

    public static StaminaUI Instance { get; private set; }

    [SerializeField] private Image staminaBar;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Player.Instance.OnStaminaChanged += UpdateStaminaUI;

        SetStaminaBarVisibility(PlayerPrefs.GetInt("ShowStaminaBar", 0) == 1 ? true : false);
    }

    private void UpdateStaminaUI() {
        staminaBar.fillAmount = Player.Instance.Stamina / Player.Instance.MaxStamina;
    }

    public void SetStaminaBarVisibility(bool visible) {
        gameObject.SetActive(visible);
    }

}
