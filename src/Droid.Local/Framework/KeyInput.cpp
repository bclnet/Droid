




#define ID_DOOM_LEGACY

#ifdef ID_DOOM_LEGACY

static const char *		cheatCodes[] = {
        "iddqd",		// Invincibility
        "idkfa",		// All weapons, keys, ammo, and 200% armor
        "idfa",			// Reset ammunition
        "idspispopd",	// Walk through walls
        "idclip",		// Walk through walls
        "idchoppers",	// Chainsaw
/*
	"idbeholds",	// Berserker strength
	"idbeholdv",	// Temporary invincibility
	"idbeholdi",	// Temporary invisibility
	"idbeholda",	// Full automap
	"idbeholdr",	// Anti-radiation suit
	"idbeholdl",	// Light amplification visor
	"idclev",		// Level select
	"iddt",			// Toggle full map; full map and objects; normal map
	"idmypos",		// Display coordinates and heading
	"idmus",		// Change music to indicated level
	"fhhall",		// Kill all enemies in level
	"fhshh",		// Invisible to enemies until attack
*/
        NULL
};
char		lastKeys[32];
int			lastKeyIndex;

#endif


