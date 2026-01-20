# Configuring Docker
---
## Default configuration: combined backend
The default configuration allows you to quickly deploy the entire backend to a single host. This will place both the database and a container to capture live traffic on the same host as the frontend which renders the visualization. This is best for simple, small-scale setups.  
1. Install the docker engine if you have not already: https://docs.docker.com/engine/install/
2. Optionally, install docker desktop: https://docs.docker.com/desktop/ (scroll down to install Docker Desktop and select either Mac, Windows, or Linux)
3. Start the containers using docker compose from your system shell:
``` sh
cd parser/NetVis/docker		# from the repository root directory
docker compose up -d
```
4. Verify the containers started correctly: 
	- From the CLI, you should see "Running 3/3" (one service and two containers)  
	- Within docker desktop, browse to containers, and verify the `netvis-stack` service is running and the `clickhouse` and `netvis-dev` containers under it are running  
	- Run `docker ps` from your system's command line and ensure you see two containers, and both are running
5. Once data is loaded, verify that the program renders it. NetVis is already configured to look for the database on the same host as the visualization by default.  

## Split deployment
You may wish to deploy one or both services on separate hosts. The connection between hosts should be low-latency for live data to support a high volume of database insertions and reads.  
1. Clone the repository to the host, or at a minimum place the `parser/` directory on the host you wish to deploy on.
2. Install the docker engine, and optionally, docker desktop (step 1-2 above)
3. Use one of the below commands to start the containers, based on your preferred deployment:
``` sh
cd parser/NetVis/docker				# from the repository root directory

docker compose up -d				# starts both database and live capture utility
docker compose up -d clickhouse		# starts the database only
docker compose up -d netvis			# starts only the live traffic capture tool
```
4. Verify the backend is up and running with the same commands as step 4 above. 
5. Modify the frontend connector to point to the backend database. This file is located at `Assets/Scripts/DBConnection.cs`. Modify the variable `DB_HOST` from localhost to the host your database is deployed to
6. If deploying the traffic capture on a different device than the database, make note of the hostname or IP address of your database. You will need to point the parser utility to the database at runtime. See `live-capture-ingest.md` for more information.  

## Further customization
Authentication to the database uses credentials specified in the `docker-compose.yml` file. We recommend changing these from the defaults. Update the `CLICKHOUSE_USER` and `CLICKHOUSE_PASSWORD` variables on lines 12 and 13. Then update the credentials used in `Assets/Scripts/DBConnection.cs` by changing the `DB_USER` and `DB_PASSWORD` variables on lines 99 and 100.

## Clearing Database
The database persists when the containers are restarted by using a docker volume.  
If you want to clear the data from the database, or rebuild the containers after modifying the schema, run `docker compose down --volumes` to delete the volumes from your host. Then follow the standard deployment process above.  