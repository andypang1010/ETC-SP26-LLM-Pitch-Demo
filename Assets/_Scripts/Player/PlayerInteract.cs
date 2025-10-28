using LLMUnity;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public bool hasNPCInRange = false;
    void Update()
    {
        if (InputController.Instance.GetInteractDown())
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 3f))
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
        }
    }
}
