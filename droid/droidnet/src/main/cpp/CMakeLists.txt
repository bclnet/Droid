# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

cmake_minimum_required(VERSION 3.10.2)
project("droidnet")
get_filename_component(lib_DIR ../libs ABSOLUTE)

add_library(droidnet
        SHARED
        MonoHost.cpp
        droidnet.cpp)

find_library(log-lib log)

# Adds the mono
add_library(lib_monosgen STATIC IMPORTED)
set_target_properties(lib_monosgen PROPERTIES IMPORTED_LOCATION ${lib_DIR}/${ANDROID_ABI}/libmonosgen-2.0.a)
include_directories(${lib_DIR}/${ANDROID_ABI}/include/mono-2.0)

target_link_libraries(droidnet
        lib_monosgen
        ${log-lib})