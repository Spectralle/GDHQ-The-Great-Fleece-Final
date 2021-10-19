using System.Collections;
using UnityEngine;
using UnityEngine.AI;


namespace TGF
{
    [RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
    public class PlayerControl : MonoBehaviour
    {
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


        private void Start()
        {
            _cameraTransform = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
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

                if (Input.GetButtonDown("Toss Coin"))
                    StartCoroutine(TossCoin());
                #endregion
            }
        }

        private void SetDestinationToClickPosition()
        {
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                _worldDestination = hit.point;
                _agent.SetDestination(_worldDestination);
            }
        }

        private IEnumerator TossCoin()
        {
            AnimState currentState = _animState;
            _animState = AnimState.Throwing;
            _agent.isStopped = true;

            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                transform.LookAt(hit.point);
            _anim.SetTrigger("Throw coin");

            yield return new WaitForSeconds(2f);

            _animState = currentState;
            _agent.isStopped = false;
        }
    }
}
