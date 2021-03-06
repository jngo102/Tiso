using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Modding;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UObject = UnityEngine.Object;

namespace Tiso
{
    public class TisoSpencer : Mod<SaveSettings>, ITogglableMod
    {
        public static readonly List<Sprite> Sprites = new List<Sprite>();
        public static readonly List<byte[]> SpriteBytes = new List<byte[]>();

        public static TisoSpencer Instance { get; private set; }

        public override string GetVersion()
        {
            return "1.0.0";
        }
        
        public static readonly Dictionary<string, GameObject> PreloadedGameObjects = new Dictionary<string, GameObject>();

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Fungus3_02", "Jellyfish/Corpse Jellyfish(Clone)"),
                ("GG_Crystal_Guardian_2", "Laser Turret Mega"),
                ("GG_God_Tamer", "Entry Object/Lancer"),
                ("GG_Hornet_2", "Boss Holder/Hornet Boss 2"),
                ("GG_Hornet_2", "Boss Holder/Hornet Boss 2/G Dash Effect"),
                ("GG_Hive_Knight", "Battle Scene/Hive Knight"),
                ("GG_Hive_Knight", "Battle Scene/Hive Knight/Slash 1"),
                ("GG_Hive_Knight", "Battle Scene/Hive Knight/Pt Jump"),
                ("GG_Hive_Knight", "Battle Scene/Globs/Hive Knight Glob"),
                ("GG_Lost_Kin", "Lost Kin"),
                ("GG_Soul_Tyrant", "Dream Mage Lord"),
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Storing GameObjects");
            PreloadedGameObjects.Add("Bee", preloadedObjects["GG_Hive_Knight"]["Battle Scene/Hive Knight"]);
            PreloadedGameObjects.Add("G Dash", preloadedObjects["GG_Hornet_2"]["Boss Holder/Hornet Boss 2/G Dash Effect"]);
            PreloadedGameObjects.Add("Glob", preloadedObjects["GG_Hive_Knight"]["Battle Scene/Globs/Hive Knight Glob"]);
            PreloadedGameObjects.Add("Hornet", preloadedObjects["GG_Hornet_2"]["Boss Holder/Hornet Boss 2"]);
            PreloadedGameObjects.Add("Kin", preloadedObjects["GG_Lost_Kin"]["Lost Kin"]);
            PreloadedGameObjects.Add("Mage", preloadedObjects["GG_Soul_Tyrant"]["Dream Mage Lord"]);
            PreloadedGameObjects.Add("Ooma Corpse", preloadedObjects["Fungus3_02"]["Jellyfish/Corpse Jellyfish(Clone)"]);
            PreloadedGameObjects.Add("Laser", preloadedObjects["GG_Crystal_Guardian_2"]["Laser Turret Mega"]);
            PreloadedGameObjects.Add("Pt Jump", preloadedObjects["GG_Hive_Knight"]["Battle Scene/Hive Knight/Pt Jump"]);
            PreloadedGameObjects.Add("Slash", preloadedObjects["GG_Hive_Knight"]["Battle Scene/Hive Knight/Slash 1"]);
            PreloadedGameObjects.Add("Tamer", preloadedObjects["GG_God_Tamer"]["Entry Object/Lancer"]);

            Instance = this;

            Log("Initializing");
            
            LoadAssets();
            
            Unload();
            
            ModHooks.Instance.BeforeSavegameSaveHook += BeforeSaveGameSave;
            ModHooks.Instance.AfterSavegameLoadHook += SaveGame;
            ModHooks.Instance.SavegameSaveHook += SaveGameSave;
            ModHooks.Instance.NewGameHook += AddComponent;
            ModHooks.Instance.LanguageGetHook += OnLangGet;
            USceneManager.activeSceneChanged += SceneChanged;
            ModHooks.Instance.SetPlayerVariableHook += SetVariableHook;
            ModHooks.Instance.GetPlayerVariableHook += GetVariableHook;
            
            // Taken from https://github.com/SalehAce1/PaleChampion/blob/master/PaleChampion/PaleChampion/PaleChampion.cs
            int index = 0;
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (string resource in assembly.GetManifestResourceNames())
            {
                if (!resource.EndsWith(".png"))
                {
                    continue;
                }
                
                using (Stream stream = assembly.GetManifestResourceStream(resource))
                {
                    if (stream == null) continue;

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Dispose();

                    // Create texture from bytes
                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(buffer, true);
                    // Create sprite from texture
                    SpriteBytes.Add(buffer);
                    Sprites.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));

                    Log("Created sprite from embedded image: " + resource + " at index " + index);
                    index++;
                }
            }
                        
            Log("Initialized.");
        }
        
        public static AssetBundle TisoAssetsBundle;
        private void LoadAssets()
        {
            Log("Loading TisoAssets");
            TisoAssetsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "tisoassets"));
        }

        // Taken from https://github.com/SalehAce1/Fennel/blob/master/ArenaFinder.cs

        private object SetVariableHook(Type t, string key, object obj)
        {
            if (key == "statueStateTiso")
                Settings.completion = (BossStatue.Completion)obj;
            return obj;
        }

        private object GetVariableHook(Type t, string key, object orig)
        {
            return key == "statueStateTiso"
                ? Settings.completion
                : orig;
        }
        
        private string _previousScene;
        private void SceneChanged(Scene previousScene, Scene nextScene)
        {
            _previousScene = previousScene.name;
        }
        
        private string OnLangGet(string key, string sheetTitle)
        {
            /*string text = Language.Language.GetInternal(key, sheettitle);
            Log("Key: " + key);
            Log("Text: " + text);
            return text;*/
            switch (key)
            {
                case "TISO_NAME":
                    return "Tiso";
                case "MAWLEK_SUPER" when _previousScene == "GG_Workshop" && PlayerData.instance.statueStateBroodingMawlek.usingAltVersion:
                    return "";
                case "MAWLEK_MAIN" when _previousScene == "GG_Workshop" && PlayerData.instance.statueStateBroodingMawlek.usingAltVersion:
                    return "Tiso";
                case "MAWLEK_SUB" when _previousScene == "GG_Workshop" && PlayerData.instance.statueStateBroodingMawlek.usingAltVersion:
                    return "by Tiso Spencer";
                case "TISO_DESC":
                    return "Tiso Placeholder";
                // "_1" is appended to the end of dream nail dialog because there is an initial convo amount of 1
                case "TISO_DIALOG_1_1":
                    return "I, Tiso, will be victorious!";
                case "TISO_DIALOG_2_1":
                    return "I seek the thrill of a true battle, not this pitiful skirmish!";
                case "TISO_DIALOG_3_1":
                    return "This tiny squib stands no chance against me!";
                case "TISO_DIALOG_4_1":
                    return "Once I defeat this weakling, the Colosseum awaits!";
                case "TISO_DIALOG_5_1":
                    return "I hope I don't run into that Cranky Templar fellow after I win this one...'";
                default:
                    return Language.Language.GetInternal(key, sheetTitle);
            }
        }

        private void BeforeSaveGameSave(SaveGameData data)
        {
            Settings.AltStatue = PlayerData.instance.statueStateBroodingMawlek.usingAltVersion;

            PlayerData.instance.statueStateBroodingMawlek.usingAltVersion = false;
        }
        
        private void SaveGame(SaveGameData data)
        {
            SaveGameSave();
            AddComponent();
        }
        
        private void SaveGameSave(int id = 0)
        {
            PlayerData.instance.statueStateBroodingMawlek.usingAltVersion = Settings.AltStatue;
        }

        private static void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<MawlekFinder>();
        }

        public void Unload()
        {
            ModHooks.Instance.BeforeSavegameSaveHook -= BeforeSaveGameSave;
            ModHooks.Instance.AfterSavegameLoadHook -= SaveGame;
            ModHooks.Instance.SavegameSaveHook -= SaveGameSave;
            ModHooks.Instance.NewGameHook -= AddComponent;
            ModHooks.Instance.LanguageGetHook -= OnLangGet;
            ModHooks.Instance.SetPlayerVariableHook -= SetVariableHook;
            ModHooks.Instance.GetPlayerVariableHook -= GetVariableHook;
            USceneManager.activeSceneChanged -= SceneChanged;

            MawlekFinder finder = GameManager.instance.gameObject.GetComponent<MawlekFinder>();
            Spencer spencer = GameManager.instance.gameObject.GetComponent<Spencer>();

            if (finder != null)
                UObject.Destroy(finder);
            
            if (spencer != null)
                UObject.Destroy(spencer);
        }
    }
}
