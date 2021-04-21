#include <jni.h>
#include <string>
#include "native-log.h"

jint JNI_OnLoad(JavaVM *vm, void *reserved) {
    ALOGI("DROID_OnLoad");
    return JNI_VERSION_1_4;
}

extern "C" JNIEXPORT jstring JNICALL
Java_com_contoso_droid_MainActivity_stringFromJNI(
        JNIEnv* env,
        jobject /* this */) {
    std::string hello = "Hello from C++";
    return env->NewStringUTF(hello.c_str());
}