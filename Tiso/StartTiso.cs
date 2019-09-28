using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;
using Logger = Modding.Logger;
using Object = UnityEngine.Object;

namespace Tiso
{
    public class StartTiso : MonoBehaviour
    {
        private PlayMakerFSM _activate;
        private AssetBundle _bundle;
        private Sprite[] _tisoSprites;

        public static GameObject Tiso;

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
            _activate.InsertCoroutine("Activate Tiso", 0, ReplaceTiso);
        }

        private IEnumerator ReplaceTiso()
        {
            Log("Instantiating new Tiso");
            Vector2 position = new Vector2(66.4f, 5f);
            Quaternion rotation = Quaternion.identity;
            Tiso = Instantiate(new GameObject(name="Tiso"), position, rotation);
            Log("Destroying Old Tiso Boss");
            Destroy(GameObject.Find("Tiso Boss"));
            Log("Adding Spencer Component to new Tiso");
            Tiso.AddComponent<Spencer>();
            //GameObject.Find("Tiso Boss").AddComponent<Spencer>();

            yield return null;
        }

        private void LoadAssets()
        {
            Log("Creating Path");
            var tempPath = Path.Combine(Application.streamingAssetsPath, "tisosprites");
            Log("Path: " + tempPath);
            var path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Hollow Knight\\hollow_knight_Data\\StreamingAssets\\tisosprites";
            Log("path exists: " + File.Exists(path));
            Log("tempPath exists: " + File.Exists(tempPath));
            Log("Loading From Path");
            _bundle = AssetBundle.LoadFromFile(path);
            Log("Bundle null? " + (_bundle == null));
            try
            {
                Log("Loading Asset with SubAssets");
                _tisoSprites = _bundle.LoadAssetWithSubAssets<Sprite>("TisoSprites");
                Log("Iterating through Tiso Sprites");
                foreach (Sprite sprite in _tisoSprites)
                {
                    Log("Sprite Name: " + sprite.name);
                }
            }
            catch (Exception e)
            {
                Log("Exception: " + e);
                throw;
            }
        }
        
        public static void Log(object message) => Logger.Log("[Start Tiso]" + message);
    }
}
