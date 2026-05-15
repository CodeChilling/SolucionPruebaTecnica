# API de Consulta de Clientes

## Descripción del Proyecto

Este proyecto implementa una solución en **.NET Core** que consume un servicio de **Power Automate** para consultar información de clientes. El servicio expone un endpoint para la consulta de clientes mediante un llamado POST.

### Funcionalidad Principal

La API recibe un identificador de cliente (`customerId`) y retorna la información básica del cliente en formato JSON, incluyendo: nombre, apellido, email, edad y ciudad.

---

## Arquitectura del Proyecto

El proyecto sigue una arquitectura por **capas** (Clean Architecture simplificada):

```
SolucionPruebaTecnica/
├── SolutionPruebaTecnica.Presentation/    (API - Capa de Presentación)
├── SolucionPruebaTecnica.Application/      (Servicios - Capa de Aplicación)
├── SolucionPruebaTecnica.Domain/           (Entidades e Interfaces - Capa de Dominio)
├── SolucionPruebaTecnica.Infrastructure/    (Implementaciones - Capa de Infraestructura)
└── SolucionPruebaTecnica.Testing/          (Pruebas Unitarias)
```

### Capa de Presentación (`Presentation`)
- **CustomerController**: Controlador REST que expone el endpoint `POST /api/customer`
- **ExceptionFilter**: Manejo global de excepciones
- **Program.cs**: Configuración de la aplicación, inyección de dependencias y Swagger

### Capa de Aplicación (`Application`)
- **CustomerService**: Servicio de lógica de negocio para consulta de clientes
- **DTOs**: CustomerRequest, CustomerResponse
- **Mappings**: Perfiles de AutoMapper

### Capa de Dominio (`Domain`)
- **Entities**: Customer (entidad del dominio)
- **Interfaces**: ICustomerService, ICustomerRepository, IOAuthTokenService
- Contratos desacoplados que permiten inversión de dependencias

### Capa de Infraestructura (`Infrastructure`)
- **PowerAutomateCustomerAdapter**: Implementación del repositorio que consume el servicio de Power Automate
- **OAuthTokenService**: Servicio de autenticación OAuth 2.0 con caching de tokens
- **Configuration**: PowerAutomateOptions para configuración

---

## Características Técnicas

### Autenticación OAuth 2.0
- Autenticación mediante **client_credentials** (clientId, clientSecret, tenantId)
- Caching de tokens para evitar llamadas innecesarias al servidor de autenticación
- Tokens almacenados en memoria con tiempo de expiración

### Seguridad
- Uso de **User Secrets** en desarrollo para almacenar credenciales sensibles
- Tokens OAuth transmitidos mediante el header `Authorization: Bearer`

### Manejo de Errores
- **Polly** para estrategias de resiliencia:
  - Retry: 3 reintentos con backoff exponencial
  - Circuit Breaker: Después de 5 errores, abre el circuito por 30 segundos
- Timeouts configurados (30 segundos por solicitud)
- Manejo de errores HTTP (404, 500, etc.)
- Manejo de excepciones específicas (TaskCanceledException, HttpRequestException)

### Programación Asíncrona
- Todos los métodos del servicio y repositorio son `async Task`
- Uso de `CancellationToken` para cancelación de operaciones

### Inyección de Dependencias
- Configuración mediante `Microsoft.Extensions.DependencyInjection`
- Registro de servicios con cyclic lifetime (AddScoped)
- HttpClient factory con políticas de resiliencia

### Documentación Interactive
- **Scalar** (OpenAPI) para documentación interactiva de la API
- Disponible en entorno de desarrollo en `/scalar/v1`

---

## Endpoints

### POST /api/customer
Consulta la información de un cliente por su ID.

**Request:**
```json
{
  "customerId": "guid-válido"
}
```

**Response (200 OK):**
```json
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "age": "int",
  "city": "string"
}
```

**Response (404 Not Found):**
```json
{
  "message": "Customer not found"
}
```

**Response (400 Bad Request):**
```json
{
  "errors": {
    "customerId": ["El ID del cliente debe ser un GUID válido."]
  }
}
```

---

## Configuración

### appsettings.json
```json
{
  "PowerAutomate": {
    "BaseUrl": "https://powerautomate.ejerciciosenior.prueba.api.powerplatform.com:100/invokeflow/",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "TenantId": "your-tenant-id",
    "Scope": "https://powerautomate.ejerciciosenior.prueba.api.powerplatform.com/.default",
    "AuthUrl": "https://login.microsoftonline.com"
  }
}
```

> **Nota**: En desarrollo, usar `dotnet user-secrets` para almacenar las credenciales de forma segura.

---

## Ejecución del Proyecto

### Requisitos Previos
- .NET SDK 10.0 o superior
- Visual Studio 2022 o VS Code con extensiones de C#

### Compilación
```bash
dotnet build
```

### Ejecución
```bash
cd SolutionPruebaTecnica.Presentation
dotnet run
```

### Documentación Swagger
Acceder a: `http://localhost:5xxx/scalar/v1` (en desarrollo)

---

## Pruebas Unitarias

El proyecto incluye 22 pruebas unitarias utilizando **xUnit**, **Moq** y **FluentAssertions**.

### Suites de Pruebas

1. **CustomerServiceTests** (6 tests)
   - Validación de ID de cliente
   - Casos exitosos
   - Manejo de errores

2. **PowerAutomateCustomerAdapterTests** (6 tests)
   - Cliente existente/no existente
   - Errores HTTP
   - Solicitudes canceladas
   - Respuestas nulas

3. **OAuthTokenServiceTests** (8 tests)
   - Obtención de tokens
   - Caching de tokens
   - Expiración de tokens
   - Validación de parámetros OAuth

### Ejecución de Pruebas
```bash
dotnet test
```

---

## Tecnologías Utilizadas

| Tecnología | Propósito |
|------------|-----------|
| .NET 10.0 | Framework principal |
| ASP.NET Core | Framework web |
| AutoMapper | Mapeo de objetos |
| Polly | Resiliencia y manejo de reintentos |
| Moq | Mocking para pruebas |
| FluentAssertions | Aserciones en pruebas |
| xUnit | Framework de pruebas |
| Scalar/OpenAPI | Documentación interactiva |

---

## Estructura de Archivos Clave

```
SolucionPruebaTecnica/
├── SolutionPruebaTecnica.Presentation/
│   ├── Controllers/CustomerController.cs
│   ├── DepedencyInjection/DependencyInjection.cs
│   ├── Exceptions/ExceptionFilter.cs
│   └── Program.cs
├── SolucionPruebaTecnica.Application/
│   ├── Servicios/CustomerService.cs
│   └── Common/DTO/CustomerRequest.cs, CustomerResponse.cs
├── SolucionPruebaTecnica.Domain/
│   ├── Entities/Customer.cs
│   └── Interfaces/ICustomerService.cs, ICustomerRepository.cs, IOAuthTokenService.cs
├── SolucionPruebaTecnica.Infrastructure/
│   ├── Adapter/PowerAutomateCustomerAdapter.cs
│   ├── Authentication/OAuthTokenService.cs
│   └── Configuration/PowerAutomateOptions.cs
└── SolucionPruebaTecnica.Testing/
    ├── CustomerServiceTests.cs
    ├── PowerAutomateCustomerAdapterTests.cs
    └── OAuthTokenServiceTests.cs
```

---

## Autor

Desarrollado como solución técnica.