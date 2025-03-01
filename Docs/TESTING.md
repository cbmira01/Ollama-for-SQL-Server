# Ollama Completions for SQL Server - Testing

## Testing and Debugging

This guide explains how to test and debug the components of `Ollama Completions for SQL Server`, 
via console programs, unit tests, and remote debugging. Use of Visual Studio is assumed.

---

### Console Programs

Two console programs are available for integration testing:

1. **JSON Library Tests**:
   - Configure the solution to `Debug`.
   - Set the `JsonClrLibrary.Tests` project as the `Startup Project`.
   - Click **Start** to run the tests.
   - Build and test results will appear in the `Output` window.

2. **Integration Tests with the Ollama API Server**:
   - Ensure the Ollama server is running.
   - Configure the solution to `Debug`.
   - Set the `OllamaSqlClr.Tests` project as the `Startup Project`.
   - Click **Start** to run the tests.
   - Monitor API events in the Ollama console for additional context.

---

### Unit Tests

Unit tests are provided for:

   - The JSON library
   - SQL/CLR functions
   - The Ollama service layer

To run unit tests:

1. Open Visual Studio's **Test Explorer**
2. Click **Run All Tests in View**
3. External services required for these tests are mocked, so no live dependencies are needed.

---

### Debugging the SQL/CLR Functions Remotely

You can attach a debugger to the SQL Server process to debug SQL/CLR functions remotely. 

Here's how:

1. **Preparation**
   - Run Visual Studio as an administrator.
   - Configure the solution to `Release`, then **Build** the solution.
   - Ensure debugging symbols (`.pdb` files) are present in the build output (`OllamaSqlClr.pdb`, `JsonClrLibrary.pdb`).
   - Start the Ollama server.
   - Set SQL Server and its agent to a running state. Note the process ID of the `SQL Server` service.

2. **Recreate Assembly Links**
   - Run the 'Relink CLR assembly' deployment step from the Deployement Manager.

3. **Attach Debugger**
   - Open Visual Studio's **Debug** menu.
   - Select **Attach to Process...**.
   - In **Connection Type**, choose `Local`.
   - Filter process names by typing `sql`.
   - Select the process ID matching the SQL Server service noted earlier.
   - Under **Code Type**, select `Managed (.NET Framework 4.x) code`.
   - Click **Attach** (you may see a warning modal).

4. **Set Breakpoints**
   - Place breakpoints in managed code where debugging is needed (e.g., `SqlClrFunctions` or `OllamaService`).
   - Trigger the breakpoints by making T-SQL function calls that invoke corresponding CLR code.

---

### Development Workflow

When making changes:

1. **Write new code**
   - Try to implement something from the [ROADMAP](./ROADMAP.md).
   - Or try to improve an existing feature.
   - Or try something new!
   - Write test code along with your new work.

2. **Build and Test**
   - Build the solution in `Debug` configuration and ensure all tests pass.

3. **Create a Release Build**
   - Build the solution in `Release` configuration to generate the CLR assembly.

4. **Redeploy the Assembly**
   - Relink the CLR function and assembly links, via the Deployment Manager.
   - Establish tables and data to test against your new work.

5. **Debug Remotely**
   - Attach the debugger to SQL Server as described above.
   - Use an API development tool like Postman to isolate API requests and responses.

---

By following these steps, you can effectively test, debug, and enhance `Ollama Completions for SQL Server`.
