using UnityEngine;
using Unity.Cinemachine;

public class DynamicShotTarget : MonoBehaviour
{
    [SerializeField] private CinemachineCamera shotCamera;

    public void Track(Transform target)
    {
        shotCamera.Follow = target;
        shotCamera.LookAt = target;
    }
}