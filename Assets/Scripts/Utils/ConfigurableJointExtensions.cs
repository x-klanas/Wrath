using UnityEngine;

namespace Utils {
    /// <summary>
    /// SetLocalTargetRotation, SetWorldTargetRotation and SetTargetRotation
    /// adapted from: https://gist.github.com/mstevenson/4958837
    /// </summary>
    public static class ConfigurableJointExtensions {
        /// <summary>
        /// Sets a joint's targetRotation to match a given local rotation.
        /// The joint transform's local rotation must be cached on joint initialization and passed into this method.
        /// </summary>
        public static void SetLocalTargetRotation(this ConfigurableJoint joint, Quaternion targetLocalRotation, Quaternion initialLocalRotation) {
            if (joint.configuredInWorldSpace) {
                Debug.LogError("SetLocalTargetRotation should not be used with joints that are configured in world space. For world space joints, use SetWorldTargetRotation.", joint);
            }

            SetTargetRotation(joint, targetLocalRotation, initialLocalRotation, Space.Self);
        }

        /// <summary>
        /// Sets a joint's targetRotation to match a given world rotation.
        /// The joint transform's world rotation must be cached on joint initialization and passed into this method.
        /// </summary>
        public static void SetWorldTargetRotation(this ConfigurableJoint joint, Quaternion targetWorldRotation, Quaternion initialWorldRotation) {
            if (!joint.configuredInWorldSpace) {
                Debug.LogError("SetWorldTargetRotation must be used with joints that are configured in world space. For local space joints, use SetLocalTargetRotation.", joint);
            }

            SetTargetRotation(joint, targetWorldRotation, initialWorldRotation, Space.World);
        }

        /// <summary>
        /// Sets a joint's targetRotation to match a given rotation.
        /// The joint transform's world rotation must be cached on joint initialization and passed into this method.
        /// </summary>
        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion targetRotation, Quaternion initialRotation, Space space) {
            // Calculate the rotation expressed by the joint's axis and secondary axis
            Vector3 right = joint.axis;
            Vector3 forward = Vector3.Cross(right, joint.secondaryAxis).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            Quaternion worldToJointSpace = Quaternion.LookRotation(forward, up);

            // Transform into world space
            Quaternion resultRotation = Quaternion.Inverse(worldToJointSpace);

            // Counter-rotate and apply the new local rotation.
            // Joint space is the inverse of world space, so we need to invert our value
            if (space == Space.World) {
                resultRotation *= initialRotation * Quaternion.Inverse(targetRotation);
            } else {
                resultRotation *= Quaternion.Inverse(targetRotation) * initialRotation;
            }

            // Transform back into joint space
            resultRotation *= worldToJointSpace;

            // Set target rotation to our newly calculated rotation
            joint.targetRotation = resultRotation;
        }
    }
}