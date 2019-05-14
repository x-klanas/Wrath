using Snapping;
using UnityEngine;

namespace Parts {
    [RequireComponent(typeof(SnapPoint))]
    public class ScrewHole : MonoBehaviour {
        public float positionSpring = 0f;
        public float positionDamper = 0f;
        public float positionMaxForce = 0f;

        public float rotationSpring = 0f;
        public float rotationDamper = 0f;
        public float rotationMaxForce = 0f;
        
        public bool applyScrewSettings = true;

        public SnapPoint reinforcedSnapPoint;
        public SnapPoint screwSnapPoint;

        private Screw screw;
        private SnapPoint reinforceTarget;

        private float initialPositionSpring;
        private float initialPositionDamper;
        private float initialPositionMaxForce;

        private float initialRotationSpring;
        private float initialRotationDamper;
        private float initialRotationMaxForce;

        private void Start() {
            if (screwSnapPoint == null) {
                screwSnapPoint = GetComponentInChildren<SnapPoint>();
            }

            

            screwSnapPoint.OnSnap += OnScrewSnap;
            screwSnapPoint.OnUnsnap += OnScrewUnsnap;
            screwSnapPoint.OnStick += OnScrewStick;
            screwSnapPoint.OnUnstick += OnScrewUnstick;

            reinforcedSnapPoint.OnSnap += OnReinforceSnap;
            reinforcedSnapPoint.OnUnsnap += OnReinforceUnsnap;
        }

        private void OnReinforceSnap(SnapPoint snappedPoint) {
            reinforceTarget = reinforcedSnapPoint.IsSnapParent ? reinforcedSnapPoint : reinforcedSnapPoint.SnappedPoint;
            
            initialPositionSpring = reinforceTarget.stickyPositionSpring;
            initialPositionDamper = reinforceTarget.stickyPositionDamper;
            initialPositionMaxForce = reinforceTarget.stickyPositionMaxForce;

            initialRotationSpring = reinforceTarget.stickyRotationSpring;
            initialRotationDamper = reinforceTarget.stickyRotationDamper;
            initialRotationMaxForce = reinforceTarget.stickyRotationMaxForce;
        }

        private void OnReinforceUnsnap(SnapPoint snappedPoint) {
            reinforceTarget.stickyPositionSpring = initialPositionSpring;
            reinforceTarget.stickyPositionDamper = initialPositionDamper;
            reinforceTarget.stickyPositionMaxForce = initialPositionMaxForce;

            reinforceTarget.stickyRotationSpring = initialRotationSpring;
            reinforceTarget.stickyRotationDamper = initialRotationDamper;
            reinforceTarget.stickyRotationMaxForce = initialRotationMaxForce;
        }

        private void OnScrewSnap(SnapPoint snappedPoint) {
            screw = screwSnapPoint.SnappedPoint.Rigidbody.GetComponentInChildren<Screw>();
        }

        private void OnScrewUnsnap(SnapPoint snappedPoint) {
            screw = null;
        }

        private void OnScrewStick(SnapPoint snappedPoint) {
            if (screw) {
                screw.OnScrewValue += UpdateProperties;
                UpdateProperties(screw.ScrewValue);
            }
        }

        private void OnScrewUnstick(SnapPoint snappedPoint) {
            if (screw) {
                screw.OnScrewValue -= UpdateProperties;
            }
        }

        private void UpdateProperties(float screwValue) {
            if (screw && reinforcedSnapPoint.IsSnapped) {
                reinforceTarget.stickyPositionSpring = initialPositionSpring + Mathf.Lerp(0, positionSpring, screwValue);
                reinforceTarget.stickyPositionDamper = initialPositionDamper + Mathf.Lerp(0, positionDamper, screwValue);
                reinforceTarget.stickyPositionMaxForce = initialPositionMaxForce + Mathf.Lerp(0, positionMaxForce, screwValue);

                reinforceTarget.stickyRotationSpring = initialRotationSpring + Mathf.Lerp(0, rotationSpring, screwValue);
                reinforceTarget.stickyRotationDamper = initialRotationDamper + Mathf.Lerp(0, rotationDamper, screwValue);
                reinforceTarget.stickyRotationMaxForce = initialRotationMaxForce + Mathf.Lerp(0, rotationMaxForce, screwValue);

                if (applyScrewSettings) {
                    reinforceTarget.stickyPositionSpring += Mathf.Lerp(0, screw.positionSpring, screwValue);
                    reinforceTarget.stickyPositionDamper += Mathf.Lerp(0, screw.positionDamper, screwValue);
                    reinforceTarget.stickyPositionMaxForce += Mathf.Lerp(0, screw.positionMaxForce, screwValue);

                    reinforceTarget.stickyRotationSpring += Mathf.Lerp(0, screw.rotationSpring, screwValue);
                    reinforceTarget.stickyRotationDamper += Mathf.Lerp(0, screw.rotationDamper, screwValue);
                    reinforceTarget.stickyRotationMaxForce += Mathf.Lerp(0, screw.rotationMaxForce, screwValue);
                }

                reinforceTarget.UpdateProperties();
            }
        }
    }
}