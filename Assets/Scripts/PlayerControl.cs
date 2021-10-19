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
        private NavMeshAgent agent;
        private Animator anim;
        private enum AnimState
        {
            Idle,
            Walking,
            Throwing
        }
        private AnimState animState;


        private void Start()
        {
            _cameraTransform = Camera.main;
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (animState != AnimState.Throwing)
            {
                #region State checks
                if (agent.velocity != Vector3.zero)
                {
                    animState = AnimState.Walking;
                    anim.SetBool("Walking", true);
                }
                else
                {
                    animState = AnimState.Idle;
                    anim.SetBool("Walking", false);
                }
                #endregion

                #region Player Input
                if (Input.GetMouseButtonDown(0))
                    SetDestinationToClickPosition();

                if (Input.GetMouseButtonDown(1))
                    StartCoroutine(ThrowCoin());
                #endregion
            }
        }

        private void SetDestinationToClickPosition()
        {
            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                _worldDestination = hit.point;
                agent.SetDestination(_worldDestination);
            }
        }

        private IEnumerator ThrowCoin()
        {
            AnimState currentState = animState;
            animState = AnimState.Throwing;
            agent.isStopped = true;

            if (Physics.Raycast(_cameraTransform.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                transform.LookAt(hit.point);
            anim.SetTrigger("Throw coin");

            yield return new WaitForSeconds(2f);

            animState = currentState;
            agent.isStopped = false;
        }
    }
}
