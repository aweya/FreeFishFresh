using UnityEngine;
using UnityEngine.InputSystem;

public class WaterPhysics : MonoBehaviour
{
    public PlayerController playerController;
    [Header("Waterstuff")]
    public bool isWet = false;
    public float linearWaterDampening;
    public float angularWaterDampening;
    public float floatingForce = 2f;
    public float boost;
    public float boostCooldown = 4f;
    public AnimationCurve boostRewardCurve;
    public float waterAirDensity = 2.5f; // like fukin sirup it up


    public float BoostCooldownProgress;


    private Rigidbody rb;

    private float originalLinearDampening;
    private float originalAngularDampening;
    private float originalAirDensity;
    private float boostTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (rb == null || playerController == null)
        {
            Debug.LogError("WaterPhysics needs a Rigidbody and PlayerController on the Player.", this);
            enabled = false;
            return;
        }

        originalAngularDampening = rb.angularDamping;
        originalLinearDampening = rb.linearDamping;
        originalAirDensity = playerController.airDensity;
        boostTimer = boostCooldown; // The first water flap is immediately available.
    }

    void Update()
    {
        if (isWet)
        {
            rb.angularDamping = angularWaterDampening;
            rb.linearDamping = linearWaterDampening;
            playerController.airDensity = waterAirDensity;
        }
        else
        {
            rb.angularDamping = originalAngularDampening;
            rb.linearDamping = originalLinearDampening;
            playerController.airDensity = originalAirDensity;
        }


        WaterFlappingprogrssive();

    }
    void FixedUpdate()
    {
        if (isWet)
        {
            Floatation();
        }
    }

    private void Floatation()
    {
        rb.AddForce(transform.up * floatingForce);
    }

    private void WaterFlapping()
    {

        if (!WasFlapPressed())
            return;

        rb.AddForce(rb.transform.up * boost, ForceMode.Impulse);
        boostTimer = 0f;
    }

    private void WaterFlappingprogrssive()
    {
        boostTimer = Mathf.Max(0f, boostTimer - Time.deltaTime);

        BoostCooldownProgress = 1f - (boostTimer / boostCooldown);

        if (playerController.springInput > 0.2 && isWet)
        {
            float boostPercent;
            boostPercent = boostRewardCurve.Evaluate(BoostCooldownProgress);
            rb.AddForce(rb.transform.up * boost * boostPercent, ForceMode.Impulse);
            boostTimer = boostCooldown;
        }
    }


    private bool WasFlapPressed()
    {
        if (playerController.playerInput == null)
            return false;

        InputAction flapAction = playerController.playerInput.actions.FindAction("Spring", false);
        return flapAction != null && flapAction.WasPressedThisFrame();
    }

    private void OnDisable()
    {
        if (rb == null || playerController == null)
            return;

        rb.angularDamping = originalAngularDampening;
        rb.linearDamping = originalLinearDampening;
        playerController.airDensity = originalAirDensity;
    }

}
