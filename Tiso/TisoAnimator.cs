using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

// Taken from https://github.com/SalehAce1/PaleChampion/blob/master/PaleChampion/PaleChampion/CustomAnimator.cs
namespace Tiso
{
    internal class TisoAnimator : MonoBehaviour
    {
        public Dictionary<string, List<Sprite>> animations = new Dictionary<string, List<Sprite>>();
        
        private SpriteRenderer _sr;
        private void Awake()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
        }

        private Shader _shader;
        private void Start()
        {
            _shader = TisoSpencer.TisoAssetsBundle.LoadAsset<Shader>("Sprites-Diffuse_Flash");
            _sr.material = new Material(_shader);
        }
        
        private IEnumerator Play(string animName, bool looping, float delay)
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

        private const float AnimFPS = 1.0f / 12;
        private Coroutine _currentAnimation;
        public void PlayAnimation(string animName, bool looping = false, float delay = AnimFPS)
        {
            Log("Stopping Current Animation");
            if (_currentAnimation != null) StopCoroutine(_currentAnimation);
            Log("Starting Next Animation");
            _currentAnimation = StartCoroutine(Play(animName, looping, delay));
        }

        private static void Log(object message) => Modding.Logger.Log("[Custom Animator] " + message);
    }
}