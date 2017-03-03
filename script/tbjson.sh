#!/bin/bash -x
cd ../../
./taskbuilder/Framework/TbUtility/TbJson/TbJson.exe /ts .

read -rsp $'Press any key or wait 5 seconds to continue...\n' -n 1 -t 5;