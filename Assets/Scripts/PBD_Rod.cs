using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PBD_Rod : MonoBehaviour
{
    [SerializeField, Range(2, 64)] int _n_gon_vertex_count = 2;
    [SerializeField, Range(0.01f, 8)] float _radius = 1;
    [SerializeField, Range(1, 100)] int _curve_accuracy = 10;

    [Space(20)]

    [SerializeField] int n = 24; //質点の個数
    [SerializeField, Range(0, 1)] float k = 0.5f; // バネの硬さ(Stiffness)
    [SerializeField] float dt = 0.01f; // Delta Time
    [SerializeField] Vector3 gravity = Vector3.zero; // 重力
    [SerializeField] float kDamping = 0.03f; // Velocity Dampingで使用する定数
    [SerializeField] Transform startPoint; // ヒモの開始点
    [SerializeField] Transform endPoint; // ヒモの終了点
    Vector3[] x = null; // 質点の位置
    Vector3[] v = null; // 質点の速度
    Vector3[] p = null; // 質点の推定位置
    bool[] isFixed = null;
    float[] m = null; // 質量
    Constraint[] constraints; // 拘束


    MeshFilter _mesh_filter;
    MeshRenderer _mesh_renderer;

    void Awake()
    {
        if (_mesh_filter == null) _mesh_filter = GetComponent<MeshFilter>();
        if (_mesh_renderer == null) _mesh_renderer = GetComponent<MeshRenderer>();


        // n個の質点の初期化
        p = new Vector3[n]; // 推定位置
        x = new Vector3[n]; // 位置
        v = new Vector3[n]; // 速度
        m = new float[n]; // 質量
        for (int i = 0; i < n; i++)
        {
            float t = (float)(i) / (n - 1);

            // 位置
            x[i] = Vector3.Lerp(startPoint.position, endPoint.position, t);

            // 速度
            v[i] = Vector3.zero;

            // 質量 
            m[i] = 1f;
        }

        // Constraintの初期化
        constraints = new Constraint[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            // 定常状態の伸びの計算
            var d = Vector3.Magnitude(x[i] - x[i + 1]);

            // 隣り合う質点を接続
            constraints[i] = new Constraint(i, i + 1, d);
        }

        // ヒモの両端の質点は固定
        isFixed = new bool[n];
        isFixed[0] = true;
        isFixed[n - 1] = true;

    }


    void Update()
    {
        // 外力による速度変化
        for (int i = 0; i < n; i++)
        {
            v[i] += gravity * dt;
            if (isFixed[i]) v[i] = Vector3.zero;
        }

        // Velocity Damping
        VelocityDamping(n, x, v, m, kDamping);

        // 位置の更新
        for (int i = 0; i < n; i++)
        {
            p[i] = x[i] + v[i] * dt;
            x[i] = p[i];
        }
        x[0] = startPoint.position;
        x[n - 1] = endPoint.position;

        // 速度の更新 (Project Constraints)
        for (int i = 0; i < constraints.Length; i++)
        {
            var c = constraints[i];
            var p1 = p[c.i];
            var p2 = p[c.j];
            float w1 = 1f / m[c.i]; // 質量の逆数
            float w2 = 1f / m[c.j]; // 質量の逆数
            float diff = Vector3.Magnitude(p1 - p2);
            var dp1 = -k * w1 / (w1 + w2) * (diff - c.d) * Vector3.Normalize(p1 - p2);
            var dp2 = k * w2 / (w1 + w2) * (diff - c.d) * Vector3.Normalize(p1 - p2);

            v[c.i] += dp1 / dt;
            v[c.j] += dp2 / dt;
        }

        // 位置反映
        Face face = new Face(_n_gon_vertex_count, _radius);
        var mesh = FaceExtruder.SolidifyPoints(SplineInterpolation.ApplyCubicUBSFuncToVectors(x, _curve_accuracy), face);

        _mesh_filter.mesh = mesh;

    }

    /// <summary>
    /// 速度のDamping
    /// </summary>
    static void VelocityDamping(
        int n, // 質点の個数
        Vector3[] x, // 質点位置
        Vector3[] v, // 速度
        float[] m, // 質量
        float k // kDampingの値 (0 ~ 1)
    )
    {
        var xcm = Vector3.zero;
        var vcm = Vector3.zero;
        var totalMass = 0f;
        for (int i = 0; i < n; i++)
        {
            xcm += x[i];
            vcm += v[i];
            totalMass += m[i];
        }
        xcm /= totalMass;
        vcm /= totalMass;

        var L = Vector3.zero;
        var I = new SquareMatrix(3);
        var rs = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 r = x[i] - xcm;
            rs[i] = r;

            var R = new SquareMatrix(3);
            R[0, 1] = r[2];
            R[0, 2] = -r[1];
            R[1, 0] = r[2];
            R[1, 2] = -r[0];
            R[2, 0] = -r[1];
            R[2, 1] = r[0];

            L += Vector3.Cross(r, m[i] * v[i]);
            I += R * R.Transpose() * m[i];
        }

        Vector3 omega = I.Inverse() * L;
        for (int i = 0; i < n; i++)
        {
            Vector3 deltaV = vcm + Vector3.Cross(omega, rs[i]) - v[i];
            v[i] += k * deltaV;
        }
    }
}

public class Constraint
{
    public int i; // 質点その1
    public int j; // 質点その2
    public float d; // 定常状態の伸び

    public Constraint(int i, int j, float d)
    {
        this.i = i;
        this.j = j;
        this.d = d;
    }
}