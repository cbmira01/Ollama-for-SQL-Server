# Ollama Completions for SQL Server - Installation

### Requirements to build and deploy

   - **Visual Studio 2022**: Community Edition is sufficient.
   - **.NET Framework 4.7.2 SDK**: Required to target CLR 4 for SQL Server 2022.
   - **SQL Server 2022 Express**: Ensure CLR integration can be enabled.
   - **Ollama API server**: Hosted locally with one or more LLM models.

### Workstation specifications

This project was developed on, and deployed to, a machine with these specifications:

   - Dell Precision 5560 laptop
   - Intel i9-11950H @ 2.60Ghz, 16 cores
   - 32 GB main memory
   - 1 TB SSD
   - NVIDIA RTX A2000 GPU, 4 GB
   - Windows 11

---

### Install and manage SQL Server

1. **Database server**: 
   - **SQL Server 2022 Express** Can run self-hosted, which is sufficient to demonstrate this project.
   - Do not deploy this project to a production database server without careful study.

2. **Server management**: Use **SQL Server Configuration Manager** to manage database services:
   - Start or stop `SQL Server (MSSQLSERVER)` and `SQL Server Agent (MSSQLSERVER)` as needed.

3. **Database client**: Install a database client to run demonstration queries. 
   - **SQL Server Management Studio** is recommended.

---

### Install and manage the Ollama API server

1. **Get Ollama**: Ollama can be installed via MSI or under Docker.
   - [Get the Ollama MSI installer](https://ollama.com/)
   - [Get the Ollama Docker image](https://hub.docker.com/r/ollama/ollama)

2. **Run Ollama**
   - Ensure your Ollama instance serves locally on `http://127.0.0.1:11434/`
   - Ollama API events can be monitored in a CLI console window.

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

4. **Optional**: An API development tool like **Postman** or **Hoppscotch** can be useful when interacting with Ollama:
   - `GET` to `http://127.0.0.1:11434/api/tags`: Retrieve a list of models.
   - `POST` to `http://127.0.0.1:11434/api/generate`: Send prompts and receive completions.
   - `POST` to `http://127.0.0.1:11434/api/show`: To get detailed information about a hosted model.
   - [Refer to the Ollama API reference](https://github.com/ollama/ollama/blob/main/docs/api.md)

5. **Optional**: Self-hosted web interfaces can also interact with Ollama.
   - [Get Open-WebUI](https://openwebui.com/)
   - [12 Tools to Provide a Web UI for Ollama](https://itsfoss.com/ollama-web-ui-tools/)

---

### Clone, build and deploy the project

1. **Clone the Repo**: Clone the project repository from one of the following sources:

```bash
git clone https://calmiracle@dev.azure.com/calmiracle/Ollama-for-SQL-Server/_git/Ollama-for-SQL-Server
```
or
```bash
git clone https://github.com/cbmira01/Ollama-for-SQL-Server
```

2. **Configure**
   - Open the solution in Visual Studio.
   - Configuration settings are in the `AppConfig` class in the `Configuration` project.
   - The project is currently configured for a typical single-workstation deployment.

3. **Build, test and release**
   - Open the solution in Visual Studio.
   - Build the solution in `Debug` configuration.
   - Run the main console program in the `JsonClrLibrary.Tests` project.
   - Run the main console program in the `OllamaSqlClr.Tests` project.
   - Open the test suite via `Test Explorer`; run all the tests discovered there.
   - Build the solution in `Release` configuration.

4. **Deploy to SQL Server**
   - Open the solution in Visual Studio.
   - Ensure a successful `Release` build has been performed.
   - Ensure `SQL Server` is running.
   - Ensure `Ollama API server` is running with the `llama3.2` model installed.
   - Run the main console program in the `DeploymentManager` project to perform deployment steps.
   - Environment check: External services `SQL Server` and `Ollama API server` should report ready.
   - Environment check: You should be able to list and interact with hosted models.
   - Required: Perform all the deployment steps for initial installation, in order.
   - Required: Perform the CLR relink deployment step. Ensure the sanity check runs successfully.
   - Optional: The current deployment can be checked or reverted at any time.
   - You are now ready to run demonstration scripts from the DB_Scripts folder.

5. **Optional**: Demonstrations for image analysis and SQL code generation.
   - Complete the deployment steps above.
   - Different large language models excel in various tasks.
   - Various LLMs are available from the [Ollama library.](https://ollama.com/library)
   - For image analysis demonstrations, I recommend the `llava` model.
   - For demonstrations of SQL code generation, I recommend the `mistral` model.
   - Your own investigations may lead you to use other large language models.
