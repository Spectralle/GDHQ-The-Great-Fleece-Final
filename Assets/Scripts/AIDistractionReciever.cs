using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace TGF
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AIDistractionReciever : MonoBehaviour
    {
        private bool _isDistracted;
        public bool IsDistracted => _isDistracted;
        [SerializeField, Min(1)] private float _defaultDistractionTime = 6f;

        private AIWaypointPatroller _aiwp;
        private NavMeshAgent _ainm;
        private Animator _anim;
        private float _alertTimer;


        private void Awake()
        {
            TryGetComponent(out _aiwp);
            TryGetComponent(out _anim);
            _ainm = GetComponent<NavMeshAgent>();
        }

        public void Distract(Transform distractionOrigin, float distractionTime = -1)
        {
            if (!_isDistracted)
            {
                _isDistracted = true;
                if (distractionTime != -1 && distractionTime > 0)
                    StartCoroutine(GuardIsDistracted(distractionOrigin, distractionTime));
                else
                    StartCoroutine(GuardIsDistracted(distractionOrigin, _defaultDistractionTime));
            }
        }

        private IEnumerator GuardIsDistracted(Transform distractionOrigin, float distractionTime)
        {
            bool wasPathfindingEnabled = _aiwp && _aiwp.IsPathfinding;
            _aiwp?.DisablePathfinding();
            _aiwp?.SetIsDistracted(true);
            _ainm.SetDestination(distractionOrigin.position);
            _anim?.SetBool("Walking", true);

            while (Vector3.Distance(transform.position, _ainm.destination) > 0.5f)
                yield return null;

            _anim?.SetBool("Alert", true);
            _anim?.SetBool("Walking", false);

            while (_alertTimer < distractionTime)
            {
                _alertTimer += Time.deltaTime;
                yield return null;
            }

            _anim?.SetBool("Alert", false);
            _anim?.SetBool("Walking", true);

            if (wasPathfindingEnabled)
                _aiwp?.EnablePathfinding();
            _aiwp?.SetIsDistracted(false);
            _aiwp?.AfterDistraction();
        }
    }
}
