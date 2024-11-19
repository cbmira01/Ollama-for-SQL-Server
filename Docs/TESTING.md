# Ollama Completions for SQL Server - TESTING

## Testing and Debugging

This guide explains how to test and debug the components of `Ollama Completions for SQL Server`, including console programs, 
unit tests, and remote debugging for SQL/CLR functions.

---

### Console Programs

Two console programs are available for testing:

1. **JSON Library Tests**:
   - Configure the solution to `Debug`.
   - Set the `JsonClrLibrary.Tests` project as the `Startup Project`.
   - Click **Start** to run the tests.
   - Build and test results will appear in the `Output` window.

2. **Integration Tests with the Ollama Server**:
   - Ensure the Ollama server is running on `http://127.0.0.1:11434`.
   - Configure the solution to `Debug`.
   - Set the `OllamaSqlClr.Tests` project as the `Startup Project`.
   - Click **Start** to execute the tests.
   - Monitor API events in the Ollama console for additional context.

---

### Unit Tests

Unit tests are provided for:
- The JSON library.
- SQL/CLR functions.

To run unit tests:
1. Open Visual Studio's **Test Explorer**.
2. Click **Run All Tests in View**.
3. External services required for these tests are mocked, so no live dependencies are needed.

---

### Debugging the SQL/CLR Functions Remotely

You can attach a debugger to the SQL Server process to debug the SQL/CLR functions. Follow these steps:

1. **Preparation**:
   - Run Visual Studio as an administrator.
   - Configure the solution to `Release`, then **Build** the solution.
   - Ensure debugging symbols (`.pdb` files) are present in the build output (`OllamaSqlClr.pdb`, `JsonClrLibrary.pdb`).
   - Start the Ollama server.
   - Set SQL Server and its agent to a running state. Note the process ID of the `SQL Server` service.

2. **Create Assembly Links**:
   - Run `Script10` in SQL Server to create the assembly and function links using T-SQL.

3. **Attach Debugger**:
   - Open Visual Studio's **Debug** menu.
   - Select **Attach to Process...**.
   - In the **Connection Type**, choose `Local`.
   - Filter process names by typing `sql`.
   - Select the process ID matching the SQL Server service noted earlier.
   - Under **Code Type**, select `Managed (.NET Framework 4.x) code`.
   - Click **Attach** (you may see a warning modal).

4. **Set Breakpoints**:
   - Place breakpoints in managed code where debugging is needed (e.g., `SqlClrFunctions` or `OllamaService`).
   - Invoke SQL/CLR functions from SQL Server to trigger the breakpoints.

---

### Development Workflow

When making changes:
1. **Build and Test**:
   - Build in `Debug` mode and ensure all tests pass.
2. **Create a Release Build**:
   - Build the solution in `Release` mode to generate the assembly.
3. **Redeploy Assembly**:
   - Run `Script10` in SQL Server to update the assembly links.
4. **Debug Remotely**:
   - Attach the debugger to SQL Server as described above.

---

By following these steps, you can effectively test, debug, and enhance `Ollama Completions for SQL Server`.