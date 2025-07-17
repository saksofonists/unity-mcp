# Unity MCP ✨


[![](https://img.shields.io/badge/Unity-000000?style=flat&logo=unity&logoColor=blue 'Unity')](https://unity.com/releases/editor/archive)
[![python](https://img.shields.io/badge/Python-3.12-3776AB.svg?style=flat&logo=python&logoColor=white)](https://www.python.org)
[![](https://badge.mcpx.dev?status=on 'MCP Enabled')](https://modelcontextprotocol.io/introduction)
![GitHub commit activity](https://img.shields.io/github/commit-activity/w/justinpbarnett/unity-mcp)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/justinpbarnett/unity-mcp)
[![](https://img.shields.io/badge/License-MIT-red.svg 'MIT License')](https://opensource.org/licenses/MIT)




**Create your Unity apps with LLMs!**

Unity MCP acts as a bridge, allowing AI assistants (like Claude, Cursor) to interact directly with your Unity Editor via a local **MCP (Model Context Protocol) Client**. Give your LLM tools to manage assets, control scenes, edit scripts, and automate tasks within Unity.

---


## Key Features 🚀

*   **🗣️ Natural Language Control:** Instruct your LLM to perform Unity tasks.
*   **🛠️ Powerful Tools:** Manage assets, scenes, materials, scripts, and editor functions.
*   **🤖 Automation:** Automate repetitive Unity workflows.
*   **🧩 Extensible:** Designed to work with various MCP Clients.

<details open>
  <summary><strong> Available Tools </strong></summary>

  Your LLM can use functions like:

  *   `read_console`: Gets messages from or clears the console.
  *   `manage_script`: Manages C# scripts (create, read, update, delete).
  *   `manage_editor`: Controls and queries the editor's state and settings.
  *   `manage_scene`: Manages scenes (load, save, create, get hierarchy, etc.).
  *   `manage_asset`: Performs asset operations (import, create, modify, delete, etc.).
  *   `manage_shader`: Performs shader CRUD operations (create, read, modify, delete).
  *   `manage_gameobject`: Manages GameObjects: create, modify, delete, find, and component operations.
  *   `execute_menu_item`: Executes a menu item via its path (e.g., "File/Save Project").
</details>

---

## How It Works 🤔

Unity MCP connects your tools using two components:

1.  **Unity MCP Bridge:** A Unity package running inside the Editor. (Installed via Package Manager).
2.  **Unity MCP Server:** A Python server that runs locally, communicating between the Unity Bridge and your MCP Client. (Installed manually).

**Flow:** `[Your LLM via MCP Client] <-> [Unity MCP Server (Python)] <-> [Unity MCP Bridge (Unity Editor)]`

---

## Installation ⚙️

> **Note:** The setup is constantly improving as we update the package. Check back if you randomly start to run into issues.

### Prerequisites


  *   **Git CLI:** For cloning the server code. [Download Git](https://git-scm.com/downloads)
  *   **Python:** Version 3.12 or newer. [Download Python](https://www.python.org/downloads/)
  *   **Unity Hub & Editor:** Version 2020.3 LTS or newer. [Download Unity](https://unity.com/download)
  *   **uv (Python package manager):**
      ```bash
      pip install uv
      # Or see: https://docs.astral.sh/uv/getting-started/installation/
      ```
  *   **An MCP Client:**
      *   [Claude Desktop](https://claude.ai/download)
      *   [Cursor](https://www.cursor.com/en/downloads)
      *   [Visual Studio Code Copilot](https://code.visualstudio.com/docs/copilot/overview)
      *   *(Others may work with manual config)*

### Step 1: Install the Unity Package (Bridge)

1.  Open your Unity project.
2.  Go to `Window > Package Manager`.
3.  Click `+` -> `Add package from git URL...`.
4.  Enter:
    ```
    https://github.com/justinpbarnett/unity-mcp.git?path=/UnityMcpBridge
    ```
5.  Click `Add`.
6. The MCP Server should automatically be installed onto your machine as a result of this process.

### Step 2: Configure Your MCP Client

Connect your MCP Client (Claude, Cursor, etc.) to the Python server you installed in Step 1.

<img width="609" alt="image" src="https://github.com/user-attachments/assets/cef3a639-4677-4fd8-84e7-2d82a04d55bb" />

**Option A: Auto-Configure (Recommended for Claude/Cursor/VSC Copilot)**

1.  In Unity, go to `Window > Unity MCP`.
2.  Click `Auto Configure` on the IDE you uses.
3.  Look for a green status indicator 🟢 and "Connected". *(This attempts to modify the MCP Client's config file automatically)*.

**Option B: Manual Configuration**

If Auto-Configure fails or you use a different client:

1.  **Find your MCP Client's configuration file.** (Check client documentation).
    *   *Claude Example (macOS):* `~/Library/Application Support/Claude/claude_desktop_config.json`
    *   *Claude Example (Windows):* `%APPDATA%\Claude\claude_desktop_config.json`
2.  **Edit the file** to add/update the `mcpServers` section, using the *exact* paths from Step 1.

<details>
<summary><strong>Click for OS-Specific JSON Configuration Snippets...</strong></summary>

**Note:** The Python server is now installed per-project in your Unity project's Library directory. Replace `YOUR_PROJECT_PATH` with your actual Unity project path.

**Windows:**

  ```json
  {
    "mcpServers": {
      "UnityMCP": {
        "command": "uv",
        "args": [
          "run",
          "--directory",
          "C:\\path\\to\\YOUR_PROJECT_PATH\\Library\\UnityMcpServer\\UnityMcpServer\\src",
          "server.py"
        ]
      }
      // ... other servers might be here ...
    }
  }
``` 

(Remember to replace YOUR_PROJECT_PATH and use double backslashes \\)

**macOS:**

```json
{
  "mcpServers": {
    "UnityMCP": {
      "command": "uv",
      "args": [
        "run",
        "--directory",
        "/path/to/YOUR_PROJECT_PATH/Library/UnityMcpServer/UnityMcpServer/src",
        "server.py"
      ]
    }
    // ... other servers might be here ...
  }
}
```

**Linux:**

```json
{
  "mcpServers": {
    "UnityMCP": {
      "command": "uv",
      "args": [
        "run",
        "--directory",
        "/path/to/YOUR_PROJECT_PATH/Library/UnityMcpServer/UnityMcpServer/src",
        "server.py"
      ]
    }
    // ... other servers might be here ...
  }
}
```

(Replace YOUR_PROJECT_PATH with your Unity project path)

</details>

---

## Usage ▶️

1. **Open your Unity Project.** The Unity MCP Bridge (package) should connect automatically. Check status via Window > Unity MCP.
    
2. **Start your MCP Client** (Claude, Cursor, etc.). It should automatically launch the Unity MCP Server (Python) using the configuration from Installation Step 3.
    
3. **Interact!** Unity tools should now be available in your MCP Client.
    
    Example Prompt: `Create a 3D player controller`, `Create a yellow and bridge sun`, `Create a cool shader and apply it on a cube`.

---

## Multiple Unity Instances Support 🎯

Unity MCP now supports running multiple Unity Editor instances simultaneously! Each Unity project gets its own isolated server installation and dynamic port assignment.

### How It Works:
- **Per-Project Installation**: The Python server is installed in each Unity project's `Library/UnityMcpServer` directory
- **Dynamic Port Selection**: Unity automatically finds an available port (starting at 6400)
- **Automatic Configuration**: The selected port is communicated to the Python server via a file

### Benefits:
- ✅ Run multiple Unity projects simultaneously
- ✅ Each project has its own isolated server instance
- ✅ No port conflicts between projects
- ✅ Zero manual configuration required

### Technical Details:
- Unity Bridge tries port 6400 first, then increments until it finds an available port
- The selected port is written to `Library/UnityMcpServer/unity-port.txt`
- Python server reads this file on startup to connect to the correct Unity instance
- Each Unity-Python pair maintains a 1:1 relationship

---

## Future Dev Plans (Besides PR) 📝

### 🔴 High Priority
- [ ] **Asset Generation Improvements** - Enhanced server request handling and asset pipeline optimization
- [ ] **Code Generation Enhancements** - Improved generated code quality, validation, and error handling
- [ ] **Robust Error Handling** - Comprehensive error messages, recovery mechanisms, and graceful degradation
- [ ] **Remote Connection Support** - Enable seamless remote connection between Unity host and MCP server
- [ ] **Documentation Expansion** - Complete tutorials for custom tool creation and API reference

### 🟡 Medium Priority
- [ ] **Custom Tool Creation GUI** - Visual interface for users to create and configure their own MCP tools
- [ ] **Advanced Logging System** - Logging with filtering, export, and debugging capabilities

### 🟢 Low Priority
- [ ] **Mobile Platform Support** - Extended toolset for mobile development workflows and platform-specific features
- [ ] **Easier Tool Setup**
- [ ] **Plugin Marketplace** - Community-driven tool sharing and distribution platform

<details open>
  <summary><strong>✅ Completed Features<strong></summary>
  
  - [x] **Shader Generation** - Generate shaders using CGProgram template
</details>

### 🔬 Research & Exploration
- [ ] **AI-Powered Asset Generation** - Integration with AI tools for automatic 3D models, textures, and animations
- [ ] **Real-time Collaboration** - Live editing sessions between multiple developers *(Currently in progress)*
- [ ] **Analytics Dashboard** - Usage analytics, project insights, and performance metrics
- [ ] **Voice Commands** - Voice-controlled Unity operations for accessibility
- [ ] **AR/VR Tool Integration** - Extended support for immersive development workflows

---

## Contributing 🤝

Help make Unity MCP better!

1. **Fork** the main repository.
    
2. **Create a branch** (`feature/your-idea` or `bugfix/your-fix`).
    
3. **Make changes.**
    
4. **Commit** (feat: Add cool new feature).
    
5. **Push** your branch.
    
6. **Open a Pull Request** against the master branch.
    

---

## Troubleshooting ❓

<details>  
<summary><strong>Click to view common issues and fixes...</strong></summary>  

- **Unity Bridge Not Running/Connecting:**
    
    - Ensure Unity Editor is open.
        
    - Check the status window: Window > Unity MCP.
        
    - Restart Unity.
        
- **MCP Client Not Connecting / Server Not Starting:**
    
    - **Verify Server Path:** Double-check the --directory path in your MCP Client's JSON config. It must exactly match the location where you cloned the UnityMCP repository in Installation Step 1 (e.g., .../Programs/UnityMCP/UnityMcpServer/src).
        
    - **Verify uv:** Make sure uv is installed and working (pip show uv).
        
    - **Run Manually:** Try running the server directly from the terminal to see errors: `# Navigate to the src directory first! cd /path/to/your/UnityMCP/UnityMcpServer/src uv run server.py`
        
    - **Permissions (macOS/Linux):** If you installed the server in a system location like /usr/local/bin, ensure the user running the MCP client has permission to execute uv and access files there. Installing in ~/bin might be easier.
        
- **Auto-Configure Failed:**
    
    - Use the Manual Configuration steps. Auto-configure might lack permissions to write to the MCP client's config file.
        

</details>  

Still stuck? [Open an Issue](https://www.google.com/url?sa=E&q=https%3A%2F%2Fgithub.com%2Fjustinpbarnett%2Funity-mcp%2Fissues) or [Join the Discord](https://discord.gg/vhTUxXaqYr)!

---

## Contact 👋

- **justinpbarnett:** [X/Twitter](https://www.google.com/url?sa=E&q=https%3A%2F%2Fx.com%2Fjustinpbarnett)
- **scriptwonder**: [Email](mailto:swu85@ur.rochester.edu), [LinkedIn](https://www.linkedin.com/in/shutong-wu-214043172/)
    

---

## License 📜

MIT License. See [LICENSE](https://www.google.com/url?sa=E&q=https%3A%2F%2Fgithub.com%2Fjustinpbarnett%2Funity-mcp%2Fblob%2Fmaster%2FLICENSE) file.

---

## Acknowledgments 🙏

Thanks to the contributors and the Unity team.


## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=unity-mcp/unity-mcp,justinpbarnett/unity-mcp&type=Date)](https://www.star-history.com/#unity-mcp/unity-mcp&justinpbarnett/unity-mcp&Date)
