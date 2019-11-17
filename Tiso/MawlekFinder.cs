using System.Collections;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
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
            
            //nextScene.PrintHierarchy();
        }

        private IEnumerator SceneChangedRoutine(string prev, string next)
        {
            yield return null;
            
            if (next == "GG_Workshop") SetStatue();
            
            if (next != "GG_Brooding_Mawlek_V") yield break;
            if (prev != "GG_Workshop") yield break;

            StartCoroutine(AddComponent());
        }

        private static void GetNailClash()
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.PrintSceneHierarchyPath() == "Hollow Shade\\Slash")
                {
                    TisoSpencer.PreloadedGameObjects["Parry FX"] = go.LocateMyFSM("nail_clash_tink").GetAction<SpawnObjectFromGlobalPool>("No Box Down", 1).gameObject.Value;
                    AudioClip tinkAudio = go.LocateMyFSM("nail_clash_tink").GetAction<AudioPlayerOneShot>("Blocked Hit", 5).audioClips[0];
                    GameObject clashAudioGO = new GameObject();
                    AudioSource clashAudio = clashAudioGO.AddComponent<AudioSource>();
                    clashAudio.clip = tinkAudio;
                    clashAudio.pitch = Random.Range(0.85f, 1.15f);
                    TisoSpencer.PreloadedGameObjects["Clash Tink"] = clashAudioGO;
                    break;
                }
            }
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

            //GetNailClash();
            
            if (PlayerData.instance.statueStateBroodingMawlek.usingAltVersion)
            {
                GameObject.Find("Battle Scene").AddComponent<StartTiso>();;
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