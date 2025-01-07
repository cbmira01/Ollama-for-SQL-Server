# Ollama Completions for SQL Server - T-SQL Scripts

SQL Server scripts for this project are found in the DB_Scripts folder.
_
## Script 00

`Script00` is a repository for miscellaneous scripts related to managing CLR assemblies. 

## Script 10

`Script10` will drop and recreate the TEST database, enable CLR integration for 
UNSAFE assemblies on that database, and will drop and recreate trusted assemblies 
for the Ollama Completions for SQL Server project. 

`Script10` is normally run only once for initial installation.

## Script 20

`Script20` sets up tables to demonstrate the new CLR functions. This is where additional
tables can be established and populated, if desired.

This script additionally sets up a KeyValuePair table to hold complex prompts for the 
QueryFromPrompt feature, and to record the database schema in JSON format. The schema 
is available for consumption by LLM models that need to understand the current database 
structure.

`Script20` is normally run only once for initial installation.

## Script 22

`Script22` is one option for bulk-inserting image files into the Images table. 

The preconditions for running this script are:
    - that Script10 has established the TEST database;
    - that Script20 has established the Images table;
    - that image files are available in a folder that xp_cmdshell can read;
    - and that the bulk-insert code is enabled in this script.

The other option for loading image files is by running the `LoadImageFiles` console program.

## Script 30

`Script30` will drop all CLR functions and the existing CLR assembly reference,
recreate the link to the most currently released CLR assembly, recreate links to the 
CLR functions, dump a list of all user-defined assemblies and all CLR functions, and
run a short query to the Ollama API server

Make sure your local Ollama API server is running so that the final sanity check
function call will have model support for its completion.

`Script30` must be run every time the SQL/CLR project is rebuilt and released.

## Script 40

`Script40` is a demonstration of proper setup and call of the various new CLR functions.

## Script 50

`Script50` has demostrations of support email sentiment, keyword extraction and 
email routing.

## Script 60

`Script60` is a demonstration of the QueryFromPrompt feature, where SQL queries are
constructed and run from natural language prompts.

## Script 70

`Script70` contains demonstrations of image analysis. Data for these demonstrations
is bulk-inserted by `Script22` or by the `LoadImageFiles` console program.
