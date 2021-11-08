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
        [SerializeField] private Transform _destinationMark;
        [Header("Coin Toss Ability:")]
        [SerializeField] private Transform _coinOrigin;
        [SerializeField] private GameObject _coinPrefab;
        [SerializeField] private Transform _coinMark;
        [SerializeField, Range(0, 100)] private int _coinCount = 1;
        [SerializeField, Range(1f, 40f)] private float _coinDistractRange = 5f;
        [SerializeField] private LayerMask _validCoinArea;
        [SerializeField] private LayerMask _distractTargets;
        [Header("Coin Arc Visuals:")]
        [SerializeField] private CoinVisualStyle coinVisualStyle;
        private enum CoinVisualStyle { RealCoin, VFXCoin }
        [SerializeField] private LineRenderer _coinTossLineRenderer;
        [SerializeField] private Transform _coinVFXObject;
        [SerializeField, Range(2, 40)] private int _arcResolution = 10;
        [SerializeField, Range(1, 10)] private int _arcHeight = 5;
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
        private LineRenderer _pathLineRenderer;

        [System.Serializable]
        public struct ArcData
        {
            public Vector3 initialVelocity;
            public Vector3[] arcPoints;

            public Vector3 origin;
            public Vector3 displacement;
            public float time;

            public ArcData(Vector3 initialVelocity, Vector3[] arcPoints, Vector3 origin, Vector3 displacement, float flightTime)
            {
                this.initialVelocity = initialVelocity;
                this.arcPoints = arcPoints;
                this.origin = origin;
                this.displacement = displacement;
                this.time = flightTime;
            }
        }


        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _pathLineRenderer = GetComponent<LineRenderer>();
            _gravity = Physics.gravity.y;
            _coinTossLineRenderer.positionCount = 0;
        }

        private void Start() => _cameraTransform = Camera.main;

        private void Update()
        {
            if (_animState != AnimState.Throwing)
            {
                #region State checks
                if (_agent.velocity != Vector3.zero)
                {
                    _pathLineRenderer.positionCount = _agent.path.corners.Length;
                    _pathLineRenderer.SetPositions(_agent.path.corners);
                    _animState = AnimState.Walking;
                    _anim.SetBool("Walking", true);
                }
                else
                {
                    _pathLineRenderer.positionCount = 0;
                    _animState = AnimState.Idle;
                    _anim.SetBool("Walking", false);
                }

                if (_destinationMark && Vector3.Distance(transform.position, _worldDestination) < 0.2f)
                    _destinationMark.gameObject.SetActive(false);
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
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 60, _moveArea))
            {
                _worldDestination = hit.point;
                if (_destinationMark)
                {
                    _destinationMark.gameObject.SetActive(true);
                    _destinationMark.position = _worldDestination + new Vector3(0, 0.02f, 0);
                }
                _agent.SetDestination(_worldDestination);
            }
        }

        private IEnumerator TossCoin()
        {
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 35, _validCoinArea))
            {
                PrepareToTossCoin(hit.point, out AnimState currentState);

                if (_coinMark)
                {
                    _coinMark.position = hit.point;
                    _coinMark.gameObject.SetActive(true);
                }

                yield return new WaitForSeconds(0.8f);

                ArcData arc = CalculateCoinArcData(_coinOrigin.position, hit.point);

                switch (coinVisualStyle)
                {
                    case CoinVisualStyle.RealCoin:
                        SpawnAndTossCoin(arc, out GameObject coin, out Rigidbody rb);
                        if (_showArcInGame)
                            DisplayArc(arc);
                        yield return new WaitForSeconds(1.2f);
                        _animState = currentState;
                        _agent.isStopped = false;
                        yield return new WaitForSeconds(arc.time - 1.2f);
                        CoinHitsGround(coin.transform, rb, hit.point);
                        break;
                    case CoinVisualStyle.VFXCoin:
                        StartCoroutine(StartCoinVFX(arc));
                        if (_showArcInGame)
                            DisplayArc(arc);
                        yield return new WaitForSeconds(1.2f);
                        _agent.isStopped = false;
                        yield return new WaitForSeconds(arc.time - 1.2f);
                        SpawnCoin(hit.point);
                        _animState = currentState;
                        break;
                }

                if (_coinMark)
                    _coinMark.gameObject.SetActive(false);
            }
        }

        private ArcData CalculateCoinArcData(Vector3 origin, Vector3 target)
        {
            Vector3 displacement = target - origin;
            float time = Mathf.Sqrt(-2 * _arcHeight / _gravity) + Mathf.Sqrt(2 * (displacement.y - _arcHeight) / _gravity);
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

            return new ArcData(initialVelocity, arcPoints, origin, displacement, time);
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

        private void SpawnCoin(Vector3 spawnPoint)
        {
            Transform coin = Instantiate(
                _coinPrefab,
                spawnPoint,
                Quaternion.identity
            ).transform;
            coin.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

            if (_showArcInGame)
                _coinTossLineRenderer.positionCount = 0;

            AudioSource _as = coin.GetComponent<AudioSource>();
            _as.pitch = Random.Range(0.9f, 1.1f);
            _as.Play();

            Destroy(_as.gameObject, 25f);
            DistractGuards(coin);
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

        private IEnumerator StartCoinVFX(ArcData arc)
        {
            _coinVFXObject.position = arc.arcPoints[0];
            _coinVFXObject.gameObject.SetActive(true);

            float pointNumber = 60;
            float waitTime = arc.time / pointNumber;
            for (int i = 1; i <= pointNumber - 8; i++)
            {
                yield return new WaitForSecondsRealtime(waitTime);
                float simulationTime = i / (pointNumber - 8) * arc.time;
                Vector3 pointDisplacement = arc.initialVelocity * simulationTime + Vector3.up * _gravity * simulationTime * simulationTime / 2f;
                _coinVFXObject.position = arc.origin + pointDisplacement;
            }

            _coinVFXObject.gameObject.SetActive(false);
        }

        private void CoinHitsGround(Transform coin, Rigidbody coinRb, Vector3 hitPoint)
        {
            if (_showArcInGame)
                _coinTossLineRenderer.positionCount = 0;

            coin.transform.rotation = Quaternion.identity;
            coin.position = new Vector3(coin.position.x, -1.98f, coin.position.z);
            coinRb.velocity = Vector3.zero;
            coinRb.constraints = RigidbodyConstraints.FreezeAll;

            AudioSource _as = coin.GetComponent<AudioSource>();
            _as.pitch = Random.Range(0.9f, 1.1f);
            _as.Play();
            
            DistractGuards(coin);
            Destroy(coin.gameObject, 25f);
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
