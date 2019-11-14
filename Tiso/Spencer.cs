using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ModCommon;
using ModCommon.Util;
using Modding;
using Logger = Modding.Logger;
using UnityEngine;
using Random = System.Random;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Tiso
{
    internal class Spencer : MonoBehaviour
    {
        private BoxCollider2D _collider;
        private PlayMakerFSM _control;
        private HealthManager _hm;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private EnemyHitEffectsUninfected _hitEff;
        private EnemyDeathEffectsUninfected _deathEff;
        private Recoil _recoil;

        private void Awake()
        {
            GameObject go = gameObject;
            _control = go.LocateMyFSM("Control");
            go.SetActive(true);
            go.layer = 11;
            
            AddComponents();
            DestroyComponents();
            
            ModHooks.Instance.LanguageGetHook += OnLangGet;
        }

        private string OnLangGet(string key, string sheettitle)
        {
            switch (key)
            {
                case "Tiso_1":
                    return "I AM TISO!!!! 1";
                case "Tiso_2":
                    return "I AM TISO!!!! 2";
                case "Tiso_3":
                    return "I AM TISO!!!! 3";
                case "Tiso_4":
                    return "I AM TISO!!!! 4";
                default:
                    return Language.Language.GetInternal(key, sheettitle);
            }
        }

        
        private IEnumerator Start()
        { 
            yield return null;

            while (HeroController.instance == null) yield return null;
            
            Log("Adding Custom Components");
            AddCustomComponents();
            
            AssignFields();
            
            _anim.PlayAnimation("Idle", true);

            yield return new WaitForSeconds(1.0f);

            gameObject.GetOrAddComponent<DebugColliders>();

            gameObject.PrintSceneHierarchyTree();
        }
        
        private void Update()
        {
            
        }

        private void AddComponents()
        {
            _rb = gameObject.GetOrAddComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _collider = gameObject.GetOrAddComponent<BoxCollider2D>();
            _collider.enabled = true;
            _collider.size = new Vector2(1, 2);
            _collider.offset = new Vector2(-0.5f, 0);
            gameObject.GetOrAddComponent<DamageHero>().enabled = true;

            _recoil = gameObject.GetOrAddComponent<Recoil>();
            _recoil.enabled = true;
            _recoil.SetAttr<bool>("freezeInPlace", false);
            _recoil.SetAttr<bool>("stopVelocityXWhenRecoilingUp", true);
            _recoil.SetAttr<bool>("preventRecoilUp", false);
            _recoil.SetAttr<float>("recoilSpeedBase", 15f);
            _recoil.SetAttr<float>("recoilDuration", 0.15f);

            _hitEff = gameObject.GetOrAddComponent<EnemyHitEffectsUninfected>();
            _hitEff.enabled = true;
            
            _deathEff = gameObject.GetOrAddComponent<EnemyDeathEffectsUninfected>();
            _deathEff.enabled = true;
            
            EnemyDreamnailReaction dreamNailReaction = gameObject.GetOrAddComponent<EnemyDreamnailReaction>();
            dreamNailReaction.enabled = true;
            dreamNailReaction.SetConvoTitle("Tiso_2");

            gameObject.GetOrAddComponent<ExtraDamageable>().enabled = true;
        }

        private void DestroyComponents()
        {
            Destroy(GetComponent<tk2dSpriteAnimator>());
            Destroy(GetComponent<tk2dSprite>());
            Destroy(GetComponent<MeshFilter>());
            Destroy(GetComponent<MeshRenderer>());
            Destroy(gameObject.FindGameObjectInChildren("Shield"));
            Destroy(gameObject.FindGameObjectInChildren("Corpse"));
        }
        
        private TisoAnimator _anim;
        private TisoAudio _audio;
        private TisoAttacks _attacks;
        private PhaseControl _phaseCtrl;
        private void AddCustomComponents()
        {
            _anim = gameObject.AddComponent<TisoAnimator>();
            _audio = gameObject.AddComponent<TisoAudio>();
            _phaseCtrl = gameObject.AddComponent<PhaseControl>();
            _attacks = gameObject.AddComponent<TisoAttacks>();
        }
        
        private void AssignFields()
        {
            HealthManager hornetHealth = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<HealthManager>();
            foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name.Contains("Prefab")))
            {
                fi.SetValue(_phaseCtrl.hm, fi.GetValue(hornetHealth));
            }

            EnemyHitEffectsUninfected hornetHitEffects = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<EnemyHitEffectsUninfected>();
            foreach (FieldInfo fi in typeof(EnemyHitEffectsUninfected).GetFields(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.CreateInstance | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding | BindingFlags.FlattenHierarchy | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.IgnoreCase | BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.SetProperty | BindingFlags.OptionalParamBinding | BindingFlags.PutDispProperty | BindingFlags.SuppressChangeType | BindingFlags.PutRefDispProperty))
            {
                fi.SetValue(_hitEff, fi.GetValue(hornetHitEffects));
            }
            
            EnemyDeathEffectsUninfected hornetDeathEffects = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<EnemyDeathEffectsUninfected>();
            foreach (FieldInfo fi in typeof(EnemyDeathEffectsUninfected).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Default | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                fi.SetValue(_deathEff, fi.GetValue(hornetDeathEffects));
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
            if (validColliders.Any(@string => collider.name.Contains(@string)))
            {
                _anim.FlashWhite(0.25f);
            }
            else if (collider.name == "Hitbox")
            {
                _anim.FlashWhite(1.0f);
            }
        }

        private void OnDestroy()
        {
            ModHooks.Instance.LanguageGetHook -= OnLangGet;
        }

        public static void Log(object message) => Logger.Log("[Spencer]" + message);
    }
    
}