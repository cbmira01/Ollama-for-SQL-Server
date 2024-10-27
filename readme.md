# Ollama Completions for SQL Server

## Overview

Ollama Completions for SQL Server is an SQL/CLR module that allows SQL Server to send prompts to and
get completions back from a large language model (LLM) hosted under Ollama. Currently, the only model 
targeted is llama3.2.

## Features

- **Single Prompt Completion**: Sends a single prompt and returns the completion from Ollama.
- **Multiple Prompt Completion**: Sends a prompt and requests multiple completions from Ollama, and returns them in a table.
- **Exception Handling**: Provides error messages if the API request fails.

## Requirements

- .NET Framework (4.7.2, to target CLR 4 for SQL Server 2022)
- SQL Server with CLR integration enabled
- Ollama server hosting llama3.2 running on localhost

## Installation

### Clone the repository

```
more about this later
```

### Open the project in Visual Studio

### Deploy to SQL Server

Sample SQL code is provided to declare the functions and link the CLR assembly.

## Usage

This class exposes two functions that can be used in SQL Server:
`CompletePrompt` and `CompleteMultiplePrompts`

### CompletePrompt

This function sends a prompt to Ollama and returns the completion.

SQL Server Usage:

```sql
SELECT dbo.CompletePrompt(askPrompt, additionalPrompt);
```

    Parameters:
        askPrompt: The main prompt or question
        additionalPrompt: Additional context or information for the prompt

### CompleteMultiplePrompts

This function sends a prompt multiple times to Ollama and returns the completions in a table.

SQL Server Usage:

```sql

SELECT * FROM dbo.CompleteMultiplePrompts(ask, body, numCompletions);
```

    Parameters:
        askPrompt: The main prompt or question
        additionalPrompt: Additional context or information for the prompt
        numCompletions: The number of prompt completions to retrieve

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