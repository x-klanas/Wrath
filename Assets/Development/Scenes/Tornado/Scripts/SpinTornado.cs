using UnityEngine;

public class SpinTornado : MonoBehaviour {

    public float spinSpeed = 120f;

    private void Update() {
        transform.RotateAround(transform.position, transform.up, spinSpeed * Time.deltaTime);
    }
}