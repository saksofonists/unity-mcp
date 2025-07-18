using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityMcpBridge.Editor.Configuration
{
    /// <summary>
    /// Centralized configuration management for Unity MCP Bridge
    /// </summary>
    public static class UnityMcpConfig
    {
        private static int? _cachedPort;
        private static string _cachedProjectPath;
        
        /// <summary>
        /// Default starting port for Unity MCP Bridge
        /// </summary>
        public const int DEFAULT_UNITY_PORT = 6400;
        
        /// <summary>
        /// Maximum port number to try before giving up
        /// </summary>
        public const int MAX_PORT = 6500;
        
        /// <summary>
        /// Default MCP server port
        /// </summary>
        public const int DEFAULT_MCP_PORT = 6500;
        
        /// <summary>
        /// Gets the current Unity port being used by this instance
        /// </summary>
        public static int UnityPort
        {
            get
            {
                // Check if we need to refresh the cached port (project changed)
                string currentProjectPath = GetProjectPath();
                if (_cachedPort == null || _cachedProjectPath != currentProjectPath)
                {
                    _cachedProjectPath = currentProjectPath;
                    _cachedPort = null;
                }
                
                return _cachedPort ?? DEFAULT_UNITY_PORT;
            }
            internal set
            {
                _cachedPort = value;
                _cachedProjectPath = GetProjectPath();
            }
        }
        
        /// <summary>
        /// Gets the MCP server port
        /// </summary>
        public static int McpPort => DEFAULT_MCP_PORT;
        
        /// <summary>
        /// Gets the current Unity project path
        /// </summary>
        public static string GetProjectPath()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }
        
        /// <summary>
        /// Gets the Library directory path for the current project
        /// </summary>
        public static string GetProjectTempPath()
        {
            return Path.Combine(GetProjectPath(), "Temp");
        }
        
        /// <summary>
        /// Gets the server installation path for the current project
        /// </summary>
        public static string GetProjectServerPath()
        {
            return Path.Combine(GetProjectTempPath(), "UnityMcpServer");
        }
        
        /// <summary>
        /// Gets the port file path for inter-process communication
        /// </summary>
        public static string GetPortFilePath()
        {
            return Path.Combine(GetProjectServerPath(), "unity-port.txt");
        }
        
        /// <summary>
        /// Writes the current port to the port file for Python server to read
        /// </summary>
        public static bool WritePortFile(int port)
        {
            try
            {
                string portFilePath = GetPortFilePath();
                string directory = Path.GetDirectoryName(portFilePath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(portFilePath, port.ToString());
                Debug.Log($"[UnityMcpConfig] Wrote port {port} to {portFilePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityMcpConfig] Failed to write port file: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Reads the port from the port file
        /// </summary>
        public static int? ReadPortFile()
        {
            try
            {
                string portFilePath = GetPortFilePath();
                if (File.Exists(portFilePath))
                {
                    string portStr = File.ReadAllText(portFilePath).Trim();
                    if (int.TryParse(portStr, out int port))
                    {
                        return port;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityMcpConfig] Failed to read port file: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Clears the cached port, forcing it to be re-evaluated
        /// </summary>
        public static void ClearCachedPort()
        {
            _cachedPort = null;
            _cachedProjectPath = null;
        }
    }
}
