using UnityEngine;

/// <summary>
/// N次正方行列
/// </summary>
[System.Serializable]
public class SquareMatrix
{
    [SerializeField] private double[] m;
    [SerializeField] private int n;
    [SerializeField] private double det;

    public int N => n;
    public double Det => det; // 行列式

    public double this[int i] {
        get => m[i];
        set => m[i] = value;
    }

    public double this[int i, int j] {
        get => m[i * n + j];
        set => m[i * n + j] = value;
    }

    public SquareMatrix(int n)
    {
        this.n = n;
        m = new double[n * n];
    }

    public SquareMatrix(int n, double[,] values)
    {
        this.n = n;
        m = new double[n * n];
        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                this[i, j] = values[i, j];
            }
        }
    }

    public SquareMatrix(int n, double[,] values, double det)
    {
        this.n = n;
        this.det = det;
        m = new double[n * n];
        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                this[i, j] = values[i, j];
            }
        }
    }

    public SquareMatrix(Vector3 rhs)
    {
        n = 3;
        this.m = new double[n];
        for (int i = 0; i < n; i++)
        {
            this[i] = rhs[i];
        }
    }

    /// <summary>
    /// 転置行列
    /// </summary>
    public SquareMatrix Transpose()
    {
        var matrix = new SquareMatrix(this.n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                matrix[j, i] = this[i, j];
            }
        }

        return matrix;
    }

    /// <summary>
    /// 逆行列
    /// </summary>
    public SquareMatrix Inverse()
    {
        var m = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                m[i, j] = this[i, j];
            }
        }

        GaussianElimination(n, m, out var det, out double[,] m2, out var I);
        return new SquareMatrix(n, I, det);
    }


    /// <summary>
    /// 掃き出し法(ガウスの消去法)
    /// </summary>
    /// <param name="n">行数・列数</param>
    /// <param name="m0">元の行列</param>
    /// <param name="m">掃き出し法の結果(階段形)</param>
    /// <param name="I">行列m0の逆行列</param>
    public static void GaussianElimination(
        int n,
        double[,] m0,
        out double det,
        out double[,] m,
        out double[,] I)
    {
        m = (double[,])m0.Clone();

        I = SquareMatrix.Identity(n);

        double diag = 1; // 対角成分の乗算

        bool[,] seen = new bool[n, n];
        for (int targetX = 0; targetX < n; targetX++)
        {
            int srcY = -1; // 0ではない要素
            bool findNonZero = false;  // 0ではない要素を見つけた
            for (int y = 0; y < n; y++)
            {
                if (seen[targetX, y]) continue;

                // 0 ではない要素を探す
                if (m[targetX, y] == 0f) continue;
                srcY = y;

                for (int xi = targetX; xi < n; xi++)
                {
                    seen[xi, srcY] = true;
                }

                findNonZero = true;
                diag *= m[targetX, y];
                break;
            }

            if (!findNonZero)
            {
                diag *= 0.0;
            }
            else
            {
                // 見ている行の左端が1になるように、行を割る
                DivideRow(n, I, srcY, m[targetX, srcY]);
                DivideRow(n, m, srcY, m[targetX, srcY]);

                for (int yi = 0; yi < n; yi++)
                {
                    if (yi == srcY) continue;

                    // 見ている行をk倍して、他の行に足す
                    double k = -m[targetX, yi];
                    AddRow(n, I, srcY, yi, k);
                    AddRow(n, m, srcY, yi, k);
                }
            }

        }

        det = diag;
    }

    public static Vector3 operator *(SquareMatrix lhs, Vector3 rhs)
    {
        Vector3 v = new Vector3();
        for (int i = 0; i < lhs.n; i++)
        {
            float sum = 0f;
            for (int j = 0; j < lhs.n; j++)
            {
                sum += (float)lhs[j, i] * rhs[j];
            }
            v[i] = sum;
        }
        return v;
    }

    /// <summary>
    /// 単位行列
    /// </summary>
    /// <param name="n"></param>
    private static double[,] Identity(int n)
    {
        var m = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            m[i, i] = 1;
        }
        return m;
    }


    /// <summary>
    /// src行目をk倍してdst行目に足す
    /// </summary>
    static void AddRow(int n, double[,] m, int src, int dst, double k)
    {
        for (int x = 0; x < n; x++)
        {
            m[x, dst] += k * m[x, src];
        }
    }

    /// <summary>
    /// src行目をkで割る
    /// </summary>
    static void DivideRow(int n, double[,] m, int y, double k)
    {
        for (int x = 0; x < n; x++)
        {
            m[x, y] /= k;
        }
    }


    public static SquareMatrix operator *(SquareMatrix lhs, float rhs)
    {
        var matrix = new SquareMatrix(lhs.n);
        int n = lhs.n;

        // matrixの(i, j)要素の計算
        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                matrix[i, j] = lhs[i, j] * rhs;
            }
        }
        return matrix;
    }

    public static SquareMatrix operator /(SquareMatrix lhs, float rhs)
    {
        var matrix = new SquareMatrix(lhs.n);
        int n = lhs.n;

        // matrixの(i, j)要素の計算
        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                matrix[i, j] = lhs[i, j] / rhs;
            }
        }
        return matrix;
    }

    public static SquareMatrix operator *(SquareMatrix lhs, double rhs)
    {
        var matrix = new SquareMatrix(lhs.n);
        int n = lhs.n;

        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                matrix[i, j] = lhs[i, j] * rhs;
            }
        }
        return matrix;
    }

    public static SquareMatrix operator +(SquareMatrix lhs, SquareMatrix rhs)
    {
        if (lhs.n != rhs.n)
            throw new System.Exception("operator+ : invalid matrix size");

        var matrix = new SquareMatrix(lhs.n);
        int n = lhs.n;

        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                matrix[i, j] = lhs[i, j] + rhs[i, j];
            }
        }
        return matrix;
    }

    public static SquareMatrix operator *(SquareMatrix lhs, SquareMatrix rhs)
    {
        if (lhs.n != rhs.n)
            throw new System.Exception("operator* : invalid matrix size");

        var matrix = new SquareMatrix(lhs.n);
        int n = lhs.n;

        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                double sum = 0f;
                for (int k = 0; k < n; k++)
                {
                    sum += lhs[k, j] * rhs[i, k];
                }
                matrix[i, j] = sum;
            }
        }
        return matrix;
    }
}