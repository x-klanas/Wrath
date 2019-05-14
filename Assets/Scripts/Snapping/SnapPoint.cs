using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Snapping {
    [RequireComponent(typeof(Collider))]
    public class SnapPoint : MonoBehaviour {
        public string[] snapsTo = {
            "default"
        };

        public string[] snapFor = {
            "default"
        };

        public Vector3 snappedRotation;

        [Header("Snap settings")]
        [Min(0)]
        public float snapBreakDistance = 0f;

        public bool unsnapOnTriggerExit = true;

        [Min(0)] public float positionSpring = 1000f;
        [Min(0)] public float positionDamper = 100f;
        [Min(0)] public float positionMaxForce = 1000f;

        [Min(0)] public float rotationSpring = 100f;
        [Min(0)] public float rotationDamper = 10f;
        [Min(0)] public float rotationMaxForce = 100f;

        [Min(0)] public float snapParentMassScale = 0f;
        [Min(0)] public float snapChildMassScale = 1f;

        [Header("Sticky snap settings")]
        [Min(0)]
        public float stickyBreakDistance = 0f;

        public bool unsnapStickyOnTriggerExit = true;

        [Min(0)] public float stickyPositionSpring = 1000f;
        [Min(0)] public float stickyPositionDamper = 100f;
        [Min(0)] public float stickyPositionMaxForce = 1000f;

        [Min(0)] public float stickyRotationSpring = 100f;
        [Min(0)] public float stickyRotationDamper = 10f;
        [Min(0)] public float stickyRotationMaxForce = 100f;

        [Min(0)] public float stickyParentMassScale = 1f;
        [Min(0)] public float stickyChildMassScale = 1f;

        [Header("Other settings")]
        public Vector3 axis = Vector3.right;

        public Vector3 secondaryAxis = Vector3.up;
        public Snappable snappable;
        public Transform anchorPoint;

        public Rigidbody Rigidbody => snappable.rigidbody;

        public bool IsSnapped { get; private set; }
        public bool IsSnapParent { get; private set; }
        public bool IsSticky { get; private set; }

        public ConfigurableJoint SnappedJoint { get; private set; }
        public SnapPoint SnappedPoint { get; private set; }

        public delegate void OnSnapPointHandler(SnapPoint snapPoint);

        public event OnSnapPointHandler OnSnap;
        public event OnSnapPointHandler OnUnsnap;
        public event OnSnapPointHandler OnStick;
        public event OnSnapPointHandler OnUnstick;

        private void Start() {
            if (!snappable) {
                snappable = GetComponentInParent<Snappable>();
            }

            if (!anchorPoint) {
                anchorPoint = transform;
            }

            snappable.AddSnapPoint(this);
        }

        private void FixedUpdate() {
            if (IsSnapped) {
                float breakDistance = IsSticky ? stickyBreakDistance : snapBreakDistance;

                if (breakDistance > 0) {
                    float distance = Vector3.Distance(anchorPoint.position, SnappedPoint.anchorPoint.position);

                    if (distance > breakDistance) {
                        Unstick();
                        Unsnap();
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (IsSnapped) {
                return;
            }

            SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();

            if (otherSnapPoint == null || !otherSnapPoint.snappable.CanSnap || otherSnapPoint.IsSnapped
                || !snapFor.Any(f => otherSnapPoint.snapsTo.Contains(f))) {
                return;
            }

            SnapTo(otherSnapPoint);
        }

        private void OnTriggerExit(Collider other) {
            if (!IsSnapped || !unsnapOnTriggerExit && !unsnapStickyOnTriggerExit) {
                return;
            }

            SnapPoint otherSnapPoint = other.GetComponent<SnapPoint>();

            if (otherSnapPoint == SnappedPoint) {
                if (IsSticky && unsnapStickyOnTriggerExit) {
                    Unstick();
                    Unsnap();
                } else if (unsnapOnTriggerExit) {
                    Unsnap();
                }
            }
        }

        public void SnapTo(SnapPoint otherSnapPoint) {
            Transform rigidTrans = Rigidbody.transform;

            SnappedJoint = Rigidbody.gameObject.AddComponent<ConfigurableJoint>();

            SnappedJoint.autoConfigureConnectedAnchor = false;
            SnappedJoint.anchor = rigidTrans.InverseTransformPoint(anchorPoint.position);
            SnappedJoint.connectedAnchor = otherSnapPoint.Rigidbody.transform.InverseTransformPoint(otherSnapPoint.anchorPoint.position);
            SnappedJoint.axis = axis;
            SnappedJoint.secondaryAxis = secondaryAxis;
            SnappedJoint.enableCollision = true;

            IsSnapped = true;
            IsSnapParent = true;
            IsSticky = false;
            SnappedPoint = otherSnapPoint;
            snappable.PointSnapped(this);

            otherSnapPoint.IsSnapped = true;
            otherSnapPoint.IsSticky = false;
            otherSnapPoint.SnappedPoint = this;
            otherSnapPoint.SnappedJoint = SnappedJoint;
            otherSnapPoint.snappable.PointSnapped(otherSnapPoint);

            UpdateRotation();
            UpdateProperties();

            OnSnap?.Invoke(this);
            otherSnapPoint.OnSnap?.Invoke(otherSnapPoint);
        }

        public void Unsnap() {
            if (IsSnapped && !IsSticky && !SnappedPoint.IsSticky) {
                if (IsSnapParent) {
                    OnUnsnap?.Invoke(this);
                    SnappedPoint.OnUnsnap?.Invoke(SnappedPoint);

                    Destroy(SnappedJoint);

                    IsSnapped = false;
                    IsSnapParent = false;
                    snappable.PointUnsnapped(this);

                    SnappedPoint.IsSnapped = false;
                    SnappedPoint.SnappedJoint = null;
                    SnappedPoint.SnappedPoint = null;
                    SnappedPoint.snappable.PointUnsnapped(SnappedPoint);

                    IsSnapped = false;
                    SnappedPoint = null;
                    SnappedJoint = null;
                } else {
                    SnappedPoint.Unsnap();
                }
            }
        }

        public void Stick() {
            if (IsSnapped && (!IsSticky || !SnappedPoint.IsSticky)) {
                if (IsSnapParent) {
                    IsSticky = true;
                    SnappedPoint.IsSticky = true;

                    UpdateProperties();

                    OnStick?.Invoke(this);
                    SnappedPoint.OnStick?.Invoke(SnappedPoint);
                } else {
                    SnappedPoint.Stick();
                }
            }
        }

        public void Unstick() {
            if (IsSnapped && (IsSticky || SnappedPoint.IsSticky)) {
                if (IsSnapParent) {
                    IsSticky = false;
                    SnappedPoint.IsSticky = false;

                    UpdateProperties();

                    OnUnstick?.Invoke(this);
                    SnappedPoint.OnUnstick?.Invoke(SnappedPoint);
                } else {
                    SnappedPoint.Unstick();
                }
            }
        }

        public void UpdateProperties() {
            if (IsSnapped) {
                if (IsSticky) {
                    if (IsSnapParent) {
                        SnappedJoint.massScale = stickyParentMassScale;
                        SnappedJoint.connectedMassScale = stickyChildMassScale;

                        float otherMass = SnappedPoint.Rigidbody.mass;

                        SnappedJoint.xDrive = SnappedJoint.yDrive = SnappedJoint.zDrive = new JointDrive {
                            positionSpring = stickyPositionSpring * otherMass,
                            positionDamper = stickyPositionDamper * otherMass,
                            maximumForce = stickyPositionMaxForce * otherMass
                        };

                        SnappedJoint.angularXDrive = SnappedJoint.angularYZDrive = new JointDrive {
                            positionSpring = stickyRotationSpring * otherMass,
                            positionDamper = stickyRotationDamper * otherMass,
                            maximumForce = stickyRotationMaxForce * otherMass
                        };
                    }
                } else {
                    if (IsSnapParent) {
                        SnappedJoint.massScale = snapParentMassScale;
                        SnappedJoint.connectedMassScale = snapChildMassScale;

                        float otherMass = SnappedPoint.Rigidbody.mass;

                        SnappedJoint.xDrive = SnappedJoint.yDrive = SnappedJoint.zDrive = new JointDrive {
                            positionSpring = positionSpring * otherMass,
                            positionDamper = positionDamper * otherMass,
                            maximumForce = positionMaxForce * otherMass
                        };

                        SnappedJoint.angularXDrive = SnappedJoint.angularYZDrive = new JointDrive {
                            positionSpring = rotationSpring * otherMass,
                            positionDamper = rotationDamper * otherMass,
                            maximumForce = rotationMaxForce * otherMass
                        };
                    }
                }
            }
        }

        public void UpdateRotation() {
            if (IsSnapped) {
                if (IsSnapParent) {
                    Transform rigidTrans = Rigidbody.transform;

                    Quaternion targetRigidbodyRotation = GetTargetJointLocalRotation(SnappedPoint);
                    Quaternion localRigidbodyRotation = rigidTrans.localRotation;

                    SnappedJoint.connectedBody = null;
                    rigidTrans.localRotation = targetRigidbodyRotation;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    SnappedJoint.connectedBody = SnappedPoint.Rigidbody;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    rigidTrans.localRotation = localRigidbodyRotation;
                } else {
                    Transform rigidTrans = SnappedPoint.Rigidbody.transform;

                    Quaternion targetRigidbodyRotation = SnappedPoint.GetTargetJointLocalRotation(this);
                    Quaternion localRigidbodyRotation = rigidTrans.localRotation;

                    SnappedJoint.connectedBody = null;
                    rigidTrans.localRotation = targetRigidbodyRotation;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    SnappedJoint.connectedBody = Rigidbody;
                    // ReSharper disable once Unity.InefficientPropertyAccess
                    rigidTrans.localRotation = localRigidbodyRotation;
                }
            }
        }

        public void UpdateAnchorPosition() {
            if (IsSnapped) {
                if (IsSnapParent) {
                    SnappedJoint.anchor = Rigidbody.transform.InverseTransformPoint(anchorPoint.position);
                    SnappedJoint.connectedBody = SnappedJoint.connectedBody;
                } else {
                    SnappedJoint.connectedAnchor = Rigidbody.transform.InverseTransformPoint(anchorPoint.position);
                    SnappedJoint.connectedBody = SnappedJoint.connectedBody;
                }
            }
        }

        public Quaternion GetTargetJointLocalRotation(SnapPoint otherSnapPoint) {
            Transform rigidTrans = Rigidbody.transform;
            Quaternion rigidbodyRotation = rigidTrans.rotation;

            Quaternion relativeObjectAndOtherPointOrientation = Quaternion.Inverse(rigidbodyRotation) * otherSnapPoint.anchorPoint.rotation;
            Quaternion relativePointAndObjectOrientation = Quaternion.Inverse(anchorPoint.rotation) * rigidbodyRotation;
            Quaternion relativeRotatedPointAndObjectOrientation = relativeObjectAndOtherPointOrientation * Quaternion.Euler(otherSnapPoint.snappedRotation) * relativePointAndObjectOrientation;

            return rigidTrans.localRotation * relativeRotatedPointAndObjectOrientation;
        }

        public Quaternion GetTargetChildLocalRotation(SnapPoint otherSnapPoint) {
            Quaternion rotation = anchorPoint.rotation;
            Transform rigidTrans = Rigidbody.transform;

            Quaternion relativePointAndOtherPointOrientation = Quaternion.Inverse(rotation) * otherSnapPoint.anchorPoint.rotation;
            Quaternion relativePointAndObjectOrientation = Quaternion.Inverse(rotation) * rigidTrans.rotation;
            Quaternion relativeOrientation = relativePointAndOtherPointOrientation * Quaternion.Euler(snappedRotation) * relativePointAndObjectOrientation;

            return rigidTrans.localRotation * relativeOrientation;
        }
    }
}