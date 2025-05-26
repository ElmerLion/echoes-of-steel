using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour {

    public static MessageUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI messageText;

    private float messageTimer;
    private float messageTimerMax;
    private bool isDisplayingMessage;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        messageText.text = "";
    }

    private void Update() {
        if (isDisplayingMessage) {
            messageTimer += Time.deltaTime;
            if (messageTimer >= messageTimerMax) {
                HideMessage();
            }
        }
    }

    public void ShowMessage(string message, float duration = 2f) {
        messageTimerMax = duration;
        messageText.text = message;
        isDisplayingMessage = true;
    }

    private void HideMessage() {
        messageText.text = "";
        isDisplayingMessage = false;
        messageTimer = 0;
    }




}
