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
        
        private Camera _cameraTransform;
        private Vector3 _worldDestination;
        private NavMeshAgent _agent;
        private Animator _anim;
        private PlayerState _playerState;
        private LineRenderer _pathLineRenderer;


        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _pathLineRenderer = GetComponent<LineRenderer>();
            _playerState = GetComponent<PlayerState>();
        }

        private void Start() => _cameraTransform = Camera.main;

        private void Update()
        {
            if (!_playerState.Compare(AnimState.Throwing))
            {
                #region State checks
                if (_agent.remainingDistance - _agent.stoppingDistance > 0f)
                {
                    _agent.isStopped = false;
                    _pathLineRenderer.positionCount = _agent.path.corners.Length;
                    _pathLineRenderer.SetPositions(_agent.path.corners);
                    if (!_playerState.Compare(AnimState.Walking))
                        _playerState.Set(AnimState.Walking);
                    if (!_anim.GetBool("Walking"))
                        _anim.SetBool("Walking", true);
                }
                else
                {
                    _agent.isStopped = true;
                    _pathLineRenderer.positionCount = 0;
                    if (!_playerState.Compare(AnimState.Idle))
                        _playerState.Set(AnimState.Idle);
                    if (_anim.GetBool("Walking"))
                     _anim.SetBool("Walking", false);
                }

                if (_destinationMark && Vector3.Distance(transform.position, _worldDestination) < 0.2f)
                    _destinationMark.gameObject.SetActive(false);
                #endregion

                #region Player Input
                if (Input.GetButtonDown("Move"))
                    SetDestinationToClickPosition();
                #endregion
            }
        }

        private void SetDestinationToClickPosition()
        {
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 60, _moveArea))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit nmhit, 2f, NavMesh.AllAreas))
                {
                    _worldDestination = nmhit.position;
                    _agent.SetDestination(_worldDestination);

                    if (_destinationMark)
                    {
                        _destinationMark.gameObject.SetActive(true);
                        _destinationMark.position = _worldDestination + new Vector3(0, 0.02f, 0);
                    }
                }
            }
        }
    }
}
