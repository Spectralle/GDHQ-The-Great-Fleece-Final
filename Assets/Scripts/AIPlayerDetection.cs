using SensorToolkit;
using UnityEngine;

namespace TGF
{
    public class AIPlayerDetection : MonoBehaviour
    {
        [SerializeField] private TriggerSensor _lineOfSightSensor;
        [SerializeField] private RangeSensor _proximitySensor;
        [SerializeField] private GameObject _captureCutscene;

        private Animator _anim;
        private AIWaypointPatroller _ai;


        private void Awake()
        {
            TryGetComponent(out _anim);
            TryGetComponent(out _ai);
        }

        public void PlayerEnteredDetection_LOS()
        {
            Debug.Log("Player detected via Line-Of-Sight");

            if (_lineOfSightSensor.DetectedObjects[0])
                _lineOfSightSensor.DetectedObjects[0].SetActive(false);
            else
                Debug.LogError($"Error: Player detected by {name} but no GameObjects in DetectedObjects array.");
            PlayerEnteredDetection();
        }

        public void PlayerEnteredDetection_PRX()
        {
            Debug.Log("Player detected via proximity");

            if (_proximitySensor.DetectedObjects[0])
                _proximitySensor.DetectedObjects[0].SetActive(false);
            else
                Debug.LogError($"Error: Player detected by {name} but no GameObjects in DetectedObjects array.");
            PlayerEnteredDetection();
        }

        private void PlayerEnteredDetection()
        {
            _ai?.DisablePathfinding();
            _anim?.SetBool("CanSeePlayer", true);
            if (_captureCutscene)
                _captureCutscene.SetActive(true);
            else
                Debug.LogError("No Captured cutscene specified.");
        }
    }
}
