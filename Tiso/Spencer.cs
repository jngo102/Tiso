using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Logger = Modding.Logger;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

using UnityEngine.SceneManagement;
using ActionData = On.HutongGames.PlayMaker.ActionData;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Tiso
{
    internal class Spencer : MonoBehaviour
    {
        private const int AttunedHealth = 1250;
        private const int AscendedHealth = 1700;

        private const float leftX = 52.3f;
        private const float rightX = 70.9F;
        private const float groundY = 5.0F;
        private const float jumpY = 13.5F;
        
        private AudioSource _audio;
        private CustomAnimator _anim;
        private PlayMakerFSM _control;
        private PlayMakerFSM _dreamNail;
        private HealthManager _hm;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private EnemyHitEffectsUninfected _hitEff;
        private EnemyDeathEffectsUninfected _deathEff;

        private Sprite[] _tisoSprites;
        private Sprite[] _tisoSpritesGodhome;
        private Shader _shader;
        
        private Recoil _recoil;
        
        private Random _rand;

        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Control");
            _dreamNail = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
            Log("Adding Components");
            AddComponents();
        }

        private IEnumerator Start()
        { 
            yield return null;

            while (HeroController.instance == null) yield return null;
            
            Log("Pos: " + transform.position);
            
            Log("Spencer Start");

            ActivateTiso();
            
            Log("Adding SpriteRenderer");
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.enabled = true;
            Log("Changing Material");
            _sr.material = new Material(_shader);
            Log("SpriteRenderer null? " + (_sr == null));
            
            Log("Starting TisoJump");
            TisoJump();

            gameObject.PrintSceneHierarchyTree();
        }

        private IEnumerator PlayIdle()
        {
            Log("Playing Idle Animation");
            StartCoroutine(_anim.Play("Idle", true, 0.075f));

            yield return null;
        }
        
        private void AddAnimations()
        {
            List<Sprite> idleSprites = new List<Sprite>()
            {
                FindSprite(_tisoSprites, "Idle0"),
                FindSprite(_tisoSprites, "Idle1"),
                FindSprite(_tisoSprites, "Idle2"),
                FindSprite(_tisoSprites, "Idle3"),
                FindSprite(_tisoSprites, "Idle4"),
                FindSprite(_tisoSprites, "Idle5"),
            };
            
            List<Sprite> landSprites = new List<Sprite>()
            {
                FindSprite(_tisoSpritesGodhome, "Land0"),
                FindSprite(_tisoSpritesGodhome, "Land1"),
                FindSprite(_tisoSpritesGodhome, "Land2"),
                FindSprite(_tisoSpritesGodhome, "Land1"),
            };
            
            List<Sprite> spinSprites = new List<Sprite>()
            {
                FindSprite(_tisoSpritesGodhome, "Spin0"),
                FindSprite(_tisoSpritesGodhome, "Spin1"),
                FindSprite(_tisoSpritesGodhome, "Spin2"),
            };
            
            _anim.animations.Add("Idle", idleSprites);
            _anim.animations.Add("Land", landSprites);
            _anim.animations.Add("Spin", spinSprites);
        }

        private Sprite FindSprite(Sprite[] spriteList, string spriteName)
        {
            foreach (Sprite sprite in spriteList)
            {
                if (sprite.name == spriteName)
                {
                    return sprite;
                }
            }
            return null;
        }
        
        private void ActivateTiso()
        {
            Log("Assigning Fields");
            AssignFields();

            Log("Retrieving Sprites");
            _tisoSprites = TisoSpencer.Bundle.LoadAssetWithSubAssets<Sprite>("TisoSprites");
            _tisoSpritesGodhome = TisoSpencer.Bundle.LoadAssetWithSubAssets<Sprite>("TisoSpritesGodhome");
            _shader = TisoSpencer.Bundle.LoadAsset<Shader>("Sprites-Diffuse_Flash");
            Log("Shader null? " + (_shader == null));

            Log("Adding Animations");
            AddAnimations();

            Log("Setting Values Based on Difficulty");
            SetValuesByDifficulty();
        }

        void Update()
        {
            Vector2 pos = transform.position;
            if (pos.y < groundY) pos.y = groundY;
            if (pos.x < leftX) pos.x = leftX;
            if (pos.x < rightX) pos.x = rightX;
        }

        private void AddComponents()
        {
            Log("Setting Active");
            gameObject.SetActive(true);
            Log("Changing layer");
            gameObject.layer = 11;
            Log("Adding AudioSource");
            _audio = gameObject.GetOrAddComponent<AudioSource>();
            Log("Adding Rigidbody2D");
            _rb = gameObject.GetOrAddComponent<Rigidbody2D>();
            Log("Making Rigidbody kinematic");
            _rb.isKinematic = true;
            Log("Making collision detection mode continuous");
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Log("Adding Custom Animator");
            _anim = gameObject.GetOrAddComponent<CustomAnimator>();
            Log("Adding BoxCollider2D");
            BoxCollider2D hitbox = gameObject.GetOrAddComponent<BoxCollider2D>();
            hitbox.enabled = true;
            Log("Changing Hitbox Size");
            hitbox.size = new Vector2(1, 2);
            Log("Changing Hitbox Offset");
            hitbox.offset = new Vector2(-0.5f, 0);
            Log("Adding Debug Colliders");
            gameObject.AddComponent<DebugColliders>();
            Log("Adding DamageHero");
            gameObject.GetOrAddComponent<DamageHero>().enabled = true;

            Log("Adding HealthManager");
            _hm = gameObject.GetOrAddComponent<HealthManager>();
            _hm.enabled = true;

            Log("Adding Recoil");
            _recoil = gameObject.GetOrAddComponent<Recoil>();
            _recoil.enabled = true;

            Log("Adding EnemyHitEffectsUninfected");
            _hitEff = gameObject.GetOrAddComponent<EnemyHitEffectsUninfected>();
            _hitEff.enabled = true;
            
            Log("Adding EnemyDeathEffectsUninfected");
            _deathEff = gameObject.GetOrAddComponent<EnemyDeathEffectsUninfected>();
            _deathEff.enabled = true;
            
            Log("Adding EnemyDreamnailReaction");
            gameObject.GetOrAddComponent<EnemyDreamnailReaction>().enabled = true;

            Log("Adding ExtraDamageable");
            gameObject.GetOrAddComponent<ExtraDamageable>().enabled = true;

            Log("Destroying tk2dSpriteAnimator");
            Destroy(gameObject.GetComponent<tk2dSpriteAnimator>());
            
            Log("Destroying tk2dSprite");
            Destroy(gameObject.GetComponent<tk2dSprite>());
            
            Log("Destroying MeshFilter");
            Destroy(gameObject.GetComponent<MeshFilter>());
            
            Log("Destroying MeshRenderer");
            Destroy(gameObject.GetComponent<MeshRenderer>());
        }

        private void AssignFields()
        {
            Log("Assigning HealthManager Fields");
            HealthManager hornetHealth = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<HealthManager>();
            foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name.Contains("Prefab")))
            {
                fi.SetValue(_hm, fi.GetValue(hornetHealth));
            }

            Log("Assigning EnemyHitEffectsUninfected Fields");
            EnemyHitEffectsUninfected hornetHitEffects = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<EnemyHitEffectsUninfected>();
            foreach (FieldInfo fi in typeof(EnemyHitEffectsUninfected).GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.CreateInstance | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.OptionalParamBinding | BindingFlags.PutDispProperty | BindingFlags.SuppressChangeType | BindingFlags.PutRefDispProperty))
            {
                Log("FieldInfo: " + fi);
                fi.SetValue(_hitEff, fi.GetValue(hornetHitEffects));
            }
            
            Log("Assigning EnemyDeathEffectsUninfected Fields");
            EnemyDeathEffectsUninfected hornetDeathEffects = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<EnemyDeathEffectsUninfected>();
            foreach (FieldInfo fi in typeof(EnemyDeathEffectsUninfected).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                fi.SetValue(_deathEff, fi.GetValue(hornetDeathEffects));
            }
            
            Log("Assigning Recoil Fields");
            Recoil hornetRecoil = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<Recoil>();
            foreach (FieldInfo fi in typeof(Recoil).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name.Contains("Prefab")))
            {
                fi.SetValue(_recoil, fi.GetValue(hornetRecoil));
            }

            Log("Finished Assigning Fields");
        }
        
        private void SetValuesByDifficulty()
        {
            int bossLevel = BossSceneController.Instance.BossLevel;
            if (bossLevel > 0)
            {
                _hm.hp = AscendedHealth;
            }
            else
            {
                _hm.hp = AttunedHealth;
            }
        }

        private List<string> validColliders = new List<string>
        {
            "Slash", "AltSlash", "DownSlash", "UpSlash", "Hit L", "Hit R", "Hit U", "Hit D", "Great Slash",
            "Dash Slash", "Q Fall Damage", "Fireball2 Spiral(Clone)", "Enemy Damager", "Shield", 
            "Grubberfly BeamL(Clone)", "Grubberfly BeamR(Clone)", "Grubberfly BeamU(Clone)", "Grubberfly BeamD(Clone)", 
            "Damager", "Sharp Shadow", "Knight Dung Trail(Clone)", "Dung Explosion(Clone)", "Knight Spore Cloud(Clone)",
        };
        private void OnTriggerEnter2D(Collider2D collider)
        {
            Log("Collided with: " + collider.name);
            if (validColliders.Any(@string => collider.name.Contains(@string)))
            {
                StartCoroutine(FlashWhite(0.25f));
            }
            else if (collider.name == "Hitbox")
            {
                StartCoroutine(FlashWhite(1.5f));
            }
        }

        private IEnumerator FlashWhite(float time)
        {
            float flashAmount = 1.0f;
            Material material = _sr.material;
            while (flashAmount > 0)
            {
                material.SetFloat("_FlashAmount", flashAmount);
                flashAmount -= 0.01f;
                yield return new WaitForSeconds(0.01f * time);
            }
            yield return null;
        }
        
        private void TisoJump()
        {
            IEnumerator JumpAntic()
            {
                StartCoroutine(_anim.Play("Land", false, 0.1f));
                
                yield return new WaitForSeconds(0.5f);
                
                Log("Jump");
                StartCoroutine(Jump());
            }

            IEnumerator Jump()
            {
                StartCoroutine(_anim.Play("Spin", true, 0.1f));
                _rb.velocity = new Vector2(-2.0f, 30f);
                
                yield return null;
                
                Log("Jumping");
                StartCoroutine(Jumping());
            }

            IEnumerator Jumping()
            {
                float yVel = _rb.velocity.y;
                while (transform.position.y >= groundY || _rb.velocity.y > 0)
                {
                    yVel = _rb.velocity.y;
                    _rb.velocity += Vector2.down * 4.0f;
                    yield return new WaitForSeconds(1.0f / 15);
                }
                Log("Exited while loop");

                yield return null;
                
                Log("Land");
                StartCoroutine(Land());
            }

            IEnumerator Land()
            {
                _rb.velocity = Vector2.zero;
                StartCoroutine(_anim.Play("Land", false, 0.1f));
                
                yield return new WaitForSeconds(0.5f);
                
                Log("Jump Recover");
                StartCoroutine(JumpRecover());
            }

            IEnumerator JumpRecover()
            {
                StartCoroutine(_anim.Play("Idle", true, 0.1f));
                yield return null;
            }

            Log("Jump Antic");
            StartCoroutine(JumpAntic());
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Log("Collision: " + collision.collider.name);
            if (collision.collider.gameObject.layer == 8 && _rb.velocity.y < 0 && transform.position.y < 6.0)
            {
                Log("Land");
            }
        }
        
        public static void Log(object message) => Logger.Log("[Spencer]" + message);
    }
    
}