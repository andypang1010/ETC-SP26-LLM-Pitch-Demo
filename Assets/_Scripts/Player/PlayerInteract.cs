using LLMUnity;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    public bool hasNPCInRange = false;
    void Update()
    {

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange))
        {
            if (hit.transform.root.TryGetComponent(out LLMCharacter _))
            {
                hasNPCInRange = true;
            }

            else
            {
                hasNPCInRange = false;
            }
        }
        
        else
        {
            hasNPCInRange = false;
        }
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = hasNPCInRange ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * interactRange);
    }
}
