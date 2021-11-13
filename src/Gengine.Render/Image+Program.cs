using System;
using System.NumericsX;
using System.NumericsX.OpenStack;
using System.Text;

namespace Gengine.Render
{
    static unsafe partial class R
    {
        // it is not possible to convert a heightmap into a normal map properly without knowing the texture coordinate stretching.
        // We can assume constant and equal ST vectors for walls, but not for characters.
        static void R_HeightmapToNormalMap(byte* data, int width, int height, float scale)
        {
            int i, j; byte* depth;

            scale = scale / 256;

            // copy and convert to grey scale
            j = width * height;
            depth = (byte*)R_StaticAlloc(j);
            for (i = 0; i < j; i++) depth[i] = (data[i * 4] + data[i * 4 + 1] + data[i * 4 + 2]) / 3;

            Vector3 dir, dir2;
            for (i = 0; i < height; i++)
                for (j = 0; j < width; j++)
                {
                    int d1, d2, d3, d4, a1, a3, a4;

                    // FIXME: look at five points?

                    // look at three points to estimate the gradient
                    a1 = d1 = depth[(i * width + j)];
                    d2 = depth[i * width + ((j + 1) & (width - 1))];
                    a3 = d3 = depth[((i + 1) & (height - 1)) * width + j];
                    a4 = d4 = depth[((i + 1) & (height - 1)) * width + ((j + 1) & (width - 1))];

                    d2 -= d1;
                    d3 -= d1;

                    dir.x = -d2 * scale; dir.y = -d3 * scale; dir.z = 1; dir.NormalizeFast();

                    a1 -= a3;
                    a4 -= a3;

                    dir2.x = -a4 * scale; dir2.y = a1 * scale; dir2.z = 1; dir2.NormalizeFast();

                    dir += dir2; dir.NormalizeFast();

                    a1 = (i * width + j) * 4;
                    data[a1 + 0] = (byte)(dir[0] * 127 + 128);
                    data[a1 + 1] = (byte)(dir[1] * 127 + 128);
                    data[a1 + 2] = (byte)(dir[2] * 127 + 128);
                    data[a1 + 3] = 255;
                }

            R_StaticFree(depth);
        }

        static void R_ImageScale(byte* data, int width, int height, float[] scale)
        {
            int i, j, c;

            c = width * height * 4;

            for (i = 0; i < c; i++)
            {
                j = (byte)(data[i] * scale[i & 3]);
                if (j < 0) j = 0;
                else if (j > 255) j = 255;
                data[i] = j;
            }
        }

        static void R_InvertAlpha(byte* data, int width, int height)
        {
            int i, c;

            c = width * height * 4;

            for (i = 0; i < c; i += 4) data[i + 3] = 255 - data[i + 3];
        }

        static void R_InvertColor(byte* data, int width, int height)
        {
            int i, c;

            c = width * height * 4;

            for (i = 0; i < c; i += 4)
            {
                data[i + 0] = 255 - data[i + 0];
                data[i + 1] = 255 - data[i + 1];
                data[i + 2] = 255 - data[i + 2];
            }
        }

        static void R_AddNormalMaps(byte* data1, int width1, int height1, byte* data2, int width2, int height2)
        {
            int i, j; byte* newMap;

            // resample pic2 to the same size as pic1
            if (width2 != width1 || height2 != height1) { newMap = R_Dropsample(data2, width2, height2, width1, height1); data2 = newMap; }
            else newMap = null;

            // add the normal change from the second and renormalize
            for (i = 0; i < height1; i++)
                for (j = 0; j < width1; j++)
                {
                    byte* d1, *d2;
                    Vector3 n;
                    float len;

                    d1 = data1 + (i * width1 + j) * 4;
                    d2 = data2 + (i * width1 + j) * 4;

                    n.x = (d1[0] - 128) / 127.0;
                    n.y = (d1[1] - 128) / 127.0;
                    n.z = (d1[2] - 128) / 127.0;

                    // There are some normal maps that blend to 0,0,0 at the edges
                    // this screws up compression, so we try to correct that here by instead fading it to 0,0,1
                    len = n.LengthFast;
                    if (len < 1.0f) n.z = MathX.Sqrt(1.0 - (n[0] * n[0]) - (n[1] * n[1]));

                    n.x += (d2[0] - 128) / 127.0; n.y += (d2[1] - 128) / 127.0; n.Normalize();

                    d1[0] = (byte)(n[0] * 127 + 128);
                    d1[1] = (byte)(n[1] * 127 + 128);
                    d1[2] = (byte)(n[2] * 127 + 128);
                    d1[3] = 255;
                }

            if (newMap != null) R_StaticFree(newMap);
        }

        static float[][] R_SmoothNormalMap_factors = {
            new[]{1f, 1f, 1f},
            new[]{1f, 1f, 1f},
            new[]{1f, 1f, 1f}
        };
        static void R_SmoothNormalMap(byte* data, int width, int height)
        {
            byte* orig;
            int i, j, k, l;
            Vector3 normal;
            byte* o;

            orig = (byte*)R_StaticAlloc(width * height * 4);
            memcpy(orig, data, width * height * 4);

            for (i = 0; i < width; i++)
                for (j = 0; j < height; j++)
                {
                    normal = Vector3.origin;
                    for (k = -1; k < 2; k++)
                        for (l = -1; l < 2; l++)
                        {
                            byte* i2;

                            i2 = orig + (((j + l) & (height - 1)) * width + ((i + k) & (width - 1))) * 4;

                            // ignore 000 and -1 -1 -1
                            if (i2[0] == 0 && i2[1] == 0 && i2[2] == 0) continue;
                            if (i2[0] == 128 && i2[1] == 128 && i2[2] == 128) continue;

                            normal[0] += R_SmoothNormalMap_factors[k + 1][l + 1] * (i2[0] - 128);
                            normal[1] += R_SmoothNormalMap_factors[k + 1][l + 1] * (i2[1] - 128);
                            normal[2] += R_SmoothNormalMap_factors[k + 1][l + 1] * (i2[2] - 128);
                        }
                    normal.Normalize();
                    o = data + (j * width + i) * 4;
                    o[0] = (byte)(128 + 127 * normal[0]);
                    o[1] = (byte)(128 + 127 * normal[1]);
                    o[2] = (byte)(128 + 127 * normal[2]);
                }

            R_StaticFree(orig);
        }

        static void R_ImageAdd(byte* data1, int width1, int height1, byte* data2, int width2, int height2)
        {
            int i, j, c; byte* newMap;

            // resample pic2 to the same size as pic1
            if (width2 != width1 || height2 != height1) { newMap = R_Dropsample(data2, width2, height2, width1, height1); data2 = newMap; }
            else newMap = null;

            c = width1 * height1 * 4;

            for (i = 0; i < c; i++)
            {
                j = data1[i] + data2[i];
                if (j > 255) j = 255;
                data1[i] = j;
            }

            if (newMap != null) R_StaticFree(newMap);
        }

        // we build a canonical token form of the image program here
        static StringBuilder parseBuffer = new();

        static void AppendToken(Token token)
        {
            // add a leading space if not at the beginning
            if (parseBuffer.Length > 0) parseBuffer.Append(' ');
            parseBuffer.Append(token);
        }

        static void MatchAndAppendToken(Lexer src, string match)
        {
            if (!src.ExpectTokenString(match)) return;
            // a matched token won't need a leading space
            parseBuffer.Append(match);
        }

        // If pic is null, the timestamps will be filled in, but no image will be generated
        // If both pic and timestamps are null, it will just advance past it, which can be used to parse an image program from a text stream.
        static bool R_ParseImageProgram_r(Lexer src, ref byte[] pic, out int width, out int height, out DateTime timestamps, ref TextureDepth depth)
        {
            float scale; DateTime timestamp;

            src.ReadToken(out var token); AppendToken(token);

            if (string.Equals(token, "heightmap", StringComparison.OrdinalIgnoreCase))
            {
                MatchAndAppendToken(src, "(");
                if (!R_ParseImageProgram_r(src, out pic, out width, out height, out timestamps, depth)) return false;
                MatchAndAppendToken(src, ",");
                src.ReadToken(out token); AppendToken(token); scale = token.FloatValue;

                // process it
                if (pic != null) { R_HeightmapToNormalMap(pic, out width, out height, scale); depth = TD_BUMP; }
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "addnormals", StringComparison.OrdinalIgnoreCase))
            {
                byte* pic2; int width2, height2;

                MatchAndAppendToken(src, "(");
                if (!R_ParseImageProgram_r(src, pic, out width, out height, out timestamps, depth)) return false;
                MatchAndAppendToken(src, ",");
                if (!R_ParseImageProgram_r(src, pic != null ? &pic2 : null, &width2, &height2, timestamps, depth)) { if (pic != null) { R_StaticFree(pic); pic = null; } return false; }

                // process it
                if (pic != null) { R_AddNormalMaps(out pic, out width, out height, pic2, width2, height2); R_StaticFree(pic2); depth = TD_BUMP; }
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "smoothnormals", StringComparison.OrdinalIgnoreCase))
            {
                MatchAndAppendToken(src, "(");
                if (!R_ParseImageProgram_r(src, pic, width, height, timestamps, depth)) return false;
                if (pic != null) { R_SmoothNormalMap(out pic, out width, out height); depth = TD_BUMP; }
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "add", StringComparison.OrdinalIgnoreCase))
            {
                byte* pic2; int width2, height2;

                MatchAndAppendToken(src, "(");
                if (!R_ParseImageProgram_r(src, pic, width, height, timestamps, depth)) return false;
                MatchAndAppendToken(src, ",");
                if (!R_ParseImageProgram_r(src, pic != null ? &pic2 : null, &width2, &height2, timestamps, depth)) { if (pic != null) { R_StaticFree(pic); pic = null; } }

                // process it
                if (pic != null) { R_ImageAdd(pic, width, height, pic2, width2, height2); R_StaticFree(pic2); }
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "scale", StringComparison.OrdinalIgnoreCase))
            {
                float scale[4]; int i;

                MatchAndAppendToken(src, "(");
                R_ParseImageProgram_r(src, pic, width, height, timestamps, depth);
                for (i = 0; i < 4; i++) { MatchAndAppendToken(src, ","); src.ReadToken(out token); AppendToken(token); scale[i] = token.FloatValue; }

                // process it
                if (pic != null) R_ImageScale(pic, width, height, scale);
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "invertAlpha", StringComparison.OrdinalIgnoreCase))
            {
                MatchAndAppendToken(src, "(");
                R_ParseImageProgram_r(src, pic, width, height, timestamps, depth);

                // process it
                if (pic != null) R_InvertAlpha(pic, width, height);
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "invertColor", StringComparison.OrdinalIgnoreCase))
            {
                MatchAndAppendToken(src, "(");
                R_ParseImageProgram_r(src, pic, width, height, timestamps, depth);

                // process it
                if (pic != null) R_InvertColor(pic, width, height);
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "makeIntensity", StringComparison.OrdinalIgnoreCase))
            {
                int i;

                MatchAndAppendToken(src, "(");
                R_ParseImageProgram_r(src, pic, width, height, timestamps, depth);

                // copy red to green, blue, and alpha
                if (pic != null)
                {
                    var c = width * height * 4;
                    for (i = 0; i < c; i += 4) pic[i + 1] = pic[i + 2] = pic[i + 3] = pic[i];
                }
                MatchAndAppendToken(src, ")");
                return true;
            }

            if (string.Equals(token, "makeAlpha", StringComparison.OrdinalIgnoreCase))
            {
                int i;

                MatchAndAppendToken(src, "(");
                R_ParseImageProgram_r(src, pic, width, height, timestamps, depth);

                // average RGB into alpha, then set RGB to white
                if (pic != null)
                {
                    var c = width * height * 4;
                    for (i = 0; i < c; i += 4)
                    {
                        pic[i + 3] = (pic[i + 0] + pic[i + 1] + pic[i + 2]) / 3;
                        pic[i + 0] = pic[i + 1] = pic[i + 2] = 255;
                    }
                }
                MatchAndAppendToken(src, ")");
                return true;
            }

            // if we are just parsing instead of loading or checking, don't do the R_LoadImage
            if (!timestamps && pic == null) return true;

            // load it as an image
            R_LoadImage(token, pic, width, height, &timestamp, true);

            if (timestamp == -1) return false;

            // add this to the timestamp
            if (timestamps)
                if (timestamp > timestamps) timestamps = timestamp;

            return true;
        }

        static void R_LoadImageProgram(string name, out byte[] pic, out int width, out int height, out DateTime timestamps, TextureDepth depth)
        {
            Lexer src = new();

            src.LoadMemory(name, name.Length, name);
            src.Flags = LEXFL.NOFATALERRORS | LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS | LEXFL.ALLOWPATHNAMES;

            parseBuffer[0] = 0;
            timestamps = 0;

            R_ParseImageProgram_r(src, pic, width, height, timestamps, depth);
            src.FreeSource();
        }

        string R_ParsePastImageProgram(Lexer src)
        {
            parseBuffer[0] = 0;
            R_ParseImageProgram_r(src, null, null, null, null, null);
            return parseBuffer;
        }
    }
}