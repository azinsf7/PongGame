using System.Collections;
using Fusion;
using NoMossStudios.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FusionPong.Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : NetworkBehaviour
    {
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] SpriteRenderer _spriteRenderer;
        private const float Speed = 10f;
        private const float MaxSpeed = 20f;
        private const float MinTime = 5f;
        private const float MaxTime = 15f;
        
        private Vector2 _cachedVelocity;
        private float ballResetGameTime;

        public bool HasScored { get; set; } = false;

        public override void Spawned()
        {
            base.Spawned();
            _rb ??= GetComponent<Rigidbody2D>();
        }
        
        public void ResetBall(float startDelay = 1f)
        {
            _spriteRenderer.enabled = false;
            _cachedVelocity = _rb.velocity;
            
            _rb.velocity = Vector2.zero;
            transform.position = Vector3.zero;

            if (startDelay > 0f)
                StartCoroutine(StartBall(startDelay));

            HasScored = false;
        }
    
        public void HideBall()
        {
            _rb.velocity = Vector2.zero;
            transform.position = new Vector3(99999f,99999f);
            _spriteRenderer.enabled = false;
        }

        public override void FixedUpdateNetwork()
        {
            if (_rb.velocity == Vector2.zero) return;
            
            var vel = _rb.velocity.normalized;
            if (Mathf.Abs(vel.x) < 0.25f)
                vel.x = 0.25f * vel.x < 0 ? -1f : 1f;

            _rb.velocity = vel * BallSpeed();
        }

        public IEnumerator StartBall(float delay)
        {
            yield return new WaitForEndOfFrame();
            _spriteRenderer.enabled = true;
            yield return new WaitForSecondsRealtime(delay);

            ballResetGameTime = Time.timeSinceLevelLoad;
            
            if (_cachedVelocity == Vector2.zero)
            {
                var xVel = Random.Range(.2f, .8f);
                if (Random.value > 0.5f)
                    xVel *= -1f;
    
                _cachedVelocity = new Vector2(xVel, 0f);
            }
                            
            var yVel = Random.Range(.1f, .9f);
            if (Random.value > 0.5f)
                yVel *= -1f;
            
            var startVel = new Vector2(_cachedVelocity.x, yVel);

            _rb.velocity = startVel.normalized * BallSpeed();
        }

        private float BallSpeed()
        {
            var time = Time.timeSinceLevelLoad - ballResetGameTime;
            return time switch
            {
                < MinTime => Speed,
                >= MaxTime => MaxSpeed,
                _ => time.Remap(MinTime, MaxTime, Speed, MaxSpeed)
            };
        }
    }
}

