using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;    
using ModCommon;
using Modding;
using Logger = Modding.Logger;
using UnityEngine;
using Random = System.Random;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Tiso
{
    internal class Spencer : MonoBehaviour
    {
        private const int AttunedHealth = 1250;
        private const int AscendedHealth = 1700;
        private const float AnimFPS = 1.0f / 12;

        private TisoAnimator _anim;
        private TisoAudio _audio;
        private BoxCollider2D _collider;
        private PlayMakerFSM _control;
        private HealthManager _hm;
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private TisoAttacks _attacks;
        private EnemyHitEffectsUninfected _hitEff;
        private EnemyDeathEffectsUninfected _deathEff;

        private Recoil _recoil;
        
        private Random _rand;

        private void Awake()
        {
            Log("Awake");
            GameObject go = gameObject;
            _control = go.LocateMyFSM("Control");
            go.SetActive(true);
            go.layer = 11;

            GetAssets();
            Log("Adding Components");
            AddComponents();
            
            _hm.OnDeath += DeathHandler;
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
            
            ActivateTiso();
            
            StartCoroutine(PlayIdle());

            yield return new WaitForSeconds(1.0f);
            
            Log("Starting TisoJump");
            _attacks.TisoJump();
            
            gameObject.GetOrAddComponent<DebugColliders>();
            
            gameObject.PrintSceneHierarchyTree();
        }
        
        private void DeathHandler()
        {
            Log("Starting Death Sequence");
            Vector2 position = transform.position + Vector3.down * 1.0f;
            Quaternion rotation = Quaternion.identity;
            GameObject deadTiso = Instantiate(new GameObject("Tiso Corpse"), position, rotation);
            deadTiso.AddComponent<SpriteRenderer>().sprite = FindSprite(_tisoSpritesGodhome, "Dead");
            deadTiso.AddComponent<TisoDeath>();
        }
        
        private IEnumerator PlayIdle()
        {
            Log("Playing Idle Animation");
            _anim.PlayAnimation("Idle", true);

            yield return null;
        }
        
        private void AddAnimations()
        {
            List<Sprite> idleSprites = new List<Sprite>
            {
                FindSprite(_tisoSprites, "Idle0"),
                FindSprite(_tisoSprites, "Idle1"),
                FindSprite(_tisoSprites, "Idle2"),
                FindSprite(_tisoSprites, "Idle3"),
                FindSprite(_tisoSprites, "Idle4"),
                FindSprite(_tisoSprites, "Idle5"),
            };
            
            List<Sprite> jumpAnticSprites = new List<Sprite>
            {
                FindSprite(_tisoSpritesGodhome, "Land2"),
                FindSprite(_tisoSpritesGodhome, "Land0"),
                FindSprite(_tisoSpritesGodhome, "Land1"),
            };

            List<Sprite> landSprites = new List<Sprite>
            {
                FindSprite(_tisoSpritesGodhome, "Land0"),
                FindSprite(_tisoSpritesGodhome, "Land1"),
                FindSprite(_tisoSpritesGodhome, "Land2"),
            };
            
            List<Sprite> spinSprites = new List<Sprite>()
            {
                FindSprite(_tisoSpritesGodhome, "Spin0"),
                FindSprite(_tisoSpritesGodhome, "Spin1"),
                FindSprite(_tisoSpritesGodhome, "Spin2"),
            };
            
            _anim.animations.Add("Idle", idleSprites);
            _anim.animations.Add("Jump Antic", jumpAnticSprites);
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
        
        private Sprite[] _tisoSprites;
        private Sprite[] _tisoSpritesGodhome;
        private void ActivateTiso()
        {
            AssignFields();
            AddAnimations();
            SetValuesByDifficulty();
        }
        
        private void GetAssets()
        {
            _tisoSprites = TisoSpencer.TisoAssetsBundle.LoadAssetWithSubAssets<Sprite>("TisoSprites");
            _tisoSpritesGodhome = TisoSpencer.TisoAssetsBundle.LoadAssetWithSubAssets<Sprite>("TisoSpritesGodhome");
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

            _hm = gameObject.GetOrAddComponent<HealthManager>();
            _hm.enabled = true;

            _recoil = gameObject.GetOrAddComponent<Recoil>();
            _recoil.enabled = true;
            _recoil.SetRecoilSpeed(15f);

            _hitEff = gameObject.GetOrAddComponent<EnemyHitEffectsUninfected>();
            _hitEff.enabled = true;
            
            _deathEff = gameObject.GetOrAddComponent<EnemyDeathEffectsUninfected>();
            _deathEff.enabled = true;
            
            EnemyDreamnailReaction dreamNailReaction = gameObject.GetOrAddComponent<EnemyDreamnailReaction>();
            dreamNailReaction.enabled = true;
            dreamNailReaction.SetConvoTitle("Tiso_2");

            gameObject.GetOrAddComponent<ExtraDamageable>().enabled = true;

            Destroy(GetComponent<tk2dSpriteAnimator>());
            Destroy(GetComponent<tk2dSprite>());
            Destroy(GetComponent<MeshFilter>());
            Destroy(GetComponent<MeshRenderer>());
            Destroy(gameObject.FindGameObjectInChildren("Shield"));
            Destroy(gameObject.FindGameObjectInChildren("Corpse"));
        }

        private void AddCustomComponents()
        {
            _anim = gameObject.AddComponent<TisoAnimator>();
            
            _audio = gameObject.AddComponent<TisoAudio>();
            
            _attacks = gameObject.AddComponent<TisoAttacks>();
            _attacks.enabled = true;
        }
        
        private void AssignFields()
        {
            HealthManager hornetHealth = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<HealthManager>();
            foreach (FieldInfo fi in typeof(HealthManager).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name.Contains("Prefab")))
            {
                fi.SetValue(_hm, fi.GetValue(hornetHealth));
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
            
            Recoil hornetRecoil = TisoSpencer.PreloadedGameObjects["Hornet"].GetComponent<Recoil>();
            foreach (FieldInfo fi in typeof(Recoil).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.Name.Contains("Prefab")))
            {
                fi.SetValue(_recoil, fi.GetValue(hornetRecoil));
            }
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
            if (validColliders.Any(@string => collider.name.Contains(@string)))
            {
                StopCoroutine(FlashWhite(0.25f));
                StartCoroutine(FlashWhite(0.25f));
            }
            else if (collider.name == "Hitbox")
            {
                StopCoroutine(FlashWhite(1.5f));
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

        private void OnDestroy()
        {
            _hm.OnDeath -= DeathHandler;
            ModHooks.Instance.LanguageGetHook -= OnLangGet;
        }

        public static void Log(object message) => Logger.Log("[Spencer]" + message);
    }
    
}