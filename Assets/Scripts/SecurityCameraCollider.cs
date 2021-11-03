using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SecurityCameraCollider : MonoBehaviour
{
    [SerializeField] private UnityEvent[] _events;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (UnityEvent e in _events)
                e.Invoke();
        }
    }
}
