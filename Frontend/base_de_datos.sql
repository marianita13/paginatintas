CREATE DATABASE IF NOT EXISTS CarbolsasDB;
USE CarbolsasDB;

CREATE TABLE Rol(
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);

-- 1. Tabla de Usuarios (Para Control de Permisos RF-05)
CREATE TABLE Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(60) NOT NULL,
    Correo VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    IdRol INT NOT NULL,
    PrimerInicio BOOLEAN DEFAULT TRUE, -- Para el Tutorial RF-06
    Activo BOOLEAN DEFAULT TRUE,
	FOREIGN KEY (IdRol) REFERENCES Rol(Id)
);

-- 2. Tabla de Empresas (Organización de Empresas RF-03)
CREATE TABLE Empresa (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreComercial VARCHAR(150) NOT NULL,
    Nit VARCHAR(20) UNIQUE,
    Telefono VARCHAR(20),
    CorreoContacto VARCHAR(100),
    FechaRegistro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Tabla de Tintas Base (Gestión de Tintas RF-01 y RF-08)
CREATE TABLE TintaBase (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreTinta VARCHAR(100) NOT NULL,
    CodigoHex VARCHAR(7),
    StockActual DECIMAL(10,2) NOT NULL, -- Volumen en ml o gr
    StockMinimo_alerta DECIMAL(10,2) NOT NULL -- Para Correo de Alerta RF-08
);

-- 4. Tabla de Fórmulas (Algoritmo de Mezcla RF-02)
CREATE TABLE Formula (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdEmpresa INT NULL,
    NombreColor VARCHAR(50) NOT NULL,
    FOREIGN KEY (IdEmpresa) REFERENCES Empresa(Id)
);

-- 5. Detalle de Fórmulas (Relación muchos a muchos entre Tintas y Fórmulas)
CREATE TABLE DetalleFormula (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdFormula INT NOT NULL,
    IdTinta INT NOT NULL,
    Porcentaje DECIMAL(5,2) NOT NULL, -- Porcentaje de la mezcla
    FOREIGN KEY (IdFormula) REFERENCES Formula(Id),
    FOREIGN KEY (IdTinta) REFERENCES TintaBase(Id)
);

-- 6. Tabla de Logotipos (Asociación RF-03)
CREATE TABLE Logotipo (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreLogo VARCHAR(100),
    UrlImagen VARCHAR(255), -- Ruta de la imagen del logo
    IdEmpresa INT, -- Empresa asociada al logo
    FOREIGN KEY (IdEmpresa) REFERENCES Empresa(Id)
);

-- 6b. Nueva tabla: Colores por Logotipo
CREATE TABLE LogotipoColor (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdLogotipo INT NOT NULL,
    IdFormula INT NOT NULL,
    EsColorPrincipal BOOLEAN DEFAULT FALSE, -- Para saber cuál es el color dominante
    FOREIGN KEY (IdLogotipo) REFERENCES Logotipo(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdFormula) REFERENCES Formula(Id)
);

-- 7. Historial de Órdenes (Control de Inventario RF-04)
CREATE TABLE OrdenImpresion (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdUsuario INT NOT NULL,
    IdLogotipo INT NOT NULL,
    FechaOrden TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    VolumenTotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id),
    FOREIGN KEY (IdLogotipo) REFERENCES Logotipo(Id)
);