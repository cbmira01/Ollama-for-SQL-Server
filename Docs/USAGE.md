# Ollama Completions for SQL Server - Usage

## Using the SQL/CLR functions

After deployment, these new functions can be used in SQL Server:

- `CompletePrompt`
- `CompleteMultiplePrompts`
- `GetAvailableModels`
- `QueryFromPrompt`

---

### CompletePrompt

Send a prompt to a model and return one completion as a projection.

```sql
SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt);
```

**Parameters:**
- `@modelName`: Name of a hosted model, such as 'llama3.2' or 'mistral'.
- `@askPrompt`: Main prompt or question.
- `@morePrompt`: Additional context or information for the prompt.

---

### CompleteMultiplePrompts

Send a prompt to a model and return multiple completions in a table.

```sql
SELECT * FROM dbo.CompleteMultiplePrompts(@modelName, @ask, @morePrompt, @numCompletions);
```

**Parameters:**
- `@modelName`: Name of a hosted model, such as 'llama3.2' or 'mistral'.
- `@askPrompt`: Main prompt or question.
- `@morePrompt`: Additional context or information for the prompt.
- `@numCompletions`: Number of prompt completions to retrieve.

---

### GetAvailableModels

Retrieve information about all Ollama-hosted LLM models in a table.

```sql
SELECT * FROM dbo.GetAvailableModels();
```

---

### QueryFromPrompt

Send a natural-language prompt to a model and return an SQL query along with the result of its execution. 

QueryFromPrompt is aware of the current database schema and will do its best effort to build an 
SQL query, run it, and show its results in a standard (JSON) format.

```sql
SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO
```

**Parameters:**
- `@modelName`: Name of a hosted model, such as 'llama3.2' or 'mistral'.
- `@prompt`: Natural-language prompt to generate an SQL query.

---

### Error Handling

In case of an error or exception, the response will include an error message:

```
    Error: <HTTP Status Code>
    Exception: <Exception Message>
```

## Examples

### Example: CompletePrompt Function

```sql
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @askPrompt NVARCHAR(MAX) = 'Hello, Llama3.2!';
DECLARE @morePrompt NVARCHAR(MAX) = 'Tell me about yourself, very briefly.';

SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt);
GO
```

Response:

| (No column name)                                                                    |
|-------------------------------------------------------------------------------------|
| Nice to meet you! I'm an AI designed to provide information and answer questions... |

---

### Example: CompleteMultiplePrompts Function

```sql
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @askPrompt NVARCHAR(MAX) = 'What are the benefits of a good credit score?';
DECLARE @morePrompt NVARCHAR(MAX) = 'Please provide a very brief explanation of 10 words or less.';
DECLARE @numCompletions INT = 3;

SELECT * 
FROM dbo.CompleteMultiplePrompts(@modelName, @askPrompt, @morePrompt, @numCompletions);
GO
```

Response:

| CompletionGuid                           | CompletedBy | OllamaCompletion                                               |
|------------------------------------------|-------------|-----------------------------------------------------------------|
| 7B4D35CA-AC44-4781-B716-430E4FD4EAB6     | llama3.2    | Better loan rates, lower interest payments, and increased financial flexibility. |
| 55AF396D-730E-4629-83CD-D0BAE431D928     | llama3.2    | Lower interest rates, better loan terms, and increased financial options.        |
| 04C515A3-66A4-4976-9CB1-CD76FFE8D105     | llama3.2    | Savings on loans, lower interest rates, and greater financial security.          |

---

### Example: GetAvailableModels

```sql
SELECT * FROM dbo.GetAvailableModels();
```

Response: 

| ModelGuid                               | Name                | Model               | ReferToName | ModifiedAt               | Size       | Family | ParameterSize | QuantizationLevel | Digest                                                             |
|-----------------------------------------|---------------------|---------------------|-------------|--------------------------|------------|--------|---------------|-------------------|-------------------------------------------------------------------|
| A5C88631-EE70-4905-AEC4-19013FEC7205    | codegemma:latest    | codegemma:latest    | codegemma   | 2024-11-03 16:48:33.450 | 5011852809 | gemma  | 9B            | Q4_0              | 0c96700aaada572ce9bb6999d1fda9b53e9e6cef5d74fda1e066a1ba811b93f3 |
| BD0D1E59-05FF-4C7B-B67F-7169B2D08318    | mistral:latest      | mistral:latest      | mistral     | 2024-11-02 00:12:38.160 | 4113301824 | llama  | 7.2B          | Q4_0              | f974a74358d62a017b37c6f424fcdf2744ca02926c4f952513ddf474b2fa5091 |
| 0AD644A7-69F5-4B61-8949-881D4EB62D79    | zephyr:latest       | zephyr:latest       | zephyr      | 2024-10-27 11:51:03.533 | 4109854934 | llama  | 7B            | Q4_0              | bbe38b81adec6be8ff951d148864ed15a368aa2e8534a5092d444f184a56e354 |
| 08FB2B3D-5C1A-4624-9147-D18F2A7E6902    | llama3.2:latest     | llama3.2:latest     | llama3.2    | 2024-09-30 10:37:15.627 | 2019393189 | llama  | 3.2B          | Q4_K_M            | a80c4f17acd55265feec403c7aef86be0c25983ab279d83f3bcd3abbcb5b8b72 |

---

### Example: QueryFromPrompt

```sql
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What was the date and time of the earliest purchase?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO
```

Response:

| QueryGuid                              | ModelName | Prompt                                   | ProposedQuery                      | Result                                   | Timestamp               |
|----------------------------------------|-----------|------------------------------------------|-------------------------------------|------------------------------------------|-------------------------|
| 5E968560-90CF-456A-A1AB-1C4BA6935715   | mistral   | What was the date and time of the earliest purchase? | SELECT TOP 1 SaleDate FROM Sales   | [{"SaleDate": "12/1/2024 10:15:00 AM"}] | 2024-12-29 05:13:47.593 |

---
Further examples can be found in `Script40`, `Script50` and `Script60` in the `DB_Scripts` folder.
