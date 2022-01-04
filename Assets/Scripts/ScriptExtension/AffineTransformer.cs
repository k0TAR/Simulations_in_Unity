using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class AffineTransformer
{
    public enum SkewAxis : byte
    {
        XYplane_Y,
        XZplane_Z,
    }

    public enum RotateAxis : byte
    {
        X, Y, Z
    }

    private static float[,] MakeSkewMatrix(float degree, SkewAxis axis)
    {
        switch (axis)
        {
            case SkewAxis.XYplane_Y:
                return new float[3, 3]
                {
                    { 1, Mathf.Tan(degree * 0.0174532924f), 0},
                    { 0,                  1, 0},
                    { 0,                  0, 1},
                };

            case SkewAxis.XZplane_Z:
                return new float[3, 3]
                {
                    { 1, 0, Mathf.Tan(degree * 0.0174532924f)},
                    { 0, 1, 0},
                    { 0, 0, 1},
                };

            default:
                return new float[3, 3]
                {
                    { 1, Mathf.Tan(degree * 0.0174532924f), 0},
                    { 0,                  1, 0},
                    { 0,                  0, 1},
                };
        }

    }

    private static float[] Calc3x3Matrix3Vector(float[,] matrix, float[] vector)
    {
        var result_vector = new float[vector.Length];
        float dot_product_total = 0;
        for (int i = 0; i < 3; i++)
        {
            dot_product_total = 0;
            for (int j = 0; j < 3; j++)
            {
                dot_product_total += matrix[i, j] * vector[j];
            }
            result_vector[i] = dot_product_total;
        }

        return result_vector;
    }

    public static Vector3orFloats Skew(Vector3 position, float degree, SkewAxis axis)
    {
        float[] subject_position = new Vector3orFloats(position);


        float[,] skewMatrix = MakeSkewMatrix(degree, axis);
        var skewed_position = Calc3x3Matrix3Vector(skewMatrix, subject_position);


        return new Vector3orFloats(skewed_position);
    }

    public static Vector3orFloats Skew(float[] position, float degree, SkewAxis axis)
    {
        float[,] skewMatrix = MakeSkewMatrix(degree, axis);


        var skewed_position = Calc3x3Matrix3Vector(skewMatrix, position);


        return new Vector3orFloats(skewed_position);
    }

    public static Vector3orFloats[] SkewPoints(Vector3[] positions, float degree, SkewAxis axis)
    {
        Vector3orFloats[] skewed_positions = new Vector3orFloats[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            skewed_positions[i] = Skew(positions[i], degree, axis);
        }

        return skewed_positions;
    }

    public static Vector3orFloats[] SkewPoints(float[][] positions, float degree, SkewAxis axis)
    {
        Vector3orFloats[] skewed_positions = new Vector3orFloats[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            skewed_positions[i] = Skew(positions[i], degree, axis);
        }

        return skewed_positions;
    }

    public static Vector3orFloats Rotate(Vector3 position, float degree, RotateAxis axis)
    {
        var rotate = Quaternion.Euler(new Vector3(
            axis == RotateAxis.X ? degree : 0,
            axis == RotateAxis.Y ? degree : 0,
            axis == RotateAxis.Z ? degree : 0
            ));
        var matrix = new Matrix4x4();
        matrix.SetTRS(Vector3.zero, rotate, Vector3.one);
        Vector3 rotatedPosition = matrix.MultiplyPoint(position);
        return new Vector3orFloats(rotatedPosition);
    }

    public static Vector3orFloats Rotate(float[] position, float degree, RotateAxis axis)
    {
        Vector3 subject_position = new Vector3orFloats(position);
        var rotate = Quaternion.Euler(new Vector3(
            axis == RotateAxis.X ? degree : 0,
            axis == RotateAxis.Y ? degree : 0,
            axis == RotateAxis.Z ? degree : 0
            ));
        var matrix = new Matrix4x4();
        matrix.SetTRS(Vector3.zero, rotate, Vector3.one);
        Vector3 rotatedPosition = matrix.MultiplyPoint(subject_position);
        return new Vector3orFloats(rotatedPosition);
    }

    public static Vector3orFloats[] RotatePoints(Vector3[] positions, float degree, RotateAxis axis)
    {
        Vector3orFloats[] rotated_positions = new Vector3orFloats[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            rotated_positions[i] = Rotate(positions[i], degree, axis);
        }

        return rotated_positions;
    }

    public static Vector3orFloats[] RotatePoints(float[][] positions, float degree, RotateAxis axis)
    {
        Vector3orFloats[] rotated_positions = new Vector3orFloats[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            rotated_positions[i] = Rotate(positions[i], degree, axis);
        }

        return rotated_positions;
    }
}

public class Vector3orFloats
{
    private float[] _position;

    public Vector3orFloats(float[] position)
    {
        this._position = position;
    }

    public Vector3orFloats(Vector3 position)
    {
        this._position = new float[3] { position.x, position.y, position.z };
    }

    public static implicit operator Vector3(Vector3orFloats value)
    {
        return new Vector3(value._position[0], value._position[1], value._position[2]);
    }

    public static implicit operator float[](Vector3orFloats value)
    {
        return value._position;
    }
}
