#include <stdio.h>
#include <stdlib.h>
#include <string.h>

void BuildFile(char FilePath[])
{
    // check if the input file exists/can be found/has rw permissions, if not, exit with code 0
    FILE *file = fopen(FilePath, "r");
    if (file)
    {
        // check if file extension ends with .dl, if not exit the program
        char *fileExtension = strrchr(FilePath, '.');
        if (fileExtension != ".dl")
        {
            printf("File does not end with a .dl extension!\n");
            exit(1);
        }
        else
        {
            //
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