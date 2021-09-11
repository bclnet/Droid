namespace System.NumericsX
{
    public static class Program
    {
        static void Main()
        {
            // initialize math
            MathX.Init();

            // test idMatX
            //MatrixX.Test();

            // test idPolynomial
            //Polynomial.Test();

            SimdTest.Test_f(null);
        }
    }
}