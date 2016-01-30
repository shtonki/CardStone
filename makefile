csc /recurse:src/*.cs /r:System.Numerics.dll /r:Connection.dll /out:stonekart.exe
mkdir game
mv stonekart.exe game/
cp Connection.dll game/
cp res game/res -r