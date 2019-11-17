using System.Collections;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;
using Logger = Modding.Logger;
using UnityEngine;

using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Tiso
{
    internal class Tamer : MonoBehaviour
    {
        private const int Health = 1700;
        
        private HealthManager _hm;
        private PlayMakerFSM _control;
        
        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Control");
            _hm = GetComponent<HealthManager>();

            On.InfectedEnemyEffects.RecieveHitEffect += ReceiveHit;
        }

        private IEnumerator Start()
        {
            yield return null;

            while (HeroController.instance == null) yield return null;

            _hm.hp = Health;

            Log("Changing Tamer Defeat y");
            _control.GetAction<SetPosition>("Defeat").y = 5.6f;
            _control.GetAction<SetPosition>("Done").y = 5.6f;

            _control.GetAction<WaitRandom>("Idle").timeMin = 0.25f;
            _control.GetAction<WaitRandom>("Idle").timeMax = 0.25f;
            _control.GetAction<FloatClamp>("Aim").minValue = -90.0f;
            _control.GetAction<FloatClamp>("Aim").maxValue = 90.0f;
            _control.GetAction<SetVelocity2d>("Launch").y = 25.0f;

            gameObject.PrintSceneHierarchyTree();
        }

        // Taken and modified from https://github.com/5FiftySix6/HollowKnight.Lost-Lord/blob/master/LostLord/Kin.cs
        private void ReceiveHit(On.InfectedEnemyEffects.orig_RecieveHitEffect orig, InfectedEnemyEffects self, float attack_direction)
        {
            if (self.GetAttr<bool>("didFireThisFrame"))
            {
                return;
            }

            if (self.GetAttr<SpriteFlash>("spriteFlash") != null)
            {
                self.GetAttr<SpriteFlash>("spriteFlash").flashFocusHeal();
            }
            
            FSMUtility.SendEventToGameObject(gameObject, "DAMAGE FLASH", true);
            self.GetAttr<AudioEvent>("impactAudio").SpawnAndPlayOneShot(self.GetAttr<AudioSource>("audioSourcePrefab"), self.transform.position);
            self.SetAttr("didFireThisFrame", true);
        }

        private void OnDestroy()
        {
            On.InfectedEnemyEffects.RecieveHitEffect -= ReceiveHit;
        }

        public static void Log(object message) => Logger.Log("[Tamer]" + message);
    }
    
}