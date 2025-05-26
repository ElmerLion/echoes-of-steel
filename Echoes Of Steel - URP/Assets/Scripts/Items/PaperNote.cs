using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Steamworks;

public class PaperNote : MonoBehaviour, IInteractable {

    public static int NotesRead { get; private set; }

    [TextArea(3, 10)]
    [SerializeField] private string noteText;
    [SerializeField] private LocalizedString localizedNoteText;
    [SerializeField] private LocalizedString localizedInteractText;

    private bool hasBeenRead = false;

    public string GetInteractText() {
        return localizedInteractText.GetLocalizedString();
    }

    public void Interact() {
        if (NoteUI.Instance.isOpen) return;

        if (!hasBeenRead) {
            NotesRead++;

            HorrorGameManager.TryUnlockAchievement("READ_NOTES");
        }
        hasBeenRead = true;

        if (localizedNoteText == null) {
            NoteUI.Instance.Show(noteText);
        } else {
            NoteUI.Instance.Show(localizedNoteText.GetLocalizedString());
        }

        AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerReadNote, 2f);
    }
}
