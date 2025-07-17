"""
Configuration settings for the Unity MCP Server.
This file contains all configurable parameters for the server.
"""

import os
from dataclasses import dataclass, field

def get_unity_port():
    """
    Read Unity port from unity-port.txt file.
    Falls back to default port 6400 if file doesn't exist or can't be read.
    """
    try:
        # Look for unity-port.txt in the same directory as the server
        port_file = os.path.join(os.path.dirname(__file__), "..", "unity-port.txt")
        if os.path.exists(port_file):
            with open(port_file, 'r') as f:
                port_str = f.read().strip()
                if port_str.isdigit():
                    port = int(port_str)
                    print(f"[Config] Read Unity port {port} from unity-port.txt")
                    return port
    except Exception as e:
        print(f"[Config] Error reading unity-port.txt: {e}")
    
    # Fall back to default
    print("[Config] Using default Unity port 6400")
    return 6400

@dataclass
class ServerConfig:
    """Main configuration class for the MCP server."""
    
    # Network settings
    unity_host: str = "localhost"
    unity_port: int = field(default_factory=get_unity_port)
    mcp_port: int = 6500
    
    # Connection settings
    connection_timeout: float = 86400.0  # 24 hours timeout
    buffer_size: int = 16 * 1024 * 1024  # 16MB buffer
    
    # Logging settings
    log_level: str = "INFO"
    log_format: str = "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
    
    # Server settings
    max_retries: int = 3
    retry_delay: float = 1.0

# Create a global config instance
config = ServerConfig() 