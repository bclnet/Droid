#include "keys.h"
#include "ovr.h"
#include "app.h"
#include "native-log.h"
#include "lib/utils.h"
#include "lib/vmath.h"

float vr_reloadtimeoutms = 300.0f;
float vr_walkdirection = 0;
float vr_weapon_pitchadjust = -30.0f;
float vr_teleport;
float vr_switch_sticks = 0;
int give_weapon_count = 1;

extern bool forceVirtualScreen;
extern bool inMenu;
extern bool inGameGuiActive;
extern bool objectiveSystemActive;
extern bool inCinematic;

void HandleTrackedControllerButton_AsButton(uint32_t buttonsNew, uint32_t buttonsOld, bool mouse, uint32_t button, int key) {
    if ((buttonsNew & button) != (buttonsOld & button)) {
        if (mouse)
            Sys_AddMouseButtonEvent(key, (buttonsNew & button) != 0);
        else
            Android_ButtonChange(key, ((buttonsNew & button) != 0) ? 1 : 0);
    }
}

void HandleTrackedControllerButton_AsKey(uint32_t buttonsNew, uint32_t buttonsOld, uint32_t button, int key) {
    if ((buttonsNew & button) != (buttonsOld & button))
        Sys_AddKeyEvent(key, (buttonsNew & button) != 0);
}

void HandleTrackedControllerButton_AsToggleButton(uint32_t buttonsNew, uint32_t buttonsOld, uint32_t button, int key) {
    if ((buttonsNew & button) != (buttonsOld & button))
        Android_ButtonChange(key, Android_GetButton(key) ? 0 : 1);
}

//void SendButtonAction(const char *action, long buttonDown) {}
//
//void SendButtonActionSimple(const char *action) {}

// the amount of yaw changed by controller
void Input_AcquireTrackedRemotesData(ovrMobile *Ovr, double displayTime) {

    for (auto i = 0;; i++) {
        ovrInputCapabilityHeader capsHeader;
        auto result = vrapi_EnumerateInputDevices(Ovr, i, &capsHeader);
        if (result < 0)
            break;

        if (capsHeader.Type == ovrControllerType_Gamepad) {
            ovrInputGamepadCapabilities remoteCaps;
            remoteCaps.Header = capsHeader;
            if (vrapi_GetInputDeviceCapabilities(Ovr, &remoteCaps.Header) >= 0) {
                ovrInputStateGamepad remoteState;
                remoteState.Header.ControllerType = ovrControllerType_Gamepad;
                if (vrapi_GetCurrentInputState(Ovr, capsHeader.DeviceID, &remoteState.Header) >= 0)
                    _appInput.footTrackedRemoteState_new = remoteState;
            }
        } else if (capsHeader.Type == ovrControllerType_TrackedRemote) {
            ovrInputTrackedRemoteCapabilities remoteCaps;
            remoteCaps.Header = capsHeader;
            if (vrapi_GetInputDeviceCapabilities(Ovr, &remoteCaps.Header) >= 0) {
                ovrInputStateTrackedRemote remoteState;
                remoteState.Header.ControllerType = ovrControllerType_TrackedRemote;

                ovrTracking remoteTracking;
                if (vrapi_GetCurrentInputState(Ovr, capsHeader.DeviceID, &remoteState.Header) >= 0 && vrapi_GetInputTrackingState(Ovr, capsHeader.DeviceID, displayTime, &remoteTracking) >= 0) {
                    if (remoteCaps.ControllerCapabilities & ovrControllerCaps_RightHand) {
                        _appInput.rightTrackedRemoteState_new = remoteState;
                        _appInput.rightRemoteTracking_new = remoteTracking;
                        _appInput.controllerIDs[1] = capsHeader.DeviceID;
                    } else {
                        _appInput.leftTrackedRemoteState_new = remoteState;
                        _appInput.leftRemoteTracking_new = remoteTracking;
                        _appInput.controllerIDs[0] = capsHeader.DeviceID;
                    }
                }
            }
        }
    }
}

extern float SS_MULTIPLIER;

void ControlMouse(bool showingMenu, ovrInputStateTrackedRemote *newState, ovrInputStateTrackedRemote *oldState) {
    static int cursorX = 0;
    static int cursorY = 0;
    static bool waitForLevelController = true;
    static bool previousShowingMenu = true;
    static float yaw;

    // has menu been toggled?
    auto toggledMenuOn = !previousShowingMenu && showingMenu;
    previousShowingMenu = showingMenu;

    if (toggledMenuOn || waitForLevelController) {
        cursorX = (float) (((_appClient.weaponangles_temp[M_YAW] - yaw) * _appClient.width) / 60.0F);
        cursorY = (float) ((_appClient.weaponangles_temp[M_PITCH] * _appClient.height) / 70.0F);
        yaw = _appClient.weaponangles_temp[M_YAW];

        Sys_AddMouseMoveEvent(-10000, -10000);
        Sys_AddMouseMoveEvent((_appClient.width / 2.0F), (_appClient.height / 2.0F));

        waitForLevelController = true;
    }

    if (!showingMenu)
        return;

    // should we carry on waiting for the controller to be pointing forwards before we start sending mouse input?
    if (waitForLevelController) {
        if (M_Between(-5, (_appClient.weaponangles_temp[M_PITCH] - 30), 5))
            waitForLevelController = false;
        return;
    }

    int newCursorX = (float) ((-(_appClient.weaponangles_temp[M_YAW] - yaw) * _appClient.width) / 70.0F);
    int newCursorY = (float) ((_appClient.weaponangles_temp[M_PITCH] * _appClient.height) / 70.0F);

    Sys_AddMouseMoveEvent(newCursorX - cursorX, newCursorY - cursorY);

    cursorY = newCursorY;
    cursorX = newCursorX;
}

void Vec_QuatToYawPitchRoll(ovrQuatf q, vec3_t rotation, vec3_t out);

void Input_HandleDefault(int controlScheme, int switchSticks,
                         ovrInputStateGamepad *footTrackingNew, ovrInputStateGamepad *footTrackingOld,
                         ovrInputStateTrackedRemote *dominantTrackedRemoteNew, ovrInputStateTrackedRemote *dominantTrackedRemoteOld,
                         ovrTracking *dominantTracking,
                         ovrInputStateTrackedRemote *offTrackedRemoteNew, ovrInputStateTrackedRemote *offTrackedRemoteOld,
                         ovrTracking *offTracking,
                         int domButton1, int domButton2, int offButton1, int offButton2) {
    static bool dominantGripPushed = false;
    static float dominantGripPushTime = 0.0f;


    // need this for the touch screen
    ovrTracking *weapon = dominantTracking;
    ovrTracking *off = offTracking;

    // all this to allow stick and button switching!
    ovrVector2f *primaryJoystick;
    ovrVector2f *secondaryJoystick;
    uint32_t primaryButtonsNew;
    uint32_t primaryButtonsOld;
    uint32_t secondaryButtonsNew;
    uint32_t secondaryButtonsOld;
    uint32_t weaponButtonsNew;
    uint32_t weaponButtonsOld;
    uint32_t offhandButtonsNew;
    uint32_t offhandButtonsOld;
    int primaryButton1;
    int primaryButton2;
    int secondaryButton1;
    int secondaryButton2;

    weaponButtonsNew = dominantTrackedRemoteNew->Buttons;
    weaponButtonsOld = dominantTrackedRemoteOld->Buttons;
    offhandButtonsNew = offTrackedRemoteNew->Buttons;
    offhandButtonsOld = offTrackedRemoteOld->Buttons;

    if ((controlScheme == 0 && switchSticks == 1) || (controlScheme == 1 && switchSticks == 0)) {
        secondaryJoystick = &dominantTrackedRemoteNew->Joystick;
        primaryJoystick = &offTrackedRemoteNew->Joystick;
        secondaryButtonsNew = dominantTrackedRemoteNew->Buttons;
        secondaryButtonsOld = dominantTrackedRemoteOld->Buttons;
        primaryButtonsNew = offTrackedRemoteNew->Buttons;
        primaryButtonsOld = offTrackedRemoteOld->Buttons;
        primaryButton1 = offButton1;
        primaryButton2 = offButton2;
        secondaryButton1 = domButton1;
        secondaryButton2 = domButton2;
    } else {
        primaryJoystick = &dominantTrackedRemoteNew->Joystick;
        secondaryJoystick = &offTrackedRemoteNew->Joystick;
        primaryButtonsNew = dominantTrackedRemoteNew->Buttons;
        primaryButtonsOld = dominantTrackedRemoteOld->Buttons;
        secondaryButtonsNew = offTrackedRemoteNew->Buttons;
        secondaryButtonsOld = offTrackedRemoteOld->Buttons;
        primaryButton1 = domButton1;
        primaryButton2 = domButton2;
        secondaryButton1 = offButton1;
        secondaryButton2 = offButton2;
    }

    {
        // store original values
        const auto quatRHand = weapon->HeadPose.Pose.Orientation;
        const auto positionRHand = weapon->HeadPose.Pose.Position;
        const auto quatLHand = off->HeadPose.Pose.Orientation;
        const auto positionLHand = off->HeadPose.Pose.Position;

        // already does this so we end up with backward hands
        if (controlScheme == 1) {
            // Left Handed
            M_Vector3Set(_appClient.lhandposition, positionRHand.x, positionRHand.y, positionRHand.z);
            M_Vector4Set(_appClient.lhand_orientation_quat, quatRHand.x, quatRHand.y, quatRHand.z, quatRHand.w);
            M_Vector3Set(_appClient.rhandposition, positionLHand.x, positionLHand.y, positionLHand.z);
            M_Vector4Set(_appClient.rhand_orientation_quat, quatLHand.x, quatLHand.y, quatLHand.z, quatLHand.w);
        } else {
            // Right Hand
            M_Vector3Set(_appClient.rhandposition, positionRHand.x, positionRHand.y, positionRHand.z);
            M_Vector4Set(_appClient.rhand_orientation_quat, quatRHand.x, quatRHand.y, quatRHand.z, quatRHand.w);
            M_Vector3Set(_appClient.lhandposition, positionLHand.x, positionLHand.y, positionLHand.z);
            M_Vector4Set(_appClient.lhand_orientation_quat, quatLHand.x, quatLHand.y, quatLHand.z, quatLHand.w);
        }

        // set gun angles - We need to calculate all those we might need (including adjustments) for the client to then take its pick
        vec3_t rotation = {0};
        rotation[M_PITCH] = 30;
        rotation[M_PITCH] = vr_weapon_pitchadjust;
        Vec_QuatToYawPitchRoll(weapon->HeadPose.Pose.Orientation, rotation, _appClient.weaponangles_temp);
        M_Vector3Sub(_appClient.weaponangles_last_temp, _appClient.weaponangles_temp, _appClient.weaponangles_delta_temp);
        M_Vector3Cpy(_appClient.weaponangles_temp, _appClient.weaponangles_last_temp);
    }

    // menu button - can be used in all modes
    HandleTrackedControllerButton_AsKey(_appInput.leftTrackedRemoteState_new.Buttons, _appInput.leftTrackedRemoteState_old.Buttons, ovrButton_Enter, K_ESCAPE);
    ControlMouse(inMenu, dominantTrackedRemoteNew, dominantTrackedRemoteOld);
    if (inMenu || inCinematic) // Specific cases where we need to interact using mouse etc
        HandleTrackedControllerButton_AsButton(dominantTrackedRemoteNew->Buttons, dominantTrackedRemoteOld->Buttons, true, ovrButton_Trigger, 1);

    if (!inCinematic && !inMenu) {
        static auto canUseQuickSave = false;
        if (offTracking->Status & (VRAPI_TRACKING_STATUS_POSITION_TRACKED | VRAPI_TRACKING_STATUS_POSITION_VALID)) canUseQuickSave = false;
        else if (!canUseQuickSave) canUseQuickSave = true;

        if (canUseQuickSave) {
            if (((secondaryButtonsNew & secondaryButton1) != (secondaryButtonsOld & secondaryButton1)) && (secondaryButtonsNew & secondaryButton1))
                Android_SetCommand("savegame quick");
            if (((secondaryButtonsNew & secondaryButton2) != (secondaryButtonsOld & secondaryButton2)) && (secondaryButtonsNew & secondaryButton2))
                Android_SetCommand("loadgame quick");
        } else {
            // pda
            if (((secondaryButtonsNew & secondaryButton1) != (secondaryButtonsOld & secondaryButton1)) && (secondaryButtonsNew & secondaryButton1))
                Android_SetImpulse(UB_IMPULSE19);
            // toggle LaserSight
            if (((secondaryButtonsNew & secondaryButton2) != (secondaryButtonsOld & secondaryButton2)) && (secondaryButtonsNew & secondaryButton2))
                Android_SetImpulse(UB_IMPULSE33);
        }

        // turn on weapon stabilisation?
        auto distance = sqrtf(powf(off->HeadPose.Pose.Position.x - weapon->HeadPose.Pose.Position.x, 2) +
                              powf(off->HeadPose.Pose.Position.y - weapon->HeadPose.Pose.Position.y, 2) +
                              powf(off->HeadPose.Pose.Position.z - weapon->HeadPose.Pose.Position.z, 2));
        _appClient.weapon_stabilised = !_appClient.oneHandOnly && (offTrackedRemoteNew->Buttons & ovrButton_GripTrigger) && distance < STABILISATION_DISTANCE;

        {
            // does weapon velocity trigger attack and is it fast enough
            static auto velocityTriggeredAttack = false;
            if (_appClient.velocitytriggered) {
                // velocity trigger only available if weapon is not stabilised with off-hand
                if (!_appClient.weapon_stabilised) {
                    static auto fired = false;
                    auto velocity = sqrtf(powf(weapon->HeadPose.LinearVelocity.x, 2) +
                                          powf(weapon->HeadPose.LinearVelocity.y, 2) +
                                          powf(weapon->HeadPose.LinearVelocity.z, 2));
                    velocityTriggeredAttack = velocity > VELOCITY_TRIGGER;
                    if (fired != velocityTriggeredAttack) {
                        ALOGV("**WEAPON EVENT** velocity triggered %s", velocityTriggeredAttack ? "+attack" : "-attack");
                        Android_ButtonChange(UB_ATTACK, velocityTriggeredAttack ? 1 : 0);
                        fired = velocityTriggeredAttack;
                    }
                }
            } else if (velocityTriggeredAttack) {
                // send a stop attack as we have an unfinished velocity attack
                velocityTriggeredAttack = false;
                ALOGV("**WEAPON EVENT**  velocity triggered -attack");
                Android_ButtonChange(UB_ATTACK, velocityTriggeredAttack ? 1 : 0);
            }

            static bool velocityTriggeredAttackOffHand = false;
            _appClient.velocitytriggeredoffhandstate = false;
            if (_appClient.velocitytriggeredoffhand) {
                static auto firedOffHand = false;
                // velocity trigger only available if weapon is not stabilised with off-hand
                if (!_appClient.weapon_stabilised) {
                    auto velocity = sqrtf(powf(off->HeadPose.LinearVelocity.x, 2) +
                                          powf(off->HeadPose.LinearVelocity.y, 2) +
                                          powf(off->HeadPose.LinearVelocity.z, 2));
                    velocityTriggeredAttackOffHand = velocity > VELOCITY_TRIGGER;
                    if (firedOffHand != velocityTriggeredAttackOffHand) {
                        ALOGV("**WEAPON EVENT** velocity triggered (offhand) %s", velocityTriggeredAttackOffHand ? "+attack" : "-attack");
                        //Android_ButtonChange(UB_IMPULSE37, velocityTriggeredAttackOffHand ? 1 : 0);
                        //Android_SetImpulse(UB_IMPULSE37);
                        _appClient.velocitytriggeredoffhandstate = firedOffHand;
                        firedOffHand = velocityTriggeredAttackOffHand;
                    }
                } else firedOffHand = false;
            }
                // This actually nevers gets run currently as we are always returning true for pVRClientInfo->velocitytriggeredoffhand (but we might not in the future when weapons are sorted)
            else {
                // send a stop attack as we have an unfinished velocity attack
                velocityTriggeredAttackOffHand = false;
                ALOGV("**WEAPON EVENT**  velocity triggered -attack (offhand)");
                //Android_ButtonChange(UB_IMPULSE37, velocityTriggeredAttackOffHand ? 1 : 0);
                _appClient.velocitytriggeredoffhandstate = false;
            }
        }

        dominantGripPushed = (dominantTrackedRemoteNew->Buttons & ovrButton_GripTrigger) != 0;

        if (dominantGripPushed) {
            if (dominantGripPushTime == 0)
                dominantGripPushTime = GetTimeInMilliSeconds();
        } else {
            if ((GetTimeInMilliSeconds() - dominantGripPushTime) < vr_reloadtimeoutms)
                Android_SetImpulse(UB_IMPULSE13); // Reload
            dominantGripPushTime = 0;
        }

        auto controllerYawHeading = 0.0f;
        // off-hand stuff
        {
            _appClient.offhandoffset_temp[0] = off->HeadPose.Pose.Position.x - _appClient.hmdposition[0];
            _appClient.offhandoffset_temp[1] = off->HeadPose.Pose.Position.y - _appClient.hmdposition[1];
            _appClient.offhandoffset_temp[2] = off->HeadPose.Pose.Position.z - _appClient.hmdposition[2];

            vec3_t rotation = {0};
            rotation[M_PITCH] = -45;
            Vec_QuatToYawPitchRoll(off->HeadPose.Pose.Orientation, rotation, _appClient.offhandangles_temp);
        }

        // dominate-hand stuff
        {
            // fire-primary
            if ((dominantTrackedRemoteNew->Buttons & ovrButton_Trigger) != (dominantTrackedRemoteOld->Buttons & ovrButton_Trigger)) {
                ALOGV("**WEAPON EVENT**  Not Grip Pushed %sattack", (dominantTrackedRemoteNew->Buttons & ovrButton_Trigger) ? "+" : "-");
                HandleTrackedControllerButton_AsButton(dominantTrackedRemoteNew->Buttons, dominantTrackedRemoteOld->Buttons, false, ovrButton_Trigger, UB_ATTACK);
            }

            // duck
            if ((primaryButtonsNew & primaryButton1) != (primaryButtonsOld & primaryButton1))
                HandleTrackedControllerButton_AsToggleButton(primaryButtonsNew, primaryButtonsOld, primaryButton1, UB_DOWN);

            // jump
            if ((primaryButtonsNew & primaryButton2) != (primaryButtonsOld & primaryButton2))
                HandleTrackedControllerButton_AsButton(primaryButtonsNew, primaryButtonsOld, false, primaryButton2, UB_UP);

            // weapon Chooser
            static auto itemSwitched = false;
            if (M_Between(-0.2f, primaryJoystick->x, 0.2f) && (M_Between(0.8f, primaryJoystick->y, 1.0f) || M_Between(-1.0f, primaryJoystick->y, -0.8f))) {
                if (!itemSwitched) {
                    Android_SetImpulse(M_Between(0.8f, primaryJoystick->y, 1.0f) ? UB_IMPULSE15 : UB_IMPULSE14); // previous/next weapon
                    itemSwitched = true;
                }
            } else itemSwitched = false;
        }

        {
            // apply a filter and quadratic scaler so small movements are easier to make
            float dist = M_Len(secondaryJoystick->x, secondaryJoystick->y);
            float nlf = M::NonLinearFilter(dist);
            float x = (nlf * secondaryJoystick->x) + footTrackingNew->LeftJoystick.x;
            float y = (nlf * secondaryJoystick->y) - footTrackingNew->LeftJoystick.y;

            _appClient.player_moving = (fabs(x) + fabs(y)) > 0.05f;

            // adjust to be off-hand controller oriented
            vec2_t v;
            M::RotateAboutOrigin(x, y, controllerYawHeading, v);

            // move a lot slower if scope is engaged
            auto vr_movement_multiplier = 127.0f;
            _appInput.remote_movementSideways = v[0] * vr_movement_multiplier;
            _appInput.remote_movementForward = v[1] * vr_movement_multiplier;

            if (dominantGripPushed) {
                if (((offhandButtonsNew & ovrButton_Joystick) != (offhandButtonsOld & ovrButton_Joystick)) && (offhandButtonsNew & ovrButton_Joystick)) {
#ifdef DEBUG
                    //Android_SetCommand("give all");
                    if (give_weapon_count == 1) {
                        Android_SetCommand("give weapon_pistol");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 2) {
                        Android_SetCommand("give weapon_shotgun");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 3) {
                        Android_SetCommand("give weapon_shotgun");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 4) {
                        Android_SetCommand("give weapon_machinegun");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 5) {
                        Android_SetCommand("give weapon_chaingun");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 6) {
                        Android_SetCommand("give weapon_rocketlauncher");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 7) {
                        Android_SetCommand("give weapon_plasmagun");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 8) {
                        Android_SetCommand("give weapon_chainsaw");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 9) {
                        Android_SetCommand("give weapon_soulcube");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 10) {
                        Android_SetCommand("give weapon_bfg");
                        give_weapon_count = give_weapon_count + 1;
                    } else if (give_weapon_count == 11) {
                        Android_SetCommand("give ammo_grenade_small");
                        give_weapon_count = 1;
                    }
#endif
                    Android_SetImpulse(UB_IMPULSE32); // Recenter Body
                }
                if (((weaponButtonsNew & ovrButton_Joystick) != (weaponButtonsOld & ovrButton_Joystick)) && (weaponButtonsNew & ovrButton_Joystick))
                    Android_SetImpulse(UB_IMPULSE35); // toggle Torch Mode
            } else {
                if (((weaponButtonsNew & ovrButton_Joystick) != (weaponButtonsOld & ovrButton_Joystick)) && (weaponButtonsNew & ovrButton_Joystick))
                    Android_SetImpulse(UB_IMPULSE34); // toggle Body
                if (((offhandButtonsNew & ovrButton_Joystick) != (offhandButtonsOld & ovrButton_Joystick)) && (offhandButtonsNew & ovrButton_Joystick))
                    Android_SetImpulse(UB_IMPULSE16); // turn on Flashlight
            }

            // we need to record if we have started firing primary so that releasing trigger will stop definitely firing, if user has pushed grip
            // in meantime, then it wouldn't stop the gun firing and it would get stuck
            HandleTrackedControllerButton_AsButton(offTrackedRemoteNew->Buttons, offTrackedRemoteOld->Buttons, false, ovrButton_Trigger, UB_SPEED);

            int vr_turn_mode = Android_GetCVarInteger("vr_turnmode");
            float vr_turn_angle = Android_GetCVarInteger("vr_turnangle");

            // No snap turn when using mounted gun
            _appInput.snapTurn = 0;
            static int increaseSnap = true;
            {
                if (primaryJoystick->x > 0.7f) {
                    if (increaseSnap) {
                        float turnAngle = vr_turn_mode ? vr_turn_angle / 9.0f : vr_turn_angle;
                        _appInput.snapTurn -= turnAngle;
                        if (vr_turn_mode == 0)
                            increaseSnap = false;
                        if (_appInput.snapTurn < -180.0f)
                            _appInput.snapTurn += 360.f;
                    } else _appInput.snapTurn = 0;
                } else if (primaryJoystick->x < 0.3f) increaseSnap = true;

                static int decreaseSnap = true;
                if (primaryJoystick->x < -0.7f) {
                    if (decreaseSnap) {
                        float turnAngle = vr_turn_mode ? vr_turn_angle / 9.0f : vr_turn_angle;
                        _appInput.snapTurn += turnAngle;
                        // if snap turn configured for less than 10 degrees
                        if (vr_turn_mode == 0)
                            decreaseSnap = false;
                        if (_appInput.snapTurn > 180.0f)
                            _appInput.snapTurn -= 360.f;
                    } else _appInput.snapTurn = 0;
                } else if (primaryJoystick->x > -0.3f) decreaseSnap = true;
            }
        }
    }

    // save state
    _appInput.rightTrackedRemoteState_old = _appInput.rightTrackedRemoteState_new;
    _appInput.leftTrackedRemoteState_old = _appInput.leftTrackedRemoteState_new;
}
