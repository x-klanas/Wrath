using UnityEngine;

namespace Snapping {
    [RequireComponent(typeof(SnapPoint))]
    public class SpinnableSnapPoint : MonoBehaviour {
        [Header("Snapped config")]
        [SerializeField]
        private bool enableWhenSnapped = true;

        public bool EnableWhenSnapped {
            get => enableWhenSnapped;
            set {
                enableWhenSnapped = value;

                if (snapPoint.IsSnapped && !snapPoint.IsSticky) {
                    if (enableWhenSnapped)
                        SaveProperties(snapPoint.SnappedJoint, snapProperties);
                    else
                        ResetProperties(snapPoint.SnappedJoint, snapProperties);
                }

                UpdateProperties();
            }
        }

        [SerializeField] private float snapTargetSpeed;

        public float SnapTargetSpeed {
            get => snapTargetSpeed;
            set {
                snapTargetSpeed = value;
                UpdateProperties();
            }
        }

        [SerializeField] private float snapTargetTorque;

        public float SnapTargetTorque {
            get => snapTargetTorque;
            set {
                snapTargetTorque = value;
                UpdateProperties();
            }
        }

        [SerializeField] private float snapMaximumForce;

        public float SnapMaximumForce {
            get => snapMaximumForce;
            set {
                snapMaximumForce = value;
                UpdateProperties();
            }
        }

        [Header("Sticky config")]
        [SerializeField]
        private bool enableWhenSticky = true;

        public bool EnableWhenSticky {
            get => enableWhenSticky;
            set {
                enableWhenSticky = value;

                if (snapPoint.IsSnapped && snapPoint.IsSticky) {
                    if (enableWhenSticky)
                        SaveProperties(snapPoint.SnappedJoint, stickyProperties);
                    else
                        ResetProperties(snapPoint.SnappedJoint, stickyProperties);
                }

                UpdateProperties();
            }
        }

        [SerializeField] private float stickyTargetSpeed;

        public float StickyTargetSpeed {
            get => stickyTargetSpeed;
            set {
                stickyTargetSpeed = value;
                UpdateProperties();
            }
        }

        [SerializeField] private float stickyTargetTorque;

        public float StickyTargetTorque {
            get => stickyTargetTorque;
            set {
                stickyTargetTorque = value;
                UpdateProperties();
            }
        }

        [SerializeField] private float stickyMaximumForce;

        public float StickyMaximumForce {
            get => stickyMaximumForce;
            set {
                stickyMaximumForce = value;
                UpdateProperties();
            }
        }

        [Header("Other settings")]
        public SnapPoint snapPoint;

        private SpinnableOverridenProperties snapProperties;
        private SpinnableOverridenProperties stickyProperties;

        private void Awake() {
            if (snapPoint == null) {
                snapPoint = GetComponent<SnapPoint>();
            }
        }

        private void Start() {
            snapPoint.OnSnap += OnSnap;
            snapPoint.OnUnsnap += OnUnsnap;
            snapPoint.OnStick += OnStick;
            snapPoint.OnUnstick += OnUnstick;
        }

        private void OnSnap() {
            if (enableWhenSnapped) {
                SaveProperties(snapPoint.SnappedJoint, snapProperties);
                UpdateProperties();
            }
        }

        private void OnUnsnap() {
            snapProperties.IsSaved = false;
            stickyProperties.IsSaved = false;
        }

        private void OnStick() {
            if (enableWhenSticky) {
                SaveProperties(snapPoint.SnappedJoint, stickyProperties);
                UpdateProperties();
            }
        }

        private void OnUnstick() {
            UpdateProperties();
        }

        private void UpdateProperties() {
            if (snapPoint && snapPoint.IsSnapped) {
                ConfigurableJoint joint = snapPoint.SnappedJoint;

                if (enableWhenSnapped && !snapPoint.IsSticky) {
                    joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
                    joint.angularXMotion = ConfigurableJointMotion.Free;

                    joint.angularXDrive = new JointDrive {
                        positionDamper = snapTargetTorque,
                        maximumForce = snapMaximumForce
                    };

                    joint.angularYZDrive = new JointDrive();

                    joint.targetAngularVelocity = new Vector3(snapTargetSpeed, 0f, 0f);
                } else if (enableWhenSticky && snapPoint.IsSticky) {
                    joint.angularYMotion = joint.angularZMotion = ConfigurableJointMotion.Limited;
                    joint.angularXMotion = ConfigurableJointMotion.Free;

                    joint.angularXDrive = new JointDrive {
                        positionDamper = stickyTargetTorque,
                        maximumForce = stickyMaximumForce
                    };

                    joint.angularYZDrive = new JointDrive();

                    joint.targetAngularVelocity = new Vector3(stickyTargetSpeed, 0f, 0f);
                }
            }
        }

        private void SaveProperties(ConfigurableJoint joint, SpinnableOverridenProperties properties) {
            if (!properties.IsSaved) {
                properties.AngularXMotion = joint.angularXMotion;
                properties.AngularYMotion = joint.angularYMotion;
                properties.AngularZMotion = joint.angularZMotion;

                properties.AngularXDrive = joint.angularXDrive;
                properties.AngularYZDrive = joint.angularYZDrive;

                properties.IsSaved = true;
            }
        }

        private void ResetProperties(ConfigurableJoint joint, SpinnableOverridenProperties properties) {
            if (properties.IsSaved) {
                joint.angularXMotion = properties.AngularXMotion;
                joint.angularYMotion = properties.AngularYMotion;
                joint.angularZMotion = properties.AngularZMotion;

                joint.angularXDrive = properties.AngularXDrive;
                joint.angularYZDrive = properties.AngularYZDrive;

                properties.IsSaved = false;
            }
        }

        private struct SpinnableOverridenProperties {
            public bool IsSaved { get; set; }

            public ConfigurableJointMotion AngularXMotion { get; set; }
            public ConfigurableJointMotion AngularYMotion { get; set; }
            public ConfigurableJointMotion AngularZMotion { get; set; }

            public JointDrive AngularXDrive { get; set; }
            public JointDrive AngularYZDrive { get; set; }
        }
    }
}