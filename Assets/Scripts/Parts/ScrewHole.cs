using Snapping;
using UnityEngine;

namespace Parts {
    [RequireComponent(typeof(SnapPoint))]
    public class ScrewHole : MonoBehaviour {
        public FullSpringSettings spring = new FullSpringSettings();

        [Range(0, 1)]
        public float reinforcementStart = 0f;
        [Range(0, 1)]
        public float reinforcementEnd = 1f;
        
        public bool applyScrewSettings = true;

        public ReinforceableSnapPoint reinforceableSnapPoint;
        public SnapPoint screwSnapPoint;

        private Screw screw;
        private readonly FullSpringSettings appliedSpringSettings = new FullSpringSettings();

        private void Awake() {
            if (screwSnapPoint == null) {
                screwSnapPoint = GetComponentInChildren<SnapPoint>();
            }
        }

        private void Start() {
            screwSnapPoint.OnSnap += OnScrewSnap;
            screwSnapPoint.OnUnsnap += OnScrewUnsnap;
            screwSnapPoint.OnStick += OnScrewStick;
            screwSnapPoint.OnUnstick += OnScrewUnstick;

            reinforceableSnapPoint.snapPoint.OnPreSnap += CancelOnSnapPoint;
            reinforceableSnapPoint.snapPoint.OnPreStick += CancelOnSnapPoint;
            reinforceableSnapPoint.snapPoint.OnPreUnstick += CancelOnSnapPoint;
        }

        private void CancelOnSnapPoint(SnapPoint.PreSnapPointEvent preSnapPointEvent) {
            if (screw && screw.ScrewValue > reinforcementStart) {
                preSnapPointEvent.Cancel();
            }
        }

        private void OnScrewSnap() {
            screw = screwSnapPoint.SnappedPoint.Rigidbody.GetComponentInChildren<Screw>();
            appliedSpringSettings.Reset();
            reinforceableSnapPoint.appliedSpringSettings.Add(appliedSpringSettings);
        }

        private void OnScrewUnsnap() {
            screw = null;
            reinforceableSnapPoint.appliedSpringSettings.Remove(appliedSpringSettings);
            reinforceableSnapPoint.UpdateSpringSettings();
        }

        private void OnScrewStick() {
            if (screw) {
                screw.OnScrewValue += UpdateProperties;
                UpdateProperties(screw.ScrewValue);
            }
        }

        private void OnScrewUnstick() {
            if (screw) {
                screw.OnScrewValue -= UpdateProperties;
                appliedSpringSettings.Reset();
                reinforceableSnapPoint.UpdateSpringSettings();
            }
        }

        private void UpdateProperties(float screwValue) {
            if (screw) {
                float adjustedScrewValue = GetAdjustedScrewValue(screwValue);
                
                appliedSpringSettings.position.Set(new SpringSettings(
                    Mathf.Lerp(0, spring.position.spring, adjustedScrewValue),
                    Mathf.Lerp(0, spring.position.damper, adjustedScrewValue),
                    Mathf.Lerp(0, spring.position.maxForce, adjustedScrewValue)
                ));

                appliedSpringSettings.rotation.Set(new SpringSettings(
                    Mathf.Lerp(0, spring.rotation.spring, adjustedScrewValue),
                    Mathf.Lerp(0, spring.rotation.damper, adjustedScrewValue),
                    Mathf.Lerp(0, spring.rotation.maxForce, adjustedScrewValue)
                ));

                if (applyScrewSettings) {
                    appliedSpringSettings.position.Add(new SpringSettings(
                        Mathf.Lerp(0, screw.spring.position.spring, adjustedScrewValue),
                        Mathf.Lerp(0, screw.spring.position.damper, adjustedScrewValue),
                        Mathf.Lerp(0, screw.spring.position.maxForce, adjustedScrewValue)
                    ));

                    appliedSpringSettings.rotation.Add(new SpringSettings(
                        Mathf.Lerp(0, screw.spring.rotation.spring, adjustedScrewValue),
                        Mathf.Lerp(0, screw.spring.rotation.damper, adjustedScrewValue),
                        Mathf.Lerp(0, screw.spring.rotation.maxForce, adjustedScrewValue)
                    ));
                }

                reinforceableSnapPoint.UpdateSpringSettings();
            }
        }

        private float GetAdjustedScrewValue(float screwValue) {
            return Mathf.Clamp01((screwValue - reinforcementStart) / (reinforcementEnd - reinforcementStart));
        }
    }
}