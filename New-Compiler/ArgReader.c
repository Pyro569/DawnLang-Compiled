#include <stdio.h>
#include <string.h>

#include "Creation.c"
#include "Tokenization.c"

int main(int argc, char **argv)
{
    for (int i = 1; i < argc; i++)
    {
        if (0 == strcmp(argv[i], "-b"))
            if (argc >= 3)
                BuildFile(argv[i + 1], argv[i + 2]);
        if (0 == strcmp(argv[i], "-br"))
            if (argc >= 3)
            {
                BuildFile(argv[i + 1], argv[i + 2]);
                char command[] = "./";
                strcat(command, argv[i + 2]);
                system(command);
            }
        if (0 == strcmp(argv[i], "-d"))
            debugMode = true;
        if (0 == strcmp(argv[i], "-version"))
            printf("DawnLang Version 1.2.0\n");
    }
}