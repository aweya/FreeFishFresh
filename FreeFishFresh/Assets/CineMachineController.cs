using UnityEngine;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;

public class CineMachineController : MonoBehaviour
{

    public CinemachineCamera glideCam;
    public CinemachineCamera pogoCam;
    private CinemachineOrbitalFollow glideOrbit;
    private CinemachineOrbitalFollow pogoOrbit;
    private PlayerController player;


    void Start()
    {
        player = GetComponent<PlayerController>();
        glideOrbit = glideCam.GetComponent<CinemachineOrbitalFollow>(); //lock to trget camera 
        pogoOrbit = pogoCam.GetComponent<CinemachineOrbitalFollow>(); // worldspace camera
    }

    void Update()
    {
        //--switch caemera

        int wingOpenAmount = Mathf.RoundToInt(player.wingInput) * 10;
        glideCam.Priority = wingOpenAmount;

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
