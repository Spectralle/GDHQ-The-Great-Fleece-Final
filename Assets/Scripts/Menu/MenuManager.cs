using UnityEngine;
using NaughtyAttributes;
using UnityEngine.SceneManagement;

namespace TGF
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField, Scene] private string _sceneToLoadOnStart;


        public void StartGame() => SceneManager.LoadScene(_sceneToLoadOnStart);

        public void QuitGame() => Application.Quit();
    } 
}
