# CozyComfort - Run Instructions

## Multiple Projects in Visual Studio

1. Open `CozyComfort.SOC.sln` in Visual Studio.
2. Right-click the **Solution** in Solution Explorer → **Properties**.
3. Under **Startup Project**, select **Multiple startup projects**.
4. Set **CozyComfort.API** → **Start**.
5. Set **CozyComfort.Web** → **Start**.
6. Press **F5** or click **Start** – both projects will run.

- **API (Backend):** http://localhost:5023  
- **Web (Frontend):** http://localhost:5250  

## Alternative: PowerShell Script

```powershell
.\RunBoth.ps1
```

## Sample Login Credentials (after first run / DB seed)

| Role        | Email                   | Password   |
|-------------|-------------------------|------------|
| Admin       | admin@cozycomfort.local | Admin@123  |
| Manufacturer| mfg@cozycomfort.local   | Mfg@123    |
| Distributor | dist@cozycomfort.local  | Dist@123   |
| Seller      | seller@cozycomfort.local| Seller@123 |
| Customer    | customer@cozycomfort.local | Customer@123 |

## Database

Uses SQL Server. Update connection string in `appsettings.json` (API) if needed. On first run, migrations apply and sample data is seeded.
