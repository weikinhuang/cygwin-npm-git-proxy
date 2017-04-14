# cygwin-npm-git-proxy
Proxy for using npm git repos in cygwin


## Build with

```bash
winpty cmd /c "$(cygpath -w "$(find /c/Windows/Microsoft.NET/Framework* -iname msbuild.exe | sort | tail -1)") $(cygpath -w "$PWD/git-npm-proxy.sln") /t:Rebuild /p:Configuration=Release"
```
