# Instrucciones para Publicar Grav-NetCore en NuGet.org

## Paso 1: Crear cuenta en NuGet.org

1. Ve a https://www.nuget.org/
2. Haz clic en "Sign in" y crea una cuenta
3. Verifica tu email

## Paso 2: Crear API Key

1. Ve a https://www.nuget.org/account/apikeys
2. Haz clic en "Create"
3. Configura:
   - **Key Name**: Grav-NetCore-Publish
   - **Package owner**: Tu cuenta
   - **Scopes**: Push new packages and package versions
   - **Glob Pattern**: Grav-NetCore
   - **Expiration**: 365 días (o el que prefieras)
4. Copia la API Key generada (se muestra solo una vez)

## Paso 3: Publicar el paquete

Ejecuta el siguiente comando desde la terminal (reemplaza YOUR_API_KEY con tu API key):

```bash
cd /Users/enriquecena/Documents/Desarrollo/Invitafy/Grav-NetCore/Grav-NetCore

dotnet nuget push nupkg/Grav-NetCore.1.0.0.nupkg \
  --api-key YOUR_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Paso 4: Verificar publicación

1. Espera unos minutos (puede tardar en indexarse)
2. Ve a https://www.nuget.org/packages/Grav-NetCore
3. Verifica que aparezca la versión 1.0.0

## Para publicar nuevas versiones

1. Actualiza el número de versión en `Grav-NetCore.csproj`:
   ```xml
   <Version>1.0.1</Version>
   ```

2. Haz los cambios necesarios en el código

3. Ejecuta:
   ```bash
   dotnet pack --configuration Release --output ./nupkg
   dotnet nuget push nupkg/Grav-NetCore.1.0.1.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```

## Versionamiento Semántico (SemVer)

- **1.0.0** → Versión inicial
- **1.0.X** → Patches (bug fixes, no rompen compatibilidad)
- **1.X.0** → Minor (nuevas features, no rompen compatibilidad)
- **X.0.0** → Major (cambios que rompen compatibilidad)

## Consideraciones

- Una vez publicado un paquete con cierta versión, NO se puede modificar
- Solo puedes "unlist" (ocultar) versiones, pero no eliminarlas
- Asegúrate de probar bien antes de publicar
- Considera usar versiones pre-release (1.0.0-beta) para pruebas

## Repositorio de Git (Opcional pero recomendado)

1. Crea un repositorio en GitHub llamado `Grav-NetCore`
2. Sube el código:
   ```bash
   cd /Users/enriquecena/Documents/Desarrollo/Invitafy/Grav-NetCore
   git init
   git add .
   git commit -m "Initial commit - Grav-NetCore v1.0.0"
   git branch -M main
   git remote add origin https://github.com/TU_USUARIO/Grav-NetCore.git
   git push -u origin main
   ```
3. Actualiza las URLs en el `.csproj` con la URL real del repositorio

## Archivos creados

- `/Users/enriquecena/Documents/Desarrollo/Invitafy/Grav-NetCore/Grav-NetCore/nupkg/Grav-NetCore.1.0.0.nupkg` - Paquete principal
- `/Users/enriquecena/Documents/Desarrollo/Invitafy/Grav-NetCore/Grav-NetCore/nupkg/Grav-NetCore.1.0.0.snupkg` - Símbolos de debug
