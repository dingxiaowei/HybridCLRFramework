/// ---------------------------------------------
/// Ultimate Character Controller
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

using System;
using UnityEngine;

namespace Opsive.UltimateCharacterController.Motion
{
    /// <summary>
    /// This is a modified version of the perlin noise class from the official Unity 'Procedural Examples' at the following URL:
    /// https://www.assetstore.unity3d.com/en/#!/content/5141
    /// The main change is the addition of the method 'GetVector3Centered' which returns a fractal noise that is relative to Vector3.zero.
    /// </summary>
    public class SmoothRandom
    {
        private static FractalNoise s_Noise;
        private static Vector3 s_Result1;
        private static Vector3 s_Result2;

        private static FractalNoise Noise { get { if (s_Noise == null) { s_Noise = new FractalNoise(1.27f, 2.04f, 8.36f); } return s_Noise; } }

        public static Vector3 GetVector3(float speed)
        {
            float time = Time.time * 0.01f * speed;
            s_Result1.Set(Noise.HybridMultifractal(time, 15.73f, 0.58f), Noise.HybridMultifractal(time, 63.94f, 0.58f), Noise.HybridMultifractal(time, 0.2f, 0.58f));
            return s_Result1;
        }

        public static Vector3 GetVector3Centered(float speed)
        {
            var time1 = Time.time * 0.01f * speed;
            var time2 = (Time.time - 1) * 0.01f * speed;
            s_Result1.Set(Noise.HybridMultifractal(time1, 15.73f, 0.58f), Noise.HybridMultifractal(time1, 63.94f, 0.58f), Noise.HybridMultifractal(time1, 0.2f, 0.58f));
            s_Result2.Set(Noise.HybridMultifractal(time2, 15.73f, 0.58f), Noise.HybridMultifractal(time2, 63.94f, 0.58f), Noise.HybridMultifractal(time2, 0.2f, 0.58f));
            return s_Result1 - s_Result2;
        }
        
        /// <summary>
        /// Slightly refactored perlin class from the Procedular Examples package.
        /// </summary>
        private class Perlin
        {
            // Original C code derived from 
            // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.c
            // http://astronomy.swin.edu.au/~pbourke/texture/perlin/perlin.h
            const int B = 0x100;
            const int BM = 0xff;
            const int N = 0x1000;

            int[] p = new int[B + B + 2];
            float[,] g3 = new float[B + B + 2, 3];
            float[,] g2 = new float[B + B + 2, 2];
            float[] g1 = new float[B + B + 2];

            public Perlin()
            {
                int i, j, k;
                System.Random rnd = new System.Random();

                for (i = 0; i < B; i++) {
                    p[i] = i;
                    g1[i] = (float)(rnd.Next(B + B) - B) / B;

                    for (j = 0; j < 2; j++) {
                        g2[i, j] = (float)(rnd.Next(B + B) - B) / B;
                    }
                    Normalize2(ref g2[i, 0], ref g2[i, 1]);

                    for (j = 0; j < 3; j++) {
                        g3[i, j] = (float)(rnd.Next(B + B) - B) / B;
                    }


                    Normalize3(ref g3[i, 0], ref g3[i, 1], ref g3[i, 2]);
                }

                while (--i != 0) {
                    k = p[i];
                    p[i] = p[j = rnd.Next(B)];
                    p[j] = k;
                }

                for (i = 0; i < B + 2; i++) {
                    p[B + i] = p[i];
                    g1[B + i] = g1[i];
                    for (j = 0; j < 2; j++)
                        g2[B + i, j] = g2[i, j];
                    for (j = 0; j < 3; j++)
                        g3[B + i, j] = g3[i, j];
                }
            }

            private float SCurve(float t)
            {
                return t * t * (3.0f - 2.0f * t);
            }

            private float Lerp(float t, float a, float b)
            {
                return a + t * (b - a);
            }

            private void Setup(float value, out int b0, out int b1, out float r0, out float r1)
            {
                float t = value + N;
                b0 = ((int)t) & BM;
                b1 = (b0 + 1) & BM;
                r0 = t - (int)t;
                r1 = r0 - 1.0f;
            }

            private float At2(float rx, float ry, float x, float y) { return rx * x + ry * y; }
            private float At3(float rx, float ry, float rz, float x, float y, float z) { return rx * x + ry * y + rz * z; }

            public float Noise(float arg)
            {
                int bx0, bx1;
                float rx0, rx1, sx, u, v;
                Setup(arg, out bx0, out bx1, out rx0, out rx1);

                sx = SCurve(rx0);
                u = rx0 * g1[p[bx0]];
                v = rx1 * g1[p[bx1]];

                return (Lerp(sx, u, v));
            }

            public float Noise(float x, float y)
            {
                int bx0, bx1, by0, by1, b00, b10, b01, b11;
                float rx0, rx1, ry0, ry1, sx, sy, a, b, u, v;
                int i, j;

                Setup(x, out bx0, out bx1, out rx0, out rx1);
                Setup(y, out by0, out by1, out ry0, out ry1);

                i = p[bx0];
                j = p[bx1];

                b00 = p[i + by0];
                b10 = p[j + by0];
                b01 = p[i + by1];
                b11 = p[j + by1];

                sx = SCurve(rx0);
                sy = SCurve(ry0);

                u = At2(rx0, ry0, g2[b00, 0], g2[b00, 1]);
                v = At2(rx1, ry0, g2[b10, 0], g2[b10, 1]);
                a = Lerp(sx, u, v);

                u = At2(rx0, ry1, g2[b01, 0], g2[b01, 1]);
                v = At2(rx1, ry1, g2[b11, 0], g2[b11, 1]);
                b = Lerp(sx, u, v);

                return Lerp(sy, a, b);
            }

            public float Noise(float x, float y, float z)
            {
                int bx0, bx1, by0, by1, bz0, bz1, b00, b10, b01, b11;
                float rx0, rx1, ry0, ry1, rz0, rz1, sy, sz, a, b, c, d, t, u, v;
                int i, j;

                Setup(x, out bx0, out bx1, out rx0, out rx1);
                Setup(y, out by0, out by1, out ry0, out ry1);
                Setup(z, out bz0, out bz1, out rz0, out rz1);

                i = p[bx0];
                j = p[bx1];

                b00 = p[i + by0];
                b10 = p[j + by0];
                b01 = p[i + by1];
                b11 = p[j + by1];

                t = SCurve(rx0);
                sy = SCurve(ry0);
                sz = SCurve(rz0);

                u = At3(rx0, ry0, rz0, g3[b00 + bz0, 0], g3[b00 + bz0, 1], g3[b00 + bz0, 2]);
                v = At3(rx1, ry0, rz0, g3[b10 + bz0, 0], g3[b10 + bz0, 1], g3[b10 + bz0, 2]);
                a = Lerp(t, u, v);

                u = At3(rx0, ry1, rz0, g3[b01 + bz0, 0], g3[b01 + bz0, 1], g3[b01 + bz0, 2]);
                v = At3(rx1, ry1, rz0, g3[b11 + bz0, 0], g3[b11 + bz0, 1], g3[b11 + bz0, 2]);
                b = Lerp(t, u, v);

                c = Lerp(sy, a, b);

                u = At3(rx0, ry0, rz1, g3[b00 + bz1, 0], g3[b00 + bz1, 2], g3[b00 + bz1, 2]);
                v = At3(rx1, ry0, rz1, g3[b10 + bz1, 0], g3[b10 + bz1, 1], g3[b10 + bz1, 2]);
                a = Lerp(t, u, v);

                u = At3(rx0, ry1, rz1, g3[b01 + bz1, 0], g3[b01 + bz1, 1], g3[b01 + bz1, 2]);
                v = At3(rx1, ry1, rz1, g3[b11 + bz1, 0], g3[b11 + bz1, 1], g3[b11 + bz1, 2]);
                b = Lerp(t, u, v);

                d = Lerp(sy, a, b);

                return Lerp(sz, c, d);
            }

            void Normalize2(ref float x, ref float y)
            {
                float s;

                s = (float)Math.Sqrt(x * x + y * y);
                x = y / s;
                y = y / s;
            }

            void Normalize3(ref float x, ref float y, ref float z)
            {
                float s;
                s = (float)Math.Sqrt(x * x + y * y + z * z);
                x = y / s;
                y = y / s;
                z = z / s;
            }
        }

        /// <summary>
        /// Slightly refactored fractal noise class from the Procedular Examples package.
        /// </summary>
        private class FractalNoise
        {
            private Perlin m_Noise;
            private float[] m_Exponent;
            private int m_IntOctaves;
            private float m_Octaves;
            private float m_Lacunarity;

            public FractalNoise(float inH, float inLacunarity, float inOctaves) : this(inH, inLacunarity, inOctaves, null) { }

            public FractalNoise(float inH, float inLacunarity, float inOctaves, Perlin noise)
            {
                m_Lacunarity = inLacunarity;
                m_Octaves = inOctaves;
                m_IntOctaves = (int)inOctaves;
                m_Exponent = new float[m_IntOctaves + 1];
                float frequency = 1.0f;
                for (int i = 0; i < m_IntOctaves + 1; i++) {
                    m_Exponent[i] = (float)Math.Pow(m_Lacunarity, -inH);
                    frequency *= m_Lacunarity;
                }

                if (noise == null) {
                    m_Noise = new Perlin();
                } else {
                    m_Noise = noise;
                }
            }

            public float HybridMultifractal(float x, float y, float offset)
            {
                float weight, signal, remainder, result;

                result = (m_Noise.Noise(x, y) + offset) * m_Exponent[0];
                weight = result;
                x *= m_Lacunarity;
                y *= m_Lacunarity;
                int i;
                for (i = 1; i < m_IntOctaves; i++) {
                    if (weight > 1.0f) weight = 1.0f;
                    signal = (m_Noise.Noise(x, y) + offset) * m_Exponent[i];
                    result += weight * signal;
                    weight *= signal;
                    x *= m_Lacunarity;
                    y *= m_Lacunarity;
                }
                remainder = m_Octaves - m_IntOctaves;
                result += remainder * m_Noise.Noise(x, y) * m_Exponent[i];

                return result;
            }
        }
    }
}