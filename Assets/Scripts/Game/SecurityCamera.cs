using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TGF
{
    [RequireComponent(typeof(Collider))]
    public class SecurityCamera : MonoBehaviour
    {
        [SerializeField] private UnityEvent _enterEvents;
        [SerializeField] private bool _enableExitEvents;
        [SerializeField] private UnityEvent _exitEvents;


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _enterEvents.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (_enableExitEvents && other.CompareTag("Player"))
            {
                StopAllCoroutines();
                _exitEvents.Invoke();
            }
        }

        public void StartCutscene(GameObject obj) => StartCoroutine(StartCS(obj));

        private IEnumerator StartCS(GameObject obj)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            obj.SetActive(true);
            GameObject.FindGameObjectWithTag("Player").SetActive(false);
        }
    } 
}
