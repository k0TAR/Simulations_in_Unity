using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FaceExtrudeTester : MonoBehaviour
{
    [SerializeField] Transform _cps_transform_parent;
    [SerializeField, Range(2, 64)] int _n_gon_vertex_count = 2;
    [SerializeField, Range(0.01f, 8)] float _radius = 1;
    [SerializeField, Range(1, 100)] int _curve_accuracy = 10;

    MeshFilter _mesh_filter;
    MeshRenderer _mesh_renderer;

    private void OnValidate()
    {
        if (_cps_transform_parent == null) return;
        if (_mesh_filter == null) _mesh_filter = GetComponent<MeshFilter>();
        if (_mesh_renderer == null) _mesh_renderer = GetComponent<MeshRenderer>();

        var cps_transform = _cps_transform_parent.GetComponentsInChildren<Transform>();
        var cps_positions = cps_transform
            .Where((val, index) => index != 0).Select(c => c.position).ToArray();

        Face face = new Face(_n_gon_vertex_count, _radius);
        var mesh = FaceExtruder.SolidifyPoints(SplineInterpolation.ApplyCubicUBSFuncToVectors(cps_positions, _curve_accuracy), face);

        _mesh_filter.mesh = mesh;

    }

    /*
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0, 1f);
        Gizmos.DrawWireMesh(_mesh_filter.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one);
    }*/
}
