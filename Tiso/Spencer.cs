using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Logger = Modding.Logger;
using UnityEngine;
using Random = System.Random;

namespace Tiso
{
    internal class Spencer : MonoBehaviour
    {
        private AudioSource _audio;
        private CustomAnimator _anim;
        private PlayMakerFSM _control;
        private PlayMakerFSM _dreamNail;
        private HealthManager _hm;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private tk2dSprite _tkSprite;
        private tk2dSpriteAnimator _tkSpriteAnim;

        private SpriteFlash spriteFlash;
        private HitInstance hitInstance;
        
        private Random _rand;

        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Control");
            _dreamNail = HeroController.instance.gameObject.LocateMyFSM("Dream Nail");
            Log("GO name: " + gameObject.name);
        }

        private IEnumerator Start()
        {
            yield return null;

            while (HeroController.instance == null) yield return null;
            
            Log("Spencer Start");

            ActivateTiso();

            yield return new WaitForSeconds(2.0f);

            //_rb.velocity = new Vector2(-0.5f, 0.5f);
            //StartCoroutine(_anim.Play("Spin", true, 0.1f));
        }

        private void AddAnimations()
        {
            _anim.animations.Add("Idle", TisoSpencer.Sprites.GetRange(1, 4));
            _anim.animations.Add("Land", TisoSpencer.Sprites.GetRange(5, 4));
            _anim.animations.Add("Spin", TisoSpencer.Sprites.GetRange(9, 3));
        }

        private void ActivateTiso()
        {
            Log("Setting Active");
            gameObject.SetActive(true);
            Log("Changing layer");
            gameObject.layer = 11;
            Log("Adding AudioSource");
            _audio = gameObject.AddComponent<AudioSource>();
            Log("Adding Rigidbody2D");
            _rb = gameObject.AddComponent<Rigidbody2D>();
            Log("Making Rigidbody kinematic");
            _rb.isKinematic = true;
            Log("Adding Custom Animator");
            _anim = gameObject.AddComponent<CustomAnimator>();
            Log("Adding BoxCollider2D");
            BoxCollider2D hitbox = gameObject.AddComponent<BoxCollider2D>();
            hitbox.enabled = true;
            Log("Changing Hitbox Size");
            hitbox.size = new Vector2(1, 2);
            //Log("Adding Debug Colliders");
            //gameObject.AddComponent<DebugColliders>();
            Log("Adding DamageHero");
            hitbox.gameObject.AddComponent<DamageHero>();
            Log("Adding HealthManager");
            _hm = gameObject.AddComponent<HealthManager>();
            _hm.hp = 1000;
            
            Log("Adding SpriteFlash");
            //spriteFlash = gameObject.AddComponent<SpriteFlash>();
            //spriteFlash.enabled = true;
            Log("Adding EnemyDreamnailReaction");
            EnemyDreamnailReaction e = gameObject.AddComponent<EnemyDreamnailReaction>();
            e.MakeReady();
            e.enabled = true;

            Log("Adding SpriteRenderer");
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sprite = TisoSpencer.Sprites[12];
            Log("Changing Material");
            _sr.material = new Material(Shader.Find("Sprites/Diffuse Flash"));
            Log("Material: " + _sr.material);

            Log("Adding tk2dSprite");
            _tkSprite = gameObject.AddComponent<tk2dSprite>();
            _tkSprite.enabled = true;
            
            Log("Adding tk2dSpriteAnimator");
            _tkSpriteAnim = gameObject.AddComponent<tk2dSpriteAnimator>();

            Log("Adding Animations");
            AddAnimations();

            Log("Playing Idle Animation");
            StartCoroutine(_anim.Play("Idle", true, 0.1f));
            
            gameObject.PrintSceneHierarchyTree();
        }

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.name == "Nail")
            {
                
            }
        }
        
        void Update()
        {
            
        }
        

        private void TisoJump()
        {
            IEnumerator JumpAntic()
            {
                yield return null;
            }

            IEnumerator Jump()
            {
                yield return null;
            }

            IEnumerator Land()
            {
                yield return null;
            }

            StartCoroutine(JumpAntic());
            StartCoroutine(Jump());
            StartCoroutine(Land());
        }
        
        public static void Log(object message) => Logger.Log("[Spencer]" + message);
    }
    
}