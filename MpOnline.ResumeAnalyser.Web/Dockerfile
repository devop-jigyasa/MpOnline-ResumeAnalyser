FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MpOnline.ResumeAnalyser.Web/MpOnline.ResumeAnalyser.Web.csproj", "MpOnline.ResumeAnalyser.Web/"]
RUN dotnet restore "MpOnline.ResumeAnalyser.Web/MpOnline.ResumeAnalyser.Web.csproj"

COPY . .
WORKDIR "/src/MpOnline.ResumeAnalyser.Web"
RUN dotnet build "MpOnline.ResumeAnalyser.Web.csproj" -c Release -o /app/build
RUN dotnet publish "MpOnline.ResumeAnalyser.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MpOnline.ResumeAnalyser.Web.dll"]
