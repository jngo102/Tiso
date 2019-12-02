using System;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using UnityEngine;

namespace Tiso
{
    public class TisoAudio : MonoBehaviour
    {
        private AudioSource _audio;
        private PlayMakerFSM _beeControl;
        private PlayMakerFSM _hornetControl;
        private PlayMakerFSM _kinControl;
        private PlayMakerFSM _tink;

        private void Awake()
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _beeControl = TisoSpencer.PreloadedGameObjects["Bee"].LocateMyFSM("Control");
            _hornetControl = TisoSpencer.PreloadedGameObjects["Hornet"].LocateMyFSM("Control");
            _kinControl = TisoSpencer.PreloadedGameObjects["Kin"].LocateMyFSM("IK Control");
            _tink = TisoSpencer.PreloadedGameObjects["Bee"].FindGameObjectInChildren("Slash 1").LocateMyFSM("nailclash_tink");
        }
        
        private AudioClip GetAudioClip(string clipName)
        {
            switch (clipName)
            {
                case "Dash":
                    return (AudioClip) _hornetControl.GetAction<AudioPlaySimple>("G Dash").oneShotClip.Value;
                case "Dive Land":
                    return (AudioClip) _kinControl.GetAction<AudioPlaySimple>("Dstab Land").oneShotClip.Value;
                case "Diving":
                    return (AudioClip) _kinControl.GetAction<AudioPlaySimple>("Dstab Fall").oneShotClip.Value;
                case "Evade Land":
                    return (AudioClip) _hornetControl.GetAction<AudioPlaySimple>("Evade Land").oneShotClip.Value;
                case "Jump":
                case "Land":
                    return (AudioClip) _beeControl.GetAction<AudioPlaySimple>(clipName).oneShotClip.Value;
                case "Slash":
                    return (AudioClip) _beeControl.GetAction<AudioPlayerOneShotSingle>("Slash 1").audioClip.Value;
                case "Tink":
                    return _tink.GetAction<AudioPlayerOneShot>("Blocked Hit").audioClips[0]; 
                default:
                    return null;
            }
        }

        public void PlayAudioClip(string clipName, float time = 0.0f)
        {
            _audio.time = time; 
            _audio.PlayOneShot(GetAudioClip(clipName));
            _audio.time = 0.0f;
        }

        private static void Log(object message) => Modding.Logger.Log("[Tiso Audio]: " + message);
    }
}