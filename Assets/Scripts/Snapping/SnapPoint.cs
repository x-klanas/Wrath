using System.Collections.Generic;
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

        [Header("Snap settings")]
        [Min(0)]
        public float snapBreakDistance = 0f;

        public bool unsnapOnTriggerExit = true;

        public FullSpringSettings spring = new FullSpringSettings(
            new SpringSettings(1000f, 100f, 1000f),
            new SpringSettings(100f, 10f, 100f)
        );

        [Min(0)] public float snapParentMassScale = 0f;
        [Min(0)] public float snapChildMassScale = 1f;

        [Header("Sticky snap settings")]
        [Min(0)]
        public float stickyBreakDistance = 0f;

        public bool unsnapStickyOnTriggerExit = true;

        public FullSpringSettings stickySpring = new FullSpringSettings(
            new SpringSettings(1000f, 100f, 1000f),
            new SpringSettings(100f, 10f, 100f)
        );

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

        public delegate void OnSnapPointHandler();

        public event OnSnapPointHandler OnSnap;
        public event OnSnapPointHandler OnUnsnap;
        public event OnSnapPointHandler OnStick;
        public event OnSnapPointHandler OnUnstick;

        public class PreSnapPointEvent {
            public bool Canceled { get; private set; }
            public void Cancel() => Canceled = true;
        }

        public delegate void OnPreSnapPointHandler(PreSnapPointEvent preSnapPointEvent);

        private readonly List<OnPreSnapPointHandler> onPreSnap = new List<OnPreSnapPointHandler>();

        public event OnPreSnapPointHandler OnPreSnap {
            add => onPreSnap.Add(value);
            remove => onPreSnap.Remove(value);
        }

        private readonly List<OnPreSnapPointHandler> onPreUnsnap = new List<OnPreSnapPointHandler>();

        public event OnPreSnapPointHandler OnPreUnsnap {
            add => onPreUnsnap.Add(value);
            remove => onPreUnsnap.Remove(value);
        }

        private readonly List<OnPreSnapPointHandler> onPreStick = new List<OnPreSnapPointHandler>();

        public event OnPreSnapPointHandler OnPreStick {
            add => onPreStick.Add(value);
            remove => onPreStick.Remove(value);
        }

        private readonly List<OnPreSnapPointHandler> onPreUnstick = new List<OnPreSnapPointHandler>();

        public event OnPreSnapPointHandler OnPreUnstick {
            add => onPreUnstick.Add(value);
            remove => onPreUnstick.Remove(value);
        }

        private void Awake() {
            if (!snappable) {
                snappable = GetComponentInParent<Snappable>();
            }

            if (!anchorPoint) {
                anchorPoint = transform;
            }
        }

        private void Start() {
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
            if (!CheckPreSnapPointEvent(onPreSnap) || !CheckPreSnapPointEvent(otherSnapPoint.onPreSnap)) {
                return;
            }

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

            OnSnap?.Invoke();
            otherSnapPoint.OnSnap?.Invoke();
        }

        public void Unsnap() {
            if (IsSnapped && !IsSticky && !SnappedPoint.IsSticky) {
                if (!CheckPreSnapPointEvent(onPreUnsnap) || !CheckPreSnapPointEvent(SnappedPoint.onPreUnsnap)) {
                    return;
                }

                if (IsSnapParent) {
                    OnUnsnap?.Invoke();
                    SnappedPoint.OnUnsnap?.Invoke();

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
                if (!CheckPreSnapPointEvent(onPreStick) || !CheckPreSnapPointEvent(SnappedPoint.onPreStick)) {
                    return;
                }

                if (IsSnapParent) {
                    IsSticky = true;
                    SnappedPoint.IsSticky = true;

                    UpdateProperties();

                    OnStick?.Invoke();
                    SnappedPoint.OnStick?.Invoke();
                } else {
                    SnappedPoint.Stick();
                }
            }
        }

        public void Unstick() {
            if (IsSnapped && (IsSticky || SnappedPoint.IsSticky)) {
                if (!CheckPreSnapPointEvent(onPreUnstick) || !CheckPreSnapPointEvent(SnappedPoint.onPreUnstick)) {
                    return;
                }

                if (IsSnapParent) {
                    IsSticky = false;
                    SnappedPoint.IsSticky = false;

                    UpdateProperties();

                    OnUnstick?.Invoke();
                    SnappedPoint.OnUnstick?.Invoke();
                } else {
                    SnappedPoint.Unstick();
                }
            }
        }

        public void UpdateProperties() {
            if (IsSnapped && IsSnapParent) {
                if (IsSticky) {
                    SnappedJoint.massScale = stickyParentMassScale;
                    SnappedJoint.connectedMassScale = stickyChildMassScale;

                    float otherMass = SnappedPoint.Rigidbody.mass;

                    SnappedJoint.xDrive = SnappedJoint.yDrive = SnappedJoint.zDrive = new JointDrive {
                        positionSpring = stickySpring.position.spring * otherMass,
                        positionDamper = stickySpring.position.damper * otherMass,
                        maximumForce = stickySpring.position.maxForce * otherMass
                    };

                    SnappedJoint.angularXDrive = SnappedJoint.angularYZDrive = new JointDrive {
                        positionSpring = stickySpring.rotation.spring * otherMass,
                        positionDamper = stickySpring.rotation.damper * otherMass,
                        maximumForce = stickySpring.rotation.maxForce * otherMass
                    };
                } else {
                    SnappedJoint.massScale = snapParentMassScale;
                    SnappedJoint.connectedMassScale = snapChildMassScale;

                    float otherMass = SnappedPoint.Rigidbody.mass;

                    SnappedJoint.xDrive = SnappedJoint.yDrive = SnappedJoint.zDrive = new JointDrive {
                        positionSpring = spring.position.spring * otherMass,
                        positionDamper = spring.position.damper * otherMass,
                        maximumForce = spring.position.maxForce * otherMass
                    };

                    SnappedJoint.angularXDrive = SnappedJoint.angularYZDrive = new JointDrive {
                        positionSpring = spring.rotation.spring * otherMass,
                        positionDamper = spring.rotation.damper * otherMass,
                        maximumForce = spring.rotation.maxForce * otherMass
                    };
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

        private bool CheckPreSnapPointEvent(IEnumerable<OnPreSnapPointHandler> handlers) {
            PreSnapPointEvent preSnapPointEvent = new PreSnapPointEvent();

            foreach (OnPreSnapPointHandler handler in handlers) {
                handler(preSnapPointEvent);

                if (preSnapPointEvent.Canceled) {
                    return false;
                }
            }

            return true;
        }
    }
}