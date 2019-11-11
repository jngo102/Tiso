using System;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Tiso
{
    public class TisoAudio : MonoBehaviour
    {
        private static AudioSource _audio;
        private static PlayMakerFSM _beeControl;

        private void Awake()
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _beeControl = TisoSpencer.PreloadedGameObjects["Bee"].LocateMyFSM("Control");
        }
        
        private AudioClip GetAudioClip(string clipName)
        {
            switch (clipName)
            {
                case "Jump":
                case "Land":
                    return (AudioClip)_beeControl.GetAction<AudioPlaySimple>(clipName).oneShotClip.Value;
                default:
                    return null;
            }
        }

        public void PlayAudioClip(string clipName)
        {
            try
            {
                Log("Getting Audio Clip");
                AudioClip audioClip = GetAudioClip(clipName);
                Log("Playing Audio Clip");
                _audio.PlayOneShot(audioClip);
            }
            catch (Exception e)
            {
                Log(e);
                throw;
            }
        }

        private static void Log(object message) => Modding.Logger.Log("[Tiso Audio]: " + message);
    }
}