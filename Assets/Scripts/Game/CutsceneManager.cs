using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TGF
{
    public class CutsceneManager : MonoBehaviour
    {
        [Header("On Enter Sleeping Guard Zone")]
        [SerializeField] private UnityEvent _onSGCutscene;
        [Header("On Enter Win Zone")]
        [SerializeField] private UnityEvent _onWinCutscene_Ready;
        [SerializeField] private UnityEvent _onWinCutscene_NotReady;


        public void TryToRunSGCutscene() => _onSGCutscene.Invoke();

        public void TryToRunWinCutscene()
        {
            if (GameManager.Instance.HasGuardsKeycard)
                _onWinCutscene_Ready.Invoke();
            else
                _onWinCutscene_NotReady.Invoke();
        }
    }
}
