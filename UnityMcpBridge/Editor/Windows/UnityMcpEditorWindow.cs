using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityMcpBridge.Editor.Configuration;
using UnityMcpBridge.Editor.Data;
using UnityMcpBridge.Editor.Helpers;
using UnityMcpBridge.Editor.Models;

namespace UnityMcpBridge.Editor.Windows
{
    public class UnityMcpEditorWindow : EditorWindow
    {
        private bool isUnityBridgeRunning = false;
        private Vector2 scrollPosition;
        private string pythonServerInstallationStatus = "Not Installed";
        private Color pythonServerInstallationStatusColor = Color.red;
        private readonly McpClients mcpClients = new();

        [MenuItem("Window/Unity MCP")]
        public static void ShowWindow()
        {
            GetWindow<UnityMcpEditorWindow>("MCP Editor");
        }

        private void OnEnable()
        {
            UpdatePythonServerInstallationStatus();

            isUnityBridgeRunning = UnityMcpBridge.IsRunning;
            foreach (McpClient mcpClient in mcpClients.clients)
            {
                CheckMcpConfiguration(mcpClient);
            }
        }

        private Color GetStatusColor(McpStatus status)
        {
            // Return appropriate color based on the status enum
            return status switch
            {
                McpStatus.Configured => Color.green,
                McpStatus.Running => Color.green,
                McpStatus.Connected => Color.green,
                McpStatus.IncorrectPath => Color.yellow,
                McpStatus.CommunicationError => Color.yellow,
                McpStatus.NoResponse => Color.yellow,
                _ => Color.red, // Default to red for error states or not configured
            };
        }

        private void UpdatePythonServerInstallationStatus()
        {
            string serverPath = ServerInstaller.GetServerPath();

            if (File.Exists(Path.Combine(serverPath, "server.py")))
            {
                string installedVersion = ServerInstaller.GetInstalledVersion();
                string latestVersion = ServerInstaller.GetLatestVersion();

                if (ServerInstaller.IsNewerVersion(latestVersion, installedVersion))
                {
                    pythonServerInstallationStatus = "Newer Version Available";
                    pythonServerInstallationStatusColor = Color.yellow;
                }
                else
                {
                    pythonServerInstallationStatus = "Up to Date";
                    pythonServerInstallationStatusColor = Color.green;
                }
            }
            else
            {
                pythonServerInstallationStatus = "Not Installed";
                pythonServerInstallationStatusColor = Color.red;
            }
        }
        
        private void ReinstallPythonServer()
        {
            try
            {
                pythonServerInstallationStatus = "Reinstalling...";
                pythonServerInstallationStatusColor = Color.yellow;
                Repaint();
                
                // Get the server directory path
                string serverPath = ServerInstaller.GetServerPath();
                string serverParentPath = Path.GetDirectoryName(serverPath);
                string serverFolder = Path.GetDirectoryName(serverParentPath);

                // Delete existing server folder if it exists
                if (Directory.Exists(serverFolder))
                {
                    try
                    {
                        Directory.Delete(serverFolder, true);
                        Debug.Log("[Unity MCP] Deleted existing Python server.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("[Unity MCP] Exception while deleting existing server. But trying to reinstall anyway.");
                    }
                }

                // Reinstall the server
                ServerInstaller.EnsureServerInstalled();
                
                // Update status
                UpdatePythonServerInstallationStatus();
                Debug.Log("[Unity MCP] Python server reinstalled successfully.");
                
                // Show success dialog
                EditorUtility.DisplayDialog(
                    "Reinstall Complete", 
                    "Python server has been reinstalled successfully.", 
                    "OK");
            }
            catch (Exception e)
            {
                pythonServerInstallationStatus = "Reinstall Failed";
                pythonServerInstallationStatusColor = Color.red;
                Debug.LogError($"[Unity MCP] Failed to reinstall Python server: {e.Message}");
                
                // Show error dialog
                EditorUtility.DisplayDialog(
                    "Reinstall Failed", 
                    $"Failed to reinstall Python server:\n{e.Message}", 
                    "OK");
            }
        }

        private void ConfigurationSection(McpClient mcpClient)
        {
            // Calculate if we should use half-width layout
            // Minimum width for half-width layout is 400 pixels
            bool useHalfWidth = position.width >= 800;
            float sectionWidth = useHalfWidth ? (position.width / 2) - 15 : position.width - 20;

            // Begin horizontal layout if using half-width
            if (useHalfWidth && mcpClients.clients.IndexOf(mcpClient) % 2 == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            // Begin section with fixed width
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(sectionWidth));

            // Header with improved styling
            EditorGUILayout.Space(5);
            Rect headerRect = EditorGUILayout.GetControlRect(false, 24);
            GUI.Label(
                new Rect(
                    headerRect.x + 8,
                    headerRect.y + 4,
                    headerRect.width - 16,
                    headerRect.height
                ),
                mcpClient.name + " Configuration",
                EditorStyles.boldLabel
            );
            EditorGUILayout.Space(5);

            // Status indicator with colored dot
            Rect statusRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            Color statusColor = GetStatusColor(mcpClient.status);

            // Draw status dot
            DrawStatusDot(statusRect, statusColor);

            // Status text with some padding
            EditorGUILayout.LabelField(
                new GUIContent("      " + mcpClient.configStatus),
                GUILayout.Height(20),
                GUILayout.MinWidth(100)
            );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            // Configure button with improved styling
            GUIStyle buttonStyle = new(GUI.skin.button)
            {
                padding = new RectOffset(15, 15, 5, 5),
                margin = new RectOffset(10, 10, 5, 5),
            };

            // Create muted button style for Manual Setup
            GUIStyle mutedButtonStyle = new(buttonStyle);

            if (mcpClient.mcpType == McpTypes.VSCode)
            {
                // Special handling for VSCode GitHub Copilot
                if (
                    GUILayout.Button(
                        "Auto Configure VSCode with GitHub Copilot",
                        buttonStyle,
                        GUILayout.Height(28)
                    )
                )
                {
                    ConfigureMcpClient(mcpClient);
                }

                if (GUILayout.Button("Manual Setup", mutedButtonStyle, GUILayout.Height(28)))
                {
                    // Show VSCode specific manual setup window
                    string configPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? mcpClient.windowsConfigPath
                        : mcpClient.linuxConfigPath;
                        
                    // Get the Python directory path
                    string pythonDir = FindPackagePythonDirectory();
                    
                    // Create VSCode-specific configuration
                    var vscodeConfig = new
                    {
                        mcp = new
                        {
                            servers = new
                            {
                                unityMCP = new
                                {
                                    command = "uv",
                                    args = new[] { "--directory", pythonDir, "run", "server.py" }
                                }
                            }
                        }
                    };
                    
                    JsonSerializerSettings jsonSettings = new() { Formatting = Formatting.Indented };
                    string manualConfigJson = JsonConvert.SerializeObject(vscodeConfig, jsonSettings);
                    
                    // Use the VSCodeManualSetupWindow directly since we're in the same namespace
                    VSCodeManualSetupWindow.ShowWindow(configPath, manualConfigJson);
                }
            }
            else
            {
                // Standard client buttons
                if (
                    GUILayout.Button(
                        $"Auto Configure {mcpClient.name}",
                        buttonStyle,
                        GUILayout.Height(28)
                    )
                )
                {
                    ConfigureMcpClient(mcpClient);
                }

                if (GUILayout.Button("Manual Setup", mutedButtonStyle, GUILayout.Height(28)))
                {
                    // Get the appropriate config path based on OS
                    string configPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? mcpClient.windowsConfigPath
                        : mcpClient.linuxConfigPath;
                    ShowManualInstructionsWindow(configPath, mcpClient);
                }
            }
            EditorGUILayout.Space(5);

            EditorGUILayout.EndVertical();

            // End horizontal layout if using half-width and at the end of a row
            if (useHalfWidth && mcpClients.clients.IndexOf(mcpClient) % 2 == 1)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            // Add space and end the horizontal layout if last item is odd
            else if (
                useHalfWidth
                && mcpClients.clients.IndexOf(mcpClient) == mcpClients.clients.Count - 1
            )
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
        }

        private void DrawStatusDot(Rect statusRect, Color statusColor)
        {
            Rect dotRect = new(statusRect.x + 6, statusRect.y + 4, 12, 12);
            Vector3 center = new(
                dotRect.x + (dotRect.width / 2),
                dotRect.y + (dotRect.height / 2),
                0
            );
            float radius = dotRect.width / 2;

            // Draw the main dot
            Handles.color = statusColor;
            Handles.DrawSolidDisc(center, Vector3.forward, radius);

            // Draw the border
            Color borderColor = new(
                statusColor.r * 0.7f,
                statusColor.g * 0.7f,
                statusColor.b * 0.7f
            );
            Handles.color = borderColor;
            Handles.DrawWireDisc(center, Vector3.forward, radius);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            // Title with improved styling
            Rect titleRect = EditorGUILayout.GetControlRect(false, 30);
            EditorGUI.DrawRect(
                new Rect(titleRect.x, titleRect.y, titleRect.width, titleRect.height),
                new Color(0.2f, 0.2f, 0.2f, 0.1f)
            );
            GUI.Label(
                new Rect(titleRect.x + 10, titleRect.y + 6, titleRect.width - 20, titleRect.height),
                "MCP Editor",
                EditorStyles.boldLabel
            );
            EditorGUILayout.Space(10);

            // Python Server Installation Status Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Python Server Status", EditorStyles.boldLabel);

            // Status indicator with colored dot
            Rect installStatusRect = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            DrawStatusDot(installStatusRect, pythonServerInstallationStatusColor);
            EditorGUILayout.LabelField("      " + pythonServerInstallationStatus);
            EditorGUILayout.EndHorizontal();

            int displayPort = UnityMcpBridge.IsRunning ? UnityMcpBridge.ActualPort : UnityMcpConfig.UnityPort;
            EditorGUILayout.LabelField($"Unity Port: {displayPort}");
            EditorGUILayout.LabelField($"MCP Port: {UnityMcpConfig.McpPort}");
            
            if (GUILayout.Button("Reinstall Python Server"))
            {
                if (EditorUtility.DisplayDialog(
                    "Reinstall Python Server", 
                    "This will delete the existing Python server and download a fresh copy. Continue?", 
                    "Yes", 
                    "Cancel"))
                {
                    ReinstallPythonServer();
                }
            }
            
            EditorGUILayout.HelpBox(
                "Your MCP client (e.g. Cursor or Claude Desktop) will start the server automatically when you start it.",
                MessageType.Info
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Unity Bridge Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Unity MCP Bridge", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Status: {(isUnityBridgeRunning ? "Running" : "Stopped")}");
            int bridgePort = UnityMcpBridge.IsRunning ? UnityMcpBridge.ActualPort : UnityMcpConfig.UnityPort;
            EditorGUILayout.LabelField($"Port: {bridgePort}");

            if (GUILayout.Button(isUnityBridgeRunning ? "Stop Bridge" : "Start Bridge"))
            {
                ToggleUnityBridge();
            }
            EditorGUILayout.EndVertical();

            foreach (McpClient mcpClient in mcpClients.clients)
            {
                EditorGUILayout.Space(10);
                ConfigurationSection(mcpClient);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ToggleUnityBridge()
        {
            if (isUnityBridgeRunning)
            {
                UnityMcpBridge.Stop();
            }
            else
            {
                UnityMcpBridge.Start();
            }

            isUnityBridgeRunning = !isUnityBridgeRunning;
        }

        private string WriteToConfig(string pythonDir, string configPath, McpClient mcpClient = null)
        {
            // Create configuration object for unityMCP
            McpConfigServer unityMCPConfig = new()
            {
                command = "uv",
                args = new[] { "--directory", pythonDir, "run", "server.py" },
            };

            JsonSerializerSettings jsonSettings = new() { Formatting = Formatting.Indented };

            // Read existing config if it exists
            string existingJson = "{}";
            if (File.Exists(configPath))
            {
                try
                {
                    existingJson = File.ReadAllText(configPath);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error reading existing config: {e.Message}.");
                }
            }

            // Parse the existing JSON while preserving all properties
            dynamic existingConfig = JsonConvert.DeserializeObject(existingJson);
            existingConfig ??= new Newtonsoft.Json.Linq.JObject();

            // Handle different client types with a switch statement
            switch (mcpClient?.mcpType)
            {
                case McpTypes.VSCode:
                    // VSCode specific configuration
                    // Ensure mcp object exists
                    if (existingConfig.mcp == null)
                    {
                        existingConfig.mcp = new Newtonsoft.Json.Linq.JObject();
                    }

                    // Ensure mcp.servers object exists
                    if (existingConfig.mcp.servers == null)
                    {
                        existingConfig.mcp.servers = new Newtonsoft.Json.Linq.JObject();
                    }
                    break;

                default:
                    // Standard MCP configuration (Claude Desktop, Cursor, etc.)
                    // Ensure mcpServers object exists
                    if (existingConfig.mcpServers == null)
                    {
                        existingConfig.mcpServers = new Newtonsoft.Json.Linq.JObject();
                    }
                    break;
            }

            // Add/update UnityMCP server in VSCode settings
            existingConfig.mcp.servers.unityMCP =
                JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JToken>(
                    JsonConvert.SerializeObject(unityMCPConfig)
                );

            // Write the merged configuration back to file
            string mergedJson = JsonConvert.SerializeObject(existingConfig, jsonSettings);
            File.WriteAllText(configPath, mergedJson);

            return "Configured successfully";
        }

        private void ShowManualConfigurationInstructions(string configPath, McpClient mcpClient)
        {
            mcpClient.SetStatus(McpStatus.Error, "Manual configuration required");

            ShowManualInstructionsWindow(configPath, mcpClient);
        }

        // New method to show manual instructions without changing status
        private void ShowManualInstructionsWindow(string configPath, McpClient mcpClient)
        {
            // Get the Python directory path using Package Manager API
            string pythonDir = FindPackagePythonDirectory();
            string manualConfigJson;
            
            // Create common JsonSerializerSettings
            JsonSerializerSettings jsonSettings = new() { Formatting = Formatting.Indented };
            
            // Use switch statement to handle different client types
            switch (mcpClient.mcpType)
            {
                case McpTypes.VSCode:
                    // Create VSCode-specific configuration with proper format
                    var vscodeConfig = new
                    {
                        mcp = new
                        {
                            servers = new
                            {
                                unityMCP = new
                                {
                                    command = "uv",
                                    args = new[] { "--directory", pythonDir, "run", "server.py" }
                                }
                            }
                        }
                    };
                    manualConfigJson = JsonConvert.SerializeObject(vscodeConfig, jsonSettings);
                    break;
                    
                default:
                    // Create standard MCP configuration for other clients
                    McpConfig jsonConfig = new()
                    {
                        mcpServers = new McpConfigServers
                        {
                            unityMCP = new McpConfigServer
                            {
                                command = "uv",
                                args = new[] { "--directory", pythonDir, "run", "server.py" },
                            },
                        },
                    };
                    manualConfigJson = JsonConvert.SerializeObject(jsonConfig, jsonSettings);
                    break;
            }

            ManualConfigEditorWindow.ShowWindow(configPath, manualConfigJson, mcpClient);
        }

        private string FindPackagePythonDirectory()
        {
            // First check project-specific installation
            string projectServerPath = ServerInstaller.GetServerPath();
            if (Directory.Exists(projectServerPath) && File.Exists(Path.Combine(projectServerPath, "server.py")))
            {
                return projectServerPath;
            }

            // If not found in project, check package installations
            try
            {
                // Try to find the package using Package Manager API
                UnityEditor.PackageManager.Requests.ListRequest request =
                    UnityEditor.PackageManager.Client.List();
                while (!request.IsCompleted) { } // Wait for the request to complete

                if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    foreach (UnityEditor.PackageManager.PackageInfo package in request.Result)
                    {
                        if (package.name == "com.justinpbarnett.unity-mcp")
                        {
                            string packagePath = package.resolvedPath;
                            string potentialPythonDir = Path.Combine(packagePath, "Python");

                            if (
                                Directory.Exists(potentialPythonDir)
                                && File.Exists(Path.Combine(potentialPythonDir, "server.py"))
                            )
                            {
                                return potentialPythonDir;
                            }
                        }
                    }
                }
                else if (request.Error != null)
                {
                    Debug.LogError("Failed to list packages: " + request.Error.message);
                }

                // If not found via Package Manager, try manual approaches
                // First check for local installation
                string[] possibleDirs =
                {
                    Path.GetFullPath(Path.Combine(Application.dataPath, "unity-mcp", "Python")),
                };

                foreach (string dir in possibleDirs)
                {
                    if (Directory.Exists(dir) && File.Exists(Path.Combine(dir, "server.py")))
                    {
                        return dir;
                    }
                }

                // If still not found, return the project path anyway
                Debug.LogWarning("Could not find Python directory, using project server path");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error finding package path: {e.Message}");
            }

            return projectServerPath;
        }

        private string ConfigureMcpClient(McpClient mcpClient)
        {
            try
            {
                // Determine the config file path based on OS
                string configPath;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    configPath = mcpClient.windowsConfigPath;
                }
                else if (
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                )
                {
                    configPath = mcpClient.linuxConfigPath;
                }
                else
                {
                    return "Unsupported OS";
                }

                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(configPath));

                // Find the server.py file location
                string pythonDir = ServerInstaller.GetServerPath();

                if (pythonDir == null || !File.Exists(Path.Combine(pythonDir, "server.py")))
                {
                    ShowManualInstructionsWindow(configPath, mcpClient);
                    return "Manual Configuration Required";
                }

                string result = WriteToConfig(pythonDir, configPath, mcpClient);

                // Update the client status after successful configuration
                if (result == "Configured successfully")
                {
                    mcpClient.SetStatus(McpStatus.Configured);
                }

                return result;
            }
            catch (Exception e)
            {
                // Determine the config file path based on OS for error message
                string configPath = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    configPath = mcpClient.windowsConfigPath;
                }
                else if (
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                )
                {
                    configPath = mcpClient.linuxConfigPath;
                }

                ShowManualInstructionsWindow(configPath, mcpClient);
                Debug.LogError(
                    $"Failed to configure {mcpClient.name}: {e.Message}\n{e.StackTrace}"
                );
                return $"Failed to configure {mcpClient.name}";
            }
        }

        private void ShowCursorManualConfigurationInstructions(
            string configPath,
            McpClient mcpClient
        )
        {
            mcpClient.SetStatus(McpStatus.Error, "Manual configuration required");

            // Get the Python directory path using Package Manager API
            string pythonDir = FindPackagePythonDirectory();

            // Create the manual configuration message
            McpConfig jsonConfig = new()
            {
                mcpServers = new McpConfigServers
                {
                    unityMCP = new McpConfigServer
                    {
                        command = "uv",
                        args = new[] { "--directory", pythonDir, "run", "server.py" },
                    },
                },
            };

            JsonSerializerSettings jsonSettings = new() { Formatting = Formatting.Indented };
            string manualConfigJson = JsonConvert.SerializeObject(jsonConfig, jsonSettings);

            ManualConfigEditorWindow.ShowWindow(configPath, manualConfigJson, mcpClient);
        }

        private void CheckMcpConfiguration(McpClient mcpClient)
        {
            try
            {
                string configPath;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    configPath = mcpClient.windowsConfigPath;
                }
                else if (
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    || RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                )
                {
                    configPath = mcpClient.linuxConfigPath;
                }
                else
                {
                    mcpClient.SetStatus(McpStatus.UnsupportedOS);
                    return;
                }

                if (!File.Exists(configPath))
                {
                    mcpClient.SetStatus(McpStatus.NotConfigured);
                    return;
                }

                string configJson = File.ReadAllText(configPath);
                string pythonDir = ServerInstaller.GetServerPath();
                
                // Use switch statement to handle different client types, extracting common logic
                string[] args = null;
                bool configExists = false;
                
                switch (mcpClient.mcpType)
                {
                    case McpTypes.VSCode:
                        dynamic config = JsonConvert.DeserializeObject(configJson);
                        
                        if (config?.mcp?.servers?.unityMCP != null)
                        {
                            // Extract args from VSCode config format
                            args = config.mcp.servers.unityMCP.args.ToObject<string[]>();
                            configExists = true;
                        }
                        break;
                        
                    default:
                        // Standard MCP configuration check for Claude Desktop, Cursor, etc.
                        McpConfig standardConfig = JsonConvert.DeserializeObject<McpConfig>(configJson);
                        
                        if (standardConfig?.mcpServers?.unityMCP != null)
                        {
                            args = standardConfig.mcpServers.unityMCP.args;
                            configExists = true;
                        }
                        break;
                }
                
                // Common logic for checking configuration status
                if (configExists)
                {
                    if (pythonDir != null && 
                        Array.Exists(args, arg => arg.Contains(pythonDir, StringComparison.Ordinal)))
                    {
                        mcpClient.SetStatus(McpStatus.Configured);
                    }
                    else
                    {
                        mcpClient.SetStatus(McpStatus.IncorrectPath);
                    }
                }
                else
                {
                    mcpClient.SetStatus(McpStatus.MissingConfig);
                }
            }
            catch (Exception e)
            {
                mcpClient.SetStatus(McpStatus.Error, e.Message);
            }
        }
    }
}
