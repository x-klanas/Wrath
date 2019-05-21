using System.Collections.Generic;
using UnityEngine;

namespace Snapping {
    public class ReinforceableSnapPoint : MonoBehaviour {
        public bool applyToOther = true;

        public List<FullSpringSettings> appliedSpringSettings = new List<FullSpringSettings>();

        public SnapPoint snapPoint;

        private FullSpringSettings initialSpring;
        private SnapPoint targetPoint;

        private void Awake() {
            if (snapPoint == null) {
                snapPoint = GetComponentInChildren<SnapPoint>();
            }
        }

        private void Start() {
            snapPoint.OnSnap += OnSnap;
            snapPoint.OnUnsnap += OnUnsnap;
            snapPoint.OnStick += OnStick;
            snapPoint.OnUnstick += OnUnstick;
        }

        private void OnSnap() {
            targetPoint = applyToOther ? snapPoint.SnappedPoint : snapPoint;
            initialSpring = targetPoint.stickySpring;
        }

        private void OnUnsnap() {
            targetPoint.stickySpring = initialSpring;
        }

        private void OnStick() {
            UpdateSpringSettings();
        }

        private void OnUnstick() {
            targetPoint.stickySpring = initialSpring;
        }

        public void UpdateSpringSettings() {
            if (targetPoint) {
                FullSpringSettings spring = new FullSpringSettings(initialSpring);

                foreach (FullSpringSettings springSetting in appliedSpringSettings) {
                    spring.Add(springSetting);
                }

                targetPoint.stickySpring = spring;
                targetPoint.UpdateProperties();
            }
        }
    }
}