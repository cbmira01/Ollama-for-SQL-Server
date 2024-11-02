# Ollama Completions for SQL Server

## Overview

Ollama Completions for SQL Server is an SQL/CLR module that allows SQL Server to send prompts to and
get completions back from large language models (LLMs) hosted under Ollama. Multiple models hosted
by Ollama can be discovered and used, such as llama3.2, zephyr and mistral.

## Features

- **Single prompt completion**: Sends a prompt and requests one completion to be returned as a projection.
- **Multiple prompt completion**: Sends a prompt and requests multiple completions to be returned as a table.
- **All Ollama hosted models can be discovered and used**
- **Extensive JSON support library**: CLR 4 has support limitations that affect other JSON libraries
- **Exception handling**: Provides error messages if the API request fails.
- **Test suites for JSON library and SQL/CLR functions**

## Requirements

- .NET Framework 4.7.2, to target CLR 4 for SQL Server 2022
- SQL Server 2022 Express, with CLR integration enabled
- Ollama server on localhost:11434 hosting llama3.2 or other models

## Installation

### Install SQL Server and client

`SQL Server 2022 Express` is sufficient to demonstrate this project. 

In Windows, `SQL Server Configuration Manager` can be used to run and stop database
services as needed. 

Under `SQL Server Service`, place `SQL Server (MSSQLSERVER)` and 
`SQL Server Agent (MSSQLSERVER)` into running or stopped states as desired.

Install a suitable database client to run the deployment scripts and run sample queries.
`SQL Server Management Studio` is sufficient to demonstrate this project.

### Install the Ollama API server

Get the Ollama installer from `https://ollama.com/` or run the Ollama Docker image.
 
However Ollama is installed, make sure your instance will serve on `http://127.0.0.1:11434/ `

Use the Windows command-line interface to interact with Ollama:

`ollama help`

`ollama pull <modelname>` - install an LLM from the Ollama Library

`ollama list` - list all hosted LLM models

`ollama serve` - start the API server

You may consider a tool like Postman to interact with the Ollama API server.

`GET` to `http://127.0.0.1:11434/api/tags` to get a list of models

`POST` to `http://127.0.0.1:11434/api/generate` to send prompts and receive completions

### Clone project repository

```
git clone https://calmiracle.visualstudio.com/OllamaCompletionsForSqlServer/_git/OllamaCompletionsForSqlServer
```

### Open the project in Visual Studio

Build and run the test programs in `Debug` configuration. 

Build in `Release` to create an assembly for use on SQL Server. 


### Deploy to SQL Server

Sample SQL code is provided to declare the functions and link the CLR assembly.

Make sure the deployment script knows where to find your release assembly.

## Using the SQL/CLR functions

After deployment, three SQL/CLR functions can now be used in SQL Server:

`CompletePrompt`, `CompleteMultiplePrompts` and `GetAvailableModels`

### CompletePrompt

Send a prompt to a model and have one completion returned as a projection.

```sql
SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt);
```

    Parameters:
        @modelName: Name of a hosted model, such as 'llama3.2' or 'mistral'
        @askPrompt: Main prompt or question
        @morePrompt: Additional context or information for the prompt

### CompleteMultiplePrompts

Send a prompt to a model and have multiple completions returned in a table.

```sql
SELECT * FROM dbo.CompleteMultiplePrompts(@modelName, @ask, @morePrompt, @numCompletions);
```

    Parameters:
        @modelName: Name of a hosted model, such as 'llama3.2' or 'mistral'
        @askPrompt: Main prompt or question
        @morePrompt: Additional context or information for the prompt
        @numCompletions: Number of prompt completions to retrieve

### GetAvailableModels

Returns information about all Ollama-hosted LLM models in a table.

```sql
SELECT * FROM dbo.GetAvailableModels();
```

### Error Handling

In the event of an error or exception, the response will present an error message:

```
    Error: <HTTP Status Code>
    Exception: <Exception Message>
```

## Examples

Example of the CompletePrompt function:

```sql
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @askPrompt NVARCHAR(MAX) = 'Hello, Llama3.2!.';
DECLARE @morePrompt NVARCHAR(MAX) = 'Tell me about yourself, very briefly.';

SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt);
GO
```

Example of the CompleteMultiplePrompts function, to get completions in a table:

```sql
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @askPrompt NVARCHAR(MAX) = 'What are the benefits of a good credit score?';
DECLARE @morePrompt NVARCHAR(MAX) = 'Please provide a very brief explanation of 10 words or less.';
DECLARE @numCompletions INT = 3;

SELECT * 
FROM dbo.CompleteMultiplePrompts(@modelName, @askPrompt, @morePrompt, @numCompletions);
GO
```

## Notes

- UNSAFE permission is required to allow external HTTP requests.
- Use this with caution as network operations can impact SQL Server performance.