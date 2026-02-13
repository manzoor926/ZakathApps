# Zakath App - Web Application

Complete ASP.NET Core MVC web application for the Zakath tracking system.

## Features

✅ **Authentication** - Login & Registration
✅ **Dashboard** - Overview of finances and Zakath status
✅ **Income Management** - Track income sources
✅ **Expense Tracking** - Monitor spending
✅ **Asset Management** - Manage wealth and assets
✅ **Zakath Calculator** - Calculate Zakath obligations
✅ **Payment Records** - Track Zakath payments
✅ **Responsive Design** - Bootstrap 5 UI
✅ **API Integration** - Connects to your existing API

## Project Structure

```
ZakathApp.Web/
├── Controllers/          # MVC Controllers
│   ├── AuthController.cs
│   ├── DashboardController.cs
│   ├── IncomeController.cs
│   ├── ExpenseController.cs
│   ├── AssetController.cs
│   ├── ZakathController.cs
│   └── PaymentController.cs
├── Models/              # ViewModels and DTOs
│   └── Models.cs
├── Services/            # API Client service
│   └── ApiClient.cs
├── Views/               # Razor views
│   ├── Auth/
│   ├── Dashboard/
│   ├── Income/
│   ├── Expense/
│   ├── Asset/
│   ├── Zakath/
│   ├── Payment/
│   └── Shared/
├── wwwroot/             # Static files
│   └── css/
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── ZakathApp.Web.csproj # Project file
```

## Setup Instructions

### Prerequisites
- .NET 9.0 SDK
- Your Zakath API running (either locally or on server)

### Step 1: Update API URL

Edit `appsettings.json` and set your API URL:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://portal.rsconline.org/API"
  }
}
```

For local testing, you can use:
```json
"BaseUrl": "http://localhost:5000/api"
```

### Step 2: Build and Run

**Option A - Using Visual Studio:**
1. Open `ZakathApp.Web.csproj` in Visual Studio
2. Press F5 to run

**Option B - Using Command Line:**
```bash
cd ZakathApp.Web
dotnet restore
dotnet build
dotnet run
```

The application will start on `https://localhost:5001` (or port shown in console).

### Step 3: First Time Setup

1. Navigate to `https://localhost:5001` in your browser
2. Click "Register" to create a new account
3. Fill in:
   - Full Name
   - Mobile Number (used for login)
   - Email (optional)
   - Password
4. After registration, login with your mobile number and password

## Publishing to IIS

### Method 1: Publish to Folder

```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\ZakathWeb
```

Then in IIS Manager:
1. Create a new site
2. Point it to `C:\inetpub\wwwroot\ZakathWeb`
3. Make sure the Application Pool is set to "No Managed Code"
4. Set the binding (port 80 for HTTP, 443 for HTTPS)

### Method 2: Publish from Visual Studio

1. Right-click the project → Publish
2. Choose "Folder" or "IIS"
3. Follow the wizard
4. Deploy the published files to your server

## Configuration

### Change API URL
Edit `appsettings.json`:
```json
"ApiSettings": {
  "BaseUrl": "https://your-api-url.com/api"
}
```

### Session Timeout
Sessions last 24 hours by default. To change, edit `Program.cs`:
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Change to 2 hours
});
```

## Usage

### Login
- Use your mobile number and password
- Sessions persist for 24 hours

### Dashboard
- View total income, expenses, assets
- See current Zakath status
- Recent transaction history

### Income/Expense Management
- Click "Add Income" or "Add Expense"
- Select category, enter amount and description
- Delete entries using the trash icon

### Asset Management
- Add assets with name, value, and category
- Track which assets are Zakath-applicable
- Update values as needed

### Zakath Calculator
- Click "Calculate Zakath" button
- System calculates based on current wealth
- View calculation history

### Payment Records
- Record Zakath payments
- Track recipient names and notes
- View payment history

## Troubleshooting

### "Cannot connect to API"
- Check that your API URL in `appsettings.json` is correct
- Verify the API is running and accessible
- Check CORS settings on the API

### "Login failed"
- Verify the API is running
- Check that the user exists in the database
- Ensure the password is correct

### Session expires too quickly
- Increase `IdleTimeout` in `Program.cs`
- Check browser cookies are enabled

### IIS 500 errors
- Make sure .NET 9 Hosting Bundle is installed
- Check Application Pool is set to "No Managed Code"
- Verify permissions on the application folder

## API Endpoints Used

This web app consumes these API endpoints:

**Auth:**
- POST `/Auth/login` - User login
- POST `/Auth/register` - User registration
- GET `/Auth/profile` - Get user profile

**Dashboard:**
- GET `/Dashboard/stats` - Financial statistics
- GET `/Dashboard/recent-transactions` - Recent activity

**Income:**
- GET `/Income` - List all income
- POST `/Income` - Create income
- DELETE `/Income/{id}` - Delete income

**Expense:**
- GET `/Expense` - List all expenses
- POST `/Expense` - Create expense
- DELETE `/Expense/{id}` - Delete expense

**Asset:**
- GET `/Asset` - List all assets
- POST `/Asset` - Create asset
- DELETE `/Asset/{id}` - Delete asset

**Zakath:**
- POST `/Zakath/calculate` - Calculate Zakath
- GET `/Zakath/history` - Calculation history

**Payment:**
- GET `/Payment` - List payments
- POST `/Payment` - Record payment
- DELETE `/Payment/{id}` - Delete payment

**Categories:**
- GET `/Category/{type}` - Income/Expense categories
- GET `/AssetCategory` - Asset categories

## Development

### Adding New Features

1. **Add Model** - Add to `Models/Models.cs`
2. **Add Service Method** - Update `Services/ApiClient.cs`
3. **Add Controller** - Create in `Controllers/`
4. **Add View** - Create in `Views/`

### Debugging

Enable detailed logging in `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Information"
  }
}
```

## Security Notes

- Passwords are sent to the API (API handles hashing)
- Sessions use secure cookies
- HTTPS recommended for production
- API tokens stored in session (server-side)

## Support

For issues:
1. Check API is running and accessible
2. Verify `appsettings.json` configuration
3. Check browser console for JavaScript errors
4. Check application logs

## License

© 2026 Zakath App. All rights reserved.
