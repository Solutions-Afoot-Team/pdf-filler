FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App


# Copy everything and restore as distinct layers
COPY . ./
RUN dotnet restore

# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .

# Run the app on container startup
# Use your project name for the second parameter
# e.g. MyProject.dll
ENTRYPOINT [ "dotnet", "PDFFiller.dll" ]

# Use the following instead for Heroku
#CMD ASPNETCORE_URLS=http://*:$PORT dotnet PDFFiller.dll