using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

// Taken and modified from https://github.com/SalehAce1/PaleChampion/blob/master/PaleChampion/PaleChampion/CustomAnimator.cs
namespace Tiso
{
    internal class TisoAnimator : MonoBehaviour
    {
        public Dictionary<string, List<Sprite>> animations = new Dictionary<string, List<Sprite>>();
        
        public static Sprite[] TisoSprites;
        public static Sprite[] TisoSpritesCustom;
        public static Sprite[] TisoSpritesGodhome;
        private Shader _shader;
        private SpriteRenderer _sr;
        private void Awake()
        {
            TisoSprites = TisoSpencer.TisoAssetsBundle.LoadAssetWithSubAssets<Sprite>("TisoSprites");
            TisoSpritesCustom = TisoSpencer.TisoAssetsBundle.LoadAssetWithSubAssets<Sprite>("TisoSpritesCustom");
            TisoSpritesGodhome = TisoSpencer.TisoAssetsBundle.LoadAssetWithSubAssets<Sprite>("TisoSpritesGodhome");
            _shader = TisoSpencer.TisoAssetsBundle.LoadAsset<Shader>("Flash Shader");
            AddAnimations();
            
            _sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        private void Start()
        {
            _sr.material = new Material(_shader);
        }
        
        private void AddAnimations()
        {
            List<Sprite> bombThrowAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "BombThrowAntic0"),
                FindSprite(TisoSpritesCustom, "BombThrowAntic0"),
                FindSprite(TisoSpritesCustom, "BombThrowAntic1"),
                FindSprite(TisoSpritesCustom, "BombThrowAntic1"),
            };
            
            List<Sprite> bombThrowSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "BombThrow0"),
            };
            
            List<Sprite> dabAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "DabAntic0"),
                FindSprite(TisoSpritesCustom, "DabAntic1"),
            };
            
            List<Sprite> dabSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Dab0"),
            };
            
            List<Sprite> dashAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "DashAntic0"),
                FindSprite(TisoSpritesCustom, "DashAntic1"),
                FindSprite(TisoSpritesCustom, "DashAntic2"),
                FindSprite(TisoSpritesCustom, "DashAntic3"),
                FindSprite(TisoSpritesCustom, "DashAntic4"),
                FindSprite(TisoSpritesCustom, "DashAntic5"),
            };

            List<Sprite> dashingSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Dashing0"),
                FindSprite(TisoSpritesCustom, "Dashing1"),
            };
            
            List<Sprite> dashRecoverSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "DashRecover0"),
                FindSprite(TisoSpritesGodhome, "Spinning0"),
                FindSprite(TisoSpritesGodhome, "Spinning1"),
                FindSprite(TisoSpritesGodhome, "Spinning2"),
                FindSprite(TisoSpritesCustom, "DashRecover4"),
            };
            
            List<Sprite> evadeAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "EvadeAntic0"),
            };

            List<Sprite> evadingSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Evading0"),
                FindSprite(TisoSpritesCustom, "Evading1"),
            };
            
            List<Sprite> evadeLandSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "EvadeAntic0"),
            };
            
            List<Sprite> idleSprites = new List<Sprite>
            {
                FindSprite(TisoSprites, "Idle0"),
                FindSprite(TisoSprites, "Idle1"),
                FindSprite(TisoSprites, "Idle2"),
                FindSprite(TisoSprites, "Idle3"),
                FindSprite(TisoSprites, "Idle4"),
                FindSprite(TisoSprites, "Idle5"),
            };
            
            List<Sprite> shieldSpinSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "ShieldSpin0"),
                FindSprite(TisoSpritesCustom, "ShieldSpin1"),
                FindSprite(TisoSpritesCustom, "ShieldSpin2"),
            };
            
            List<Sprite> jumpAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesGodhome, "Land2"),
                FindSprite(TisoSpritesGodhome, "Land0"),
                FindSprite(TisoSpritesGodhome, "Land1"),
            };

            List<Sprite> landSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesGodhome, "Land0"),
                FindSprite(TisoSpritesGodhome, "Land1"),
                FindSprite(TisoSpritesGodhome, "Land2"),
            };
            
            List<Sprite> raiseToIdleSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "RaiseToIdle0"),
                FindSprite(TisoSpritesCustom, "RaiseToIdle1"),
            };
            
            List<Sprite> slashAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "SlashAntic0"),
                FindSprite(TisoSpritesCustom, "SlashAntic1"),
                FindSprite(TisoSpritesCustom, "SlashAntic2"),
                FindSprite(TisoSpritesCustom, "SlashAntic3"),
                FindSprite(TisoSpritesCustom, "SlashAntic4"),
            };
            
            List<Sprite> slash1Sprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Slash1_0"),
            };

            List<Sprite> slash2AnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Slash2Antic0"),
            };
            
            List<Sprite> slash2Sprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Slash2_0"),
            };
            
            List<Sprite> slashRecoverSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "SlashAntic1"),
                FindSprite(TisoSpritesCustom, "SlashAntic0"),
            };
            
            List<Sprite> spinningSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesGodhome, "Spinning0"),
                FindSprite(TisoSpritesGodhome, "Spinning1"),
                FindSprite(TisoSpritesGodhome, "Spinning2"),
            };

            List<Sprite> shieldThrowAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesGodhome, "Land2"),
                FindSprite(TisoSpritesCustom, "ShieldSpin0"),
                FindSprite(TisoSpritesCustom, "ShieldSpin1"),
                FindSprite(TisoSpritesCustom, "ShieldSpin2"),
                FindSprite(TisoSpritesCustom, "ShieldSpin0"),
                FindSprite(TisoSpritesCustom, "ShieldSpin1"),
                FindSprite(TisoSpritesCustom, "ShieldSpin2"),
            };
            
            List<Sprite> shieldThrowRecoverSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "ShieldThrowRecover0"),
            };
            
            List<Sprite> shieldThrowSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "ShieldThrow0"),
            };
            
            List<Sprite> shieldThrowWaitSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "ShieldThrowWait0"),
                FindSprite(TisoSpritesCustom, "ShieldThrowWait1"),
                FindSprite(TisoSpritesCustom, "ShieldThrowWait2"),
            };

            List<Sprite> turnSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Turn0"),
            };
            
            List<Sprite> upslashAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "UpslashAntic0"),
                FindSprite(TisoSpritesCustom, "UpslashAntic0"),
            };
            
            List<Sprite> upslashSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Upslash0"),
            };
            
            animations.Add("Bomb Throw Antic", bombThrowAnticSprites);
            animations.Add("Bomb Throw", bombThrowSprites);
            
            animations.Add("Dab Antic", dabAnticSprites);
            animations.Add("Dab", dabSprites);
            
            animations.Add("Dash Antic", dashAnticSprites);
            animations.Add("Dashing", dashingSprites);
            animations.Add("Dash Recover", dashRecoverSprites);

            animations.Add("Evade Antic", evadeAnticSprites);
            animations.Add("Evading", evadingSprites);
            animations.Add("Evade Land", evadeLandSprites);
            
            animations.Add("Idle", idleSprites);
            
            animations.Add("Shield Spin", shieldSpinSprites);
            
            animations.Add("Jump Antic", jumpAnticSprites);
            animations.Add("Land", landSprites);
            animations.Add("Spinning", spinningSprites);
            
            animations.Add("Raise to Idle", raiseToIdleSprites);
            
            animations.Add("Slash Antic", slashAnticSprites);
            animations.Add("Slash 1", slash1Sprites);
            animations.Add("Slash 2 Antic", slash2AnticSprites);
            animations.Add("Slash 2", slash2Sprites);
            animations.Add("Slash Recover", slashRecoverSprites);
            
            animations.Add("Shield Throw Antic", shieldThrowAnticSprites);
            animations.Add("Shield Throw", shieldThrowSprites);
            animations.Add("Shield Throw Wait", shieldThrowWaitSprites);
            animations.Add("Shield Throw Recover", shieldThrowRecoverSprites);
            
            animations.Add("Turn", turnSprites);
            
            animations.Add("Upslash Antic", upslashAnticSprites);
            animations.Add("Upslash", upslashSprites);
        }

        public static Sprite FindSprite(Sprite[] spriteList, string spriteName)
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

        public float GetAnimDuration(string animName, float delay = AnimFPS)
        {
            int numFrames = animations[animName].Count;
            return numFrames * delay;
        }
        
        private const float AnimFPS = 1.0f / 12;
        private Coroutine _currentAnimation;
        public void PlayAnimation(string animName, bool looping = false, float delay = AnimFPS)
        {
            IEnumerator Play()
            {
                List<Sprite> animation = animations[animName];
                do
                {
                    foreach (var frame in animation)
                    {
                        _sr.sprite = frame;
                        yield return new WaitForSeconds(delay);
                    }
                } 
                while (looping);
            }
            
            if (_currentAnimation != null) StopCoroutine(_currentAnimation);
            _currentAnimation = StartCoroutine(Play());
        }

        private Coroutine _flashRoutine;
        public void FlashWhite(float time)
        {
            IEnumerator Flash()
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
            
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(Flash());
        }
        
        private static void Log(object message) => Modding.Logger.Log("[Tiso Animator] " + message);
    }
}