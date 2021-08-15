using Gengine.Render;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.NumericsX;
using System.NumericsX.Core;
using static Gengine.Lib;

namespace Gengine.UI
{
    public abstract class WinVar
    {
        const string VAR_GUIPREFIX = "gui::";

        protected Dictionary<string, string> guiDict;
        protected string name;
        protected bool eval;

        public WinVar(WinVar other)
        {
            guiDict = other.guiDict;
            Name = other.name;
        }
        public WinVar()
        {
            guiDict = null;
            name = null;
            eval = true;
        }

        public void SetGuiInfo(Dictionary<string, string> gd, string name)
        {
            guiDict = gd;
            Name = name;
        }

        public string Name
        {
            get
            {
                if (name != null)
                    return guiDict != null && name[0] == '*'
                        ? guiDict.GetString(name[1])
                        : name;
                return string.Empty;
            }
            set => name = value;
        }

        public Dictionary<string, string> Dict => guiDict;
        public bool NeedsUpdate => guiDict != null;

        public virtual void Init(string name, Window win)
        {
            var key = name;
            guiDict = null;
            var len = key.Length;
            if (len > 5 && key[0] == 'g' && key[1] == 'u' && key[2] == 'i' && key[3] == ':')
            {
                key = key.Right(len - VAR_GUIPREFIX.Length);
                SetGuiInfo(win.Gui.StateDict, key);
                win.AddUpdateVar(this);
            }
            else Set(name);
        }

        public abstract void Set(string val);

        public abstract void Update();

        public virtual int Size => 0;

        public abstract void WriteToSaveGame(VFile savefile);
        public abstract void ReadFromSaveGame(VFile savefile);

        public abstract float x { get; }

        public bool Eval
        {
            get => eval;
            set => eval = value;
        }
    }

    public class WinBool : WinVar
    {
        protected bool data;

        public WinBool(WinBool other)
        {
            data = other;
            guiDict?.SetBool(Name, data);
        }
        public WinBool() : base() { }

        public override float x => data ? 1f : 0f;

        public static bool operator ==(WinBool _, bool a) => _.data == a;
        public static bool operator !=(WinBool _, bool a) => _.data != a;
        public static implicit operator WinBool(bool s) => new(s);
        public static implicit operator bool(WinBool t) => t.data;
        public override string ToString() => $"{data}";

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetBool(Name);
        }

        public override void Set(string val)
        {
            data = int.Parse(val) != 0;
            guiDict?.SetBool(Name, data);
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetBool(s);
        }

        // SaveGames
        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval, sizeof(bool));
            savefile.Write(data, sizeof(bool));
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(eval, sizeof(bool));
            savefile.Read(data, sizeof(bool));
        }
    }

    public class WinStr : WinVar
    {
        protected string data;

        public WinStr(string other)
        {
            data = other;
            guiDict?.Set(Name, data);
        }
        public WinStr(WinStr other) : base(other)
            => data = other.data;
        public WinStr() : base() { }
        public override int Size => 0;

        public static bool operator ==(WinStr _, string a) => _.data == a;
        public static bool operator !=(WinStr _, string a) => _.data != a;
        public static implicit operator WinStr(string s) => new(s);
        public static implicit operator string(WinStr t) => t.data;
        public override string ToString() => data;

        // return whether string is emtpy
        public override float x => data.Length > 0 ? 1f : 0f;

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetString(Name);
        }

        public int LengthWithoutColors
        {
            get
            {
                if (guiDict != null && !string.IsNullOrEmpty(name))
                    data = guiDict.GetString(Name);
                return data.LengthWithoutColors;
            }
        }
        public int Length
        {
            get
            {
                if (guiDict != null && !string.IsNullOrEmpty(name))
                    data = guiDict.GetString(Name);
                return data.Length;
            }
        }

        public void RemoveColors()
        {
            if (guiDict != null && !string.IsNullOrEmpty(name))
                data = guiDict.GetString(Name);
            data.RemoveColors();
        }

        public override void Set(string val)
        {
            data = val;
            guiDict?.Set(Name, data);
        }

        public override void Update()
        {
            var s = Name();
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetString(s);
        }

        // SaveGames
        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval);

            var len = data.Length;
            savefile.Write(len);
            if (len > 0)
                savefile.WriteASCII(data, len);
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(out eval);

            savefile.Read(out int len);
            if (len > 0)
                savefile.ReadASCII(out data, len);
        }
    }

    public class WinInt : WinVar
    {
        protected int data;

        public WinInt(int other)
        {
            data = other;
            guiDict?.SetInt(Name, data);
        }
        public WinInt(WinInt other) : base()
            => data = other.data;
        public WinInt() : base() { }

        public static bool operator ==(WinInt _, int a) => _.data == a;
        public static bool operator !=(WinInt _, int a) => _.data != a;
        public static implicit operator WinInt(int s) => new(s);
        public static implicit operator int(WinInt t) => t.data;
        public override string ToString() => $"{data}";

        // no suitable conversion
        public override float x { get { Debug.Assert(false); return 0f; } }

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetInt(Name);
        }

        public override void Set(string val)
        {
            data = int.Parse(val);
            guiDict?.SetInt(Name, data);
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetInt(s);
        }

        // SaveGames
        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval);
            savefile.Write(data);
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(out eval);
            savefile.Read(out data);
        }
    }

    public class WinFloat : WinVar
    {
        protected float data;

        public WinFloat(WinFloat other) : base(other)
            => data = other.data;
        public WinFloat(float other)
        {
            data = other;
            guiDict?.SetFloat(Name, data);
        }
        public WinFloat() : base() { }

        public static bool operator ==(WinFloat _, float a) => _.data == a;
        public static bool operator !=(WinFloat _, float a) => _.data != a;
        public static implicit operator WinFloat(float s) => new(s);
        public static implicit operator float(WinFloat t) => t.data;
        public override string ToString() => $"{data}";

        public override float x => data;

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetFloat(Name);
        }

        public override void Set(string val)
        {
            data = int.Parse(val);
            guiDict?.SetFloat(Name, data);
        }
        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetFloat(s);
        }

        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval, sizeof(bool));
            savefile.Write(data, sizeof(float));
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(eval, sizeof(bool));
            savefile.Read(data, sizeof(float));
        }
    }

    public class WinRectangle : WinVar
    {
        protected Rectangle data;

        public WinRectangle(WinRectangle other) : base(other)
            => data = other.data;
        public WinRectangle(Vector4 other)
        {
            data = other;
            guiDict?.SetVec4(Name, other);
        }
        public WinRectangle(Rectangle other)
        {
            data = other;
            guiDict?.SetVec4(Name, data.ToVec4);
        }
        public WinRectangle() : base() { }

        public static bool operator ==(WinRectangle _, Rectangle a) => _.data == a;
        public static bool operator !=(WinRectangle _, Rectangle a) => _.data != a;
        public static implicit operator WinRectangle(Vector4 s) => new(s);
        public static implicit operator WinRectangle(Rectangle s) => new(s);
        public static implicit operator Rectangle(WinRectangle t) => t.data;
        public override string ToString() => $"{data.ToVec4()}";

        public float x => data.x;
        public float y => data.y;
        public float w => data.w;
        public float h => data.h;
        public float Right => data.Right;
        public float Bottom => data.Bottom;
        public Vector4 ToVec4 => data.ToVec4();

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
            {
                var v = guiDict.GetVec4(Name);
                data.x = v.x;
                data.y = v.y;
                data.w = v.z;
                data.h = v.w;
            }
        }

        public override void Set(string val)
        {
            if (strchr(val, ',')) sscanf(val, "%f,%f,%f,%f", data.x, data.y, data.w, data.h);
            else sscanf(val, "%f %f %f %f", data.x, data.y, data.w, data.h);
            guiDict?.SetVec4(Name, data.ToVec4);
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
            {
                var v = guiDict.GetVec4(s);
                data.x = v.x;
                data.y = v.y;
                data.w = v.z;
                data.h = v.w;
            }
        }

        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval, sizeof(bool));
            savefile.Write(data, sizeof(Rectangle));
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(eval, sizeof(bool));
            savefile.Read(data, sizeof(Rectangle));
        }
    }

    public class WinVec2 : WinVar
    {
        protected Vector2 data;

        public WinVec2(WinVec2 other) : base(other)
            => data = other.data;
        public WinVec2(Vector2 other)
        {
            data = other;
            guiDict?.SetVec2(Name, data);
        }
        public WinVec2() : base() { }

        public static bool operator ==(WinVec2 _, Vector2 a) => _.data == a;
        public static bool operator !=(WinVec2 _, Vector2 a) => _.data != a;
        public static implicit operator WinVec2(Vector2 s) => new(s);
        public static implicit operator Vector2(WinVec2 t) => t.data;
        public override string ToString() => $"{data}";

        public float x => data.x;
        public float y => data.y;

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetVec2(Name);
        }

        public override void Set(string val)
        {
            if (strchr(val, ',')) sscanf(val, "%f,%f", data.x, data.y);
            else sscanf(val, "%f %f", data.x, data.y);
            guiDict?.SetVec2(Name, data);
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetVec2(s);
        }
        public void Zero()
            => data.Zero();

        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval, sizeof(bool));
            savefile.Write(data, sizeof(Vector2));
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(eval, sizeof(bool));
            savefile.Read(data, sizeof(Vector2));
        }
    }

    public class WinVec3 : WinVar
    {
        protected Vector3 data;

        public WinVec3(WinVec3 other) : base(other)
            => data = other.data;
        public WinVec3(Vector3 other)
        {
            data = other;
            guiDict?.SetVector(Name, data);
        }
        public WinVec3() : base() { }

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetVector(Name);
        }

        public static bool operator ==(WinVec3 _, Vector3 a) => _.data == a;
        public static bool operator !=(WinVec3 _, Vector3 a) => _.data != a;
        public static implicit operator WinVec3(Vector3 s) => new(s);
        public static implicit operator Vector3(WinVec3 t) => t.data;
        public override string ToString() => $"{data}";

        public float x => data.x;
        public float y => data.y;
        public float z => data.z;

        public override void Set(string val)
        {
            sscanf(val, "%f %f %f", data.x, data.y, data.z);
            guiDict?.SetVector(Name, data);
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetVector(s);
        }

        public void Zero()
        {
            data.Zero();
            if (guiDict != null)
                guiDict.SetVector(Name, data);
        }

        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval);
            savefile.Write(data, sizeof(Vector3));
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(out eval);
            savefile.Read(data, sizeof(Vector3));
        }
    }

    public class WinVec4 : WinVar
    {
        protected Vector4 data;

        public WinVec4(WinVec4 other) : base(other)
            => data = other.data;
        public WinVec4(Vector4 other)
        {
            data = other;
            guiDict?.SetVec4(Name, data);
        }
        public WinVec4() : base() { }

        public static bool operator ==(WinVec4 _, Vector4 a) => _.data == a;
        public static bool operator !=(WinVec4 _, Vector4 a) => _.data != a;
        public static implicit operator WinVec4(Vector4 s) => new(s);
        public static implicit operator Vector4(WinVec4 t) => t.data;
        public override string ToString() => $"{data}";

        public float x => data.x;
        public float y => data.y;
        public float z => data.z;
        public float w => data.w;

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetVec4(Name);
        }

        public override void Set(string val)
        {
            if (strchr(val, ',')) sscanf(val, "%f,%f,%f,%f", data.x, data.y, data.z, data.w);
            else sscanf(val, "%f %f %f %f", data.x, data.y, data.z, data.w);
            guiDict?.SetVec4(Name, data);
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && s.Length > 0)
                data = guiDict.GetVec4(s);
        }

        public void Zero()
        {
            data.Zero();
            guiDict?.SetVec4(Name, data);
        }

        public Vector3 ToVec3 => data.ToVec3();

        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval, sizeof(bool));
            savefile.Write(data, sizeof(Vector4));
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(eval, sizeof(eval));
            savefile.Read(data, sizeof(Vector4));
        }
    }

    public class WinBackground : WinStr
    {
        protected string data;
        protected Action<Material> mat;

        public WinBackground(string other)
        {
            data = other;
            guiDict?.Set(Name, data);
            mat?.Invoke(data == "" ? null : declManager.FindMaterial(data));
        }
        public WinBackground(WinBackground other) : base((WinStr)other)
        {
            data = other.data;
            mat = other.mat;
            mat?.Invoke(data == "" ? null : declManager.FindMaterial(data));
        }
        public WinBackground() : base()
            => mat = null;
        public override int Size => 0;

        public static bool operator ==(WinBackground _, string a) => _.data == a;
        public static bool operator !=(WinBackground _, string a) => _.data != a;
        public static implicit operator WinBackground(string s) => new(s);
        public static implicit operator string(WinBackground t) => t.data;
        public override string ToString() => $"{data}";

        public override void Init(string name, Window win)
        {
            base.Init(name, win);
            if (guiDict != null)
                data = guiDict.GetString(Name);
        }

        public int Length
        {
            get
            {
                if (guiDict != null)
                    data = guiDict.GetString(Name);
                return data.Length;
            }
        }

        public override void Set(string val)
        {
            data = val;
            guiDict?.Set(Name, data);
            mat?.Invoke(data == "" ? null : declManager.FindMaterial(data));
        }

        public override void Update()
        {
            var s = Name;
            if (guiDict != null && !string.IsNullOrEmpty(s))
            {
                data = guiDict.GetString(s);
                mat?.Invoke(data == "" ? null : declManager.FindMaterial(data));
            }
        }

        public void SetMaterialPtr(Action<Material> m)
            => mat = m;

        public override void WriteToSaveGame(VFile savefile)
        {
            savefile.Write(eval, sizeof(bool));

            var len = data.Length;
            savefile.Write(len, sizeof(int));
            if (len > 0)
                savefile.Write(data, len);
        }
        public override void ReadFromSaveGame(VFile savefile)
        {
            savefile.Read(eval, sizeof(eval));

            savefile.Read(out int len, sizeof(int));
            if (len > 0)
            {
                data.Fill(' ', len);
                savefile.Read(data[0], len);
            }
            mat?.Invoke(len > 0 ? declManager.FindMaterial(data) : null);
        }
    }

    // multiplexes access to a list if idWinVar
    public class MultiWinVar : List<WinVar>
    {
        public void Set(string val)
        {
            for (var i = 0; i < Count; i++)
                this[i].Set(val);
        }

        public void Update()
        {
            for (var i = 0; i < Count; i++)
                this[i].Update();
        }

        public void SetGuiInfo(Dictionary<string, string> dict)
        {
            for (var i = 0; i < Count; i++)
                this[i].SetGuiInfo(dict, this[i].ToString());
        }
    }
}