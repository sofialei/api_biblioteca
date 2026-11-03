# api_biblioteca

API REST y GraphQL para gestionar una biblioteca: libros, socios y préstamos/devoluciones.
Proyecto final desarrollado en equipo con ASP.NET Core, Entity Framework Core (Code First) y GraphQL.

## Funcionalidades

- **Libros:** alta, baja, modificación y consulta.
- **Socios:** alta, baja, modificación y consulta, con validación de DNI y email.
- **Préstamos:** registrar préstamo, registrar devolución, listar préstamos activos e historial por socio.
- **Reglas de negocio:** no se presta un libro ya prestado, el libro y el socio deben existir, no se permiten devoluciones repetidas y el historial se conserva.
- **Dos accesos a los mismos servicios:** REST (con Swagger) y GraphQL.

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server o SQL Server LocalDB (viene con Visual Studio)
- Herramienta de migraciones de EF Core:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

## Validaciones

- **ISBN:** 10 o 13 dígitos, sin guiones, y único por libro.
- **DNI:** 7 u 8 dígitos, sin puntos, y único por socio.
- **Email:** formato válido y único por socio (se guarda en minúsculas).
- Un libro o socio con préstamos registrados no se elimina, para conservar el historial.

## Instalación

```bash
git clone https://github.com/sofialei/api_biblioteca.git
cd api_biblioteca
dotnet restore
```

## Configuración

La cadena de conexión está en `Biblioteca_API/appsettings.json`. Por defecto usa SQL Server LocalDB; si usás otro servidor, cambiá el valor de `Server`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BibliotecaDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```


Crear la base de datos aplicando las migraciones:

```bash
dotnet ef database update --project Biblioteca_API
```

## Ejecución

```bash
dotnet run --project Biblioteca_API
```

Una vez iniciada la aplicación (la consola muestra el puerto):

| Recurso | Ruta |
|---|---|
| Swagger (REST) | `/swagger` |
| GraphQL | `/graphql` |

## API REST

| Método | Endpoint | Función |
|---|---|---|
| GET | `/api/libros` | Listar libros |
| GET | `/api/libros/{id}` | Buscar un libro |
| POST | `/api/libros` | Crear un libro |
| PUT | `/api/libros/{id}` | Modificar un libro |
| DELETE | `/api/libros/{id}` | Eliminar un libro |
| GET | `/api/socios` | Listar socios |
| GET | `/api/socios/{id}` | Buscar un socio |
| POST | `/api/socios` | Crear un socio |
| PUT | `/api/socios/{id}` | Modificar un socio |
| DELETE | `/api/socios/{id}` | Eliminar un socio |
| POST | `/api/prestamos` | Registrar un préstamo |
| PUT | `/api/prestamos/{id}/devolucion` | Registrar una devolución |
| GET | `/api/prestamos/activos` | Listar préstamos activos |
| GET | `/api/socios/{id}/prestamos` | Historial de préstamos de un socio |


## GraphQL

Se accede en `/graphql`. Allí se puede usar el IDE integrado (Banana Cake Pop) o cualquier cliente
que envíe un `POST` con `{ "query": "..." }`.
Usa los mismos servicios que REST, por lo que las reglas de negocio son idénticas.

### Consultas

Todos los libros:

```graphql
query {
  libros {
    id
    titulo
    autor
    isbn
    genero
    disponible
  }
}
```

Un libro por ID:

```graphql
query {
  libro(id: 1) {
    id
    titulo
    disponible
  }
}
```

Socios:

```graphql
query {
  socios {
    id
    nombre
    email
    dni
  }
}
```

Préstamos activos:

```graphql
query {
  prestamosActivos {
    id
    libroId
    tituloLibro
    socioId
    nombreSocio
    fechaPrestamo
    devuelto
  }
}
```

Historial de préstamos de un socio:

```graphql
query {
  historialSocio(socioId: 2) {
    id
    libroId
    tituloLibro
    fechaPrestamo
    fechaDevolucion
    devuelto
  }
}
```

### Mutaciones

Registrar un préstamo:

```graphql
mutation {
  registrarPrestamo(input: { libroId: 1, socioId: 2 }) {
    id
    libroId
    tituloLibro
    socioId
    nombreSocio
    devuelto
  }
}
```

Registrar una devolución:

```graphql
mutation {
  devolverPrestamo(id: 1) {
    id
    devuelto
    fechaDevolucion
  }
}
```

Si una regla de negocio no se cumple (por ejemplo, el libro ya está prestado), la respuesta incluye un campo `errors` con el mensaje.

## Pruebas

```bash
dotnet test
```

El proyecto `Biblioteca_API.Tests` incluye pruebas unitarias de cada módulo y pruebas de integración
(`IntegrationTests.cs`) que levantan la API en memoria con una base de datos en memoria y prueban REST y GraphQL.

## Estructura del proyecto

```
Biblioteca_API/
├── Controllers/    Endpoints REST
├── Models/         Entidades (Libro, Socio, Prestamo)
├── DTOs/           Objetos de transferencia
├── Interfaces/     Contratos de los servicios
├── Services/       Lógica de negocio
├── Data/           DbContext y configuración de EF Core
├── GraphQL/        Query y Mutation
├── Migrations/     Migraciones de EF Core
└── Program.cs      Configuración e inyección de dependencias
Biblioteca_API.Tests/   Pruebas unitarias y de integración
```

## Diagramas

### DER (modelo entidad-relación)

Un socio puede tener muchos préstamos y un libro puede aparecer en muchos préstamos (a lo largo del tiempo).
Las claves foráneas usan `Restrict`, por eso no se puede borrar un libro o socio con préstamos y el historial se conserva.

```mermaid
erDiagram
    SOCIO ||--o{ PRESTAMO : realiza
    LIBRO ||--o{ PRESTAMO : "se presta en"

    LIBRO {
        int Id PK
        string Titulo
        string Autor
        string ISBN UK
        bool Disponible
        string Genero
    }
    SOCIO {
        int Id PK
        string Nombre
        string Email UK
        string DNI UK
    }
    PRESTAMO {
        int Id PK
        int LibroId FK
        int SocioId FK
        datetime FechaPrestamo
        datetime FechaDevolucion "puede ser nulo"
        bool Devuelto
    }
```

### UML (clases y dependencias)

Los controladores y los resolvers de GraphQL dependen de las interfaces; las interfaces las implementan los servicios,
y los servicios usan el `BibliotecaDbContext`.

```mermaid
classDiagram
    direction LR

    class Libro {
        +int Id
        +string Titulo
        +string Autor
        +string ISBN
        +bool Disponible
        +string Genero
    }
    class Socio {
        +int Id
        +string Nombre
        +string Email
        +string DNI
    }
    class Prestamo {
        +int Id
        +int LibroId
        +int SocioId
        +DateTime FechaPrestamo
        +DateTime? FechaDevolucion
        +bool Devuelto
    }
    Libro "1" --> "*" Prestamo
    Socio "1" --> "*" Prestamo

    class ILibroService {
        <<interface>>
    }
    class ISocioService {
        <<interface>>
    }
    class IPrestamoService {
        <<interface>>
        +RegistrarAsync(PrestamoDTO)
        +DevolverAsync(int)
        +ObtenerActivosAsync()
        +ObtenerHistorialPorSocioAsync(int)
    }
    ILibroService <|.. LibroService
    ISocioService <|.. SocioService
    IPrestamoService <|.. PrestamoService

    class BibliotecaDbContext
    LibroService --> BibliotecaDbContext
    SocioService --> BibliotecaDbContext
    PrestamoService --> BibliotecaDbContext
    BibliotecaDbContext --> Libro
    BibliotecaDbContext --> Socio
    BibliotecaDbContext --> Prestamo

    LibrosController --> ILibroService
    SociosController --> ISocioService
    PrestamosController --> IPrestamoService
    Query --> ILibroService
    Query --> ISocioService
    Query --> IPrestamoService
    Mutation --> IPrestamoService
```

## Diseño: SOLID, Clean Code e inyección de dependencias

### SOLID

| Principio | Dónde se aplica | Cómo |
|---|---|---|
| **S** – Responsabilidad única | `LibroService`, `SocioService`, `PrestamoService`, controladores | Cada servicio atiende un solo módulo (`PrestamoService` solo se ocupa de las reglas de préstamos). Los controladores solo traducen HTTP a llamadas al servicio y las excepciones a códigos de estado. |
| **O** – Abierto/cerrado | Interfaces de servicio | Se puede agregar otra implementación de una interfaz (por ejemplo, con caché) sin modificar controladores ni GraphQL; solo se cambia el registro en `Program.cs`. |
| **L** – Sustitución de Liskov | `ILibroService`, `ISocioService`, `IPrestamoService` | Cualquier implementación respeta el contrato: devuelve `null` o `false` si el recurso no existe y lanza `KeyNotFoundException` o `InvalidOperationException` ante una regla incumplida (documentado en `IPrestamoService`). |
| **I** – Segregación de interfaces | Tres interfaces separadas | Cada consumidor depende solo de lo que usa; por ejemplo, la clase `Mutation` solo conoce `IPrestamoService`. |
| **D** – Inversión de dependencias | Constructores de los controladores y parámetros `[Service]` de GraphQL | Las capas superiores dependen de interfaces, no de clases concretas; `Program.cs` decide qué implementación usar. |

### Clean Code

- **Nombres descriptivos:** `RegistrarAsync`, `DevolverAsync`, `ObtenerActivosAsync`, `ObtenerHistorialPorSocioAsync`, `PrestamoResponseDTO`.
- **Métodos con una sola tarea:** cada método del servicio resuelve una operación, y la conversión de entidad a DTO está en un método privado `ToDto`.
- **Sin duplicación de reglas:** REST y GraphQL usan los mismos servicios, así que cada regla de negocio existe en un solo lugar.
- **Separación de responsabilidades:** modelos, datos, contratos, lógica, DTOs, controladores y GraphQL en carpetas distintas.
- **Validaciones declarativas:** anotaciones en los DTOs (`Required`, `StringLength`, `RegularExpression`, `EmailAddress`) que `[ApiController]` aplica antes de llegar al servicio.
- **Mensajes de error claros** y códigos de estado coherentes: 400 (datos inválidos), 404 (no existe) y 409 (conflicto con el estado actual).
- **Entidades protegidas:** la API nunca devuelve entidades de la base, solo DTOs.

### Inyección de dependencias

Los servicios se registran una sola vez en `Program.cs` y el contenedor de .NET los entrega:

```csharp
builder.Services.AddScoped<ILibroService, LibroService>();
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IPrestamoService, PrestamoService>();
```

Los controladores reciben la interfaz por constructor, sin crear el servicio con `new`:

```csharp
public LibrosController(ILibroService libroService)
{
    _libroService = libroService;
}
```

En GraphQL se usa `[Service]` para recibir la misma interfaz en cada resolver. `AddScoped` crea una instancia por solicitud, que comparte el mismo `BibliotecaDbContext`.

## Equipo

| Módulo | Integrante |
|---|---|
| Libros | Tati |
| Socios | Mateo |
| Préstamos y devoluciones | Angie |
| Base de datos, arquitectura y REST | Sofia |
| GraphQL, pruebas de integración y documentación | Valeria |
