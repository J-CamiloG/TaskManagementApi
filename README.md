# Task Management System - Backend API

Sistema de gestión de tareas desarrollado con .NET 8, Entity Framework Core y autenticación JWT.

---

##  Características

- ✅ API RESTful con .NET 8
- ✅ Autenticación JWT
- ✅ Base de datos SQL Server con Entity Framework Core
- ✅ Arquitectura por capas (Clean Architecture)
- ✅ Documentación con Swagger/OpenAPI
- ✅ AutoMapper para mapeo de DTOs
- ✅ Inyección de dependencias

---

##  Requisitos

- .NET 8 SDK
- Visual Studio 2022 o Visual Studio Code

---

## ⚙ Instalación y ejecución local

1. Clona este repositorio:
   ```bash
   git clone https://github.com/J-CamiloG/TaskManagementApi.git
    ```
2. Restaura las dependencias del proyecto
   ```bash
   dotnet restore
    ```
3. Crea un archivo .env en la raiz de la carpeta TaskManagement.API y configura las variables que fueron enviadas

4. Ejecuta el proyecto:
    ```bash
   dotnet run
    ```
5. Por defecto, la API estará disponible en: 
    ```bash
   http://localhost:5230
    ```
---


##  Conexión con el frontend
El frontend del proyecto está desplegado en Vercel:
🔗 https://task-managment-front.vercel.app

---
#  Importante: en el frontend, la URL http://localhost:5230 ya está configurada como valor por defecto en las variables de entorno.
Esto significa que si clonas y ejecutas este backend en tu máquina local, el frontend desplegado se podrá comunicar correctamente con él sin cambios adicionales. sin tu maquina no despliaga la app en ese puerto  entonces no coincidira con el del front.  sin embargo se configuro la app para que simepre corra en el puerto correcto, sin embargo para tener en cuanta. 

---

## Documentación
Una vez corras el proyecto localmente, puedes acceder a Swagger en:
http://localhost:5230/swagger

---
## Tecnologías utilizadas
- ✅ .NET 8

- ✅ Entity Framework Core

- ✅ AutoMapper

- ✅ FluentValidation

- ✅ JWT

- ✅ Swagger 

- ✅ Serilog

