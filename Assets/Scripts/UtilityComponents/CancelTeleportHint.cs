using UnityEngine;
using Valve.VR.InteractionSystem;

namespace UtilityComponents {
    public class CancelTeleportHint : MonoBehaviour {
        private void Start() {
            Teleport teleport = Teleport.instance;

            if (teleport) {
                teleport.CancelTeleportHint();
            }
        }
    }
}