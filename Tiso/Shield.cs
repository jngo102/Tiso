using System;
using System.Collections;
using IL.HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;

namespace Tiso
{
    public class Shield : MonoBehaviour
    {
        private const float ThrowVelocity = 10.0f;
        private const float Gravity = 5.0f;
        private const float BounceVelocityY = 10.0f;
        private const float LeftY = 51.0f;
        private const float RightY = 71.0f;

        public int direction;
        
        private BoxCollider2D _collider;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private GameObject _tiso;
        private Vector2 _tisoPos;

        public delegate void ShieldReturnedToTisoEventHandler();
        public event ShieldReturnedToTisoEventHandler ReturnedToTiso;

        protected virtual void OnReturnedToTiso()
        {
            if (ReturnedToTiso != null) ReturnedToTiso();
        }
        
        private void Awake()
        {
            gameObject.SetActive(true);
            gameObject.layer = 11;
            
            gameObject.AddComponent<DebugColliders>();
            gameObject.AddComponent<DamageHero>().damageDealt = 1;
            gameObject.AddComponent<TinkEffect>();
            gameObject.AddComponent<TinkSound>();
            
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            _collider = gameObject.AddComponent<BoxCollider2D>();
            _collider.enabled = true;
            _collider.size = new Vector2(2.0f, 0.75f);
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _tiso = GameObject.Find("Tiso Boss");
            _tisoPos = _tiso.transform.position;

            _sr.sprite = TisoAnimator.FindSprite(TisoAnimator.TisoSpritesGodhome, "Shield");
        }

        private void Start()
        {
            StartCoroutine(Thrown());
        }

        private IEnumerator Thrown()
        {
            _rb.velocity = Vector2.right * ThrowVelocity * direction;

            while (transform.position.x >= LeftY && transform.position.x <= RightY)
            {
                yield return null;
            }

            Log("Hit Wall");

            StartCoroutine(Bounce());
        }

        private IEnumerator Bounce()
        {
            float time = (2 * BounceVelocityY) / Gravity;
            float xVel = (_tisoPos.x - transform.position.x) / time;
            _rb.velocity = new Vector2(xVel, BounceVelocityY);
            
            Log("Initial Velocity: " + _rb.velocity);
            
            yield return null;

            StartCoroutine(Falling());
        }

        private IEnumerator Falling()
        {
            float x1 = transform.position.x;
            float x2 = _tisoPos.x;
            float y1 = transform.position.y;
            float y2 = _tisoPos.y;

            float distance = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
            while (distance > 2.0f)
            {
                x1 = transform.position.x;
                y1 = transform.position.y;
                distance = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
                Log("Distance: " + distance);
                _rb.velocity += Vector2.down * Gravity * 0.0GIT1f;
                yield return new WaitForSeconds(0.01f);
            }
            
            OnReturnedToTiso();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Log("Collided with: " + collision.collider.gameObject.name);
        }

        private static void Log(object message) => Modding.Logger.Log("[Shield]: " + message);
    }
}