# Ollama Completions for SQL Server

## Overview

Ollama Completions for SQL Server is an SQL/CLR module that allows SQL Server to send prompts to and
get completions back from a large language model (LLM) hosted under Ollama. Multiple models hosted
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

SQL Server 2022 Express is sufficient to demonstrate this project. 

In Windows, `SQL Server Configuration Manager` can be used to run and stop database
services as needed. Under `SQL Server Service`, place `SQL Server (MSSQLSERVER)` and 
`SQL Server Agent (MSSQLSERVER)` into running or stopped states as desired.

Install a suitable database client to run the deployment scripot and run sample queries.
SQL Server Management Studio is sufficient to demonstrate this project.

### Install Ollama

### Clone project repository

```
git clone https://calmiracle.visualstudio.com/OllamaCompletionsForSqlServer/_git/OllamaCompletionsForSqlServer
```

### Open the project in Visual Studio

Build and run the test programs in Debug configuration. 

Build in Release to create an assembly for use on SQL Server. 


### Deploy to SQL Server

Sample SQL code is provided to declare the functions and link the CLR assembly.
Make sure the deployment script knows where to find your release assembly.

## Usage

Class SqlClrFunctions exposes three functions that can be used in SQL Server:
`CompletePrompt`, `CompleteMultiplePrompts` and `GetAvailableModels`

### CompletePrompt

This function sends a prompt to Ollama and returns the completion.

SQL Server Usage:

```sql
SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt);
```

    Parameters:
        @modelName: A hosted model name, such as 'llama3.2' or 'mistral'
        @askPrompt: The main prompt or question
        @morePrompt: Additional context or information for the prompt

### CompleteMultiplePrompts

This function sends a prompt multiple times to Ollama and returns the completions in a table.

SQL Server Usage:

```sql

SELECT * FROM dbo.CompleteMultiplePrompts(@modelName, @ask, @morePrompt, @numCompletions);
```

    Parameters:
        @modelName: A hosted model name, such as 'llama3.2' or 'mistral'
        @askPrompt: The main prompt or question
        @morePrompt: Additional context or information for the prompt
        @numCompletions: The number of prompt completions to retrieve

### Error Handling

In the event of an error or exception, the response will have an error code:

```
    Error: <HTTP Status Code>
    Exception: <Exception Message>
```

## Examples

Here is an example of calling the CompletePrompt function:

```sql
DECLARE @askPrompt NVARCHAR(MAX) = 'Hello, Llama3.2!.';
DECLARE @additionalPrompt NVARCHAR(MAX) = 'Tell me about yourself, very briefly.';

SELECT dbo.CompletePrompt(@askPrompt, @additionalPrompt);
GO
```

Here is an example of calling the CompleteMultiplePrompts function to get the completions in a table:

```sql
DECLARE @askPrompt NVARCHAR(MAX) = 'What are the benefits of a good credit score?';
DECLARE @additionalPrompt NVARCHAR(MAX) = 'Please provide a very brief explanation of 10 words or less.';
DECLARE @numCompletions INT = 3;

SELECT * 
FROM dbo.CompleteMultiplePrompts(@askPrompt, @additionalPrompt, @numCompletions);
GO
```

## Notes

- UNSAFE permission is required to allow external HTTP requests.
- Use this with caution as network operations can impact SQL Server performance.