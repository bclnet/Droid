#ifndef APP_H
#define APP_H

#include <VrApi_Input.h>

/*
================================================================================
appClient
================================================================================
*/

#define NUM_WEAPON_SAMPLES      7
#define OLDER_READING            (NUM_WEAPON_SAMPLES-1)
#define NEWER_READING            (NUM_WEAPON_SAMPLES-5)

typedef float vec3_t[3];
typedef float vec4_t[4];

typedef struct {
    float screenYaw;
    float playerYaw;
    int width;
    int height;

    bool screen;
    float fov;
    bool weapon_stabilised;
    bool oneHandOnly;
    bool right_handed;
    bool player_moving;
    bool visible_hud;
    bool dualwield;
    int weaponid;
    int lastweaponid;

    // fp - Carry original values
    vec4_t hmdorientation_quat;
    vec3_t hmdtranslation;
    vec3_t lhandposition;
    vec3_t rhandposition;
    vec4_t lhand_orientation_quat;
    vec4_t rhand_orientation_quat;

    vec3_t hmdposition;
    vec3_t hmdposition_last; // Don't use this, it is just for calculating delta!
    vec3_t hmdposition_delta;

    // fp - Temp Variables for other stuff
    vec3_t hmdorientation_temp;
    vec3_t weaponangles_temp;
    vec3_t weaponangles_last_temp; // Don't use this, it is just for calculating delta!
    vec3_t weaponangles_delta_temp;

    vec3_t throw_origin;
    vec3_t throw_trajectory;
    float throw_power;

    bool velocitytriggered; // Weapon attack triggered by velocity (knife)
    bool velocitytriggeredoffhand; // Weapon attack triggered by velocity (puncher)
    bool velocitytriggeredoffhandstate; // Weapon attack triggered by velocity (puncher)

    vec3_t offhandangles_temp;
    vec3_t offhandoffset_temp;
} appClient;

extern appClient _appClient;

/*
================================================================================
appInput
================================================================================
*/

//enum control_scheme;
#define STABILISATION_DISTANCE   0.5
#define FLASHLIGHT_HOLSTER_DISTANCE   0.15
#define VELOCITY_TRIGGER        1.6

typedef struct {
    ovrInputStateTrackedRemote leftTrackedRemoteState_old;
    ovrInputStateTrackedRemote leftTrackedRemoteState_new;
    ovrTracking leftRemoteTracking_new;

    ovrInputStateTrackedRemote rightTrackedRemoteState_old;
    ovrInputStateTrackedRemote rightTrackedRemoteState_new;
    ovrTracking rightRemoteTracking_new;

    ovrInputStateGamepad footTrackedRemoteState_old;
    ovrInputStateGamepad footTrackedRemoteState_new;

    ovrDeviceID controllerIDs[2];

    float remote_movementSideways;
    float remote_movementForward;
    float remote_movementUp;
    float positional_movementSideways;
    float positional_movementForward;
    float snapTurn;
} appInput;

extern appInput _appInput;

/*
================================================================================
app
================================================================================
*/

void App_FrameSetup(int controlScheme, int switchSticks, int refreshRate);

void App_PrepareEyeBuffer();

void App_FinishEyeBuffer();

void App_SubmitFrame();

/*
================================================================================
appInput
================================================================================
*/

void Sys_AddMouseMoveEvent(int dx, int dy);

void Sys_AddMouseButtonEvent(int button, bool pressed);

void Sys_AddKeyEvent(int key, bool pressed);

void Android_ButtonChange(int key, int state);

int Android_GetButton(int key);

void Android_SetImpulse(int impulse);

void Android_SetCommand(const char *cmd);

int Android_GetCVarInteger(const char *cvar);

#endif // APP_H