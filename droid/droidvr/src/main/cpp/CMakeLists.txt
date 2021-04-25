# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

cmake_minimum_required(VERSION 3.10.2)
project("droidvr")
get_filename_component(lib_DIR ../jniLibs ABSOLUTE)

include_directories(${lib_DIR}/include)

add_library(droidvr
        SHARED
        lib/egl.cpp
        lib/utils.cpp
        lib/vmath.cpp
        ovr.cpp
        ovrcompositor.cpp
        ovrinput.cpp
        droidvr.cpp)

find_library(android-lib android)
find_library(EGL-lib EGL)
find_library(GLESv3-lib GLESv3)
find_library(log-lib log)

add_library(vrapi-lib SHARED IMPORTED)
set_target_properties(vrapi-lib PROPERTIES IMPORTED_LOCATION ${lib_DIR}/${ANDROID_ABI}/libvrapi.so)

target_link_libraries(droidvr
        vrapi-lib
        ${android-lib}
        ${EGL-lib}
        ${GLESv3-lib}
        ${log-lib})