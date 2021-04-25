# For more information about using CMake with Android Studio, read the
# documentation: https://d.android.com/studio/projects/add-native-code.html

cmake_minimum_required(VERSION 3.10.2)
project("droidjs")
get_filename_component(lib_DIR ../jniLibs ABSOLUTE)

include_directories(${lib_DIR}/include)

add_library(droidjs
        SHARED
        droidjs.cpp)

find_library(log-lib log)

#add_library(vrapi-lib STATIC IMPORTED)
#set_target_properties(vrapi-lib PROPERTIES IMPORTED_LOCATION ${lib_DIR}/${ANDROID_ABI}/libvrapi.a)

target_link_libraries(droidjs
        ${log-lib})