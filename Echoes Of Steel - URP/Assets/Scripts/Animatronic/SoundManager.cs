using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    private List<SoundEmitter> soundEmitters = new List<SoundEmitter>();

    private void Awake() {
        Instance = this;
    }

    public void EmitSound(Vector3 position, float duration, float loudness) {
        SoundEmitter newEmitter = new SoundEmitter(position, duration, loudness);
        soundEmitters.Add(newEmitter);

        foreach (Animatronic animatronic in AnimatronicManager.Instance.ActiveAnimatronics) {
            animatronic.OnSoundEmitted(newEmitter);
        }
    }

    private void Update() {
        for (int i = soundEmitters.Count - 1; i >= 0; i--) {
            if (soundEmitters[i].IsExpired()) {
                soundEmitters.RemoveAt(i);
            }
        }
    }

    public class SoundEmitter {
        public Vector3 Position { get; private set; }
        public float Loudness { get; private set; }
        private float expirationTime;

        public SoundEmitter(Vector3 position, float duration, float loudness) {
            Position = position;
            expirationTime = Time.time + duration;
            this.Loudness = loudness;
        }

        public bool IsExpired() {
            return Time.time > expirationTime;
        }
    }
}
