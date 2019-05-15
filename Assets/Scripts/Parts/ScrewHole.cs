using Snapping;
using UnityEngine;

namespace Parts {
    [RequireComponent(typeof(SnapPoint))]
    public class ScrewHole : MonoBehaviour {
        public FullSpringSettings spring = new FullSpringSettings();

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
                appliedSpringSettings.position.Set(new SpringSettings(
                    Mathf.Lerp(0, spring.position.spring, screwValue),
                    Mathf.Lerp(0, spring.position.damper, screwValue),
                    Mathf.Lerp(0, spring.position.maxForce, screwValue)
                ));

                appliedSpringSettings.rotation.Set(new SpringSettings(
                    Mathf.Lerp(0, spring.rotation.spring, screwValue),
                    Mathf.Lerp(0, spring.rotation.damper, screwValue),
                    Mathf.Lerp(0, spring.rotation.maxForce, screwValue)
                ));

                if (applyScrewSettings) {
                    appliedSpringSettings.position.Add(new SpringSettings(
                        Mathf.Lerp(0, screw.spring.position.spring, screwValue),
                        Mathf.Lerp(0, screw.spring.position.damper, screwValue),
                        Mathf.Lerp(0, screw.spring.position.maxForce, screwValue)
                    ));

                    appliedSpringSettings.rotation.Add(new SpringSettings(
                        Mathf.Lerp(0, screw.spring.rotation.spring, screwValue),
                        Mathf.Lerp(0, screw.spring.rotation.damper, screwValue),
                        Mathf.Lerp(0, screw.spring.rotation.maxForce, screwValue)
                    ));
                }

                reinforceableSnapPoint.UpdateSpringSettings();
            }
        }
    }
}