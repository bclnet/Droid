#include <jni.h>
#include <unistd.h>
#include "app.h"
#include "native-log.h"

void AppMain(int argc, char **argv) {
    ALOGI("APPMAIN");
    while (true) {
        App_FrameSetup(0, 0, 60);
        usleep(1000);
        App_SubmitFrame();
    }
}

void Sys_AddMouseMoveEvent(int dx, int dy) {
    ALOGI("Sys_AddMouseMoveEvent");
}

void Sys_AddMouseButtonEvent(int button, bool pressed) {
    ALOGI("Sys_AddMouseButtonEvent");
}

void Sys_AddKeyEvent(int key, bool pressed) {
    ALOGI("Sys_AddKeyEvent");
}

void Android_ButtonChange(int key, int state) {
    ALOGI("Android_ButtonChange");
}

int Android_GetButton(int key) {
    ALOGI("Android_GetButton");
    return 0;
}

void Android_SetImpulse(int impulse) {
    ALOGI("Android_SetImpulse");
}

void Android_SetCommand(const char *cmd) {
    ALOGI("Android_SetCommand");
}

int Android_GetCVarInteger(const char *cvar) {
    ALOGI("Android_GetCVarInteger");
    return 0;
}