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
        private AIWaypointPatroller _aiwp;


        private void Awake()
        {
            TryGetComponent(out _anim);
            TryGetComponent(out _aiwp);
        }

        public void PlayerEnteredDetection_LOS()
        {
            Debug.Log("Player detected via Line-Of-Sight by " + name);

            if (_lineOfSightSensor.DetectedObjects[0])
                _lineOfSightSensor.DetectedObjects[0].SetActive(false);
            else
                Debug.LogError($"Error: Player detected by {name} but no GameObjects in DetectedObjects array.");
            PlayerEnteredDetection();
        }

        public void PlayerEnteredDetection_PRX()
        {
            Debug.Log("Player detected via proximity to " + name);

            if (_proximitySensor.DetectedObjects[0])
                _proximitySensor.DetectedObjects[0].SetActive(false);
            else
                Debug.LogError($"Error: Player detected by {name} but no GameObjects in DetectedObjects array.");
            PlayerEnteredDetection();
        }

        private void PlayerEnteredDetection()
        {
            _aiwp?.DisablePathfinding();
            _anim?.SetBool("CanSeePlayer", true);
            if (_captureCutscene)
                _captureCutscene.SetActive(true);
            else
                Debug.LogError("No Captured cutscene specified.");
        }
    }
}
