using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    void Start()
    {
        CameraShotDirector.Instance.LockPlayerInput(true);
        CameraShotDirector.Instance.RequestShot(director.GetComponent<CinemachineCamera>());
        director.stopped += OnIntroFinished;
        director.Play();
    }

    void OnIntroFinished(PlayableDirector d)
    {
        director.stopped -= OnIntroFinished;
        CameraShotDirector.Instance.ReleaseShot(director.GetComponent<CinemachineCamera>());
        CameraShotDirector.Instance.LockPlayerInput(false);
    }
}