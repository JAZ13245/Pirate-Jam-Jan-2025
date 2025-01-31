using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BloodBlob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rigidBody3D;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private GameObject paintSplatterDecalPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        // If the paint blob has collidied with the ground, then place a decal at that location
        if ((LayerMask.GetMask("Default") & (1 << collision.gameObject.layer)) > 0 || (LayerMask.GetMask("Water") & (1 << collision.gameObject.layer)) > 0)
        {
            Vector3 collisionNormal = collision.contacts[0].normal;
            Vector3 collisionPoint = collision.contacts[0].point;

            // Place a decal at the location of the collision
            // Slightly adding the normal to the position to make sure the decal is visible
            GameObject paintSplatterDecalObject = Instantiate(paintSplatterDecalPrefab, collisionPoint + (0.01f * collisionNormal), Quaternion.identity, collision.transform);

            // Set the color of the decal to the color of the paint blob
            DecalProjector decalProjector = paintSplatterDecalObject.GetComponent<DecalProjector>();
            Material decalMaterial = new Material(decalProjector.material);
            decalProjector.material = decalMaterial;

            // Face the paint splatter decal towards the velocity of the paint blob
            paintSplatterDecalObject.transform.LookAt(collisionPoint);

            // Randomly rotate the decal to make the splatters seem more random
            // Since the decals are facing towards the normal vector and are already rotated towards it, all we need to do is rotate the decals around the z axis
            paintSplatterDecalObject.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));

            // Destroy the paint blob object
            Destroy(gameObject);
        }
        
    }
}
