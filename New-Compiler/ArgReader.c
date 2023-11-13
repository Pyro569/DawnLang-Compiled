#include <stdio.h>

#include "Tokenization.c"

int main(int argc, char **argv)
{
    for (int i = 1; i < argc; i++)
    {
        if (0 == strcmp(argv[i], "-b"))
            BuildFile(argv[i + 1]);
        if (0 == strcmp(argv[i], "-br"))
            printf("Build and Run File\n");
        if (0 == strcmp(argv[i], "-d"))
            printf("Development Debug Mode\n");
        if (0 == strcmp(argv[i], "-version"))
            printf("DawnLang Version 1.1.0\n");
    }
}