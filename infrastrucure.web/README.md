# Infrastructure Web UI

This project provides an ASP.NET Core MVC frontend for the Infrastructure API.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

## Configuration

The frontend reads the API base URL from `appsettings.json` (`ApiBaseUrl`). By default, it points to `https://localhost:7100/`.

## Running the solution

1. **Start the API**
   ```bash
   dotnet run --project ../infrastructure.api
   ```
   The API exposes Swagger at https://localhost:7100/swagger.

2. **Start the web application**
   ```bash
   dotnet run --project infrastrucure.web --urls https://localhost:7200
   ```
   The UI will be available at https://localhost:7200/components.

## Usage

1. Navigate to https://localhost:7200/components.
2. Create a new component using the **New Component** button.
3. Use the action buttons in the list to provision, deploy, or tear down a component.
4. Edit or delete components using the corresponding icons.

The interface is responsive, styled with Bootstrap 5, and uses FontAwesome for icons.
