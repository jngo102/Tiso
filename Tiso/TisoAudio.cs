using System;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Tiso
{
    public class TisoAudio : MonoBehaviour
    {
        private static AudioSource _audio;
        private static PlayMakerFSM _beeControl;
        private static PlayMakerFSM _hornetControl;

        private void Awake()
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _beeControl = TisoSpencer.PreloadedGameObjects["Bee"].LocateMyFSM("Control");
            _hornetControl = TisoSpencer.PreloadedGameObjects["Hornet"].LocateMyFSM("Control");
        }
        
        private AudioClip GetAudioClip(string clipName)
        {
            switch (clipName)
            {
                case "Dash":
                    return (AudioClip)_hornetControl.GetAction<AudioPlaySimple>("G Dash").oneShotClip.Value;
                case "Jump":
                case "Land":
                    return (AudioClip)_beeControl.GetAction<AudioPlaySimple>(clipName).oneShotClip.Value;
                default:
                    return null;
            }
        }

        public void PlayAudioClip(string clipName) => _audio.PlayOneShot(GetAudioClip(clipName));

        private static void Log(object message) => Modding.Logger.Log("[Tiso Audio]: " + message);
    }
}