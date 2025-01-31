# Ollama Completions for SQL Server - Usage

## Using the SQL/CLR functions

After deployment, these new functions can be used in SQL Server:

- [`CompletePrompt`](#completeprompt)
- [`CompleteMultiplePrompts`](#completemultipleprompts)
- [`GetAvailableModels`](#getavailablemodels)
- [`QueryFromPrompt`](#queryfromprompt)
- [`ExamineImage`](#examineimage)

- [`Error handling, and other usage examples`](#error-handling-and-other-examples)

---

## CompletePrompt

Send a prompt to a model and obtain one completion as a projection.

```sql
SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt);
```

**Parameters:**
- `@modelName`: Name of a hosted model.
- `@askPrompt`: Main prompt or question.
- `@morePrompt`: Additional context or information for the prompt.

### Example

```sql
DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @askPrompt NVARCHAR(MAX) = 'Hello, Llama3.2!';
DECLARE @morePrompt NVARCHAR(MAX) = 'Tell me about yourself, very briefly.';

SELECT dbo.CompletePrompt(@modelName, @askPrompt, @morePrompt) As Result;
GO
```

Response:

| Result                                                                              |
|-------------------------------------------------------------------------------------|
| Nice to meet you! I'm an AI designed to provide information and answer questions... |

[`Using the new SQL/CLR functions`](#using-the-sqlclr-functions)

---

## CompleteMultiplePrompts

Send a prompt to a model and obtain multiple completions in a table.

```sql
SELECT * FROM dbo.CompleteMultiplePrompts(@modelName, @ask, @morePrompt, @numCompletions);
```

**Parameters:**
- `@modelName`: Name of a hosted model.
- `@askPrompt`: Main prompt or question.
- `@morePrompt`: Additional context or information for the prompt.
- `@numCompletions`: Number of prompt completions to retrieve.

### Example

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
|------------------------------------------|-------------|----------------------------------------------------------------|
| 7B4D35CA-AC44-4781-B716-430E4FD4EAB6     | llama3.2    | Better loan rates, lower interest payments, and increased financial flexibility. |
| 55AF396D-730E-4629-83CD-D0BAE431D928     | llama3.2    | Lower interest rates, better loan terms, and increased financial options.        |
| 04C515A3-66A4-4976-9CB1-CD76FFE8D105     | llama3.2    | Savings on loans, lower interest rates, and greater financial security.          |

[`Using the new SQL/CLR functions`](#using-the-sqlclr-functions)

---

## GetAvailableModels

Retrieve in a table information about all LLM models currently hosted on Ollama.

```sql
SELECT * FROM dbo.GetAvailableModels();
```

### Example

```sql
SELECT * FROM dbo.GetAvailableModels();
```

Response: 

| ModelGuid   | Name                | Model               | ReferToName | ModifiedAt               | Size       | Family | ParameterSize | QuantizationLevel | Digest    |
|-------------|---------------------|---------------------|-------------|--------------------------|------------|--------|---------------|-------------------|-----------|
| 0D95C299... | codegemma:latest    | codegemma:latest    | codegemma   | 2024-11-03 16:48:33.450  | 5011852809 | gemma  | 9B            | Q4_0              | 0c9...    |
| 8A7C7B43... | mistral:latest      | mistral:latest      | mistral     | 2024-11-02 00:12:38.160  | 4113301824 | llama  | 7.2B          | Q4_0              | f97...    |
| B9549A9B... | zephyr:latest       | zephyr:latest       | zephyr      | 2024-10-27 11:51:03.533  | 4109854934 | llama  | 7B            | Q4_0              | bbe...    |
| B58CBD1F... | llama3.2:latest     | llama3.2:latest     | llama3.2    | 2024-09-30 10:37:15.627  | 2019393189 | llama  | 3.2B          | Q4_K_M            | a80...    |

[`Using the new SQL/CLR functions`](#using-the-sqlclr-functions)

---

## QueryFromPrompt

Send a natural-language prompt to a model and obtain an SQL query along with the result of its execution. 

QueryFromPrompt is aware of the current database schema and will do its best effort to build an SQL Server query, 
run it, and show its results in a standard (JSON) format.

```sql
SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO
```

**Parameters:**
- `@modelName`: Name of a hosted model.
- `@prompt`: Natural-language prompt to generate an SQL query.

### Example

```sql
DECLARE @modelName NVARCHAR(MAX) = 'mistral';
DECLARE @prompt NVARCHAR(MAX) = 'What was the date and time of the earliest purchase?';

SELECT * FROM dbo.QueryFromPrompt(@modelName, @prompt);
GO
```

Response:

| QueryGuid    | ModelName | Prompt   | ProposedQuery   | Result   | Timestamp   |
|--------------|-----------|----------|-----------------|----------|-------------|
| 5E968560...  | mistral   | What was the date and time of the earliest purchase? | SELECT TOP 1 SaleDate FROM Sales   | [{"SaleDate": "12/1/2024 10:15:00 AM"}] | 2024-12-29 05:13:47.593 |

[`Using the new SQL/CLR functions`](#using-the-sqlclr-functions)

---

## ExamineImage

Obtain a model completion on a prompt and JPEG image data, as a projection.

```sql
SELECT dbo.ExamineImage(@modelName, @prompt, @imageData);
```

**Parameters:**
- `@modelName`: Name of a hosted model.
- `@prompt`: Main prompt or question.
- `@imageData`: JPEG image data in VARBINARY(MAX) format.

### Example

```sql
DECLARE @FileName NVARCHAR(100) = 'pexels-brunoscramgnon-596134-moon_resized.jpg';
DECLARE @Prompt NVARCHAR(100) = 'Do you recognize anything in this image?';
DECLARE @ModelName NVARCHAR(100) = 'llava';
DECLARE @ImageData VARBINARY(MAX);

SELECT @ImageData = ImageData FROM Images WHERE FileName = @FileName;

SELECT dbo.ExamineImage(@ModelName, @Prompt, @ImageData) AS Result;
GO
```

Response:

| Result   |
|----------|
| The image shows a full moon illuminated against the night sky. Below the moon, there appears to be a silhouette... |

[`Using the new SQL/CLR functions`](#using-the-sqlclr-functions)

---

## Error handling and other examples

In case of an error or exception, the response will include an error message:

```
    Error: <HTTP Status Code>
    Exception: <Exception Message>
```

Other usage examples of the new SQL/CLR functions can be found in the `DB_Scripts` solution folder.

[`Using the new SQL/CLR functions`](#using-the-sqlclr-functions)
