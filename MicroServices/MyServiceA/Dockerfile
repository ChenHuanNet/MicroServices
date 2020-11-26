
#FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
#如果linux系统是ubuntu 使用这个
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic
WORKDIR /app
EXPOSE 80
#WORKDIR /usr/local/jenkins/workspace/MyBid

RUN apt-get update -y && apt-get install -y libgdiplus && apt-get clean && ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll

COPY . .

ENTRYPOINT ["dotnet", "MyServiceA.dll"]