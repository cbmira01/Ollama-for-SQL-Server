# Ollama Completions for SQL Server - Installation

### Requirements to build and deploy

   - **Visual Studio 2022**
   - **.NET Framework 4.7.2 SDK**: Required to target CLR 4 for SQL Server 2022.
   - **SQL Server 2022 Express**: Ensure CLR integration can be enabled.
   - **Ollama API server**: Hosted locally on `http://127.0.0.1:11434/` with one or more LLM models.

### Workstation specifications

This project was developed on, and deployed to, a machine with these specifications:

   - Dell Precision 5560 laptop
   - Intel i9-11950H @ 2.60Ghz, 16 cores
   - 32 GB main memory
   - 1 TB SSD
   - NVIDIA RTX A2000 GPU, 4 GB
   - Windows 11

---

### Install SQL Server and client

1. **Database server**: 
   - **SQL Server 2022 Express** self-hosted is sufficient to demonstrate for this project.
   - Do not deploy this project to a production server without careful study.

2. **Server management**: Use **SQL Server Configuration Manager** to manage database services:
   - Start or stop `SQL Server (MSSQLSERVER)` and `SQL Server Agent (MSSQLSERVER)` as needed.

3. **Database client**: Install a database client to run deployment scripts and queries. 
   - **SQL Server Management Studio** is recommended.

---

### Install and manage the Ollama API server

1. **Get Ollama**: Ollama can be set up with an installer, or run under Docker.
   - [Get the Ollama installer](https://ollama.com/)
   - [Get the Ollama Docker image](https://hub.docker.com/r/ollama/ollama)

2. **Run Ollama**
   - Ensure your Ollama instance serves locally on `http://127.0.0.1:11434/`
   - Ollama API events can be monitored in a console window.

   Ollama supports the following commands:
   - `ollama help`: Display help.
   - `ollama pull <modelname>`: Install a given LLM from the Ollama Library.
   - `ollama list`: List all hosted LLM models.
   - `ollama serve`: Start the API server.

3. **Pull Ollama Models**: Pull one or more large language models to your Ollama API server.
   - [Refer to the Ollama library catalog](https://ollama.com/library)
   - LLM `llama3.2` is a good choice for general completions on natural language prompts.

4. **Optional**: A tool like **Postman** can interact with Ollama's API:
   - `GET` to `http://127.0.0.1:11434/api/tags`: Retrieve a list of models.
   - `POST` to `http://127.0.0.1:11434/api/generate`: Send prompts and receive completions.
   - [Refer to the Ollama API reference](https://github.com/ollama/ollama/blob/main/docs/api.md)

5. **Optional**: Self-hosted web interfaces can also interact with Ollama.
   - [Get Open-WebUI](https://openwebui.com/)
   - [12 Tools to Provide a Web UI for Ollama](https://itsfoss.com/ollama-web-ui-tools/)

---

### Clone the project repository

1. **Clone the Repo**: Clone the project repository from one of the following sources:

```bash
git clone https://calmiracle@dev.azure.com/calmiracle/Ollama-for-SQL-Server/_git/Ollama-for-SQL-Server
```
or
```bash
git clone https://github.com/cbmira01/Ollama-for-SQL-Server
```

2. **Build, test and release**: Open the project in Visual Studio.

   - Build and run the test programs in `JsonClrLibrary.Tests` and `OllamaSqlClr.Tests`, in `Debug` configuration.
   - Run the test suite via the `Test Explorer`.
   - Build in `Release` configuration to create an assembly for SQL Server deployment.

3. **Deploy to SQL Server**: Run T-SQL scripts for CLR assembly linkage

   - Ensure your Ollama API server is running with at least `llama3.2` installed.
   - Ensure the `@RepositoryPath` symbol In `Script10` and `Script30` is set to your repo path.
   - Run `Script10` to set up the `TEST` database and enable CLR integration for it.
   - Run `Script20` to set up demonstration tables and a schema in JSON format.
   - Run `Script30` to link to the current assembly release and create the function links.
   - Ensure the `Script30` sanity check to the Ollama API was successful.
   - Refer to the script documentation for more information.
   - You can now run demonstrations of model completions from `Script40` and `Script50`.

