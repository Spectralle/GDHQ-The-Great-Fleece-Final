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


    private void Awake() => _gravity = Physics.gravity.y;

    private void Update()
    {
        Debug.DrawLine(_target + (Vector3.down * 0.3f), _target + (Vector3.up * 0.3f));
        Debug.DrawLine(_target + (Vector3.forward * 0.3f), _target + (Vector3.back * 0.3f));
        Debug.DrawLine(_target + (Vector3.left * 0.3f), _target + (Vector3.right * 0.3f));

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("Click");
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, _validCoinArea))
            {
                _target = hit.point;
                CalculateCoinArcData();
                DisplayArc();
                Debug.Log("Render");
            }
        }
    }

    private void CalculateCoinArcData()
    {
        #region Calculate initial velocity
        Vector3 displacement = _target - _origin.position;
        float time = Mathf.Sqrt(-2 * _arcHeight / _gravity) + Mathf.Sqrt(2 * (displacement.y - _arcHeight) / _gravity);
        Vector3 initialVelocity = displacement / time;
        initialVelocity.y = Mathf.Sqrt(-2 * _gravity * _arcHeight) * -Mathf.Sign(_gravity);
        #endregion

        #region Calculate points on arc
        Vector3[] arcPoints = new Vector3[_arcResolution + 1];
        arcPoints[0] = _origin.position;
        arcPoints[arcPoints.Length - 1] = _target;
        for (int i = 1; i <= _arcResolution; i++)
        {
            float simulationTime = i / (float)_arcResolution * time;
            Vector3 pointDisplacement = initialVelocity * simulationTime + Vector3.up * _gravity * simulationTime * simulationTime / 2f;
            arcPoints[i] = _origin.position + pointDisplacement;
        }
        #endregion

        _arc = new ArcData(initialVelocity, arcPoints);
    }

    private void DisplayArc()
    {
        _coinTossLineRenderer.positionCount = _arc.arcPoints.Length;
        _coinTossLineRenderer.SetPositions(_arc.arcPoints);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(_origin.position, _arc.initialVelocity.normalized * 2);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _arc.arcPoints.Length - 1; i++)
                Gizmos.DrawLine(_arc.arcPoints[i], _arc.arcPoints[i + 1]);
        }
    }
}
