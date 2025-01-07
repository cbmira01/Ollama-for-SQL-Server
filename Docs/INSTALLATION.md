# Ollama Completions for SQL Server - Installation

### Requirements to build and deploy

   - **Visual Studio 2022**: Community Edition is sufficient.
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
   - **SQL Server 2022 Express** Can run self-hosted, which is sufficient to demonstrate for this project.
   - Do not deploy this project to a production server without careful study.

2. **Server management**: Use **SQL Server Configuration Manager** to manage database services:
   - Start or stop `SQL Server (MSSQLSERVER)` and `SQL Server Agent (MSSQLSERVER)` as needed.

3. **Database client**: Install a database client to run deployment scripts and queries. 
   - **SQL Server Management Studio** is recommended.

---

### Install and manage the Ollama API server

1. **Get Ollama**: Ollama can be run in a command console, or under Docker.
   - [Get the Ollama installer](https://ollama.com/)
   - [Get the Ollama Docker image](https://hub.docker.com/r/ollama/ollama)

2. **Run Ollama**
   - Ensure your Ollama instance serves locally on `http://127.0.0.1:11434/`
   - Ollama API events can be monitored in a console window.

   Ollama supports the following commands:
   - `ollama help`: Display help.
   - `ollama pull <modelname>`: Install a given LLM from the Ollama library.
   - `ollama list`: List all hosted LLM models.
   - `ollama serve`: Start the API server.

3. **Pull Ollama Models**: Pull one or more large language models to your Ollama API server.
   - LLM `llama3.2` is a good choice for general completions on natural language prompts.
   - LLM `mistral` is very successful at producing SQL queries from natural-language prompts.
   - LLM `llava` is very successful at image analysis.
   - [Refer to the Ollama library](https://ollama.com/library)

4. **Optional**: A tool like **Postman** can be useful when interacting with Ollama:
   - `GET` to `http://127.0.0.1:11434/api/tags`: Retrieve a list of models.
   - `POST` to `http://127.0.0.1:11434/api/generate`: Send prompts and receive completions.
   - `POST` to `http://127.0.0.1:11434/api/show`: To get detailed information about a hosted model.
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

2. **Build, test and release**
   - Open the project in Visual Studio.
   - Build the solution in `Debug` configuration.
   - Run the main console program in `JsonClrLibrary.Tests`.
   - Run the main console program in `OllamaSqlClr.Tests`.
   - Open the test suite via `Test Explorer`, and run all tests.
   - Build the solution in `Release` configuration, then deploy to SQL Server.

3. **Deploy to SQL Server**: Link CLR assemblies, load demonstration data
   - Ensure your Ollama API server is running with at least `llama3.2` installed.
   - Ensure the `@RepositoryPath` symbols In `Script10` and `Script30` are set to your repo path.
   - Run `Script10` to set up the `TEST` database and enable CLR integration for it.
   - Run `Script20` to set up sample data tables, and produce a schema reference.
   - Run `Script30` to link to the current CLR assembly release and create the function links.
   - Ensure the `Script30` sanity check to the Ollama API runs successfully.
   - Refer to the script documentation for more information.
   - You can now run demonstrations of model completions from `Script40`, `Script50` and `Script60`.

4. **Optional**: Load data for image analysis demonstrations
   - Complete the procedures in step 3 above.
   - Ensure your Ollama API server running, and has LLM model `llava` installed.
   - Image files can be bulk-inserted into the Images table by enabling and running `Script22`.
   - Alternatively, image files can be loaded by running the console program `LoadImageFiles`.
   - Refer to documentation in `Script22` for more information.
   - You can now run demonstrations of image analysis from `Script70`.
