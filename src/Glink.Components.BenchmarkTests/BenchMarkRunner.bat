echo "Start BenchMark"
dotnet clean --configuration Release
dotnet build --configuration Release
dotnet run -c Release
echo "End"