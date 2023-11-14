#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#include "Tokenization.c"

void BuildFile(char FilePath[])
{
    // check if the input file exists/can be found/has rw permissions, if not, exit with code 0
    FILE *file = fopen(FilePath, "r");
    if (file)
        // check if file extension ends with .dl, if not exit the program
        for (int i = 0; i < strlen(FilePath); i++)
            if (FilePath[i] != '.')
                i += 1;
            else
            {
                if (FilePath[i] == '.' && FilePath[i + 1] == 'd' && FilePath[i + 2] == 'l')
                {
                    // continue
                    ReadFile(FilePath);
                }
                else
                {
                    printf("ERROR: File to build does not end in .dl extension!\n");
                    exit(1);
                }
            }
    else
    {
        printf("ERROR: Failed to find file!\n");
        printf("Reasons for this error can include:\n");
        printf("-File does not exist\n-Program does not have permissions to read file\n-Incorrect file path\n");
        exit(1);
    }
}