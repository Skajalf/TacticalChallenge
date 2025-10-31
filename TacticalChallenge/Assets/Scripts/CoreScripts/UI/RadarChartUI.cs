using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RadarChartUI : Graphic
{
    [Header("Stat Data")]
    public StatGraphics stats = new StatGraphics { statValues = new int[] { 80, 70, 60, 50, 40, 30 } };

    [Header("Visual Settings")]
    public float chartRadius = 50f;
    public Color fillColor = new Color(1f, 0.5f, 0.5f, 0.5f);

    [Header("Grid Settings")]
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public float gridLineThickness = 1.5f;

    [Header("Outline Settings")]
    public Color outlineColor = Color.white;
    public float outlineThickness = 2.0f;

    [Header("Labels")]
    public TextMeshProUGUI[] statLabels = new TextMeshProUGUI[6];
    public float labelOffset = 10f;

    protected override void Awake()
    {
        base.Awake();
        stats.OnStatsChanged += SetVerticesDirty;
        stats.OnStatsChanged += UpdateStatLabels;
    }

    protected override void OnDestroy()
    {
        stats.OnStatsChanged -= SetVerticesDirty;
        stats.OnStatsChanged -= UpdateStatLabels;
        base.OnDestroy();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
        UpdateStatLabels();
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        int sides = stats.statValues.Length;
        if (sides == 0 || stats.maxValue <= 0) return;

        Vector2 center = Vector2.zero;

        int gridStartIndex = vh.currentVertCount;
        List<Vector3> gridVertices = new List<Vector3>();

        for (int i = 0; i < sides; i++)
        {
            float angleDeg = (i * (360f / sides)) + 90f;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float x = chartRadius * Mathf.Cos(angleRad);
            float y = chartRadius * Mathf.Sin(angleRad);

            gridVertices.Add(new Vector3(x, y));
        }

        for (int i = 0; i < gridVertices.Count; i++)
        {
            UIVertex vert = UIVertex.simpleVert;
            vert.position = gridVertices[i];
            vert.color = gridColor;
            vh.AddVert(vert);
        }

        for (int i = 0; i < sides; i++)
        {
            DrawUILine(vh, gridVertices[i], gridVertices[(i + 1) % sides], gridLineThickness, gridColor);
            DrawUILine(vh, center, gridVertices[i], gridLineThickness, gridColor);
        }

        color = fillColor;

        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(center);

        for (int i = 0; i < sides; i++)
        {
            float angleDeg = (i * (360f / sides)) + 90f;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float normalizedValue = (float)stats.statValues[i] / stats.maxValue;

            float currentRadius = chartRadius * normalizedValue;

            float x = currentRadius * Mathf.Cos(angleRad);
            float y = currentRadius * Mathf.Sin(angleRad);

            vertices.Add(new Vector3(x, y));
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vert = UIVertex.simpleVert;
            vert.position = vertices[i];
            vert.color = color;
            vh.AddVert(vert);
        }

        for (int i = 0; i < sides; i++)
        {
            int centerIndex = vh.currentVertCount - vertices.Count;
            int currentStatIndex = centerIndex + i + 1;
            int nextStatIndex = (i + 1) % sides + 1;

            if (nextStatIndex == 1) nextStatIndex = centerIndex + 1;
            else nextStatIndex = centerIndex + nextStatIndex;

            vh.AddTriangle(centerIndex, currentStatIndex, nextStatIndex);
        }

        List<Vector3> statPoints = new List<Vector3>();
        for (int i = 1; i < vertices.Count; i++)
        {
            statPoints.Add(vertices[i]);
        }

        for (int i = 0; i < sides; i++)
        {
            Vector3 currentPoint = statPoints[i];
            Vector3 nextPoint = statPoints[(i + 1) % sides];

            DrawUILine(vh, currentPoint, nextPoint, outlineThickness, outlineColor);
        }

        UpdateStatLabels();
    }

    private void UpdateStatLabels()
    {
        if (statLabels.Length != 6) return;

        int sides = stats.statValues.Length;

        for (int i = 0; i < sides; i++)
        {
            if (statLabels[i] == null) continue;

            float angleDeg = (i * (360f / sides)) + 90f;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float finalRadius = chartRadius + labelOffset;

            float x = finalRadius * Mathf.Cos(angleRad);
            float y = finalRadius * Mathf.Sin(angleRad);

            statLabels[i].text = ((StatGraphics.StatType)i).ToString();

            statLabels[i].rectTransform.localPosition = new Vector3(x, y, 0);

            if (x > 0.01f)
                statLabels[i].alignment = TextAlignmentOptions.MidlineLeft;
            else if (x < -0.01f)
                statLabels[i].alignment = TextAlignmentOptions.MidlineRight;
            else
                statLabels[i].alignment = TextAlignmentOptions.Midline;
        }
    }

    private static void DrawUILine(VertexHelper vh, Vector3 p1, Vector3 p2, float thickness, Color color)
    {
        Vector3 dir = (p2 - p1).normalized;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x) * thickness * 0.5f;

        int vIndex = vh.currentVertCount;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        vert.position = p1 - perpendicular;
        vh.AddVert(vert);

        vert.position = p1 + perpendicular;
        vh.AddVert(vert);

        vert.position = p2 + perpendicular;
        vh.AddVert(vert);

        vert.position = p2 - perpendicular;
        vh.AddVert(vert);

        vh.AddTriangle(vIndex, vIndex + 1, vIndex + 2);
        vh.AddTriangle(vIndex + 2, vIndex + 3, vIndex);
    }
}