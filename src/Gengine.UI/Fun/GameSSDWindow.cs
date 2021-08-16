using Gengine.Render;
using System;
using System.Collections.Generic;
using System.NumericsX;
using System.NumericsX.Core;
using System.NumericsX.Sys;
using static Gengine.Lib;
using static System.NumericsX.Core.Key;
using static System.NumericsX.Lib;

namespace Gengine.UI
{
    class SSDCrossHair
    {
        public enum CROSSHAIR
        {
            STANDARD = 0,
            SUPER,
            COUNT
        }
        public Material[] crosshairMaterial = new Material[(int)CROSSHAIR.COUNT];
        public int currentCrosshair;
        public float crosshairWidth, crosshairHeight;

        public SSDCrossHair();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile);

        public void InitCrosshairs();
        public void Draw(DeviceContext dc, Vector2 cursor);
    }

    enum SSD_ENTITY
    {
        BASE = 0,
        ASTEROID,
        ASTRONAUT,
        EXPLOSION,
        POINTS,
        PROJECTILE,
        POWERUP
    }

    class SSDEntity
    {
        //SSDEntity Information
        public int type;
        public int id;
        public string materialName;
        public Material material;
        public Vector3 position;
        public Vector2 size;
        public float radius;
        public float hitRadius;
        public float rotation;

        public Vector4 matColor;

        public string text;
        public float textScale;
        public Vector4 foreColor;

        public GameSSDWindow game;
        public int currentTime;
        public int lastUpdate;
        public int elapsed;

        public bool destroyed;
        public bool noHit;
        public bool noPlayerDamage;

        public bool inUse;

        public SSDEntity();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public void EntityInit();

        public void SetGame(idGameSSDWindow game);
        public void SetMaterial(string name);
        public void SetPosition(Vector3 _position);
        public void SetSize(Vector2 size);
        public void SetRadius(float radius, float hitFactor = 1f);
        public void SetRotation(float rotation);

        public void Update();
        public bool HitTest(Vector2 pt);

        public virtual void EntityUpdate() { }
        public virtual void Draw(DeviceContext dc);
        public virtual void DestroyEntity();

        public virtual void OnHit(int key) { }
        public virtual void OnStrikePlayer() { }

        public Bounds WorldToScreen(Bounds worldBounds);
        public Vector3 WorldToScreen(Vector3 worldPos);

        public Vector3 ScreenToWorld(Vector3 screenPos);
    }


    // SSDMover
    class SSDMover : SSDEntity
    {
        public Vector3 speed;
        public float rotationSpeed;

        public SSDMover();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public void MoverInit(Vector3 speed, float rotationSpeed);

        public virtual void EntityUpdate();
    }

    // SSDAsteroid
    class SSDAsteroid : SSDMover
    {
        public const int MAX_ASTEROIDS = 64;
        public int health;
        protected static SSDAsteroid[] asteroidPool = new SSDAsteroid[MAX_ASTEROIDS];

        public SSDAsteroid();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public void Init(GameSSDWindow game, Vector3 startPosition, Vector2 size, float speed, float rotate, int health);

        public virtual void EntityUpdate();
        public static SSDAsteroid GetNewAsteroid(GameSSDWindow game, Vector3 startPosition, Vector2 size, float speed, float rotate, int health);
        public static SSDAsteroid GetSpecificAsteroid(int id);
        public static void WriteAsteroids(VFile savefile);
        public static void ReadAsteroids(VFile savefile, GameSSDWindow game);

    }

    // SSDAstronaut
    class SSDAstronaut : SSDMover
    {
        public const int MAX_ASTRONAUT = 8;
        public int health;
        protected static SSDAstronaut[] astronautPool = new SSDAstronaut[MAX_ASTRONAUT];

        public SSDAstronaut();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public void Init(GameSSDWindow game, Vector3 startPosition, float speed, float rotate, int health);

        public static SSDAstronaut GetNewAstronaut(GameSSDWindow game, Vector3 startPosition, float speed, float rotate, int health);
        public static SSDAstronaut GetSpecificAstronaut(int id);
        public static void WriteAstronauts(VFile savefile);
        public static void ReadAstronauts(VFile savefile, GameSSDWindow game);
    }

    // SSDExplosion
    class SSDExplosion : SSDEntity
    {
        public enum EXPLOSION
        {
            NORMAL = 0,
            TELEPORT = 1
        }

        public const int MAX_EXPLOSIONS = 64;
        public Vector2 finalSize;
        public int length;
        public int beginTime;
        public int endTime;
        public int explosionType;

        //The entity that is exploding
        public SSDEntity buddy;
        public bool killBuddy;
        public bool followBuddy;

        protected static SSDExplosion[] explosionPool = new SSDExplosion[MAX_EXPLOSIONS];

        public SSDExplosion();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public void Init(GameSSDWindow game, Vector3 position, Vector2 size, int length, int type, SSDEntity buddy, bool killBuddy = true, bool followBuddy = true);

        public virtual void EntityUpdate();
        public static SSDExplosion GetNewExplosion(GameSSDWindow game, Vector3 position, Vector2 size, int length, int type, SSDEntity buddy, bool killBuddy = true, bool followBuddy = true);
        public static SSDExplosion GetSpecificExplosion(int id);
        public static void WriteExplosions(VFile savefile);
        public static void ReadExplosions(VFile savefile, GameSSDWindow game);
    }

    class SSDPoints : SSDEntity
    {
        public const int MAX_POINTS = 16;
        public int length;
        public int distance;
        public int beginTime;
        public int endTime;

        public Vector3 beginPosition;
        public Vector3 endPosition;

        public Vector4 beginColor;
        public Vector4 endColor;
        protected static SSDPoints[] pointsPool = new SSDPoints[MAX_POINTS];

        public SSDPoints();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public void Init(GameSSDWindow game, SSDEntity _ent, int points, int length, int distance, Vector4 color);
        public virtual void EntityUpdate();

        public static SSDPoints GetNewPoints(GameSSDWindow game, SSDEntity ent, int points, int length, int distance, Vector4 color);
        public static SSDPoints GetSpecificPoints(int id);
        public static void WritePoints(VFile savefile);
        public static void ReadPoints(VFile savefile, GameSSDWindow game);
    }


    class SSDProjectile : SSDEntity
    {
        public const int MAX_PROJECTILES = 64;

        public Vector3 dir;
        public Vector3 speed;
        public int beginTime;
        public int endTime;

        public Vector3 endPosition;


        public SSDProjectile();

        public virtual void WriteToSaveGame(idFile* savefile);
        public virtual void ReadFromSaveGame(idFile* savefile, idGameSSDWindow* _game);

        public void Init(idGameSSDWindow* _game, const Vector3& _beginPosition, const Vector3& _endPosition, float _speed, float _size);
        public virtual void EntityUpdate();

        public static SSDProjectile* GetNewProjectile(idGameSSDWindow* _game, const Vector3& _beginPosition, const Vector3& _endPosition, float _speed, float _size);
        public static SSDProjectile* GetSpecificProjectile(int id);
        public static void WriteProjectiles(idFile* savefile);
        public static void ReadProjectiles(idFile* savefile, idGameSSDWindow* _game);

        protected static SSDProjectile[] projectilePool = new SSDProjectile[MAX_PROJECTILES];
    }


    /**
    * Powerups work in two phases:
    *	1.) Closed container hurls at you If you shoot the container it open
    *	3.) If an opened powerup hits the player he aquires the powerup
    * Powerup Types:
    *	Health - Give a specific amount of health
    *	Super Blaster - Increases the power of the blaster (lasts a specific amount of time)
    *	Asteroid Nuke - Destroys all asteroids on screen as soon as it is aquired
    *	Rescue Powerup - Rescues all astronauts as soon as it is acquited
    *	Bonus Points - Gives some bonus points when acquired
*/
    class SSDPowerup : SSDMover
    {
        public const int MAX_POWERUPS = 64;

        enum POWERUP_STATE
        {
            CLOSED = 0,
            OPEN
        }

        enum POWERUP_TYPE
        {
            HEALTH = 0,
            SUPER_BLASTER,
            ASTEROID_NUKE,
            RESCUE_ALL,
            BONUS_POINTS,
            DAMAGE,
            MAX
        }

        public int powerupState;
        public int powerupType;
        protected static SSDPowerup[] powerupPool = new SSDPowerup[MAX_POWERUPS];

        public SSDPowerup();

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameSSDWindow game);

        public virtual void OnHit(int key);
        public virtual void OnStrikePlayer();

        public void OnOpenPowerup();
        public void OnActivatePowerup();

        public void Init(GameSSDWindow game, float speed, float rotation);

        public static SSDPowerup GetNewPowerup(VGameSSDWindow game, float speed, float rotation);
        public static SSDPowerup GetSpecificPowerup(int id);
        public static void WritePowerups(VFile savefile);
        public static void ReadPowerups(VFile savefile, GameSSDWindow game);
    }

    struct SSDLevelData
    {
        public float spawnBuffer;
        public int needToWin;
    }

    struct SSDAsteroidData
    {
        public float speedMin, speedMax;
        public float sizeMin, sizeMax;
        public float rotateMin, rotateMax;
        public int spawnMin, spawnMax;
        public int asteroidHealth;
        public int asteroidPoints;
        public int asteroidDamage;
    }

    struct SSDAstronautData
    {

        public float speedMin, speedMax;
        public float rotateMin, rotateMax;
        public int spawnMin, spawnMax;
        public int health;
        public int points;
        public int penalty;
    }

    struct SSDPowerupData
    {
        public float speedMin, speedMax;
        public float rotateMin, rotateMax;
        public int spawnMin, spawnMax;
    }

    struct SSDWeaponData
    {
        public float speed;
        public int damage;
        public int size;
    }

    // Data that is used for each level. This data is reset each new level.
    struct SSDLevelStats
    {
        public int shotCount;
        public int hitCount;
        public int destroyedAsteroids;
        public int nextAsteroidSpawnTime;

        public int killedAstronauts;
        public int savedAstronauts;

        // Astronaut Level Data
        public int nextAstronautSpawnTime;

        // Powerup Level Data
        public int nextPowerupSpawnTime;

        public SSDEntity targetEnt;
    }

    //	Data that is used for the game that is currently running. Memset this to completely reset the game
    struct SSDGameStats
    {
        public bool gameRunning;

        public int score;
        public int prebonusscore;

        public int health;

        public int currentWeapon;
        public int currentLevel;
        public int nextLevel;

        public SSDLevelStats levelStats;
    }

    class GameSSDWindow : Window
    {
        public static RandomX random = new();
        public int ssdTime;
        // WinVars used to call functions from the guis
        public WinBool beginLevel;
        public WinBool resetGame;
        public WinBool continueGame;
        public WinBool refreshGuiData;

        public SSDCrossHair crosshair;
        public Bounds screenBounds;

        // Level Data
        public int levelCount;
        public List<SSDLevelData> levelData = new();
        public List<SSDAsteroidData> asteroidData = new();
        public List<SSDAstronautData> astronautData = new();
        public List<SSDPowerupData> powerupData = new();

        // Weapon Data
        public int weaponCount;
        public List<SSDWeaponData> weaponData = new();

        public int superBlasterTimeout;

        // All current game data is stored in this structure (except the entity list)
        public SSDGameStats gameStats;
        public List<SSDEntity> entities = new();

        public int currentSound;

        protected override bool ParseInternalVar(string name, Parser src);
        void ParseLevelData(int level, string levelDataString);
        void ParseAsteroidData(int level, string asteroidDataString);
        void ParseWeaponData(int weapon, string weaponDataString);
        void ParseAstronautData(int level, string astronautDataString);
        void ParsePowerupData(int level, string powerupDataString);

        void CommonInit();

        public GameSSDWindow(UserInterfaceLocal gui);
        public GameSSDWindow(DeviceContext dc, UserInterfaceLocal gui);

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile);

        public virtual string HandleEvent(SysEvent ev, Action<bool> updateVisuals);
        public virtual WinVar GetWinVarByName(string name, bool winLookup = false, DrawWin owner = null);

        public virtual void Draw(int time, float x, float y);

        public void AddHealth(int health);
        public void AddScore(SSDEntity ent, int points);
        public void AddDamage(int damage);

        public void OnNuke();
        public void OnRescueAll();
        public void OnSuperBlaster();

        public SSDEntity GetSpecificEntity(int type, int id);

        public void PlaySound(string sound);

        void ResetGameStats();
        void ResetLevelStats();
        void ResetEntities();

        // Game Running Methods
        void StartGame();
        void StopGame();
        void GameOver();

        // Starting the Game
        void BeginLevel(int level);
        void ContinueGame();

        // Stopping the Game
        void LevelComplete();
        void GameComplete();

        void UpdateGame();
        void CheckForHits();
        void ZOrderEntities();

        void SpawnAsteroid();

        void FireWeapon(int key);
        SSDEntity EntityHitTest(Vector2 pt);

        void HitAsteroid(SSDAsteroid asteroid, int key);
        void AsteroidStruckPlayer(SSDAsteroid asteroid);

        void RefreshGuiData();

        Vector2 GetCursorWorld();

        // Astronaut Methods
        void SpawnAstronaut();
        void HitAstronaut(SSDAstronaut astronaut, int key);
        void AstronautStruckPlayer(SSDAstronaut astronaut);

        // Powerup Methods
        void SpawnPowerup();

        void StartSuperBlaster();
        void StopSuperBlaster();

        //void FreeSoundEmitter(bool immediate);
    }
}
