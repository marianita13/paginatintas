@echo off
setlocal enabledelayedexpansion

REM Pregunta al usuario por el número de entidades
set /p "num_entidades=Introduce el número de entidades: "

REM Ciclo para crear las entidades
set "count=0"
:loop
if %count% lss %num_entidades% (
    set /p "nombre_entidad=Introduce el nombre de la entidad %count%: "

    REM Crea la entidad en Core\Entities
    echo.>Domain\Entities\!nombre_entidad!.cs

    REM Crea la interfaz en Core\Interfaces
    echo.>Domain\Interfaces\I!nombre_entidad!.cs

    REM Crea el controlador en API\Controllers
    echo.>API\Controllers\!nombre_entidad!Controller.cs

    REM Crea el Dto en API\Dtos
    echo.>API\Dtos\!nombre_entidad!Dto.cs

    REM Crea el Repository en API\Repository
    echo.>Application\Repository\!nombre_entidad!Repository.cs

    REM Crea la Configuracion en Persistencia\Data\Configuration
    echo.>Persistence\Data\Configuration\!nombre_entidad!Configuration.cs

    set /a "count+=1"
    goto :loop
)