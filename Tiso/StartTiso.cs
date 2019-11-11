﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Color;
using Logger = Modding.Logger;
using Object = UnityEngine.Object;
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
            _activate.InsertCoroutine("Activate Tiso", 0, ReplaceTiso);
        }

        private IEnumerator ReplaceTiso()
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

            /*Tiso = Instantiate(new GameObject("Tiso"), position, rotation);
            Log("Destroying Old Tiso Boss");
            Destroy(GameObject.Find("Tiso Boss"));
            Log("Adding Spencer Component to new Tiso");
            Tiso.AddComponent<Spencer>();*/
            Log("Finding Tiso");
            GameObject tiso = GameObject.Find("Tiso Boss");
            Log("Adding Spencer to Tiso");
            tiso.AddComponent<Spencer>();
            Log("Instantiating new Tiso");
            Vector2 position = tiso.transform.position;
            Quaternion rotation = Quaternion.identity;
            Tiso = Instantiate(tiso, position, rotation);
            Log("Setting State of New Tiso to Roar End");
            Tiso.LocateMyFSM("Control").SetState("Roar End");
            yield return new WaitForSeconds(1.0f);
            Log("Destroying Old Tiso");
            Destroy(tiso);
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
