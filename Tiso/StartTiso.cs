using System.Collections;
using IL.HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;
using Logger = Modding.Logger;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Tiso
{
    public class StartTiso : MonoBehaviour
    {
        private PlayMakerFSM _activate;

        public static GameObject Tiso;
        public static GameObject Tamer;

        private static GameObject preloadedGO;

        private void Awake()
        {
            _activate = gameObject.LocateMyFSM("Activate Boss");
        }

        private IEnumerator Start()
        {
            yield return null;

            while (HeroController.instance == null) yield return null;
            
            _activate.ChangeTransition("In Pantheon?", "STATUE", "Tiso?");
            _activate.CreateState("Activate Tiso");
            _activate.ChangeTransition("Tiso", "TISO END", "Activate Tiso");
            _activate.InsertCoroutine("Activate Tiso", 0, TisoStart);
        }

        private IEnumerator TisoStart()
        {
            /*Log("Instantiating Tiso");
            GameObject TisoBoss = Instantiate(TisoSpencer.PreloadedGameObjects["Tiso"], new Vector2(66.4f, 10.0f),
                Quaternion.identity);
            TisoBoss.SetActive(true);
            TisoBoss.GetComponent<SpriteRenderer>().material = new Material(Shader.Find("Sprites/Default"));*/

            Log("Finding Tiso");
            GameObject tiso = GameObject.Find("Tiso Boss");
            Log("Adding Spencer to Tiso");
            tiso.AddComponent<Spencer>();

            int bossLevel = BossSceneController.Instance.BossLevel;
            if (bossLevel > 0) SummonGodTamer();

            yield return null;
        }

        private void SummonGodTamer()
        {;
            Vector2 tamerPos = new Vector2(66.4f, 20f);
            Quaternion rotation = Quaternion.identity;
            Log("Instantiating God Tamer");
            Tamer = Instantiate(TisoSpencer.PreloadedGameObjects["Tamer"], tamerPos, rotation);
            Tamer.SetActive(true);
            Log("Adding Tamer Component to God Tamer");
            Tamer.AddComponent<Tamer>();
        }
        
        public static void Log(object message) => Logger.Log("[Start Tiso]" + message);
    }
}
