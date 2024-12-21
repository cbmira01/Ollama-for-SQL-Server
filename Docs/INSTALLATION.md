# Ollama Completions for SQL Server - Installation

## Installation

### Requirements

- **.NET Framework 4.7.2 SDK**: Required to target CLR 4 for SQL Server 2022.
- **SQL Server 2022 Express**: Ensure CLR integration is enabled.
- **Ollama API server**: Hosted on `http://127.0.0.1:11434/` with one or more LLM models.

---

### Install SQL Server and Client

**SQL Server 2022 Express** is sufficient for this project.

Use **SQL Server Configuration Manager** to manage database services:
- Start or stop `SQL Server (MSSQLSERVER)` and `SQL Server Agent (MSSQLSERVER)` as needed.

Install a database client for deployment scripts and queries. **SQL Server Management Studio** is recommended.

---

### Install the Ollama API Server

1. **Get the Installer**: Download from [ollama.com](https://ollama.com/) or use the Ollama Docker image.

2. **Run Ollama**: Ensure Ollama serves on `http://127.0.0.1:11434/`.

   Use the following commands in the Ollama CLI:
   - `ollama help`: Display help.
   - `ollama pull <modelname>`: Install an LLM from the Ollama Library.
   - `ollama list`: List all hosted LLM models.
   - `ollama serve`: Start the API server.

3. **Optional**: Use a tool like **Postman** to interact with Ollama's API:
   - `GET` to `http://127.0.0.1:11434/api/tags`: Retrieve a list of models.
   - `POST` to `http://127.0.0.1:11434/api/generate`: Send prompts and receive completions.

4. **Optional**: A self-hosted AI web interface such as **Open-WebUI** can also be used to interact with Ollama.

---

### Clone the Project Repository

1. Clone the project repository from one of the following sources:

```bash
git clone https://calmiracle@dev.azure.com/calmiracle/Ollama-for-SQL-Server/_git/Ollama-for-SQL-Server
```
or
```bash
git clone https://github.com/cbmira01/Ollama-for-SQL-Server
```

2. Open the project in Visual Studio

   - Build and run the test programs in Debug configuration.
   - Run the test sute via the Test Explorer.
   - Build in Release configuration to create an assembly for SQL Server deployment.

3. Deploy to SQL Server

Use the provided SQL Server deployment script, Script10, to:

   - Declare the SQL/CLR functions
   - Link the CLR assembly

Note: Ensure the deployment script points to your assembly release location.
