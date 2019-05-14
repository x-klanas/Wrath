using Snapping;
using UnityEngine;

namespace Parts {
    [RequireComponent(typeof(Snappable))]
    public class Screw : MonoBehaviour {
        public float positionSpring = 100f;
        public float positionDamper = 10f;
        public float positionMaxForce = 100f;

        public float rotationSpring = 50f;
        public float rotationDamper = 5f;
        public float rotationMaxForce = 50f;
        
        [SerializeField]
        [Range(0, 1)]
        private float screwValue = 0f;

        public float ScrewValue {
            get => screwValue;
            set {
                screwValue = Mathf.Clamp01(value);
                UpdateProperties();
                OnScrewValue?.Invoke(screwValue);
            }
        }

        public Transform unscrewedAnchorPoint;
        public Transform screwedAnchorPoint;

        public Collider[] collidersToDisable;

        public SnapPoint snapPoint;
        
        public delegate void OnScrewValueHandler(float screwValue);

        public event OnScrewValueHandler OnScrewValue;

        private Transform anchor;

        private void Start() {
            if (snapPoint == null) {
                snapPoint = GetComponentInChildren<SnapPoint>();
            }

            anchor = new GameObject("screw anchor").transform;
            anchor.parent = transform;

            snapPoint.anchorPoint = anchor;

            UpdateProperties();

            snapPoint.OnUnsnap += OnUnsnap;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                ScrewValue += 0.1f;
                UpdateProperties();
            } else if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                ScrewValue -= 0.1f;
                UpdateProperties();
            }
        }

        private void OnUnsnap(SnapPoint snappedPoint) {
            screwValue = 0f;
            UpdateProperties();
        }

        public void UpdateProperties() {
            bool enableColliders = screwValue <= 0;

            foreach (Collider col in collidersToDisable) {
                col.enabled = enableColliders;
            }

            anchor.position = Vector3.Lerp(unscrewedAnchorPoint.position, screwedAnchorPoint.position, screwValue);
            anchor.rotation = Quaternion.Lerp(unscrewedAnchorPoint.rotation, screwedAnchorPoint.rotation, screwValue);

            snapPoint.UpdateAnchorPosition();
        }
    }
}