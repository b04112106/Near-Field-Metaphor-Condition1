using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pyramid : MonoBehaviour
{
    public float height = 1;
    public float width = 1;
    public float length = 1;
    void Start() 
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Mesh();

        var widthOffset = width * 0.5f;
        var lengthOffset = length * 0.5f;

        // var points = new Vector3[] {
        //     new Vector3(-widthOffset, 0, -lengthOffset),
        //     new Vector3(widthOffset, 0, -lengthOffset),
        //     new Vector3(widthOffset, 0, lengthOffset),
        //     new Vector3(-widthOffset, 0, lengthOffset),
        //     new Vector3(0, height, 0)
        // };

        // mesh.vertices = new Vector3[] {
        //     points[0], points[1], points[2],
        //     points[0], points[2], points[3],
        //     points[0], points[1], points[4],
        //     points[1], points[2], points[4],
        //     points[2], points[3], points[4],
        //     points[3], points[0], points[4]
        // };

        // mesh.triangles = new int[] {
        //     0, 1, 2,
        //     3, 4, 5,
        //     8, 7, 6,
        //     11, 10, 9,
        //     14, 13, 12,
        //     17, 16, 15
        // };
        Vector3 p0 = new Vector3(0,0,0);
        Vector3 p1 = new Vector3(1,0,0);
        Vector3 p2 = new Vector3(0.5f,0,Mathf.Sqrt(0.75f));
        Vector3 p3 = new Vector3(0.5f,Mathf.Sqrt(0.75f),Mathf.Sqrt(0.75f)/3);
        mesh.Clear();
        
        mesh.vertices = new Vector3[]{
            p0,p1,p2,
            p0,p2,p3,
            p2,p1,p3,
            p0,p3,p1
        };
        mesh.triangles = new int[]{
            0,1,2,
            3,4,5,
            6,7,8,
            9,10,11
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        meshFilter.mesh = mesh;
    }
}
