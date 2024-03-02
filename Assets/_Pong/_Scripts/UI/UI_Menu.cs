using Fusion;
using NoMossStudios.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FusionPong.UI
{
    public class UI_Menu : Singleton<UI_Menu>
    {
        [SerializeField] private Button quickPlayButton;
        [SerializeField] private TMP_InputField sessionNameInputField;

        private void Start()
        {
            quickPlayButton.onClick.AddListener(HandleQuickPlayButtonPressed);

            sessionNameInputField.onValueChanged.AddListener((text) =>
            {
                Debug.Log($"Session Code: {text}");
            });
        }
        
        private static void HandleQuickPlayButtonPressed()
        {
            Instance.quickPlayButton.interactable = false;
            
            NetworkManager.Instance.StartQuickGame(GameMode.AutoHostOrClient);
        }
        
    }
}
