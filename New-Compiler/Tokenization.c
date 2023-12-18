#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>

bool debugMode = false;

#define MAX_LINES 5000
#define MAX_LENGTH 255

char fileContents[MAX_LINES][MAX_LENGTH] = {};
char currentToken[MAX_LENGTH] = {};
char Tokens[MAX_LINES][MAX_LENGTH] = {};
char convertedTokens[MAX_LINES][MAX_LENGTH] = {};

char IntNames[500][255] = {};
char Ints[500] = {};
int IntsDeclared = 0;

char Strings[500][255] = {};
int StringsDeclared = 0;

int tokenSpot = 0;

void TokenizeFile(char BinaryPath[])
{
    int tokenSpot = 0;
    int hasNonEmptyChar = 0; // Flag to track if a line has non-empty characters

    for (int i = 0; i < MAX_LINES; i++)
    {
        int tokenIndex = 0;
        hasNonEmptyChar = 0; // Reset flag for each line
        for (int j = 0; j < strlen(fileContents[i]); j++)
        {
            if (fileContents[i][j] == ';' || fileContents[i][j] == '{' || fileContents[i][j] == '}' || fileContents[i][j] == '(' || fileContents[i][j] == ')' || fileContents[i][j] == '"' || fileContents[i][j] == '=' || fileContents[i][j] == '+' || fileContents[i][j] == '-' || fileContents[i][j] == '<' || fileContents[i][j] == '>' || fileContents[i][j] == ' ' || fileContents[i][j] == '[' || fileContents[i][j] == ']')
            {
                if (tokenIndex > 0)
                {
                    currentToken[tokenIndex] = '\0'; // Null-terminate the token
                    strncpy(Tokens[tokenSpot], currentToken, sizeof(currentToken));
                    tokenSpot++;
                    tokenIndex = 0;
                    memset(currentToken, 0, sizeof(currentToken));
                }
                if (fileContents[i][j] != ' ')
                { // Include spaces as separate tokens
                    currentToken[0] = fileContents[i][j];
                    // currentToken[1] = '\0'; // Null-terminate the currentToken
                    strncpy(Tokens[tokenSpot], currentToken, sizeof(currentToken));
                    tokenSpot++;
                }
            }
            else
            {
                currentToken[tokenIndex] = fileContents[i][j];
                tokenIndex++;
                hasNonEmptyChar = 1; // Set the flag for non-empty character
            }
        }
        // Check for non-empty line and add its token if present
        if (hasNonEmptyChar && tokenIndex > 0)
        {
            currentToken[tokenIndex] = '\0'; // Null-terminate the token
            strncpy(Tokens[tokenSpot], currentToken, sizeof(currentToken));
            tokenSpot++;
            memset(currentToken, 0, sizeof(currentToken));
        }
    }

    if (debugMode == 1)
        printf("Tokenization complete\n");

    if (debugMode == 1)
        for (int k = 0; k < sizeof(Tokens) / sizeof(Tokens[0]); k++)
            printf("%s\n", Tokens[k]);

    Compile(BinaryPath);
}

void ReadFile(char FilePath[], char BinaryPath[])
{
    FILE *fptr = fopen(FilePath, "r");
    if (fptr)
    {
        char line[MAX_LENGTH];
        int lineCount = 0;

        while (fgets(line, sizeof(line), fptr) && lineCount < MAX_LINES)
        {
            strncpy(fileContents[lineCount], line, sizeof(fileContents[lineCount]) - 1);
            fileContents[lineCount][sizeof(fileContents[lineCount]) - 1] = '\0';

            lineCount++;
        }

        fclose(fptr);

        TokenizeFile(BinaryPath);
    }
    else
    {
        printf("ERROR: Error in reading file");
        exit(1);
    }
}

void addConvertedToken(char Token[], int *location)
{
    strcpy(convertedTokens[*location], Token);
    (*location) += 1;
}

void Compile(char BinaryPath[])
{
    int convertedTokenLocation = 0;

    if (debugMode == 1)
        printf("Compiling\n");

    int numTokens = sizeof(Tokens) / sizeof(Tokens[0]); // Calculate the number of tokens once

    for (int i = 0; i < numTokens - 1; i++)
    {
        if (0 == strcmp(Tokens[i], "function") && 0 == strcmp(Tokens[i + 1], "main") && 0 == strcmp(Tokens[i + 2], "("))
        {
            addConvertedToken("int main(", &convertedTokenLocation);
            if (0 == strcmp(Tokens[i + 4], "args)"))
                addConvertedToken("int argc, char** argv)", &convertedTokenLocation);
            else
                addConvertedToken(")", &convertedTokenLocation);
        }
        else if (0 == strcmp(Tokens[i], "{"))
        {
            addConvertedToken("{", &convertedTokenLocation);
        }
        else if (0 == strcmp(Tokens[i], "}"))
        {
            addConvertedToken("}", &convertedTokenLocation);
        }
        else if (0 == strcmp(Tokens[i], "print"))
        {
            addConvertedToken("printf(\"", &convertedTokenLocation);
            if (0 == strcmp(Tokens[i + 2], "\""))
            {
                for (int z = i + 3; z < sizeof(Tokens) / sizeof(Tokens[0]); z++)
                    if (0 == strcmp(Tokens[z], "\""))
                    {
                        addConvertedToken("\"", &convertedTokenLocation);
                        break;
                    }
                    else
                    {
                        addConvertedToken(Tokens[z], &convertedTokenLocation);

                        if (0 != strcmp(Tokens[z + 1], "\""))
                            addConvertedToken(" ", &convertedTokenLocation);
                    }
                addConvertedToken(");", &convertedTokenLocation);
            }
        }
        else if (0 == strcmp(Tokens[i], "#include"))
        {
            addConvertedToken("#include", &convertedTokenLocation);
            if (0 == strcmp(Tokens[i + 1], "dawnlang.io"))
                addConvertedToken("<stdio.h>\n", &convertedTokenLocation);
            else if (0 == strcmp(Tokens[i + 1], "dawnlang.data.types"))
                addConvertedToken("<string.h>\n", &convertedTokenLocation);
        }
        else if (0 == strcmp(Tokens[i], "C"))
        {
            if (0 == strcmp(Tokens[i + 1], "["))
                for (int z = i + 2; z < sizeof(Tokens) / sizeof(Tokens[0]); z++)
                    if (0 != strcmp(Tokens[z + 1], "-") && 0 != strcmp(Tokens[z + 2], "End"))
                    {
                        addConvertedToken(Tokens[z], &convertedTokenLocation);

                        // if (0 != strcmp(Tokens[z + 2], "-") && 0 != strcmp(Tokens[z + 3], "End"))
                        //{
                        //     addConvertedToken(" ", &convertedTokenLocation);
                        //  z++;
                        //}
                        // else
                        //    break;
                    }
                    else
                        break;
        }
        else if (0 == strcmp(Tokens[i], "int"))
        {
            addConvertedToken(Tokens[i], &convertedTokenLocation);
            addConvertedToken(" ", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 1], &convertedTokenLocation);
            if (0 == strcmp(Tokens[i + 2], "="))
            {
                addConvertedToken(Tokens[i + 2], &convertedTokenLocation);
                addConvertedToken(Tokens[i + 3], &convertedTokenLocation);

                Ints[IntsDeclared] = atoi(Tokens[i + 3]);
                strncpy(IntNames[IntsDeclared], Tokens[i + 2], sizeof(Tokens[i + 2]));
                IntsDeclared += 1;
            }
            addConvertedToken(";", &convertedTokenLocation);

            char Empty[] = "";
            for (int j = i + 1; j < i + 3; j++)
                strncpy(Tokens[j], Empty, sizeof(Empty));
        }
        else if (0 == strcmp(Tokens[i], "print.int"))
        {
            addConvertedToken("printf(\"%%d\\n\",", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 2], &convertedTokenLocation);
            addConvertedToken(");", &convertedTokenLocation);
        }
        else if (0 == strcmp(Tokens[i], "string"))
        {
            addConvertedToken("char ", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 1], &convertedTokenLocation);
            addConvertedToken("[] = \"", &convertedTokenLocation);

            for (int z = i + 4; z < i + 7; z++)
            {
                addConvertedToken(Tokens[z], &convertedTokenLocation);
                if (0 != strcmp(Tokens[z + 1], "\""))
                    addConvertedToken(" ", &convertedTokenLocation);
            }
            addConvertedToken(";", &convertedTokenLocation);

            char Empty[] = "";
            for (int j = i + 1; j < i + 7; j++)
                strncpy(Tokens[j], Empty, sizeof(Empty));
        }
        else if (0 == strcmp(Tokens[i], "print.string"))
        {
            addConvertedToken("printf(\"%%s\\n\",", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 2], &convertedTokenLocation);
            addConvertedToken(");", &convertedTokenLocation);
        }
        else if (0 == strcmp(Tokens[i], "realloc"))
        {
            addConvertedToken("strncpy(", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 2], &convertedTokenLocation);
            addConvertedToken("\"", &convertedTokenLocation);

            int stopPoint = 0;

            for (int j = i + 4; j < numTokens; j++)
                if (0 != strcmp(Tokens[j], "\""))
                {
                    addConvertedToken(Tokens[j], &convertedTokenLocation);
                    if (0 != strcmp(Tokens[j + 1], "\""))
                        addConvertedToken(" ", &convertedTokenLocation);
                }
                else
                {
                    stopPoint = j;
                    break;
                }

            addConvertedToken("\",sizeof(", &convertedTokenLocation);

            // remove the comma from the end of Tokens[i+2] because I am too lazy to fix the lexer to add commas as a seperate token
            int length = strlen(Tokens[i + 2]);
            char modified[length];
            strncpy(modified, Tokens[i + 2], length - 1);
            modified[length - 1] = '\0';

            addConvertedToken(modified, &convertedTokenLocation);
            addConvertedToken("));", &convertedTokenLocation);

            char Empty[] = "";
            for (int j = i; j < stopPoint; j++)
                strncpy(Tokens[j], Empty, sizeof(Empty));
        }
        else if (0 == strcmp(Tokens[i], "function"))
        {
            int stopSpot = 0;

            addConvertedToken("void ", &convertedTokenLocation);
            for (int j = i + 1; j < numTokens - 1; j++)
                if (0 != strcmp(Tokens[j], "{"))
                {
                    if (0 == strcmp(Tokens[j], "string"))
                    {
                        addConvertedToken("char ", &convertedTokenLocation);
                        addConvertedToken(Tokens[j + 1], &convertedTokenLocation);
                        addConvertedToken("[]", &convertedTokenLocation);
                        j++;
                    }
                    else
                        addConvertedToken(Tokens[j], &convertedTokenLocation);

                    if (0 == strcmp(Tokens[j], "int"))
                    {
                        Ints[IntsDeclared] = atoi(Tokens[j + 1]);
                        strncpy(IntNames[IntsDeclared], Tokens[i + 2], sizeof(Tokens[i + 2]));
                        IntsDeclared += 1;
                    }

                    if (0 != strcmp(Tokens[j + 1], "{"))
                        addConvertedToken(" ", &convertedTokenLocation);
                }
                else
                {
                    stopSpot = j;
                    break;
                }

            char Empty[] = "";
            for (int j = i; j < stopSpot; j++)
                strncpy(Tokens[j], Empty, sizeof(Empty));
        }
        else if (0 == strcmp(Tokens[i], "free"))
        {
            addConvertedToken("memset(", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 2], &convertedTokenLocation);
            addConvertedToken(", 0, sizeof(", &convertedTokenLocation);
            addConvertedToken(Tokens[i + 2], &convertedTokenLocation);
            addConvertedToken("));", &convertedTokenLocation);
        }
        // else
        //{
        //     for (int l = 0; l < sizeof(IntNames) / sizeof(IntNames[0]); l++)
        //     {
        //         if (0 == strcmp(Tokens[i], IntNames[l]))
        //         {
        //             addConvertedToken(Tokens[i], &convertedTokenLocation);
        //             break;
        //         }
        //     }
    }
}

Finishing(BinaryPath);
}

void Finishing(char BinaryPath[])
{
    FILE *fptr = fopen("Main.cpp", "w");
    for (int i = 0; i < sizeof(convertedTokens) / sizeof(convertedTokens[0]); i++)
        fprintf(fptr, convertedTokens[i]);
    fclose(fptr);

    system("gcc Main.cpp -w -lstdc++");

    char command[] = "mv a.out ";
    strcat(command, BinaryPath);
    system(command);

    exit(1);
}