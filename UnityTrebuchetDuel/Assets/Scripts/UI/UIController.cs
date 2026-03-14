using System;
using TrebuchetDuel.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TrebuchetDuel.UI
{
    /// <summary>
    /// Handles touch-friendly gameplay HUD and menu interactions.
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("HUD")]
        public TMP_Text turnText;
        public TMP_Text commanderText;
        public TMP_Text messageText;
        public TMP_Text trebP1Text;
        public TMP_Text trebP2Text;

        [Header("Buttons")]
        public Button buildButton;
        public Button sabotageButton;
        public Button repairButton;
        public Button siegeButton;
        public Button fireButton;
        public Button endTurnButton;
        public Button restartButton;

        [Header("Win Panel")]
        public GameObject winPanel;
        public TMP_Text winText;
        public Button winRestartButton;

        [Header("Commander Dropdowns")]
        public TMP_Dropdown p1Dropdown;
        public TMP_Dropdown p2Dropdown;
        public Button startMatchButton;

        public Action<ActionType> OnActionPressed;
        public Action OnEndTurnPressed;
        public Action OnRestartPressed;
        public Action OnStartMatchPressed;

        private void Awake()
        {
            buildButton.onClick.AddListener(() => OnActionPressed?.Invoke(ActionType.Build));
            sabotageButton.onClick.AddListener(() => OnActionPressed?.Invoke(ActionType.Sabotage));
            repairButton.onClick.AddListener(() => OnActionPressed?.Invoke(ActionType.Repair));
            siegeButton.onClick.AddListener(() => OnActionPressed?.Invoke(ActionType.Siege));
            fireButton.onClick.AddListener(() => OnActionPressed?.Invoke(ActionType.Fire));
            endTurnButton.onClick.AddListener(() => OnEndTurnPressed?.Invoke());
            restartButton.onClick.AddListener(() => OnRestartPressed?.Invoke());
            winRestartButton.onClick.AddListener(() => OnRestartPressed?.Invoke());
            startMatchButton.onClick.AddListener(() => OnStartMatchPressed?.Invoke());
        }

        public void BindCommanderOptions(string[] names)
        {
            p1Dropdown.ClearOptions();
            p2Dropdown.ClearOptions();
            p1Dropdown.AddOptions(new System.Collections.Generic.List<string>(names));
            p2Dropdown.AddOptions(new System.Collections.Generic.List<string>(names));
        }

        public (int p1, int p2) CommanderIndexes() => (p1Dropdown.value, p2Dropdown.value);

        public void SetMessage(string msg) => messageText.text = msg;

        public void Refresh(GameState state)
        {
            turnText.text = $"Turn: {(state.CurrentPlayer == PlayerId.Player1 ? "Red" : "Blue")}";
            commanderText.text = $"Red: {state.Commanders[PlayerId.Player1].Name} | Blue: {state.Commanders[PlayerId.Player2].Name}";
            trebP1Text.text = $"Red Trebuchet: {state.Trebuchets[PlayerId.Player1].CompletedParts}/4";
            trebP2Text.text = $"Blue Trebuchet: {state.Trebuchets[PlayerId.Player2].CompletedParts}/4";

            winPanel.SetActive(state.MatchOver);
            if (state.MatchOver && state.Winner.HasValue)
                winText.text = $"{(state.Winner.Value == PlayerId.Player1 ? "Red" : "Blue")} wins!";
        }

        public void SetActionButtons(bool build, bool sabotage, bool repair, bool siege, bool fire, bool endTurn)
        {
            buildButton.interactable = build;
            sabotageButton.interactable = sabotage;
            repairButton.interactable = repair;
            siegeButton.interactable = siege;
            fireButton.interactable = fire;
            endTurnButton.interactable = endTurn;
        }
    }
}
