using TMPro;
using UnityEngine;
using DG.Tweening;

namespace FusionPong.UI
{
    public class UI_PlayerInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerInfoText;
        [SerializeField] private int playerRef;

        private bool _isInit = false;
        
        private void Awake()
        {
            playerInfoText ??= GetComponentInChildren<TextMeshProUGUI>();
        }

        public void Init(Player player)
        {
            if (_isInit) return;
            _isInit = true;
            
            Debug.Log($"Init UI_PlayerInfo for PlayerNum: {player.PlayerNumber}");
            
            player.OnScoreChangedEvent.AddListener(HandleOnScoreChanged);
            HandleOnScoreChanged(0);
        }

    private void HandleOnScoreChanged(int newScore)
        {
            playerInfoText.text = $"Player {playerRef + 1} - {newScore}";
            
            if (newScore > 0)
                playerInfoText.rectTransform.DOPunchScale(Vector3.one * 1.5f, 1f, 2);
        }
    }
}
