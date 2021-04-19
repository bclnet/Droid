#include <jni.h>
#include <string>
#include <unistd.h>
#include <pthread.h>

#include "native-log.h"

static JavaVM *_jVM;

int start_logger();

void mono_initialize(const char *assemblyDir, const char *configDir, const char *domain, int argc,
                     char *argv[]);

void mono_shutdown();

jint JNI_OnLoad(JavaVM *vm, void *reserved) {
    ALOGI("DROIDNET_OnLoad");
    start_logger();
    JNIEnv *env;
    _jVM = vm;
    if (_jVM->GetEnv((void **) &env, JNI_VERSION_1_4) != JNI_OK) {
        ALOGE("Failed JNI_OnLoad");
        return -1;
    }
    return JNI_VERSION_1_4;
}

void JNI_Shutdown() {
    mono_shutdown();
    ALOGI("DROIDNET_Shutdown");
    JNIEnv *env;
    if (_jVM->GetEnv((void **) &env, JNI_VERSION_1_4) < 0)
        _jVM->AttachCurrentThread(&env, NULL);
}

extern "C" JNIEXPORT void JNICALL
Java_com_contoso_droidnet_DroidNet_monoJNI(JNIEnv *env, jobject /* this */,
                                                       jstring assemblyDir, jstring configDir,
                                                       jstring domain, jstring name) {
    jboolean iscopy;
    const char *assemblyDirUTF = env->GetStringUTFChars(assemblyDir, &iscopy);
    const char *configDirUTF = env->GetStringUTFChars(configDir, &iscopy);
    const char *domainUTF = env->GetStringUTFChars(domain, &iscopy);
    const char *nameUTF = env->GetStringUTFChars(name, &iscopy);
    char *assemblyDirZ =
            assemblyDirUTF && strlen(assemblyDirUTF) ? strdup(assemblyDirUTF) : nullptr;
    char *configDirZ = configDirUTF && strlen(configDirUTF) ? strdup(configDirUTF) : nullptr;
    char *domainZ = domainUTF && strlen(domainUTF) ? strdup(domainUTF) : nullptr;
    char *nameZ = nameUTF && strlen(nameUTF) ? strdup(nameUTF) : nullptr;
    env->ReleaseStringUTFChars(assemblyDir, assemblyDirUTF);
    env->ReleaseStringUTFChars(configDir, configDirUTF);
    env->ReleaseStringUTFChars(domain, domainUTF);
    env->ReleaseStringUTFChars(name, nameUTF);
    char *argv[1] = {nameZ};
    mono_initialize(assemblyDirZ, configDirZ, domainZ, 1, argv);
}

static int pfd[2];
static pthread_t thr;

static void *thread_func(void *arg) {
    ssize_t rdsz;
    char buf[128];
    while ((rdsz = read(pfd[0], buf, sizeof buf - 1)) > 0) {
        if (buf[rdsz - 1] == '\n') --rdsz;
        buf[rdsz] = 0;
        __android_log_write(ANDROID_LOG_INFO, ALOG_TAG, buf);
    }
    return nullptr;
}

int start_logger() {
    ALOGI("start_logger()");
    /* make stdout line-buffered and stderr unbuffered */
    setvbuf(stdout, 0, _IOLBF, 0);
    setvbuf(stderr, 0, _IONBF, 0);

    /* create the pipe and redirect stdout and stderr */
    pipe(pfd);
    dup2(pfd[1], 1);
    dup2(pfd[1], 2);

    /* spawn the logging thread */
    if (pthread_create(&thr, nullptr, thread_func, 0) == -1)
        return -1;
    pthread_detach(thr);
    return 0;
}

