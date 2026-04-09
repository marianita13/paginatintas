CREATE DATABASE IF NOT EXISTS CarbolsasDB;
USE CarbolsasDB;

-- 1. Tabla de Usuarios (Para Control de Permisos RF-05)
CREATE TABLE Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(60) NOT NULL,
    Correo VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(20) NOT NULL,
    Rol ENUM('Administrador', 'Empleado') NOT NULL,
    PrimerInicio BOOLEAN DEFAULT TRUE -- Para el Tutorial RF-06
);

-- 2. Tabla de Empresas (Organización de Empresas RF-03)
CREATE TABLE Empresas (
    id_empresa INT AUTO_INCREMENT PRIMARY KEY,
    nombre_comercial VARCHAR(150) NOT NULL,
    nit VARCHAR(20) UNIQUE,
    telefono VARCHAR(20),
    correo_contacto VARCHAR(100),
    fecha_registro TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Tabla de Tintas Base (Gestión de Tintas RF-01 y RF-08)
CREATE TABLE TintaBase (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    nombre_tinta VARCHAR(100) NOT NULL,
    codigo_hex VARCHAR(7),
    stock_actual DECIMAL(10,2) NOT NULL, -- Volumen en ml o gr
    stock_minimo_alerta DECIMAL(10,2) NOT NULL -- Para Correo de Alerta RF-08
);

-- 4. Tabla de Fórmulas (Algoritmo de Mezcla RF-02)
CREATE TABLE Formula (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdEmpresa INT NULL,
    nombreColor VARCHAR(50) NOT NULL,
    FOREIGN KEY (id_empresa) REFERENCES Empresas(id_empresa)
);

-- 5. Detalle de Fórmulas (Relación muchos a muchos entre Tintas y Fórmulas)
CREATE TABLE DetalleFormula (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdFormula INT NOT NULL,
    IdTinta INT NOT NULL,
    Porcentaje DECIMAL(5,2) NOT NULL, -- Porcentaje de la mezcla
    FOREIGN KEY (IdFormula) REFERENCES Formula(IdFormula),
    FOREIGN KEY (IdTinta) REFERENCES TintaBase(IdTinta)
);

-- 6. Tabla de Logotipos (Asociación RF-03)
CREATE TABLE Logotipo (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    NombreLogo VARCHAR(100),
    UrlImagen VARCHAR(255), -- Ruta de la imagen del logo
    IdFormula INT, -- Color principal asociado
    IdEmpresa INT, -- Empresa asociada al logo
    FOREIGN KEY (IdEmpresa) REFERENCES Empresa(IdEmpresa),
    FOREIGN KEY (IdFormula) REFERENCES Formula(IdFormula)
);

-- 7. Historial de Órdenes (Control de Inventario RF-04)
CREATE TABLE OrdenImpresion (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    IdUsuario INT NOT NULL,
    IdLogotipo INT NOT NULL,
    FechaOrden TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    VolumenTotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    FOREIGN KEY (IdLogotipo) REFERENCES Logotipo(IdLogotipo)
);

CREATE TABLE Rol(
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO Roles (nombre) VALUES ('Administrador'), ('Empleado');