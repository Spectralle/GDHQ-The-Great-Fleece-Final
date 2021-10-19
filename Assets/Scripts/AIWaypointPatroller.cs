using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace TGF
{
    [RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Animator))]
    public class AIWaypointPatroller : MonoBehaviour
    {
        [Header("Path Settings:")]
        [SerializeField] private bool _isPathfinding = true;
        [SerializeField] private float _waitAtWaypoint = 5f;
        [SerializeField] private float _turnSpeed = 5f;
        [SerializeField] private List<WaypointContainer> _waypoints;
        [Header("Editor Settings:")]
        [SerializeField] private Color _waypointColor = new Color(0f, 1f, 0f, 1f);
        [SerializeField] private Color _alternateColor = new Color(0f, 0.75f, 1f, 1f);
        [SerializeField] private Color _lookPointColor = new Color(1f, 0f, 0f, 1f);
        [System.Serializable]
        public struct WaypointContainer
        {
            public Waypoint Primary;
            public bool AllowPathBranch;
            public Waypoint Alternate;
        }
        [System.Serializable]
        public struct Waypoint
        {
            public Vector3 Position;
            public Vector3 LookDirection;
        }

        private NavMeshAgent _agent;
        private Animator _anim;
        private int _currentWaypointID = 0;
        private float _waitTimer = 0f;
        private int _IDModifier = 1;
        private enum AgentState
        {
            Idle,
            Walking,
        }
        private AgentState _agentState;
        private Vector3 lookDirection;


        private void Awake()
        {
            if (_waypoints.Count == 0)
            {
                Debug.LogError("No waypoints specified! Disabling AI...");
                enabled = false;
                return;
            }

            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<Animator>();
            _waitTimer = Random.Range(0, _waitAtWaypoint - (_waitAtWaypoint / 3));
            transform.position = _waypoints[0].Primary.Position;
            transform.LookAt(transform.position + _waypoints[0].Primary.LookDirection);
        }

        private void Update()
        {
            if (_isPathfinding)
            {
                if (_agentState == AgentState.Idle)
                {
                    if (_waitTimer < _waitAtWaypoint)
                        _waitTimer += Time.deltaTime;
                    else
                    {
                        if (_IDModifier == 1 ? (_currentWaypointID + _IDModifier < _waypoints.Count) : (_currentWaypointID + _IDModifier >= 0))
                            _currentWaypointID += _IDModifier;
                        else
                        {
                            _IDModifier = -_IDModifier;
                            _currentWaypointID += _IDModifier;
                        }

                        bool useAlternate = _waypoints[_currentWaypointID].AllowPathBranch && Random.value > 0.5f;
                        Vector3 destination = !useAlternate ? _waypoints[_currentWaypointID].Primary.Position : _waypoints[_currentWaypointID].Alternate.Position;
                        lookDirection = !useAlternate ?
                            _waypoints[_currentWaypointID].Primary.LookDirection :
                            _waypoints[_currentWaypointID].Alternate.LookDirection;
                        _agent.SetDestination(destination);

                        _agentState = AgentState.Walking;
                        _anim.SetBool("Walking", true);
                        _waitTimer = 0;
                    }
                }
                else
                {
                    if (_agent.remainingDistance - _agent.stoppingDistance < 0.01f)
                    {
                        _agentState = AgentState.Idle;
                        _anim.SetBool("Walking", false);
                    }
                }
            }
            else
            {
                if (_agent.destination != transform.position)
                    _agent.SetDestination(transform.position);
            }
        }


        private void OnDrawGizmos()
        {
            for (int i = 0; i < _waypoints.Count; i++)
            {
                #region Current Position
                Gizmos.color = _alternateColor;
                Gizmos.DrawSphere(transform.position, 0.15f);
                #endregion

                #region Line To Point/s
                if (!_waypoints[i].AllowPathBranch)
                {
                    Gizmos.color = _waypointColor;
                    Gizmos.DrawSphere(_waypoints[i].Primary.Position, 0.2f);
                    if (_waypoints.Count > (i + 1))
                    {
                        if (_waypoints[i + 1].AllowPathBranch)
                        {
                            Gizmos.color = _alternateColor;
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Alternate.Position);
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);
                        }
                        else
                        {
                            Gizmos.color = _waypointColor;
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);
                        }
                    }
                }
                else
                {
                    Gizmos.color = _alternateColor;
                    Gizmos.DrawSphere(_waypoints[i].Primary.Position, 0.2f);
                    Gizmos.DrawSphere(_waypoints[i].Alternate.Position, 0.2f);
                    if (_waypoints.Count > (i + 1))
                    {
                        if (_waypoints[i + 1].AllowPathBranch)
                        {
                            Gizmos.color = _alternateColor;
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Alternate.Position);
                        }
                        else
                        {
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);
                            Gizmos.DrawLine(_waypoints[i].Alternate.Position, _waypoints[i + 1].Primary.Position);
                        }
                    }
                }
                #endregion

                #region Face Direction
                Gizmos.color = _lookPointColor;
                Gizmos.DrawSphere(_waypoints[i].Primary.Position + _waypoints[i].Primary.LookDirection, 0.1f);
                Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i].Primary.Position + _waypoints[i].Primary.LookDirection);

                if (_waypoints[i].AllowPathBranch)
                {
                    Gizmos.DrawSphere(_waypoints[i].Alternate.Position + _waypoints[i].Alternate.LookDirection, 0.1f);
                    Gizmos.DrawLine(_waypoints[i].Alternate.Position, _waypoints[i].Alternate.Position + _waypoints[i].Alternate.LookDirection);
                }
                #endregion
            }
        }
    } 
}
