using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float cameraExplosionForce;
    public bool isDead;
    GameObject cam;

    void Start()
    {
        cam = Camera.main.transform.parent.gameObject;
    }

    public IEnumerator Die()
    {
        isDead = true;

        // Remove HeadBob and MoveCamera
        cam.GetComponent<HeadBob>().enabled = false;
        cam.GetComponent<MoveCamera>().enabled = false;

        // Add rigidbody and collider to camera
        Rigidbody camRB = Camera.main.gameObject.AddComponent<Rigidbody>();
        camRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        camRB.AddForce(cameraExplosionForce * Camera.main.transform.up, ForceMode.Impulse);

        Camera.main.gameObject.AddComponent<SphereCollider>();

        DestroyPlayer();

        yield return new WaitForSeconds(3);
    }

    private void DestroyPlayer()
    {
        foreach (Component component in GetComponents(typeof(Component)))
        {
            if (component == this || component.GetType() == typeof(Transform))
            {
                continue;
            }
            else
            {
                Destroy(component);
            }
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        Destroy(GameObject.Find("UI Camera"));
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Death Zone"))
        {
            StartCoroutine(Die());
        }
    }
}
