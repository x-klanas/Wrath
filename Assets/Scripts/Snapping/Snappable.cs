using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace Snapping {
    [RequireComponent(typeof(Rigidbody))]
    public class Snappable : MonoBehaviour {
        public new Rigidbody rigidbody;

        [SerializeField] private bool steamVRThrowable = true;

        public bool SteamVRThrowable {
            get => steamVRThrowable;
            set {
                steamVRThrowable = value;

                if (steamVRThrowable) {
                    SteamVR_Actions.default_Interact.onStateDown += OnVRButtonDown;
                } else {
                    SteamVR_Actions.default_Interact.onStateDown -= OnVRButtonDown;
                }
            }
        }

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

        private readonly List<SnapPoint> snapPoints = new List<SnapPoint>();
        private readonly List<SnapPoint> currentlySnappedPoints = new List<SnapPoint>();

        private void Awake() {
            if (rigidbody == null) {
                rigidbody = GetComponent<Rigidbody>();
            }
        }

        private void Start() {
            if (steamVRThrowable) {
                SteamVR_Actions.default_Interact.onStateDown += OnVRButtonDown;
            }
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
                CanSnap = true;
                AllowSticking = true;
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void OnDetachedFromHand() {
            if (steamVRThrowable) {
                CanSnap = false;
                AllowSticking = false;
            }
        }
    }
}