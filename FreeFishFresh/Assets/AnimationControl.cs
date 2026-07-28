using UnityEngine;
using UnityEngine.InputSystem;
public class AnimationControl : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private PlayerController characterController;
    private InputAction wingsOpen;
    private InputAction chargePressed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        wingsOpen = InputSystem.actions.FindAction("Wings");
        chargePressed=InputSystem.actions.FindAction("Spring");
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(characterController.isTipGrounded)
        {
            animator.SetTrigger("Bounce");
        }
        */
        if(wingsOpen.WasPressedThisFrame())
        {
            animator.SetBool("FlyingActivated",true);
        }
        if(wingsOpen.WasReleasedThisFrame())
        {
            animator.SetBool("FlyingActivated",false);
        }

        if(chargePressed.WasPressedThisFrame())
        {
            animator.SetBool("ChargeMaintained",true);
        }
        if(chargePressed.WasReleasedThisFrame())
        {
            animator.SetBool("ChargeMaintained",false);
        }
    }
}
