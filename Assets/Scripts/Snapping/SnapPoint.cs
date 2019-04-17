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

        public float snapBreakDistance = 0.5f;

        public Vector3 snappedRotation;

        public Snappable snappable;

        public Rigidbody Rigidbody => snappable.rigidbody;

        public bool IsSnapped { get; private set; }
        public bool IsSnapParent { get; private set; }

        private ConfigurableJoint snappedJoint;
        private SnapPoint snappedPoint;
        private bool snappedUseGravity;

        private void Start() {
            if (snappable == null) {
                snappable = GetComponentInParent<Snappable>();
            }

            snappable.AddSnapPoint(this);
        }

        private void FixedUpdate() {
            if (IsSnapped) {
                float distance = (transform.position - snappedPoint.transform.position).magnitude;

                if (distance >= snapBreakDistance) {
                    Unsnap();
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

        public void SnapTo(SnapPoint otherSnapPoint) {
            Transform trans = transform;
            Transform rigidTrans = Rigidbody.transform;

            snappedJoint = Rigidbody.gameObject.AddComponent<ConfigurableJoint>();

            snappedJoint.connectedBody = otherSnapPoint.Rigidbody;
            snappedJoint.autoConfigureConnectedAnchor = false;
            snappedJoint.anchor = rigidTrans.InverseTransformPoint(trans.position);
            snappedJoint.connectedAnchor = otherSnapPoint.Rigidbody.transform.InverseTransformPoint(otherSnapPoint.transform.position);
            snappedJoint.enableCollision = false;

            // This is probably an overkill, there must be a less expensive solution (right?)
            Quaternion rotation = trans.rotation;
            Quaternion rigidbodyRotation = rigidTrans.rotation;
            Quaternion rigidbodyLocalRotation = rigidTrans.localRotation;
            Quaternion relativePointOrientation = Quaternion.Inverse(rotation) * otherSnapPoint.transform.rotation;
            Quaternion rotatedPoint = rotation * relativePointOrientation * Quaternion.Euler(otherSnapPoint.snappedRotation);
            Quaternion relativePointAndObjectOrientation = Quaternion.Inverse(rotation) * rigidbodyRotation;
            Quaternion relativeRotatedPointAndObjectOrientation = Quaternion.Inverse(rigidbodyRotation) * rotatedPoint * relativePointAndObjectOrientation;
            snappedJoint.SetLocalTargetRotation(
                rigidbodyLocalRotation * relativeRotatedPointAndObjectOrientation,
                rigidbodyLocalRotation);

            snappedJoint.xDrive = snappedJoint.yDrive = snappedJoint.zDrive = new JointDrive {
                positionSpring = 1000f,
                positionDamper = 10f,
                maximumForce = 100f
            };

            snappedJoint.angularXDrive = snappedJoint.angularYZDrive = new JointDrive {
                positionSpring = 100f,
                positionDamper = 10f,
                maximumForce = 100f
            };

            snappedUseGravity = otherSnapPoint.Rigidbody.useGravity;
            otherSnapPoint.Rigidbody.useGravity = false;

            IsSnapped = true;
            IsSnapParent = true;
            snappedPoint = otherSnapPoint;

            otherSnapPoint.IsSnapped = true;
            otherSnapPoint.snappedPoint = this;
            otherSnapPoint.snappedJoint = snappedJoint;
        }

        public void Unsnap() {
            if (IsSnapped) {
                if (IsSnapParent) {
                    Destroy(snappedJoint);

                    IsSnapped = false;
                    IsSnapParent = false;

                    snappedPoint.IsSnapped = false;
                    snappedPoint.snappedJoint = null;
                    snappedPoint.snappedPoint = null;
                    snappedPoint.Rigidbody.useGravity = snappedUseGravity;

                    IsSnapped = false;
                    snappedPoint = null;
                    snappedJoint = null;
                } else {
                    snappedPoint.Unsnap();
                }
            }
        }
    }
}