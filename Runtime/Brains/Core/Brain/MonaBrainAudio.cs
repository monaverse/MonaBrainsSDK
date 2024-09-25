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
        private bool _pausable = true;
        private bool _paused = false;
        public MonaBrainAudio BrainAudioInstance;

        private bool CanPause => Pausable && IsPlaying;
        public bool IsPlaying => _source != null && _source.isActiveAndEnabled && _source.isPlaying;
        public float AssignedVolume => BaseVolumeLevel * ClipVolumeLevel;
        public float BaseVolumeLevel => BrainAudioInstance.GetVolumeLevel(_type);

        public float ClipVolumeLevel { get => _clipVolumeLevel; set => _clipVolumeLevel = Mathf.Clamp01(value); }
        public bool Pausable { get => _pausable; set => _pausable = value; }
        public AudioClassificationType Type { get => _type; set => _type = value; }
        public AudioSource Source { get => _source; set => _source = value; }

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

        public void StopPlayback(AudioClassificationType checkType)
        {
            if (checkType == _type)
                StopPlayback();
        }

        public void PausePlayback()
        {
            if (!CanPause || _paused)
                return;

            _source.Pause();
            _paused = true;
        }

        public void PausePlayback(AudioClassificationType checkType)
        {
            if (checkType == _type)
                PausePlayback();
        }

        public void UnPausePlayback()
        {
            if (_source == null || !_paused)
                return;

            _source.UnPause();
            _paused = false;
        }

        public void UnPausePlayback(AudioClassificationType checkType)
        {
            if (checkType == _type)
                UnPausePlayback();
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
        private AudioClassVolumeLevel UIAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel CutsceneAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel MicAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel ChatAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel NotificationAudio = new AudioClassVolumeLevel();
        private AudioClassVolumeLevel MiscAudio = new AudioClassVolumeLevel();

        private List<AudioObjectInfo> audioObjects = new List<AudioObjectInfo>();

        public float MasterBaseVolume => MasterAudio.CurrentVolume;
        public float SFXBaseVolume => SFXAudio.CurrentVolume;
        public float VoxBaseVolume => VoxAudio.CurrentVolume;
        public float MusicBaseVolume => MusicAudio.CurrentVolume;
        public float AmbienceBaseVolume => AmbienceAudio.CurrentVolume;
        public float UIBaseVolume => UIAudio.CurrentVolume;
        public float CutsceneBaseVolume => CutsceneAudio.CurrentVolume;
        public float MicBaseVolume => MicAudio.CurrentVolume;
        public float ChatBaseVolume => ChatAudio.CurrentVolume;
        public float NotificationBaseVolume => NotificationAudio.CurrentVolume;
        public float MiscBaseVolume => MiscAudio.CurrentVolume;

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
                case AudioClassificationType.UserInterface:
                    return MasterAudio.CurrentVolume * UIAudio.CurrentVolume;
                case AudioClassificationType.Cutscene:
                    return MasterAudio.CurrentVolume * CutsceneAudio.CurrentVolume;
                case AudioClassificationType.Microphone:
                    return MasterAudio.CurrentVolume * MicAudio.CurrentVolume;
                case AudioClassificationType.VoiceChat:
                    return MasterAudio.CurrentVolume * ChatAudio.CurrentVolume;
                case AudioClassificationType.Notification:
                    return MasterAudio.CurrentVolume * NotificationAudio.CurrentVolume;
                case AudioClassificationType.Miscellaneous:
                    return MasterAudio.CurrentVolume * MiscAudio.CurrentVolume;
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
                case AudioClassificationType.UserInterface:
                    return UIAudio.Volume;
                case AudioClassificationType.Cutscene:
                    return CutsceneAudio.Volume;
                case AudioClassificationType.Microphone:
                    return MicAudio.Volume;
                case AudioClassificationType.VoiceChat:
                    return ChatAudio.Volume;
                case AudioClassificationType.Notification:
                    return NotificationAudio.Volume;
                case AudioClassificationType.Miscellaneous:
                    return MiscAudio.Volume;
                default:
                    return MasterAudio.Volume;
            }
        }

        public void SetVolumeLevel(AudioClassificationType type, float newVolumeLevel, bool updateAudioObjects = true)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Voice: VoxAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Music: MusicAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Ambience: AmbienceAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.UserInterface: UIAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Cutscene: CutsceneAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Microphone: MicAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.VoiceChat: ChatAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Notification: NotificationAudio.Volume = newVolumeLevel; break;
                case AudioClassificationType.Miscellaneous: MiscAudio.Volume = newVolumeLevel; break;
                default: MasterAudio.Volume = newVolumeLevel; break;
            }

            if (updateAudioObjects)
                UpdateAudioObjectVolumes(type);
        }

        public void SetDefaulVolumeLevel(AudioClassificationType type, float newVolumeLevel)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Voice: VoxAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Music: MusicAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Ambience: AmbienceAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.UserInterface: UIAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Cutscene: CutsceneAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Microphone: MicAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.VoiceChat: ChatAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Notification: NotificationAudio.DefaultVolume = newVolumeLevel; break;
                case AudioClassificationType.Miscellaneous: MiscAudio.DefaultVolume = newVolumeLevel; break;
                default: MasterAudio.DefaultVolume = newVolumeLevel; break;
            }
        }

        public void SetVolumeLevelToDefault(AudioClassificationType type, bool updateAudioObjects = true)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.SetToDefault(); break;
                case AudioClassificationType.Voice: VoxAudio.SetToDefault(); break;
                case AudioClassificationType.Music: MusicAudio.SetToDefault(); break;
                case AudioClassificationType.Ambience: AmbienceAudio.SetToDefault(); break;
                case AudioClassificationType.UserInterface: UIAudio.SetToDefault(); break;
                case AudioClassificationType.Cutscene: CutsceneAudio.SetToDefault(); break;
                case AudioClassificationType.Microphone: MicAudio.SetToDefault(); break;
                case AudioClassificationType.VoiceChat: ChatAudio.SetToDefault(); break;
                case AudioClassificationType.Notification: NotificationAudio.SetToDefault(); break;
                case AudioClassificationType.Miscellaneous: MiscAudio.SetToDefault(); break;
                default: MasterAudio.SetToDefault(); break;
            }

            if (updateAudioObjects)
                UpdateAudioObjectVolumes(type);
        }

        public void SetAllVolumesToDefault()
        {
            SetVolumeLevelToDefault(AudioClassificationType.SoundEffect, false);
            SetVolumeLevelToDefault(AudioClassificationType.Voice, false);
            SetVolumeLevelToDefault(AudioClassificationType.Music, false);
            SetVolumeLevelToDefault(AudioClassificationType.Ambience, false);
            SetVolumeLevelToDefault(AudioClassificationType.UserInterface, false);
            SetVolumeLevelToDefault(AudioClassificationType.Cutscene, false);
            SetVolumeLevelToDefault(AudioClassificationType.Microphone, false);
            SetVolumeLevelToDefault(AudioClassificationType.VoiceChat, false);
            SetVolumeLevelToDefault(AudioClassificationType.Notification, false);
            SetVolumeLevelToDefault(AudioClassificationType.Master, true);
        }

        public void AdjustVolumeLevelByStep(AudioClassificationType type, float step, bool updateAudioObjects = true)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.Volume += step; break;
                case AudioClassificationType.Voice: VoxAudio.Volume += step; break;
                case AudioClassificationType.Music: MusicAudio.Volume += step; break;
                case AudioClassificationType.Ambience: AmbienceAudio.Volume += step; break;
                case AudioClassificationType.UserInterface: UIAudio.Volume += step; break;
                case AudioClassificationType.Cutscene: CutsceneAudio.Volume += step; break;
                case AudioClassificationType.Microphone: MicAudio.Volume += step; break;
                case AudioClassificationType.VoiceChat: ChatAudio.Volume += step; break;
                case AudioClassificationType.Notification: NotificationAudio.Volume += step; break;
                case AudioClassificationType.Miscellaneous: MiscAudio.Volume += step; break;
                default: MasterAudio.Volume += step; break;
            }

            if (updateAudioObjects)
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
                case AudioClassificationType.UserInterface: return UIAudio.Muted;
                case AudioClassificationType.Cutscene: return CutsceneAudio.Muted;
                case AudioClassificationType.Microphone: return MicAudio.Muted;
                case AudioClassificationType.VoiceChat: return ChatAudio.Muted;
                case AudioClassificationType.Notification: return NotificationAudio.Muted;
                case AudioClassificationType.Miscellaneous: return AmbienceAudio.Muted;
                default: return MasterAudio.Muted;
            }
        }

        public void SetVolumeMuteState(AudioClassificationType type, bool muted, bool updateAudioObjects = true)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.Muted = muted; break;
                case AudioClassificationType.Voice: VoxAudio.Muted = muted; break;
                case AudioClassificationType.Music: MusicAudio.Muted = muted; break;
                case AudioClassificationType.Ambience: AmbienceAudio.Muted = muted; break;
                case AudioClassificationType.UserInterface: UIAudio.Muted = muted; break;
                case AudioClassificationType.Cutscene: CutsceneAudio.Muted = muted; break;
                case AudioClassificationType.Microphone: MicAudio.Muted = muted; break;
                case AudioClassificationType.VoiceChat: ChatAudio.Muted = muted; break;
                case AudioClassificationType.Notification: NotificationAudio.Muted = muted; break;
                case AudioClassificationType.Miscellaneous: MiscAudio.Muted = muted; break;
                default: MasterAudio.Muted = muted; break;
            }

            if (updateAudioObjects)
                UpdateAudioObjectVolumes(type);
        }

        public void ToggleVolumeMuteState(AudioClassificationType type, bool updateAudioObjects = true)
        {
            switch (type)
            {
                case AudioClassificationType.SoundEffect: SFXAudio.ToggleMute(); break;
                case AudioClassificationType.Voice: VoxAudio.ToggleMute(); break;
                case AudioClassificationType.Music: MusicAudio.ToggleMute(); break;
                case AudioClassificationType.Ambience: AmbienceAudio.ToggleMute(); break;
                case AudioClassificationType.UserInterface: UIAudio.ToggleMute(); break;
                case AudioClassificationType.Cutscene: CutsceneAudio.ToggleMute(); break;
                case AudioClassificationType.Microphone: MicAudio.ToggleMute(); break;
                case AudioClassificationType.VoiceChat: ChatAudio.ToggleMute(); break;
                case AudioClassificationType.Notification: NotificationAudio.ToggleMute(); break;
                case AudioClassificationType.Miscellaneous: MiscAudio.ToggleMute(); break;
                default: MasterAudio.ToggleMute(); break;
            }

            if (updateAudioObjects)
                UpdateAudioObjectVolumes(type);
        }

        public void MuteAll()
        {
            SetVolumeMuteState(AudioClassificationType.SoundEffect, true, false);
            SetVolumeMuteState(AudioClassificationType.Voice, true, false);
            SetVolumeMuteState(AudioClassificationType.Music, true, false);
            SetVolumeMuteState(AudioClassificationType.Ambience, true, false);
            SetVolumeMuteState(AudioClassificationType.UserInterface, true, false);
            SetVolumeMuteState(AudioClassificationType.Cutscene, true, false);
            SetVolumeMuteState(AudioClassificationType.Microphone, true, false);
            SetVolumeMuteState(AudioClassificationType.VoiceChat, true, false);
            SetVolumeMuteState(AudioClassificationType.Notification, true, false);
            SetVolumeMuteState(AudioClassificationType.Miscellaneous, true, false);
            SetVolumeMuteState(AudioClassificationType.Master, true, true);
        }

        public void UnmuteAll()
        {
            SetVolumeMuteState(AudioClassificationType.SoundEffect, false, false);
            SetVolumeMuteState(AudioClassificationType.Voice, false, false);
            SetVolumeMuteState(AudioClassificationType.Music, false, false);
            SetVolumeMuteState(AudioClassificationType.Ambience, false, false);
            SetVolumeMuteState(AudioClassificationType.UserInterface, false, false);
            SetVolumeMuteState(AudioClassificationType.Cutscene, false, false);
            SetVolumeMuteState(AudioClassificationType.Microphone, false, false);
            SetVolumeMuteState(AudioClassificationType.VoiceChat, false, false);
            SetVolumeMuteState(AudioClassificationType.Notification, false, false);
            SetVolumeMuteState(AudioClassificationType.Miscellaneous, false, false);
            SetVolumeMuteState(AudioClassificationType.Master, false, true);
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

        public void PauseAudio()
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i] != null)
                    audioObjects[i].PausePlayback();
            }
        }

        public void PauseAudio(AudioClassificationType type)
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i] != null)
                    audioObjects[i].PausePlayback(type);
            }
        }

        public void UnPauseAudio()
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i] != null)
                    audioObjects[i].UnPausePlayback();
            }
        }

        public void UnPauseAudio(AudioClassificationType type)
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i] != null)
                    audioObjects[i].UnPausePlayback(type);
            }
        }

        public void StopAudioPlayback()
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i] != null)
                    audioObjects[i].StopPlayback();
            }
        }

        public void StopAudioPlayback(AudioClassificationType type)
        {
            for (int i = 0; i < audioObjects.Count; i++)
            {
                if (audioObjects[i] != null)
                    audioObjects[i].StopPlayback(type);
            }
        }
    }
}
