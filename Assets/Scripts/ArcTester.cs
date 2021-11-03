using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcTester : MonoBehaviour
{
    public Transform _origin;
    public Vector3 _target;
    public LineRenderer _coinTossLineRenderer;
    [Min(0.1f)] public float _arcHeight;
    [Min(2)] public int _arcResolution = 12;
    public LayerMask _validCoinArea;

    private float _gravity;
    public ArcData _arc;


    private void Awake()
    {
        _gravity = Mathf.Abs(Physics.gravity.y);

        CalculateCoinArcData();
        DisplayArc();
    }

    private void Update()
    {
        Debug.DrawLine(_target + (Vector3.down * 0.3f), _target + (Vector3.up * 0.3f));
        Debug.DrawLine(_target + (Vector3.forward * 0.3f), _target + (Vector3.back * 0.3f));
        Debug.DrawLine(_target + (Vector3.left * 0.3f), _target + (Vector3.right * 0.3f));

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _validCoinArea))
            {
                _target = hit.point;
                CalculateCoinArcData();
            }
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _arc.arcPoints.Length > 0)
            CalculateCoinArcData();
    }

    private void DisplayArc()
    {
        //_coinTossLineRenderer.positionCount = _coinResolution + 1;
        //_coinTossLineRenderer.SetPositions(_arc);
    }

    // Angle and Velocity version
    /*
    private void CalculateArcArray()
    {
        _arc = new Vector3[_coinResolution + 1];

        _radianAngle = Mathf.Deg2Rad * _coinAngle;
        float maxDistance = (_coinVelocity * _coinVelocity * Mathf.Sin(2 * _radianAngle)) / Mathf.Abs(Physics.gravity.y);

        for (int i = 0; i <= _coinResolution; i++)
        {
            float t = (float)i / (float)_coinResolution;
            _arc[i] = CalculateArcPoint(t, maxDistance);
        }
    }

    private Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float X = t * maxDistance;
        float Y = _target.y + X * Mathf.Tan(_radianAngle) - ((Mathf.Abs(Physics.gravity.y) * X * X) /
            (2 * _coinVelocity * _coinVelocity * Mathf.Cos(_radianAngle) * Mathf.Cos(_radianAngle)));
        //float Z = ;
        return transform.position + new Vector3(0, Y, X);
    }
    */

    /// Destination Calculation version (Kinematic Equations)
    ///      --------------------------------------------->
    ///      ^                       ACROSS                 |
    ///      |                  ___-----___                    |
    ///      | UP          /          |          \                 |  DOWN
    ///      |           /               |               \            |
    ///      |      /                    | Height        \       V
    ///      |  /                        |                       End
    ///     Start                                                  ^
    ///                                                               | Py
    ///      <-------------------- Px ----------------->v
    /// UP: Displacement = Height, Acceleration = Gravity, Final Velocity = 0
    /// ACROSS: Displacement = Px, Acceleration = 0, Time = TimeUp + TimeDown
    /// DOWN: Displacement = Py - Height, Acceleration = Gravity, Initial Velocity = 0

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

    private void CalculateCoinArcData()
    {
        #region Calculate arc
        // Quote from tutorial
        float displacementY = _target.y - _origin.position.y;
        Vector3 displacementXZ = new Vector3(_target.x - _origin.position.x, 0, _target.z - _origin.position.z);
        float time = Mathf.Sqrt(2 * _arcHeight / _gravity) + Mathf.Sqrt(-2 * (displacementY - _arcHeight) / _gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * _gravity * _arcHeight);
        Vector3 velocityXZ = displacementXZ / time;
        Vector3 initialVelocity = velocityXZ + velocityY * -Mathf.Sign(_gravity);

        // My refactored version of the above
        // Vector3 displacement = _target - _origin.position;
        // float time = Mathf.Sqrt(-2 * _coinArcHeight / _gravity) + Mathf.Sqrt(2 * (displacement.y - _coinArcHeight) / _gravity);
        // Vector3 initialVelocity = displacement / time;
        // initialVelocity.y = Mathf.Sqrt(-2 * _gravity * _coinArcHeight) * -Mathf.Sign(_gravity);
        #endregion

        #region Calculate points on arc
        Vector3[] arcPoints = new Vector3[_arcResolution + 1];
        arcPoints[0] = _origin.position;
        arcPoints[arcPoints.Length - 1] = _target;
        for (int i = 1; i <= _arcResolution; i++)
        {
            float simulationTime = i / (float)_arcResolution * time;
            Vector3 displacement = initialVelocity * simulationTime + Vector3.up * _gravity * simulationTime * simulationTime / 2f;
            arcPoints[i] = _origin.position + displacement;
        }
        #endregion

        _arc = new ArcData(initialVelocity, arcPoints);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _arc.arcPoints.Length - 1; i++)
                Gizmos.DrawLine(_arc.arcPoints[i], _arc.arcPoints[i + 1]);
        }
    }
}
