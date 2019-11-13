using UnityEngine;

namespace Tiso
{
    public class PhaseControl : MonoBehaviour
    {
        private int _phase1Health;
        private int _phase2Health;
        private int _phase3Health;
        
        public HealthManager hm;

        public delegate void TriggeredPhase2EventHandler();
        public event TriggeredPhase2EventHandler TriggeredPhase2;

        protected virtual void OnTriggeredPhase2()
        {
            if (TriggeredPhase2 != null) TriggeredPhase2();
        }
        
        public delegate void TriggeredPhase3EventHandler();
        public event TriggeredPhase3EventHandler TriggeredPhase3;

        protected virtual void OnTriggeredPhase3()
        {
            if (TriggeredPhase3 != null) TriggeredPhase3();
        }

        private void Awake()
        {
            hm = gameObject.AddComponent<HealthManager>();
            
            hm.OnDeath += DeathHandler;
        }

        
        private void Start()
        {
            int bossLevel = BossSceneController.Instance.BossLevel;
            if (bossLevel > 0)
            {
                // ascended+ health
                _phase1Health = 550;
                _phase2Health = 550;
                _phase3Health = 600;
            }
            else
            {
                // attuned health
                _phase1Health = 400;
                _phase2Health = 400;
                _phase3Health = 450;
            }

            int totalHealth = _phase1Health + _phase2Health + _phase3Health;
            hm.hp = totalHealth;
        }

        private bool _enteredPhase2;
        private bool _enteredPhase3;
        private void Update()
        {
            if (hm.hp < _phase2Health + _phase3Health && !_enteredPhase2)
            {
                _enteredPhase2 = true;
                OnTriggeredPhase2();
            }
            
            if (hm.hp < _phase3Health && !_enteredPhase3)
            {
                _enteredPhase3 = true;
                OnTriggeredPhase3();
            }
        }

        private void DeathHandler()
        {
            Vector2 position = transform.position + Vector3.down * 1.0f;
            Quaternion rotation = Quaternion.identity;
            GameObject deadTiso = Instantiate(new GameObject("Tiso Corpse"), position, rotation);
            Sprite deadTisoSprite = TisoAnimator.FindSprite(TisoAnimator.TisoSpritesGodhome, "Dead");
            deadTiso.AddComponent<SpriteRenderer>().sprite = deadTisoSprite;
            deadTiso.AddComponent<TisoDeath>();
        }

        private void OnDestroy()
        {
            hm.OnDeath -= DeathHandler;
        }

        private static void Log(object message) => Modding.Logger.Log("[Phase Control]: " + message);
    }
}