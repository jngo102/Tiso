using System;
using System.Collections;
using System.Reflection;
using ModCommon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Modding.Logger;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

// Taken and modified from https://github.com/5FiftySix6/HollowKnight.Pale-Prince/blob/master/Pale%20Prince/PrinceFinder.cs

namespace Tiso
{
    internal class MawlekFinder : MonoBehaviour
    {
        private void Start()
        {
            USceneManager.activeSceneChanged += SceneChanged;
        }

        private void SceneChanged(Scene previousScene, Scene nextScene)
        {
            /* Passing the strings instead of the Scenes because that's all we use and
             Unity kills the prev scene's name after <1 frame */
            StartCoroutine(SceneChangedRoutine(previousScene.name, nextScene.name));
        }

        private IEnumerator SceneChangedRoutine(string prev, string next)
        {
            yield return null;
            
            if (next == "GG_Workshop") SetStatue();
            
            if (next != "GG_Brooding_Mawlek_V") yield break;
            if (prev != "GG_Workshop") yield break;

            StartCoroutine(AddComponent());
        }

        private void SetStatue()
        {
            Log("Setting up statues...");

            GameObject statue = GameObject.Find("GG_Statue_Mawlek");

            BossScene scene = ScriptableObject.CreateInstance<BossScene>();
            scene.sceneName = "GG_Brooding_Mawlek_V";

            BossStatue bs = statue.GetComponent<BossStatue>();
            bs.dreamBossScene = scene;
            bs.dreamStatueStatePD = "statueStateTiso";

            bs.SetPlaquesVisible(bs.StatueState.isUnlocked && bs.StatueState.hasBeenSeen || bs.isAlwaysUnlocked);
            
            Destroy(statue.FindGameObjectInChildren("StatueAlt"));
            GameObject displayStatue = bs.statueDisplay;
            GameObject alt = Instantiate
            (
                displayStatue,
                displayStatue.transform.parent,
                true
            );
            alt.SetActive(bs.UsingDreamVersion);
            SpriteRenderer spriteRenderer = alt.GetComponentInChildren<SpriteRenderer>();
            spriteRenderer.sprite = TisoSpencer.Sprites[0];
            spriteRenderer.transform.position += Vector3.down;
            alt.name = "StatueAlt";
            bs.statueDisplayAlt = alt;

            BossStatue.BossUIDetails details = new BossStatue.BossUIDetails();
            details.nameKey = details.nameSheet = "Tiso_Name";
            details.descriptionKey = details.descriptionSheet = "Tiso_Desc";
            bs.dreamBossDetails = details;

            GameObject altLever = statue.FindGameObjectInChildren("alt_lever");
            altLever.SetActive(true);
            altLever.transform.position = new Vector3(45.5f, 7.5f, 0.9f);

            GameObject switchBracket = altLever.FindGameObjectInChildren("GG_statue_switch_bracket");
            switchBracket.SetActive(true);

            GameObject switchLever = altLever.FindGameObjectInChildren("GG_statue_switch_lever");
            switchLever.SetActive(true);

            BossStatueLever toggle = statue.GetComponentInChildren<BossStatueLever>();
            toggle.SetOwner(bs);
            toggle.SetState(true);
        }

        private static IEnumerator AddComponent()
        {
            yield return null;

            if (PlayerData.instance.statueStateBroodingMawlek.usingAltVersion)
            {
                Log("Adding StartTiso");
                GameObject.Find("Battle Scene").AddComponent<StartTiso>();

                yield return new WaitForSeconds(5.0f);
                
                Log("Adding Spencer");
                //GameObject.Find("Tiso Boss").AddComponent<Spencer>();
            }
        }

        private void OnDestroy()
        {
            USceneManager.activeSceneChanged -= SceneChanged;
        }

        public static void Log(object o)
        {
            Logger.Log($"[{Assembly.GetExecutingAssembly().GetName().Name}]: " + o);
        }
    }
}