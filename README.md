DawnLang is a high-level compiled programming language with the speed of C and the relative simplicity of python!  
  
To get started with DawnLang clone this repository and compile it. Next, make a file ending in the extension .dl, this will be our DawnLang file. To get started writing DawnLang, you can check out the example files folder in this repository or use the example below:  
`#import dawnlang.io;`  
``  
`function main(){`  
`  print("Hello, World!\n");`  
`}`  
After creating this file, to compile it use the compiler application, an example of a command to compile it is `./"DawnLang Compiler" "-b" "helloworld.dl" "hello world"`  
To compile and run the file immediately after, switch the "-b" parameter for "-br"  

Currently a few requirements are needed to compile DawnLang files, so make sure you meet all of the following requirements:  
  -Linux Distribution or Windows Subsystem for Linux  
  -GCC C Compiler  
