# Ollama Completions for SQL Server

## Overview

`Ollama Completions for SQL Server` enables SQL Server users to use Large Language Models 
(LLMs) hosted on Ollama directly within SQL workflows. By using SQL/CLR functions, users can 
send prompts to multiple LLMs, such as `llama3.2`, `zephyr`, and `mistral`, and receive 
language-based completions without leaving the SQL Server environment. 

Use this project for natural language processing tasks such as:

- data classification
- data summaries
- entity extraction
- conversational responses
- sentiment analysis

## License and Disclaimer

Ollama Completions for SQL Server is open source and licensed under the MIT License. 
By contributing, you agree that your contributions will be licensed under the same terms.

Disclaimer: This project is experimental and intended for educational or research 
purposes. It is provided “as-is” without warranties or guarantees of performance. 
Please use caution in production environments, as external API calls can impact SQL Server 
performance and may require additional security considerations.

## Contributions

Bug reports, feature requests, code contributions, and documentation are welcome.

## Features

- **Single prompt completion**: Sends a prompt and requests one completion to be returned as a projection.
- **Multiple prompt completion**: Sends a prompt and requests multiple completions to be returned as a table.
- **All Ollama hosted models can be discovered and used**
- **Extensive JSON support library**: CLR 4 has support limitations that affect other JSON libraries
- **Exception handling**: Provides error messages if the API request fails.
- **Test suites for JSON library and SQL/CLR functions**

## Requirements

- `.NET Framework 4.7.2 SDK` to target CLR 4 for SQL Server 2022
- `SQL Server 2022 Express`, with CLR integration enabled
- `Ollama API server` on `http://127.0.0.1:11434/` hosting one or more LLM models

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

- UNSAFE assembly permission is required to allow external HTTP requests from SQL Server.
- Use these function with caution as network operations can impact SQL Server performance.

This project was written to investigate the Ollama API, and the use of AI as a code companion.

Over 90% of the code in this project was written by GPT 4.

This project needs much better documentation and test coverage of the JSON library.

## Roadmap for future work: possible functions for SQL Server to interact with LLMs

### AskAllModels Function
- **Description:** A function that sends a prompt to all available hosted models and returns their responses in a table format.
- **Usage:** This function can be useful for comparing outputs from different models, performing ensemble analysis, or selecting the best response based on certain criteria.
- **Output Structure:** The function could return a table with columns such as `ModelName`, `Response`, `ConfidenceScore`, and `Metadata`.

### SummarizeText Function
- **Description:** Takes a large text input and returns a concise summary generated by an LLM.
- **Usage:** Useful for generating summaries of lengthy documents stored in the database, such as logs, reports, or articles.

### ClassifyText Function
- **Description:** Classifies text data into predefined categories using an LLM.
- **Usage:** Helpful for tagging content, sentiment analysis, or organizing unstructured text data.

### GenerateSQLQuery Function
- **Description:** Translates natural language questions into SQL queries using an LLM trained for code generation.
- **Usage:** Enables users to write queries in plain English, which are then converted into executable SQL commands.

### TranslateText Function
- **Description:** Translates text from one language to another using an LLM with translation capabilities.
- **Usage:** Useful for international applications where data needs to be accessible in multiple languages.

### ExplainQueryPlan Function
- **Description:** Uses an LLM to provide a human-readable explanation of SQL query execution plans.
- **Usage:** Assists developers and DBAs in understanding complex query plans and optimizing queries.

### GenerateInsights Function
- **Description:** Analyzes data patterns and generates insights or recommendations.
- **Usage:** Can be used for business intelligence, identifying trends, or anomaly detection.

### ExtractEntities Function
- **Description:** Extracts entities such as names, dates, locations, etc., from text data.
- **Usage:** Useful for structuring unstructured data, populating relational tables from text fields.

### ChatWithData Function
- **Description:** Allows users to have a conversational interaction with the data, asking questions and receiving answers generated by an LLM.
- **Usage:** Enhances data exploration and accessibility for non-technical users.
