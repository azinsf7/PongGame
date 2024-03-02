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

    
        private void Start()
        {
            quickPlayButton.onClick.AddListener(HandleQuickPlayButtonPressed);
        }
        
        private static void HandleQuickPlayButtonPressed()
        {
            Instance.quickPlayButton.interactable = false;
            
            NetworkManager.Instance.StartQuickGame(GameMode.AutoHostOrClient);
        }
    


    }
}
