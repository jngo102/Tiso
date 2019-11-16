using System.Collections;
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
            /*string tisoScenePath = TisoSpencer.TisoBundle.GetAllScenePaths()[0];
            Log("Loading Tiso Scene");
            AsyncOperation operation = USceneManager.LoadSceneAsync(tisoScenePath);
            while (!operation.isDone) yield return null;
            Log("Getting Active Scene...");
            Scene activeScene = USceneManager.GetActiveScene();
            Log("Active Scene: " + activeScene.name);
            GameObject[] gos = activeScene.GetRootGameObjects();
            foreach (var go in gos)
            {
                Log("GO Name: " + go.name);
            }*/
            
            Log("Finding Tiso");
            GameObject tiso = GameObject.Find("Tiso Boss");
            Log("Adding Spencer to Tiso");
            tiso.AddComponent<Spencer>();
            
            int bossLevel = BossSceneController.Instance.BossLevel;
            if (bossLevel > 0) SummonGodTamer();

            TisoSpencer.PreloadedGameObjects["Bee"].PrintSceneHierarchyTree();
            
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
