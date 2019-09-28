using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        public static GameObject Hornet;
        
        public static TisoSpencer Instance { get; private set; }

        public static readonly Dictionary<string, GameObject> PreloadedGameObjects = new Dictionary<string, GameObject>();
        
        public override string GetVersion()
        {
            return "1.0.0";
        }

        private string _previousScene;

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("Crossroads_50", "Tiso Lake NPC"),
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Storing GameObjects");
            PreloadedGameObjects.Add("Tiso Idle", preloadedObjects["Crossroads_50"]["Tiso Lake NPC"]);

            Instance = this;

            Log("Initializing");
            
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
        
        private void SceneChanged(Scene previousScene, Scene nextScene)
        {
            _previousScene = previousScene.name;
        }
        
        private string OnLangGet(string key, string sheettitle)
        {
            switch (key)
            {
                case "Tiso_Name":
                    return "Tiso";
                case "MAWLEK_SUPER" when _previousScene == "GG_Workshop" && PlayerData.instance.statueStateBroodingMawlek.usingAltVersion:
                    return "";
                case "MAWLEK_MAIN" when _previousScene == "GG_Workshop" && PlayerData.instance.statueStateBroodingMawlek.usingAltVersion:
                    return "Tiso";
                case "MAWLEK_SUB" when _previousScene == "GG_Workshop" && PlayerData.instance.statueStateBroodingMawlek.usingAltVersion:
                    return "by Tiso Spencer";
                case "Tiso_Desc":
                    return "Tiso Placeholder";
                default:
                    return Language.Language.GetInternal(key, sheettitle);
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

            if (finder != null)
                UObject.Destroy(finder);
        }
    }
}
