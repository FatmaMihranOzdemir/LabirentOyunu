
using Unity.Cinemachine;
using UnityEngine;

public class IntroCameraSequence : MonoBehaviour
{
    public CinemachineVirtualCamera introCam;
    public CinemachineVirtualCamera followCam;
    public float introDuration = 4f;

    void Start()
    {
        introCam.Priority = 20;
        followCam.Priority = 10;
        Invoke(nameof(SwitchToFollowCam), introDuration);
    }

    void SwitchToFollowCam()
    {
        followCam.Priority = 20;
        introCam.Priority = 10;
    }
}
