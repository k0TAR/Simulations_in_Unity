using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FaceExtruder
{
    public static Mesh SolidifyPoints(Vector3[] positions, Face extrudingFace)
    {
        Mesh solidified = new Mesh();
        Vector3[] face_vertices = extrudingFace._face_vertices;
        var mesh_vertices = new Vector3[positions.Length * face_vertices.Length];

        Quaternion prev_quat = Quaternion.identity;
        bool face_vertice_switcher = true;

        Vector3 prev_normal = Vector3.zero;
        for (int i = 0; i < positions.Length; i++)
        {
            int prior_index = Mathf.Clamp(i - 1, 0, positions.Length - 1);
            int target_index = i;
            int post_index = Mathf.Clamp(i + 1, 0, positions.Length - 1);

            Vector3 tangent = IdentifyTangent(positions[prior_index], positions[target_index], positions[post_index]);
            //Vector3 normal = CalculateUpVector(positions[prior_index], positions[target_index], positions[post_index] );
            //var RotationMatrix = GenerateTRSMatrixFrom2Vector(Vector3.forward, tangent);
            //normal = (prev_normal + normal) / 2f;
            //prev_normal = normal;

            Quaternion rotation_quaternion;
            
            if(tangent == Vector3.zero)
            {
                rotation_quaternion = Quaternion.identity;
            } else
            {
                rotation_quaternion = Quaternion.LookRotation(tangent);
            }
            

            Vector3[] rotated_face_vertices = new Vector3[face_vertices.Length];
            Vector3[] subject_face_vertices = new Vector3[face_vertices.Length];

            if (rotation_quaternion.x < 0.0f && rotation_quaternion.w > 0.0f && prev_quat.x > 0.0f && prev_quat.y > 0.0f && prev_quat.z > 0)
            {
                face_vertice_switcher = !face_vertice_switcher;
            }
            /*
            Debug.Log("X: " + rotation_quaternion.x
            + "   Y: " + rotation_quaternion.y
            + "   Z:" + rotation_quaternion.z
            + "   W:" + rotation_quaternion.w);
            */

            subject_face_vertices = face_vertice_switcher ? face_vertices : extrudingFace._inverce_face_vertices;

            for (int j = 0; j < face_vertices.Length; j++)
            {
                rotated_face_vertices[j] = rotation_quaternion * subject_face_vertices[j];
                rotated_face_vertices[j] += positions[i];

                mesh_vertices[i * face_vertices.Length + j] = rotated_face_vertices[j];
            }
            prev_quat = rotation_quaternion;
        }

        solidified.SetVertices(mesh_vertices);
        int[] mesh_triangles_index = new int[(positions.Length - 1) * 6 * extrudingFace._n_gon] ;

        for(int i = 0, j = 0; i < mesh_triangles_index.Length; j += extrudingFace._n_gon)
        {
            for(int k = 0; k < extrudingFace._n_gon; i += 6 , k++)
            {
                mesh_triangles_index[i    ] = j + k;
                mesh_triangles_index[i + 1] = j + extrudingFace._n_gon + k;
                mesh_triangles_index[i + 2] = j + (1 + k) % extrudingFace._n_gon;

                mesh_triangles_index[i + 3] = j + extrudingFace._n_gon + k;
                mesh_triangles_index[i + 4] = j + extrudingFace._n_gon + (1 + k) % extrudingFace._n_gon;
                mesh_triangles_index[i + 5] = j + (1 + k) % extrudingFace._n_gon;
            }
        }


        solidified.triangles = mesh_triangles_index;
        solidified.RecalculateBounds();
        solidified.RecalculateNormals();


        return solidified;
    }

    private static Vector3 IdentifyTangent(Vector3 prior_pos, Vector3 pos, Vector3 post_pos)
    {
        var pri_tangent = (pos - prior_pos);
        var post_tangent = (post_pos - pos);
        var avg_tangent = ( pri_tangent + post_tangent ) / 2f;
        return avg_tangent.normalized;
    }

    private static Matrix4x4 GenerateTRSMatrixFrom2Vector(Vector3 original_direction, Vector3 target)
    {
        Quaternion rotate_quaternion = Quaternion.FromToRotation(original_direction, target);
        //var rotating_axis = Vector3.Cross(original_direction, target).normalized;

        var matrix = new Matrix4x4();
        matrix.SetTRS(Vector3.zero, rotate_quaternion, Vector3.one);

        return matrix;
    }

    private static Vector3 CalculateUpVector(Vector3 prior_pos, Vector3 pos, Vector3 post_pos)
    {
        var pri_tangent = (pos - prior_pos).normalized;
        var post_tangent = (post_pos - pos).normalized;
        var up_vector = (post_tangent - pri_tangent).normalized;
        return up_vector;
    }
}

public class Face
{
    //Make Z axis the vector which the face looks at.
    //and rotate the face in FaceExtruder by looking at neighbour _middle point
    //(take average of the vector from front and back vertices).
    public Vector3 _middle;
    public Vector3 _normal = new Vector3(0,0,1);
    public Vector3 _up = new Vector3(0,1,0);
    public int _n_gon;
    public Vector3[] _face_vertices;
    public Vector3[] _inverce_face_vertices;

    public Face(int n_gon_vertex_count, float radius)//, float offset_angle
    {
        _middle = new Vector3(0,0,0);
        _n_gon = n_gon_vertex_count;
        _face_vertices = new Vector3[n_gon_vertex_count];
        _inverce_face_vertices = new Vector3[n_gon_vertex_count];

        Vector3 initial_pos = _middle + new Vector3(radius, 0, 0);

        for(int i = 0; i < n_gon_vertex_count; i++)
        {
            Vector3 rotated_pos = AffineTransformer.Rotate(
                initial_pos, - 360f / n_gon_vertex_count * i, AffineTransformer.RotateAxis.Z
                );
            _face_vertices[i] = rotated_pos;
        }

        initial_pos = _middle - new Vector3(radius, 0, 0);
        for (int i = 0; i < n_gon_vertex_count; i++)
        {
            Vector3 rotated_pos = AffineTransformer.Rotate(
                initial_pos, -360f / n_gon_vertex_count * i, AffineTransformer.RotateAxis.Z
                );
            _inverce_face_vertices[i] = rotated_pos;
        }
    }

}


//Quaternion2Matrix
//var x = rot.x;
//var y = rot.y;
//var z = rot.z;
//var w = rot.w;
//matrix[0, 0] = 1 - 2 * y * y - 2 * z * z;
//matrix[0, 1] = 2 * x * y + 2 * w * z;
//matrix[0, 2] = 2 * x * z - 2 * w * y;
//matrix[0, 3] = 0;
//matrix[1, 0] = 2 * x * y - 2 * w * z;
//matrix[1, 1] = 1 - 2 * x * x + 2 * z * z;
//matrix[1, 2] = 2 * y * z - 2 * w * x;
//matrix[1, 3] = 0;
//matrix[2, 0] = 2 * x * z + 2 * w * y;
//matrix[2, 1] = 2 * y * z - 2 * w * x;
//matrix[2, 2] = 1 - 2 * x * x - 2 * y * y;
//matrix[2, 3] = 0; 
//matrix[3, 0] = 0;
//matrix[3, 1] = 0;
//matrix[3, 2] = 0;
//matrix[3, 3] = 1;