using System.Collections;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;

namespace Tiso
{
    public class Bomb : MonoBehaviour
    {
        private const float Lifetime = 1.0f;
        private const float ThrowHeight = 30.0f;
        private const float Gravity = 60.0f;

        private CircleCollider2D _cc;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _cc = gameObject.AddComponent<CircleCollider2D>();
            _rb = gameObject.AddComponent<Rigidbody2D>();
            _sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        private IEnumerator Start()
        {
            yield return null;
            
            while (HeroController.instance == null) yield return null;

            float heroX = HeroController.instance.transform.position.x;
            float bombX = transform.position.x;
            float velX = (heroX - bombX) / Lifetime;

            _cc.radius = 0.2f;
            _cc.isTrigger = true;
            
            _rb.isKinematic = true;
            _rb.velocity = new Vector2(velX, ThrowHeight);

            _sr.sprite = TisoAnimator.FindSprite(TisoAnimator.TisoSpritesCustom, "Bomb");

            gameObject.AddComponent<DebugColliders>();
            gameObject.AddComponent<NonBouncer>();
            
            yield return new WaitForSeconds(Lifetime);
            
            Explode();
        }

        private void FixedUpdate()
        {
            _rb.velocity += Vector2.down * Gravity * Time.deltaTime;
        }
        
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.name == "Knight")
            {
                Explode();
            }
        }
        
        private void Explode()
        {
            GameObject oomaCorpse = Instantiate(TisoSpencer.PreloadedGameObjects["Ooma Corpse"], Vector2.zero, Quaternion.identity);
            oomaCorpse.SetActive(true);
            PlayMakerFSM corpseFSM = oomaCorpse.LocateMyFSM("corpse");
            GameObject explosion = corpseFSM.GetAction<SpawnObjectFromGlobalPool>("Instant Explosion").gameObject.Value;
            GameObject explosionInstance = Instantiate(explosion, transform.position, Quaternion.identity);
            explosionInstance.SetActive(true);
            foreach (Transform childTransform in explosionInstance.transform)
            {
                GameObject child = childTransform.gameObject;
                child.AddComponent<NonBouncer>().SetActive(true);
            }
            Destroy(explosionInstance.LocateMyFSM("damages_enemy"));
            Destroy(gameObject);
        }
        
        private static void Log(object message) => Modding.Logger.Log("[Bomb]: " + message);
    }
}