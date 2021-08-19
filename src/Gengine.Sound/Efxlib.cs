#define EFX_VERBOSE
using System;
using System.Collections.Generic;
using System.NumericsX.Core;
using ALuint = System.UInt32;
using static System.NumericsX.Lib;

namespace Gengine.Sound
{
	class SoundEffect
	{
		string name;
		ALuint effect;

		SoundEffect() => throw new NotImplementedException();

		bool alloc() => throw new NotImplementedException();
	}

	public class EFXFile
	{
		List<SoundEffect> effects = new();

		public EFXFile() => throw new NotImplementedException();

		bool FindEffect(string name, ALuint effect) => throw new NotImplementedException();
		bool LoadFile(string filename, bool OSPath = false) => throw new NotImplementedException();
		void Clear() => throw new NotImplementedException();

		bool ReadEffect(Lexer lexer, SoundEffect effect) => throw new NotImplementedException();

#if EFX_VERBOSE
		public static void EFXprintf(string fmt, params object[] args) => common.Printf(fmt, args);
#else
		public static void EFXprintf(string fmt, params object[] args) { }
#endif
	}
}