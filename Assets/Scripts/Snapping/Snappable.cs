using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace Snapping {
    [RequireComponent(typeof(Rigidbody), typeof(Interactable))]
    public class Snappable : MonoBehaviour {
        public new Rigidbody rigidbody;

        [SerializeField] private bool canSnap;

        public bool CanSnap {
            get => canSnap;
            set {
                canSnap = value;

                if (!canSnap) {
                    Unsnap();
                }
            }
        }

        [SerializeField] private bool allowSticking;

        public bool AllowSticking {
            get => allowSticking;
            set => allowSticking = value;
        }

        public bool IsSnapped => currentlySnappedPoints.Count > 0;

        [Header("SteamVR specific settings")]
        public bool steamVRThrowable = true;

        public bool noHandCanSnap = false;
        public bool noHandAllowSticking = false;

        public bool handCanSnap = true;
        public bool handAllowSticking = true;

        public Interactable interactable;

        private readonly List<SnapPoint> snapPoints = new List<SnapPoint>();
        private readonly List<SnapPoint> currentlySnappedPoints = new List<SnapPoint>();

        private void Awake() {
            if (rigidbody == null) {
                rigidbody = GetComponent<Rigidbody>();
            }

            if (interactable == null) {
                interactable = GetComponent<Interactable>();
            }
        }

        private void Start() {
            if (steamVRThrowable) {
                interactable.onAttachedToHand += OnHandAttach;
                interactable.onDetachedFromHand += OnHandDetach;
            }
        }

        private void OnHandAttach(Hand hand) {
            SteamVR_Actions.default_Interact.AddOnStateDownListener(OnVRButtonDown, hand.handType);
        }

        private void OnHandDetach(Hand hand) {
            SteamVR_Actions.default_Interact.RemoveOnStateDownListener(OnVRButtonDown, hand.handType);
        }

        private void OnVRButtonDown(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) {
            StickToSnappedPoints();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Return)) {
                StickToSnappedPoints();
            }
        }

        public void StickToSnappedPoints() {
            if (allowSticking) {
                foreach (SnapPoint snappedPoint in currentlySnappedPoints) {
                    if (snappedPoint.IsSticky) {
                        snappedPoint.Unstick();
                    } else {
                        snappedPoint.Stick();
                    }
                }
            }
        }

        public void PointSnapped(SnapPoint snapPoint) {
            currentlySnappedPoints.Add(snapPoint);
        }

        public void PointUnsnapped(SnapPoint snapPoint) {
            currentlySnappedPoints.Remove(snapPoint);
        }

        public void Unsnap() {
            foreach (SnapPoint snapPoint in snapPoints) {
                snapPoint.Unsnap();
            }
        }

        public void AddSnapPoint(SnapPoint snapPoint) {
            snapPoints.Add(snapPoint);
        }

        // ReSharper disable once UnusedMember.Local
        private void OnAttachedToHand() {
            if (steamVRThrowable) {
                CanSnap = handCanSnap;
                AllowSticking = handAllowSticking;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDetachedFromHand() {
            if (steamVRThrowable) {
                CanSnap = noHandCanSnap;
                AllowSticking = noHandAllowSticking;
            }
        }
    }
}