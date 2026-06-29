using UnityEngine;
using Unity.Cinemachine;

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
        glideOrbit = glideCam.GetComponent<CinemachineOrbitalFollow>();
        pogoOrbit = pogoCam.GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        int wingOpenAmount = Mathf.RoundToInt(player.wingInput) * 10;
        glideCam.Priority = wingOpenAmount;

        SyncCams();

        if (wingOpenAmount > 5)
        {
            SyncCams();
        }
    }

    void SyncCams()
    {
        //sync pogo to glide cam
        pogoCam.transform.position = glideCam.transform.position;
        pogoCam.transform.rotation = glideCam.transform.rotation;
    }

}
