# MCP Server Setup for Daggerheart-Helper

This document explains how to configure MCP (Model Context Protocol) servers for enhanced AI assistant capabilities in this project.

## Recommended MCP Servers

### SQLite (Database Schema Inspection)

**Why useful:** The Daggerheart-Helper uses SQLite for persistence. An MCP server can help inspect database schema, run queries, and understand data relationships.

**Setup for VS Code with Cursor or Claude extension:**

1. Install the MCP SQLite server:
```bash
npm install -g @modelcontextprotocol/server-sqlite
```

2. Add to your editor's MCP configuration (varies by IDE):
   - **Cursor:** Add to `.cursor/mcp.json` or settings
   - **VS Code Claude extension:** Add to extension settings
   - **Claude desktop app:** Add to `~/Library/Application\ Support/Claude/mcp.json` (macOS) or equivalent

3. Configuration example:
```json
{
  "mcpServers": {
    "sqlite": {
      "command": "mcp-server-sqlite",
      "args": ["--db-path", "Daggerheart-Helper.Web/bin/Debug/net9.0/daggerheart.db"]
    }
  }
}
```

After the app runs, this server enables queries like:
- View all tables and schema
- Query Characters, Abilities, Features, etc.
- Understand relationships between game entities

## IDEs & Setup Instructions

- **Rider (JetBrains):** Use the Copilot plugin with built-in MCP support if available
- **VS Code:** Install Copilot extension, configure MCP servers in settings
- **Cursor:** Built-in MCP support; configure in `.cursor/mcp.json` or command palette settings

## Database Location

The SQLite database is created at:
```
Daggerheart-Helper.Web/bin/Debug/net9.0/daggerheart.db
```

(after running the web application once)

The connection string can be modified in `Daggerheart-Helper.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=daggerheart.db"
  }
}
```
