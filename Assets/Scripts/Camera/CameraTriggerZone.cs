using UnityEngine;
using Unity.Cinemachine;

public class CameraTriggerZone : MonoBehaviour
{
    [SerializeField] private CinemachineCamera shotCamera;
    [SerializeField] private bool lockPlayerWhileInside = false;
    [SerializeField] private bool oneShot = false; // se true, não reverte no OnTriggerExit

    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || (oneShot && triggered)) return;
        triggered = true;
        CameraShotDirector.Instance.RequestShot(shotCamera);
        if (lockPlayerWhileInside) CameraShotDirector.Instance.LockPlayerInput(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || oneShot) return;
        CameraShotDirector.Instance.ReleaseShot(shotCamera);
        if (lockPlayerWhileInside) CameraShotDirector.Instance.LockPlayerInput(false);
    }
}