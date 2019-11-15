using System;
using ModCommon;
using UnityEngine;

namespace Tiso
{
    public class Shield : MonoBehaviour
    {
        private const float ShieldVelocity = 15.0f;
        private const float Gravity = 20.0f;
        private const float BounceVelocityY = 50.0f;
        private const float LeftY = 51.0f;

        public int direction;
        
        private BoxCollider2D _collider;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private GameObject _tiso;
        private Vector2 _tisoPos;

        private void Awake()
        {
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            _collider = gameObject.AddComponent<BoxCollider2D>();
            _collider.enabled = true;
            _collider.size = new Vector2(2.0f, 0.75f);
            gameObject.AddComponent<DebugColliders>();
            gameObject.AddComponent<DamageHero>().enabled = true;
            try
            {
                gameObject.AddComponent<TinkEffect>();
            }
            catch (Exception e)
            {
                Log(e);
                throw;
            }
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _tiso = GameObject.Find("Tiso Boss(Clone)");
            _tisoPos = _tiso.transform.position;

            _sr.sprite = TisoAnimator.FindSprite(TisoAnimator.TisoSpritesGodhome, "Shield");
        }

        private void Start()
        {
            _rb.velocity = Vector2.left * ShieldVelocity * direction;
        }

        private void Update()
        {
            
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            Log("Collided with: " + collision.collider.gameObject.name);
        }

        private static void Log(object message) => Modding.Logger.Log("[Shield]: " + message);
    }
}