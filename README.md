# DnaVastgoed

This is a script to crawle DnaVastgoed.be for properties.

To run this project:

- Run `dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p { password here }` and `dotnet dev-certs https --trust` to trust the certificate
- Create a folder `Database` in the root of the project
- Copy the `.env.dist` to `.env` and fill in the correct values
- Run `docker-compose build` to build the docker image
- Run `docker-compose up -d` to run the script
