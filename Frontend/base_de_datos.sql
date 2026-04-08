CREATE DATABASE IF NOT EXISTS CarbolsasDB;
USE CarbolsasDB;

-- 1. Tabla de Usuarios (Para Control de Permisos RF-05)
CREATE TABLE Usuarios (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL,
    correo VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    rol ENUM('Administrador', 'Empleado') NOT NULL,
    primer_inicio BOOLEAN DEFAULT TRUE -- Para el Tutorial RF-06
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
CREATE TABLE TintasBase (
    id_tinta INT AUTO_INCREMENT PRIMARY KEY,
    nombre_tinta VARCHAR(100) NOT NULL,
    codigo_hex VARCHAR(7),
    stock_actual DECIMAL(10,2) NOT NULL, -- Volumen en ml o gr
    stock_minimo_alerta DECIMAL(10,2) NOT NULL -- Para Correo de Alerta RF-08
);

-- 4. Tabla de Fórmulas (Algoritmo de Mezcla RF-02)
CREATE TABLE Formulas (
    id_formula INT AUTO_INCREMENT PRIMARY KEY,
    id_empresa INT NULL,
    nombre_color VARCHAR(100) NOT NULL,
    descripcion_color TEXT
    FOREIGN KEY (id_empresa) REFERENCES Empresas(id_empresa)
);

-- 5. Detalle de Fórmulas (Relación muchos a muchos entre Tintas y Fórmulas)
CREATE TABLE DetalleFormulas (
    id_detalle INT AUTO_INCREMENT PRIMARY KEY,
    id_formula INT NOT NULL,
    id_tinta INT NOT NULL,
    porcentaje DECIMAL(5,2) NOT NULL, -- Porcentaje de la mezcla
    FOREIGN KEY (id_formula) REFERENCES Formulas(id_formula),
    FOREIGN KEY (id_tinta) REFERENCES TintasBase(id_tinta)
);

-- 6. Tabla de Logotipos (Asociación RF-03)
CREATE TABLE Logotipos (
    id_logotipo INT AUTO_INCREMENT PRIMARY KEY,
    id_empresa INT NOT NULL,
    nombre_logo VARCHAR(100),
    url_imagen VARCHAR(255), -- Ruta de la imagen del logo
    id_formula INT, -- Color principal asociado
    FOREIGN KEY (id_empresa) REFERENCES Empresas(id_empresa),
    FOREIGN KEY (id_formula) REFERENCES Formulas(id_formula)
);

-- 7. Historial de Órdenes (Control de Inventario RF-04)
CREATE TABLE OrdenesImpresion (
    id_orden INT AUTO_INCREMENT PRIMARY KEY,
    id_usuario INT NOT NULL,
    id_logotipo INT NOT NULL,
    fecha_orden TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    volumen_total DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario),
    FOREIGN KEY (id_logotipo) REFERENCES Logotipos(id_logotipo)
);