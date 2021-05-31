using System.Diagnostics;

namespace Droid.Core
{
    partial class Polynomial
    {
        public static void Test()
        {
            int i, num; float[] roots; float value; Complex[] complexRoots; Complex complexValue; Polynomial p;

            p = new(-5f, 4f);
            num = p.GetRoots(out roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(-5f, 4f, 3f);
            num = p.GetRoots(out roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(1f, 4f, 3f, -2f);
            num = p.GetRoots(out roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(5f, 4f, 3f, -2f);
            num = p.GetRoots(out roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(-5f, 4f, 3f, 2f, 1f);
            num = p.GetRoots(out roots);
            for (i = 0; i < num; i++)
            {
                value = p.GetValue(roots[i]);
                Debug.Assert(MathX.Fabs(value) < 1e-4f);
            }

            p = new(1f, 4f, 3f, -2f);
            num = p.GetRoots(out complexRoots);
            for (i = 0; i < num; i++)
            {
                complexValue = p.GetValue(complexRoots[i]);
                Debug.Assert(MathX.Fabs(complexValue.r) < 1e-4f && MathX.Fabs(complexValue.i) < 1e-4f);
            }

            p = new(5f, 4f, 3f, -2f);
            num = p.GetRoots(out complexRoots);
            for (i = 0; i < num; i++)
            {
                complexValue = p.GetValue(complexRoots[i]);
                Debug.Assert(MathX.Fabs(complexValue.r) < 1e-4f && MathX.Fabs(complexValue.i) < 1e-4f);
            }
        }
    }
}