#define EFX_VERBOSE
using Gengine.NumericsX.Core;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.NumericsX;
using static Gengine.NumericsX.Lib;
using static Gengine.Sound.Lib;

namespace Gengine.Sound
{
    class SoundEffect
    {
        public string name;
        public int effect;

        public SoundEffect() => effect = 0;
        public void Dispose()
        {
            if (soundSystemLocal.alIsEffect(effect))
                soundSystemLocal.alDeleteEffects(1, effect);
        }

        public bool Alloc()
        {
            AL.GetError();

            soundSystemLocal.alGenEffects(1, effect);
            var e = AL.GetError();
            if (e != ALError.NoError)
            {
                common.Warning($"SoundEffect::alloc: alGenEffects failed: 0x{e}");
                return false;
            }

            soundSystemLocal.alEffecti(effect, AL_EFFECT_TYPE, AL_EFFECT_EAXREVERB);
            e = AL.GetError();
            if (e != ALError.NoError)
            {
                common.Warning($"SoundEffect::alloc: alEffecti failed: 0x{e}");
                return false;
            }
            return true;
        }
    }

    public class EFXFile
    {
        //static float mB_to_gain(float millibels, property) _mB_to_gain(millibels,AL_EAXREVERB_MIN_ ## property, AL_EAXREVERB_MAX_ ## property)
        static float _mB_to_gain(float millibels, float min, float max)
            => MathX.ClampFloat(min, max, MathX.Pow(10f, millibels / 2000f));

        List<SoundEffect> effects = new();

        public EFXFile() => throw new NotImplementedException();
        public void Dispose() => Clear();

        public bool FindEffect(string name, out int effect)
        {
            for (var i = 0; i < effects.Count; i++)
                if (effects[i].name == name)
                {
                    effect = effects[i].effect;
                    return true;
                }
            effect = default;
            return false;
        }

        bool LoadFile(string filename, bool OSPath = false)
        {
            var src = new Lexer(LEXFL.NOSTRINGCONCAT);

            src.LoadFile(filename, OSPath);
            if (!src.IsLoaded) return false;
            if (!src.ExpectTokenString("Version")) return false;
            if (src.ParseInt() != 1) { src.Error("EFXFile::LoadFile: Unknown file version"); return false; }

            while (!src.EndOfFile())
            {
                var effect = new SoundEffect();
                if (!effect.Alloc())
                {
                    Clear();
                    return false;
                }

                if (ReadEffect(src, effect))
                    effects.Add(effect);
            }

            return true;
        }

        void Clear() => effects.Clear();

        bool ReadEffect(Lexer src, SoundEffect effect)
        {
            void efxi(string paramName, object param, int v)
            {
                do
                {
                    EFXprintf($"alEffecti({paramName}, {v})\n");
                    soundSystemLocal.alEffecti(effect.effect, param, v);
                    var err = AL.GetError();
                    if (err != ALError.NoError)
                        common.Warning($"alEffecti({paramName}, {v}) failed: 0x{err}");
                } while (false);
            }

            void efxf(string paramName, object param, float v)
            {
                do
                {
                    EFXprintf($"alEffectf({paramName}, {v:.3})\n");
                    soundSystemLocal.alEffectf(effect.effect, param, v);
                    var err = AL.GetError();
                    if (err != ALError.NoError)
                        common.Warning($"alEffectf({paramName}, {v:.3}) failed: 0x{err}");
                } while (false);
            }

            void efxfv(string paramName, object param, float value0, float value1, float value2)
            {
                do
                {
                    var v = new[] { value0, value1, value2 };
                    EFXprintf($"alEffectfv({paramName}, {v[0]:.3}, {v[1]:.3}, {v[2]:.3})\n");
                    soundSystemLocal.alEffectfv(effect.effect, param, v);
                    var err = AL.GetError();
                    if (err != ALError.NoError)
                        common.Warning($"alEffectfv({paramName}, {v[0]:.3}, {v[1]:.3}, {v[2]:.3}) failed: 0x{err}");
                } while (false);
            }

            if (!src.ReadToken(out var token))
                return false;

            // reverb effect
            if (token != "reverb")
            {
                // other effect (not supported at the moment)
                src.Error("EFXFile::ReadEffect: Unknown effect definition");
                return false;
            }

            src.ReadTokenOnLine(out token);
            var name = token;

            if (!src.ReadToken(out token))
                return false;

            if (token != "{")
            {
                src.Error($"EFXFile::ReadEffect: {{ not found, found {token}");
                return false;
            }

            AL.GetError();
            EFXprintf($"Loading EFX effect '{name}' (#{effect.effect})\n");

            do
            {
                if (!src.ReadToken(out token)) { src.Error("EFXFile::ReadEffect: EOF without closing brace"); return false; }
                if (token == "}") { effect.name = name; break; }
                if (token == "environment") src.ParseInt(); // the "environment" token should be ignored (efx has nothing equatable to it)
                else if (token == "environment size") { var size = src.ParseFloat(); efxf(AL_EAXREVERB_DENSITY, size < 2f ? size - 1f : 1f); }
                else if (token == "environment diffusion") efxf(AL_EAXREVERB_DIFFUSION, src.ParseFloat());
                else if (token == "room") efxf(AL_EAXREVERB_GAIN, mB_to_gain(src.ParseInt(), GAIN));
                else if (token == "room hf") efxf(AL_EAXREVERB_GAINHF, mB_to_gain(src.ParseInt(), GAINHF));
                else if (token == "room lf") efxf(AL_EAXREVERB_GAINLF, mB_to_gain(src.ParseInt(), GAINLF));
                else if (token == "decay time") efxf(AL_EAXREVERB_DECAY_TIME, src.ParseFloat());
                else if (token == "decay hf ratio") efxf(AL_EAXREVERB_DECAY_HFRATIO, src.ParseFloat());
                else if (token == "decay lf ratio") efxf(AL_EAXREVERB_DECAY_LFRATIO, src.ParseFloat());
                else if (token == "reflections") efxf(AL_EAXREVERB_REFLECTIONS_GAIN, mB_to_gain(src.ParseInt(), REFLECTIONS_GAIN));
                else if (token == "reflections delay") efxf(AL_EAXREVERB_REFLECTIONS_DELAY, src.ParseFloat());
                else if (token == "reflections pan") efxfv(AL_EAXREVERB_REFLECTIONS_PAN, src.ParseFloat(), src.ParseFloat(), src.ParseFloat());
                else if (token == "reverb") efxf(AL_EAXREVERB_LATE_REVERB_GAIN, mB_to_gain(src.ParseInt(), LATE_REVERB_GAIN));
                else if (token == "reverb delay") efxf(AL_EAXREVERB_LATE_REVERB_DELAY, src.ParseFloat());
                else if (token == "reverb pan") efxfv(AL_EAXREVERB_LATE_REVERB_PAN, src.ParseFloat(), src.ParseFloat(), src.ParseFloat());
                else if (token == "echo time") efxf(AL_EAXREVERB_ECHO_TIME, src.ParseFloat());
                else if (token == "echo depth") efxf(AL_EAXREVERB_ECHO_DEPTH, src.ParseFloat());
                else if (token == "modulation time") efxf(AL_EAXREVERB_MODULATION_TIME, src.ParseFloat());
                else if (token == "modulation depth") efxf(AL_EAXREVERB_MODULATION_DEPTH, src.ParseFloat());
                else if (token == "air absorption hf") efxf(AL_EAXREVERB_AIR_ABSORPTION_GAINHF, mB_to_gain(src.ParseFloat(), AIR_ABSORPTION_GAINHF));
                else if (token == "hf reference") efxf(AL_EAXREVERB_HFREFERENCE, src.ParseFloat());
                else if (token == "lf reference") efxf(AL_EAXREVERB_LFREFERENCE, src.ParseFloat());
                else if (token == "room rolloff factor") efxf(AL_EAXREVERB_ROOM_ROLLOFF_FACTOR, src.ParseFloat());
                else if (token == "flags") { src.ReadTokenOnLine(out token); var flags = token.UnsignedIntValue; efxi(AL_EAXREVERB_DECAY_HFLIMIT, (flags & 0x20) != 0 ? AL_TRUE : AL_FALSE); } // the other SCALE flags have no equivalent in efx
                else { src.ReadTokenOnLine(out _); src.Error("EFXFile::ReadEffect: Invalid parameter in reverb definition"); }
            } while (true);

            return true;
        }

#if EFX_VERBOSE
        public static void EFXprintf(string fmt, params object[] args) => common.Printf(fmt, args);
#else
		public static void EFXprintf(string fmt, params object[] args) { }
#endif
    }
}
