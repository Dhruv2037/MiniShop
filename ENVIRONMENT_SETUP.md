# Environment Setup Guide

## Prerequisites

### Windows

#### 1. .NET 9.0 SDK
- Download: https://dotnet.microsoft.com/download/dotnet/9.0
- Install the **SDK** (not just runtime)
- Verify: `dotnet --version` (should show 9.0.x)

#### 2. Node.js & npm
- Download: https://nodejs.org/ (LTS version 18+)
- Install with default settings
- Verify:
  ```bash
  node --version  # v18.x.x or higher
  npm --version   # 9.x.x or higher
  ```

#### 3. SQL Server
- Download: https://www.microsoft.com/sql-server/sql-server-downloads
- Install SQL Server Express (free, suitable for development)
- Ensure Windows Authentication is enabled
- Connection string format:
  ```
  Server=localhost;Database=MiniShop_Catalog;Trusted_Connection=True;TrustServerCertificate=True
  ```

#### 4. SQL Server Management Studio (SSMS) - Optional
- Download: https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
- Useful for viewing/managing databases
- Or use Azure Data Studio (cross-platform)

#### 5. Visual Studio Code or Visual Studio
- VS Code: https://code.visualstudio.com/ (recommended for lightweight setup)
  - Extensions: C# Dev Kit, SQL Tools
- Visual Studio: https://visualstudio.microsoft.com/ (full IDE, heavier)

### macOS

#### 1. .NET 9.0 SDK
```bash
# Using Homebrew
brew install dotnet

# Or download from https://dotnet.microsoft.com/download/dotnet/9.0
```

#### 2. Node.js & npm
```bash
# Using Homebrew
brew install node

# Or download from https://nodejs.org/
```

#### 3. SQL Server
- SQL Server for Mac: Not available directly
- Options:
  - **Docker**: `docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" mcr.microsoft.com/mssql/server`
  - **Azure SQL Database**: Cloud-based (easier for dev)
  - **Parallels/VMware**: Windows VM with SQL Server

#### 4. VS Code
```bash
brew install --cask visual-studio-code
```

### Linux

#### 1. .NET 9.0 SDK
```bash
# Ubuntu/Debian
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --version 9.0

# CentOS/RHEL
sudo dnf install dotnet-sdk-9.0
```

#### 2. Node.js & npm
```bash
# Ubuntu/Debian
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs
```

#### 3. SQL Server
- **Docker** (recommended):
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass@123" \
    -p 1433:1433 \
    mcr.microsoft.com/mssql/server:latest
  ```
  Then update connection string:
  ```
  Server=localhost,1433;User Id=sa;Password=Pass@123;TrustServerCertificate=True
  ```
- **Azure SQL Database**: Cloud option (requires Azure account)

#### 4. VS Code
```bash
sudo snap install code --classic
# Or: https://code.visualstudio.com/download
```

## Trust Development HTTPS Certificate

After installing .NET, trust the development certificate:

```bash
# All platforms
dotnet dev-certs https --trust
```

On macOS, you may need to approve the certificate in Keychain.

## Verify Installation

Run from repository root:

```bash
# Check .NET
dotnet --version                # Should show 9.0.x

# Check Node.js
node --version                  # Should show v18.x.x or higher
npm --version                   # Should show 9.x.x or higher

# Test building solution
dotnet build MiniShop.sln       # Should complete successfully
```

## Database Setup

### Option 1: SQL Server Express (Windows)

1. Install SQL Server Express from Microsoft Download
2. Create databases using SSMS:
   ```sql
   CREATE DATABASE MiniShop_Catalog;
   CREATE DATABASE MiniShop_Orders;
   ```

### Option 2: Azure Data Studio (All Platforms)

1. Download: https://learn.microsoft.com/en-us/sql/azure-data-studio/
2. Connect to your SQL Server instance
3. Run:
   ```sql
   CREATE DATABASE MiniShop_Catalog;
   CREATE DATABASE MiniShop_Orders;
   ```

### Option 3: Entity Framework Migrations

Databases will be created automatically when running migrations:

```bash
dotnet ef database update --project CatalogService
dotnet ef database update --project OrdersService
```

## Environment Variables

### Frontend (.env)
Create `frontend/.env`:
```
REACT_APP_API_URL=https://localhost:7000/api
```

### Backend (appsettings.json)
Connection strings in each service's `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Server=localhost;Database=MiniShop_Catalog;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

For Docker SQL Server:
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Server=localhost,1433;User Id=sa;Password=Pass@123;Database=MiniShop_Catalog;TrustServerCertificate=True"
  }
}
```

## VS Code Extensions (Recommended)

```
C# Dev Kit
C# Tools
REST Client
SQL Tools
Thunder Client (or Postman)
```

Install from VS Code Extensions (Ctrl+Shift+X / Cmd+Shift+X)

## Verify Setup

Run the setup script:

**Windows:**
```bash
setup.bat
```

**macOS/Linux:**
```bash
chmod +x setup.sh
./setup.sh
```

Or manual verification:
1. `dotnet build MiniShop.sln` - Should complete
2. `cd frontend && npm install` - Should complete
3. Create SQL databases
4. Run migrations: `dotnet ef database update --project CatalogService`
5. Start services and frontend

## Troubleshooting

### dotnet command not found
- Add .NET to PATH (usually automatic)
- Restart terminal/IDE after install
- On macOS: `export PATH="$PATH:$HOME/.dotnet"`

### npm command not found
- Reinstall Node.js
- Close and reopen terminal

### SQL Server connection failed
- Verify SQL Server is running
- Check Windows Authentication enabled
- Verify `Trusted_Connection=True` in connection string
- For Docker: ensure container is running: `docker ps`

### Port already in use
- Change port in `launchSettings.json` if needed
- Kill process: `netstat -ano | findstr :7001` (Windows)

### HTTPS certificate error
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## Performance Tips

- Use **SSD** for SQL Server databases
- Allocate sufficient RAM to SQL Server (at least 2GB)
- Use **VS Code** for lighter IDE footprint
- Keep `node_modules` on local drive (avoid network shares)
- Disable Windows Defender scanning for code directories

## Security Notes

⚠️ **Development Only**

The current setup uses:
- Windows Authentication (local development)
- `AllowAnyOrigin()` CORS (development convenience)
- Self-signed HTTPS certificates

**For production**, implement:
- JWT authentication
- Restricted CORS policies
- Valid SSL certificates (Let's Encrypt, etc.)
- API key authentication
- Rate limiting
- Input validation

---

**Last Updated:** December 31, 2025
