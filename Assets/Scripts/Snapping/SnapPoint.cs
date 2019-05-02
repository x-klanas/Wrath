using System.Linq;
using UnityEngine;

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

        [Header("Snap settings")] [Min(0)] public float snapBreakDistance = 0f;
        public bool unsnapOnTriggerExit = true;

        [Min(0)] public float positionSpring = 1000f;
        [Min(0)] public float positionDamper = 100f;
        [Min(0)] public float positionMaxForce = 1000f;

        [Min(0)] public float rotationSpring = 100f;
        [Min(0)] public float rotationDamper = 10f;
        [Min(0)] public float rotationMaxForce = 100f;

        [Header("Sticky snap settings")] [Min(0)]
        public float stickyBreakDistance = 0f;

        public bool unsnapStickyOnTriggerExit = true;

        [Min(0)] public float stickyPositionSpring = 1000f;
        [Min(0)] public float stickyPositionDamper = 100f;
        [Min(0)] public float stickyPositionMaxForce = 1000f;

        [Min(0)] public float stickyRotationSpring = 100f;
        [Min(0)] public float stickyRotationDamper = 10f;
        [Min(0)] public float stickyRotationMaxForce = 100f;

        [Header("Other settings")] public Snappable snappable;

        public Rigidbody Rigidbody => snappable.rigidbody;

        public bool IsSnapped { get; private set; }
        public bool IsSnapParent { get; private set; }
        public bool IsSticky { get; private set; }

        public ConfigurableJoint SnappedJoint { get; private set; }
        public SnapPoint SnappedPoint { get; private set; }

        private void Start() {
            if (!snappable) {
                snappable = GetComponentInParent<Snappable>();
            }

            snappable.AddSnapPoint(this);
        }

        private void FixedUpdate() {
            if (IsSnapped) {
                float breakDistance = IsSticky ? stickyBreakDistance : snapBreakDistance;

                if (breakDistance > 0) {
                    float distance = Vector3.Distance(transform.position, SnappedPoint.transform.position);

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

            if (otherSnapPoint == null || !otherSnapPoint.snappable.canSnap || otherSnapPoint.IsSnapped
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
            Transform trans = transform;
            Transform rigidTrans = Rigidbody.transform;
            float otherMass = otherSnapPoint.Rigidbody.mass;

            SnappedJoint = Rigidbody.gameObject.AddComponent<ConfigurableJoint>();

            SnappedJoint.connectedBody = otherSnapPoint.Rigidbody;
            SnappedJoint.autoConfigureConnectedAnchor = false;
            SnappedJoint.anchor = rigidTrans.InverseTransformPoint(trans.position);
            SnappedJoint.connectedAnchor = otherSnapPoint.Rigidbody.transform.InverseTransformPoint(otherSnapPoint.transform.position);
            SnappedJoint.enableCollision = true;

            SnappedJoint.SetLocalTargetRotation(GetDesiredJointLocalRotation(otherSnapPoint), rigidTrans.localRotation);

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
        }

        public void Unsnap() {
            if (IsSnapped && !IsSticky && !SnappedPoint.IsSticky) {
                if (IsSnapParent) {
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
                } else {
                    SnappedPoint.Unstick();
                }
            }
        }

        public Quaternion GetDesiredJointLocalRotation(SnapPoint otherSnapPoint) {
            Transform rigidTrans = Rigidbody.transform;
            Quaternion rigidbodyRotation = rigidTrans.rotation;

            Quaternion relativeObjectAndOtherPointOrientation = Quaternion.Inverse(rigidbodyRotation) * otherSnapPoint.transform.rotation;
            Quaternion relativePointAndObjectOrientation = Quaternion.Inverse(transform.rotation) * rigidbodyRotation;
            Quaternion relativeRotatedPointAndObjectOrientation = relativeObjectAndOtherPointOrientation * Quaternion.Euler(otherSnapPoint.snappedRotation) * relativePointAndObjectOrientation;

            return rigidTrans.localRotation * relativeRotatedPointAndObjectOrientation;
        }

        public Quaternion GetDesiredChildLocalRotation(SnapPoint otherSnapPoint) {
            Quaternion rotation = transform.rotation;
            Transform rigidTrans = Rigidbody.transform;

            Quaternion relativePointAndOtherPointOrientation = Quaternion.Inverse(rotation) * otherSnapPoint.transform.rotation;
            Quaternion relativePointAndObjectOrientation = Quaternion.Inverse(rotation) * rigidTrans.rotation;
            Quaternion relativeOrientation = relativePointAndOtherPointOrientation * Quaternion.Euler(snappedRotation) * relativePointAndObjectOrientation;

            return rigidTrans.localRotation * relativeOrientation;
        }
    }
}