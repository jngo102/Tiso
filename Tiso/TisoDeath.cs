using System.Collections;
using UnityEngine;

namespace Tiso
{
    public class TisoDeath : MonoBehaviour
    {
        private IEnumerator Start()
        {
            if (StartTiso.Tamer != null) StartTiso.Tamer.LocateMyFSM("Death Detect").SendEvent("LOBSTER KILLED");
            yield return new WaitForSeconds(4.0f);
            //BossSceneController controller = GetComponent<BossSceneController>();
            //controller.DoDreamReturn();
            PlayMakerFSM dreamReturn = GameObject.Find("Boss Scene Controller").LocateMyFSM("Dream Return");
            dreamReturn.SendEvent("DREAM RETURN");
        }

        private void Log(object message) => Modding.Logger.Log("[Tiso Death]: " + message);
    }
}