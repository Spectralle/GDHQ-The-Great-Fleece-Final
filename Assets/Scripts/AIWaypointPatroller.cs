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
        public void EnablePathfinding() => _isPathfinding = true;
        public void DisablePathfinding() => _isPathfinding = false;
        [SerializeField, Range(0f, 50f)] private float _waitAtWaypoint = 5f;
        [SerializeField, Range(0.1f, 10f)] private float _turnSpeed = 2.5f;
        [SerializeField, Range(0f, 3f)] private float _turnEarlyDist = 0.5f;
        [SerializeField] private bool _pingpongLoop = true;
        [SerializeField] private List<WaypointContainer> _waypoints;
        [Header("Editor Settings:")]
        [SerializeField] private bool _showDebugPath = true;
        [SerializeField] private bool _showPlaymodeDebugPath = true;
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
        private Waypoint _currentWaypoint;
        private Vector3 targetDirection = Vector3.zero;


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
            _currentWaypoint = _waypoints[0].Primary;
            transform.position = _currentWaypoint.Position;
            transform.LookAt(_currentWaypoint.Position + _currentWaypoint.LookDirection);
        }

        private void Update() => UpdateBehaviour();

        private void UpdateBehaviour()
        {
            if (_isPathfinding)
            {
                if (_agentState == AgentState.Idle)
                {
                    if (_waitTimer < _waitAtWaypoint)
                        Wait();
                    else
                        SetDestinationAndStartWalking();
                }
                else
                {
                    float remainingDistance = _agent.remainingDistance - _agent.stoppingDistance;
                    if (remainingDistance < 0.01f)
                    {
                        StopWalking();
                        _waitTimer = Random.Range(0f, _waitAtWaypoint / 4);
                    }

                    if (remainingDistance < _turnEarlyDist)
                    {
                        targetDirection = _currentWaypoint.LookDirection.normalized;
                        RotateToFaceDirection();
                    }
                }
            }
            else
            {
                if (_agent.destination != transform.position)
                    _agent.SetDestination(transform.position);
            }
        }

        private void SetDestinationAndStartWalking()
        {
            if (_pingpongLoop)
            {
                if (_IDModifier == 1 ? (_currentWaypointID + _IDModifier < _waypoints.Count) : (_currentWaypointID + _IDModifier >= 0))
                    _currentWaypointID += _IDModifier;
                else
                {
                    _IDModifier = -_IDModifier;
                    _currentWaypointID += _IDModifier;
                }
            }
            else
            {
                if (_currentWaypointID + _IDModifier < _waypoints.Count)
                    _currentWaypointID += _IDModifier;
                else
                    _currentWaypointID = 0;
            }

            bool useAlternate = _waypoints[_currentWaypointID].AllowPathBranch && Random.value > 0.5f;
            _currentWaypoint = !useAlternate ? _waypoints[_currentWaypointID].Primary : _waypoints[_currentWaypointID].Alternate;
            _agent.SetDestination(_currentWaypoint.Position);

            _agentState = AgentState.Walking;
            _anim.SetBool("Walking", true);
            _waitTimer = 0;
        }

        private void Wait()
        {
            _waitTimer += Time.deltaTime;
            RotateToFaceDirection();
        }

        private void RotateToFaceDirection()
        {
            Quaternion rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDirection, _turnSpeed * Time.deltaTime, 0.0f));
            rotation.x = 0;
            rotation.z = 0;
            transform.rotation = rotation;
        }

        private void StopWalking()
        {
            _agentState = AgentState.Idle;
            _anim.SetBool("Walking", false);
            targetDirection = _currentWaypoint.LookDirection.normalized;
        }


        private void OnDrawGizmos()
        {
            if (!_showDebugPath)
                return;
            else if(!_showPlaymodeDebugPath && Application.isPlaying)
                return;

            for (int i = 0; i < _waypoints.Count; i++)
            {
                #region Current Position
                Gizmos.color = _alternateColor;
                Gizmos.DrawSphere(transform.position, 0.15f);
                #endregion

                #region Line To Waypoint/s
                #region Draw Early turn backwards lines
                if (i > 0 && _turnEarlyDist > 0f)
                {
                    Gizmos.color = _lookPointColor;
                    Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i - 1].Primary.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                    if (_waypoints[i].AllowPathBranch)
                    {
                        Gizmos.DrawRay(_waypoints[i].Alternate.Position, (_waypoints[i - 1].Primary.Position - _waypoints[i].Alternate.Position).normalized * _turnEarlyDist);

                        if (_waypoints[i - 1].AllowPathBranch)
                        {
                            Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i - 1].Alternate.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                            Gizmos.DrawRay(_waypoints[i].Alternate.Position, (_waypoints[i - 1].Primary.Position - _waypoints[i].Alternate.Position).normalized * _turnEarlyDist);
                            Gizmos.DrawRay(_waypoints[i].Alternate.Position, (_waypoints[i - 1].Alternate.Position - _waypoints[i].Alternate.Position).normalized * _turnEarlyDist);
                        }
                    }
                    else if (_waypoints[i - 1].AllowPathBranch)
                        Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i - 1].Alternate.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                }
                #endregion

                if (!_waypoints[i].AllowPathBranch)
                {
                    Gizmos.color = _waypointColor;
                    Gizmos.DrawSphere(_waypoints[i].Primary.Position, 0.2f);
                    if (_waypoints.Count > (i + 1))
                    {
                        if (_waypoints[i + 1].AllowPathBranch)
                        {
                            Gizmos.color = _alternateColor;
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Alternate.Position);

                            if (_turnEarlyDist > 0f)
                            {
                                Gizmos.color = _lookPointColor;
                                Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i + 1].Primary.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                                Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i + 1].Alternate.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                            }
                        }
                        else
                        {
                            Gizmos.color = _waypointColor;
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);

                            if (_turnEarlyDist > 0f)
                            {
                                Gizmos.color = _lookPointColor;
                                Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i + 1].Primary.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                            }
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
                            Gizmos.DrawLine(_waypoints[i].Alternate.Position, _waypoints[i + 1].Primary.Position);
                            Gizmos.DrawLine(_waypoints[i].Alternate.Position, _waypoints[i + 1].Alternate.Position);

                            if (_turnEarlyDist > 0f)
                            {
                                Gizmos.color = _lookPointColor;
                                Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i + 1].Primary.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                                Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i + 1].Alternate.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                                Gizmos.DrawRay(_waypoints[i].Alternate.Position, (_waypoints[i + 1].Primary.Position - _waypoints[i].Alternate.Position).normalized * _turnEarlyDist);
                                Gizmos.DrawRay(_waypoints[i].Alternate.Position, (_waypoints[i + 1].Alternate.Position - _waypoints[i].Alternate.Position).normalized * _turnEarlyDist);
                            }
                        }
                        else
                        {
                            Gizmos.DrawLine(_waypoints[i].Primary.Position, _waypoints[i + 1].Primary.Position);
                            Gizmos.DrawLine(_waypoints[i].Alternate.Position, _waypoints[i + 1].Primary.Position);

                            if (_turnEarlyDist > 0f)
                            {
                                Gizmos.color = _lookPointColor;
                                Gizmos.DrawRay(_waypoints[i].Primary.Position, (_waypoints[i + 1].Primary.Position - _waypoints[i].Primary.Position).normalized * _turnEarlyDist);
                                Gizmos.DrawRay(_waypoints[i].Alternate.Position, (_waypoints[i + 1].Primary.Position - _waypoints[i].Alternate.Position).normalized * _turnEarlyDist);
                            }
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
