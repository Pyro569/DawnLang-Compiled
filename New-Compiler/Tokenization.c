#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define MAX_LINES 5000
#define MAX_LENGTH 255

char fileContents[MAX_LINES][MAX_LENGTH] = {};
char currentToken[MAX_LENGTH] = {};
char Tokens[MAX_LINES][MAX_LENGTH] = {};

void TokenizeFile()
{
    for (int i = 0; i < MAX_LINES; i++)
    {
        for (int j = 0; j < strlen(fileContents[i]); j++)
        {
            if (fileContents[i][j] != ';' && fileContents[i][j] != '{' && fileContents[i][j] != '}' && fileContents[i][j] != '(' && fileContents[i][j] != ')' && fileContents[i][j] != '\\' && fileContents[i][j] != '"' && fileContents[i][j] != '=' && fileContents[i][j] != '+' && fileContents[i][j] != '-' && fileContents[i][j] != '<' && fileContents[i][j] != '>')
            {
                currentToken[strlen(currentToken)] = fileContents[i][j];
            }
            else
            {
                currentToken[strlen(currentToken)] = fileContents[i][j];
                strncpy(Tokens[sizeof(Tokens) / sizeof(Tokens[0])], currentToken, sizeof(currentToken));
                memset(currentToken, 0, strlen(currentToken));
            }
        }
    }
}

void ReadFile(char FilePath[])
{
    FILE *fptr = fopen(FilePath, "r");
    if (fptr)
    {
        char line[MAX_LENGTH];
        int lineCount = 0;

        while (fgets(line, sizeof(line), fptr) && lineCount < MAX_LINES)
        {
            if (line[strlen(line) - 1] == '\n')
            {
                line[strlen(line) - 1] = '\0';
            }

            strncpy(fileContents[lineCount], line, sizeof(fileContents[lineCount]) - 1);
            fileContents[lineCount][sizeof(fileContents[lineCount]) - 1] = '\0';

            lineCount++;
        }

        fclose(fptr);

        TokenizeFile();
    }
    else
    {
        printf("ERROR: Error in reading file");
        exit(1);
    }
}