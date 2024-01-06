FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["VPLink/VPLink.csproj", "VPLink/"]
RUN dotnet restore "VPLink/VPLink.csproj"
COPY . .
WORKDIR "/src/VPLink"
RUN dotnet build "VPLink.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VPLink.csproj" -c Release -o /app/publish

FROM base AS vpsdk
WORKDIR /vpsdk
ADD http://edwin-share.virtualparadise.org/2024/01/vpsdk_20240102_e6701b5e_linux_debian10_x86_64.tar.gz ./vpsdk.tar.gz
RUN echo "5784270749FFC3AD31EFF9BA7DD82203C31968C60DD732A4F76CBB2947C1DB6D vpsdk.tar.gz" | sha256sum -c -&& \
    tar xfv vpsdk.tar.gz --strip-components=1 && \
    rm -r vpsdk.tar.gz include

FROM base AS final
WORKDIR /app
COPY --from=vpsdk /vpsdk/lib/libvpsdk.so .
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VPLink.dll"]
