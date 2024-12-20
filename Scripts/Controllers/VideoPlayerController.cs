using Mirror;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    VideoPlayer videoPlayer;
    GameObject videoScreenCanvas;

    [ClientCallback]
    private void Awake()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        videoScreenCanvas = transform.Find("VideoScreenCanvas").gameObject;
    }

    [ClientCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (videoPlayer == null)
            return;
        if (other.gameObject != Player.Instance.gameObject)
            return;

        videoScreenCanvas.gameObject.SetActive(true);
        videoPlayer.Play();
    }

    [ClientCallback]
    private void OnTriggerExit(Collider other)
    {
        if (videoPlayer == null)
            return;
        if (other.gameObject != Player.Instance.gameObject)
            return;

        videoPlayer.Stop();
        videoScreenCanvas.gameObject.SetActive(false);
    }
}
