function print_goodbye_world(){
    print("Goodbye, World!\n");
}

function print_hello_world(){
    print("Hello, World!\n");
    print_goodbye_world();
}

function main(){
    print_hello_world();
}