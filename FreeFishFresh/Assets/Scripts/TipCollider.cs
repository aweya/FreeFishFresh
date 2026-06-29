using UnityEngine;

public class TipCollision : MonoBehaviour
{
    private PlayerController playerController;

    public float impactForce;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    void Update(){

        

        

    }

    void OnTriggerEnter(Collider other)
    {
        
            // Calculate the impact force based on the Rigidbody's velocity
             impactForce = playerController.GetComponent<Rigidbody>().linearVelocity.magnitude;
             

            
            //playerController.ApplyBounce(impactForce);
            playerController.isTipGrounded=true;
        
    }
}
