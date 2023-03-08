# DnaVastgoed

This is a script to crawle DnaVastgoed.be for properties.

To run this project:

- Run `dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p { password here }` to create the HTTPS certificate
- Run `dotnet dev-certs https --trust` to trust the HTTPS certificate
- Copy the `.env.dist` to `.env` and fill in the correct values
- Create a folder `Database` in the root of the project
- Run `docker-compose build` to build the docker image
- Run `docker-compose up -d` to run the script

The server is avaalable on `https://localhost:5001`

**Note: Run all commands in current folder**
