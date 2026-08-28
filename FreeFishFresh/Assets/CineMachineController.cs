using UnityEngine;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using System.Buffers.Text;

public class CineMachineController : MonoBehaviour
{
    [Header("Cams")]

    public CinemachineCamera glideCam;
    public CinemachineCamera pogoCam;
    [Header("vars")]
    public float BaseFov = 90f;
    public float FovMultilier;
    public float maxFov = 100f;
    public float wingTreshHold = 0.5f;
    private CinemachineOrbitalFollow glideOrbit;
    private CinemachineOrbitalFollow pogoOrbit;
    private PlayerController player;


    void Start()
    {
        player = GetComponent<PlayerController>();
        glideOrbit = glideCam.GetComponent<CinemachineOrbitalFollow>(); //lock to trget camera 
        pogoOrbit = pogoCam.GetComponent<CinemachineOrbitalFollow>(); // worldspace camera
        glideCam.Lens.FieldOfView = BaseFov;
        pogoCam.Lens.FieldOfView = BaseFov;
    }

    void Update()
    {
        //--switch caemera
        if (player.wingInput > wingTreshHold)
        {
            glideCam.Priority = 11;
        }
        else
        {
            glideCam.Priority = 5;
        }

        /*   int wingOpenAmount = Mathf.RoundToInt(player.wingInput) * 10;
          glideCam.Priority = wingOpenAmount; */

        // channge Fov with speed
        float clampSpeed = Mathf.Clamp(player.speed, 1f, 10000f);
        float effectiveFov = Mathf.Clamp(BaseFov * clampSpeed * FovMultilier, BaseFov, maxFov);
        glideCam.Lens.FieldOfView = effectiveFov;
        //pogoCam.Lens.FieldOfView = effectiveFov;

        // if (player.wingInput > 0.5f)
        // {
        //     glideOrbit.TrackerSettings.BindingMode = BindingMode.LockToTarget;
        // }
        // else
        // {
        //     glideOrbit.TrackerSettings.BindingMode = BindingMode.WorldSpace;
        // }

        // pogoOrbit.TrackerSettings.BindingMode = BindingMode.LockToTarget;

        // // SyncCams();
        // if (player.wingInput < 5)
        // {
        //     pogoOrbit.TrackerSettings.BindingMode = BindingMode.WorldSpace;
        // }
        // else
        // {
        //     pogoOrbit.TrackerSettings.BindingMode = BindingMode.LockToTarget;
        // }
    }





    void SyncCams()
    {
        pogoOrbit.HorizontalAxis.Value = glideOrbit.HorizontalAxis.Value;
        pogoOrbit.VerticalAxis.Value = glideOrbit.VerticalAxis.Value;
        pogoOrbit.Radius = glideOrbit.Radius;
    }

}
