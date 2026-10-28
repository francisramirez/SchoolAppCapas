# Servicio de departamentos

`SchoolApp.BL.Services.DepartmentServices` implementa `IDepartmentServices` y recibe
`IDepartmentRepository` e `ILogger<DepartmentServices>` por constructor. Se registra
con `AddBusinessLogic()` y se habilita en el inicio de `SchoolApp.Web`.

## Trazabilidad

Fuentes: `BRD_SchoolApp_SGCE.docx` y `SRS_SchoolApp_SGCE.docx`, versión 1.0.

| Requisito | Implementación |
| --- | --- |
| BN-002, CU-002, RF-005 | `AddAsync` registra el departamento mediante el repositorio. |
| RF-006 | `GetAllAsync`, `GetByIdAsync` y `UpdateAsync` consultan y actualizan departamentos. Las consultas entregan `DepartmentModel`. |
| RN-011, RF-028 | Se exige usuario positivo para altas y modificaciones; las fechas UTC se asignan en el servidor. Se registran operación, usuario, fecha, resultado y correlación. El repositorio conserva los campos de creación al actualizar. |
| RNF-003 | Se validan objetos nulos, identificadores y campos; los errores se entregan mediante `OperationResult`. |
| RNF-007, RNF-008 | Las trazas incluyen correlación y tipo de excepción, sin contenido de solicitudes ni mensajes internos del proveedor de persistencia. |
| RNF-010 | `SchoolApp.BL.Tests` verifica validaciones, auditoría, consultas, fallos y resolución del servicio por inyección de dependencias. |

## Validaciones y decisiones

Se mantienen las validaciones existentes del repositorio: nombre obligatorio,
presupuesto no negativo, fecha de inicio distinta de su valor predeterminado y,
si se proporciona administrador, identificador positivo. Los nombres se recortan
antes de persistir. La identidad del usuario se recibe en los objetos actuales;
una futura API deberá obtenerla del contexto autenticado para evitar suplantación.

Los documentos no definen unicidad de nombres, límite máximo de presupuesto,
restricciones sobre fechas futuras ni obligación de asignar un administrador.
La matriz de trazabilidad del SRS deja las reglas específicas de CU-002 por validar.
Estas políticas no se agregan como requisitos confirmados.

La interfaz de negocio cubre registrar, consultar y actualizar, conforme a RF-005
y RF-006. No expone eliminación, que no está especificada para CU-002. El borrado
lógico ya existente en DAL se conserva.

## Límites de esta implementación

Se utiliza la arquitectura BL/DAL solicitada. No implementa la migración completa
a Domain/Application/Infrastructure/API del SRS (RNF-006), los contratos REST,
los códigos HTTP ni la autorización por roles. `OperationResult` aún no dispone
de un código de error que distinga automáticamente 404, 409 y 500.

Los campos de auditoría se guardan con la entidad en `SaveChangesAsync`; las trazas
de resultado se envían al proveedor de logging configurado. No se implementa un
almacén de auditoría permanente ni se garantiza una transacción conjunta entre
persistencia y logging. Un despliegue deberá configurar retención y consulta de
las trazas para completar la auditoría operacional.

Las pruebas usan un repositorio simulado; no sustituyen pruebas de integración
con SQL Server, autorización o contratos API.

## Verificación

```powershell
dotnet test SchoolApp.BL.Tests/SchoolApp.BL.Tests.csproj
dotnet build SchoolApp.sln
```
