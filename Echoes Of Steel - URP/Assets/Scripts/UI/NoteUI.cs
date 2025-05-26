using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteUI : BaseUI {

    public static NoteUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI noteText;

    private void Awake() {
        Instance = this;
    }

    public override void Start() {
        base.Start();
        GameInput.Instance.OnInteract += GameInput_OnInteract;
    }

    private void GameInput_OnInteract() {
        if (isOpen) {
            Hide();
        }
    }

    public void Show(string text) {
        noteText.text = text;
        base.Show();
    }
}
