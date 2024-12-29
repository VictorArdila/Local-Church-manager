--Crea base de datos
create database LocalChurch
--Comando para eliminar(drop database YouGym)
use LocalChurch

create table USUARIO(
Id varchar(12) primary key not null,
Codigo_Usuario nvarchar(10) not null,
Tipo_De_Id char(2)not null,
Nombres varchar(30) not null,
Apellidos varchar(30) not null,
Fecha_De_Nacimiento datetime not null,
Edad int,
Sexo varchar(5),
Direccion_Domicilio text,
Telefono varchar(15),
Rol varchar(15),
Correo varchar(100),
NombreUsuario nvarchar(50),
Contrase�a nvarchar(50)
);
create table SOFTWARE(
Nombre_De_Software varchar(30) primary key not null,
Fecha_De_Instalacion datetime not null,
Fecha_De_Expiracion varchar(12) not null,
Hora_De_Expiracion varchar(15) not null,
Fecha_De_Activacion varchar(12) not null,
Hora_De_Activacion varchar(15) not null,
Estado_De_Licencia varchar(10) not null,
);
create table LICENCIA(
Licencia nvarchar(30) not null
);
create table IGLESIA(
Id_Iglesia nvarchar(6) primary key not null,
Nombre_De_Iglesia nvarchar(100)not null,
NIT nvarchar(50)not null,
Numero_De_Iglesia text,
PBX nvarchar(30),
Direccion nvarchar(100),
Telefono varchar(15)
);
create table CONTACTO(
IdContacto nvarchar(15) primary key not null,
Nombre varchar(40) not null,
Apellido varchar(40) not null,
TelefonoContacto varchar(15),
TelefonoWhatsapp nvarchar(15),
Oficio nvarchar(40),
EstadoCivil varchar(20),
NumeroHijos varchar(5),
NombreConyugue varchar(30)
);
create table MIEMBRO(
Folio nvarchar(5) primary key not null,
ImagenPerfil image,
IdContacto nvarchar(15) not null,
Nombre varchar(40) not null,
Apellido varchar(40) not null,
TipoDoc varchar(3) not null,
NumeroDoc varchar(12) not null,
FechaDeNacimiento datetime not null,
Edad int not null,
Genero varchar(10) not null,
Oficio varchar(70) not null,
Direccion nvarchar(70) not null,
Telefono varchar(12) not null,
ParentezcoPadre varchar(70) not null,
ParentezcoMadre varchar(70) not null,
EstadoCivil varchar(20) not null,
NumeroHijos int not null,
NombreConyugue varchar(40) not null,
Bautizado varchar(3) not null,
FechaDeBautizmo datetime not null,
LugarBautizmo varchar(100) not null,
PastorOficiante varchar(40) not null,
Sellado varchar(3) not null,
SelladoRecuerdo varchar(20) not null,
FechaSellado datetime not null,
TiempoConversion int not null,
TiempoPromesa int not null,
IglesiaProcedente varchar(30) not null,
PastorAsistente varchar(40) not null,
CargosDesempe�ados varchar(400) not null,
Acto varchar(12) not null,
FechaCorreccion datetime not null,
TiempoCorreccion int not null,
Motivo varchar(700) not null,
Membresia varchar(12) not null,
LugarTraslado varchar(50) not null,
Observaciones varchar(400) not null
);
create table SIMPATIZANTE(
NumeroDoc varchar(12) primary key not null,
ImagenPerfil image,
IdContacto nvarchar(15) not null,
Nombre varchar(40) not null,
Apellido varchar(40) not null,
TipoDoc varchar(3) not null,
FechaDeNacimiento datetime not null,
Edad int not null,
Genero varchar(10) not null,
Oficio varchar(70) not null,
Direccion nvarchar(70) not null,
Telefono varchar(12) not null,
);
create table DIRECTIVA(
IdDirectiva nvarchar(15) primary key not null,
Nombre varchar(70) not null,
Cargo varchar(50) not null,
Comite varchar(40) not null,
Vigencia varchar(10) not null,
Observacion nvarchar(1000) not null,
);
create table REUNION(
NumeroActa nvarchar(12) primary key not null,
FechaDeReunion datetime not null,
LugarDeReunion varchar(70) not null,
OrdenDelDia varchar(500) not null,
TextoActa varchar(5000) not null,
);
create table APUNTE(
IdNota nvarchar(12) primary key not null,
Titulo varchar(70) not null,
Nota varchar(600) not null,
);
--Tesoreria
create table INGRESO(
CodigoComprobante nvarchar(10) primary key not null,
FechaDeIngreso datetime not null,
Comite varchar(70) not null,
Concepto varchar(150) not null,
Valor int not null,
Detalle varchar(250) not null,
);
create table EGRESO(
CodigoComprobante nvarchar(10) primary key not null,
FechaDeEgreso datetime not null,
Comite varchar(70) not null,
Concepto varchar(150) not null,
Valor int not null,
Detalle varchar(250) not null,
);
create table ENVIABLE(
Id nvarchar(15) primary key not null,
FechaDeEnvio datetime not null,
Comite varchar(70) not null,
Concepto varchar(150) not null,
Valor int not null,
Detalle varchar(250) not null,
);
create table LIQUIDACION(
Id nvarchar(15) primary key not null,
FechaDeLiquidacion datetime not null,
Valor int not null,
Detalle varchar(250) not null,
Estado varchar(22) not null
);
create table PRESUPUESTO(
Id int primary key not null,
AnoPresupuesto varchar(6) not null,
InicioIntervalo varchar(15) not null,
FinIntervalo varchar(15) not null,
Comite varchar(30) not null,
Ofrenda int not null,
Actividad int not null,
Voto int not null,
OtroConcepto varchar(30) not null,
ValorOtroConcepto int not null,
TotalEgresos int not null,
TotalPresupuesto int not null
);
