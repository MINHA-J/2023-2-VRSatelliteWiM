using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Includes Vector3.XyzToUvw, Vector3.XyzToSide, Vector3.XyzToUvwForceSide, Vector3.UvwToXyz
using CubemapTransform;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CubemapTest : MonoBehaviour {

    public bool bakeIntoMesh;

    public int dimensions = 1024;
    public Shader renderTextureCubemapShader;
    public ComputeShader renderTextureWriter;

    public RenderTexture cubemapRenderTexture;

    // Use this for initialization
    void Start()
    {
        // Create Render Texture
        cubemapRenderTexture = new RenderTexture(dimensions, dimensions, 0, RenderTextureFormat.ARGB32);
        {
            cubemapRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray;
            cubemapRenderTexture.volumeDepth = 6;
            cubemapRenderTexture.wrapMode = TextureWrapMode.Clamp;
            cubemapRenderTexture.filterMode = FilterMode.Trilinear;
            cubemapRenderTexture.enableRandomWrite = true;
            cubemapRenderTexture.isPowerOfTwo = true;
            cubemapRenderTexture.Create();
        }

        // Create material using rendertexture as cubemap
        MeshRenderer target = GetComponent<MeshRenderer>();
        {
            target.material = new Material(renderTextureCubemapShader);
            target.material.mainTexture = cubemapRenderTexture;
        }

        // If we're baking into the mesh, we'll make our own cube
        if (bakeIntoMesh)
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            filter.mesh = MakeCubemapMesh();
        }
    }

    void MakeCubemapSide(
        Vector3 sideRight, Vector3 sideUp,
        List<Vector3> outPositions, List<Vector3> outBakedCoords, List<int> outTriangleIndices)
    {
        // Reserve tris
        {
            int currentStartIndex = outPositions.Count;
            outTriangleIndices.Add(currentStartIndex + 0);
            outTriangleIndices.Add(currentStartIndex + 1);
            outTriangleIndices.Add(currentStartIndex + 2);

            outTriangleIndices.Add(currentStartIndex + 3);
            outTriangleIndices.Add(currentStartIndex + 2);
            outTriangleIndices.Add(currentStartIndex + 1);
        }

        // Make verts
        {
            Vector3 sideForward = Vector3.Cross(sideUp, sideRight);
            Vector3[] vertices = new Vector3[4];
            int idx = 0;
            vertices[idx++] = sideForward + ( sideUp) + (-sideRight); // Top left
            vertices[idx++] = sideForward + ( sideUp) + ( sideRight); // Top right
            vertices[idx++] = sideForward + (-sideUp) + (-sideRight); // Bottom left
            vertices[idx++] = sideForward + (-sideUp) + ( sideRight); // Bottom right

            int sideIndex = sideForward.XyzToSide();
            foreach (Vector3 vertex in vertices)
            {
                outPositions.Add(vertex / 2); // Divide in half to match the dimensions of unity's cube
                outBakedCoords.Add(vertex.XyzToUvwForceSide(sideIndex));
            }
        }
    }

    Mesh MakeCubemapMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> positions     = new List<Vector3>();
        List<Vector3> bakedCoords   = new List<Vector3>();
        List<int> triangleIndices   = new List<int>();

        MakeCubemapSide(Vector3.right,   Vector3.up,      positions, bakedCoords, triangleIndices); // +X
        MakeCubemapSide(Vector3.left,    Vector3.up,      positions, bakedCoords, triangleIndices); // -X
        MakeCubemapSide(Vector3.up,      Vector3.forward, positions, bakedCoords, triangleIndices); // +Y
        MakeCubemapSide(Vector3.down,    Vector3.forward, positions, bakedCoords, triangleIndices); // -Y
        MakeCubemapSide(Vector3.forward, Vector3.right,   positions, bakedCoords, triangleIndices); // +Z
        MakeCubemapSide(Vector3.back,    Vector3.right,   positions, bakedCoords, triangleIndices); // -Z

        mesh.vertices   = positions.ToArray();
        mesh.normals    = bakedCoords.ToArray();
        mesh.triangles  = triangleIndices.ToArray();

        return mesh;
    }

    private int counter = 0;
    void Update()
    {
        // Draw to Render Texture with Compute shader every few seconds
        // So feel free to recompile the compute shader while the editor is running
        if (counter == 0)
        {
            string kernelName = "CSMain";
            int kernelIndex = renderTextureWriter.FindKernel(kernelName);

            renderTextureWriter.SetTexture(kernelIndex, "o_cubeMap", cubemapRenderTexture);
            renderTextureWriter.SetInt("i_dimensions", dimensions);
            renderTextureWriter.Dispatch(kernelIndex, dimensions, dimensions, 1);
        }
        counter = (counter + 1) % 300;
    }

}
