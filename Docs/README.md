# Ollama Completions for SQL Server

## Overview

`Ollama Completions for SQL Server` brings the power of Large Language Models (LLMs) directly to SQL Server workflows. 
By leveraging SQL/CLR functions, users can seamlessly interact with LLMs hosted on Ollama, such as `llama3.2`, `zephyr`, and `mistral`.

With this project, you can perform advanced natural language processing (NLP) tasks directly within SQL Server, including:

- **Data Classification**: Automatically categorize text data into predefined groups.
- **Data Summaries**: Generate concise summaries of lengthy documents, logs, or articles.
- **Entity Extraction**: Identify and extract structured entities like names, dates, and locations from text.
- **Conversational Responses**: Create interactive, natural language outputs for chat-like experiences.
- **Sentiment Analysis**: Determine the emotional tone of textual data for insights.

This project enables SQL Server users to harness the power of LLMs without leaving their database environment, 
making it ideal for data-driven NLP tasks.

### Key Features:
- **SQL/CLR Integration**: Use familiar SQL workflows to send prompts and receive model completions.
- **Flexible Outputs**: Retrieve results as single projections or structured tables.
- **Model Support**: Easily switch between multiple LLMs hosted on Ollama.

### Documentation

Refer to the documentation files in order:
- **README.md**
- **INSTALLATION.md**
- **USAGE.md**
- **TESTING.md**
- **CONTRIBUTING.md**
- **JSON-LIBRARY.md** (coming soon)
- **ROADMAP.md**

### Notes

- UNSAFE assembly permission is required to allow external HTTP requests from SQL Server.
- Use these functions with caution as network operations can impact SQL Server performance.
- This project was written to investigate the Ollama API, and the use of AI as a code companion.
