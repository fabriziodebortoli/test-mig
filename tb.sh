#!/bin/bash
echo Hello World
bash --init-file <(echo "cd ./client/web-sandbox/; ng serve --port=4300")

read -rsp $'Press any key or wait 5 seconds to continue...\n' -n 1 -t 5;