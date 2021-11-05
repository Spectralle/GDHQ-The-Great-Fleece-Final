using System.Collections.Generic;
using UnityEngine;

namespace TGF
{
    [RequireComponent(typeof(Collider))]
    public class AudioTrigger : MonoBehaviour
    {
        [SerializeField] private AudioSource _camera;
        [SerializeField] private bool _playOnce;
        [SerializeField] private List<string> _activationTags;
        [Space]
        [SerializeField] private AudioClip[] _playOnEnter;
        [SerializeField] private AudioClip[] _playOnExit;

        private bool _hasBeenPlayed;


        private void OnTriggerEnter(Collider other) => PlayAnyClips(other.tag, _playOnEnter);

        private void OnTriggerExit(Collider other) => PlayAnyClips(other.tag, _playOnExit);

        private void PlayAnyClips(string tag, AudioClip[] clipList)
        {
            if (_activationTags.Contains(tag))
            {
                if ((_playOnce && !_hasBeenPlayed) || !_playOnce)
                {
                    _hasBeenPlayed = true;
                    foreach (AudioClip clip in clipList)
                        _camera.PlayOneShot(clip);
                }
            }
        }
    } 
}
