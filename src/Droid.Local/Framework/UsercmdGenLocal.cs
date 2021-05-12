using System;
using static Droid.Framework.UB;

namespace Droid.Framework
{
    enum UB
    {
        UB_NONE,

        UB_UP,
        UB_DOWN,
        UB_LEFT,
        UB_RIGHT,
        UB_FORWARD,
        UB_BACK,
        UB_LOOKUP,
        UB_LOOKDOWN,
        UB_STRAFE,
        UB_MOVELEFT,
        UB_MOVERIGHT,

        UB_BUTTON0,
        UB_BUTTON1,
        UB_BUTTON2,
        UB_BUTTON3,
        UB_BUTTON4,
        UB_BUTTON5,
        UB_BUTTON6,
        UB_BUTTON7,

        UB_ATTACK,
        UB_SPEED,
        UB_ZOOM,
        UB_SHOWSCORES,
        UB_MLOOK,

        UB_IMPULSE0,
        UB_IMPULSE1,
        UB_IMPULSE2,
        UB_IMPULSE3,
        UB_IMPULSE4,
        UB_IMPULSE5,
        UB_IMPULSE6,
        UB_IMPULSE7,
        UB_IMPULSE8,
        UB_IMPULSE9,
        UB_IMPULSE10,
        UB_IMPULSE11,
        UB_IMPULSE12,
        UB_IMPULSE13,
        UB_IMPULSE14,
        UB_IMPULSE15,
        UB_IMPULSE16,
        UB_IMPULSE17,
        UB_IMPULSE18,
        UB_IMPULSE19,
        UB_IMPULSE20,
        UB_IMPULSE21,
        UB_IMPULSE22,
        UB_IMPULSE23,
        UB_IMPULSE24,
        UB_IMPULSE25,
        UB_IMPULSE26,
        UB_IMPULSE27,
        UB_IMPULSE28,
        UB_IMPULSE29,
        UB_IMPULSE30,
        UB_IMPULSE31,
        UB_IMPULSE32,
        UB_IMPULSE33,
        UB_IMPULSE34,
        UB_IMPULSE35,
        UB_IMPULSE36,
        UB_IMPULSE37,
        UB_IMPULSE38,
        UB_IMPULSE39,
        UB_IMPULSE40,
        UB_IMPULSE41,
        UB_IMPULSE42,
        UB_IMPULSE43,
        UB_IMPULSE44,
        UB_IMPULSE45,
        UB_IMPULSE46,
        UB_IMPULSE47,
        UB_IMPULSE48,
        UB_IMPULSE49,
        UB_IMPULSE50,
        UB_IMPULSE51,
        UB_IMPULSE52,
        UB_IMPULSE53,
        UB_IMPULSE54,
        UB_IMPULSE55,
        UB_IMPULSE56,
        UB_IMPULSE57,
        UB_IMPULSE58,
        UB_IMPULSE59,
        UB_IMPULSE60,
        UB_IMPULSE61,
        UB_IMPULSE62,
        UB_IMPULSE63,

        UB_MAX_BUTTONS
    }

    struct buttonState
    {
        public int on;
        public bool held;

        public void Clear() { held = false; on = 0; }
        public void SetKeyState(int keystate, bool toggle)
        {
            if (!toggle) { held = false; on = keystate; }
            else if (keystate == 0) held = false;
            else if (!held) { held = true; on ^= 1; }
        }
    }

    internal class UsercmdGenLocal : UsercmdGen
    {
        const int KEY_MOVESPEED = 127;

        static readonly (string s, UB ub)[] userCmdStrings = new (string, UB)[]{
            ( "_moveUp",        UB_UP ),
            ( "_moveDown",      UB_DOWN ),
            ( "_left",          UB_LEFT ),
            ( "_right",         UB_RIGHT ),
            ( "_forward",       UB_FORWARD ),
            ( "_back",          UB_BACK ),
            ( "_lookUp",        UB_LOOKUP ),
            ( "_lookDown",      UB_LOOKDOWN ),
            ( "_strafe",        UB_STRAFE ),
            ( "_moveLeft",      UB_MOVELEFT ),
            ( "_moveRight",     UB_MOVERIGHT ),

            ( "_attack",        UB_ATTACK ),
            ( "_speed",         UB_SPEED ),
            ( "_zoom",          UB_ZOOM ),
            ( "_showScores",    UB_SHOWSCORES ),
            ( "_mlook",         UB_MLOOK ),

            ( "_button0",       UB_BUTTON0 ),
            ( "_button1",       UB_BUTTON1 ),
            ( "_button2",       UB_BUTTON2 ),
            ( "_button3",       UB_BUTTON3 ),
            ( "_button4",       UB_BUTTON4 ),
            ( "_button5",       UB_BUTTON5 ),
            ( "_button6",       UB_BUTTON6 ),
            ( "_button7",       UB_BUTTON7 ),

            ( "_impulse0",      UB_IMPULSE0 ),
            ( "_impulse1",      UB_IMPULSE1 ),
            ( "_impulse2",      UB_IMPULSE2 ),
            ( "_impulse3",      UB_IMPULSE3 ),
            ( "_impulse4",      UB_IMPULSE4 ),
            ( "_impulse5",      UB_IMPULSE5 ),
            ( "_impulse6",      UB_IMPULSE6 ),
            ( "_impulse7",      UB_IMPULSE7 ),
            ( "_impulse8",      UB_IMPULSE8 ),
            ( "_impulse9",      UB_IMPULSE9 ),
            ( "_impulse10",     UB_IMPULSE10 ),
            ( "_impulse11",     UB_IMPULSE11 ),
            ( "_impulse12",     UB_IMPULSE12 ),
            ( "_impulse13",     UB_IMPULSE13 ),
            ( "_impulse14",     UB_IMPULSE14 ),
            ( "_impulse15",     UB_IMPULSE15 ),
            ( "_impulse16",     UB_IMPULSE16 ),
            ( "_impulse17",     UB_IMPULSE17 ),
            ( "_impulse18",     UB_IMPULSE18 ),
            ( "_impulse19",     UB_IMPULSE19 ),
            ( "_impulse20",     UB_IMPULSE20 ),
            ( "_impulse21",     UB_IMPULSE21 ),
            ( "_impulse22",     UB_IMPULSE22 ),
            ( "_impulse23",     UB_IMPULSE23 ),
            ( "_impulse24",     UB_IMPULSE24 ),
            ( "_impulse25",     UB_IMPULSE25 ),
            ( "_impulse26",     UB_IMPULSE26 ),
            ( "_impulse27",     UB_IMPULSE27 ),
            ( "_impulse28",     UB_IMPULSE28 ),
            ( "_impulse29",     UB_IMPULSE29 ),
            ( "_impulse30",     UB_IMPULSE30 ),
            ( "_impulse31",     UB_IMPULSE31 ),
            ( "_impulse32",     UB_IMPULSE32 ),
            ( "_impulse33",     UB_IMPULSE33 ),
            ( "_impulse34",     UB_IMPULSE34 ),
            ( "_impulse35",     UB_IMPULSE35 ),
            ( "_impulse36",     UB_IMPULSE36 ),
            ( "_impulse37",     UB_IMPULSE37 ),
            ( "_impulse38",     UB_IMPULSE38 ),
            ( "_impulse39",     UB_IMPULSE39 ),
            ( "_impulse40",     UB_IMPULSE40 ),
            ( "_impulse41",     UB_IMPULSE41 ),
            ( "_impulse42",     UB_IMPULSE42 ),
            ( "_impulse43",     UB_IMPULSE43 ),
            ( "_impulse44",     UB_IMPULSE44 ),
            ( "_impulse45",     UB_IMPULSE45 ),
            ( "_impulse46",     UB_IMPULSE46 ),
            ( "_impulse47",     UB_IMPULSE47 ),
            ( "_impulse48",     UB_IMPULSE48 ),
            ( "_impulse49",     UB_IMPULSE49 ),
            ( "_impulse50",     UB_IMPULSE50 ),
            ( "_impulse51",     UB_IMPULSE51 ),
            ( "_impulse52",     UB_IMPULSE52 ),
            ( "_impulse53",     UB_IMPULSE53 ),
            ( "_impulse54",     UB_IMPULSE54 ),
            ( "_impulse55",     UB_IMPULSE55 ),
            ( "_impulse56",     UB_IMPULSE56 ),
            ( "_impulse57",     UB_IMPULSE57 ),
            ( "_impulse58",     UB_IMPULSE58 ),
            ( "_impulse59",     UB_IMPULSE59 ),
            ( "_impulse60",     UB_IMPULSE60 ),
            ( "_impulse61",     UB_IMPULSE61 ),
            ( "_impulse62",     UB_IMPULSE62 ),
            ( "_impulse63",     UB_IMPULSE63 ),
        };

        const int MAX_CHAT_BUFFER = 127;

        public UsercmdGenLocal()
        {
            lastCommandTime = 0;
            initialized = false;
            InitForNewMap();
        }

        public override void Init() => initialized = true;

        public override void InitForNewMap()
        {
            flags = 0;
            impulse = 0;

            toggled_crouch.Clear();
            toggled_run.Clear();
            toggled_zoom.Clear();
            toggled_run.on = in_alwaysRun.Bool ? 1 : 0;

            ClearAngles();
            Clear();
        }

        public override void Shutdown() { }

        public override void Clear()
        {
            // clears all key states
            Array.Clear(buttonState, 0, buttonState.Length);
            Array.Clear(keyState, 0, keyState.Length);

            inhibitCommands = false;

            mouseDx = mouseDy = 0;
            mouseButton = 0;
            mouseDown = false;
        }

        public override void ClearAngles()
        {
            viewangles.Zero();
        }

        public override usercmd TicCmd(int ticNumber)
        {
            // the packetClient code can legally ask for com_ticNumber+1, because it is in the async code and com_ticNumber hasn't been updated yet,
            // but all other code should never ask for anything > com_ticNumber
            if (ticNumber > com_ticNumber + 1)
                G.common.Error("UsercmdGenLocal::TicCmd ticNumber > com_ticNumber");

            if (ticNumber <= com_ticNumber - MAX_BUFFERED_USERCMD)
            {
                // this can happen when something in the game code hitches badly, allowing the async code to overflow the buffers
                //G.common.Printf("warning: idUsercmdGenLocal::TicCmd ticNumber <= com_ticNumber - MAX_BUFFERED_USERCMD\n");
            }

            return buffered[ticNumber & (MAX_BUFFERED_USERCMD - 1)];
        }

        public override void InhibitUsercmd(INHIBIT subsystem, bool inhibit)
        {
            if (inhibit)
                inhibitCommands |= 1 << (int)subsystem;
            else
                inhibitCommands &= (0xffffffff ^ (1 << (int)subsystem));
        }

        /// <summary>
        /// Called asyncronously
        /// </summary>
        public override void UsercmdInterrupt()
        {
            // dedicated servers won't create usercmds
            if (!initialized)
            {
                return;
            }

            // init the usercmd for com_ticNumber+1
            InitCurrent();

            // process the system mouse events
            Mouse();

            // process the system keyboard events
            Keyboard();

            // process the system joystick events
            Joystick();

            // create the usercmd for com_ticNumber+1
            MakeCurrent();

            // save a number for debugging cmdDemos and networking
            cmd.sequence = com_ticNumber + 1;

            buffered[(com_ticNumber + 1) & (MAX_BUFFERED_USERCMD - 1)] = cmd;
        }

        /// <summary>
        /// Returns the button if the command string is used by the async usercmd generator.
        /// </summary>
        /// <param name="cmdString">The command string.</param>
        /// <returns></returns>
        public override int CommandStringUsercmdData(string cmdString)
        {
            for (userCmdString_t* ucs = userCmdStrings; ucs->string ; ucs++ ) {
                if (idStr::Icmp(cmdString, ucs->string) == 0)
                    return ucs->button;
                return UB_NONE;
            }

        public override int GetNumUserCommands() => NUM_USER_COMMANDS;

        public override string GetUserCommandName(int index) => index >= 0 && index < NUM_USER_COMMANDS ? userCmdStrings[index].string : string.Empty;

        public override void MouseState(out int x, out int y, out int button, out bool down)
        {
            x = continuousMouseX;
            y = continuousMouseY;
            button = mouseButton;
            down = mouseDown;
        }

        /// <summary>
        /// Returns (the fraction of the frame) that the key was down
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override int ButtonState(int key) => key < 0 || key >= UB_MAX_BUTTONS ? -1 : buttonState[key] > 0 || Android_GetButton(key) ? 1 : 0;

        /// <summary>
        /// Returns (the fraction of the frame) that the key was down bk20060111
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public override int KeyState(int key) => key < 0 || key >= K_LAST_KEY ? -1 : keyState[key] ? 1 : 0;

        public override (string s, UB ub) GetDirectUsercmd()
        {
            // initialize current usercmd
            InitCurrent();

            // process the system mouse events
            Mouse();

            // process the system keyboard events
            Keyboard();

            // process the system joystick events
            //Joystick();

            int imp = Android_GetNextImpulse();
            if (imp)
            {
                if (!Inhibited())
                {
                    if (imp >= UB_IMPULSE0 && imp <= UB_IMPULSE61)
                    {
                        cmd.impulse = imp - UB_IMPULSE0;
                        cmd.flags ^= UCF_IMPULSE_SEQUENCE;
                    }
                }
            }

            // create the usercmd
            MakeCurrent();

            cmd.duplicateCount = 0;

            return cmd;
        }

        /// <summary>
        /// creates the current command for this frame
        /// </summary>
        void MakeCurrent()
        {
            idVec3 oldAngles = viewangles;
            static int thirdPersonTime = Sys_Milliseconds();
            int i;
            static float prevYaw = 0;

            oldAngles = viewangles;

            if (!Inhibited)
            {
                // update toggled key states
                toggled_crouch.SetKeyState(ButtonState(UB_DOWN), in_toggleCrouch.GetBool());
                toggled_run.SetKeyState(ButtonState(UB_SPEED), in_toggleRun.GetBool() && idAsyncNetwork::IsActive());
                toggled_zoom.SetKeyState(ButtonState(UB_ZOOM), in_toggleZoom.GetBool());

                // keyboard angle adjustment
                AdjustAngles();

                // set button bits
                CmdButtons();

                // get basic movement from keyboard
                KeyMove();

                //Call game specific VR stuff and gubbins
                int buttonCurrentlyClicked = ButtonState(UB_IMPULSE41);

                //Dr Beefs Code
                float forward, strafe;
                float hmd_forward, hmd_strafe;
                float up = 0;
                float yaw = 0;
                float pitch = 0;
                float roll = 0;
                VR_GetMove(out var forward, out var strafe, out var hmd_forward, out var hmd_strafe, out var up, out var yaw, out var pitch, out var roll);

                //Maybe this is right as long as I don't include HMD
                cmd.rightmove = idMath::ClampChar(cmd.rightmove + strafe);
                cmd.forwardmove = idMath::ClampChar(cmd.forwardmove + forward);

                game->EvaluateVRMoveMode(viewangles, cmd, buttonCurrentlyClicked, yaw);

                // check to make sure the angles haven't wrapped
                if (viewangles[PITCH] - oldAngles[PITCH] > 90)
                    viewangles[PITCH] = oldAngles[PITCH] + 90;
                else if (oldAngles[PITCH] - viewangles[PITCH] > 90)
                    viewangles[PITCH] = oldAngles[PITCH] - 90;


#if false
		// get basic movement from mouse
		MouseMove();

		// get basic movement from joystick
		JoystickMove();
#endif

                /*float forward,strafe;
                float hmd_forward,hmd_strafe;
                float up = 0;
                float yaw = 0;
                float pitch = 0;
                float roll = 0;

                static int previous = 0;
                int t = Sys_Milliseconds();
                int frameTime = t - previous;
                previous = t;
                if(frameTime > 100)
                    frameTime = 100;

                VR_GetMove(&forward, &strafe, &hmd_forward, &hmd_strafe, &up, &yaw, &pitch, &roll);

                cmd.rightmove = idMath::ClampChar( cmd.rightmove + strafe + hmd_strafe );
                cmd.forwardmove = idMath::ClampChar( cmd.forwardmove + forward + hmd_forward);
                viewangles[YAW] -= prevYaw;
                viewangles[YAW] += yaw;
                viewangles[PITCH] = pitch;
                viewangles[ROLL] = roll;
                prevYaw = yaw;

                // check to make sure the angles haven't wrapped
                if ( viewangles[PITCH] - oldAngles[PITCH] > 90 ) {
                    viewangles[PITCH] = oldAngles[PITCH] + 90;
                } else if ( oldAngles[PITCH] - viewangles[PITCH] > 90 ) {
                    viewangles[PITCH] = oldAngles[PITCH] - 90;
                }*/
            }
            else
            {
                mouseDx = 0;
                mouseDy = 0;
            }

            for (i = 0; i < 3; i++)
                cmd.angles[i] = ANGLE2SHORT(viewangles[i]);  // Koz this sets player body

            cmd.mx = continuousMouseX;
            cmd.my = continuousMouseY;

            flags = cmd.flags;
            impulse = cmd.impulse;
        }

        /// <summary>
        /// inits the current command for this frame
        /// </summary>
        void InitCurrent()
        {
            memset(&cmd, 0, sizeof(cmd));
            cmd.flags = flags;
            cmd.impulse = impulse;
            cmd.buttons |= (in_alwaysRun.GetBool() && idAsyncNetwork::IsActive()) ? BUTTON_RUN : 0;
            cmd.buttons |= in_freeLook.GetBool() ? BUTTON_MLOOK : 0;
        }

        /// <summary>
        /// is user cmd generation inhibited
        /// </summary>
        /// <value>
        ///   <c>true</c> if inhibited; otherwise, <c>false</c>.
        /// </value>
        bool Inhibited => inhibitCommands != 0;
        /// <summary>
        /// Moves the local angle positions
        /// </summary>
        void AdjustAngles()
        {
            float speed;

            var speed = toggled_run.on ^ (in_alwaysRun.Bool && AsyncNetwork.IsActive)
                ? Math.M_MS2SEC * USERCMD_MSEC * in_angleSpeedKey.Float
                : Math.M_MS2SEC * USERCMD_MSEC;

            if (!ButtonState(UB_STRAFE))
            {
                viewangles[YAW] -= speed * in_yawSpeed.Float * ButtonState(UB_RIGHT);
                viewangles[YAW] += speed * in_yawSpeed.Float * ButtonState(UB_LEFT);
            }

            viewangles[PITCH] -= speed * in_pitchSpeed.Float * ButtonState(UB_LOOKUP);
            viewangles[PITCH] += speed * in_pitchSpeed.Float * ButtonState(UB_LOOKDOWN);
        }
        /// <summary>
        /// Sets the usercmd_t based on key states
        /// </summary>
        void KeyMove()
        {
            int forward, side, up;

            forward = 0;
            side = 0;
            up = 0;
            /*	if ( ButtonState( UB_STRAFE ) ) {
                    side += KEY_MOVESPEED * ButtonState( UB_RIGHT );
                    side -= KEY_MOVESPEED * ButtonState( UB_LEFT );
                }

                side += KEY_MOVESPEED * ButtonState( UB_MOVERIGHT );
                side -= KEY_MOVESPEED * ButtonState( UB_MOVELEFT );
            */
            up -= KEY_MOVESPEED * toggled_crouch.on;
            up += KEY_MOVESPEED * ButtonState(UB_UP);

            /*	forward += KEY_MOVESPEED * ButtonState( UB_FORWARD );
                forward -= KEY_MOVESPEED * ButtonState( UB_BACK );

                cmd.forwardmove = idMath::ClampChar( forward );
                cmd.rightmove = idMath::ClampChar( side );
             */
            cmd.upmove = Math.ClampChar(up);
        }
        void JoystickMove()
        {
            float anglespeed;

            if (toggled_run.on ^ (in_alwaysRun.Bool && AsyncNetwork.IsActive))
                anglespeed = Math.M_MS2SEC * USERCMD_MSEC * in_angleSpeedKey.Float;
            else
                anglespeed = Math.M_MS2SEC * USERCMD_MSEC;

            if (!ButtonState(UB_STRAFE))
            {
                viewangles[YAW] += anglespeed * in_yawSpeed.GetFloat() * joystickAxis[AXIS_SIDE];
                viewangles[PITCH] += anglespeed * in_pitchSpeed.GetFloat() * joystickAxis[AXIS_FORWARD];
            }
            else
            {
                cmd.rightmove = Math.ClampChar(cmd.rightmove + joystickAxis[AXIS_SIDE]);
                cmd.forwardmove = Math.ClampChar(cmd.forwardmove + joystickAxis[AXIS_FORWARD]);
            }

            cmd.upmove = Math.ClampChar(cmd.upmove + joystickAxis[AXIS_UP]);
        }
        void MouseMove()
        {
            float mx, my, strafeMx, strafeMy;
            static int history[8][2];
            static int historyCounter;
            int i;

            history[historyCounter & 7][0] = mouseDx;
            history[historyCounter & 7][1] = mouseDy;

            // allow mouse movement to be smoothed together
            var smooth = m_smooth.Integer;
            if (smooth < 1)
                smooth = 1;
            if (smooth > 8)
                smooth = 8;
            mx = 0;
            my = 0;
            for (i = 0; i < smooth; i++)
            {
                mx += history[(historyCounter - i + 8) & 7][0];
                my += history[(historyCounter - i + 8) & 7][1];
            }
            mx /= smooth;
            my /= smooth;

            // use a larger smoothing for strafing
            smooth = m_strafeSmooth.Integer;
            if (smooth < 1)
                smooth = 1;
            if (smooth > 8)
                smooth = 8;
            strafeMx = 0;
            strafeMy = 0;
            for (i = 0; i < smooth; i++)
            {
                strafeMx += history[(historyCounter - i + 8) & 7][0];
                strafeMy += history[(historyCounter - i + 8) & 7][1];
            }
            strafeMx /= smooth;
            strafeMy /= smooth;

            historyCounter++;

            if (Math.Fabs(mx) > 1000 || Math.Fabs(my) > 1000)
            {
                Sys_DebugPrintf("UsercmdGenLocal::MouseMove: Ignoring ridiculous mouse delta.\n");
                mx = my = 0;
            }

            mx *= sensitivity.Float;
            my *= sensitivity.Float;

            if (m_showMouseRate.Bool)
                Sys_DebugPrintf("[%3i %3i  = %5.1f %5.1f = %5.1f %5.1f] ", mouseDx, mouseDy, mx, my, strafeMx, strafeMy);

            mouseDx = 0;
            mouseDy = 0;

            if (strafeMx == 0 && strafeMy == 0)
                return;

            if (ButtonState(UB_STRAFE) || !(cmd.buttons & BUTTON_MLOOK))
            {
                // add mouse X/Y movement to cmd
                strafeMx *= m_strafeScale.Float;
                strafeMy *= m_strafeScale.Float;
                // clamp as a vector, instead of separate floats
                var len = (float)Math.Sqrt(strafeMx * strafeMx + strafeMy * strafeMy);
                if (len > 127)
                {
                    strafeMx = strafeMx * 127 / len;
                    strafeMy = strafeMy * 127 / len;
                }
            }

            if (!ButtonState(UB_STRAFE))
                viewangles[YAW] -= m_yaw.Float * mx;
            else
                cmd.rightmove = Math.ClampChar((int)(cmd.rightmove + strafeMx));

            if (!ButtonState(UB_STRAFE) && (cmd.buttons & BUTTON_MLOOK))
                viewangles[PITCH] += m_pitch.Float * my;
            else
                cmd.forwardmove = Math.ClampChar((int)(cmd.forwardmove - strafeMy));
        }
        void CmdButtons()
        {
            cmd.buttons = 0;

            // Koz begin cancel teleport if fire button pressed.
            static int teleportCanceled = 0;
            bool performAttack = true;

            // figure button bits
            for (var i = 0; i <= 7; i++)
                if (ButtonState((usercmdButton_t)(UB_BUTTON0 + i)))
                    cmd.buttons |= 1 << i;

            // check the attack button
            if (ButtonState(UB_ATTACK))
            {
                performAttack = game->CMDButtonsAttackCall(teleportCanceled);
                if (performAttack)
                    cmd.buttons |= BUTTON_ATTACK;
            }

            teleportCanceled &= ButtonState(UB_ATTACK);

            // check the run button
            if (toggled_run.on ^ (in_alwaysRun.Bool && AsyncNetwork.IsActive))
                cmd.buttons |= BUTTON_RUN;
            // check the zoom button
            if (toggled_zoom.on)
                cmd.buttons |= BUTTON_ZOOM;
            // check the scoreboard button
            if (ButtonState(UB_SHOWSCORES) || ButtonState(UB_IMPULSE19))
                cmd.buttons |= BUTTON_SCORES; // the button is toggled in SP mode as well but without effect
            // check the mouse look button
            if (ButtonState(UB_MLOOK) ^ in_freeLook.GetInteger())
                cmd.buttons |= BUTTON_MLOOK;
            if (ButtonState(UB_UP))
                cmd.buttons |= BUTTON_JUMP;
            if (toggled_crouch.on)
                cmd.buttons |= BUTTON_CROUCH;
            if (G.game.CMDButtonsPhysicalCrouch())
                cmd.buttons |= BUTTON_CROUCH;
        }

        void Mouse()
        {
            // Study each of the buffer elements and process them.
            var numEvents = Sys_PollMouseInputEvents();
            if (numEvents != 0)
                for (var i = 0; i < numEvents; i++)
                {
                    int action, value;
                    if (Sys_ReturnMouseInputEvent(i, action, value))
                    {
                        if (action >= M_ACTION1 && action <= M_ACTION8)
                        {
                            mouseButton = K_MOUSE1 + (action - M_ACTION1);
                            mouseDown = (value != 0);
                            Key(mouseButton, mouseDown);
                        }
                        else
                        {
                            switch (action)
                            {
                                case M_DELTAX:
                                    mouseDx += value;
                                    continuousMouseX += value;
                                    break;
                                case M_DELTAY:
                                    mouseDy += value;
                                    continuousMouseY += value;
                                    break;
                                case M_DELTAZ:
                                    int key = value < 0 ? K_MWHEELDOWN : K_MWHEELUP;
                                    value = abs(value);
                                    while (value-- > 0)
                                    {
                                        Key(key, true);
                                        Key(key, false);
                                        mouseButton = key;
                                        mouseDown = true;
                                    }
                                    break;
                            }
                        }
                    }
                }
            Sys_EndMouseInputEvents();
        }

        void Keyboard()
        {
            // Study each of the buffer elements and process them.
            var numEvents = Sys_PollKeyboardInputEvents();
            if (numEvents != 0)
                for (var i = 0; i < numEvents; i++)
                    if (Sys_ReturnKeyboardInputEvent(i, out var key, out var state))
                        Key(key, state);
            Sys_EndKeyboardInputEvents();
        }

        void Joystick() => Array.Clear(joystickAxis, 0, joystickAxis.Length);

        /// <summary>
        /// Handles async mouse/keyboard button actions
        /// </summary>
        /// <param name="keyNum">The key number.</param>
        /// <param name="down">if set to <c>true</c> [down].</param>
        void Key(int keyNum, bool down)
        {
            // Sanity check, sometimes we get double message :(
            if (keyState[keyNum] == down)
                return;
            keyState[keyNum] = down;

            var action = KeyInput.GetUsercmdAction(keyNum);
            if (down)
            {
                buttonState[action]++;

                if (!Inhibited)
                    if (action >= (int)UB_IMPULSE0 && action <= (int)UB_IMPULSE61)
                    {
                        cmd.impulse = (sbyte)(action - UB_IMPULSE0);
                        cmd.flags ^= usercmd.UCF_IMPULSE_SEQUENCE;
                    }
            }
            else
            {
                buttonState[action]--;
                // we might have one held down across an app active transition
                if (buttonState[action] < 0)
                    buttonState[action] = 0;
            }
        }

        Vec3 viewangles;
        int flags;
        int impulse;

        buttonState toggled_crouch;
        buttonState toggled_run;
        buttonState toggled_zoom;

        int[] buttonState = new int[(int)UB_MAX_BUTTONS];
        bool[] keyState = new bool[(int)K.K_LAST_KEY];

        int inhibitCommands;    // true when in console or menu locally
        int lastCommandTime;

        bool initialized;

        usercmd cmd;      // the current cmd being built
        usercmd[] buffered = new usercmd[MAX_BUFFERED_USERCMD];

        int continuousMouseX, continuousMouseY; // for gui event generatioin, never zerod
        int mouseButton;                        // for gui event generatioin
        bool mouseDown;

        int mouseDx, mouseDy;   // added to by mouse events
        int[] joystickAxis = new int[MAX_JOYSTICK_AXIS];    // set by joystick events

        static CVar in_yawSpeed = new("in_yawspeed", "140", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "yaw change speed when holding down _left or _right button");
        static CVar in_pitchSpeed = new("in_pitchspeed", "140", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "pitch change speed when holding down look _lookUp or _lookDown button");
        static CVar in_angleSpeedKey = new("in_anglespeedkey", "1.5", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "angle change scale when holding down _speed button");
        static CVar in_freeLook = new("in_freeLook", "1", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.BOOL, "look around with mouse (reverse _mlook button)");
        static CVar in_alwaysRun = new("in_alwaysRun", "0", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.BOOL, "always run (reverse _speed button) - only in MP");
        static CVar in_toggleRun = new("in_toggleRun", "0", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.BOOL, "pressing _speed button toggles run on/off - only in MP");
        static CVar in_toggleCrouch = new("in_toggleCrouch", "1", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.BOOL, "pressing _movedown button toggles player crouching/standing");
        static CVar in_toggleZoom = new("in_toggleZoom", "0", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.BOOL, "pressing _zoom button toggles zoom on/off");
        static CVar sensitivity = new("sensitivity", "5", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "mouse view sensitivity");
        static CVar m_pitch = new("m_pitch", "0.022", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "mouse pitch scale");
        static CVar m_yaw = new("m_yaw", "0.022", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "mouse yaw scale");
        static CVar m_strafeScale = new("m_strafeScale", "6.25", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.FLOAT, "mouse strafe movement scale");
        static CVar m_smooth = new("m_smooth", "1", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.INTEGER, "number of samples blended for mouse viewing", 1, 8, CmdSystem.ArgCompletion_Integer(1, 8));
        static CVar m_strafeSmooth = new("m_strafeSmooth", "4", CVAR.SYSTEM | CVAR.ARCHIVE | CVAR.INTEGER, "number of samples blended for mouse moving", 1, 8, CmdSystem.ArgCompletion_Integer(1, 8));
        static CVar m_showMouseRate = new("m_showMouseRate", "0", CVAR.SYSTEM | CVAR.BOOL, "shows mouse movement");
    }
}