using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class LineWaveForm : Graphic
{
    // Declare the script and property name we want to retrieve the data from
    public OrientationUtility orientationUtility;
    public float maxRange = 90f;
    // record the maximum and minimum value of the data
    public float max = 0;
    public float min = 0; 

    List<float> pointsX;
    public List<float> pointsY;
    public float thickness = 0.5f;
    float width, height;
    protected override void Start()
    {
        if (orientationUtility == null)
        {
            orientationUtility = FindObjectOfType<OrientationUtility>();
        }
        if (orientationUtility == null)
        {
            Debug.LogError("Please assign a script to the LineWaveForm script");
            Application.Quit();
        }
        pointsX = new List<float>();
        pointsY = new List<float>();
        for (int i = 0; i < 100; i++)
        {
            pointsX.Add(i / 100f);
            pointsY.Add(0);
        }
    }
    public void PointSignal(float y)
    {
        pointsY.RemoveAt(0);
        pointsY.Add(y);
    }
    void Update()
    {
        if(max < orientationUtility.PitchAngle)
        {
            max = orientationUtility.PitchAngle;
        }
        else if(min > orientationUtility.PitchAngle)
        {
            min = orientationUtility.PitchAngle;
        }
        PointSignal(orientationUtility.PitchAngle/maxRange);
        SetAllDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (pointsX.Count < 2)
        {
            return;
        }
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;
        vh.Clear();
        float angle = 0;
        for (int i = 0; i < pointsX.Count; i++)
        {
            Vector2 point = new Vector2(pointsX[i], pointsY[i]);
            if (i < pointsX.Count - 1)
            {
                angle = GetDegreeBetweenPoints(point, new Vector2(pointsX[i + 1], pointsY[i + 1]));
            }
            DrawVerticesForPoint(point, vh, angle);
        }
        for (int i = 0; i < pointsX.Count - 1; i++)
        {
            int index = i * 2;
            vh.AddTriangle(index, index + 1, index + 3);
            vh.AddTriangle(index + 3, index + 2, index);
        }
    }
    float GetDegreeBetweenPoints(Vector2 now, Vector2 next)
    {
        return (float)(Mathf.Atan2(next.y - now.y, next.x - now.x) / Mathf.PI * 180 * 45);
    }
    void DrawVerticesForPoint(Vector2 point, VertexHelper vh, float angle)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(width * point.x, height * point.y);
        vh.AddVert(vertex);
        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(0, thickness / 2);
        vertex.position += new Vector3(width * point.x, height * point.y);
        vh.AddVert(vertex);
    }
}
