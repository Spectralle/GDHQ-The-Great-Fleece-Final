using Febucci.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TGF
{
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

        [SerializeField] private TextAnimatorPlayer _screenMessage;

        public bool HasGuardsKeycard { get; set; }


        private void Awake() => _instance = this;

        public void RestartLevel() => SceneManager.LoadScene("Main");

        public void QuitGame() => Application.Quit();

        public void DisplayMessageOnScreen(string message) => StartCoroutine(DisplayMessage(message));
        public void DisplayMessageOnScreen(string message, int duration) => StartCoroutine(DisplayMessage(message, duration));
        private IEnumerator DisplayMessage(string message, int duration = 6)
        {
            _screenMessage.ShowText(message);
            yield return new WaitForSecondsRealtime(duration);
            _screenMessage.ShowText(string.Empty);
        }
    }
}
