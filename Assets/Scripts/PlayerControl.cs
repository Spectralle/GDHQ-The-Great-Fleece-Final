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
        [SerializeField] private Transform _coinOrigin;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private LineRenderer _coinTossLineRenderer;
        [SerializeField, Range(0, 100)] private int _coinCount = 1;
        [SerializeField, Range(1f, 40f)] private float _coinDistractRange = 5f;
        [SerializeField, Range(2, 40)] private int _arcResolution = 10;
        [SerializeField, Range(1, 10)] private int _arcHeight = 5;
        [SerializeField] private LayerMask _validCoinArea;
        [SerializeField] private LayerMask _distractTargets;
        [SerializeField] private bool _showArcInGame;
        
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
        private int _coinsTossedAlready = 0;
        Vector3 target;

        [System.Serializable]
        public struct ArcData
        {
            public Vector3 initialVelocity;
            public Vector3[] arcPoints;

            public ArcData(Vector3 initialVelocity, Vector3[] arcPoints)
            {
                this.initialVelocity = initialVelocity;
                this.arcPoints = arcPoints;
            }
        }


        private void Start()
        {
            _cameraTransform = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _gravity = Physics.gravity.y;
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

                if (Input.GetButtonDown("Toss Coin") && _coinsTossedAlready < _coinCount)
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
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 30, _validCoinArea))
            {
                PrepareToTossCoin(hit.point, out AnimState currentState);

                yield return new WaitForSeconds(0.8f);

                ArcData arc = CalculateCoinArcData(_coinOrigin.position, hit.point, out float timeTillReached);

                SpawnAndTossCoin(arc, out GameObject coin, out Rigidbody rb);
                if (_showArcInGame)
                    DisplayArc(arc);

                yield return new WaitForSeconds(1.2f);

                _animState = currentState;
                _agent.isStopped = false;

                yield return new WaitForSeconds(timeTillReached - 1.2f);

                CoinHitsGround(coin.transform, rb, hit.point);
                DistractGuards(coin.transform);
            }
        }

        private ArcData CalculateCoinArcData(Vector3 origin, Vector3 target, out float time)
        {
            Vector3 displacement = target - origin;
            time = Mathf.Sqrt(-2 * _arcHeight / _gravity) + Mathf.Sqrt(2 * (displacement.y - _arcHeight) / _gravity);
            Vector3 initialVelocity = displacement / time;
            initialVelocity.y = Mathf.Sqrt(-2 * _gravity * _arcHeight) * -Mathf.Sign(_gravity);

            Vector3[] arcPoints = new Vector3[_arcResolution + 1];
            arcPoints[0] = origin;
            arcPoints[arcPoints.Length - 1] = target;
            for (int i = 1; i <= _arcResolution; i++)
            {
                float simulationTime = i / (float)_arcResolution * time;
                Vector3 pointDisplacement = initialVelocity * simulationTime + Vector3.up * _gravity * simulationTime * simulationTime / 2f;
                arcPoints[i] = origin + pointDisplacement;
            }

            return new ArcData(initialVelocity, arcPoints);
        }

        private void PrepareToTossCoin(Vector3 hitPoint, out AnimState currentState)
        {
            transform.LookAt(hitPoint);
            _coinsTossedAlready++;
            currentState = _animState;
            _animState = AnimState.Throwing;
            _agent.isStopped = true;
            _anim.Play("Throw");
        }

        private void SpawnAndTossCoin(ArcData arc, out GameObject coin, out Rigidbody rb)
        {
            coin = Instantiate(
                _coinPrefab,
                _coinOrigin.position,
                Quaternion.Euler(Random.Range(-80, 80), Random.Range(0, 360), 0)
            );
            rb = coin.GetComponent<Rigidbody>();
            rb.velocity = arc.initialVelocity;
        }

        private void DisplayArc(ArcData arc)
        {
            _coinTossLineRenderer.positionCount = arc.arcPoints.Length;
            _coinTossLineRenderer.SetPositions(arc.arcPoints);
        }

        private void CoinHitsGround(Transform coin, Rigidbody coinRb, Vector3 hitPoint)
        {
            if (_showArcInGame)
                _coinTossLineRenderer.positionCount = 0;
            coin.transform.rotation = Quaternion.identity;
            coin.position = new Vector3(coin.position.x, -1.98f, coin.position.z);
            coinRb.velocity = Vector3.zero;
            coinRb.constraints = RigidbodyConstraints.FreezeAll;
            Destroy(coin.gameObject, 25f);

            AudioSource _as = coin.GetComponent<AudioSource>();
            _as.pitch = Random.Range(0.9f, 1.1f);
            _as.Play();
        }

        private void DistractGuards(Transform coin)
        {
            Collider[] nearbyGuards = Physics.OverlapSphere(
                coin.position,
                _coinDistractRange,
                _distractTargets
            );
            foreach (Collider guard in nearbyGuards)
            {
                guard.transform.parent.TryGetComponent(out AIDistractionReciever aidr);
                aidr?.Distract(coin);
            }
        }
    }
}
