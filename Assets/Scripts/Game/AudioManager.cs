using UnityEngine;

namespace TGF
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _uiSource;
        [SerializeField] private AudioSource _voiceOverSource;


        private void Awake()
        {
            if (!_musicSource || !_sfxSource || !_uiSource || !_voiceOverSource)
                Debug.LogError($"ERROR: Missing AudioSource reference: " +
                    $"Has music = {_musicSource != null}. " +
                    $"Has sfx = {_sfxSource != null}. " +
                    $"Has ui = {_uiSource != null}. " +
                    $"Has voiceover = {_voiceOverSource != null}.");
        }

        #region Source Variants
        public void PlayMusicAudio(AudioClip clip) => PlayAudio(clip, _musicSource);
        public void PlayMusicAudioOneShot(AudioClip clip) => PlayAudioOneShot(clip, _musicSource);
        public void StopMusicAudio() => StopAudio(_musicSource);

        public void PlaySFXAudio(AudioClip clip) => PlayAudio(clip, _sfxSource);
        public void PlaySFXAudioOneShot(AudioClip clip) => PlayAudioOneShot(clip, _sfxSource);
        public void StopSFXAudio() => StopAudio(_sfxSource);

        public void PlayUIAudio(AudioClip clip) => PlayAudio(clip, _uiSource);
        public void PlayUIAudioOneShot(AudioClip clip) => PlayAudioOneShot(clip, _uiSource);
        public void StopUIAudio() => StopAudio(_uiSource);

        public void PlayVoiceOverAudio(AudioClip clip) => PlayAudio(clip, _voiceOverSource);
        public void PlayVoiceOverAudioOneShot(AudioClip clip) => PlayAudioOneShot(clip, _voiceOverSource);
        public void StopVoiceOverAudio() => StopAudio(_voiceOverSource);
        #endregion

        public void PlayAudio(AudioClip clip, AudioSource audioSource)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        public void PlayAudioOneShot(AudioClip clip, AudioSource audioSource) => audioSource.PlayOneShot(clip);
        public void StopAudio(AudioSource audioSource) => audioSource.Stop();

        public void PlayAudioAtPoint(AudioClip clip, Vector3 audioPoint) => AudioSource.PlayClipAtPoint(clip, audioPoint);
    } 
}
