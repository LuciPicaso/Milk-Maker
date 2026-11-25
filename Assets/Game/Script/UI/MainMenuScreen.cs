using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MilkMaker.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class MainMenuScreen : MonoBehaviour
    {
        [Header("Scenes")]
        [SerializeField] private string _gameplayScene = "SampleScene";

        private VisualElement _mainPage;
        private VisualElement _helpPage;

        private Button _playButton;
        private Button _helpButton;
        private Button _exitButton;
        private Button _backButton;

        private bool _isLeaving;

        private void Start()
        {
            Time.timeScale = 1f;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            VisualElement root =
                GetComponent<UIDocument>().rootVisualElement;

            _mainPage = root?.Q<VisualElement>("main-page");
            _helpPage = root?.Q<VisualElement>("help-page");

            _playButton = root?.Q<Button>("play-button");
            _helpButton = root?.Q<Button>("help-button");
            _exitButton = root?.Q<Button>("exit-button");
            _backButton = root?.Q<Button>("back-button");

            if (_mainPage == null ||
                _helpPage == null ||
                _playButton == null ||
                _helpButton == null ||
                _exitButton == null ||
                _backButton == null)
            {
                Debug.LogError(
                    "[MainMenuScreen] Invalid UI configuration.",
                    this);

                enabled = false;
                return;
            }

            _playButton.clicked += Play;
            _helpButton.clicked += ShowHelp;
            _exitButton.clicked += Exit;
            _backButton.clicked += ShowMain;

            ShowMain();
        }

        private void Play()
        {
            if (_isLeaving)
                return;

            if (!Application.CanStreamedLevelBeLoaded(_gameplayScene))
            {
                Debug.LogError(
                    $"[MainMenuScreen] Scene not found: {_gameplayScene}",
                    this);

                return;
            }

            _isLeaving = true;

            Time.timeScale = 1f;

            SceneManager.LoadScene(_gameplayScene);
        }

        private void ShowHelp()
        {
            _mainPage.style.display = DisplayStyle.None;
            _helpPage.style.display = DisplayStyle.Flex;
        }

        private void ShowMain()
        {
            _helpPage.style.display = DisplayStyle.None;
            _mainPage.style.display = DisplayStyle.Flex;
        }

        private void Exit()
        {
            if (_isLeaving)
                return;

            _isLeaving = true;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (_playButton != null)
                _playButton.clicked -= Play;

            if (_helpButton != null)
                _helpButton.clicked -= ShowHelp;

            if (_exitButton != null)
                _exitButton.clicked -= Exit;

            if (_backButton != null)
                _backButton.clicked -= ShowMain;
        }
    }
}