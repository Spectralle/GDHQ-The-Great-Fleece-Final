using Febucci.UI;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace TGF
{
    [RequireComponent(typeof(AudioManager))]
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError("ERROR: No GameManager exists in scene.");
                return _instance;
            }
        }

        public static AudioManager Audio;

        [Header("Intro Cutscene:")]
        [SerializeField] private PlayableDirector _introCutscene;

        [Header("Screen Messages:")]
        [SerializeField] private TextAnimatorPlayer _screenMessage;
        [SerializeField] private TextAnimatorPlayer _screenMinorMessage;

        [Header("Audio Manager:")]
        [SerializeField] private AudioClip _ambientMusic;

        public bool HasGuardsKeycard { get; set; }

        private IEnumerator _runningMajorMessage;
        private IEnumerator _runningMinorMessage;


        private void Awake()
        {
            _instance = this;
            Audio = GetComponent<AudioManager>();

            Audio.PlayMusicAudio(_ambientMusic);
        }

        private void Update()
        {
            if (_introCutscene.state == PlayState.Playing)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
                {
                    _introCutscene.time = _introCutscene.duration - 0.05f;
                    StopCoroutine(DisplayMinorMessage("Press Left Mouse or Space to skip"));
                    _screenMinorMessage.ShowText(string.Empty);
                    _runningMinorMessage = null;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
                SceneManager.LoadScene(0);
        }

        public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        public void QuitGame() => Application.Quit();

        public void DisplayMessageOnScreen(string message)
        {
            if (_runningMajorMessage != null)
                StopCoroutine(_runningMajorMessage);
            _runningMajorMessage = DisplayMessage(message);
            StartCoroutine(_runningMajorMessage);
        }
        public void DisplayMessageOnScreen(string message, int duration)
        {
            if (_runningMajorMessage != null)
                StopCoroutine(_runningMajorMessage);
            _runningMajorMessage = DisplayMessage(message, duration);
            StartCoroutine(_runningMajorMessage);
        }
        private IEnumerator DisplayMessage(string message, int duration = 6)
        {
            _screenMessage.ShowText(message);
            yield return new WaitForSecondsRealtime(duration);
            _screenMessage.ShowText(string.Empty);
            _runningMajorMessage = null;
        }

        public void DisplayMinorMessageOnScreen(string message)
        {
            if (_runningMinorMessage != null)
                StopCoroutine(_runningMinorMessage);
            _runningMinorMessage = DisplayMinorMessage(message);
            StartCoroutine(_runningMinorMessage);
        }
        public void DisplayMinorMessageOnScreen(string message, int duration)
        {
            if (_runningMinorMessage != null)
                StopCoroutine(_runningMinorMessage);
            _runningMinorMessage = DisplayMinorMessage(message, duration);
            StartCoroutine(DisplayMinorMessage(message, duration));
        }
        private IEnumerator DisplayMinorMessage(string message, int duration = 6)
        {
            _screenMinorMessage.ShowText(message);
            yield return new WaitForSecondsRealtime(duration);
            _screenMinorMessage.ShowText(string.Empty);
            _runningMinorMessage = null;
        }
    }
}
