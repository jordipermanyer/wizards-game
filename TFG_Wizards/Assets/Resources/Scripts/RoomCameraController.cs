using UnityEngine;
using System.Collections.Generic;

public class RoomCameraController : MonoBehaviour
{
    private Camera currentActiveCamera;
    private List<Camera> allCameras = new List<Camera>();

    void Start()
    {
        allCameras.Clear();
        foreach (GameObject room in GameObject.FindGameObjectsWithTag("Room"))
        {
            Camera roomCamera = room.GetComponentInChildren<Camera>(true);
            if (roomCamera != null)
            {
                roomCamera.gameObject.SetActive(false);
                allCameras.Add(roomCamera);
            }
        }

        if (allCameras.Count > 0)
        {
            currentActiveCamera = allCameras[0];
            currentActiveCamera.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No se encontraron cámaras en las habitaciones.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Room"))
        {
            Camera roomCamera = other.GetComponentInChildren<Camera>(true);

            if (roomCamera != null && roomCamera != currentActiveCamera)
            {
                SwitchToCamera(roomCamera);
            }
        }
    }

    private void SwitchToCamera(Camera newCamera)
    {
        if (currentActiveCamera != null)
        {
            currentActiveCamera.gameObject.SetActive(false);
        }

        currentActiveCamera = newCamera;
        currentActiveCamera.gameObject.SetActive(true);
    }
}
