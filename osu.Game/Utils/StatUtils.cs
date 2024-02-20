// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

// All code is referenced from the following:
// https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/SpecialFunctions/Erf.cs
// https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/Optimization/NelderMeadSimplex.cs

/*
 Copyright (c) 2002-2022 Math.NET
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;

namespace osu.Game.Utils
{
    public static class StatUtils
    {
        private const double sqrt2_pi = 2.5066282746310005024157652848110452530069867406099d;

        /// <summary>
        /// **************************************
        /// COEFFICIENTS FOR METHOD ErfInvImp    *
        /// **************************************
        /// </summary>
        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0, 0.5].
        /// </summary>
        private static readonly double[] erv_inv_imp_an = { -0.000508781949658280665617, -0.00836874819741736770379, 0.0334806625409744615033, -0.0126926147662974029034, -0.0365637971411762664006, 0.0219878681111168899165, 0.00822687874676915743155, -0.00538772965071242932965 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0, 0.5].
        /// </summary>
        private static readonly double[] erv_inv_imp_ad = { 1, -0.970005043303290640362, -1.56574558234175846809, 1.56221558398423026363, 0.662328840472002992063, -0.71228902341542847553, -0.0527396382340099713954, 0.0795283687341571680018, -0.00233393759374190016776, 0.000886216390456424707504 };

        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.5, 0.75].
        /// </summary>
        private static readonly double[] erv_inv_imp_bn = { -0.202433508355938759655, 0.105264680699391713268, 8.37050328343119927838, 17.6447298408374015486, -18.8510648058714251895, -44.6382324441786960818, 17.445385985570866523, 21.1294655448340526258, -3.67192254707729348546 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.5, 0.75].
        /// </summary>
        private static readonly double[] erv_inv_imp_bd = { 1, 6.24264124854247537712, 3.9713437953343869095, -28.6608180499800029974, -20.1432634680485188801, 48.5609213108739935468, 10.8268667355460159008, -22.6436933413139721736, 1.72114765761200282724 };

        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x less than 3.
        /// </summary>
        private static readonly double[] erv_inv_imp_cn = { -0.131102781679951906451, -0.163794047193317060787, 0.117030156341995252019, 0.387079738972604337464, 0.337785538912035898924, 0.142869534408157156766, 0.0290157910005329060432, 0.00214558995388805277169, -0.679465575181126350155e-6, 0.285225331782217055858e-7, -0.681149956853776992068e-9 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x less than 3.
        /// </summary>
        private static readonly double[] erv_inv_imp_cd = { 1, 3.46625407242567245975, 5.38168345707006855425, 4.77846592945843778382, 2.59301921623620271374, 0.848854343457902036425, 0.152264338295331783612, 0.01105924229346489121 };

        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x between 3 and 6.
        /// </summary>
        private static readonly double[] erv_inv_imp_dn = { -0.0350353787183177984712, -0.00222426529213447927281, 0.0185573306514231072324, 0.00950804701325919603619, 0.00187123492819559223345, 0.000157544617424960554631, 0.460469890584317994083e-5, -0.230404776911882601748e-9, 0.266339227425782031962e-11 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x between 3 and 6.
        /// </summary>
        private static readonly double[] erv_inv_imp_dd = { 1, 1.3653349817554063097, 0.762059164553623404043, 0.220091105764131249824, 0.0341589143670947727934, 0.00263861676657015992959, 0.764675292302794483503e-4 };

        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x between 6 and 18.
        /// </summary>
        private static readonly double[] erv_inv_imp_en = { -0.0167431005076633737133, -0.00112951438745580278863, 0.00105628862152492910091, 0.000209386317487588078668, 0.149624783758342370182e-4, 0.449696789927706453732e-6, 0.462596163522878599135e-8, -0.281128735628831791805e-13, 0.99055709973310326855e-16 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x between 6 and 18.
        /// </summary>
        private static readonly double[] erv_inv_imp_ed = { 1, 0.591429344886417493481, 0.138151865749083321638, 0.0160746087093676504695, 0.000964011807005165528527, 0.275335474764726041141e-4, 0.282243172016108031869e-6 };

        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x between 18 and 44.
        /// </summary>
        private static readonly double[] erv_inv_imp_fn = { -0.0024978212791898131227, -0.779190719229053954292e-5, 0.254723037413027451751e-4, 0.162397777342510920873e-5, 0.396341011304801168516e-7, 0.411632831190944208473e-9, 0.145596286718675035587e-11, -0.116765012397184275695e-17 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x between 18 and 44.
        /// </summary>
        private static readonly double[] erv_inv_imp_fd = { 1, 0.207123112214422517181, 0.0169410838120975906478, 0.000690538265622684595676, 0.145007359818232637924e-4, 0.144437756628144157666e-6, 0.509761276599778486139e-9 };

        /// <summary> Polynomial coefficients for a numerator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x greater than 44.
        /// </summary>
        private static readonly double[] erv_inv_imp_gn = { -0.000539042911019078575891, -0.28398759004727721098e-6, 0.899465114892291446442e-6, 0.229345859265920864296e-7, 0.225561444863500149219e-9, 0.947846627503022684216e-12, 0.135880130108924861008e-14, -0.348890393399948882918e-21 };

        /// <summary> Polynomial coefficients for a denominator of ErfInvImp
        /// calculation for Erf^-1(z) in the interval [0.75, 1] with x greater than 44.
        /// </summary>
        private static readonly double[] erv_inv_imp_gd = { 1, 0.0845746234001899436914, 0.00282092984726264681981, 0.468292921940894236786e-4, 0.399968812193862100054e-6, 0.161809290887904476097e-8, 0.231558608310259605225e-11 };

        /// <summary>Calculates the inverse error function evaluated at z.</summary>
        /// <returns>The inverse error function evaluated at given value.</returns>
        /// <remarks>
        ///   <list type="bullet">
        ///     <item>returns double.PositiveInfinity if <c>z &gt;= 1.0</c>.</item>
        ///     <item>returns double.NegativeInfinity if <c>z &lt;= -1.0</c>.</item>
        ///   </list>
        /// </remarks>
        /// <summary>Calculates the inverse error function evaluated at z.</summary>
        /// <param name="z">value to evaluate.</param>
        /// <returns>the inverse error function evaluated at Z.</returns>
        public static double ErfInv(double z)
        {
            if (z == 0.0)
            {
                return 0.0;
            }

            if (z >= 1.0)
            {
                return double.PositiveInfinity;
            }

            if (z <= -1.0)
            {
                return double.NegativeInfinity;
            }

            double p, q, s;

            if (z < 0)
            {
                p = -z;
                q = 1 - p;
                s = -1;
            }
            else
            {
                p = z;
                q = 1 - z;
                s = 1;
            }

            return erfInvImpl(p, q, s);
        }

        /// <summary>
        /// The implementation of the inverse error function.
        /// </summary>
        /// <param name="p">First intermediate parameter.</param>
        /// <param name="q">Second intermediate parameter.</param>
        /// <param name="s">Third intermediate parameter.</param>
        /// <returns>the inverse error function.</returns>
        private static double erfInvImpl(double p, double q, double s)
        {
            double result;

            if (p <= 0.5)
            {
                // Evaluate inverse erf using the rational approximation:
                //
                // x = p(p+10)(Y+R(p))
                //
                // Where Y is a constant, and R(p) is optimized for a low
                // absolute error compared to |Y|.
                //
                // double: Max error found: 2.001849e-18
                // long double: Max error found: 1.017064e-20
                // Maximum Deviation Found (actual error term at infinite precision) 8.030e-21
                const float y = 0.0891314744949340820313f;
                double g = p * (p + 10);
                double r = EvaluatePolynomial(p, erv_inv_imp_an) / EvaluatePolynomial(p, erv_inv_imp_ad);
                result = (g * y) + (g * r);
            }
            else if (q >= 0.25)
            {
                // Rational approximation for 0.5 > q >= 0.25
                //
                // x = sqrt(-2*log(q)) / (Y + R(q))
                //
                // Where Y is a constant, and R(q) is optimized for a low
                // absolute error compared to Y.
                //
                // double : Max error found: 7.403372e-17
                // long double : Max error found: 6.084616e-20
                // Maximum Deviation Found (error term) 4.811e-20
                const float y = 2.249481201171875f;
                double g = Math.Sqrt(-2 * Math.Log(q));
                double xs = q - 0.25;
                double r = EvaluatePolynomial(xs, erv_inv_imp_bn) / EvaluatePolynomial(xs, erv_inv_imp_bd);
                result = g / (y + r);
            }
            else
            {
                // For q < 0.25 we have a series of rational approximations all
                // of the general form:
                //
                // let: x = sqrt(-log(q))
                //
                // Then the result is given by:
                //
                // x(Y+R(x-B))
                //
                // where Y is a constant, B is the lowest value of x for which
                // the approximation is valid, and R(x-B) is optimized for a low
                // absolute error compared to Y.
                //
                // Note that almost all code will really go through the first
                // or maybe second approximation.  After than we're dealing with very
                // small input values indeed: 80 and 128 bit long double's go all the
                // way down to ~ 1e-5000 so the "tail" is rather long...
                double x = Math.Sqrt(-Math.Log(q));

                if (x < 3)
                {
                    // Max error found: 1.089051e-20
                    const float y = 0.807220458984375f;
                    double xs = x - 1.125;
                    double r = EvaluatePolynomial(xs, erv_inv_imp_cn) / EvaluatePolynomial(xs, erv_inv_imp_cd);
                    result = (y * x) + (r * x);
                }
                else if (x < 6)
                {
                    // Max error found: 8.389174e-21
                    const float y = 0.93995571136474609375f;
                    double xs = x - 3;
                    double r = EvaluatePolynomial(xs, erv_inv_imp_dn) / EvaluatePolynomial(xs, erv_inv_imp_dd);
                    result = (y * x) + (r * x);
                }
                else if (x < 18)
                {
                    // Max error found: 1.481312e-19
                    const float y = 0.98362827301025390625f;
                    double xs = x - 6;
                    double r = EvaluatePolynomial(xs, erv_inv_imp_en) / EvaluatePolynomial(xs, erv_inv_imp_ed);
                    result = (y * x) + (r * x);
                }
                else if (x < 44)
                {
                    // Max error found: 5.697761e-20
                    const float y = 0.99714565277099609375f;
                    double xs = x - 18;
                    double r = EvaluatePolynomial(xs, erv_inv_imp_fn) / EvaluatePolynomial(xs, erv_inv_imp_fd);
                    result = (y * x) + (r * x);
                }
                else
                {
                    // Max error found: 1.279746e-20
                    const float y = 0.99941349029541015625f;
                    double xs = x - 44;
                    double r = EvaluatePolynomial(xs, erv_inv_imp_gn) / EvaluatePolynomial(xs, erv_inv_imp_gd);
                    result = (y * x) + (r * x);
                }
            }

            return s * result;
        }

        /// <summary>
        /// Evaluate a polynomial at point x.
        /// Coefficients are ordered ascending by power with power k at index k.
        /// Example: coefficients [3,-1,2] represent y=2x^2-x+3.
        /// </summary>
        /// <param name="z">The location where to evaluate the polynomial at.</param>
        /// <param name="coefficients">The coefficients of the polynomial, coefficient for power k at index k.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="coefficients"/> is a null reference.
        /// </exception>
        public static double EvaluatePolynomial(double z, params double[] coefficients)
        {
            // 2020-10-07 jbialogrodzki #730 Since this is public API we should probably
            // handle null arguments? It doesn't seem to have been done consistently in this class though.
            if (coefficients == null)
            {
                throw new ArgumentNullException(nameof(coefficients));
            }

            // 2020-10-07 jbialogrodzki #730 Zero polynomials need explicit handling.
            // Without this check, we attempted to peek coefficients at negative indices!
            int n = coefficients.Length;

            if (n == 0)
            {
                return 0;
            }

            double sum = coefficients[n - 1];

            for (int i = n - 2; i >= 0; --i)
            {
                sum *= z;
                sum += coefficients[i];
            }

            return sum;
        }
    }
}
