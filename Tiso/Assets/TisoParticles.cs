using UnityEngine;

namespace Tiso
{
    public class TisoParticles : MonoBehaviour
    {
        private PlayMakerFSM _beeControl;
        
        private void Awake()
        {
            _beeControl = TisoSpencer.PreloadedGameObjects["Bee"].LocateMyFSM("Control");
        }

        private void GetParticles()
        {
            
        }
    }
}