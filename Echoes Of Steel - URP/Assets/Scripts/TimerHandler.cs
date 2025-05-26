using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerHandler : MonoBehaviour  {

    
    private static TimerHandler instance;
    private List<Timer> timers = new List<Timer>();

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        for (int i = 0; i < timers.Count; i++) {
            timers[i].Update();
        }
    }

    public static void StartTimer(float duration, System.Action callback) {
        instance.timers.Add(new Timer(duration, callback));
    }

    private class Timer {
        private float duration;
        private System.Action callback;

        public Timer(float duration, System.Action callback) {
            this.duration = duration;
            this.callback = callback;
        }

        public void Update() {
            duration -= Time.deltaTime;
            if (duration <= 0) {
                callback();
                instance.timers.Remove(this);
            }
        }
    }
    
}
