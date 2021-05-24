//#define MATX_SIMD
using System;
using System.Diagnostics;

namespace Droid.Core
{
    partial struct MatrixX
    {
        #region Update

        /// <summary>
        /// Updates the matrix to obtain the matrix: A + alpha * v * w'
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns></returns>
        public void Update_RankOne(VectorX v, VectorX w, float alpha)
        {
            int i, j; float s;
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(w.Size >= numColumns);
            for (i = 0; i < numRows; i++)
            {
                s = alpha * v[i];
                for (j = 0; j < numColumns; j++)
                    this[i][j] += s * w[j];
            }
        }
        /// <summary>
        /// Updates the matrix to obtain the matrix: A + alpha * v * v'
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns></returns>
        public void Update_RankOneSymmetric(VectorX v, float alpha)
        {
            int i, j; float s;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            for (i = 0; i < numRows; i++)
            {
                s = alpha * v[i];
                for (j = 0; j < numColumns; j++)
                    this[i][j] += s * v[j];
            }
        }
        /// <summary>
        /// Updates the matrix to obtain the matrix:
        /// 
        ///     [ 0  a  0 ]
        /// A + [ d  b  e ]
        ///     [ 0  c  0 ]
        /// where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1], d = w[0,r-1], w[r] = 0.0f, e = w[r+1,numColumns-1]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public void Update_RowColumn(VectorX v, VectorX w, int r)
        {
            int i;
            Debug.Assert(w[r] == 0.0f);
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);
            for (i = 0; i < numRows; i++)
                this[i][r] += v[i];
            for (i = 0; i < numColumns; i++)
                this[r][i] += w[i];
        }
        /// <summary>
        /// Updates the matrix to obtain the matrix:
        /// 
        ///     [ 0  a  0 ]
        /// A + [ a  b  c ]
        ///     [ 0  c  0 ]
        /// 
        /// where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public void Update_RowColumnSymmetric(VectorX v, int r)
        {
            int i;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            for (i = 0; i < r; i++)
            {
                this[i][r] += v[i];
                this[r][i] += v[i];
            }
            this[r][r] += v[r];
            for (i = r + 1; i < numRows; i++)
            {
                this[i][r] += v[i];
                this[r][i] += v[i];
            }
        }
        /// <summary>
        /// Updates the matrix to obtain the matrix:
        /// 
        /// [ A  a ]
        /// [ c  b ]
        /// 
        /// where: a = v[0,numRows-1], b = v[numRows], c = w[0,numColumns-1]], w[numColumns] = 0
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <returns></returns>
        public void Update_Increment(VectorX v, VectorX w)
        {
            int i;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);
            Debug.Assert(w.Size >= numColumns + 1);
            ChangeSize(numRows + 1, numColumns + 1, false);
            for (i = 0; i < numRows; i++)
                this[i][numColumns - 1] = v[i];
            for (i = 0; i < numColumns - 1; i++)
                this[numRows - 1][i] = w[i];
        }
        /// <summary>
        /// Updates the matrix to obtain the matrix:
        ///
        /// [ A  a ]
        /// [ a  b ]
        ///
        /// where: a = v[0,numRows-1], b = v[numRows]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public void Update_IncrementSymmetric(VectorX v)
        {
            int i;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);
            ChangeSize(numRows + 1, numColumns + 1, false);
            for (i = 0; i < numRows - 1; i++)
                this[i][numColumns - 1] = v[i];
            for (i = 0; i < numColumns; i++)
                this[numRows - 1][i] = v[i];
        }
        /// <summary>
        /// Updates the matrix to obtain a matrix with row r and column r removed.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public void Update_Decrement(int r)
            => RemoveRowColumn(r);

        #endregion

        #region Inverse

        /// <summary>
        /// in-place inversion using Gauss-Jordan elimination
        /// </summary>
        /// <returns></returns>
        public unsafe bool Inverse_GaussJordan()                    // invert in-place with Gauss-Jordan elimination
        {
            int i, j, k, r, c; float d, max;
            Debug.Assert(numRows == numColumns);

            var columnIndex = stackalloc int[numRows];
            var rowIndex = stackalloc int[numRows];
            var pivot = stackalloc bool[numRows];

            U.memset(pivot, 0, numRows * sizeof(float));

            // elimination with full pivoting
            for (i = 0; i < numRows; i++)
            {
                // search the whole matrix except for pivoted rows for the maximum absolute value
                max = 0.0f;
                r = c = 0;
                for (j = 0; j < numRows; j++)
                    if (!pivot[j])
                        for (k = 0; k < numRows; k++)
                            if (!pivot[k])
                            {
                                d = MathX.Fabs(this[j][k]);
                                if (d > max)
                                {
                                    max = d;
                                    r = j;
                                    c = k;
                                }
                            }

                if (max == 0.0f)
                    // matrix is not invertible
                    return false;

                pivot[c] = true;

                // swap rows such that entry (c,c) has the pivot entry
                if (r != c)
                    SwapRows(r, c);

                // keep track of the row permutation
                rowIndex[i] = r;
                columnIndex[i] = c;

                // scale the row to make the pivot entry equal to 1
                d = 1.0f / this[c][c];
                this[c][c] = 1.0f;
                for (k = 0; k < numRows; k++)
                    this[c][k] *= d;

                // zero out the pivot column entries in the other rows
                for (j = 0; j < numRows; j++)
                    if (j != c)
                    {
                        d = this[j][c];
                        this[j][c] = 0.0f;
                        for (k = 0; k < numRows; k++)
                            this[j][k] -= this[c][k] * d;
                    }
            }

            // reorder rows to store the inverse of the original matrix
            for (j = numRows - 1; j >= 0; j--)
                if (rowIndex[j] != columnIndex[j])
                    for (k = 0; k < numRows; k++)
                    {
                        d = this[k][rowIndex[j]];
                        this[k][rowIndex[j]] = this[k][columnIndex[j]];
                        this[k][columnIndex[j]] = d;
                    }

            return true;
        }

        /// <summary>
        /// Updates the in-place inverse using the Sherman-Morrison formula to obtain the inverse for the matrix: A + alpha * v * w'
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns></returns>
        public bool Inverse_UpdateRankOne(VectorX v, VectorX w, float alpha)
        {
            int i, j; float beta, s; VectorX y = new(), z = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);

            y.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            z.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            Multiply(y, v);
            TransposeMultiply(z, w);
            beta = 1.0f + (w * y);

            if (beta == 0.0f)
                return false;

            alpha /= beta;

            for (i = 0; i < numRows; i++)
            {
                s = y[i] * alpha;
                for (j = 0; j < numColumns; j++)
                    this[i][j] -= s * z[j];
            }
            return true;
        }

        /// <summary>
        ///  Updates the in-place inverse to obtain the inverse for the matrix:
        ///
        ///	     [ 0  a  0 ]
        ///  A + [ d  b  e ]
        ///	     [ 0  c  0 ]
        ///
        ///  where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1], d = w[0,r-1], w[r] = 0.0f, e = w[r+1,numColumns-1]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public bool Inverse_UpdateRowColumn(VectorX v, VectorX w, int r)
        {
            VectorX s = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows && r < numColumns);
            Debug.Assert(w[r] == 0.0f);

            s.SetData(Math.Max(numRows, numColumns), VectorX.VECX_ALLOCA(Math.Max(numRows, numColumns)));
            s.Zero();
            s[r] = 1.0f;

            if (!Inverse_UpdateRankOne(v, s, 1.0f))
                return false;
            if (!Inverse_UpdateRankOne(s, w, 1.0f))
                return false;
            return true;
        }

        /// <summary>
        ///  Updates the in-place inverse to obtain the inverse for the matrix:
        ///
        ///  [ A  a ]
        ///  [ c  b ]
        ///
        ///  where: a = v[0,numRows-1], b = v[numRows], c = w[0,numColumns-1], w[numColumns] = 0
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <returns></returns>
        public bool Inverse_UpdateIncrement(VectorX v, VectorX w)
        {
            VectorX v2 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);
            Debug.Assert(w.Size >= numColumns + 1);

            ChangeSize(numRows + 1, numColumns + 1, true);
            this[numRows - 1][numRows - 1] = 1.0f;

            v2.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            v2 = v;
            v2[numRows - 1] -= 1.0f;

            return Inverse_UpdateRowColumn(v2, w, numRows - 1);
        }
        /// <summary>
        /// Updates the in-place inverse to obtain the inverse of the matrix with row r and column r removed.
        /// v and w should store the column and row of the original matrix respectively.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public bool Inverse_UpdateDecrement(VectorX v, VectorX w, int r)
        {
            VectorX v1 = new(), w1 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(w.Size >= numColumns);
            Debug.Assert(r >= 0 && r < numRows && r < numColumns);

            v1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            w1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            // update the row and column to identity
            v1 = -v;
            w1 = -w;
            v1[r] += 1.0f;
            w1[r] = 0.0f;

            if (!Inverse_UpdateRowColumn(v1, w1, r))
                return false;

            // physically remove the row and column
            Update_Decrement(r);

            return true;
        }

        /// <summary>
        /// Solve Ax = b with A inverted
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public void Inverse_Solve(VectorX x, VectorX b)
            => Multiply(x, b);

        #endregion

        #region LU

        /// <summary>
        /// in-place factorization: LU
        /// L is a triangular matrix stored in the lower triangle.
        /// L has ones on the diagonal that are not stored.
        /// U is a triangular matrix stored in the upper triangle.
        /// If index != null partial pivoting is used for numerical stability.
        /// If index != null it must point to an array of numRow integers and is used to keep track of the row permutation.
        /// If det != null the determinant of the matrix is calculated and stored.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="det">The det.</param>
        /// <returns></returns>
        public bool LU_Factor(int[] index, out float det)      // factor in-place: L * U
        {
            int i, j, k, newi, min; float s, t, d, w;

            // if partial pivoting should be used
            if (index != null)
                for (i = 0; i < numRows; i++)
                    index[i] = i;

            w = 1.0f;
            min = Math.Min(numRows, numColumns);
            for (i = 0; i < min; i++)
            {
                newi = i;
                s = MathX.Fabs(this[i][i]);

                if (index != null)
                    // find the largest absolute pivot
                    for (j = i + 1; j < numRows; j++)
                    {
                        t = MathX.Fabs(this[j][i]);
                        if (t > s)
                        {
                            newi = j;
                            s = t;
                        }
                    }

                if (s == 0.0f)
                {
                    det = 0f;
                    return false;
                }

                if (newi != i)
                {
                    w = -w;

                    // swap index elements
                    k = index[i];
                    index[i] = index[newi];
                    index[newi] = k;

                    // swap rows
                    for (j = 0; j < numColumns; j++)
                    {
                        t = this[newi][j];
                        this[newi][j] = this[i][j];
                        this[i][j] = t;
                    }
                }

                if (i < numRows)
                {
                    d = 1.0f / this[i][i];
                    for (j = i + 1; j < numRows; j++)
                        this[j][i] *= d;
                }

                if (i < min - 1)
                    for (j = i + 1; j < numRows; j++)
                    {
                        d = this[j][i];
                        for (k = i + 1; k < numColumns; k++)
                            this[j][k] -= d * this[i][k];
                    }
            }

            for (i = 0; i < numRows; i++)
                w *= this[i][i];
            det = w;

            return true;
        }

        /// <summary>
        /// Updates the in-place LU factorization to obtain the factors for the matrix: LU + alpha * v * w'
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public unsafe bool LU_UpdateRankOne(VectorX v, VectorX w, float alpha, int[] index)
        {
            int i, j, max; float diag, beta, p0, p1, d;
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);

            var y = stackalloc float[v.Size];
            var z = stackalloc float[w.Size];

            if (index != null)
                for (i = 0; i < numRows; i++)
                    y[i] = alpha * v[index[i]];
            else
                for (i = 0; i < numRows; i++)
                    y[i] = alpha * v[i];
            fixed (float* w_ = w.p)
                U.memcpy(z, w_ + w.pi, w.Size * sizeof(float));

            max = Math.Min(numRows, numColumns);
            for (i = 0; i < max; i++)
            {
                diag = this[i][i];

                p0 = y[i];
                p1 = z[i];
                diag += p0 * p1;

                if (diag == 0.0f)
                    return false;

                beta = p1 / diag;

                this[i][i] = diag;

                for (j = i + 1; j < numColumns; j++)
                {
                    d = this[i][j];

                    d += p0 * z[j];
                    z[j] -= beta * d;

                    this[i][j] = d;
                }

                for (j = i + 1; j < numRows; j++)
                {
                    d = this[j][i];

                    y[j] -= p0 * d;
                    d += beta * y[j];

                    this[j][i] = d;
                }
            }
            return true;
        }

        /// <summary>
        // Updates the in-place LU factorization to obtain the factors for the matrix:
        // 
        //      [ 0  a  0 ]
        // LU + [ d  b  e ]
        //      [ 0  c  0 ]
        // 
        // where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1], d = w[0,r-1], w[r] = 0.0f, e = w[r+1,numColumns-1]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="r">The r.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public unsafe bool LU_UpdateRowColumn(VectorX v, VectorX w, int r, int[] index)
        {
            int i, j, min, max, rp; float diag, beta0, beta1, p0, p1, q0, q1, d;
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);
            Debug.Assert(r >= 0 && r < numColumns && r < numRows);
            Debug.Assert(w[r] == 0.0f);

            var y0 = stackalloc float[v.Size];
            var z0 = stackalloc float[w.Size];
            var y1 = stackalloc float[v.Size];
            var z1 = stackalloc float[w.Size];

            if (index != null)
            {
                for (i = 0; i < numRows; i++)
                    y0[i] = v[index[i]];
                rp = r;
                for (i = 0; i < numRows; i++)
                    if (index[i] == r)
                    {
                        rp = i;
                        break;
                    }
            }
            else
            {
                fixed (float* v_ = v.p)
                    U.memcpy(y0, v_ + v.pi, v.Size * sizeof(float));
                rp = r;
            }

            U.memset(y1, 0, v.Size * sizeof(float));
            y1[rp] = 1.0f;

            U.memset(z0, 0, w.Size * sizeof(float));
            z0[r] = 1.0f;

            fixed (float* w_ = w.p)
                U.memcpy(z1, w_ + w.pi, w.Size * sizeof(float));

            // update the beginning of the to be updated row and column
            min = Math.Min(r, rp);
            for (i = 0; i < min; i++)
            {
                p0 = y0[i];
                beta1 = z1[i] / this[i][i];

                this[i][r] += p0;
                for (j = i + 1; j < numColumns; j++)
                    z1[j] -= beta1 * this[i][j];
                for (j = i + 1; j < numRows; j++)
                    y0[j] -= p0 * this[j][i];
                this[rp][i] += beta1;
            }

            // update the lower right corner starting at r,r
            max = Math.Min(numRows, numColumns);
            for (i = min; i < max; i++)
            {
                diag = this[i][i];

                p0 = y0[i];
                p1 = z0[i];
                diag += p0 * p1;

                if (diag == 0.0f)
                    return false;

                beta0 = p1 / diag;

                q0 = y1[i];
                q1 = z1[i];
                diag += q0 * q1;

                if (diag == 0.0f)
                    return false;

                beta1 = q1 / diag;

                this[i][i] = diag;

                for (j = i + 1; j < numColumns; j++)
                {
                    d = this[i][j];

                    d += p0 * z0[j];
                    z0[j] -= beta0 * d;

                    d += q0 * z1[j];
                    z1[j] -= beta1 * d;

                    this[i][j] = d;
                }

                for (j = i + 1; j < numRows; j++)
                {

                    d = this[j][i];

                    y0[j] -= p0 * d;
                    d += beta0 * y0[j];

                    y1[j] -= q0 * d;
                    d += beta1 * y1[j];

                    this[j][i] = d;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the in-place LU factorization to obtain the factors for the matrix:
        /// 
        /// [ A  a ]
        /// [ c  b ]
        /// 
        /// where: a = v[0,numRows-1], b = v[numRows], c = w[0,numColumns-1], w[numColumns] = 0
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool LU_UpdateIncrement(VectorX v, VectorX w, int[] index)
        {
            int i, j; float sum;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);
            Debug.Assert(w.Size >= numColumns + 1);

            ChangeSize(numRows + 1, numColumns + 1, true);

            // add row to L
            for (i = 0; i < numRows - 1; i++)
            {
                sum = w[i];
                for (j = 0; j < i; j++)
                    sum -= this[numRows - 1][j] * this[j][i];
                this[numRows - 1][i] = sum / this[i][i];
            }

            // add row to the permutation index
            if (index != null)
                index[numRows - 1] = numRows - 1;

            // add column to U
            for (i = 0; i < numRows; i++)
            {
                if (index != null)
                    sum = v[index[i]];
                else
                    sum = v[i];
                for (j = 0; j < i; j++)
                    sum -= this[i][j] * this[j][numRows - 1];
                this[i][numRows - 1] = sum;
            }

            return true;
        }

        /// <summary>
        /// Updates the in-place LU factorization to obtain the factors for the matrix with row r and column r removed.
        /// v and w should store the column and row of the original matrix respectively.
        /// If index != null then u should store row index[r] of the original matrix.If index == null then u = w.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="u">The u.</param>
        /// <param name="r">The r.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public bool LU_UpdateDecrement(VectorX v, VectorX w, VectorX u, int r, int[] index)
        {
            int i, p; VectorX v1 = new(), w1 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows && r < numColumns);

            v1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            w1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            if (index != null)
            {
                // find the pivot row
                for (p = i = 0; i < numRows; i++)
                    if (index[i] == r)
                    {
                        p = i;
                        break;
                    }

                // update the row and column to identity
                v1 = -v;
                w1 = -u;

                if (p != r)
                {
                    {
                        var c = v1[index[r]];
                        v1[index[r]] = v1[index[p]];
                        v1[index[p]] = c;
                    }
                    {
                        var c = index[r];
                        index[r] = index[p];
                        index[p] = c;
                    }
                }

                v1[r] += 1.0f;
                w1[r] = 0.0f;

                if (!LU_UpdateRowColumn(v1, w1, r, index))
                    return false;

                if (p != r)
                {
                    if (MathX.Fabs(u[p]) < 1e-4f) { } // NOTE: an additional row interchange is required for numerical stability

                    // move row index[r] of the original matrix to row index[p] of the original matrix
                    v1.Zero();
                    v1[index[p]] = 1.0f;
                    w1 = u - w;

                    if (!LU_UpdateRankOne(v1, w1, 1.0f, index))
                        return false;
                }

                // remove the row from the permutation index
                for (i = r; i < numRows - 1; i++)
                    index[i] = index[i + 1];
                for (i = 0; i < numRows - 1; i++)
                    if (index[i] > r)
                        index[i]--;
            }
            else
            {
                v1 = -v;
                w1 = -w;
                v1[r] += 1.0f;
                w1[r] = 0.0f;

                if (!LU_UpdateRowColumn(v1, w1, r, index))
                    return false;
            }

            // physically remove the row and column
            Update_Decrement(r);

            return true;
        }

        /// <summary>
        /// Solve Ax = b with A factored in-place as: LU
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public void LU_Solve(VectorX x, VectorX b, int[] index)
        {
            int i, j; float sum;

            Debug.Assert(x.Size == numColumns && b.Size == numRows);

            // solve L
            for (i = 0; i < numRows; i++)
            {
                if (index != null)
                    sum = b[index[i]];
                else
                    sum = b[i];
                for (j = 0; j < i; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum;
            }

            // solve U
            for (i = numRows - 1; i >= 0; i--)
            {
                sum = x[i];
                for (j = i + 1; j < numRows; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum / this[i][i];
            }
        }
        /// <summary>
        /// Calculates the inverse of the matrix which is factored in-place as LU
        /// </summary>
        /// <param name="inv">The inv.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public void LU_Inverse(MatrixX inv, int[] index)
        {
            int i, j; VectorX x = new(), b = new();
            Debug.Assert(numRows == numColumns);

            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();
            inv.SetSize(numRows, numColumns);

            for (i = 0; i < numRows; i++)
            {
                b[i] = 1.0f;
                LU_Solve(x, b, index);
                for (j = 0; j < numRows; j++)
                    inv[j][i] = x[j];
                b[i] = 0.0f;
            }
        }

        /// <summary>
        /// Unpacks the in-place LU factorization.
        /// </summary>
        /// <param name="L">The l.</param>
        /// <param name="U">The u.</param>
        /// <returns></returns>
        public void LU_UnpackFactors(MatrixX L, MatrixX U)
        {
            int i, j;

            L.Zero(numRows, numColumns);
            U.Zero(numRows, numColumns);
            for (i = 0; i < numRows; i++)
            {
                for (j = 0; j < i; j++)
                    L[i][j] = this[i][j];
                L[i][i] = 1.0f;
                for (j = i; j < numColumns; j++)
                    U[i][j] = this[i][j];
            }
        }

        /// <summary>
        ///  Multiplies the factors of the in-place LU factorization to form the original matrix.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public void LU_MultiplyFactors(MatrixX m, int[] index)
        {
            int r, rp, i, j; float sum;

            m.SetSize(numRows, numColumns);

            for (r = 0; r < numRows; r++)
            {
                if (index != null)
                    rp = index[r];
                else
                    rp = r;

                // calculate row of matrix
                for (i = 0; i < numColumns; i++)
                {
                    if (i >= r)
                        sum = this[r][i];
                    else
                        sum = 0.0f;
                    for (j = 0; j <= i && j < r; j++)
                        sum += this[r][j] * this[j][i];
                    m[rp][i] = sum;
                }
            }
        }

        #endregion

        #region QR

        /// <summary>
        /// in-place factorization: QR
        /// Q is an orthogonal matrix represented as a product of Householder matrices stored in the lower triangle and c.
        /// R is a triangular matrix stored in the upper triangle except for the diagonal elements which are stored in d.
        /// The initial matrix has to be square.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public bool QR_Factor(VectorX c, VectorX d)                // factor in-place: Q * R
        {
            int i, j, k; float scale, s, t, sum; bool singular = false;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(c.Size >= numRows && d.Size >= numRows);

            for (k = 0; k < numRows - 1; k++)
            {
                scale = 0.0f;
                for (i = k; i < numRows; i++)
                {
                    s = MathX.Fabs(this[i][k]);
                    if (s > scale)
                        scale = s;
                }
                if (scale == 0.0f)
                {
                    singular = true;
                    c[k] = d[k] = 0.0f;
                }
                else
                {
                    s = 1.0f / scale;
                    for (i = k; i < numRows; i++)
                        this[i][k] *= s;

                    sum = 0.0f;
                    for (i = k; i < numRows; i++)
                    {
                        s = this[i][k];
                        sum += s * s;
                    }

                    s = MathX.Sqrt(sum);
                    if (this[k][k] < 0.0f)
                        s = -s;
                    this[k][k] += s;
                    c[k] = s * this[k][k];
                    d[k] = -scale * s;

                    for (j = k + 1; j < numRows; j++)
                    {
                        sum = 0.0f;
                        for (i = k; i < numRows; i++)
                            sum += this[i][k] * this[i][j];
                        t = sum / c[k];
                        for (i = k; i < numRows; i++)
                            this[i][j] -= t * this[i][k];
                    }
                }
            }
            d[numRows - 1] = this[numRows - 1][numRows - 1];
            if (d[numRows - 1] == 0.0f)
                singular = true;

            return !singular;
        }

        /// <summary>
        /// Performs a Jacobi rotation on the rows i and i+1 of the unpacked QR factors.
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="i">The i.</param>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        private void QR_Rotate(MatrixX R, int i, float a, float b)
        {
            int j; float f, c, s, w, y;

            if (a == 0.0f)
            {
                c = 0.0f;
                s = (b >= 0.0f) ? 1.0f : -1.0f;
            }
            else if (MathX.Fabs(a) > MathX.Fabs(b))
            {
                f = b / a;
                c = MathX.Fabs(1.0f / MathX.Sqrt(1.0f + f * f));
                if (a < 0.0f)
                    c = -c;
                s = f * c;
            }
            else
            {
                f = a / b;
                s = MathX.Fabs(1.0f / MathX.Sqrt(1.0f + f * f));
                if (b < 0.0f)
                    s = -s;
                c = f * s;
            }
            for (j = i; j < numRows; j++)
            {
                y = R[i][j];
                w = R[i + 1][j];
                R[i][j] = c * y - s * w;
                R[i + 1][j] = s * y + c * w;
            }
            for (j = 0; j < numRows; j++)
            {
                y = this[j][i];
                w = this[j][i + 1];
                this[j][i] = c * y - s * w;
                this[j][i + 1] = s * y + c * w;
            }
        }

        /// <summary>
        /// Updates the unpacked QR factorization to obtain the factors for the matrix: QR + alpha * v * w'
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns></returns>
        public bool QR_UpdateRankOne(MatrixX R, VectorX v, VectorX w, float alpha)
        {
            int i, k; float f; VectorX u = new();
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);

            u.SetData(v.Size, VectorX.VECX_ALLOCA(v.Size));
            TransposeMultiply(u, v);
            u *= alpha;

            for (k = v.Size - 1; k > 0; k--)
                if (u[k] != 0.0f)
                    break;
            for (i = k - 1; i >= 0; i--)
            {
                QR_Rotate(R, i, u[i], -u[i + 1]);
                if (u[i] == 0.0f)
                    u[i] = MathX.Fabs(u[i + 1]);
                else if (MathX.Fabs(u[i]) > MathX.Fabs(u[i + 1]))
                {
                    f = u[i + 1] / u[i];
                    u[i] = MathX.Fabs(u[i]) * MathX.Sqrt(1.0f + f * f);
                }
                else
                {
                    f = u[i] / u[i + 1];
                    u[i] = MathX.Fabs(u[i + 1]) * MathX.Sqrt(1.0f + f * f);
                }
            }
            for (i = 0; i < v.Size; i++)
                R[0][i] += u[0] * w[i];
            for (i = 0; i < k; i++)
                QR_Rotate(R, i, -R[i][i], R[i + 1][i]);
            return true;
        }

        /// <summary>
        /// Updates the unpacked QR factorization to obtain the factors for the matrix:
        /// 
        ///      [ 0  a  0 ]
        /// QR + [ d  b  e ]
        ///      [ 0  c  0 ]
        /// 
        /// where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1], d = w[0,r-1], w[r] = 0.0f, e = w[r+1,numColumns-1]
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public bool QR_UpdateRowColumn(MatrixX R, VectorX v, VectorX w, int r)
        {
            VectorX s = new();
            Debug.Assert(v.Size >= numColumns);
            Debug.Assert(w.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows && r < numColumns);
            Debug.Assert(w[r] == 0.0f);

            s.SetData(Math.Max(numRows, numColumns), VectorX.VECX_ALLOCA(Math.Max(numRows, numColumns)));
            s.Zero();
            s[r] = 1.0f;

            if (!QR_UpdateRankOne(R, v, s, 1.0f))
                return false;
            if (!QR_UpdateRankOne(R, s, w, 1.0f))
                return false;
            return true;
        }

        /// <summary>
        /// Updates the unpacked QR factorization to obtain the factors for the matrix:
        /// 
        /// [ A  a ]
        /// [ c  b ]
        /// 
        /// where: a = v[0,numRows-1], b = v[numRows], c = w[0,numColumns-1], w[numColumns] = 0
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <returns></returns>
        public bool QR_UpdateIncrement(MatrixX R, VectorX v, VectorX w)
        {
            VectorX v2 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);
            Debug.Assert(w.Size >= numColumns + 1);

            ChangeSize(numRows + 1, numColumns + 1, true);
            this[numRows - 1][numRows - 1] = 1.0f;

            R.ChangeSize(R.numRows + 1, R.numColumns + 1, true);
            R[R.numRows - 1][R.numRows - 1] = 1.0f;

            v2.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            v2 = v;
            v2[numRows - 1] -= 1.0f;

            return QR_UpdateRowColumn(R, v2, w, numRows - 1);
        }

        /// <summary>
        /// Updates the unpacked QR factorization to obtain the factors for the matrix with row r and column r removed.
        /// v and w should store the column and row of the original matrix respectively.
        /// </summary>
        /// <param name="R">The r.</param>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public bool QR_UpdateDecrement(MatrixX R, VectorX v, VectorX w, int r)
        {
            VectorX v1 = new(), w1 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(w.Size >= numColumns);
            Debug.Assert(r >= 0 && r < numRows && r < numColumns);

            v1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            w1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            // update the row and column to identity
            v1 = -v;
            w1 = -w;
            v1[r] += 1.0f;
            w1[r] = 0.0f;

            if (!QR_UpdateRowColumn(R, v1, w1, r))
                return false;

            // physically remove the row and column
            Update_Decrement(r);
            R.Update_Decrement(r);

            return true;
        }
        /// <summary>
        /// Solve Ax = b with A factored in-place as: QR
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public void QR_Solve(VectorX x, VectorX b, VectorX c, VectorX d)
        {
            int i, j; float sum, t;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(x.Size >= numRows && b.Size >= numRows);
            Debug.Assert(c.Size >= numRows && d.Size >= numRows);

            for (i = 0; i < numRows; i++)
                x[i] = b[i];

            // multiply b with transpose of Q
            for (i = 0; i < numRows - 1; i++)
            {
                sum = 0.0f;
                for (j = i; j < numRows; j++)
                    sum += this[j][i] * x[j];
                t = sum / c[i];
                for (j = i; j < numRows; j++)
                    x[j] -= t * this[j][i];
            }

            // backsubstitution with R
            for (i = numRows - 1; i >= 0; i--)
            {
                sum = x[i];
                for (j = i + 1; j < numRows; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum / d[i];
            }
        }

        /// <summary>
        /// Solve Ax = b with A factored as: QR
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <param name="R">The r.</param>
        /// <returns></returns>
        public void QR_Solve(VectorX x, VectorX b, MatrixX R)
        {
            int i, j; float sum;
            Debug.Assert(numRows == numColumns);

            // multiply b with transpose of Q
            TransposeMultiply(x, b);

            // backsubstitution with R
            for (i = numRows - 1; i >= 0; i--)
            {
                sum = x[i];
                for (j = i + 1; j < numRows; j++)
                    sum -= R[i][j] * x[j];
                x[i] = sum / R[i][i];
            }
        }

        /// <summary>
        /// Calculates the inverse of the matrix which is factored in-place as: QR
        /// </summary>
        /// <param name="inv">The inv.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public void QR_Inverse(MatrixX inv, VectorX c, VectorX d)
        {
            int i, j; VectorX x = new(), b = new();
            Debug.Assert(numRows == numColumns);

            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();
            inv.SetSize(numRows, numColumns);

            for (i = 0; i < numRows; i++)
            {
                b[i] = 1.0f;
                QR_Solve(x, b, c, d);
                for (j = 0; j < numRows; j++)
                    inv[j][i] = x[j];
                b[i] = 0.0f;
            }
        }

        /// <summary>
        /// Unpacks the in-place QR factorization.
        /// </summary>
        /// <param name="Q">The q.</param>
        /// <param name="R">The r.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public void QR_UnpackFactors(MatrixX Q, MatrixX R, VectorX c, VectorX d)
        {
            int i, j, k; float sum;

            Q.Identity(numRows, numColumns);
            for (i = 0; i < numColumns - 1; i++)
            {
                if (c[i] == 0.0f)
                    continue;
                for (j = 0; j < numRows; j++)
                {
                    sum = 0.0f;
                    for (k = i; k < numColumns; k++)
                        sum += this[k][i] * Q[j][k];
                    sum /= c[i];
                    for (k = i; k < numColumns; k++)
                        Q[j][k] -= sum * this[k][i];
                }
            }

            R.Zero(numRows, numColumns);
            for (i = 0; i < numRows; i++)
            {
                R[i][i] = d[i];
                for (j = i + 1; j < numColumns; j++)
                    R[i][j] = this[i][j];
            }
        }

        /// <summary>
        /// Multiplies the factors of the in-place QR factorization to form the original matrix.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="c">The c.</param>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        public void QR_MultiplyFactors(MatrixX m, VectorX c, VectorX d)
        {
            int i, j, k; float sum; MatrixX Q = new();

            Q.Identity(numRows, numColumns);
            for (i = 0; i < numColumns - 1; i++)
            {
                if (c[i] == 0.0f)
                    continue;
                for (j = 0; j < numRows; j++)
                {
                    sum = 0.0f;
                    for (k = i; k < numColumns; k++)
                        sum += this[k][i] * Q[j][k];
                    sum /= c[i];
                    for (k = i; k < numColumns; k++)
                        Q[j][k] -= sum * this[k][i];
                }
            }

            for (i = 0; i < numRows; i++)
                for (j = 0; j < numColumns; j++)
                {
                    sum = Q[i][j] * d[i];
                    for (k = 0; k < i; k++)
                        sum += Q[i][k] * this[j][k];
                    m[i][j] = sum;
                }
        }

        #endregion

        #region SVD

        void SVD_BiDiag(VectorX w, VectorX rv1, float anorm)
        {
            int i, j, k, l; float f, h, r, g, s, scale;

            anorm = 0.0f;
            g = s = scale = 0.0f;
            for (i = 0; i < numColumns; i++)
            {
                l = i + 1;
                rv1[i] = scale * g;
                g = s = scale = 0.0f;
                if (i < numRows)
                {
                    for (k = i; k < numRows; k++)
                        scale += MathX.Fabs(this[k][i]);
                    if (scale != 0f)
                    {
                        for (k = i; k < numRows; k++)
                        {
                            this[k][i] /= scale;
                            s += this[k][i] * this[k][i];
                        }
                        f = this[i][i];
                        g = MathX.Sqrt(s);
                        if (f >= 0.0f)
                            g = -g;
                        h = f * g - s;
                        this[i][i] = f - g;
                        if (i != (numColumns - 1))
                            for (j = l; j < numColumns; j++)
                            {
                                for (s = 0.0f, k = i; k < numRows; k++)
                                    s += this[k][i] * this[k][j];
                                f = s / h;
                                for (k = i; k < numRows; k++)
                                    this[k][j] += f * this[k][i];
                            }
                        for (k = i; k < numRows; k++)
                            this[k][i] *= scale;
                    }
                }
                w[i] = scale * g;
                g = s = scale = 0.0f;
                if (i < numRows && i != (numColumns - 1))
                {
                    for (k = l; k < numColumns; k++)
                        scale += MathX.Fabs(this[i][k]);
                    if (scale != 0f)
                    {
                        for (k = l; k < numColumns; k++)
                        {
                            this[i][k] /= scale;
                            s += this[i][k] * this[i][k];
                        }
                        f = this[i][l];
                        g = MathX.Sqrt(s);
                        if (f >= 0.0f)
                            g = -g;
                        h = 1.0f / (f * g - s);
                        this[i][l] = f - g;
                        for (k = l; k < numColumns; k++)
                            rv1[k] = this[i][k] * h;
                        if (i != (numRows - 1))
                            for (j = l; j < numRows; j++)
                            {
                                for (s = 0.0f, k = l; k < numColumns; k++)
                                    s += this[j][k] * this[i][k];
                                for (k = l; k < numColumns; k++)
                                    this[j][k] += s * rv1[k];
                            }
                        for (k = l; k < numColumns; k++)
                            this[i][k] *= scale;
                    }
                }
                r = MathX.Fabs(w[i]) + MathX.Fabs(rv1[i]);
                if (r > anorm)
                    anorm = r;
            }
        }

        void SVD_InitialWV(VectorX w, MatrixX V, VectorX rv1)
        {
            int i, j, k, l; float f, g, s;

            g = 0.0f;
            for (i = (numColumns - 1); i >= 0; i--)
            {
                l = i + 1;
                if (i < (numColumns - 1))
                {
                    if (g != 0f)
                    {
                        for (j = l; j < numColumns; j++)
                            V[j][i] = (this[i][j] / this[i][l]) / g;
                        // double division to reduce underflow
                        for (j = l; j < numColumns; j++)
                        {
                            for (s = 0.0f, k = l; k < numColumns; k++)
                                s += this[i][k] * V[k][j];
                            for (k = l; k < numColumns; k++)
                                V[k][j] += s * V[k][i];
                        }
                    }
                    for (j = l; j < numColumns; j++)
                        V[i][j] = V[j][i] = 0.0f;
                }
                V[i][i] = 1.0f;
                g = rv1[i];
            }
            for (i = numColumns - 1; i >= 0; i--)
            {
                l = i + 1;
                g = w[i];
                if (i < (numColumns - 1))
                    for (j = l; j < numColumns; j++)
                        this[i][j] = 0.0f;
                if (g != 0f)
                {
                    g = 1.0f / g;
                    if (i != (numColumns - 1))
                        for (j = l; j < numColumns; j++)
                        {
                            for (s = 0.0f, k = l; k < numRows; k++)
                                s += this[k][i] * this[k][j];
                            f = (s / this[i][i]) * g;
                            for (k = i; k < numRows; k++)
                                this[k][j] += f * this[k][i];
                        }
                    for (j = i; j < numRows; j++)
                        this[j][i] *= g;
                }
                else
                    for (j = i; j < numRows; j++)
                        this[j][i] = 0.0f;
                this[i][i] += 1.0f;
            }
        }

        /// <summary>
        /// in-place factorization: U* Diag(w) * V.Transpose()
        /// known as the Singular Value Decomposition.
        /// U is a column-orthogonal matrix which overwrites the original matrix.
        /// w is a diagonal matrix with all elements >= 0 which are the singular values.
        /// V is the transpose of an orthogonal matrix.
        /// </summary>
        /// <param name="w">The w.</param>
        /// <param name="V">The v.</param>
        /// <returns></returns>
        public bool SVD_Factor(VectorX w, MatrixX V)               // factor in-place: U * Diag(w) * V.Transpose()
        {
            int flag, i, its, j, jj, k, l, nm; float c, f, h, s, x, y, z, r, g = 0.0f; float anorm = 0.0f; VectorX rv1 = new();

            if (numRows < numColumns)
                return false;

            rv1.SetData(numColumns, VectorX.VECX_ALLOCA(numColumns));
            rv1.Zero();
            w.Zero(numColumns);
            V.Zero(numColumns, numColumns);

            SVD_BiDiag(w, rv1, anorm);
            SVD_InitialWV(w, V, rv1);

            for (k = numColumns - 1; k >= 0; k--)
                for (its = 1; its <= 30; its++)
                {
                    flag = 1;
                    nm = 0;
                    for (l = k; l >= 0; l--)
                    {
                        nm = l - 1;
                        if ((MathX.Fabs(rv1[l]) + anorm) == anorm) //: MathX.Fabs( rv1[l] ) < MathX.FLT_EPSILON
                        {
                            flag = 0;
                            break;
                        }
                        if ((MathX.Fabs(w[nm]) + anorm) == anorm) //: MathX.Fabs( w[nm] ) < MathX.FLT_EPSILON
                            break;
                    }
                    if (flag != 0f)
                    {
                        c = 0.0f;
                        s = 1.0f;
                        for (i = l; i <= k; i++)
                        {
                            f = s * rv1[i];

                            if ((MathX.Fabs(f) + anorm) != anorm) //: MathX.Fabs( f ) > MathX.FLT_EPSILON
                            {
                                g = w[i];
                                h = Pythag(f, g);
                                w[i] = h;
                                h = 1.0f / h;
                                c = g * h;
                                s = -f * h;
                                for (j = 0; j < numRows; j++)
                                {
                                    y = this[j][nm];
                                    z = this[j][i];
                                    this[j][nm] = y * c + z * s;
                                    this[j][i] = z * c - y * s;
                                }
                            }
                        }
                    }
                    z = w[k];
                    if (l == k)
                    {
                        if (z < 0.0f)
                        {
                            w[k] = -z;
                            for (j = 0; j < numColumns; j++)
                                V[j][k] = -V[j][k];
                        }
                        break;
                    }
                    if (its == 30)
                        return false;       // no convergence
                    x = w[l];
                    nm = k - 1;
                    y = w[nm];
                    g = rv1[nm];
                    h = rv1[k];
                    f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0f * h * y);
                    g = Pythag(f, 1.0f);
                    r = f >= 0.0f ? g : -g;
                    f = ((x - z) * (x + z) + h * ((y / (f + r)) - h)) / x;
                    c = s = 1.0f;
                    for (j = l; j <= nm; j++)
                    {
                        i = j + 1;
                        g = rv1[i];
                        y = w[i];
                        h = s * g;
                        g = c * g;
                        z = Pythag(f, h);
                        rv1[j] = z;
                        c = f / z;
                        s = h / z;
                        f = x * c + g * s;
                        g = g * c - x * s;
                        h = y * s;
                        y *= c;
                        for (jj = 0; jj < numColumns; jj++)
                        {
                            x = V[jj][j];
                            z = V[jj][i];
                            V[jj][j] = x * c + z * s;
                            V[jj][i] = z * c - x * s;
                        }
                        z = Pythag(f, h);
                        w[j] = z;
                        if (z != 0f)
                        {
                            z = 1.0f / z;
                            c = f * z;
                            s = h * z;
                        }
                        f = (c * g) + (s * y);
                        x = (c * y) - (s * g);
                        for (jj = 0; jj < numRows; jj++)
                        {
                            y = this[jj][j];
                            z = this[jj][i];
                            this[jj][j] = y * c + z * s;
                            this[jj][i] = z * c - y * s;
                        }
                    }
                    rv1[l] = 0.0f;
                    rv1[k] = f;
                    w[k] = x;
                }
            return true;
        }

        /// <summary>
        /// Solve Ax = b with A factored as: U * Diag(w) * V.Transpose()
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <param name="w">The w.</param>
        /// <param name="V">The v.</param>
        /// <returns></returns>
        public void SVD_Solve(VectorX x, VectorX b, VectorX w, MatrixX V)
        {
            int i, j; float sum; VectorX tmp = new();
            Debug.Assert(x.Size >= numColumns);
            Debug.Assert(b.Size >= numColumns);
            Debug.Assert(w.Size == numColumns);
            Debug.Assert(V.NumRows == numColumns && V.NumColumns == numColumns);

            tmp.SetData(numColumns, VectorX.VECX_ALLOCA(numColumns));

            for (i = 0; i < numColumns; i++)
            {
                sum = 0.0f;
                if (w[i] >= MathX.FLT_EPSILON)
                {
                    for (j = 0; j < numRows; j++)
                        sum += this[j][i] * b[j];
                    sum /= w[i];
                }
                tmp[i] = sum;
            }
            for (i = 0; i < numColumns; i++)
            {
                sum = 0.0f;
                for (j = 0; j < numColumns; j++)
                    sum += V[i][j] * tmp[j];
                x[i] = sum;
            }
        }

        /// <summary>
        /// Calculates the inverse of the matrix which is factored in-place as: U * Diag(w) * V.Transpose()
        /// </summary>
        /// <param name="inv">The inv.</param>
        /// <param name="w">The w.</param>
        /// <param name="V">The v.</param>
        /// <returns></returns>
        public void SVD_Inverse(MatrixX inv, VectorX w, MatrixX V)
        {
            int i, j, k; float wi, sum; MatrixX V2 = new();
            Debug.Assert(numRows == numColumns);

            V2 = V;

            // V * [diag(1/w[i])]
            for (i = 0; i < numRows; i++)
            {
                wi = w[i];
                wi = (wi < MathX.FLT_EPSILON) ? 0.0f : 1.0f / wi;
                for (j = 0; j < numColumns; j++)
                    V2[j][i] *= wi;
            }

            // V * [diag(1/w[i])] * Ut
            for (i = 0; i < numRows; i++)
                for (j = 0; j < numColumns; j++)
                {
                    sum = V2[i][0] * this[j][0];
                    for (k = 1; k < numColumns; k++)
                        sum += V2[i][k] * this[j][k];
                    inv[i][j] = sum;
                }
        }

        /// <summary>
        /// Multiplies the factors of the in-place SVD factorization to form the original matrix.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="w">The w.</param>
        /// <param name="V">The v.</param>
        /// <returns></returns>
        public void SVD_MultiplyFactors(MatrixX m, VectorX w, MatrixX V)
        {
            int r, i, j; float sum;

            m.SetSize(numRows, V.NumRows);

            for (r = 0; r < numRows; r++)
            {
                // calculate row of matrix
                if (w[r] >= MathX.FLT_EPSILON)
                    for (i = 0; i < V.NumRows; i++)
                    {
                        sum = 0.0f;
                        for (j = 0; j < numColumns; j++)
                            sum += this[r][j] * V[i][j];
                        m[r][i] = sum * w[r];
                    }
                else
                    for (i = 0; i < V.NumRows; i++)
                        m[r][i] = 0.0f;
            }
        }

        #endregion

        #region Cholesky

        /// <summary>
        /// in-place Cholesky factorization: LL'
        /// L is a triangular matrix stored in the lower triangle.
        /// The upper triangle is not cleared.
        /// The initial matrix has to be symmetric positive definite.
        /// </summary>
        /// <returns></returns>
        public unsafe bool Cholesky_Factor()                        // factor in-place: L * L.Transpose()
        {
            int i, j, k; float sum;
            Debug.Assert(numRows == numColumns);

            var invSqrt = stackalloc float[numRows];

            for (i = 0; i < numRows; i++)
            {
                for (j = 0; j < i; j++)
                {
                    sum = this[i][j];
                    for (k = 0; k < j; k++)
                        sum -= this[i][k] * this[j][k];
                    this[i][j] = sum * invSqrt[j];
                }

                sum = this[i][i];
                for (k = 0; k < i; k++)
                    sum -= this[i][k] * this[i][k];

                if (sum <= 0.0f)
                    return false;

                invSqrt[i] = MathX.InvSqrt(sum);
                this[i][i] = invSqrt[i] * sum;
            }
            return true;
        }

        /// <summary>
        /// Updates the in-place Cholesky factorization to obtain the factors for the matrix: LL' + alpha * v * v'
        /// If offset > 0 only the lower right corner starting at(offset, offset) is updated.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public unsafe bool Cholesky_UpdateRankOne(VectorX v, float alpha, int offset = 0)
        {
            int i, j; float diag, invDiag, diagSqr, newDiag, newDiagSqr, beta, p, d;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(offset >= 0 && offset < numRows);

            var y = stackalloc float[v.Size];
            fixed (float* v_ = v.p)
                U.memcpy(y, v_ + v.pi, v.Size * sizeof(float));

            for (i = offset; i < numColumns; i++)
            {
                p = y[i];
                diag = this[i][i];
                invDiag = 1.0f / diag;
                diagSqr = diag * diag;
                newDiagSqr = diagSqr + alpha * p * p;

                if (newDiagSqr <= 0.0f)
                    return false;

                this[i][i] = newDiag = MathX.Sqrt(newDiagSqr);

                alpha /= newDiagSqr;
                beta = p * alpha;
                alpha *= diagSqr;

                for (j = i + 1; j < numRows; j++)
                {
                    d = this[j][i] * invDiag;

                    y[j] -= p * d;
                    d += beta * y[j];

                    this[j][i] = d * newDiag;
                }
            }
            return true;
        }

        /// <summary>
        /// Updates the in-place Cholesky factorization to obtain the factors for the matrix:
        /// 
        ///	      [ 0  a  0 ]
        /// LL' + [ a  b  c ]
        ///	      [ 0  c  0 ]
        /// 
        /// where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public unsafe bool Cholesky_UpdateRowColumn(VectorX v, int r)
        {
            int i, j; float sum; VectorX addSub = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows);

            addSub.SetData(numColumns, new float[numColumns]);

            if (r == 0)
            {
                if (numColumns == 1)
                {
                    var v0 = v[0];
                    sum = this[0][0];
                    sum *= sum;
                    sum += v0;
                    if (sum <= 0.0f)
                        return false;
                    this[0][0] = MathX.Sqrt(sum);
                    return true;
                }
                for (i = 0; i < numColumns; i++)
                    addSub[i] = v[i];
            }
            else
            {
                var original = stackalloc float[numColumns];

                // calculate original row/column of matrix
                for (i = 0; i < numRows; i++)
                {
                    sum = 0.0f;
                    for (j = 0; j <= i; j++)
                        sum += this[r][j] * this[i][j];
                    original[i] = sum;
                }

                // solve for y in L * y = original + v
                for (i = 0; i < r; i++)
                {
                    sum = original[i] + v[i];
                    for (j = 0; j < i; j++)
                        sum -= this[r][j] * this[i][j];
                    this[r][i] = sum / this[i][i];
                }

                // if the last row/column of the matrix is updated
                if (r == numColumns - 1)
                {
                    // only calculate new diagonal
                    sum = original[r] + v[r];
                    for (j = 0; j < r; j++)
                        sum -= this[r][j] * this[r][j];
                    if (sum <= 0.0f)
                        return false;
                    this[r][r] = MathX.Sqrt(sum);
                    return true;
                }

                // calculate the row/column to be added to the lower right sub matrix starting at (r, r)
                for (i = r; i < numColumns; i++)
                {
                    sum = 0.0f;
                    for (j = 0; j <= r; j++)
                        sum += this[r][j] * this[i][j];
                    addSub[i] = v[i] - (sum - original[i]);
                }
            }

            // add row/column to the lower right sub matrix starting at (r, r)

            float diag, invDiag, diagSqr, newDiag, newDiagSqr;
            float alpha1, alpha2, beta1, beta2, p1, p2, d;

            var v1 = stackalloc float[numColumns];
            var v2 = stackalloc float[numColumns];

            d = MathX.SQRT_1OVER2;
            v1[r] = (0.5f * addSub[r] + 1.0f) * d;
            v2[r] = (0.5f * addSub[r] - 1.0f) * d;
            for (i = r + 1; i < numColumns; i++)
                v1[i] = v2[i] = addSub[i] * d;

            alpha1 = 1.0f;
            alpha2 = -1.0f;

            // simultaneous update/downdate of the sub matrix starting at (r, r)
            for (i = r; i < numColumns; i++)
            {
                p1 = v1[i];
                diag = this[i][i];
                invDiag = 1.0f / diag;
                diagSqr = diag * diag;
                newDiagSqr = diagSqr + alpha1 * p1 * p1;

                if (newDiagSqr <= 0.0f)
                    return false;

                alpha1 /= newDiagSqr;
                beta1 = p1 * alpha1;
                alpha1 *= diagSqr;

                p2 = v2[i];
                diagSqr = newDiagSqr;
                newDiagSqr = diagSqr + alpha2 * p2 * p2;

                if (newDiagSqr <= 0.0f)
                    return false;

                this[i][i] = newDiag = MathX.Sqrt(newDiagSqr);

                alpha2 /= newDiagSqr;
                beta2 = p2 * alpha2;
                alpha2 *= diagSqr;

                for (j = i + 1; j < numRows; j++)
                {
                    d = this[j][i] * invDiag;

                    v1[j] -= p1 * d;
                    d += beta1 * v1[j];

                    v2[j] -= p2 * d;
                    d += beta2 * v2[j];

                    this[j][i] = d * newDiag;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the in-place Cholesky factorization to obtain the factors for the matrix:
        /// 
        /// [ A  a ]
        /// [ a  b ]
        /// 
        /// where: a = v[0,numRows-1], b = v[numRows]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public unsafe bool Cholesky_UpdateIncrement(VectorX v)
        {
            int i, j; float sum;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);

            ChangeSize(numRows + 1, numColumns + 1, false);

            var x = stackalloc float[numRows];

            // solve for x in L * x = v
            for (i = 0; i < numRows - 1; i++)
            {
                sum = v[i];
                for (j = 0; j < i; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum / this[i][i];
            }

            // calculate new row of L and calculate the square of the diagonal entry
            sum = v[numRows - 1];
            for (i = 0; i < numRows - 1; i++)
            {
                this[numRows - 1][i] = x[i];
                sum -= x[i] * x[i];
            }

            if (sum <= 0.0f)
                return false;

            // store the diagonal entry
            this[numRows - 1][numRows - 1] = MathX.Sqrt(sum);

            return true;
        }

        /// <summary>
        /// Updates the in-place Cholesky factorization to obtain the factors for the matrix with row r and column r removed.
        /// v should store the row of the original matrix.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public bool Cholesky_UpdateDecrement(VectorX v, int r)
        {
            VectorX v1 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows);

            v1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            // update the row and column to identity
            v1 = -v;
            v1[r] += 1.0f;

            // NOTE:	msvc compiler bug: the this pointer stored in edi is expected to stay
            //			untouched when calling Cholesky_UpdateRowColumn in the if statement
            var ret = Cholesky_UpdateRowColumn(v1, r);
            if (!ret)
                return false;

            // physically remove the row and column
            Update_Decrement(r);

            return true;
        }

        /// <summary>
        /// Solve Ax = b with A factored in-place as: LL'
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public void Cholesky_Solve(VectorX x, VectorX b)
        {
            int i, j; float sum;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(x.Size >= numRows && b.Size >= numRows);

            // solve L
            for (i = 0; i < numRows; i++)
            {
                sum = b[i];
                for (j = 0; j < i; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum / this[i][i];
            }

            // solve Lt
            for (i = numRows - 1; i >= 0; i--)
            {
                sum = x[i];
                for (j = i + 1; j < numRows; j++)
                    sum -= this[j][i] * x[j];
                x[i] = sum / this[i][i];
            }
        }

        /// <summary>
        /// Calculates the inverse of the matrix which is factored in-place as: LL'
        /// </summary>
        /// <param name="inv">The inv.</param>
        /// <returns></returns>
        public void Cholesky_Inverse(MatrixX inv)
        {
            int i, j; VectorX x = new(), b = new();
            Debug.Assert(numRows == numColumns);

            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();
            inv.SetSize(numRows, numColumns);

            for (i = 0; i < numRows; i++)
            {
                b[i] = 1.0f;
                Cholesky_Solve(x, b);
                for (j = 0; j < numRows; j++)
                    inv[j][i] = x[j];
                b[i] = 0.0f;
            }
        }

        /// <summary>
        /// Multiplies the factors of the in-place Cholesky factorization to form the original matrix.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        public void Cholesky_MultiplyFactors(MatrixX m)
        {
            int r, i, j; float sum;

            m.SetSize(numRows, numColumns);

            for (r = 0; r < numRows; r++)
            {
                // calculate row of matrix
                for (i = 0; i < numRows; i++)
                {
                    sum = 0.0f;
                    for (j = 0; j <= i && j <= r; j++)
                        sum += this[r][j] * this[i][j];
                    m[r][i] = sum;
                }
            }
        }

        #endregion

        #region LDLT

        /// <summary>
        /// in-place factorization: LDL'
        /// L is a triangular matrix stored in the lower triangle.
        /// L has ones on the diagonal that are not stored.
        /// D is a diagonal matrix stored on the diagonal.
        /// The upper triangle is not cleared.
        /// The initial matrix has to be symmetric.
        /// </summary>
        /// <returns></returns>
        public unsafe bool LDLT_Factor()                            // factor in-place: L * D * L.Transpose()
        {
            int i, j, k; float d, sum;
            Debug.Assert(numRows == numColumns);

            var v = stackalloc float[numRows];

            for (i = 0; i < numRows; i++)
            {
                sum = this[i][i];
                for (j = 0; j < i; j++)
                {
                    d = this[i][j];
                    v[j] = this[j][j] * d;
                    sum -= v[j] * d;
                }

                if (sum == 0.0f)
                    return false;

                this[i][i] = sum;
                d = 1.0f / sum;

                for (j = i + 1; j < numRows; j++)
                {
                    sum = this[j][i];
                    for (k = 0; k < i; k++)
                        sum -= this[j][k] * v[k];
                    this[j][i] = sum * d;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the in-place LDL' factorization to obtain the factors for the matrix: LDL' + alpha * v * v'
        /// If offset > 0 only the lower right corner starting at(offset, offset) is updated.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public unsafe bool LDLT_UpdateRankOne(VectorX v, float alpha, int offset = 0)
        {
            int i, j; float diag, newDiag, beta, p, d;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(offset >= 0 && offset < numRows);

            var y = stackalloc float[v.Size];
            fixed (float* v_ = v.p)
                U.memcpy(y, v_ + v.pi, v.Size * sizeof(float));

            for (i = offset; i < numColumns; i++)
            {
                p = y[i];
                diag = this[i][i];
                this[i][i] = newDiag = diag + alpha * p * p;

                if (newDiag == 0.0f)
                    return false;

                alpha /= newDiag;
                beta = p * alpha;
                alpha *= diag;

                for (j = i + 1; j < numRows; j++)
                {
                    d = this[j][i];

                    y[j] -= p * d;
                    d += beta * y[j];

                    this[j][i] = d;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the in-place LDL' factorization to obtain the factors for the matrix:
        ///
        ///	       [ 0  a  0 ]
        /// LDL' + [ a  b  c ]
        ///	       [ 0  c  0 ]
        ///
        /// where: a = v[0,r-1], b = v[r], c = v[r+1,numRows-1]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public unsafe bool LDLT_UpdateRowColumn(VectorX v, int r)
        {
            int i, j; float sum; VectorX addSub = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows);

            addSub.SetData(numColumns, new float[numColumns]);

            if (r == 0)
            {
                if (numColumns == 1)
                {
                    this[0][0] += v[0];
                    return true;
                }
                for (i = 0; i < numColumns; i++)
                    addSub[i] = v[i];
            }
            else
            {
                var original = stackalloc float[numColumns];
                var y = stackalloc float[numColumns];

                // calculate original row/column of matrix
                for (i = 0; i < r; i++)
                    y[i] = this[r][i] * this[i][i];
                for (i = 0; i < numColumns; i++)
                {
                    if (i < r)
                        sum = this[i][i] * this[r][i];
                    else if (i == r)
                        sum = this[r][r];
                    else
                        sum = this[r][r] * this[i][r];
                    for (j = 0; j < i && j < r; j++)
                        sum += this[i][j] * y[j];
                    original[i] = sum;
                }

                // solve for y in L * y = original + v
                for (i = 0; i < r; i++)
                {
                    sum = original[i] + v[i];
                    for (j = 0; j < i; j++)
                        sum -= this[i][j] * y[j];
                    y[i] = sum;
                }

                // calculate new row of L
                for (i = 0; i < r; i++)
                    this[r][i] = y[i] / this[i][i];

                // if the last row/column of the matrix is updated
                if (r == numColumns - 1)
                {
                    // only calculate new diagonal
                    sum = original[r] + v[r];
                    for (j = 0; j < r; j++)
                        sum -= this[r][j] * y[j];
                    if (sum == 0.0f)
                        return false;
                    this[r][r] = sum;
                    return true;
                }

                // calculate the row/column to be added to the lower right sub matrix starting at (r, r)
                for (i = 0; i < r; i++)
                    y[i] = this[r][i] * this[i][i];
                for (i = r; i < numColumns; i++)
                {
                    if (i == r)
                        sum = this[r][r];
                    else
                        sum = this[r][r] * this[i][r];
                    for (j = 0; j < r; j++)
                        sum += this[i][j] * y[j];
                    addSub[i] = v[i] - (sum - original[i]);
                }
            }

            // add row/column to the lower right sub matrix starting at (r, r)
            float d, diag, newDiag, p1, p2, alpha1, alpha2, beta1, beta2;

            var v1 = stackalloc float[numColumns];
            var v2 = stackalloc float[numColumns];

            d = MathX.SQRT_1OVER2;
            v1[r] = (0.5f * addSub[r] + 1.0f) * d;
            v2[r] = (0.5f * addSub[r] - 1.0f) * d;
            for (i = r + 1; i < numColumns; i++)
                v1[i] = v2[i] = addSub[i] * d;

            alpha1 = 1.0f;
            alpha2 = -1.0f;

            // simultaneous update/downdate of the sub matrix starting at (r, r)
            for (i = r; i < numColumns; i++)
            {
                diag = this[i][i];
                p1 = v1[i];
                newDiag = diag + alpha1 * p1 * p1;

                if (newDiag == 0.0f)
                    return false;

                alpha1 /= newDiag;
                beta1 = p1 * alpha1;
                alpha1 *= diag;

                diag = newDiag;
                p2 = v2[i];
                newDiag = diag + alpha2 * p2 * p2;

                if (newDiag == 0.0f)
                    return false;

                alpha2 /= newDiag;
                beta2 = p2 * alpha2;
                alpha2 *= diag;

                this[i][i] = newDiag;

                for (j = i + 1; j < numRows; j++)
                {
                    d = this[j][i];

                    v1[j] -= p1 * d;
                    d += beta1 * v1[j];

                    v2[j] -= p2 * d;
                    d += beta2 * v2[j];

                    this[j][i] = d;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the in-place LDL' factorization to obtain the factors for the matrix:
        ///
        /// [ A  a ]
        /// [ a  b ]
        ///
        /// where: a = v[0,numRows-1], b = v[numRows]
        /// </summary>
        /// <param name="v">The v.</param>
        /// <returns></returns>
        public unsafe bool LDLT_UpdateIncrement(VectorX v)
        {
            int i, j; float sum, d;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows + 1);

            ChangeSize(numRows + 1, numColumns + 1, false);

            var x = stackalloc float[numRows];

            // solve for x in L * x = v
            for (i = 0; i < numRows - 1; i++)
            {
                sum = v[i];
                for (j = 0; j < i; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum;
            }

            // calculate new row of L and calculate the diagonal entry
            sum = v[numRows - 1];
            for (i = 0; i < numRows - 1; i++)
            {
                this[numRows - 1][i] = d = x[i] / this[i][i];
                sum -= d * x[i];
            }

            if (sum == 0.0f)
                return false;

            // store the diagonal entry
            this[numRows - 1][numRows - 1] = sum;

            return true;
        }

        /// <summary>
        /// Updates the in-place LDL' factorization to obtain the factors for the matrix with row r and column r removed.
        /// v should store the row of the original matrix.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <param name="r">The r.</param>
        /// <returns></returns>
        public bool LDLT_UpdateDecrement(VectorX v, int r)
        {
            VectorX v1 = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(v.Size >= numRows);
            Debug.Assert(r >= 0 && r < numRows);

            v1.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            // update the row and column to identity
            v1 = -v;
            v1[r] += 1.0f;

            // NOTE:	msvc compiler bug: the this pointer stored in edi is expected to stay
            //			untouched when calling LDLT_UpdateRowColumn in the if statement
            var ret = LDLT_UpdateRowColumn(v1, r);
            if (!ret)
                return false;

            // physically remove the row and column
            Update_Decrement(r);

            return true;
        }

        /// <summary>
        /// Solve Ax = b with A factored in-place as: LDL'
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public void LDLT_Solve(VectorX x, VectorX b)
        {
            int i, j; float sum;
            Debug.Assert(numRows == numColumns);
            Debug.Assert(x.Size >= numRows && b.Size >= numRows);

            // solve L
            for (i = 0; i < numRows; i++)
            {
                sum = b[i];
                for (j = 0; j < i; j++)
                    sum -= this[i][j] * x[j];
                x[i] = sum;
            }

            // solve D
            for (i = 0; i < numRows; i++)
                x[i] /= this[i][i];

            // solve Lt
            for (i = numRows - 2; i >= 0; i--)
            {
                sum = x[i];
                for (j = i + 1; j < numRows; j++)
                    sum -= this[j][i] * x[j];
                x[i] = sum;
            }
        }

        /// <summary>
        /// Calculates the inverse of the matrix which is factored in-place as: LDL'
        /// </summary>
        /// <param name="inv">The inv.</param>
        /// <returns></returns>
        public void LDLT_Inverse(MatrixX inv)
        {
            int i, j; VectorX x = new(), b = new();
            Debug.Assert(numRows == numColumns);

            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();
            inv.SetSize(numRows, numColumns);

            for (i = 0; i < numRows; i++)
            {
                b[i] = 1.0f;
                LDLT_Solve(x, b);
                for (j = 0; j < numRows; j++)
                    inv[j][i] = x[j];
                b[i] = 0.0f;
            }
        }

        /// <summary>
        /// Unpacks the in-place LDL' factorization.
        /// </summary>
        /// <param name="L">The l.</param>
        /// <param name="D">The d.</param>
        /// <returns></returns>
        public void LDLT_UnpackFactors(MatrixX L, MatrixX D)
        {
            int i, j;

            L.Zero(numRows, numColumns);
            D.Zero(numRows, numColumns);
            for (i = 0; i < numRows; i++)
            {
                for (j = 0; j < i; j++)
                    L[i][j] = this[i][j];
                L[i][i] = 1.0f;
                D[i][i] = this[i][i];
            }
        }

        /// <summary>
        /// Multiplies the factors of the in-place LDL' factorization to form the original matrix.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        public unsafe void LDLT_MultiplyFactors(MatrixX m)
        {
            int r, i, j; float sum;

            var v = stackalloc float[numRows];
            m.SetSize(numRows, numColumns);

            for (r = 0; r < numRows; r++)
            {
                // calculate row of matrix
                for (i = 0; i < r; i++)
                    v[i] = this[r][i] * this[i][i];
                for (i = 0; i < numColumns; i++)
                {
                    if (i < r)
                        sum = this[i][i] * this[r][i];
                    else if (i == r)
                        sum = this[r][r];
                    else
                        sum = this[r][r] * this[i][r];
                    for (j = 0; j < i && j < r; j++)
                        sum += this[i][j] * v[j];
                    m[r][i] = sum;
                }
            }
        }

        #endregion

        #region TriDiagonal

        public void TriDiagonal_ClearTriangles()
        {
            int i, j;

            Debug.Assert(numRows == numColumns);
            for (i = 0; i < numRows - 2; i++)
                for (j = i + 2; j < numColumns; j++)
                {
                    this[i][j] = 0.0f;
                    this[j][i] = 0.0f;
                }
        }

        /// <summary>
        /// Solve Ax = b with A being tridiagonal.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public bool TriDiagonal_Solve(VectorX x, VectorX b)
        {
            int i; float d; VectorX tmp = new();
            Debug.Assert(numRows == numColumns);
            Debug.Assert(x.Size >= numRows && b.Size >= numRows);

            tmp.SetData(numRows, VectorX.VECX_ALLOCA(numRows));

            d = this[0][0];
            if (d == 0.0f)
                return false;
            d = 1.0f / d;
            x[0] = b[0] * d;
            for (i = 1; i < numRows; i++)
            {
                tmp[i] = this[i - 1][i] * d;
                d = this[i][i] - this[i][i - 1] * tmp[i];
                if (d == 0.0f)
                    return false;
                d = 1.0f / d;
                x[i] = (b[i] - this[i][i - 1] * x[i - 1]) * d;
            }
            for (i = numRows - 2; i >= 0; i--)
                x[i] -= tmp[i + 1] * x[i + 1];
            return true;
        }

        /// <summary>
        /// Calculates the inverse of a tri-diagonal matrix.
        /// </summary>
        /// <param name="inv">The inv.</param>
        /// <returns></returns>
        public void TriDiagonal_Inverse(MatrixX inv)
        {
            int i, j; VectorX x = new(), b = new();
            Debug.Assert(numRows == numColumns);

            x.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            b.Zero();
            inv.SetSize(numRows, numColumns);

            for (i = 0; i < numRows; i++)
            {
                b[i] = 1.0f;
                TriDiagonal_Solve(x, b);
                for (j = 0; j < numRows; j++)
                    inv[j][i] = x[j];
                b[i] = 0.0f;
            }
        }

        #endregion

        #region Eigen

        /// <summary>
        /// Determine eigen values and eigen vectors for a symmetric tri-diagonal matrix.
        /// The eigen values are stored in 'eigenValues'.
        /// Column i of the original matrix will store the eigen vector corresponding to the eigenValues[i].
        /// The initial matrix has to be symmetric tri-diagonal.
        /// </summary>
        /// <param name="eigenValues">The eigen values.</param>
        /// <returns></returns>
        public bool Eigen_SolveSymmetricTriDiagonal(VectorX eigenValues)
        {
            int i; VectorX subd = new();
            Debug.Assert(numRows == numColumns);

            subd.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            eigenValues.SetSize(numRows);

            for (i = 0; i < numRows - 1; i++)
            {
                eigenValues[i] = this[i][i];
                subd[i] = this[i + 1][i];
            }
            eigenValues[numRows - 1] = this[numRows - 1][numRows - 1];

            Identity();

            return QL(eigenValues, subd);
        }

        /// <summary>
        /// Determine eigen values and eigen vectors for a symmetric matrix.
        /// The eigen values are stored in 'eigenValues'.
        /// Column i of the original matrix will store the eigen vector corresponding to the eigenValues[i].
        /// The initial matrix has to be symmetric.
        /// </summary>
        /// <param name="eigenValues">The eigen values.</param>
        /// <returns></returns>
        public bool Eigen_SolveSymmetric(VectorX eigenValues)
        {
            VectorX subd = new();
            Debug.Assert(numRows == numColumns);

            subd.SetData(numRows, VectorX.VECX_ALLOCA(numRows));
            eigenValues.SetSize(numRows);

            HouseholderReduction(eigenValues, subd);
            return QL(eigenValues, subd);
        }

        /// <summary>
        /// Determine eigen values and eigen vectors for a square matrix.
        /// The eigen values are stored in 'realEigenValues' and 'imaginaryEigenValues'.
        /// Column i of the original matrix will store the eigen vector corresponding to the realEigenValues[i] and imaginaryEigenValues[i].
        /// </summary>
        /// <param name="realEigenValues">The real eigen values.</param>
        /// <param name="imaginaryEigenValues">The imaginary eigen values.</param>
        /// <returns></returns>
        public bool Eigen_Solve(VectorX realEigenValues, VectorX imaginaryEigenValues)
        {
            MatrixX H;
            Debug.Assert(numRows == numColumns);

            realEigenValues.SetSize(numRows);
            imaginaryEigenValues.SetSize(numRows);

            H = this;

            // reduce to Hessenberg form
            HessenbergReduction(H);

            // reduce Hessenberg to real Schur form
            return HessenbergToRealSchur(H, realEigenValues, imaginaryEigenValues);
        }

        /// <summary>
        /// Eigens the sort increasing.
        /// </summary>
        /// <param name="eigenValues">The eigen values.</param>
        /// <returns></returns>
        public void Eigen_SortIncreasing(VectorX eigenValues)
        {
            int i, j, k; float min;

            for (i = 0; i <= numRows - 2; i++)
            {
                j = i;
                min = eigenValues[j];
                for (k = i + 1; k < numRows; k++)
                    if (eigenValues[k] < min)
                    {
                        j = k;
                        min = eigenValues[j];
                    }
                if (j != i)
                {
                    eigenValues.SwapElements(i, j);
                    SwapColumns(i, j);
                }
            }
        }

        /// <summary>
        /// Eigens the sort decreasing.
        /// </summary>
        /// <param name="eigenValues">The eigen values.</param>
        /// <returns></returns>
        public void Eigen_SortDecreasing(VectorX eigenValues)
        {
            int i, j, k; float max;

            for (i = 0; i <= numRows - 2; i++)
            {
                j = i;
                max = eigenValues[j];
                for (k = i + 1; k < numRows; k++)
                    if (eigenValues[k] > max)
                    {
                        j = k;
                        max = eigenValues[j];
                    }
                if (j != i)
                {
                    eigenValues.SwapElements(i, j);
                    SwapColumns(i, j);
                }
            }
        }

        #endregion
    }
}
