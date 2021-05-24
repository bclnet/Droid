//#define MATX_SIMD
using System;
using System.Diagnostics;

namespace Droid.Core
{
    partial struct MatrixX
    {
        /// <summary>
        /// Computes (a^2 + b^2)^1/2 without underflow or overflow.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        float Pythag(float a, float b)
        {
            var at = MathX.Fabs(a);
            var bt = MathX.Fabs(b);
            float ct;
            if (at > bt)
            {
                ct = bt / at;
                return at * MathX.Sqrt(1.0f + ct * ct);
            }
            else
            {
                if (bt != 0) { ct = at / bt; return bt * MathX.Sqrt(1.0f + ct * ct); }
                else return 0.0f;
            }
        }

        /// <summary>
        /// Householder reduction to symmetric tri-diagonal form.
        /// The original matrix is replaced by an orthogonal matrix effecting the accumulated householder transformations.
        /// The diagonal elements of the diagonal matrix are stored in diag.
        /// The off-diagonal elements of the diagonal matrix are stored in subd.
        /// The initial matrix has to be symmetric.
        /// </summary>
        /// <param name="diag">The diag.</param>
        /// <param name="subd">The subd.</param>
        /// <returns></returns>
        void HouseholderReduction(VectorX diag, VectorX subd)
        {
            int i0, i1, i2, i3;
            float h, f, g, invH, halfFdivH, scale, invScale, sum;

            Debug.Assert(numRows == numColumns);

            diag.SetSize(numRows);
            subd.SetSize(numRows);

            for (i0 = numRows - 1, i3 = numRows - 2; i0 >= 1; i0--, i3--)
            {
                h = 0.0f;
                scale = 0.0f;

                if (i3 > 0)
                {
                    for (i2 = 0; i2 <= i3; i2++)
                        scale += MathX.Fabs(this[i0][i2]);
                    if (scale == 0)
                        subd[i0] = this[i0][i3];
                    else
                    {
                        invScale = 1.0f / scale;
                        for (i2 = 0; i2 <= i3; i2++)
                        {
                            this[i0][i2] *= invScale;
                            h += this[i0][i2] * this[i0][i2];
                        }
                        f = this[i0][i3];
                        g = MathX.Sqrt(h);
                        if (f > 0.0f)
                            g = -g;
                        subd[i0] = scale * g;
                        h -= f * g;
                        this[i0][i3] = f - g;
                        f = 0.0f;
                        invH = 1.0f / h;
                        for (i1 = 0; i1 <= i3; i1++)
                        {
                            this[i1][i0] = this[i0][i1] * invH;
                            g = 0.0f;
                            for (i2 = 0; i2 <= i1; i2++)
                                g += this[i1][i2] * this[i0][i2];
                            for (i2 = i1 + 1; i2 <= i3; i2++)
                                g += this[i2][i1] * this[i0][i2];
                            subd[i1] = g * invH;
                            f += subd[i1] * this[i0][i1];
                        }
                        halfFdivH = 0.5f * f * invH;
                        for (i1 = 0; i1 <= i3; i1++)
                        {
                            f = this[i0][i1];
                            g = subd[i1] - halfFdivH * f;
                            subd[i1] = g;
                            for (i2 = 0; i2 <= i1; i2++)
                                this[i1][i2] -= f * subd[i2] + g * this[i0][i2];
                        }
                    }
                }
                else
                    subd[i0] = this[i0][i3];

                diag[i0] = h;
            }

            diag[0] = 0.0f;
            subd[0] = 0.0f;
            for (i0 = 0, i3 = -1; i0 <= numRows - 1; i0++, i3++)
            {
                if (diag[i0] != 0)
                {
                    for (i1 = 0; i1 <= i3; i1++)
                    {
                        sum = 0.0f;
                        for (i2 = 0; i2 <= i3; i2++)
                            sum += this[i0][i2] * this[i2][i1];
                        for (i2 = 0; i2 <= i3; i2++)
                            this[i2][i1] -= sum * this[i2][i0];
                    }
                }
                diag[i0] = this[i0][i0];
                this[i0][i0] = 1.0f;
                for (i1 = 0; i1 <= i3; i1++)
                {
                    this[i1][i0] = 0.0f;
                    this[i0][i1] = 0.0f;
                }
            }

            // re-order
            for (i0 = 1, i3 = 0; i0 < numRows; i0++, i3++)
                subd[i3] = subd[i0];
            subd[numRows - 1] = 0.0f;
        }

        /// <summary>
        /// QL algorithm with implicit shifts to determine the eigenvalues and eigenvectors of a symmetric tri-diagonal matrix.
        /// diag contains the diagonal elements of the symmetric tri-diagonal matrix on input and is overwritten with the eigenvalues.
        /// subd contains the off-diagonal elements of the symmetric tri-diagonal matrix and is destroyed.
        /// This matrix has to be either the identity matrix to determine the eigenvectors for a symmetric tri-diagonal matrix,
        /// or the matrix returned by the Householder reduction to determine the eigenvalues for the original symmetric matrix.
        /// </summary>
        /// <param name="diag">The diag.</param>
        /// <param name="subd">The subd.</param>
        /// <returns></returns>
        bool QL(VectorX diag, VectorX subd)
        {
            const int maxIter = 32;
            int i0, i1, i2, i3;
            float a, b, f, g, r, p, s, c;

            Debug.Assert(numRows == numColumns);

            for (i0 = 0; i0 < numRows; i0++)
            {
                for (i1 = 0; i1 < maxIter; i1++)
                {
                    for (i2 = i0; i2 <= numRows - 2; i2++)
                    {
                        a = MathX.Fabs(diag[i2]) + MathX.Fabs(diag[i2 + 1]);
                        if (MathX.Fabs(subd[i2]) + a == a)
                            break;
                    }
                    if (i2 == i0)
                        break;

                    g = (diag[i0 + 1] - diag[i0]) / (2.0f * subd[i0]);
                    r = MathX.Sqrt(g * g + 1.0f);
                    if (g < 0.0f)
                        g = diag[i2] - diag[i0] + subd[i0] / (g - r);
                    else
                        g = diag[i2] - diag[i0] + subd[i0] / (g + r);
                    s = 1.0f;
                    c = 1.0f;
                    p = 0.0f;
                    for (i3 = i2 - 1; i3 >= i0; i3--)
                    {
                        f = s * subd[i3];
                        b = c * subd[i3];
                        if (MathX.Fabs(f) >= MathX.Fabs(g))
                        {
                            c = g / f;
                            r = MathX.Sqrt(c * c + 1.0f);
                            subd[i3 + 1] = f * r;
                            s = 1.0f / r;
                            c *= s;
                        }
                        else
                        {
                            s = f / g;
                            r = MathX.Sqrt(s * s + 1.0f);
                            subd[i3 + 1] = g * r;
                            c = 1.0f / r;
                            s *= c;
                        }
                        g = diag[i3 + 1] - p;
                        r = (diag[i3] - g) * s + 2.0f * b * c;
                        p = s * r;
                        diag[i3 + 1] = g + p;
                        g = c * r - b;

                        for (var i4 = 0; i4 < numRows; i4++)
                        {
                            f = this[i4][i3 + 1];
                            this[i4][i3 + 1] = s * this[i4][i3] + c * f;
                            this[i4][i3] = c * this[i4][i3] - s * f;
                        }
                    }
                    diag[i0] -= p;
                    subd[i0] = g;
                    subd[i2] = 0.0f;
                }
                if (i1 == maxIter)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Reduction to Hessenberg form.
        /// </summary>
        /// <param name="H">The h.</param>
        /// <returns></returns>
        void HessenbergReduction(MatrixX H)
        {
            int i, j, m;
            int low = 0;
            int high = numRows - 1;
            float scale, f, g, h;
            var v = new VectorX();

            v.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            for (m = low + 1; m <= high - 1; m++)
            {
                scale = 0.0f;
                for (i = m; i <= high; i++)
                    scale += MathX.Fabs(H[i][m - 1]);
                if (scale != 0.0f)
                {
                    // compute Householder transformation.
                    h = 0.0f;
                    for (i = high; i >= m; i--)
                    {
                        v[i] = H[i][m - 1] / scale;
                        h += v[i] * v[i];
                    }
                    g = MathX.Sqrt(h);
                    if (v[m] > 0.0f)
                        g = -g;
                    h -= v[m] * g;
                    v[m] = v[m] - g;

                    // apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)
                    for (j = m; j < numRows; j++)
                    {
                        f = 0.0f;
                        for (i = high; i >= m; i--)
                            f += v[i] * H[i][j];
                        f /= h;
                        for (i = m; i <= high; i++)
                            H[i][j] -= f * v[i];
                    }

                    for (i = 0; i <= high; i++)
                    {
                        f = 0.0f;
                        for (j = high; j >= m; j--)
                            f += v[j] * H[i][j];
                        f /= h;
                        for (j = m; j <= high; j++)
                            H[i][j] -= f * v[j];
                    }
                    v[m] = scale * v[m];
                    H[m][m - 1] = scale * g;
                }
            }

            // accumulate transformations
            Identity();
            for (m = high - 1; m >= low + 1; m--)
            {
                if (H[m][m - 1] != 0.0f)
                {
                    for (i = m + 1; i <= high; i++)
                        v[i] = H[i][m - 1];
                    for (j = m; j <= high; j++)
                    {
                        g = 0.0f;
                        for (i = m; i <= high; i++)
                            g += v[i] * this[i][j];
                        // float division to avoid possible underflow
                        g = g / v[m] / H[m][m - 1];
                        for (i = m; i <= high; i++)
                            this[i][j] += g * v[i];
                    }
                }
            }
        }
        /// <summary>
        /// Complex scalar division.
        /// </summary>
        /// <param name="xr">The xr.</param>
        /// <param name="xi">The xi.</param>
        /// <param name="yr">The yr.</param>
        /// <param name="yi">The yi.</param>
        /// <param name="cdivr">The cdivr.</param>
        /// <param name="cdivi">The cdivi.</param>
        /// <returns></returns>
        void ComplexDivision(float xr, float xi, float yr, float yi, ref float cdivr, ref float cdivi)
        {
            float r, d;
            if (MathX.Fabs(yr) > MathX.Fabs(yi))
            {
                r = yi / yr;
                d = yr + r * yi;
                cdivr = (xr + r * xi) / d;
                cdivi = (xi - r * xr) / d;
            }
            else
            {
                r = yr / yi;
                d = yi + r * yr;
                cdivr = (r * xr + xi) / d;
                cdivi = (r * xi - xr) / d;
            }
        }
        /// <summary>
        /// Reduction from Hessenberg to real Schur form.
        /// </summary>
        /// <param name="H">The h.</param>
        /// <param name="realEigenValues">The real eigen values.</param>
        /// <param name="imaginaryEigenValues">The imaginary eigen values.</param>
        /// <returns></returns>
        bool HessenbergToRealSchur(MatrixX H, VectorX realEigenValues, VectorX imaginaryEigenValues)
        {
            int i, j, k;
            int n = numRows - 1;
            int low = 0;
            int high = numRows - 1;
            float eps = 2e-16f, exshift = 0.0f;
            float p = 0.0f, q = 0.0f, r = 0.0f, s = 0.0f, z = 0.0f, t, w, x, y;

            // store roots isolated by balanc and compute matrix norm
            var norm = 0.0f;
            for (i = 0; i < numRows; i++)
            {
                if (i < low || i > high)
                {
                    realEigenValues[i] = H[i][i];
                    imaginaryEigenValues[i] = 0.0f;
                }
                for (j = Math.Max(i - 1, 0); j < numRows; j++)
                    norm += MathX.Fabs(H[i][j]);
            }

            var iter = 0;
            while (n >= low)
            {
                // look for single small sub-diagonal element
                var l = n;
                while (l > low)
                {
                    s = MathX.Fabs(H[l - 1][l - 1]) + MathX.Fabs(H[l][l]);
                    if (s == 0.0f)
                        s = norm;
                    if (MathX.Fabs(H[l][l - 1]) < eps * s)
                        break;
                    l--;
                }

                // check for convergence
                if (l == n)
                {   // one root found
                    H[n][n] = H[n][n] + exshift;
                    realEigenValues[n] = H[n][n];
                    imaginaryEigenValues[n] = 0.0f;
                    n--;
                    iter = 0;
                }
                else if (l == n - 1)
                {   // two roots found
                    w = H[n][n - 1] * H[n - 1][n];
                    p = (H[n - 1][n - 1] - H[n][n]) / 2.0f;
                    q = p * p + w;
                    z = MathX.Sqrt(MathX.Fabs(q));
                    H[n][n] = H[n][n] + exshift;
                    H[n - 1][n - 1] = H[n - 1][n - 1] + exshift;
                    x = H[n][n];

                    if (q >= 0.0f)
                    {   // real pair
                        if (p >= 0.0f)
                            z = p + z;
                        else
                            z = p - z;
                        realEigenValues[n - 1] = x + z;
                        realEigenValues[n] = realEigenValues[n - 1];
                        if (z != 0.0f)
                            realEigenValues[n] = x - w / z;
                        imaginaryEigenValues[n - 1] = 0.0f;
                        imaginaryEigenValues[n] = 0.0f;
                        x = H[n][n - 1];
                        s = MathX.Fabs(x) + MathX.Fabs(z);
                        p = x / s;
                        q = z / s;
                        r = MathX.Sqrt(p * p + q * q);
                        p /= r;
                        q /= r;

                        // modify row
                        for (j = n - 1; j < numRows; j++)
                        {
                            z = H[n - 1][j];
                            H[n - 1][j] = q * z + p * H[n][j];
                            H[n][j] = q * H[n][j] - p * z;
                        }

                        // modify column
                        for (i = 0; i <= n; i++)
                        {
                            z = H[i][n - 1];
                            H[i][n - 1] = q * z + p * H[i][n];
                            H[i][n] = q * H[i][n] - p * z;
                        }

                        // accumulate transformations
                        for (i = low; i <= high; i++)
                        {
                            z = this[i][n - 1];
                            this[i][n - 1] = q * z + p * this[i][n];
                            this[i][n] = q * this[i][n] - p * z;
                        }
                    }
                    else
                    {   // complex pair
                        realEigenValues[n - 1] = x + p;
                        realEigenValues[n] = x + p;
                        imaginaryEigenValues[n - 1] = z;
                        imaginaryEigenValues[n] = -z;
                    }
                    n -= 2;
                    iter = 0;
                }
                else
                {   // no convergence yet

                    // form shift
                    x = H[n][n];
                    y = 0.0f;
                    w = 0.0f;
                    if (l < n)
                    {
                        y = H[n - 1][n - 1];
                        w = H[n][n - 1] * H[n - 1][n];
                    }

                    // Wilkinson's original ad hoc shift
                    if (iter == 10)
                    {
                        exshift += x;
                        for (i = low; i <= n; i++)
                            H[i][i] -= x;
                        s = MathX.Fabs(H[n][n - 1]) + MathX.Fabs(H[n - 1][n - 2]);
                        x = y = 0.75f * s;
                        w = -0.4375f * s * s;
                    }

                    // new ad hoc shift
                    if (iter == 30)
                    {
                        s = (y - x) / 2.0f;
                        s = s * s + w;
                        if (s > 0)
                        {
                            s = MathX.Sqrt(s);
                            if (y < x)
                                s = -s;
                            s = x - w / ((y - x) / 2.0f + s);
                            for (i = low; i <= n; i++)
                                H[i][i] -= s;
                            exshift += s;
                            x = y = w = 0.964f;
                        }
                    }

                    iter++;

                    // look for two consecutive small sub-diagonal elements
                    int m;
                    for (m = n - 2; m >= l; m--)
                    {
                        z = H[m][m];
                        r = x - z;
                        s = y - z;
                        p = (r * s - w) / H[m + 1][m] + H[m][m + 1];
                        q = H[m + 1][m + 1] - z - r - s;
                        r = H[m + 2][m + 1];
                        s = MathX.Fabs(p) + MathX.Fabs(q) + MathX.Fabs(r);
                        p /= s;
                        q /= s;
                        r /= s;
                        if (m == l || MathX.Fabs(H[m][m - 1]) * (MathX.Fabs(q) + MathX.Fabs(r)) < eps * (MathX.Fabs(p) * (MathX.Fabs(H[m - 1][m - 1]) + MathX.Fabs(z) + MathX.Fabs(H[m + 1][m + 1]))))
                            break;
                    }

                    for (i = m + 2; i <= n; i++)
                    {
                        H[i][i - 2] = 0.0f;
                        if (i > m + 2)
                            H[i][i - 3] = 0.0f;
                    }

                    // double QR step involving rows l:n and columns m:n
                    for (k = m; k <= n - 1; k++)
                    {
                        var notlast = k != n - 1;
                        if (k != m)
                        {
                            p = H[k][k - 1];
                            q = H[k + 1][k - 1];
                            r = notlast ? H[k + 2][k - 1] : 0.0f;
                            x = MathX.Fabs(p) + MathX.Fabs(q) + MathX.Fabs(r);
                            if (x != 0.0f)
                            {
                                p /= x;
                                q /= x;
                                r /= x;
                            }
                        }
                        if (x == 0.0f)
                            break;
                        s = MathX.Sqrt(p * p + q * q + r * r);
                        if (p < 0.0f)
                            s = -s;
                        if (s != 0.0f)
                        {
                            if (k != m)
                                H[k][k - 1] = -s * x;
                            else if (l != m)
                                H[k][k - 1] = -H[k][k - 1];
                            p += s;
                            x = p / s;
                            y = q / s;
                            z = r / s;
                            q /= p;
                            r /= p;

                            // modify row
                            for (j = k; j < numRows; j++)
                            {
                                p = H[k][j] + q * H[k + 1][j];
                                if (notlast)
                                {
                                    p += r * H[k + 2][j];
                                    H[k + 2][j] = H[k + 2][j] - p * z;
                                }
                                H[k][j] = H[k][j] - p * x;
                                H[k + 1][j] = H[k + 1][j] - p * y;
                            }

                            // modify column
                            for (i = 0; i <= Math.Min(n, k + 3); i++)
                            {
                                p = x * H[i][k] + y * H[i][k + 1];
                                if (notlast)
                                {
                                    p += z * H[i][k + 2];
                                    H[i][k + 2] = H[i][k + 2] - p * r;
                                }
                                H[i][k] = H[i][k] - p;
                                H[i][k + 1] = H[i][k + 1] - p * q;
                            }

                            // accumulate transformations
                            for (i = low; i <= high; i++)
                            {
                                p = x * this[i][k] + y * this[i][k + 1];
                                if (notlast)
                                {
                                    p += z * this[i][k + 2];
                                    this[i][k + 2] = this[i][k + 2] - p * r;
                                }
                                this[i][k] = this[i][k] - p;
                                this[i][k + 1] = this[i][k + 1] - p * q;
                            }
                        }
                    }
                }
            }

            // backsubstitute to find vectors of upper triangular form
            if (norm == 0.0f)
                return false;

            for (n = numRows - 1; n >= 0; n--)
            {
                p = realEigenValues[n];
                q = imaginaryEigenValues[n];

                if (q == 0.0f)
                {   // real vector
                    var l = n;
                    H[n][n] = 1.0f;
                    for (i = n - 1; i >= 0; i--)
                    {
                        w = H[i][i] - p;
                        r = 0.0f;
                        for (j = l; j <= n; j++)
                            r += H[i][j] * H[j][n];
                        if (imaginaryEigenValues[i] < 0.0f)
                        {
                            z = w;
                            s = r;
                        }
                        else
                        {
                            l = i;
                            if (imaginaryEigenValues[i] == 0.0f)
                            {
                                if (w != 0.0f)
                                    H[i][n] = -r / w;
                                else
                                    H[i][n] = -r / (eps * norm);
                            }
                            else
                            {       // solve real equations
                                x = H[i][i + 1];
                                y = H[i + 1][i];
                                q = (realEigenValues[i] - p) * (realEigenValues[i] - p) + imaginaryEigenValues[i] * imaginaryEigenValues[i];
                                t = (x * s - z * r) / q;
                                H[i][n] = t;
                                if (MathX.Fabs(x) > MathX.Fabs(z))
                                    H[i + 1][n] = (-r - w * t) / x;
                                else
                                    H[i + 1][n] = (-s - y * t) / z;
                            }

                            // overflow control
                            t = MathX.Fabs(H[i][n]);
                            if ((eps * t) * t > 1)
                                for (j = i; j <= n; j++)
                                    H[j][n] = H[j][n] / t;
                        }
                    }
                }
                else if (q < 0.0f)
                {   // complex vector
                    var l = n - 1;

                    // last vector component imaginary so matrix is triangular
                    if (MathX.Fabs(H[n][n - 1]) > MathX.Fabs(H[n - 1][n]))
                    {
                        H[n - 1][n - 1] = q / H[n][n - 1];
                        H[n - 1][n] = -(H[n][n] - p) / H[n][n - 1];
                    }
                    else
                        ComplexDivision(0.0f, -H[n - 1][n], H[n - 1][n - 1] - p, q, ref H[n - 1][n - 1], ref H[n - 1][n]);
                    H[n][n - 1] = 0.0f;
                    H[n][n] = 1.0f;
                    for (i = n - 2; i >= 0; i--)
                    {
                        float ra, sa, vr, vi;
                        ra = 0.0f;
                        sa = 0.0f;
                        for (j = l; j <= n; j++)
                        {
                            ra += H[i][j] * H[j][n - 1];
                            sa += H[i][j] * H[j][n];
                        }
                        w = H[i][i] - p;

                        if (imaginaryEigenValues[i] < 0.0f)
                        {
                            z = w;
                            r = ra;
                            s = sa;
                        }
                        else
                        {
                            l = i;
                            if (imaginaryEigenValues[i] == 0.0f)
                                ComplexDivision(-ra, -sa, w, q, ref H[i][n - 1], ref H[i][n]);
                            else
                            {
                                // solve complex equations
                                x = H[i][i + 1];
                                y = H[i + 1][i];
                                vr = (realEigenValues[i] - p) * (realEigenValues[i] - p) + imaginaryEigenValues[i] * imaginaryEigenValues[i] - q * q;
                                vi = (realEigenValues[i] - p) * 2.0f * q;
                                if (vr == 0.0f && vi == 0.0f)
                                    vr = eps * norm * (MathX.Fabs(w) + MathX.Fabs(q) + MathX.Fabs(x) + MathX.Fabs(y) + MathX.Fabs(z));
                                ComplexDivision(x * r - z * ra + q * sa, x * s - z * sa - q * ra, vr, vi, ref H[i][n - 1], ref H[i][n]);
                                if (MathX.Fabs(x) > (MathX.Fabs(z) + MathX.Fabs(q)))
                                {
                                    H[i + 1][n - 1] = (-ra - w * H[i][n - 1] + q * H[i][n]) / x;
                                    H[i + 1][n] = (-sa - w * H[i][n] - q * H[i][n - 1]) / x;
                                }
                                else
                                    ComplexDivision(-r - y * H[i][n - 1], -s - y * H[i][n], z, q, ref H[i + 1][n - 1], ref H[i + 1][n]);
                            }

                            // overflow control
                            t = Math.Max(MathX.Fabs(H[i][n - 1]), MathX.Fabs(H[i][n]));
                            if (eps * t * t > 1)
                                for (j = i; j <= n; j++)
                                {
                                    H[j][n - 1] = H[j][n - 1] / t;
                                    H[j][n] = H[j][n] / t;
                                }
                        }
                    }
                }
            }

            // vectors of isolated roots
            for (i = 0; i < numRows; i++)
                if (i < low || i > high)
                    for (j = i; j < numRows; j++)
                        this[i][j] = H[i][j];

            // back transformation to get eigenvectors of original matrix
            for (j = numRows - 1; j >= low; j--)
                for (i = low; i <= high; i++)
                {
                    z = 0.0f;
                    for (k = low; k <= Math.Min(j, high); k++)
                        z += this[i][k] * H[k][j];
                    this[i][j] = z;
                }

            return true;
        }
    }
}
