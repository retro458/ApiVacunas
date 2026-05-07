# API Vacunas

API REST para la app movil de vacunas, la api fue desarrollada con **ASP.NET Core 9** con **Oracle XE 21c**

---

## Tabla de contenido ##

-[Requisitos](#requisitos)
-[Configuracion](#configuracion)
-[Autenticacion](#autenticacion)
-[Endpoints](#endpoints)
-[Ejemplos en Java/Android](#ejemplos-en-javaandroid)
-[Codigos de respuesta](#codigos-de-respuesta)

---

## Requisitos
 
- .NET 9 SDK
- Oracle XE 21c (Docker)
- Docker + Docker Compose
---
 
## Configuración
 
### Desarrollo local
 
```bash
git clone https://github.com/retro458/ApiVacunas
cd ApiVacunas
```
 
Configurar secrets:
 
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Oracle" "User Id=vacunas_usr;Password=TuPassword;Data Source=localhost:1521/vacunas;"
dotnet user-secrets set "Jwt:Key" "ClaveJWT"
dotnet user-secrets set "Google:ClientId" "client-id.apps.googleusercontent.com"
```
 
Correr la API:
 
```bash
dotnet run
```
 
Swagger disponible en: `http://localhost:5190/swagger`
 
---
 
## Autenticación
 
La API usa **JWT Bearer Token**. Todos los endpoints excepto `/api/auth/*` requieren el header:
 
```
Authorization: Bearer {token}
```
 
El token se obtiene al hacer login o registro y expira en **24 horas**.
 
---
 
## Endpoints

### Auth
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/registro` | Registrar nuevo usuario | No |
| POST | `/api/auth/login` | Iniciar sesión | No |
| POST | `/api/auth/google` | Login con Google | No |
 
#### POST /api/auth/registro
 
```json
// Request
{
  "nombre": "María López",
  "correo": "maria@correo.com",
  "password": "MiPassword123!",
  "telefono": "78001234"
}
 
// Response 200
{
  "exito": true,
  "mensaje": "Usuario registrado correctamente.",
  "data": { "idUsuario": 1 }
}
```
 
#### POST /api/auth/login
 
```json
// Request
{
  "correo": "maria@correo.com",
  "password": "MiPassword123!"
}
 
// Response 200
{
  "exito": true,
  "mensaje": "Login exitoso.",
  "data": {
    "token": "eyJhbGci...",
    "nombre": "María López",
    "correo": "maria@correo.com",
    "rol": "usuario",
    "idUsuario": 1
  }
}
```
 
#### POST /api/auth/google
 
```json
// Request — idToken generado por Google Sign-In SDK en Android
{
  "idToken": "eyJhbGci..."
}
 
// Response 200 — mismo formato que login normal
{
  "exito": true,
  "mensaje": "Login con Google exitoso.",
  "data": {
    "token": "eyJhbGci...",
    "nombre": "Erick Baudriz",
    "correo": "correo@gmail.com",
    "rol": "usuario",
    "idUsuario": 6
  }
}
```
 
---

### Miembros
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/vacunas` | Catálogo completo |
| GET | `/api/vacunas?tipo=humano` | Filtrar por tipo |
| GET | `/api/vacunas/{id}` | Vacuna + esquema de dosis |
 
---
 ### Historial de navegacion
  Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/historial/{idMiembro}` | Historial completo |
| POST | `/api/historial` | Registrar vacunación |
| GET | `/api/historial/proximas-dosis` | Próximas dosis pendientes |
 
```json
// POST /api/historial
{
  "idMiembro": 1,
  "idVacuna": 1,
  "idCentro": 1,
  "fechaAplicacion": "2026-05-07",
  "dosisNumero": 1,
  "lote": "LOT-2026-001",
  "nombreMedico": "Dr. García",
  "observaciones": null
}
```
 
> Al registrar una vacunación, el sistema calcula automáticamente la próxima dosis y crea el recordatorio. esto gracias a un sp en oracle para facilitar la transabilidad entre tablas
 
 ### Recordatorios
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/recordatorios` | Todos los recordatorios |
| GET | `/api/recordatorios?estado=pendiente` | Filtrar por estado |
| GET | `/api/recordatorios/proximos` | Próximos 30 días |
| GET | `/api/recordatorios/proximos?dias=7` | Próximos N días |
| GET | `/api/recordatorios/campanias` | Campañas próximas |
| PATCH | `/api/recordatorios/{id}/estado` | Actualizar estado |
 
**Estados válidos:** `pendiente` \| `completado` \| `pospuesto`
 
---

### Centros de Vacunación
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/centros` | Lista todos los centros |
| GET | `/api/centros/{id}` | Centro específico |
| GET | `/api/centros/{id}/detalle` | Centro + vacunas disponibles |
| GET | `/api/centros/cercanos?latitud=13.69&longitud=-89.21&radioKm=5` | Centros cercanos |
 
---
### Campañas
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/campanias` | Campañas activas |
| GET | `/api/campanias/{id}` | Campaña específica |
 
---
### IMC
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/imc/{idMiembro}` | Historial IMC con clasificación |
| POST | `/api/imc` | Registrar peso y altura |
| GET | `/api/imc/carnet/{idMiembro}` | Carnets escaneados |
| POST | `/api/imc/carnet` | Registrar carnet de clínica |
| DELETE | `/api/imc/carnet/{id}` | Eliminar carnet |
 
```json
// POST /api/imc — altura en metros, peso en kg
{
  "idMiembro": 1,
  "peso": 70.5,
  "altura": 1.75,
  "fecha": "2026-05-07"
}
 
// Response — Oracle calcula el resultado automáticamente
{
  "exito": true,
  "data": {
    "idImc": 1,
    "resultado": 23.02,
    "clasificacion": "Normal"
  }
}
```
 
**Clasificaciones:** `Bajo peso` \| `Normal` \| `Sobrepeso` \| `Obesidad`
 
---

### Alergias
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/alergias` | Catálogo de alergias |
| GET | `/api/alergias/miembro/{idMiembro}` | Alergias de un miembro |
| POST | `/api/alergias/miembro/{idMiembro}` | Asignar alergia |
| DELETE | `/api/alergias/miembro/{idMiembro}/{idAlergia}` | Quitar alergia |
 
```json
// POST /api/alergias/miembro/1
{
  "idAlergia": 21
}
```
 
---

### Certificado
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/certificado/miembro/{idMiembro}` | Certificados del miembro |
| POST | `/api/certificado` | Generar certificado QR |
| DELETE | `/api/certificado/{id}` | Eliminar certificado |
 
---
### Dispositivo (Push Notifications)
 
| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/dispositivousuario` | Dispositivos registrados |
| POST | `/api/dispositivousuario` | Registrar token FCM |
| DELETE | `/api/dispositivousuario/{id}` | Desactivar dispositivo |
 
```json
// POST /api/dispositivousuario — llamar al iniciar sesión
{
  "tokenPush": "fcm-token-del-dispositivo",
  "plataforma": "android"
}
```
 
---
 
## Ejemplos en Java/Android
 
### Configuración de Retrofit
 
```java
// ApiClient.java
public class ApiClient {
    private static final String BASE_URL = "http://IP_TAILSCALE:5001/";
    private static Retrofit retrofit;
 
    public static Retrofit getInstance() {
        if (retrofit == null) {
            OkHttpClient client = new OkHttpClient.Builder()
                .addInterceptor(chain -> {
                    String token = SessionManager.getToken();
                    Request request = chain.request().newBuilder()
                        .addHeader("Authorization", "Bearer " + token)
                        .addHeader("Content-Type", "application/json")
                        .build();
                    return chain.proceed(request);
                })
                .build();
 
            retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .client(client)
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        }
        return retrofit;
    }
}
```
 
### Interface de endpoints
 
```java
// ApiService.java
public interface ApiService {
 
    // Auth
    @POST("api/auth/login")
    Call<RespuestaDto<AuthResponseDto>> login(@Body LoginDto body);
 
    @POST("api/auth/registro")
    Call<RespuestaDto<Object>> registro(@Body RegistroDto body);
 
    @POST("api/auth/google")
    Call<RespuestaDto<AuthResponseDto>> loginGoogle(@Body GoogleAuthDto body);
 
    // Miembros
    @GET("api/miembros")
    Call<RespuestaDto<List<MiembroDto>>> getMiembros();
 
    @GET("api/miembros/{id}/perfil")
    Call<RespuestaDto<PerfilMiembroDto>> getPerfilMiembro(@Path("id") int id);
 
    @POST("api/miembros")
    Call<RespuestaDto<Object>> crearMiembro(@Body CrearMiembroDto body);
 
    // Historial
    @GET("api/historial/{idMiembro}")
    Call<RespuestaDto<List<HistorialDto>>> getHistorial(@Path("idMiembro") int idMiembro);
 
    @POST("api/historial")
    Call<RespuestaDto<Object>> registrarVacunacion(@Body RegistrarVacunacionDto body);
 
    // Recordatorios
    @GET("api/recordatorios/proximos")
    Call<RespuestaDto<List<RecordatorioDto>>> getProximos(@Query("dias") int dias);
 
    @PATCH("api/recordatorios/{id}/estado")
    Call<RespuestaDto<Object>> actualizarEstado(
        @Path("id") int id,
        @Body ActualizarEstadoDto body
    );
 
    // Centros
    @GET("api/centros/cercanos")
    Call<RespuestaDto<List<CentroDto>>> getCentrosCercanos(
        @Query("latitud") double latitud,
        @Query("longitud") double longitud,
        @Query("radioKm") double radioKm
    );
 
    // IMC
    @POST("api/imc")
    Call<RespuestaDto<ImcResultDto>> registrarImc(@Body RegistrarImcDto body);
 
    // Alergias
    @GET("api/alergias")
    Call<RespuestaDto<List<AlergiaDto>>> getAlergias();
 
    @POST("api/alergias/miembro/{idMiembro}")
    Call<RespuestaDto<Object>> asignarAlergia(
        @Path("idMiembro") int idMiembro,
        @Body AsignarAlergiaDto body
    );
}
```
 
### Ejemplo de uso en una Activity
 
```java
// LoginActivity.java
ApiService api = ApiClient.getInstance().create(ApiService.class);
 
LoginDto loginDto = new LoginDto("correo@ejemplo.com", "password123");
 
api.login(loginDto).enqueue(new Callback<RespuestaDto<AuthResponseDto>>() {
    @Override
    public void onResponse(Call<RespuestaDto<AuthResponseDto>> call,
                           Response<RespuestaDto<AuthResponseDto>> response) {
        if (response.isSuccessful() && response.body().isExito()) {
            // Guardar token
            String token = response.body().getData().getToken();
            SessionManager.saveToken(token);
 
            // Ir a MainActivity
            startActivity(new Intent(LoginActivity.this, MainActivity.class));
        } else {
            Toast.makeText(LoginActivity.this,
                response.body().getMensaje(), Toast.LENGTH_SHORT).show();
        }
    }
 
    @Override
    public void onFailure(Call<RespuestaDto<AuthResponseDto>> call, Throwable t) {
        Toast.makeText(LoginActivity.this,
            "Error de conexión", Toast.LENGTH_SHORT).show();
    }
});
```
 
### SessionManager para el token
 
```java
// SessionManager.java
public class SessionManager {
    private static final String KEY_TOKEN    = "jwt_token";
    private static final String KEY_USUARIO  = "id_usuario";
    private static SharedPreferences prefs;
 
    public static void init(Context context) {
        prefs = context.getSharedPreferences("ApiVacunas", Context.MODE_PRIVATE);
    }
 
    public static void saveToken(String token) {
        prefs.edit().putString(KEY_TOKEN, token).apply();
    }
 
    public static String getToken() {
        return prefs.getString(KEY_TOKEN, "");
    }
 
    public static void saveIdUsuario(int id) {
        prefs.edit().putInt(KEY_USUARIO, id).apply();
    }
 
    public static int getIdUsuario() {
        return prefs.getInt(KEY_USUARIO, -1);
    }
 
    public static void cerrarSesion() {
        prefs.edit().clear().apply();
    }
}
```
 
---
 
## Códigos de respuesta
 
| Código | Descripción |
|--------|-------------|
| 200 | OK — operación exitosa |
| 201 | Created — recurso creado |
| 400 | Bad Request — datos inválidos |
| 401 | Unauthorized — token inválido o expirado |
| 403 | Forbidden — sin permisos (requiere admin) |
| 404 | Not Found — recurso no encontrado |
| 409 | Conflict — recurso duplicado |
| 500 | Internal Server Error — error del servidor |
 
---
 
## Estructura de respuesta
 
Todos los endpoints devuelven este formato:
 
```json
{
  "exito": true,
  "mensaje": "Descripción del resultado",
  "data": { }
}
```
 
---
 
## Contacto

El grupo :>