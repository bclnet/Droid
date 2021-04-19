#include <string.h>
#include <stdlib.h>
#include <mono/jit/jit.h>
//#include <mono/metadata/object.h>
#include <mono/metadata/environment.h>
#include <mono/metadata/assembly.h>
//#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-config.h>
#include "native-log.h"

// https://www.mono-project.com/docs/advanced/embedding/
// https://github.com/mono/mono/blob/master/samples/embed/test-invoke.c

#ifndef FALSE
#define FALSE 0
#endif

MonoDomain *_domain;

//static void main_(MonoDomain *domain, const char *name, int argc, char **argv) {
//    //create_object(domain, mono_assembly_get_image(assembly));
//}

void PrintMethod(MonoString *string) {
    char *cppString = mono_string_to_utf8(string);
    __android_log_write(ANDROID_LOG_ERROR, ALOG_TAG, cppString);
    mono_free(cppString);
}

void mono_initialize(const char *assemblyDir, const char *configDir, const char *domain, int argc,
                     char *argv[]) {
    ALOGI("MONO_Initialize");
    const char *name = argv[0];
//    ALOGI("DIR: %s - %s", assemblyDir, configDir);
//    ALOGI("DOMAIN: %s::%s", domain, name);
    mono_set_assemblies_path(assemblyDir);
    mono_set_dirs(assemblyDir, configDir);
    _domain = mono_jit_init(domain);

    // Load the binary file as an Assembly
    MonoAssembly *assembly = mono_domain_assembly_open(_domain, name);
    if (!assembly) {
        ALOGE("MONO: Unable to load file %s. exiting.", name);
        return; //exit(0);
    }

    // Namespace.Class::Method + a Function pointer with the actual definition
    //mono_add_internal_call("JellyBitEngine.JellyBitEngine::PrintMethod", &PrintMethod);

    // Call the main method in this code
     mono_jit_exec(_domain, assembly, argc, argv);
    ALOGI("MONO_Ready");
}

int mono_shutdown() {
    ALOGI("MONO_Shutdown");
    int r = mono_environment_exitcode_get();
    mono_jit_cleanup(_domain);
    return r;
}
