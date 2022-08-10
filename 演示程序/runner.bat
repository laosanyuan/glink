echo "Start"

cd Demo-DataVisualization
start .\Glink.Demo.exe

cd ..\Demo-Calculation
start .\Glink.WebApi.exe --urls http://*:5000

start microsoftedge http://localhost:5000/swagger/index.html

echo "End"




