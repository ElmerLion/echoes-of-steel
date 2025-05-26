using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
    
    public static AudioManager Instance { get; private set; }

    public enum Sound {
        PlayerFootstep,
        HeardPlayer,
        WallMoved,
        RitualProgress,
        AnimatronicFootstep,
        AnimatronicWakeUp,
        PlayerSpotted,
        PlayerHeard,
        AnimatronicPeek,
        PlayerPickedUpItem,
        PlayerReadNote,
        PlayerBreathing,
        PlayerSprinting,
        Jumpscare,
        JumpscareBackground,
        PlayerRespawn,
        AncientMineOpened,
        MetallicBang,
        WaterDrop,
        GravelFalling,
        DroppingMetalObject,
        RocksExploding,
        FlashlightToggle,
        SwitchPower,
        LightsFlickerOn,
        Earthquake,
        SupernaturalDamaged,
        FailedEscape,
        Taser,
    }

    public enum Music {
        FactoryBackgroundMusic,
        ChasingMusic,
        MainMenuMusic,
        SuspenseMusic,
        MineBackgroundMusic,
        EscapeMusic,
    }

    public enum AudioType {
        Master,
        Music,
        SFX,
        Environment,
        Jumpscare
    }

    [System.Serializable]
    public class SoundAudioClip {
        public AudioType audioType;
        public Sound sound;
        public bool emitSoundEvent;
        public bool repeat;
        public float emitterHearingDistance;
        public float audioPlayDistance;
        public List<AudioClip> audioClip;
    }

    [System.Serializable]
    public class MusicAudioClip {
        public Music music;
        public List<AudioClip> audioClip;
    }

    [System.Serializable]
    public class RandomIntervalAudio {
        public SoundAudioClip soundAudioClip;
        public float minInterval;
        public float maxInterval;
        public float timer;
        public float spawnDistanceToPlayer;
        public float volumeMultiplier = 1f;
        public List<PlayerLocation> activeDuringLocation;
    }

    [Header("Sound Effects")]
    public List<SoundAudioClip> soundAudioClipList;

    [Header("Music Tracks")]
    public List<MusicAudioClip> musicAudioClipList;

    [Header("Random Interval Audio")]
    public List<RandomIntervalAudio> randomIntervalAudioList;

    private Dictionary<Sound, List<AudioClip>> soundAudioClips;
    private Dictionary<Music, List<AudioClip>> musicAudioClips;
    private Dictionary<AudioType, float> audioTypeVolume;

    private AudioSource musicAudioSource;
    private AudioSource generatorAudioSource;
    private Music currentMusic;
    private int currentTrackIndex = 0;

    private void Awake() {
        Instance = this;

        DontDestroyOnLoad(this);

        soundAudioClips = new Dictionary<Sound, List<AudioClip>>();
        musicAudioClips = new Dictionary<Music, List<AudioClip>>();
        audioTypeVolume = new Dictionary<AudioType, float>();

        foreach (SoundAudioClip soundAudioClip in soundAudioClipList) {
            soundAudioClips[soundAudioClip.sound] = soundAudioClip.audioClip;
        }

        foreach (MusicAudioClip musicAudioClip in musicAudioClipList) {
            musicAudioClips[musicAudioClip.music] = musicAudioClip.audioClip;
        }

        audioTypeVolume[AudioType.Master] = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        audioTypeVolume[AudioType.Music] = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        audioTypeVolume[AudioType.SFX] = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        audioTypeVolume[AudioType.Environment] = PlayerPrefs.GetFloat("EnvironmentVolume", 0.5f);
        audioTypeVolume[AudioType.Jumpscare] = PlayerPrefs.GetFloat("JumpscareVolume", 0.5f);

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        musicAudioSource.loop = true;
        SetVolume(audioTypeVolume[AudioType.Music], AudioType.Music);
    }

    private void Start() {
        if (HorrorGameManager.Instance != null) {
            HorrorGameManager.Instance.OnPlayerLocationChanged += HorrorGameManager_OnPlayerLocationChanged;
        }
    }

    private void HorrorGameManager_OnPlayerLocationChanged(PlayerLocation obj) {
        CheckMusic();
    }

    public void ResetMusic() {
        PlayMusic(Music.FactoryBackgroundMusic);
        CheckMusic();
    }

    public void CheckMusic() {
        if (currentMusic == Music.EscapeMusic) return;
        PlayerLocation location = HorrorGameManager.Instance.CurrentPlayerLocation;

        if (location == PlayerLocation.FactoryZone) {
            PlayMusic(Music.FactoryBackgroundMusic);
        } else if (location == PlayerLocation.MineZone) {
            PlayMusic(Music.MineBackgroundMusic);
        }
    }

    private void Update() {
        if (!musicAudioSource.isPlaying) {
            PlayNextTrack();
        }

        PlayRandomIntervalAudio();
    }

    public void PlayRandomIntervalAudio() {
        if (AnimatronicManager.Instance == null) return;

        foreach (RandomIntervalAudio randomIntervalAudio in randomIntervalAudioList) {
            if (!randomIntervalAudio.activeDuringLocation.Contains(HorrorGameManager.Instance.CurrentPlayerLocation)) {
                continue;
            }

            randomIntervalAudio.timer -= Time.deltaTime;

            if (randomIntervalAudio.timer <= 0f) {
                randomIntervalAudio.timer = Random.Range(randomIntervalAudio.minInterval, randomIntervalAudio.maxInterval);

                Vector3 playerPos = Camera.main.transform.position;
                Vector3 spawnPos = new Vector3(playerPos.x + randomIntervalAudio.spawnDistanceToPlayer, playerPos.y, playerPos.z + randomIntervalAudio.spawnDistanceToPlayer);
                PlaySound(randomIntervalAudio.soundAudioClip, spawnPos, randomIntervalAudio.volumeMultiplier);
            }
        }
    }
    public void PlaySound(Sound sound, float volumeMultiplier = 1f) {
        PlaySound(sound, Camera.main.transform.position, volumeMultiplier);
    }

    public void PlaySound(Sound sound, Vector3 position, float volumeMultiplier = 1f) {
        if (soundAudioClips.ContainsKey(sound)) {
            SoundAudioClip soundAudioClip = soundAudioClipList.Find(x => x.sound == sound);
            PlaySound(soundAudioClip, position, volumeMultiplier);
        } else {
            Debug.LogWarning("Sound " + sound + " not found!");
        }
    }

    public void PlaySound(SoundAudioClip soundAudioClip, Vector3 position, float volumeMultiplier = 1f) {
        float specificVolume = audioTypeVolume[soundAudioClip.audioType];
        float masterVolume = audioTypeVolume[AudioType.Master];
        float finalVolume = (specificVolume * masterVolume) * volumeMultiplier;

        if (finalVolume > 0f) {
            PlayAudioClip(soundAudioClip, position, finalVolume);

            if (soundAudioClip.emitSoundEvent) {
                SoundManager.Instance.EmitSound(position, 1f, soundAudioClip.emitterHearingDistance);
            }
        }
    }

    public void AssignGenerator(AudioSource audioSource) {
        generatorAudioSource = audioSource;
    }

    public void PlayGeneratorSound() {
        generatorAudioSource.volume = audioTypeVolume[AudioType.Environment] * audioTypeVolume[AudioType.Master];
        generatorAudioSource.Play();
        generatorAudioSource.loop = true;
    }

    public void StopGeneratorSound() {
        generatorAudioSource.Stop();
    }

    public void PlayMusic(Music music) {
        if (music == currentMusic && musicAudioSource.isPlaying) {
            return;
        }

        if (musicAudioClips.ContainsKey(music)) {
            currentMusic = music;
            currentTrackIndex = Random.Range(0, musicAudioClips[music].Count);
            PlayCurrentTrack();
        } else {
            Debug.LogWarning("Music " + music + " not found!");
        }
    }

    private void PlayCurrentTrack() {
        if (musicAudioClips.ContainsKey(currentMusic)) {
            AudioClip clip = musicAudioClips[currentMusic][currentTrackIndex];
            musicAudioSource.clip = clip;

            musicAudioSource.volume = audioTypeVolume[AudioType.Music] * audioTypeVolume[AudioType.Master];
            musicAudioSource.Play();
        }
    }

    private void PlayNextTrack() {
        currentTrackIndex++;
        if (currentTrackIndex >= musicAudioClips[currentMusic].Count) {
            currentTrackIndex = 0; 
        }
        PlayCurrentTrack();
    }

    public void StopMusic() {
        musicAudioSource.Stop();
    }

    public void SetVolume(float volume, AudioType audioType) {
        if (audioTypeVolume.ContainsKey(audioType)) {
            audioTypeVolume[audioType] = volume;
        } else {
            Debug.LogWarning("AudioType " + audioType + " not found!");
            return;
        }

        if (audioType == AudioType.Music || audioType == AudioType.Master) {
            musicAudioSource.volume = audioTypeVolume[AudioType.Music] * audioTypeVolume[AudioType.Master];
        }
    }

    private void PlayAudioClip(SoundAudioClip soundAudioClip, Vector3 position, float volume) {
        AudioClip clip = soundAudioClip.audioClip[Random.Range(0, soundAudioClip.audioClip.Count)];

        GameObject tempAudioSource = new GameObject("TempAudio_" + clip.name);
        tempAudioSource.transform.position = position;

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;

        audioSource.spatialBlend = 1.0f;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = soundAudioClip.audioPlayDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear; 

        audioSource.Play();
        if (!soundAudioClip.repeat) {
            Destroy(tempAudioSource, clip.length);
        } else {
            audioSource.loop = true;
        }

    }


    private void OnApplicationQuit() {
        PlayerPrefs.SetFloat("MasterVolume", audioTypeVolume[AudioType.Master]);
        PlayerPrefs.SetFloat("MusicVolume", audioTypeVolume[AudioType.Music]);
        PlayerPrefs.SetFloat("SFXVolume", audioTypeVolume[AudioType.SFX]);
        PlayerPrefs.SetFloat("EnvironmentVolume", audioTypeVolume[AudioType.Environment]);
        PlayerPrefs.SetFloat("JumpscareVolume", audioTypeVolume[AudioType.Jumpscare]);
    }

}
