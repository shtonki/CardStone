csc *.cs GUI/*.cs /r:System.Numerics.dll /r:Connection.dll /out:stonekart.exe
rm -r game
mkdir game
cp stonekart.exe game
cp Connection.dll game/
cp res game/res -r