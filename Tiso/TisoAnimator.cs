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
            _shader = TisoSpencer.TisoAssetsBundle.LoadAsset<Shader>("Sprites-Diffuse_Flash");
            AddAnimations();
            
            _sr = gameObject.AddComponent<SpriteRenderer>();
        }
        
        private void Start()
        {
            _sr.material = new Material(_shader);
        }
        
        private void AddAnimations()
        {
            List<Sprite> dashAnticSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "DashAntic0"),
            };

            List<Sprite> dashingSprites = new List<Sprite>
            {
                FindSprite(TisoSpritesCustom, "Dashing0"),
                FindSprite(TisoSpritesCustom, "Dashing1"),
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
            
            List<Sprite> spinSprites = new List<Sprite>()
            {
                FindSprite(TisoSpritesGodhome, "Spin0"),
                FindSprite(TisoSpritesGodhome, "Spin1"),
                FindSprite(TisoSpritesGodhome, "Spin2"),
            };
            
            animations.Add("Dash Antic", dashAnticSprites);
            animations.Add("Dashing", dashingSprites);
            animations.Add("Idle", idleSprites);
            animations.Add("Jump Antic", jumpAnticSprites);
            animations.Add("Land", landSprites);
            animations.Add("Spinning", spinSprites);
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