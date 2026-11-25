using MilkMaker.Economy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MilkMaker.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIDocument))]
    public sealed class VictoryScreen : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private MilkEconomy _economy;

        [Header("Gameplay Behaviours")]
        [SerializeField] private Behaviour[] _disableOnVictory;

        private VisualElement _screen;
        private Label _finalBalance;

        private Button _restartButton;
        private Button _exitButton;

        private bool _hasWon;
        private bool _isLeaving;

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _debug;
#endif

        private void Start()
        {
            UIDocument document =
                GetComponent<UIDocument>();

            VisualElement root =
                document.rootVisualElement;

            _screen =
                root?.Q<VisualElement>("victory-screen");

            _finalBalance =
                root?.Q<Label>("final-balance");

            _restartButton =
                root?.Q<Button>("restart-button");

            _exitButton =
                root?.Q<Button>("exit-button");

            if (_economy == null ||
                _screen == null ||
                _finalBalance == null ||
                _restartButton == null ||
                _exitButton == null)
            {
                Debug.LogError(
                    "[VictoryScreen] Invalid configuration.",
                    this);

                enabled = false;
                return;
            }

            _screen.style.display = DisplayStyle.None;

            _restartButton.clicked += Restart;
            _exitButton.clicked += Exit;

            _economy.OnBalanceChanged += OnBalanceChanged;

            TryComplete();
        }

        private void OnBalanceChanged(double balance)
        {
            TryComplete();
        }

        private void TryComplete()
        {
            if (!isActiveAndEnabled ||
                _hasWon ||
                _economy == null ||
                !_economy.IsGoalReached)
            {
                return;
            }

            _hasWon = true;

            _finalBalance.text =
                $"{MilkFormatter.Format(_economy.Balance)} L";

            if (_disableOnVictory != null)
            {
                foreach (Behaviour behaviour in _disableOnVictory)
                {
                    if (behaviour != null &&
                        behaviour != this &&
                        behaviour != _economy)
                    {
                        behaviour.enabled = false;
                    }
                }
            }

            Time.timeScale = 0f;

            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;

            _screen.style.display = DisplayStyle.Flex;

#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log(
                    "[VictoryScreen] Debt cleared.",
                    this);
            }
#endif
        }

        private void Restart()
        {
            if (!_hasWon || _isLeaving)
                return;

            _isLeaving = true;

            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex);
        }

        private void Exit()
        {
            if (!_hasWon || _isLeaving)
                return;

            _isLeaving = true;

            Time.timeScale = 1f;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (_economy != null)
            {
                _economy.OnBalanceChanged -=
                    OnBalanceChanged;
            }

            if (_restartButton != null)
                _restartButton.clicked -= Restart;

            if (_exitButton != null)
                _exitButton.clicked -= Exit;

            if (_hasWon)
                Time.timeScale = 1f;
        }
    }
}