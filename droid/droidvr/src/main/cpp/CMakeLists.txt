# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

cmake_minimum_required(VERSION 3.10.2)
project("droidvr")
get_filename_component(lib_DIR ../jniLibs ABSOLUTE)

add_library(droidnet
        SHARED
        droidvr.cpp)

find_library(log-lib log)

#add_library(lib_vrapi STATIC IMPORTED)
#set_target_properties(lib_vrapi PROPERTIES IMPORTED_LOCATION ${lib_DIR}/${ANDROID_ABI}/libvrapi.a)

include_directories(${lib_DIR}/include)

target_link_libraries(droidvr
        ${log-lib})