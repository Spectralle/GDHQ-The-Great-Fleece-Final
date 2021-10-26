using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace TGF
{
    [RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
    public class PlayerControl : MonoBehaviour
    {
        [Header("Click Movement:")]
        [SerializeField] private LayerMask _moveArea;
        [Header("Coin Toss Ability:")]
        [SerializeField, Min(0)] private int _coinCount = 1;
        [SerializeField] private LayerMask _coinArea;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private float _coinDistractRange = 5f;
        [SerializeField] private LayerMask _distractTargets;
        [SerializeField] private LineRenderer _coinTossLineRenderer;
        [SerializeField] private float _coinVelocity;
        [SerializeField] private float _coinAngle;
        [SerializeField, Min(2)] private int _coinResolution = 10;

        private Camera _cameraTransform;
        private Vector3 _worldDestination;
        private NavMeshAgent _agent;
        private Animator _anim;
        private enum AnimState
        {
            Idle,
            Walking,
            Throwing
        }
        private AnimState _animState;
        private float _gravity;
        private float _radianAngle;
        private int _coinsTossed = 0;


        private void Start()
        {
            _cameraTransform = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _gravity = Mathf.Abs(Physics.gravity.y);
            _coinTossLineRenderer.positionCount = 0;
        }

        private void Update()
        {
            if (_animState != AnimState.Throwing)
            {
                #region State checks
                if (_agent.velocity != Vector3.zero)
                {
                    _animState = AnimState.Walking;
                    _anim.SetBool("Walking", true);
                }
                else
                {
                    _animState = AnimState.Idle;
                    _anim.SetBool("Walking", false);
                }
                #endregion

                #region Player Input
                if (Input.GetButtonDown("Move"))
                    SetDestinationToClickPosition();

                if (Input.GetButtonDown("Toss Coin") && _coinsTossed < _coinCount)
                    StartCoroutine(TossCoin());
                #endregion
            }
        }

        private void SetDestinationToClickPosition()
        {
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _moveArea))
            {
                _worldDestination = hit.point;
                _agent.SetDestination(_worldDestination);
            }
        }

        private IEnumerator TossCoin()
        {
            _coinsTossed++;
            AnimState currentState = _animState;
            _animState = AnimState.Throwing;
            _agent.isStopped = true;

            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _coinArea))
                transform.LookAt(hit.point);

            _anim.SetTrigger("Throw coin");

            //RenderCoinArc();

            yield return new WaitForSeconds(1.5f);

            GameObject coin = Instantiate(
                _coinPrefab,
                hit.point + new Vector3(0, 0.5f, 0),
                Quaternion.Euler(Random.Range(-80, 80), Random.Range(0, 360), 0)
            );
            coin.GetComponent<AudioSource>().Play();
            Destroy(coin, 5f);

            Collider[] nearbyGuards = Physics.OverlapSphere(coin.transform.position, _coinDistractRange, _distractTargets);
            Debug.Log("Distracted " + nearbyGuards.Length + " guards.");
            foreach (Collider guard in nearbyGuards)
            {
                guard.transform.parent.TryGetComponent(out AIDistractionReciever aidr);
                aidr?.Distract(coin.transform.position);
            }


            yield return new WaitForSeconds(0.5f);

            _animState = currentState;
            _agent.isStopped = false;
        }

        private void RenderCoinArc()
        {
            _coinTossLineRenderer.positionCount = _coinResolution + 1;
            _coinTossLineRenderer.SetPositions(CalculateArcArray());
        }

        private Vector3[] CalculateArcArray()
        {
            Vector3[] arcArray = new Vector3[_coinResolution + 1];

            _radianAngle = Mathf.Deg2Rad * _coinAngle;
            float maxDistance = (_coinVelocity * _coinVelocity * Mathf.Sin(2 * _radianAngle)) / _gravity;

            for (int i = 0; i <= _coinResolution; i++)
            {
                float t = (float)i / (float)_coinResolution;
                arcArray[i] = CalculateArcPoint(t, maxDistance);
            }

            return arcArray;
        }

        private Vector3 CalculateArcPoint(float t, float maxDistance)
        {
            float X = t * maxDistance;
            float Y = X * Mathf.Tan(_radianAngle) - ((_gravity * X * X) / (2 * _coinVelocity * _coinVelocity * Mathf.Cos(_radianAngle) * Mathf.Cos(_radianAngle)));
            //float Z = ;
            return new Vector3(0, Y, X);
        }
    }
}
