#include <time.h>
#include <cctype>

double GetTimeInMilliSeconds() {
    struct timespec now;
    clock_gettime(CLOCK_MONOTONIC, &now);
    return (now.tv_sec * 1e9 + now.tv_nsec) * (double) (1e-6);
}

int Sys_Milliseconds() {
    static int curtime;
    static int sys_timeBase;
    struct timeval tp;
    struct timezone tzp;
    gettimeofday(&tp, &tzp);
    if (!sys_timeBase) {
        sys_timeBase = tp.tv_sec;
        return tp.tv_usec / 1000;
    }
    curtime = (tp.tv_sec - sys_timeBase) * 1000 + tp.tv_usec / 1000;
    return curtime;
}

static void UnEscapeQuotes(char *arg) {
    char *last = nullptr;
    while (*arg) {
        if (*arg == '"' && *last == '\\') {
            char *c_curr = arg;
            char *c_last = last;
            while (*c_curr) {
                *c_last = *c_curr;
                c_last = c_curr;
                c_curr++;
            }
            *c_last = '\0';
        }
        last = arg;
        arg++;
    }
}

int ParseCommandLine(char *cmdline, char **argv) {
    char *p;
    char *last_p = nullptr;
    int argc = 0, last_argc = 0;
    //argc = last_argc = 0;
    for (p = cmdline; *p;) {
        while (isspace(*p)) ++p;
        if (*p == '"') {
            ++p;
            if (*p) {
                if (argv) argv[argc] = p;
                ++argc;
            }
            while (*p && (*p != '"' || *last_p == '\\')) {
                last_p = p;
                ++p;
            }
        } else {
            if (*p) {
                if (argv) argv[argc] = p;
                ++argc;
            }
            while (*p && !isspace(*p)) ++p;
        }
        if (*p) {
            if (argv) *p = '\0';
            ++p;
        }
        if (argv && last_argc != argc)
            UnEscapeQuotes(argv[last_argc]);
        last_argc = argc;
    }
    if (argv)
        argv[argc] = nullptr;
    return argc;
}