using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Electric : MonoBehaviour {
    private LineRenderer lRend;
    public Transform transformPointA;
    public Transform transformPointB;
    private readonly int pointsCount = 5; //5
    private readonly int half = 2; //2
    private float randomness;
    public float randomMod;
    private Vector3[] points;

    private readonly int pointIndexA = 0;
    private readonly int pointIndexB = 1;
    private readonly int pointIndexC = 2;
    private readonly int pointIndexD = 3;
    private readonly int pointIndexE = 4;

    private readonly string mainTexture = "_MainTex";
    private Vector2 mainTextureScale = Vector2.one;
    private Vector2 mainTextureOffset = Vector2.one;

    private float timer;
    private float timerTimeOut = 0.05f;

    private void Start() {
        lRend = GetComponent<LineRenderer>();
        points = new Vector3[pointsCount];
        lRend.positionCount = pointsCount;
    }

    private void Update() {
        CalculatePoints();
    }

    public void CalculatePoints() {
        timer += Time.deltaTime;

        if (timer > timerTimeOut) {
            timer = 0;

            points[pointIndexA] = transformPointA.position;
            points[pointIndexE] = transformPointB.position;
            points[pointIndexC] = GetCenter(points[pointIndexA], points[pointIndexE]);
            points[pointIndexB] = GetCenter(points[pointIndexA], points[pointIndexC]);
            points[pointIndexD] = GetCenter(points[pointIndexC], points[pointIndexE]);

            float distance = Vector3.Distance(transformPointA.position, transformPointB.position) / points.Length;
            mainTextureScale.x = distance;
            mainTextureOffset.x = Random.Range(-randomness, randomness);
            lRend.material.SetTextureScale(mainTexture, mainTextureScale);
            lRend.material.SetTextureOffset(mainTexture, mainTextureOffset);

            randomness = distance / (pointsCount * half);

            SetRandomness();

            lRend.SetPositions(points);
        }
    }

    private void SetRandomness() {
        for (int i = 0; i < points.Length; i++) {
            if (i != pointIndexA && i != pointIndexE) {
                points[i].x += Random.Range(-randomness * randomMod, randomness * randomMod);
                points[i].y += Random.Range(-randomness * randomMod, randomness * randomMod);
                points[i].z += Random.Range(-randomness * randomMod, randomness * randomMod);
            }
        }
    }

    private Vector3 GetCenter(Vector3 a, Vector3 b) {
        return (a + b) / half;
    }
    //////////////////////////////////
    //[SerializeField] Transform point1;
    //[SerializeField] Transform point2;
    //[SerializeField] Transform point3;
    //[Min(1)] [SerializeField] int vertexResolution = 12;
    //[SerializeField] Vector2 noiseScale;
    //LineRenderer lineRenderer;
    //void Start() => lineRenderer = GetComponent<LineRenderer>();
    //void Update() {
    //    List<Vector3> pointPositions = new List<Vector3>();
    //    for (float i = 0; i <= 1; i += 1f / vertexResolution) {
    //        Vector2 tangentLineA = Vector2.Lerp(point1.position, point2.position, i);
    //        Vector2 tangentLineB = Vector2.Lerp(point2.position, point3.position, i);
    //        // Each point along the curve
    //        Vector2 bezierPoint = Vector2.Lerp(tangentLineA, tangentLineB, i);
    //        // Checks if the current vertex is the first or the last, which makes both ends of the lightning stay still.
    //        if (i >= 1f / vertexResolution && i <= 1 - 1f / vertexResolution) {
    //            // Creates noise and maps it from 0 - 1 to -1 and 1.
    //            float noise = Mathf.PerlinNoise(Time.time, 0) * 2 - 1;
    //            // Changes the point's position randomly based on the noise.
    //            Vector2 point = new Vector2(bezierPoint.x + noise * noiseScale.x, bezierPoint.y + noise * noiseScale.y);
    //            pointPositions.Add(point);
    //        }
    //        else {
    //            pointPositions.Add(bezierPoint);
    //        }
    //    }

    //    lineRenderer.positionCount = pointPositions.Count;
    //    lineRenderer.SetPositions(pointPositions.ToArray());
    //}
}
