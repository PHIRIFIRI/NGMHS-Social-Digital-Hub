# NGMHS Digital Hub - Setup and Deployment Guide

This guide explains how to run the MVC solution, onboard users, and expose the app to your WordPress site as a shortcut button.

## 1. Prerequisites

- Windows machine with Visual Studio 2026 (or latest preview with .NET 10 SDK)
- SQL Server LocalDB or SQL Server Express
- Network access from office laptops to the host machine

## 2. Configure the Application

Open `appsettings.json` and update:

- `ConnectionStrings:DefaultConnection`
- `SeedAdmin` credentials (recommended before first launch)
- `Storage` path/limits if needed

Current defaults:

- Database: `NGMHSDigitalHubDB`
- Admin email: `admin@ngmhs.local`
- Admin password: `Admin#2026`

## 3. Run the App Locally

From the project folder:

```powershell
$env:DOTNET_CLI_HOME='C:\Users\phiri\source\repos\NGMHS\.dotnet'
dotnet run
```

The app auto-creates the database schema (`EnsureCreated`) and seeds the admin account on first run.

## 4. User and Role Flow

- Social workers register from `/Account/Register`
- Admin signs in with the seeded account
- Admin can promote/demote users from `Admin > Users`
- All activity is tracked by authenticated user identity

## 5. Main Modules

- Dashboard: `/Dashboard/Index`
- Social work forms CRUD: `/SocialWorkForms/Index`
  - Templates: Form 2, 7, 33, 56, 77
  - Statuses: Active, Completed, Submitted
  - Print and TXT download per form
  - File attachments (PDF/images/docs)
- External public query form: `/ExternalQuery/Create`
  - Staff management: `/ExternalQuery/Index`
- Fundraising/marketing outreach letters: `/Outreach/Index`
  - Invitations, donation requests, funding proposals
  - Print and TXT download
  - File attachments
- Admin exports and files:
  - `/Admin/Index`
  - CSV exports for forms/queries/outreach
  - Central file access page

## 6. Publish for Office Use

```powershell
dotnet publish -c Release -o .\publish
```

Deploy the `publish` output to the office host PC/server.

Run hosted app on LAN (example port 5055):

```powershell
.\NGMHS.exe --urls "http://0.0.0.0:5055"
```

Or:

```powershell
dotnet .\NGMHS.dll --urls "http://0.0.0.0:5055"
```

Then allow the port in Windows Firewall.

## 7. WordPress Shortcut Button

Add this in a WordPress Custom HTML block (replace IP/hostname):

```html
<a class="ngmhs-shortcut" href="http://192.168.1.50:5055" target="_blank" rel="noopener">
  Open NGMHS Digital Hub
</a>
```

Optional CSS (Appearance -> Customize -> Additional CSS):

```css
.ngmhs-shortcut {
  display: inline-block;
  padding: 12px 20px;
  border-radius: 8px;
  background: #0b4f6c;
  color: #fff;
  font-weight: 700;
  text-decoration: none;
}
.ngmhs-shortcut:hover {
  background: #083a50;
  color: #fff;
}
```

## 8. Desktop Shortcut on Office Laptops

Create browser/bookmark shortcuts pointing to:

- `http://192.168.1.50:5055`

This gives social workers one-click access from laptops.

## 9. Security and Operations Notes

- Change seeded admin password immediately after first login.
- Use HTTPS behind IIS/reverse proxy for production.
- Back up SQL Server database and `Storage` folder regularly.
- Keep one account per employee for full audit traceability.
