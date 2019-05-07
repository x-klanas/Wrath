using System.Collections.Generic;
using UnityEngine;

namespace UtilityComponents {
    public class TornadoSuckAndSpin : MonoBehaviour {
        public float bottomTargetRadius = 5f;
        public float bottomMinRadius = 0f;
        public float bottomAffectiveRadius = 10f;

        public float topTargetRadius = 15f;
        public float topMinRadius = 10f;
        public float topAffectiveRadius = 25f;

        public float pullUpForce = 10f;
        public float pullInsideForce = 10f;
        public float pullSideForce = 10f;

        public Transform bottom;
        public Transform top;

        private void FixedUpdate() {
            Vector3 upDirection = top.position - bottom.position;
            Vector3 normalizedUpDirection = upDirection.normalized;

            foreach (Rigidbody rigid in GetAllRigidbodies()) {
                Vector3 rigidFromOrigin = rigid.position - bottom.position;
                float k = Vector3.Dot(normalizedUpDirection, rigidFromOrigin) / upDirection.magnitude;
                if (k < 0 || k > 1) continue;

                float affectiveRadius = Mathf.Lerp(bottomAffectiveRadius, topAffectiveRadius, k);
                Vector3 projectedRigid = bottom.position + upDirection * k;

                float distance = Vector3.Distance(projectedRigid, rigid.position);
                if (distance > affectiveRadius) continue;

                float targetRadius = Mathf.Lerp(bottomTargetRadius, topTargetRadius, k);
                float minRadius = Mathf.Lerp(bottomMinRadius, topMinRadius, k);
                float radiusOffset = distance < minRadius ? minRadius - distance : distance > targetRadius ? targetRadius - distance : 0f;
                    
                Vector3 axisToPointDirection = (rigid.position - projectedRigid).normalized;
                Vector3 sideDirection = (Quaternion.LookRotation(axisToPointDirection, upDirection) * Vector3.left).normalized;

                rigid.AddForce(normalizedUpDirection * pullUpForce * (1 - k));
                rigid.AddForce(sideDirection * pullSideForce);
                rigid.AddForce(axisToPointDirection * pullInsideForce * radiusOffset);
            }
        }

        private IEnumerable<Rigidbody> GetAllRigidbodies() {
            return FindObjectsOfType<Rigidbody>();
        }
    }
}