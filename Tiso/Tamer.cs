using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using Logger = Modding.Logger;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = System.Random;

using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Tiso
{
    internal class Tamer : MonoBehaviour
    {
        private const int Health = 1700;
        
        private HealthManager _healthManager;
        private PlayMakerFSM _control;
        
        private void Awake()
        {
            _control = gameObject.LocateMyFSM("Control");
            _healthManager = GetComponent<HealthManager>();
        }

        private IEnumerator Start()
        {
            yield return null;

            while (HeroController.instance == null) yield return null;

            _healthManager.hp = Health;

            Log("Changing Tamer Defeat y");
            _control.GetAction<SetPosition>("Defeat").y = 4.0f;
            _control.InsertMethod("Defeat", 0, LogPos);

            _control.GetAction<WaitRandom>("Idle").timeMin = 0.25f;
            _control.GetAction<WaitRandom>("Idle").timeMax = 0.25f;
            _control.GetAction<FloatClamp>("Aim").minValue = -90.0f;
            _control.GetAction<FloatClamp>("Aim").maxValue = 90.0f;
            _control.GetAction<SetVelocity2d>("Launch").y = 25.0f;

            gameObject.PrintSceneHierarchyTree();
        }

        private void LogPos()
        {
            Log("Death Position Before: " + transform.position);
            gameObject.transform.SetPosition2D(transform.position.x, 4.0f);
            Vector2 pos = transform.position;
            pos.y = 4.0f;
            Log("Death Position After: " + transform.position);
        }
        
        void Update()
        {

        }

        public static void Log(object message) => Logger.Log("[Tamer]" + message);
    }
    
}