FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
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
ADD http://static.virtualparadise.org/dev-downloads/vpsdk_20231217_422bcbfc_linux_debian10_x86_64.tar.gz ./vpsdk.tar.gz
RUN echo "5F5710EEFC1FC2C5246434A52B9DC05F477A6EFE91394001FAA4097F5CA7BC77 vpsdk.tar.gz" | sha256sum -c -&& \
    tar xfv vpsdk.tar.gz --strip-components=1 && \
    rm -r vpsdk.tar.gz include

FROM base AS final
WORKDIR /app
COPY --from=vpsdk /vpsdk/lib/libvpsdk.so .
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VPLink.dll"]
