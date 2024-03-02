using UnityEngine;

namespace FusionPong.Game
{
    [RequireComponent(typeof(Collider2D))]
    public class GoalController : MonoBehaviour
    {
        [SerializeField] private Collider2D _collider;
        [SerializeField] private PlayerPosition playerPosition;
        [Space]
        [SerializeField] private AudioSource _audioSource;
    
        private void Awake()
        {
            _collider ??= GetComponent<Collider2D>();
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            var ball = other.GetComponent<BallController>();
            if (!ball) return;

            ball.HasScored = true;
            _audioSource.Play();
            
            GameManager.GoalScored(playerPosition);
        }
    }
}
