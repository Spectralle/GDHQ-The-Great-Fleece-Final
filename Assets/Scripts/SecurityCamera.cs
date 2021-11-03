using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    private Animator _anim;


    public void StopCameraMovement()
    {
        if (!_anim)
            _anim = GetComponent<Animator>();

        _anim.StopPlayback();
    }
}
