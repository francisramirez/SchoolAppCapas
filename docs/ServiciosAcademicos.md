# Inscripciones modalidades y calificaciones

Se agregan objetos `Base`, `Add`, `Update`, `Remove` y `Model`, repositorios e
interfaces, y servicios e interfaces para `CourseEnrollment`, `EnrollmentStatus`,
`OnlineCourse`, `OnsiteCourse` y `StudentGrade`. Se conserva el estilo de métodos
`async` con validaciones y `try/catch` explícitos, `OperationResult` y logging.
Los repositorios exponen CRUD con borrado lógico. Los servicios exponen registro,
actualización y consulta, como `DepartmentServices`. No exponen eliminación ni un
proceso de retiro académico.

## Requisitos aplicables y límites

Fuentes: `BRD_SchoolApp_SGCE.docx` y `SRS_SchoolApp_SGCE.docx`, versión 1.0.

| Referencia | Implementación y alcance |
| --- | --- |
| RF-016, RF-020 | Registro y actualización de inscripciones con estudiante, curso y estado existentes. Un estado debe estar habilitado (`IsActive`) y no eliminado para asignarlo. |
| RF-018 | Consulta de inscripciones por estudiante mediante `GetByStudentIdAsync`. |
| RN-005, RNF-004, RNF-005 | El modelo actual solo identifica curso y estudiante. Se rechaza una segunda inscripción no eliminada para esa pareja. Las comprobaciones y escrituras se realizan en transacciones serializables para evitar carreras dentro del repositorio en SQL Server. Esto no equivale al control de duplicidad por oferta del SRS. |
| RN-008, RF-021 | La nota debe corresponder exactamente al estudiante y curso de una inscripción no eliminada. Se bloquea cambiar esos datos de la inscripción cuando hay nota registrada, y eliminar la inscripción con nota no eliminada. |
| RN-009, RF-022 | Escala institucional configurable. Si no está definida o es inválida se rechaza el registro y modificación de notas. No se fija una escala por defecto. |
| RF-023 | Consulta de notas por estudiante mediante `GetByStudentIdAsync`. |
| RN-011, RF-028 | Autor y fechas UTC asignadas por los servicios; auditoría persistida con la entidad. Las altas y modificaciones registran el resultado y correlación. El logging requiere configurar almacenamiento y retención en el despliegue. |
| RNF-003, RNF-008 | Errores controlados y sin mensajes internos del proveedor de base de datos. Se validan entradas antes de persistir. |
| RNF-010 | Pruebas de servicios y repositorios usando SQLite en memoria, más pruebas de resolución de dependencias. No sustituyen las pruebas de concurrencia con SQL Server. |

Las modalidades no tienen reglas específicas en esos documentos. Sus validaciones
técnicas son: curso existente y no eliminado, una configuración por modalidad y
curso, URL absoluta HTTP/HTTPS para cursos en línea, ubicación, días y hora para
cursos presenciales. No se presupone exclusión entre modalidades ni se inventa
un vocabulario de días de clase.

Se normalizan códigos de estado recortando espacios y convirtiendo a mayúsculas.
No se permite repetir un código entre estados no eliminados. No se puede eliminar
un estado utilizado en inscripciones no eliminadas. Esta es una validación de
integridad del catálogo, no una lista institucional definitiva de estados.

## Requisitos que necesitan ampliar el modelo

`CourseEnrollment` aún no contiene `CourseOfferingId` ni período académico.
`Student` no tiene un estado académico activo/inactivo. `EnrollmentStatus.IsActive`
habilita un elemento del catálogo; no determina si una inscripción está activa,
retirada o completada. No se asigna un significado institucional a los códigos.

Por ello quedan pendientes RN-003 (estado académico del estudiante), RN-004
(oferta abierta), RN-006 (cupo), RN-007 (plazos de retiro), el alcance completo de
RN-005 por oferta y RF-024 (publicación autorizada de notas). La verificación
actual de estudiante solo garantiza existencia y ausencia de borrado lógico.
Actualizar un estado es una operación de mantenimiento; no implementa ni debe
utilizarse como el caso de uso de retiro regulado por período.

El esquema actual permite una nota por `EnrollmentId`. Una evaluación múltiple
requeriría otra clave y modelo, decisión que el SRS mantiene pendiente. Los
registros con claves compartidas (`OnlineCourse`, `OnsiteCourse`, `StudentGrade`)
no pueden volver a insertarse con la misma clave tras un borrado lógico. Una
restauración explícita debe diseñarse si se necesita esa funcionalidad.

La arquitectura BL/DAL sigue siendo la solicitada. La API, autorización por roles,
notificaciones, publicación y migración a Clean Architecture no se implementan
en este cambio. Los usuarios de auditoría deben proceder del contexto autenticado
cuando se agregue la API; hoy se reciben en los objetos de entrada.

## Configuración de calificaciones

`GradeScaleOptions` se registra en BL y Web lo enlaza con la sección `GradeScale`.
La institución debe configurar `GradeScale:Minimum` y `GradeScale:Maximum`, o las
variables `GradeScale__Minimum` y `GradeScale__Maximum`. El mínimo debe ser menor
que el máximo y ambos extremos se admiten. Las pruebas usan 0 a 100 como datos
de prueba; no se establece ese rango como política del sistema.

Las notas se almacenan como `decimal(18,4)` y el servicio rechaza valores con más
de cuatro decimales para evitar redondeos silenciosos.

## Base de datos

`SchoolDbContext` incorpora los cinco conjuntos, claves y relaciones. Las claves
de modalidades y notas no son autogeneradas: se reutilizan `CourseId` y
`EnrollmentId`, respectivamente. Los borrados físicos en las relaciones nuevas
se restringen.

`EnrollmentStatus`, `OnlineCourse`, `OnsiteCourse` y `StudentGrade` heredan ahora
de `AuditEntity`. Los campos de auditoría existentes de `EnrollmentStatus` se
conservan por herencia; se incorporan los de borrado lógico. Esto modifica el
esquema y requiere preparar y aplicar una migración de EF Core acorde con la
base existente antes de ejecutar estos módulos. `EnsureCreated` no actualiza
tablas existentes. No se aplicaron cambios sobre una base real.

## Verificación

```powershell
dotnet build SchoolApp.sln
dotnet test SchoolApp.BL.Tests/SchoolApp.BL.Tests.csproj
```

Las pruebas relacionales están en `AcademicServicesTests.cs`. La concurrencia y
los bloqueos serializables deben verificarse adicionalmente en SQL Server.
