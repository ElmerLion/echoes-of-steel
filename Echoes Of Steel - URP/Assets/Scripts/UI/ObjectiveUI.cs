using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using static ObjectiveUI;

public class ObjectiveUI : MonoBehaviour {

    public static ObjectiveUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private LocalizedString timerString;

    [Header("Objectives")]
    [SerializeField] private List<Objective> objectives;

    public ObjectiveType currentObjective { get; private set; }


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        HorrorGameManager.Instance.OnPlayerLocationChanged += OnPlayerLocationChanged;
        timerText.gameObject.SetActive(false);
    }

    private void OnPlayerLocationChanged(PlayerLocation location) {
        if (currentObjective == ObjectiveType.FindSoundSource && location == PlayerLocation.MineZone) {
            SetObjective(ObjectiveType.ExploreMine);
        } 
    }

    public void UpdateTimerText(float secondsLeft) {
        timerText.text = string.Format(timerString.GetLocalizedString(), (int)secondsLeft);

        if (secondsLeft <= 0) {
            timerText.gameObject.SetActive(false);
        } else {
            timerText.gameObject.SetActive(true);
        }
    }


    public void SetObjective(ObjectiveType type) {
        Objective objective = objectives.Find(o => o.Type == type);
        if (objective != null) {
            objectiveText.text = objective.Description.GetLocalizedString();
        }
    }
}

public enum ObjectiveType {
    FindWayOut,
    PerformFirstRitual,
    FindSoundSource,
    ExploreMine,
    PerformSecondRitual,
    FindExit,
    NoEscape,
}

[System.Serializable]
public class Objective {
    public ObjectiveType Type;
    public LocalizedString Description;
}
