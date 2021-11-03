using System.Collections.Generic;
using UnityEngine;

namespace TGF
{
    [RequireComponent(typeof(Collider))]
    public class AudioTrigger : MonoBehaviour
    {
        [SerializeField] private AudioSource _camera;
        [SerializeField] private List<string> _activationTags;
        [Space]
        [SerializeField] private AudioClip[] _playOnEnter;
        [SerializeField] private AudioClip[] _playOnExit;


        private void OnTriggerEnter(Collider other) => PlayAnyClips(other.tag, _playOnEnter);

        private void OnTriggerExit(Collider other) => PlayAnyClips(other.tag, _playOnExit);

        private void PlayAnyClips(string tag, AudioClip[] clipList)
        {
            if (_activationTags.Contains(tag))
            {
                foreach (AudioClip clip in clipList)
                {
                    _camera.PlayOneShot(clip);
                    Debug.Log("Audio clip (" + clip.name + ") is now playing for " + clip.length + " seconds!");
                }
            }
        }
    } 
}
