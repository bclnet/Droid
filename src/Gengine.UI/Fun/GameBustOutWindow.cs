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
    enum POWERUP
    {
        NONE = 0,
        BIGPADDLE,
        MULTIBALL
    }

    class BOEntity
    {
        public bool visible;
        public string materialName;
        public Material material;
        public float width, height;
        public Vector4 color;
        public Vector2 position;
        public Vector2 velocity;
        public POWERUP powerup;
        public bool removed;
        public bool fadeOut;
        public GameBustOutWindow game;

        public BOEntity(GameBustOutWindow game);

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameBustOutWindow game);

        public void SetMaterial(string name);
        public void SetSize(float width, float _height);
        public void SetColor(float r, float g, float b, float a);
        public void SetVisible(bool isVisible);

        public virtual void Update(float timeslice, int guiTime);
        public virtual void Draw(DeviceContext dc);
    }

    enum COLLIDE
    {
        NONE = 0,
        DOWN,
        UP,
        LEFT,
        RIGHT
    }

    class BOBrick
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public POWERUP powerup;
        public bool isBroken;
        public BOEntity ent;

        public BOBrick();
        public BOBrick(BOEntity ent, float x, float y, float width, float height);

        public virtual void WriteToSaveGame(VFile savefile);
        public virtual void ReadFromSaveGame(VFile savefile, GameBustOutWindow game);

        public void SetColor(Vector4 bcolor);
        public COLLIDE checkCollision(Vector2 pos, Vector2 vel);
    }

    public class GameBustOutWindow : Window
    {
        public const int BOARD_ROWS = 12;

        WinBool gamerunning;
        WinBool onFire;
        WinBool onContinue;
        WinBool onNewGame;
        WinBool onNewLevel;

        float timeSlice;
        bool gameOver;

        int numLevels;
        byte[] levelBoardData;
        bool boardDataLoaded;

        int numBricks;
        int currentLevel;

        bool updateScore;
        int gameScore;
        int nextBallScore;

        int bigPaddleTime;
        float paddleVelocity;

        float ballSpeed;
        int ballsRemaining;
        int ballsInPlay;
        bool ballHitCeiling;

        List<BOEntity> balls;
        List<BOEntity> powerUps;

        BOBrick paddle;
        List<BOBrick>[] board = new List<BOBrick>[BOARD_ROWS];

        protected override bool ParseInternalVar(string name, Parser src);

        public GameBustOutWindow(UserInterfaceLocal gui);
        public GameBustOutWindow(DeviceContext dc, UserInterfaceLocal gui);

        public override void WriteToSaveGame(VFile savefile);
        public override void ReadFromSaveGame(VFile savefile);

        public override string HandleEvent(SysEvent ev, Action<bool> updateVisuals);
        public override void PostParse();
        public override void Draw(int time, float x, float y);
        public override WinVar GetWinVarByName(string name, bool winLookup = false, DrawWin owner = null);

        public List<BOEntity> entities;

        void CommonInit();
        void ResetGameState();

        void ClearBoard();
        void ClearPowerups();
        void ClearBalls();

        void LoadBoardFiles();
        void SetCurrentBoard();
        void UpdateGame();
        void UpdatePowerups();
        void UpdatePaddle();
        void UpdateBall();
        void UpdateScore();

        BOEntity CreateNewBall();
        BOEntity CreatePowerup(BOBrick brick);
    }
}

