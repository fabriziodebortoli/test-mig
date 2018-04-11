set watch=false
IF [%~1]==[watch] IF NOT [%~2]==[] set watch=%~2

node --max_old_space_size=9120 "node_modules\@angular\cli\bin\ng" build --no-sourcemaps --output-path="..\..\..\WebFramework\M4Rs" --bh /development/M4Rs/ --watch=%watch%