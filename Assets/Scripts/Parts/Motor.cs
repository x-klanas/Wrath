using System.Collections.Generic;
using Snapping;
using UnityEngine;

namespace Parts {
    public class Motor : MonoBehaviour {
        public bool findSpinnableSnapPoints = true;
        public List<SpinnableSnapPoint> spinnableSnapPoints = new List<SpinnableSnapPoint>();

        public bool isOn;

        [Header("Snapped config")] public bool enableWhenSnapped = false;

        public bool EnableWhenSnapped {
            get => enableWhenSnapped;
            set {
                enableWhenSnapped = value;
                UpdateProperties();
            }
        }

        public float snapTargetSpeed;

        public float SnapTargetSpeed {
            get => snapTargetSpeed;
            set {
                snapTargetSpeed = value;
                UpdateProperties();
            }
        }

        public float snapTargetTorque;

        public float SnapTargetTorque {
            get => snapTargetTorque;
            set {
                snapTargetTorque = value;
                UpdateProperties();
            }
        }

        public float snapMaximumForce;

        public float SnapMaximumForce {
            get => snapMaximumForce;
            set {
                snapMaximumForce = value;
                UpdateProperties();
            }
        }

        public float snapBreakingTorque;

        public float SnapBreakingTorque {
            get => snapBreakingTorque;
            set {
                snapBreakingTorque = value;
                UpdateProperties();
            }
        }

        public float snapBreakingMaxForce;

        public float SnapBreakingMaxForce {
            get => snapBreakingMaxForce;
            set {
                snapBreakingMaxForce = value;
                UpdateProperties();
            }
        }

        [Header("Sticky config")] public bool enableWhenSticky = true;

        public bool EnableWhenSticky {
            get => enableWhenSticky;
            set {
                enableWhenSticky = value;
                UpdateProperties();
            }
        }

        public float stickyTargetSpeed = 10f;

        public float StickyTargetSpeed {
            get => stickyTargetSpeed;
            set {
                stickyTargetSpeed = value;
                UpdateProperties();
            }
        }

        public float stickyTargetTorque = 5f;

        public float StickyTargetTorque {
            get => stickyTargetTorque;
            set {
                stickyTargetTorque = value;
                UpdateProperties();
            }
        }

        public float stickyMaximumForce = 10f;

        public float StickyMaximumForce {
            get => stickyMaximumForce;
            set {
                stickyMaximumForce = value;
                UpdateProperties();
            }
        }

        public float stickyBreakingTorque;

        public float StickyBreakingTorque {
            get => stickyBreakingTorque;
            set {
                stickyBreakingTorque = value;
                UpdateProperties();
            }
        }

        public float stickyBreakingMaxForce;

        public float StickyBreakingMaxForce {
            get => stickyBreakingMaxForce;
            set {
                stickyBreakingMaxForce = value;
                UpdateProperties();
            }
        }

        private void Start() {
            if (findSpinnableSnapPoints) {
                spinnableSnapPoints.AddRange(GetComponentsInChildren<SpinnableSnapPoint>());
            }

            if (isOn) {
                TurnOn();
            } else {
                TurnOff();
            }
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.KeypadEnter)) {
                if (isOn) {
                    TurnOff();
                } else {
                    TurnOn();
                }
            }
        }

        public void TurnOn() {
            isOn = true;
            UpdateProperties();
        }

        public void TurnOff() {
            isOn = false;
            UpdateProperties();
        }

        private void UpdateProperties() {
            foreach (SpinnableSnapPoint spinnableSnapPoint in spinnableSnapPoints) {
                if (isOn) {
                    if (enableWhenSnapped) {
                        spinnableSnapPoint.SnapTargetSpeed = snapTargetSpeed;
                        spinnableSnapPoint.SnapTargetTorque = snapTargetTorque;
                        spinnableSnapPoint.SnapMaximumForce = snapMaximumForce;
                    } else if (enableWhenSticky) {
                        spinnableSnapPoint.StickyTargetSpeed = stickyTargetSpeed;
                        spinnableSnapPoint.StickyTargetTorque = stickyTargetTorque;
                        spinnableSnapPoint.StickyMaximumForce = stickyMaximumForce;
                    }
                } else {
                    spinnableSnapPoint.SnapTargetSpeed = 0f;
                    spinnableSnapPoint.SnapTargetTorque = snapBreakingTorque;
                    spinnableSnapPoint.SnapMaximumForce = snapBreakingMaxForce;

                    spinnableSnapPoint.StickyTargetSpeed = 0f;
                    spinnableSnapPoint.StickyTargetTorque = stickyBreakingTorque;
                    spinnableSnapPoint.StickyMaximumForce = stickyBreakingMaxForce;
                }
            }
        }
    }
}