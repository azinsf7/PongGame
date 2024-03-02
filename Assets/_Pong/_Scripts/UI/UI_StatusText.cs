using NoMossStudios.Utilities;
using TMPro;
using UnityEngine;

namespace FusionPong.UI
{
    public class UI_StatusText : PersistentSingleton<UI_StatusText>
    {
        [SerializeField] private TextMeshProUGUI statusText;
    
        public static void SetStatusText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                Debug.Log(text);
            
            Instance.statusText.text = text;
        }
    }
}

