using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayDetector : MonoBehaviour
{
    private LineRenderer Ray;
    private float lineLength = 20f;
    private float lineWidth = 0.5f;
    private Color lineColor = new Color(1f, 1f, 1f, 0f);
    public Material lineMaterial;

    public GameObject Manager;

    void Start()
    {
        Ray = gameObject.AddComponent<LineRenderer>();
        Ray.positionCount = 2;
        Ray.startWidth = lineWidth;
        Ray.endWidth = lineWidth;
        Ray.material = lineMaterial;
        Ray.startColor = lineColor;
        Ray.endColor = lineColor;
    }

    void Update() {

        // Draw Ray
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraDirection = Camera.main.transform.forward;

        Ray.SetPosition(0, cameraPosition);
        Ray.SetPosition(1, cameraPosition + cameraDirection * lineLength);

        RaycastHit hit;
        if (Physics.Raycast(cameraPosition, cameraDirection, out hit, lineLength)) {
            if (hit.collider.TryGetComponent<Renderer>(out var hitRenderer)) {
                if (hitRenderer.name != null) {
                    Manager.SendMessage("Hit" + hitRenderer.name);
                }
            }
        }
    }
}
