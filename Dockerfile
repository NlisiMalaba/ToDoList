FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy source into /src (so we get /src/ToDoList.Api, /src/ToDoList.Domain, ...)
COPY src/ .

WORKDIR /src/ToDoList.Api
RUN dotnet restore ToDoList.Api.csproj
RUN dotnet publish ToDoList.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "ToDoList.Api.dll"]

