using Snapping;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace Parts {
    [RequireComponent(typeof(Snappable), typeof(Interactable))]
    public class AutomaticScrewdriver : MonoBehaviour {
        public float screwingSpeed = 1f;
        public ScrewingDirection screwingDirection = ScrewingDirection.In;

        public SteamVR_Action_Boolean screwAction = SteamVR_Actions.screwing_Screw;
        public SteamVR_Action_Boolean changeDirectionAction = SteamVR_Actions.screwing_ChangeDirection;

        public SnapPoint snapPoint;
        public Interactable interactable;

        private Screw screw;

        private void Awake() {
            if (snapPoint == null) {
                snapPoint = GetComponentInChildren<SnapPoint>();
            }
            
            if (interactable == null) {
                interactable = GetComponent<Interactable>();
            }
        }

        private void Start() {
            snapPoint.OnSnap += OnSnap;
            snapPoint.OnUnsnap += OnUnsnap;

            interactable.onAttachedToHand += hand => changeDirectionAction.AddOnStateDownListener(OnDirectionButton, hand.handType);
            interactable.onDetachedFromHand += hand => changeDirectionAction.RemoveOnStateDownListener(OnDirectionButton, hand.handType);
        }

        private void Update() {
            if (interactable.attachedToHand && screwAction.GetState(interactable.attachedToHand.handType) && screw) {
                screw.ScrewValue += screwingSpeed * Time.deltaTime * (int) screwingDirection;
            }
        }

        private void OnSnap() {
            screw = snapPoint.SnappedPoint.GetComponentInParent<Screw>();
        }

        private void OnUnsnap() {
            screw = null;
        }

        private void OnDirectionButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
            screwingDirection = screwingDirection == ScrewingDirection.In ? ScrewingDirection.Out : ScrewingDirection.In;
        }
    }
}