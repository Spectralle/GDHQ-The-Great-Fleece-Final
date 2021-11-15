using System.Collections;
using UnityEditor;
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

        private AIWaypointPatroller _aiWayPat;
        private NavMeshAgent _agent;
        private Animator _anim;
        private float _alertTimer;

        private Vector3 _originPosition;
        private Vector3 _originRotation;


        private void Awake()
        {
            TryGetComponent(out _aiWayPat);
            TryGetComponent(out _anim);
            _agent = GetComponent<NavMeshAgent>();

            _originPosition = transform.position;
            _originRotation = transform.forward;
        }

        private void Update()
        {
            if (!_aiWayPat)
            {
                if (!_isDistracted && Vector3.Distance(_originRotation, transform.forward) > 0.02f)
                {
                    Quaternion rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, _originRotation, 2f * Time.deltaTime, 0.0f));
                    rotation.x = 0;
                    rotation.z = 0;
                    transform.rotation = rotation;
                }
            }
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
            #region Pause normal behaviour
            _aiWayPat?.Distract();
            _agent.SetDestination(distractionOrigin.position);
            _anim?.SetBool("Walking", true);
            #endregion

            #region Walking to/reached distraction
            while (Vector3.Distance(transform.position, _agent.destination) > 0.5f)
                yield return null;

            _anim?.SetBool("Walking", false);
            _anim?.SetBool("Alert", true);
            #endregion

            #region Alert at distraction
            _alertTimer = 0;
            while (_alertTimer < distractionTime)
            {
                _alertTimer += Time.deltaTime;
                yield return null;
            }
            #endregion

            #region Return to normal behaviour
            _anim?.SetBool("Alert", false);
            _anim?.SetBool("Walking", true);

            if (!_aiWayPat)
            {
                _agent.SetDestination(_originPosition);

                while (Vector3.Distance(transform.position, _originPosition) > 0.2f)
                    yield return null;

                _anim?.SetBool("Walking", false);
            }

            _aiWayPat?.RegainFocus();
            _isDistracted = false;
            #endregion
        }
    }
}
