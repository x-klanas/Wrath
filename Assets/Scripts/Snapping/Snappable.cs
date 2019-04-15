using System.Collections.Generic;
using UnityEngine;

namespace Snapping {
    [RequireComponent(typeof(Rigidbody))]
    public class Snappable : MonoBehaviour {
        public new Rigidbody rigidbody;

        public bool canSnap;

        private List<SnapPoint> snapPoints = new List<SnapPoint>();

        private void Start() {
            if (rigidbody == null) {
                rigidbody = GetComponent<Rigidbody>();
            }
        }

        public void SetCanSnap(bool canSnap) {
            this.canSnap = canSnap;

            if (!canSnap) {
                foreach (SnapPoint snapPoint in snapPoints) {
                    snapPoint.Unsnap();
                }
            }
        }

        public void AddSnapPoint(SnapPoint snapPoint) {
            snapPoints.Add(snapPoint);
        }
    }
}