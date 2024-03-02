using UnityEngine;
using UnityEngine.UI;

namespace NoMossStudios.Utilities
{
    /// <summary>
    /// Attaches to a UI Button to open a specified URL via onClick
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UrlButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private string url;

        private void Awake()
        {
            button ??= GetComponent<Button>();
            
            button.onClick.AddListener(() =>
            {
                Application.OpenURL(url);
            });
        }
    }
}