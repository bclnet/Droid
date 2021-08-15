using Gengine.Render;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;
using static Gengine.Lib;

namespace Gengine.UI
{
    public class DeviceContext
    {
        public const int VIRTUAL_WIDTH = 640;
        public const int VIRTUAL_HEIGHT = 480;
        public const int BLINK_DIVISOR = 200;

        public enum CURSOR
        {
            ARROW,
            HAND,
            COUNT
        }

        public enum ALIGN : byte
        {
            LEFT,
            CENTER,
            RIGHT
        }

        public enum SCROLLBAR
        {
            HBACK,
            VBACK,
            THUMB,
            RIGHT,
            LEFT,
            UP,
            DOWN,
            COUNT
        }

        public static Vector4 colorPurple = new();
        public static Vector4 colorOrange = new();
        public static Vector4 colorYellow = new();
        public static Vector4 colorGreen = new();
        public static Vector4 colorBlue = new();
        public static Vector4 colorRed = new();
        public static Vector4 colorWhite = new();
        public static Vector4 colorBlack = new();
        public static Vector4 colorNone = new();


        static readonly CVar gui_smallFontLimit = new("gui_smallFontLimit", "0.30", CVAR.GUI | CVAR.ARCHIVE, "");
        static readonly CVar gui_mediumFontLimit = new("gui_mediumFontLimit", "0.60", CVAR.GUI | CVAR.ARCHIVE, "");

        Material[] cursorImages = new Material[(int)CURSOR.COUNT];
        Material[] scrollBarImages = new Material[(int)SCROLLBAR.COUNT];
        Material whiteImage;
        FontInfoEx activeFont;
        FontInfo useFont;
        string fontName;
        float xScale;
        float yScale;

        float vidHeight;
        float vidWidth;

        CURSOR cursor;

        List<Rectangle> clipRects = new();

        static List<FontInfoEx> fonts = new();
        string fontLang;

        bool enableClipping;

        bool overStrikeMode;

        Matrix3x3 mat;
        Vector3 origin;
        bool initialized;

        bool mbcs;

        // DG: this is used for the "make sure menus are rendered as 4:3" hack
        Vector2 fixScaleForMenu;
        Vector2 fixOffsetForMenu;

        public DeviceContext()
            => Clear();

        public void Init()
        {
            xScale = 0f;
            SetSize(VIRTUAL_WIDTH, VIRTUAL_HEIGHT);
            whiteImage = declManager.FindMaterial("guis/assets/white.tga");
            whiteImage.Sort = SS_GUI;
            mbcs = false;
            SetupFonts();
            activeFont = fonts[0];
            colorPurple = new Vector4(1, 0, 1, 1);
            colorOrange = new Vector4(1, 1, 0, 1);
            colorYellow = new Vector4(0, 1, 1, 1);
            colorGreen = new Vector4(0, 1, 0, 1);
            colorBlue = new Vector4(0, 0, 1, 1);
            colorRed = new Vector4(1, 0, 0, 1);
            colorWhite = new Vector4(1, 1, 1, 1);
            colorBlack = new Vector4(0, 0, 0, 1);
            colorNone = new Vector4(0, 0, 0, 0);
            cursorImages[(int)CURSOR.ARROW] = declManager.FindMaterial("ui/assets/guicursor_arrow.tga");
            cursorImages[(int)CURSOR.HAND] = declManager.FindMaterial("ui/assets/guicursor_hand.tga");
            scrollBarImages[(int)SCROLLBAR.HBACK] = declManager.FindMaterial("ui/assets/scrollbarh.tga");
            scrollBarImages[(int)SCROLLBAR.VBACK] = declManager.FindMaterial("ui/assets/scrollbarv.tga");
            scrollBarImages[(int)SCROLLBAR.THUMB] = declManager.FindMaterial("ui/assets/scrollbar_thumb.tga");
            scrollBarImages[(int)SCROLLBAR.RIGHT] = declManager.FindMaterial("ui/assets/scrollbar_right.tga");
            scrollBarImages[(int)SCROLLBAR.LEFT] = declManager.FindMaterial("ui/assets/scrollbar_left.tga");
            scrollBarImages[(int)SCROLLBAR.UP] = declManager.FindMaterial("ui/assets/scrollbar_up.tga");
            scrollBarImages[(int)SCROLLBAR.DOWN] = declManager.FindMaterial("ui/assets/scrollbar_down.tga");
            cursorImages[(int)CURSOR.ARROW].Sort = SS_GUI;
            cursorImages[(int)CURSOR.HAND].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.HBACK].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.VBACK].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.THUMB].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.RIGHT].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.LEFT].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.UP].Sort = SS_GUI;
            scrollBarImages[(int)SCROLLBAR.DOWN].Sort = SS_GUI;
            cursor = (int)CURSOR.ARROW;
            enableClipping = true;
            overStrikeMode = true;
            mat.Identity();
            origin.Zero();
            initialized = true;

            // DG: this is used for the "make sure menus are rendered as 4:3" hack
            fixScaleForMenu.Set(1, 1);
            fixOffsetForMenu.Set(0, 0);
        }
        public void Shutdown()
        {
            fontName = string.Empty;
            clipRects.Clear();
            fonts.Clear();
            Clear();
        }
        public bool Initialized => initialized;
        public void EnableLocalization();

        public void GetTransformInfo(out Vector3 origin, out Matrix3x3 mat)
        {
            mat = this.mat;
            origin = this.origin;
        }
        public void SetTransformInfo(Vector3 origin, Matrix3x3 mat)
        {
            this.origin = origin;
            this.mat = mat;
        }

        public void DrawMaterial(float x, float y, float w, float h, Material mat, Vector4 color, float scalex = 1f, float scaley = 1f)
        {
            renderSystem.SetColor(color);

            float s0, s1, t0, t1;
            //
            //  handle negative scales as well
            if (scalex < 0) { w *= -1; scalex *= -1; }
            if (scaley < 0) { h *= -1; scaley *= -1; }
            //
            if (w < 0) { w = -w; s0 = 1 * scalex; s1 = 0; } // flip about vertical
            else { s0 = 0; s1 = 1 * scalex; }

            if (h < 0) { h = -h; t0 = 1 * scaley; t1 = 0; } // flip about horizontal
            else { t0 = 0; t1 = 1 * scaley; }

            if (ClippedCoords(ref x, ref y, ref w, ref h, ref s0, ref t0, ref s1, ref t1))
                return;

            AdjustCoords(ref x, ref y, ref w, ref h);

            DrawStretchPic(x, y, w, h, s0, t0, s1, t1, mat);
        }

        public void DrawRect(float x, float y, float width, float height, float size, Vector4 color)
        {
            if (color.w == 0f)
                return;

            renderSystem.SetColor(color);

            if (ClippedCoords(ref x, ref y, ref width, ref height, null, null, null, null))
                return;

            AdjustCoords(ref x, ref y, ref width, ref height);
            DrawStretchPic(x, y, size, height, 0, 0, 0, 0, whiteImage);
            DrawStretchPic(x + width - size, y, size, height, 0, 0, 0, 0, whiteImage);
            DrawStretchPic(x, y, width, size, 0, 0, 0, 0, whiteImage);
            DrawStretchPic(x, y + height - size, width, size, 0, 0, 0, 0, whiteImage);
        }

        public void DrawFilledRect(float x, float y, float width, float height, Vector4 color)
        {
            if (color.w == 0f)
                return;

            renderSystem.SetColor(color);

            if (ClippedCoords(ref x, ref y, ref width, ref height, null, null, null, null))
                return;

            AdjustCoords(ref x, ref y, ref width, ref height);
            DrawStretchPic(x, y, width, height, 0, 0, 0, 0, whiteImage);
        }

        public int DrawText(string text, float textScale, ALIGN textAlign, Vector4 color, Rectangle rectDraw, bool wrap, int cursor = -1, bool calcOnly = false, List<int> breaks = null, int limit = 0)
        {
            const char* p, *textPtr, *newLinePtr;
            char buff[1024];
            int len, newLine, newLineWidth, count;
            float y;
            float textWidth;

            float charSkip = MaxCharWidth(textScale) + 1;
            float lineSkip = MaxCharHeight(textScale);

            float cursorSkip = cursor >= 0 ? charSkip : 0;

            bool lineBreak, wordBreak;

            SetFontByScale(textScale);

            textWidth = 0;
            newLinePtr = null;

            if (!calcOnly && !(text && *text))
            {
                if (cursor == 0)
                {
                    renderSystem.SetColor(color);
                    DrawEditCursor(rectDraw.x, lineSkip + rectDraw.y, textScale);
                }
                return MathX.FtoiFast(rectDraw.w / charSkip);
            }

            textPtr = text;

            y = lineSkip + rectDraw.y;
            len = 0;
            buff[0] = '\0';
            newLine = 0;
            newLineWidth = 0;
            p = textPtr;

            if (breaks)
                breaks.Add(0);
            count = 0;
            textWidth = 0;
            lineBreak = false;
            wordBreak = false;
            while (p != null)
            {
                if (*p == '\n' || *p == '\r' || *p == '\0')
                {
                    lineBreak = true;
                    if ((*p == '\n' && *(p + 1) == '\r') || (*p == '\r' && *(p + 1) == '\n'))
                        p++;
                }

                int nextCharWidth = (StringX.CharIsPrintable(*p) ? CharWidth(*p, textScale) : cursorSkip);
                // FIXME: this is a temp hack until the guis can be fixed not not overflow the bounding rectangles the side-effect is that list boxes and edit boxes will draw over their scroll bars
                //	The following line and the !linebreak in the if statement below should be removed
                nextCharWidth = 0;

                if (!lineBreak && (textWidth + nextCharWidth) > rectDraw.w)
                {
                    // The next character will cause us to overflow, if we haven't yet found a suitable break spot, set it to be this character
                    if (len > 0 && newLine == 0)
                    {
                        newLine = len;
                        newLinePtr = p;
                        newLineWidth = textWidth;
                    }
                    wordBreak = true;
                }
                else if (lineBreak || (wrap && (*p == ' ' || *p == '\t')))
                {
                    // The next character is in view, so if we are a break character, store our position
                    newLine = len;
                    newLinePtr = p + 1;
                    newLineWidth = textWidth;
                }

                if (lineBreak || wordBreak)
                {
                    var x = rectDraw.x;

                    if (textAlign == ALIGN.RIGHT) x = rectDraw.x + rectDraw.w - newLineWidth;
                    else if (textAlign == ALIGN.CENTER) x = rectDraw.x + (rectDraw.w - newLineWidth) / 2;

                    if (wrap || newLine > 0)
                    {
                        buff[newLine] = '\0';

                        // This is a special case to handle breaking in the middle of a word. if we didn't do this, the cursor would appear on the end of this line and the beginning of the next.
                        if (wordBreak && cursor >= newLine && newLine == len)
                            cursor++;
                    }

                    if (!calcOnly)
                        count += DrawText(x, y, textScale, color, buff, 0, 0, 0, cursor);

                    if (cursor < newLine) cursor = -1;
                    else if (cursor >= 0) cursor -= (newLine + 1);

                    if (!wrap)
                        return newLine;

                    if ((limit && count > limit) || *p == '\0')
                        break;

                    y += lineSkip + 5;

                    if (!calcOnly && y > rectDraw.Bottom)
                        break;

                    p = newLinePtr;

                    if (breaks)
                        breaks.Add(p - text);

                    len = 0;
                    newLine = 0;
                    newLineWidth = 0;
                    textWidth = 0;
                    lineBreak = false;
                    wordBreak = false;
                    continue;
                }

                buff[len++] = *p++;
                buff[len] = '\0';
                // update the width
                if (buff[len - 1] != C_COLOR_ESCAPE && (len <= 1 || buff[len - 2] != C_COLOR_ESCAPE))
                    textWidth += textScale * useFont.glyphScale * useFont.glyphs[buff[len - 1]].xSkip;
            }

            return MathX.FtoiFast(rectDraw.w / charSkip);
        }

        public void DrawMaterialRect(float x, float y, float w, float h, float size, Material mat, Vector4 color)
        {
            if (color.w == 0f)
                return;

            renderSystem.SetColor(color);
            DrawMaterial(x, y, size, h, mat, color);
            DrawMaterial(x + w - size, y, size, h, mat, color);
            DrawMaterial(x, y, w, size, mat, color);
            DrawMaterial(x, y + h - size, w, size, mat, color);
        }

        public void DrawStretchPic(float x, float y, float w, float h, float s0, float t0, float s1, float t1, Material mat)
        {
            DrawVert[] verts[4];
            GlIndex[] indexes[6];
            indexes[0] = 3;
            indexes[1] = 0;
            indexes[2] = 2;
            indexes[3] = 2;
            indexes[4] = 0;
            indexes[5] = 1;
            verts[0].xyz[0] = x;
            verts[0].xyz[1] = y;
            verts[0].xyz[2] = 0;
            verts[0].st[0] = s1;
            verts[0].st[1] = t1;
            verts[0].normal[0] = 0;
            verts[0].normal[1] = 0;
            verts[0].normal[2] = 1;
            verts[0].tangents[0][0] = 1;
            verts[0].tangents[0][1] = 0;
            verts[0].tangents[0][2] = 0;
            verts[0].tangents[1][0] = 0;
            verts[0].tangents[1][1] = 1;
            verts[0].tangents[1][2] = 0;
            verts[1].xyz[0] = x + w;
            verts[1].xyz[1] = y;
            verts[1].xyz[2] = 0;
            verts[1].st[0] = s2;
            verts[1].st[1] = t1;
            verts[1].normal[0] = 0;
            verts[1].normal[1] = 0;
            verts[1].normal[2] = 1;
            verts[1].tangents[0][0] = 1;
            verts[1].tangents[0][1] = 0;
            verts[1].tangents[0][2] = 0;
            verts[1].tangents[1][0] = 0;
            verts[1].tangents[1][1] = 1;
            verts[1].tangents[1][2] = 0;
            verts[2].xyz[0] = x + w;
            verts[2].xyz[1] = y + h;
            verts[2].xyz[2] = 0;
            verts[2].st[0] = s2;
            verts[2].st[1] = t2;
            verts[2].normal[0] = 0;
            verts[2].normal[1] = 0;
            verts[2].normal[2] = 1;
            verts[2].tangents[0][0] = 1;
            verts[2].tangents[0][1] = 0;
            verts[2].tangents[0][2] = 0;
            verts[2].tangents[1][0] = 0;
            verts[2].tangents[1][1] = 1;
            verts[2].tangents[1][2] = 0;
            verts[3].xyz[0] = x;
            verts[3].xyz[1] = y + h;
            verts[3].xyz[2] = 0;
            verts[3].st[0] = s1;
            verts[3].st[1] = t2;
            verts[3].normal[0] = 0;
            verts[3].normal[1] = 0;
            verts[3].normal[2] = 1;
            verts[3].tangents[0][0] = 1;
            verts[3].tangents[0][1] = 0;
            verts[3].tangents[0][2] = 0;
            verts[3].tangents[1][0] = 0;
            verts[3].tangents[1][1] = 1;
            verts[3].tangents[1][2] = 0;

            bool ident = !mat.IsIdentity;
            if (ident)
            {
                verts[0].xyz -= origin;
                verts[0].xyz *= mat;
                verts[0].xyz += origin;
                verts[1].xyz -= origin;
                verts[1].xyz *= mat;
                verts[1].xyz += origin;
                verts[2].xyz -= origin;
                verts[2].xyz *= mat;
                verts[2].xyz += origin;
                verts[3].xyz -= origin;
                verts[3].xyz *= mat;
                verts[3].xyz += origin;
            }

            renderSystem.DrawStretchPic(&verts[0], &indexes[0], 4, 6, shader, ident);
        }

        public void DrawMaterialRotated(float x, float y, float w, float h, Material mat, Vector4 color, float scalex = 1f, float scaley = 1f, float angle = 0f)
        {
            float s0, s1, t0, t1;

            renderSystem.SetColor(color);

            //  handle negative scales as well
            if (scalex < 0) { w *= -1; scalex *= -1; }
            if (scaley < 0) { h *= -1; scaley *= -1; }
            //
            if (w < 0) { w = -w; s0 = 1 * scalex; s1 = 0; } // flip about vertical
            else { s0 = 0; s1 = 1 * scalex; }

            if (h < 0) { h = -h; t0 = 1 * scaley; t1 = 0; } // flip about horizontal
            else { t0 = 0; t1 = 1 * scaley; }

            if (angle == 0f && ClippedCoords(ref x, ref y, ref w, ref h, ref s0, ref t0, ref s1, ref t1))
                return;

            AdjustCoords(ref x, ref y, ref w, ref h);

            DrawStretchPicRotated(x, y, w, h, s0, t0, s1, t1, mat, angle);
        }

        public void DrawStretchPicRotated(float x, float y, float w, float h, float s0, float t0, float s1, float t1, Material mat, float angle = 0f)
        {
            DrawVert[] verts[4];
            GlIndex[] indexes[6];
            indexes[0] = 3;
            indexes[1] = 0;
            indexes[2] = 2;
            indexes[3] = 2;
            indexes[4] = 0;
            indexes[5] = 1;
            verts[0].xyz[0] = x;
            verts[0].xyz[1] = y;
            verts[0].xyz[2] = 0;
            verts[0].st[0] = s1;
            verts[0].st[1] = t1;
            verts[0].normal[0] = 0;
            verts[0].normal[1] = 0;
            verts[0].normal[2] = 1;
            verts[0].tangents[0][0] = 1;
            verts[0].tangents[0][1] = 0;
            verts[0].tangents[0][2] = 0;
            verts[0].tangents[1][0] = 0;
            verts[0].tangents[1][1] = 1;
            verts[0].tangents[1][2] = 0;
            verts[1].xyz[0] = x + w;
            verts[1].xyz[1] = y;
            verts[1].xyz[2] = 0;
            verts[1].st[0] = s2;
            verts[1].st[1] = t1;
            verts[1].normal[0] = 0;
            verts[1].normal[1] = 0;
            verts[1].normal[2] = 1;
            verts[1].tangents[0][0] = 1;
            verts[1].tangents[0][1] = 0;
            verts[1].tangents[0][2] = 0;
            verts[1].tangents[1][0] = 0;
            verts[1].tangents[1][1] = 1;
            verts[1].tangents[1][2] = 0;
            verts[2].xyz[0] = x + w;
            verts[2].xyz[1] = y + h;
            verts[2].xyz[2] = 0;
            verts[2].st[0] = s2;
            verts[2].st[1] = t2;
            verts[2].normal[0] = 0;
            verts[2].normal[1] = 0;
            verts[2].normal[2] = 1;
            verts[2].tangents[0][0] = 1;
            verts[2].tangents[0][1] = 0;
            verts[2].tangents[0][2] = 0;
            verts[2].tangents[1][0] = 0;
            verts[2].tangents[1][1] = 1;
            verts[2].tangents[1][2] = 0;
            verts[3].xyz[0] = x;
            verts[3].xyz[1] = y + h;
            verts[3].xyz[2] = 0;
            verts[3].st[0] = s1;
            verts[3].st[1] = t2;
            verts[3].normal[0] = 0;
            verts[3].normal[1] = 0;
            verts[3].normal[2] = 1;
            verts[3].tangents[0][0] = 1;
            verts[3].tangents[0][1] = 0;
            verts[3].tangents[0][2] = 0;
            verts[3].tangents[1][0] = 0;
            verts[3].tangents[1][1] = 1;
            verts[3].tangents[1][2] = 0;

            bool ident = !mat.IsIdentity;
            if (ident)
            {
                verts[0].xyz -= origin;
                verts[0].xyz *= mat;
                verts[0].xyz += origin;
                verts[1].xyz -= origin;
                verts[1].xyz *= mat;
                verts[1].xyz += origin;
                verts[2].xyz -= origin;
                verts[2].xyz *= mat;
                verts[2].xyz += origin;
                verts[3].xyz -= origin;
                verts[3].xyz *= mat;
                verts[3].xyz += origin;
            }

            //Generate a translation so we can translate to the center of the image rotate and draw
            Vector3 origTrans;
            origTrans.x = x + (w / 2);
            origTrans.y = y + (h / 2);
            origTrans.z = 0;


            //Rotate the verts about the z axis before drawing them
            Matrix4x4 rotz;
            rotz.Identity();
            float sinAng = MathX.Sin(angle);
            float cosAng = MathX.Cos(angle);
            rotz[0][0] = cosAng;
            rotz[0][1] = sinAng;
            rotz[1][0] = -sinAng;
            rotz[1][1] = cosAng;
            for (var i = 0; i < 4; i++)
            {
                //Translate to origin
                verts[i].xyz -= origTrans;

                //Rotate
                verts[i].xyz = rotz * verts[i].xyz;

                //Translate back
                verts[i].xyz += origTrans;
            }

            renderSystem.DrawStretchPic(&verts[0], &indexes[0], 4, 6, shader, (angle == 0.0) ? false : true);
        }

        public int CharWidth(char c, float scale)
        {
            GlyphInfo glyph;
            float useScale;
            SetFontByScale(scale);
            FontInfo font = useFont;
            useScale = scale * font.glyphScale;
            glyph = font.glyphs[(byte)c];
            return MathX.FtoiFast(glyph.xSkip * useScale);
        }
        
        public int TextWidth(string text, float scale, int limit)
        {
            int i, width;

            SetFontByScale(scale);
            GlyphInfo glyphs = useFont.glyphs;

            if (text == null)
                return 0;

            width = 0;
            if (limit > 0)
                for (i = 0; text[i] != '\0' && i < limit; i++)
                    if (StringX.IsColor(text + i)) i++;
                    else width += glyphs[((byte)text)[i]].xSkip;
            else
                for (i = 0; text[i] != '\0'; i++)
                    if (StringX.IsColor(text + i)) i++;
                    else width += glyphs[((byte)text)[i]].xSkip;
            return MathX.FtoiFast(scale * useFont.glyphScale * width);
        }

        public int TextHeight(string text, float scale, int limit)
        {
            int len, count;
            float max;
            GlyphInfo glyph;
            float useScale;
            string s = text;
            SetFontByScale(scale);
            FontInfo font = useFont;

            useScale = scale * font.glyphScale;
            max = 0;
            if (text)
            {
                len = strlen(text);
                if (limit > 0 && len > limit)
                {
                    len = limit;
                }

                count = 0;
                while (s && *s && count < len)
                {
                    if (StringX.IsColor(s))
                    {
                        s += 2;
                        continue;
                    }
                    else
                    {
                        glyph = &font.glyphs[(byte)s];
                        if (max < glyph.height)
                            max = glyph.height;

                        s++;
                        count++;
                    }
                }
            }

            return MathX.FtoiFast(max * useScale);
        }

        public int MaxCharHeight(float scale)
        {
            SetFontByScale(scale);
            var useScale = scale * useFont.glyphScale;
            return MathX.FtoiFast(activeFont.maxHeight * useScale);
        }

        public int MaxCharWidth(float scale)
        {
            SetFontByScale(scale);
            float useScale = scale * useFont.glyphScale;
            return idMath::FtoiFast(activeFont.maxWidth * useScale);
        }

        public int FindFont(string name)
        {
            int c = fonts.Num();
            for (int i = 0; i < c; i++)
            {
                if (idStr::Icmp(name, fonts[i].name) == 0)
                {
                    return i;
                }
            }

            // If the font was not found, try to register it
            idStr fileName = name;
            fileName.Replace("fonts", va("fonts/%s", fontLang.c_str()));

            fontInfoEx_t fontInfo;
            int index = fonts.Append(fontInfo);
            if (renderSystem.RegisterFont(fileName, fonts[index]))
            {
                idStr::Copynz(fonts[index].name, name, sizeof(fonts[index].name) );
                return index;
            }
            else
            {
                common.Printf("Could not register font %s [%s]\n", name, fileName.c_str());
                return -1;
            }
        }
        public void SetupFonts()
        {
            fonts.SetGranularity(1);

            fontLang = cvarSystem.GetCVarString("sys_lang");

            // western european languages can use the english font
            if (fontLang == "french" || fontLang == "german" || fontLang == "spanish" || fontLang == "italian")
            {
                fontLang = "english";
            }

            // Default font has to be added first
            FindFont("fonts");
        }

        // this only supports left aligned text
        public Region GetTextRegion(string text, float textScale, Rectangle rectDraw, float xStart, float yStart)
            => null;

        public void SetSize(float width, float height)
        {
            vidWidth = VIRTUAL_WIDTH;
            vidHeight = VIRTUAL_HEIGHT;
            xScale = yScale = 0f;
            if (width != 0f && height != 0f)
            {
                xScale = vidWidth * (1f / width);
                yScale = vidHeight * (1f / height);
            }
        }

        public Material GetScrollBarImage(int index)
        {
            if (index >= SCROLLBAR_HBACK && index < SCROLLBAR_COUNT)
            {
                return scrollBarImages[index];
            }
            return scrollBarImages[SCROLLBAR_HBACK];
        }

        public void DrawCursor(ref float x, ref float y, float size)
        {
            if (*x < 0)
            {
                *x = 0;
            }

            if (*x >= vidWidth)
            {
                *x = vidWidth;
            }

            if (*y < 0)
            {
                *y = 0;
            }

            if (*y >= vidHeight)
            {
                *y = vidHeight;
            }

            renderSystem.SetColor(colorWhite);

            // DG: I use this instead of plain AdjustCursorCoords and the following lines
            //     to scale menus and other fullscreen GUIs to 4:3 aspect ratio
            AdjustCursorCoords(ref x, ref y, ref size, ref size);
            float sizeW = size * fixScaleForMenu.x;
            float sizeH = size * fixScaleForMenu.y;
            float fixedX = x * fixScaleForMenu.x + fixOffsetForMenu.x;
            float fixedY = y * fixScaleForMenu.y + fixOffsetForMenu.y;

            DrawStretchPic(fixedX, fixedY, sizeW, sizeH, 0, 0, 1, 1, cursorImages[(int)cursor]);
        }

        public void SetCursor(CURSOR n)
            => cursor = (n < CURSOR.ARROW || n >= CURSOR.COUNT) ? CURSOR.ARROW : n;

        public void AdjustCoords(ref float x, ref float y, ref float w, ref float h)
        {
            if (x)
            {
                *x *= xScale;

                *x *= fixScaleForMenu.x; // DG: for "render menus as 4:3" hack
                *x += fixOffsetForMenu.x;
            }
            if (y)
            {
                *y *= yScale;

                *y *= fixScaleForMenu.y; // DG: for "render menus as 4:3" hack
                *y += fixOffsetForMenu.y;
            }
            if (w)
            {
                *w *= xScale;

                *w *= fixScaleForMenu.x; // DG: for "render menus as 4:3" hack
            }
            if (h)
            {
                *h *= yScale;

                *h *= fixScaleForMenu.y; // DG: for "render menus as 4:3" hack
            }
        }

        // DG: same as AdjustCoords, but ignore fixupMenus because for the cursor that must be handled seperately
        // DG: added for "render menus as 4:3" hack
        public void AdjustCursorCoords(ref float x, ref float y, ref float w, ref float h)
        {
            x *= xScale;
            y *= yScale;
            w *= xScale;
            h *= yScale;
        }

        //public bool ClippedCoords(ref float x, ref float y, ref float w, ref float h);
        public bool ClippedCoords(ref float x, ref float y, ref float w, ref float h, ref float s1, ref float t1, ref float s2, ref float t2)
        {
            if (enableClipping == false || clipRects.Count == 0)
                return false;

            var c = clipRects.Count;
            while (--c > 0)
            {
                var clipRect = clipRects[c];

                var ox = x;
                var oy = y;
                var ow = w;
                var oh = h;

                if (ow <= 0f || oh <= 0f)
                    break;

                if (x < clipRect.x)
                {
                    w -= clipRect.x - x;
                    x = clipRect.x;
                }
                else if (x > clipRect.x + clipRect.w)
                    x = w = y = h = 0;
                if (y < clipRect.y)
                {
                    h -= clipRect.y - y;
                    y = clipRect.y;
                }
                else if (y > clipRect.y + clipRect.h)
                    x = w = y = h = 0;
                if (w > clipRect.w)
                    w = clipRect.w - x + clipRect.x;
                else if (x + w > clipRect.x + clipRect.w)
                    w = clipRect.Right - x;
                if (h > clipRect.h)
                    h = clipRect.h - y + clipRect.y;
                else if (y + h > clipRect.y + clipRect.h)
                    h = clipRect.Bottom - y;

                if (s1 != 0f && s2 != 0f && t1 != 0f && t2 != 0f && ow > 0f)
                {
                    float ns1, ns2, nt1, nt2;
                    // upper left
                    float u = (x - ox) / ow;
                    ns1 = s1 * (1f - u) + s2 * (u);

                    // upper right
                    u = (x + w - ox) / ow;
                    ns2 = s1 * (1f - u) + s2 * (u);

                    // lower left
                    u = (y - oy) / oh;
                    nt1 = t1 * (1f - u) + t2 * (u);

                    // lower right
                    u = (y + h - oy) / oh;
                    nt2 = t1 * (1f - u) + t2 * (u);

                    // set values
                    s1 = ns1;
                    s2 = ns2;
                    t1 = nt1;
                    t2 = nt2;
                }
            }

            return w == 0f || h == 0f;
        }

        public void PushClipRect(float x, float y, float w, float h)
            => clipRects.Add(new Rectangle(x, y, w, h));
        public void PushClipRect(Rectangle r)
            => clipRects.Add(r);
        public void PopClipRect()
        {
            if (clipRects.Count != 0)
                clipRects.RemoveAt(clipRects.Count - 1);
        }

        public void EnableClipping(bool b) => enableClipping = b;
        public void SetFont(int num)
        {
            if (num >= 0 && num < fonts.Num())
            {
                activeFont = &fonts[num];
            }
            else
            {
                activeFont = &fonts[0];
            }
        }

        public bool OverStrike
        {
            get => overStrikeMode;
            set => overStrikeMode = value;
        }

        public void DrawEditCursor(float x, float y, float scale)
        {
            if ((int)(com_ticNumber >> 4) & 1)
            {
                return;
            }
            SetFontByScale(scale);
            float useScale = scale * useFont.glyphScale;
            const glyphInfo_t* glyph2 = &useFont.glyphs[overStrikeMode ? int('_') : int('|')];
            float yadj = useScale * glyph2.top;
            PaintChar(x, y - yadj, glyph2.imageWidth, glyph2.imageHeight, useScale, glyph2.s, glyph2.t, glyph2.s2, glyph2.t2, glyph2.glyph);
        }


        // DG: this is used for the "make sure menus are rendered as 4:3" hack
        public void SetMenuScaleFix(bool enable)
        {
            if (enable)
            {
                float w = renderSystem.GetScreenWidth();
                float h = renderSystem.GetScreenHeight();
                float aspectRatio = w / h;
                static const float virtualAspectRatio = float(VIRTUAL_WIDTH) / float(VIRTUAL_HEIGHT); // 4:3
                if (aspectRatio > 1.4f)
                {
                    // widescreen (4:3 is 1.333 3:2 is 1.5, 16:10 is 1.6, 16:9 is 1.7778)
                    // => we need to scale and offset X
                    // All the coordinates here assume 640x480 (VIRTUAL_WIDTH x VIRTUAL_HEIGHT)
                    // screensize, so to fit a 4:3 menu into 640x480 stretched to a widescreen,
                    // we need do decrease the width to something smaller than 640 and center
                    // the result with an offset
                    float scaleX = virtualAspectRatio / aspectRatio;
                    float offsetX = (1f - scaleX) * (VIRTUAL_WIDTH * 0.5f); // (640 - scale*640)/2
                    fixScaleForMenu.Set(scaleX, 1);
                    fixOffsetForMenu.Set(offsetX, 0);
                }
                else if (aspectRatio < 1.24f)
                {
                    // portrait-mode, "thinner" than 5:4 (which is 1.25)
                    // => we need to scale and offset Y
                    // it's analogue to the other case, but inverted and with height and Y
                    float scaleY = aspectRatio / virtualAspectRatio;
                    float offsetY = (1f - scaleY) * (VIRTUAL_HEIGHT * 0.5f); // (480 - scale*480)/2
                    fixScaleForMenu.Set(1, scaleY);
                    fixOffsetForMenu.Set(0, offsetY);
                }
            }
            else
            {
                fixScaleForMenu.Set(1, 1);
                fixOffsetForMenu.Set(0, 0);
            }
        }
        public bool IsMenuScaleFixActive => fixOffsetForMenu.x != 0f || fixOffsetForMenu.y != 0f;

        int DrawText(float x, float y, float scale, Vector4 color, string text, float adjust, int limit, int style, int cursor = -1)
        {
            int len, count;
            new Vector4 newColor;
            const glyphInfo_t* glyph;
            float useScale;
            SetFontByScale(scale);
            useScale = scale * useFont.glyphScale;
            count = 0;
            if (text && color.w != 0f)
            {
                const unsigned char* s = (const unsigned char*)text;
                renderSystem.SetColor(color);
                memcpy(&newColor[0], &color[0], sizeof(new Vector4));
                len = strlen(text);
                if (limit > 0 && len > limit)
                {
                    len = limit;
                }

                while (s && *s && count < len)
                {
                    if (*s < GLYPH_START || *s > GLYPH_END)
                    {
                        s++;
                        continue;
                    }
                    glyph = &useFont.glyphs[*s];

                    //
                    // int yadj = Assets.textFont.glyphs[text[i]].bottom +
                    // Assets.textFont.glyphs[text[i]].top; float yadj = scale *
                    // (Assets.textFont.glyphs[text[i]].imageHeight -
                    // Assets.textFont.glyphs[text[i]].height);
                    //
                    if (idStr::IsColor((const char*)s) ) {
                        if (*(s + 1) == C_COLOR_DEFAULT)
                        {
                            newColor = color;
                        }
                        else
                        {
                            newColor = idStr::ColorForIndex(*(s + 1));
                            newColor[3] = color[3];
                        }
                        if (cursor == count || cursor == count + 1)
                        {
                            float partialSkip = ((glyph.xSkip * useScale) + adjust) / 5f;
                            if (cursor == count)
                            {
                                partialSkip *= 2f;
                            }
                            else
                            {
                                renderSystem.SetColor(newColor);
                            }
                            DrawEditCursor(x - partialSkip, y, scale);
                        }
                        renderSystem.SetColor(newColor);
                        s += 2;
                        count += 2;
                        continue;
                    } else
                    {
                        float yadj = useScale * glyph.top;
                        PaintChar(x, y - yadj, glyph.imageWidth, glyph.imageHeight, useScale, glyph.s, glyph.t, glyph.s2, glyph.t2, glyph.glyph);

                        if (cursor == count)
                        {
                            DrawEditCursor(x, y, scale);
                        }
                        x += (glyph.xSkip * useScale) + adjust;
                        s++;
                        count++;
                    }
                }
                if (cursor == len)
                {
                    DrawEditCursor(x, y, scale);
                }
            }
            return count;
        }

        void PaintChar(float x, float y, float width, float height, float scale, float s, float t, float s2, float t2, Material hShader)
        {
            var w = width * scale;
            var h = height * scale;

            if (ClippedCoords(out x, out y, out w, out h, out s, out t, out s2, out t2))
                return;

            AdjustCoords(out x, out y, out w, out h);
            DrawStretchPic(x, y, w, h, s, t, s2, t2, hShader);
        }

        void SetFontByScale(float scale)
        {
            if (scale <= gui_smallFontLimit.Float)
            {
                useFont = activeFont.fontInfoSmall;
                activeFont.maxHeight = activeFont.maxHeightSmall;
                activeFont.maxWidth = activeFont.maxWidthSmall;
            }
            else if (scale <= gui_mediumFontLimit.Float)
            {
                useFont = activeFont.fontInfoMedium;
                activeFont.maxHeight = activeFont.maxHeightMedium;
                activeFont.maxWidth = activeFont.maxWidthMedium;
            }
            else
            {
                useFont = activeFont.fontInfoLarge;
                activeFont.maxHeight = activeFont.maxHeightLarge;
                activeFont.maxWidth = activeFont.maxWidthLarge;
            }
        }

        void Clear()
        {
            initialized = false;
            useFont = null;
            activeFont = null;
            mbcs = false;
        }

        static int index = 0;
        static string[] str = new string[48];

        string String()
        {
            // use an array so that multiple toString's won't collide
            var s = str[index] = $"{x:2} {y:2} {w:2} {h:2}";
            index = (index + 1) & 7;
            return s;
        }
    }
}