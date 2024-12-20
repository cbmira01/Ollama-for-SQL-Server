# Ollama Completions for SQL Server - T-SQL Scripts

SQL Server scripts for this project can be found in the DB_Scripts folder.
_
## Script 00

Script00 is the place to find useful small scripts related to viewing
and managing defined CLR functions and assemblies.

## Script 10

Script10 will drop and recreate the TEST database, enable CLR integration for 
UNSAFE assemblies on that database, and will drop and recreate trusted assemblies 
for the Ollama Completions for SQL Server project. 

Script10 is normally run only once for initial installation.

## Script 20

Script20 sets up tables to demonstrate the new CLR functions. It may be conceivably 
extended with additional tables or more data if desired. This script will finally
record the database schema in JSON format, available for consumption by LLM models 
that need to understand the database structure.

Script20 is normally run only once for initial installation.

## Script 30

Script30 will drop all CLR functions and the existing CLR assembly reference,
recreate the link to the most currently released CLR assembly, recreate links to the 
CLR functions, dump a list of all user-defined assemblies and all CLR functions, and
run a short query to the Ollama API server

Make sure your local Ollama API server is running so that the final sanity check
function call will have model support for its completion.

Script30 must be run every time the SQL/CLR project is rebuilt and redeployed.

## Script 40

Script40 is a demonstration of proper setup and call of the various new CLR
functions.

## Script 50

Script50 is a small sentiment analysis study of customer support emails.

## Script 60

Script60 is a demonstration of the QueryFromPrompt feature.

