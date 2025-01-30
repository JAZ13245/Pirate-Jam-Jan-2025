using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BloodSplatter : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float sizeVariance = 0.2f; // The percentage of randomization to apply

    // Start is called before the first frame update
    private void Start()
    {
        // Get the DecalProjector component attached to this object
        DecalProjector decalProjector = GetComponent<DecalProjector>();

        // Get the original size of the DecalProjector
        Vector3 originalSize = decalProjector.size;

        // Randomize the size by a factor
        float randomFactor = 1 + Random.Range(-sizeVariance, sizeVariance);

        // Apply the same factor to both width and height (keeping it square)
        decalProjector.size = new Vector3(originalSize.x * randomFactor, originalSize.y * randomFactor, originalSize.z);
    }
}
