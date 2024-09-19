using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.Core.Brain
{
    [System.Serializable]
    public class AudioObjectInfo
    {
        private AudioClassificationType _type = AudioClassificationType.Master;
        private AudioSource _source;
        private float _clipVolumeLevel = 1f;
        public MonaBrainAudio BrainAudioInstance;

        public AudioClassificationType Type { get => _type; set => _type = value; }
        public AudioSource Source { get => _source; set => _source = value; }

        public float AssignedVolume => BaseVolumeLevel * ClipVolumeLevel;
        public float ClipVolumeLevel { get => _clipVolumeLevel; set => _clipVolumeLevel = Mathf.Clamp01(value); }
        public float BaseVolumeLevel => BrainAudioInstance.GetVolumeLevel(_type);
        public bool IsPlaying => _source != null && _source.isActiveAndEnabled && _source.isPlaying;

        public void UpdateVolume()
        {
            if (BrainAudioInstance == null || _source == null)
                return;

            _source.volume = AssignedVolume;
        }

        public void UpdateVolume(float newClipVolume)
        {
            _clipVolumeLevel = newClipVolume;
            UpdateVolume();
        }

        public void StopPlayback()
        {
            if (IsPlaying)
                _source.Stop();
        }
    }

    public class AudioClassVolumeLevel
    {
        private float _volume = 1f;
        private float _defaultVolume = 1f;
        private bool _muted = false;

        public float CurrentVolume => _muted ? 0f : _volume;
        public float Volume { get => _volume; set => _volume = Mathf.Clamp01(value); }
        public float DefaultVolume { get => _defaultVolume; set => _defaultVolume = Mathf.Clamp01(value); }
        public bool Muted { get => _muted; set => _muted = value; }

        public void ToggleMute() { _muted = !_muted; }
        public void SetToDefault() { _volume = _defaultVolume; }
    }

    public class MonaBrainAudio : MonoBehaviour
    {
        private static MonaBrainAudio _instance;

        private AudioClassVolumeLevel MasterAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel SFXAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel VoxAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel MusicAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel AmbienceAudio = new AudioClassVolumeLevel();

        private List<AudioObjectInfo> audioObjects = new List<AudioObjectInfo>();

        public float MasterBaseVolume => MasterAudio.CurrentVolume;
        public float SFXBaseVolume => SFXAudio.CurrentVolume;
        public float VoxBaseVolume => VoxAudio.CurrentVolume;
        public float MusicBaseVolume => MusicAudio.CurrentVolume;
        public float AmbienceBaseVolume => AmbienceAudio.CurrentVolume;

        public static MonaBrainAudio Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public float GetVolumeLevel(AudioClassificationType type)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect:
                    return MasterAudio.CurrentVolume * SFXAudio.CurrentVolume;
                case AudioClassificationType.Voice:
                    return MasterAudio.CurrentVolume * VoxAudio.CurrentVolume;
                case AudioClassificationType.Music:
                    return MasterAudio.CurrentVolume * MusicAudio.CurrentVolume;
                case AudioClassificationType.Ambience:
                    return MasterAudio.CurrentVolume * AmbienceAudio.CurrentVolume;
                default:
                    return MasterAudio.CurrentVolume;
            }
        }

        public float GetUnmodifiedVolumeLevel(AudioClassificationType type)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect:
                    return SFXAudio.Volume;
                case AudioClassificationType.Voice:
                    return VoxAudio.Volume;
                case AudioClassificationType.Music:
                    return MusicAudio.Volume;
                case AudioClassificationType.Ambience:
                    return AmbienceAudio.Volume;
                default:
                    return MasterAudio.Volume;
            }
        }

        public void SetVolumeLevel(AudioClassificationType type, float newVolumeLevel)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Voice: VoxAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Music: MusicAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Ambience: AmbienceAudio.Volume = newVolumeLevel; break;
                default: MasterAudio.Volume = newVolumeLevel; break;
            }

            UpdateAudioObjectVolumes(type);
        }

        public void AdjustVolumeLevelByStep(AudioClassificationType type, float step)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.Volume += step; break;
                case AudioClassificationType.Voice: VoxAudio.Volume += step; break;
                case AudioClassificationType.Music: MusicAudio.Volume += step; break;
                case AudioClassificationType.Ambience: AmbienceAudio.Volume += step; break;
                default: MasterAudio.Volume += step; break;
            }

            UpdateAudioObjectVolumes(type);
        }

        public bool GetVolumeMutedState(AudioClassificationType type)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: return SFXAudio.Muted;
                case AudioClassificationType.Voice: return VoxAudio.Muted;
                case AudioClassificationType.Music: return MusicAudio.Muted;
                case AudioClassificationType.Ambience: return AmbienceAudio.Muted;
                default: return MasterAudio.Muted;
            }
        }

        public void SetVolumeMuteState(AudioClassificationType type, bool muted)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.Muted = muted; break;
                case AudioClassificationType.Voice: VoxAudio.Muted = muted; break;
                case AudioClassificationType.Music: MusicAudio.Muted = muted; break;
                case AudioClassificationType.Ambience: AmbienceAudio.Muted = muted; break;
                default: MasterAudio.Muted = muted; break;
            }

            UpdateAudioObjectVolumes(type);
        }

        public void ToggleVolumeMuteState(AudioClassificationType type)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.ToggleMute(); break;
                case AudioClassificationType.Voice: VoxAudio.ToggleMute(); break;
                case AudioClassificationType.Music: MusicAudio.ToggleMute(); break;
                case AudioClassificationType.Ambience: AmbienceAudio.ToggleMute(); break;
                default: MasterAudio.ToggleMute(); break;
            }

            UpdateAudioObjectVolumes(type);
        }

        private void UpdateAudioObjectVolumes(AudioClassificationType type)
        {
            if (type == AudioClassificationType.Master)
            {
                for (int i = 0; i < audioObjects.Count; i++)
                {
                    if (audioObjects[i] == null)
                        continue;

                    audioObjects[i].UpdateVolume();
                }
            }
            else
            {
                for (int i = 0; i < audioObjects.Count; i++)
                {
                    if (audioObjects[i].Type != type)
                        continue;

                    audioObjects[i].UpdateVolume();
                }
            }
        }

        public void AddAudioObject (AudioObjectInfo audioObject)
        {
            if (audioObject != null && audioObject.Source != null)
                audioObjects.Add(audioObject);
        }

        public AudioObjectInfo CreateNewAudioObject(AudioSource source, AudioClassificationType type, float clipVolume)
        {
            if (source == null)
                return null;

            AudioObjectInfo newAudioObject = new AudioObjectInfo
            {
                Type = type,
                Source = source,
                ClipVolumeLevel = clipVolume,
                BrainAudioInstance = _instance
            };

            audioObjects.Add(newAudioObject);
            return newAudioObject;
        }

        public bool HasAudioObject(AudioObjectInfo audioObject)
        {
            if (audioObject == null)
                return false;

            return audioObjects.Contains(audioObject);
        }

        public bool HasAudioObject(AudioSource source)
        {
            return GetAudioObject(source) != null;
        }

        public AudioObjectInfo GetAudioObject(AudioSource source)
        {
            if (source == null)
                return null;

            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i].Source == source)
                    return audioObjects[i];
            }

            return null;
        }

        public AudioObjectInfo GetAudioObject(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i].Source != null && audioObjects[i].Source.gameObject == gameObject)
                    return audioObjects[i];
            }

            return null;
        }

        public void RemoveAudioObject(AudioObjectInfo audioObject)
        {
            if (audioObject == null)
                return;

            audioObjects.Remove(audioObject);
        }

        public void StopAudioPlayback()
        {
            StopAudioPlayback(AudioClassificationType.Master);
            StopAudioPlayback(AudioClassificationType.SoundEffect);
            StopAudioPlayback(AudioClassificationType.Voice);
            StopAudioPlayback(AudioClassificationType.Music);
            StopAudioPlayback(AudioClassificationType.Ambience);
        }

        public void StopAudioPlayback(AudioClassificationType type)
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i].Type != type)
                    continue;

                audioObjects[i].StopPlayback();
            }
        }
    }
}
