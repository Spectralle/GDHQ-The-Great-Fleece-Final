using Febucci.UI;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TGF
{
    public class LoadingMenu : MonoBehaviour
    {
        [Header("Loading Progress:")]
        [SerializeField, Scene] private string _gameScene;
        [SerializeField, Required] private Image _progressBar;
        [SerializeField, Required] private TextMeshProUGUI _percentProgressText;
        [SerializeField, Required] private TextMeshProUGUI _continueText;
        [SerializeField, Range(0f, 10f)] private float _minimumLoadTime = 5f;
        [Header("Gameplay Tips:")]
        [SerializeField, Required] private TextAnimatorPlayer _gameTipsText;
        [SerializeField, Range(5f, 15f)] private float _tipShowDuration = 8f;
        [SerializeField, Range(1f, 3f)] private float _tipBreakDuration = 1f;
        [SerializeField, TextArea] private string[] _tipsList;

        [SerializeField] private float _minLoadTimer = 0f;
        [SerializeField] private float _tipShowTimer = 0f;
        [SerializeField] private float _tipBreakTimer = 0f;


        private void Awake() => StartCoroutine(LoadAsyncGameScene());

        private void Update()
        {
            if (_minLoadTimer < _minimumLoadTime)
                _minLoadTimer += Time.deltaTime;

            if (_tipShowTimer <= 0f)
            {
                if (_tipBreakTimer <= 0f)
                {
                    _gameTipsText.ShowText(_tipsList[Random.Range(0, _tipsList.Length - 1)]);
                    _tipShowTimer = _tipShowDuration;
                    _tipBreakTimer = _tipBreakDuration;
                }
                else
                {
                    if (_tipBreakTimer == _tipBreakDuration)
                    _gameTipsText.ShowText(string.Empty);
                    _tipBreakTimer -= Time.deltaTime;
                }
            }
            else
                _tipShowTimer -= Time.deltaTime;
        }

        private IEnumerator LoadAsyncGameScene()
        {
            yield return null;

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_gameScene);
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                _progressBar.fillAmount = asyncOperation.progress;
                _percentProgressText.SetText($"{asyncOperation.progress * 100}%");
                if (asyncOperation.progress >= 0.9f)
                    break;
                yield return null;
            }

            _progressBar.fillAmount = 1;
            _percentProgressText.SetText("100%");
            _continueText.SetText("Press Space to continue");
            while (!Input.GetKeyDown(KeyCode.Space))
                yield return null;

            asyncOperation.allowSceneActivation = true;
        }
    }
}
