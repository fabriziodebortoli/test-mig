set /p release="Enter Release branch name: release/"

cd ..
git worktree add C:\development-release\Standard\TaskBuilder\ release/%release%
cd ..\Applications\ERP\
git worktree add C:\development-release\Standard\Applications\ERP\ origin/release/%release%
cd ..\TBF\
git worktree add C:\development-release\Standard\Applications\TBF\ origin/release/%release%
cd ..\TBS\
git worktree add C:\development-release\Standard\Applications\TBS\ origin/release/%release%