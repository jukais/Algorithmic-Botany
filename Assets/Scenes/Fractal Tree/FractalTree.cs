using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalTree : MonoBehaviour {

  public Material material;

  [Range(0.01f, 1.0f)]
  public float taperMultiplier = .75f;
  [Range(0.01f, 180.0f)]
  public float rotation = 45.0f;
  [Range(1.0f, 100.0f)]
  public float height = 5.0f;
  [Range(0.1f, 0.95f)]
  public float heightMultiplier = 0.55f;
  [Range(1, 14)]
  public int iterations = 5;

  readonly Vector3[] vertices = {
        new Vector3 (-1, 0, -1),
        new Vector3 (+1, 0, -1),
        new Vector3 (+1, 2, -1),
        new Vector3 (-1, 2, -1),

        new Vector3 (-1, 2, 1),
        new Vector3 (+1, 2, 1),
        new Vector3 (+1, 0, 1),
        new Vector3 (-1, 0, 1),
    };

  readonly int[] triangles = {
        0, 2, 1, //face front
        0, 3, 2,
        2, 3, 4, //face top
        2, 4, 5,
        1, 2, 5, //face right
        1, 5, 6,
        0, 7, 4, //face left
        0, 4, 3,
        5, 4, 7, //face back
        5, 7, 6,
        0, 6, 7, //face bottom
        0, 1, 6
    };

  readonly GameObject[] branches = new GameObject[Mathf.RoundToInt(Mathf.Pow(2, 14))];

  void CreateCube(
      Transform parent,
      Vector3 position,
      Vector3 rotation,
      float height,
      Vector2 taper,
      int index) {
    Vector3[] scaledVertices = new Vector3[vertices.Length];
    for (int i = 0; i < vertices.Length; i++) {
      scaledVertices[i] = Vector3.Scale(vertices[i], new(1f, height, 1f)) / 2.0f;

      var scale = i < 2 || i > 5 ? taper.x : taper.y;
      scaledVertices[i] = Vector3.Scale(scaledVertices[i], new(scale, 1, scale));
    }
    Mesh mesh = branches[index].GetComponent<MeshFilter>().mesh;
    mesh.vertices = scaledVertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    mesh.Optimize();

    branches[index].transform.SetParent(parent);
    branches[index].transform.localPosition = position;
    branches[index].transform.localRotation = Quaternion.Euler(rotation);
    branches[index].SetActive(true);
  }

  int CreateFractalTree(
      Transform parent,
      Vector3 position,
      Vector3 rotation,
      float height,
      Vector2 taper,
      int depth,
      int index) {

    if (depth == 0 || index >= branches.Length) {
      return index;
    }

    CreateCube(
        parent, position, rotation, height, taper, index);
    var branch = branches[index].transform;

    index = CreateFractalTree(
        branch.transform,
        new(0, height, 0),
        rotation,
        height * heightMultiplier,
        new(taper.y, taper.y * taperMultiplier),
        depth - 1,
        index + 1);

    return CreateFractalTree(
       branch.transform,
       new(0, height, 0),
       -rotation,
       height * heightMultiplier,
       new(taper.y, taper.y * taperMultiplier),
       depth - 1,
       index);
  }

  void Start() {
    for (int index = 0; index < branches.Length; index++) {
      GameObject branch = new("Branch " + index);
      Mesh mesh = new() {
        vertices = vertices,
        triangles = triangles
      };

      branch.AddComponent<MeshFilter>().mesh = mesh;
      branch.AddComponent<MeshRenderer>().material = material;
      branches[index] = branch;
      branches[index].SetActive(false);
    }
  }

  // Update is called once per frame
  void Update() {
    transform.localRotation = Quaternion.Euler(new(-rotation, 0, 0));
    for (int i = 0; i < branches.Length; i++) {
      branches[i].SetActive(false);
    }
    CreateFractalTree(
       transform,
       Vector3.zero,
       new(rotation, 0, 0),
       height,
       new(1, taperMultiplier),
       iterations,
       0);
  }
}
