using Fusion;
using UnityEngine;

namespace FusionPong.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PaddleController : NetworkBehaviour
    {
        private const float BaseSpeed = 10f;
        private const float BoundY = 3f;
        private Rigidbody2D _rb;
        private float _speedMultiplier = 10f; // Default speed multiplier

        private bool _isSpeedBoostActive = false; // Track if speed boost is active
        private  float _speedBoostDuration = 5f; // Duration of speed boost
        private float _speedBoostTimer = 0f; // Timer for speed boost
        
        public float Speed
        {
            get { return BaseSpeed; }
            set { _speedMultiplier = value; }
        }
        private void Awake()
        {
            _rb ??= GetComponent<Rigidbody2D>();
        }

        public override void Spawned()
        {
            base.Spawned();
        }

        private float _direction = 0f;

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out NetworkInputData data)) return;
            
            if (Input.GetKey(KeyCode.Space))
            {
                if (!_isSpeedBoostActive)
                {
                    Debug.Log("Space has been pressed");
                    ActivateSpeedBoost();
                    _speedBoostTimer -= Time.fixedDeltaTime;
                    if (_speedBoostTimer <= 0f)
                    {
                        _isSpeedBoostActive = false;
                        Speed /= _speedMultiplier; // Revert speed back to normal
                    }
                }
            }
            _direction = data.Direction;
            
            if (!GameManager.GamePlaying || Mathf.Approximately(_direction, 0f))
            {
                return;
            }
            
            var yMovement = _direction * Speed * Runner.DeltaTime;
            var targetPos = _rb.position + (Vector2.up * yMovement);
            targetPos.y = Mathf.Clamp(targetPos.y, -BoundY, BoundY);

            _rb.MovePosition(targetPos);
       }
        public void ActivateSpeedBoost()
        {
            _isSpeedBoostActive = true;
            _speedBoostTimer = _speedBoostDuration;
            Speed *= _speedMultiplier; // Increase speed
        }
    }
}
