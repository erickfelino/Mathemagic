using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;

public class CameraShotDirector : MonoBehaviour
{
    public static CameraShotDirector Instance { get; private set; }

    [SerializeField] private CinemachineCamera followCam;   // câmera "casa", costas do jogador
     [SerializeField] private GameObject player;
    private StarterAssetsInputs playerInputs;
    private PlayerInput playerInputComponent;

    private const int BasePriority = 10;
    private const int ShotPriority = 20;

    void Awake()
    {
        Instance = this;
        playerInputs = player.GetComponentInChildren<StarterAssetsInputs>();
        playerInputComponent = player.GetComponent<PlayerInput>();
    }

    public void RequestShot(CinemachineCamera cam)
    {
        cam.Priority = ShotPriority;
    }

    public void ReleaseShot(CinemachineCamera cam)
    {
        cam.Priority = 0; // abaixo da followCam, some da disputa
    }

    public void LockPlayerInput(bool locked)
    {
        if (playerInputs != null) playerInputs.SetInputEnabled(!locked);
        if (playerInputComponent != null) playerInputComponent.enabled = !locked;
    }
}