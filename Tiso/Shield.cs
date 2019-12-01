using System;
using System.Collections;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;

namespace Tiso
{
    public class Shield : MonoBehaviour
    {
        private const float BounceVelocityY = 30.0f;
        private const float Gravity = 120.0f;
        private const float LeftX = 51.8f;
        private const float RightX = 72.0f;
        private const float ThrowVelocity = 60.0f;
        private const float TimeInAir = 0.5f;

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
            GameObject go = gameObject;
            go.SetActive(true);
            go.layer = 12;
            
            go.AddComponent<DebugColliders>();
            go.AddComponent<DamageHero>().damageDealt = 1;
            go.AddComponent<TinkEffect>();
            go.AddComponent<TinkSound>();

            _audio = go.AddComponent<AudioSource>();
            _rb = go.AddComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            _collider = go.AddComponent<BoxCollider2D>();
            _collider.enabled = true;
            _collider.size = new Vector2(2.0f, 0.75f);
            _sr = go.AddComponent<SpriteRenderer>();
            _tiso = GameObject.Find("Tiso Boss");
            _tisoPos = _tiso.transform.position;

            _sr.sprite = TisoAnimator.FindSprite(TisoAnimator.TisoSpritesGodhome, "Shield");
        }

        private bool _bouncing;
        private void FixedUpdate()
        {
            if (_bouncing)
            {
                _rb.velocity += Vector2.down * Gravity * Time.deltaTime;   
            }
        }
        
        private void Start()
        {
            StartCoroutine(Thrown());
        }

        private IEnumerator Thrown()
        {
            _rb.velocity = Vector2.right * ThrowVelocity * direction;

            while (transform.position.x >= LeftX && transform.position.x <= RightX) yield return null;

            StartCoroutine(Bounce());
        }
        
        private IEnumerator Bounce()
        {
            _bouncing = true;
            Vector2 pos = transform.position; 
            float xVel = (_tisoPos.x - pos.x) / TimeInAir;
            _rb.velocity = new Vector2(xVel, BounceVelocityY);
            
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
                yield return null;
            }

            OnReturnedToTiso();
        }

        private static void Log(object message) => Modding.Logger.Log("[Shield]: " + message);
    }
}