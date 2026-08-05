using System.Collections;
using NUnit.Framework;
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
    public float boostDuration = 2f;
    public AnimationCurve boostRewardCurve;
    public AnimationCurve boostApplicationCurve;
    public float waterAirDensity = 2.5f; // like fukin sirup it up
    public bool isFirstBoost = true;
    [HideInInspector] public float boostPercent;


    public float BoostCooldownProgress;


    private Rigidbody rb;

    private float originalLinearDampening;
    private float originalAngularDampening;
    private float originalAirDensity;
    private float boostTimer = 0f;
    //private bool corutineIsRunning = false;


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

        if (isFirstBoost)
        {
            FirstBoost();
        }
        else
        {
            WaterFlappingprogrssive();
        }
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
        rb.AddForce(Vector3.up * floatingForce);
    }

    private void FirstBoost()
    {

        if (playerController.springInput > 0.2 && isWet)
        {

            ApplyBoost(1);
            //StartCoroutine(BoostCorutine(1));
            boostTimer = boostCooldown;
            isFirstBoost = false;
        }
    }

    private void WaterFlappingprogrssive()
    {
        boostTimer = Mathf.Max(0f, boostTimer - Time.deltaTime);

        BoostCooldownProgress = 1f - (boostTimer / boostCooldown);
        if (BoostCooldownProgress >= 1)
        {
            isFirstBoost = true;
        }
        if (playerController.springInput > 0.2 && isWet)
        {

            boostPercent = boostRewardCurve.Evaluate(BoostCooldownProgress);
            ApplyBoost(boostPercent);
            boostTimer = boostCooldown;
        }
    }
    IEnumerator BoostCorutine(float boostPercent)
    {
        for (float t = 0; t <= 1; t += Time.fixedDeltaTime / boostDuration)
        {
            float scaleT = boostApplicationCurve.Evaluate(t);
            rb.AddForce(rb.transform.up * boost * boostPercent * scaleT);

            yield return new WaitForFixedUpdate();
        }
    }
    private void ApplyBoost(float boostPercent)
    {


        rb.AddForce(rb.transform.up * boost * boostPercent, ForceMode.Impulse);

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
