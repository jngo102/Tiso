using System;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;

namespace Tiso
{
    public class Shield : MonoBehaviour
    {
        private const float ThrowVelocity = 15.0f;
        private const float Gravity = 15.0f;
        private const float LeftX = 51.8f;
        private const float RightX = 72.0f;
        private const float TimeInAir = 1.0f;
        private const float MaxHeight = 5.0f;

        public int direction;

        private AudioSource _audio;
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
            gameObject.layer = 12;
            
            gameObject.AddComponent<DebugColliders>();
            gameObject.AddComponent<DamageHero>().damageDealt = 1;
            gameObject.AddComponent<TinkEffect>();
            gameObject.AddComponent<TinkSound>();

            _audio = gameObject.AddComponent<AudioSource>();
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

            while (transform.position.x >= LeftX && transform.position.x <= RightX)
            {
                yield return null;
            }        

            Log("Hit Wall");

            StartCoroutine(Bounce());
        }
        
        private IEnumerator Bounce()
        {
            Vector2 pos = transform.position; 
            float dx = _tisoPos.x - pos.x;
            float dy = _tisoPos.y - pos.y;
            float time = Mathf.Sqrt(2 * MaxHeight / Gravity) + Mathf.Sqrt(2 * MaxHeight / Gravity);
            float xVel = dx / time;
            float yVel = Mathf.Sqrt(2 * Gravity * MaxHeight);
            _rb.velocity = new Vector2(xVel, yVel);

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
            while (distance > 5.0f)
            {
                x1 = transform.position.x;
                y1 = transform.position.y;
                distance = Mathf.Sqrt(Mathf.Pow(x2 - x1, 2) + Mathf.Pow(y2 - y1, 2));
                Log("Distance: " + distance + ", Velocity: " + _rb.velocity);
                _rb.velocity += Vector2.down * Gravity * 0.01f;
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