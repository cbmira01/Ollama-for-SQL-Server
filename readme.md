# Ollama Completions for SQL Server

## Overview

`Ollama Completions for SQL Server` brings the power of Large Language Models (LLMs) directly 
to SQL Server workflows. By leveraging SQL/CLR functions, users can seamlessly interact with 
LLMs locally hosted on Ollama, such as `llama3.2`, `zephyr`, and `mistral`.

With this project, you can perform advanced natural language processing (NLP) tasks directly 
within SQL Server, including:

- **Data Classification**: Automatically categorize text data into predefined groups.
- **Data Summaries**: Generate concise summaries of lengthy documents, logs, or articles.
- **Entity Extraction**: Identify and extract structured entities like names, dates, and locations from text.
- **Conversational Responses**: Create interactive, natural language outputs for chat-like experiences.
- **Sentiment Analysis**: Determine the emotional tone of textual data for insights.
- **SQL Query Generation**: Generate SQL queries from natural-language prompts.
- **Image Analysis**: Describe presented images, recognize features and text in images.

This project enables SQL Server users to harness the power of LLMs without leaving their 
database environment, making it ideal for data-driven NLP tasks.

## Key Features

- **SQL/CLR Integration**: Use familiar SQL workflows to send prompts and receive model completions.
- **Flexible Outputs**: Retrieve results as single projections or structured tables.
- **Model Support**: Easily switch between multiple LLMs hosted on Ollama.

## Documentation

Refer to the documentation files:

+ [INSTALLATION](Docs/INSTALLATION.md): How to clone and set up this project
+ [SCRIPTS](Docs/SCRIPTS.md): T-SQL scripts for installation and demonstration
+ [USAGE](Docs/USAGE.md): How to use the defined CLR functions
+ [JSON-LIBRARY](Docs/JSON-LIBRARY.md): JSON building and handling library
+ [TESTING](Docs/TESTING.md): How to test and debug this project
+ [CONTRIBUTING](Docs/CONTRIBUTING.md): How to contribute to this project
+ [ROADMAP](Docs/ROADMAP.md): Future new functions that may be implemented
+ [WORKS-CONSULTED](Docs/WORKS-CONSULTED.md): More information about Ollama and SQL/CLR

## Why this project?

- What are locally-hosted large language models capable of?
- How can LLM capabilities be pulled into SQL Server?
- How does one interact with Ollama's API surface?
- How well can LLMs write SQL code from natural-language prompts?
- How are SQL/CLR functions implemented?
- How well does an LLM (ChatGPT) work as a coding companion?

## Notes

- UNSAFE assembly permission is required to allow external HTTP requests from SQL Server.
- Use these functions with caution as network operations can impact SQL Server performance.
- Do not deploy this project to a production instance of SQL Server without careful consideration.
