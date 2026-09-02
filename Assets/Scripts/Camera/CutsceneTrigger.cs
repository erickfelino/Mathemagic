using UnityEngine;
using UnityEngine.Playables;

public class CutsceneTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    public void PlayCutscene()
    {
        CameraShotDirector.Instance.LockPlayerInput(true);
        director.stopped += OnFinished;
        director.Play();
    }

    void OnFinished(PlayableDirector d)
    {
        director.stopped -= OnFinished;
        CameraShotDirector.Instance.LockPlayerInput(false);
    }
}