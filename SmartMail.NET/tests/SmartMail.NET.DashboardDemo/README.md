# SmartMail.NET Dashboard Demo

This demo project shows how to integrate and use the SmartMail.NET Dashboard in your ASP.NET Core application.

## Features Demonstrated

- Integration of SmartMail.NET Dashboard
- JWT Authentication for dashboard access
- Multiple email provider configuration
- Real-time provider monitoring
- Email statistics tracking

## Getting Started

1. Run the application:
```bash
dotnet run
```

2. The application will start with the following endpoints:
   - Dashboard: `https://localhost:5001/SmartMail`
   - Swagger UI: `https://localhost:5001/swagger`
   - Token endpoint: `https://localhost:5001/api/token`

## Accessing the Dashboard

1. First, get a JWT token by calling the token endpoint:
```bash
curl https://localhost:5001/api/token
```

2. Use the token to access the dashboard:
   - Open `https://localhost:5001/SmartMail` in your browser
   - Add the token to your request header:
     ```
     Authorization: Bearer <your-token>
     ```

## Demo Configuration

The demo includes three email providers:
1. Gmail SMTP (500 emails quota)
2. Office 365 SMTP (1000 emails quota)
3. AWS SES (2000 emails quota)

## Security

The dashboard is configured with:
- JWT authentication
- Role-based access control (Admin role required)
- HTTPS enabled

## Customization

You can customize the dashboard by modifying the `Program.cs` file:
- Change the dashboard path
- Modify authentication requirements
- Add or remove email providers
- Adjust quota limits

## Notes

- This is a demo application with mock credentials
- The JWT secret key is hardcoded for demonstration purposes
- In a production environment, use proper secrets management 