using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UI.Properties;
using BLL;
using Entity;
using Cloud;
using Google.Cloud.Firestore;
using Microsoft.Win32;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using DocumentFormat.OpenXml.Spreadsheet;
using Ldb;

namespace UI
{
    public partial class FormMiembros : Form
    {
        TabPage tabPage;
        public readonly Validaciones validaciones;
        RutasTxtService rutasTxtService = new RutasTxtService();
        MiembroService miembroService;
        SimpatizanteService simpatizanteService;
        MemberMaps memberMaps;
        SympathizerMaps sympathizerMaps;
        ContactMaps contactMaps;
        List<Miembro> miembros;
        List<MemberData> memberData;
        List<SympathizerData> sympathizerData;
        Miembro miembro;
        Reporte reporte;
        Simpatizante simpatizante;
        List<Simpatizante> simpatizantes;
        ContactoService contactoService;
        List<Contacto> contactos;
        Contacto contacto;
        string id = "";
        string idContacto = "";
        string numeroWhatsapp = "";
        string folio = "";
        string nombres = "";
        string apellidos = "";
        byte[] imagenPicture = null;
        byte[] imagenPerfil = null;
        bool encontrado = false;
        bool encontradoNombre = false;
        bool encontradoApellido = false;
        string generoDelTipo="";
        public FormMiembros()
        {
            simpatizanteService = new SimpatizanteService(DbConfig.ConnectionString);
            contactoService = new ContactoService(DbConfig.ConnectionString);
            miembroService = new MiembroService(DbConfig.ConnectionString);
            contactMaps = new ContactMaps();
            memberMaps = new MemberMaps();
            reporte = new Reporte();
            InitializeComponent();
            Inicializar();
            validaciones = new Validaciones();
        }
        private void Inicializar()
        {
            CalcularFolio();
            ConsultarYLlenarGridDeMiembros();
            ConsultarYLlenarGridDeSimpatizante();
        }
        private Image ResizeImageToFitCell(DataGridViewImageCell cell, Image originalImage)
        {
            int cellWidth = cell.Size.Width - cell.Style.Padding.Horizontal;
            int cellHeight = cell.Size.Height - cell.Style.Padding.Vertical;

            float widthRatio = (float)cellWidth / originalImage.Width;
            float heightRatio = (float)cellHeight / originalImage.Height;
            float minRatio = Math.Min(widthRatio, heightRatio);

            int newWidth = (int)(originalImage.Width * minRatio);
            int newHeight = (int)(originalImage.Height * minRatio);

            return new Bitmap(originalImage, new Size(newWidth, newHeight));
        }
        private async void ConsultarYLlenarGridDeSimpatizante()
        {
            try
            {
                var db = FirebaseService.Database;
                var registros = new List<SympathizerData>();
                Google.Cloud.Firestore.Query membersQuery = db.Collection("SympathizerData").WhereEqualTo("Bautizado", "Si");
                QuerySnapshot snap = await membersQuery.GetSnapshotAsync();
                sympathizerData = snap.Documents.Select(docsnap => docsnap.ConvertTo<SympathizerData>()).ToList();
                var hombres = snap.Documents.Where(docsnap => docsnap.ConvertTo<SympathizerData>().Genero == "Masculino").Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                var mujeres = snap.Documents.Where(docsnap => docsnap.ConvertTo<SympathizerData>().Genero == "Femenino").Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                if (snap.Documents.Count > 0)
                {
                    textTotalHombres2.Text = hombres.Count.ToString();
                    textTotalMujeres2.Text = mujeres.Count.ToString();
                    textTotal2.Text = sympathizerData.Count.ToString();
                    dataGridSimpatizantes.DataSource = null;
                    dataGridSimpatizantes.DataSource = memberData;
                }
                else
                {
                    dataGridSimpatizantes.DataSource = null;
                    textTotal2.Text = "0";
                }
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaSimpatizanteRespuesta respuesta = new ConsultaSimpatizanteRespuesta();
                string tipo = comboGenero.Text;
                if (tipo == "Genero" || tipo == "Todos")
                {
                    textTotal2.Enabled = true;
                    textTotalHombres2.Enabled = true;
                    textTotalMujeres2.Enabled = true;
                    dataGridSimpatizantes.DataSource = null;
                    respuesta = simpatizanteService.ConsultarTodos();
                    simpatizantes = respuesta.Simpatizantes.ToList();
                    if (respuesta.Simpatizantes.Count != 0 && respuesta.Simpatizantes != null)
                    {
                        dataGridSimpatizantes.DataSource = respuesta.Simpatizantes;
                        Borrar.Visible = true;
                        textTotal2.Text = miembroService.Totalizar().Cuenta.ToString();
                        textTotalHombres2.Text = miembroService.TotalizarTipo("Masculino").Cuenta.ToString();
                        textTotalMujeres2.Text = miembroService.TotalizarTipo("Femenino").Cuenta.ToString();
                        foreach (DataGridViewRow row in dataGridMiembros.Rows)
                        {
                            DataGridViewImageCell cell = row.Cells["ImagenPerfil"] as DataGridViewImageCell;
                            if (cell != null)
                            {
                                byte[] imageBytes = cell.Value as byte[];
                                if (imageBytes != null)
                                {
                                    // Convierte los bytes en una imagen
                                    Image originalImage;
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        originalImage = Image.FromStream(ms);
                                    }

                                    // Redimensiona la imagen para que quepa en la celda
                                    Image resizedImage = ResizeImageToFitCell(cell, originalImage);

                                    // Asigna la imagen redimensionada a la celda
                                    cell.Value = resizedImage;
                                }
                            }
                        }
                    }
                }
            }
        }
        private async void ConsultarYLlenarGridDeMiembros()
        {
            try
            {
                var db = FirebaseService.Database;
                var registros = new List<MemberData>();
                Google.Cloud.Firestore.Query membersQuery = db.Collection("MembersData").WhereEqualTo("Bautizado", "Si");
                QuerySnapshot snap = await membersQuery.GetSnapshotAsync();
                registros = snap.Documents.Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                memberData = registros.Where(registro => registro.Bautizado == "Si").ToList();
                var hombres = snap.Documents.Where(docsnap => docsnap.ConvertTo<MemberData>().Genero == "Masculino").Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                var mujeres = snap.Documents.Where(docsnap => docsnap.ConvertTo<MemberData>().Genero == "Femenino").Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();

                if (snap.Documents.Count > 0)
                {
                    textTotalHombres.Text = hombres.Count.ToString();
                    textTotalMujeres.Text = mujeres.Count.ToString();
                    textTotal.Text = registros.Count.ToString();
                    dataGridMiembros.DataSource = null;
                    dataGridMiembros.DataSource = memberData;
                }
                else
                {
                    dataGridMiembros.DataSource = null;
                    textTotal.Text = "0";
                }
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch (Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaMiembroRespuesta respuesta = new ConsultaMiembroRespuesta();
                string tipo = comboGenero.Text;
                if (tipo == "Genero" || tipo == "Todos")
                {
                    textTotal.Enabled = true;
                    textTotalHombres.Enabled = true;
                    textTotalMujeres.Enabled = true;
                    dataGridMiembros.DataSource = null;
                    respuesta = miembroService.ConsultarTodos();
                    miembros = respuesta.Miembros.ToList();
                    if (respuesta.Miembros.Count != 0 && respuesta.Miembros != null)
                    {
                        dataGridMiembros.DataSource = respuesta.Miembros;
                        Borrar.Visible = true;
                        textTotal.Text = miembroService.Totalizar().Cuenta.ToString();
                        textTotalHombres.Text = miembroService.TotalizarTipo("Masculino").Cuenta.ToString();
                        textTotalMujeres.Text = miembroService.TotalizarTipo("Femenino").Cuenta.ToString();
                        foreach (DataGridViewRow row in dataGridMiembros.Rows)
                        {
                            DataGridViewImageCell cell = row.Cells["ImagenPerfil"] as DataGridViewImageCell;
                            if (cell != null)
                            {
                                byte[] imageBytes = cell.Value as byte[];
                                if (imageBytes != null)
                                {
                                    // Convierte los bytes en una imagen
                                    Image originalImage;
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        originalImage = Image.FromStream(ms);
                                    }

                                    // Redimensiona la imagen para que quepa en la celda
                                    Image resizedImage = ResizeImageToFitCell(cell, originalImage);

                                    // Asigna la imagen redimensionada a la celda
                                    cell.Value = resizedImage;
                                }
                            }
                        }
                    }
                }
            }
        }
        void LimpiarCampos()
        {
            textNombre.Text = "Nombre";
            comboTipoDocumento.Text = "CC";
            textNumeroDeDocumento.Text = "Numero de documento";
            dateFechaDeNacimiento.Value = DateTime.Now;
            textDireccion.Text = "Direccion";
            textTelefono.Text = "Telefono";
            using (MemoryStream ms = new MemoryStream())
            {
                picturePerfil.Image = Properties.Resources.User;
            }
            textNombreDelPadre.Text = "Nombre del padre";
            textNombreDeLaMadre.Text = "Nombre de la madre";
            dateFechaDeBautismo.Value = DateTime.Now;
            textTiempoDeConversion.Text = "0";
            dateFechaEspirituSanto.Value = DateTime.Now;
            textLugarBautizmo.Text = "Lugar bautizmo";
            comboPastorOficiante.Text = "Emiro Diaz";

            // Campos adicionales
            comboOficio.Text = "Oficio";
            comboEstadoCivil.Text = "Estado Civil";
            textNumeroDeHijos.Text = "0";
            textNombreDelConyugue.Text = "Nombre del conyuge";
            comboBautizado.Text = "Sí";
            textLugarBautizmo.Text = "Lugar bautismo";
            comboPastorOficiante.Text = "Nombre del pastor";
            comboSellado.Text = "Sí";
            comboRecuerda.Text = "Recuerdo";
            dateFechaEspirituSanto.Value = DateTime.Now;
            textTiempoDeConversion.Text = "0";
            textTiempoPromesa.Text = "0";
            textIglesiaProcedente.Text = "Iglesia procedente";
            comboPastorAsistente.Text = "Nombre del pastor asistente";
            textCargosDesempeñados.Text = "Cargos desempeñados";
            comboActoParaServir.Text = "Acto para servir";
            dateFechaDeCorreccion.Value = DateTime.Now;
            textTiempoCorreccion.Text = "0";
            comboMembresia.Text = "Membresía";
            textLugarDeTraslado.Text = "Lugar de traslado";
            textObservaciones.Text = "Observaciones";
        }
        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGestionarMiembros_Click(object sender, EventArgs e)
        {
            tabMiembros.SelectedIndex = 1;
        }
        private async void CalcularFolio()
        {
            try
            {
                var db = FirebaseService.Database;
                string numeroFolio = "";

                // Obtener el máximo número de comprobante directamente desde Firestore
                var memberQuery = db.Collection("MembersData").OrderByDescending("Folio");
                var snapshot = await memberQuery.GetSnapshotAsync();

                if (snapshot.Documents.Count > 0)
                {
                    var folioCurrent = snapshot.Documents.Count;
                    int numeroMayor = folioCurrent + 1;
                    numeroFolio = numeroMayor.ToString("0000");
                    textNumeroFolio.Text = numeroFolio;
                }
                else
                {
                    numeroFolio = "0001";
                    textNumeroFolio.Text = numeroFolio;
                }
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                ConsultaMiembroRespuesta respuesta = new ConsultaMiembroRespuesta();
                respuesta = miembroService.ConsultarTodos();
                miembros = respuesta.Miembros.ToList();
                if (respuesta.Miembros.Count != 0 && respuesta.Miembros != null)
                {
                    textTotal.Text = miembroService.Totalizar().Cuenta.ToString();
                    var totalFolio = Convert.ToInt32(textTotal.Text);
                    folio = (totalFolio + 1).ToString("0000");
                    textNumeroFolio.Text = folio;
                }
                else
                {
                    var totalFolio = 1;
                    folio = totalFolio.ToString("0000");
                    textNumeroFolio.Text = folio;
                }
            }
        }
        private void ExtraerNombreYApellido()
        {
            var nombreCompleto = textNombre.Text;
            // Dividir el nombre completo en palabras usando espacio como separador
            string[] palabras = nombreCompleto.Split(' ');

            if (palabras.Length >= 2)
            {
                // Al menos hay dos palabras (nombre y apellido)
                nombres = palabras[0];

                if (palabras.Length == 2)
                {
                    // Caso 1: Exactamente dos palabras, asignar la segunda al apellido
                    apellidos = palabras[1];
                }
                else if (palabras.Length == 3)
                {
                    // Caso 2: Tres palabras, asignar la segunda al nombre y la tercera al apellido
                    nombres = palabras[0];
                    apellidos = palabras[1] + " " + palabras[2];
                }
                else if (palabras.Length >= 4)
                {
                    // Caso 3: Cuatro o más palabras, asignar las dos primeras al nombre y las dos últimas al apellido
                    nombres = palabras[0] + " " + palabras[1];
                    apellidos = palabras[palabras.Length - 2] + " " + palabras[palabras.Length - 1];
                }

                // Ahora tienes los nombres y apellidos en las variables 'nombres' y 'apellidos'
                // Puedes usar estas variables como desees
            }
            else
            {
                string mensaje = "Nombre mal digitado, por favor digite su nombre completo";
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
        }
        private void GenerarIdContacto()
        {
            contacto = new Contacto();
            contacto.GenerarId();
            idContacto = contacto.IdContacto;
        }
        static string FormatearFecha(string fechaOriginal)
        {
            string fechaFormateada = "";
            if (fechaOriginal.Contains(" p. m."))
            {
                fechaFormateada = fechaOriginal.Replace(" p. m.", "");
                fechaFormateada = fechaFormateada + " PM";
            }
            else
            {
                if (fechaOriginal.Contains(" a. m."))
                {
                    fechaFormateada = fechaOriginal.Replace(" a. m.", "");
                    fechaFormateada = fechaFormateada + " AM";
                }
                else
                {
                    fechaFormateada = fechaOriginal; 
                }
            }
            return fechaFormateada;
        }
        private async void FiltroPorIdentificacion(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var membersQuery = db.Collection("MembersData");
                var members = new List<MemberData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await membersQuery.GetSnapshotAsync();
                members = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var membersFiltrados = members.Where(ingreso => ingreso.Folio == filtro).ToList();
                if (membersFiltrados.Any())
                {
                    var ingresoFiltrado = membersFiltrados.First(); // Obtener el primer elemento de la lista
                    encontrado = true;
                    idContacto = ingresoFiltrado.IdContacto;
                    textNumeroFolio.Text = ingresoFiltrado.Folio;
                    textNombre.Text = ingresoFiltrado.Nombre + " " + ingresoFiltrado.Apellido;
                    comboTipoDocumento.Text = ingresoFiltrado.TipoDoc;
                    textNumeroDeDocumento.Text = ingresoFiltrado.NumeroDoc;
                    string formatoFecha = "d/M/yyyy h:mm:ss:tt";
                    DateTime fechaConvertida;
                    string fechaNacimiento = FormatearFecha(ingresoFiltrado.FechaNacimiento);
                    if (DateTime.TryParseExact(fechaNacimiento, formatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaConvertida))
                    {
                        // La conversión fue exitosa, puedes usar fechaConvertida
                        dateFechaDeNacimiento.Value = fechaConvertida;
                    }
                    comboOficio.Text= ingresoFiltrado.Oficio;
                    comboGeneroRegistrar.Text = ingresoFiltrado.Genero;
                    textDireccion.Text = ingresoFiltrado.Direccion;
                    textTelefono.Text = ingresoFiltrado.Telefono;
                    imagenPerfil = ingresoFiltrado.ImagenPerfil;
                    using (MemoryStream ss = new MemoryStream(ingresoFiltrado.ImagenPerfil))
                    {
                        // Carga la imagen desde el MemoryStream
                        Image imagen = Image.FromStream(ss);
                        // Asigna la imagen al control PictureBox
                        picturePerfil.Image = imagen;
                        imagenPicture = ss.ToArray();
                    }
                    textNombreDelPadre.Text = ingresoFiltrado.ParentezcoPadre;
                    textNombreDeLaMadre.Text = ingresoFiltrado.ParentezcoMadre;
                    comboEstadoCivil.Text = ingresoFiltrado.EstadoCivil;
                    textNumeroDeHijos.Text = ingresoFiltrado.NumeroHijos.ToString(); // Agregar número de hijos
                    textNombreDelConyugue.Text = ingresoFiltrado.NombreConyugue; // Agregar nombre del cónyuge
                    comboBautizado.Text = ingresoFiltrado.Bautizado; // Agregar campo de bautizado 
                    string fechaBautismo = FormatearFecha(ingresoFiltrado.FechaDeBautizmo);
                    if (DateTime.TryParseExact(fechaBautismo, formatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaConvertida))
                    {
                        // La conversión fue exitosa, puedes usar fechaConvertida
                        dateFechaDeBautismo.Value = fechaConvertida;
                    }
                    textLugarBautizmo.Text = ingresoFiltrado.LugarBautizmo; // Agregar lugar de bautismo
                    comboPastorOficiante.Text = ingresoFiltrado.PastorOficiante; // Agregar pastor oficiante en el bautismo
                    comboSellado.Text = ingresoFiltrado.Sellado; // Agregar campo de sellado
                    comboRecuerda.Text = ingresoFiltrado.SelladoRecuerdo; // Agregar sellado recuerdo
                    string fechaPromesa = FormatearFecha(ingresoFiltrado.FechaPromesa);
                    if (DateTime.TryParseExact(fechaPromesa, formatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaConvertida))
                    {
                        // La conversión fue exitosa, puedes usar fechaConvertida
                        dateFechaEspirituSanto.Value = fechaConvertida;
                    }
                    textTiempoDeConversion.Text = ingresoFiltrado.TiempoConversion.ToString(); // Agregar tiempo de conversión
                    textTiempoPromesa.Text = ingresoFiltrado.TiempoPromesa.ToString(); // Agregar tiempo de promesa
                    textIglesiaProcedente.Text = ingresoFiltrado.IglesiaProcedente; // Agregar iglesia de procedencia
                    comboPastorAsistente.Text = ingresoFiltrado.PastorAsistente; // Agregar pastor asistente
                    textCargosDesempeñados.Text = ingresoFiltrado.CargosDesempenados; // Agregar cargos desempeñados
                    comboActoParaServir.Text = ingresoFiltrado.Acto; // Agregar acto
                    dateFechaDeCorreccion.Value = DateTime.Parse(FormatearFecha(ingresoFiltrado.FechaCorreccion)); // Agregar fecha de corrección
                    string fechaCorreccion = FormatearFecha(ingresoFiltrado.FechaCorreccion);
                    if (DateTime.TryParseExact(fechaCorreccion, formatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaConvertida))
                    {
                        // La conversión fue exitosa, puedes usar fechaConvertida
                        dateFechaEspirituSanto.Value = fechaConvertida;
                    }
                    textTiempoCorreccion.Text = ingresoFiltrado.TiempoCorreccion.ToString(); // Agregar tiempo de corrección
                    comboMembresia.Text = ingresoFiltrado.Membresia; // Agregar membresía
                    textLugarDeTraslado.Text = ingresoFiltrado.LugarTraslado; // Agregar lugar de traslado
                    textObservaciones.Text = ingresoFiltrado.Observaciones; // Agregar observaciones
                    tabMiembros.SelectedIndex = 1;
                }
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                BusquedaMiembroRespuesta respuesta = new BusquedaMiembroRespuesta();
                respuesta = miembroService.BuscarPorIdentificacion(filtro);
                var registro = respuesta.Miembro;
                if (registro != null)
                {
                    encontrado = true;
                    idContacto = registro.IdContacto;
                    textNumeroFolio.Text = registro.Folio;
                    textNombre.Text = registro.Nombre + " " + registro.Apellido;
                    comboTipoDocumento.Text = registro.TipoDoc;
                    textNumeroDeDocumento.Text = registro.NumeroDoc;
                    dateFechaDeNacimiento.Value = registro.FechaNacimiento;
                    miembro.Oficio = comboOficio.Text;
                    comboGenero.Text = registro.Genero;
                    textDireccion.Text = registro.Direccion;
                    textTelefono.Text = registro.Telefono;
                    imagenPerfil = registro.ImagenPerfil;
                    using (MemoryStream ss = new MemoryStream(registro.ImagenPerfil))
                    {
                        // Carga la imagen desde el MemoryStream
                        Image imagen = Image.FromStream(ss);
                        // Asigna la imagen al control PictureBox
                        picturePerfil.Image = imagen;
                        imagenPicture = ss.ToArray();
                    }
                    textNombreDelPadre.Text = registro.ParentezcoPadre;
                    textNombreDeLaMadre.Text = registro.ParentezcoMadre;
                    comboEstadoCivil.Text = registro.EstadoCivil;
                    textNumeroDeHijos.Text = registro.NumeroHijos.ToString(); // Agregar número de hijos
                    textNombreDelConyugue.Text = registro.NombreConyugue; // Agregar nombre del cónyuge
                    comboBautizado.Text = registro.Bautizado; // Agregar campo de bautizado
                    dateFechaDeBautismo.Value = registro.FechaDeBautizmo;
                    textLugarBautizmo.Text = registro.LugarBautizmo; // Agregar lugar de bautismo
                    comboPastorOficiante.Text = registro.PastorOficiante; // Agregar pastor oficiante en el bautismo
                    comboSellado.Text = registro.Sellado; // Agregar campo de sellado
                    comboRecuerda.Text = registro.SelladoRecuerdo; // Agregar sellado recuerdo
                    dateFechaEspirituSanto.Value = registro.FechaPromesa; // Agregar fecha de promesa
                    textTiempoDeConversion.Text = registro.TiempoConversion.ToString(); // Agregar tiempo de conversión
                    textTiempoPromesa.Text = registro.TiempoPromesa.ToString(); // Agregar tiempo de promesa
                    textIglesiaProcedente.Text = registro.IglesiaProcedente; // Agregar iglesia de procedencia
                    comboPastorAsistente.Text = registro.PastorAsistente; // Agregar pastor asistente
                    textCargosDesempeñados.Text = registro.CargosDesempenados; // Agregar cargos desempeñados
                    comboActoParaServir.Text = registro.Acto; // Agregar acto
                    dateFechaDeCorreccion.Value = registro.FechaCorreccion; // Agregar fecha de corrección
                    textTiempoCorreccion.Text = registro.TiempoCorreccion.ToString(); // Agregar tiempo de corrección
                    comboMembresia.Text = registro.Membresia; // Agregar membresía
                    textLugarDeTraslado.Text = registro.LugarTraslado; // Agregar lugar de traslado
                    textObservaciones.Text = registro.Observaciones; // Agregar observaciones
                }
            }
        }
        void FiltroPorApellido(string filtro)
        {
            BusquedaContactoRespuesta respuesta = new BusquedaContactoRespuesta();
            respuesta = contactoService.BuscarPorApellido(filtro);
            var registro = respuesta.Contacto;
            if (registro != null)
            {
                encontradoNombre = true;
            }
        }
        void FiltroPorNombre(string filtro)
        {
            BusquedaContactoRespuesta respuesta = new BusquedaContactoRespuesta();
            respuesta = contactoService.BuscarPorNombre(filtro);
            var registro = respuesta.Contacto;
            if (registro != null)
            {
                encontradoApellido = true;
            }
        }
        private Contacto MapearDatosContacto()
        {
            contacto = new Contacto();
            contacto.IdContacto = idContacto;
            contacto.Nombre = nombres;
            contacto.Apellido = apellidos;
            contacto.TelefonoContacto = textTelefono.Text;
            contacto.TelefonoWhatsapp = numeroWhatsapp;
            contacto.Oficio = comboOficio.Text;
            return contacto;
        }
        private Miembro MapearDatosMiembro()
        {
            miembro = new Miembro();
            miembro.IdContacto = idContacto;
            miembro.Folio = textNumeroFolio.Text;
            miembro.Nombre = nombres;
            miembro.Apellido = apellidos;
            miembro.TipoDoc = comboTipoDocumento.Text;
            miembro.NumeroDoc = textNumeroDeDocumento.Text;
            miembro.FechaNacimiento = dateFechaDeNacimiento.Value;
            miembro.CalcularEdad();
            miembro.Genero = comboGeneroRegistrar.Text;
            miembro.Oficio=comboOficio.Text;
            miembro.Direccion = textDireccion.Text;
            miembro.Telefono = textTelefono.Text;

            // Mapeo de la imagen del perfil
            if (imagenPerfil != null)
            {
                if (imagenPicture.Length != imagenPerfil.Length)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        picturePerfil.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        miembro.ImagenPerfil = ms.ToArray();
                    }
                }
                else
                {
                    miembro.ImagenPerfil = imagenPicture;
                }
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    picturePerfil.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    miembro.ImagenPerfil = ms.ToArray();
                }
            }
            miembro.ParentezcoPadre = textNombreDelPadre.Text;
            miembro.ParentezcoMadre = textNombreDeLaMadre.Text;
            miembro.EstadoCivil = comboEstadoCivil.Text; // Agregar estado civil
            miembro.NumeroHijos = Convert.ToInt32(textNumeroDeHijos.Text); // Agregar número de hijos
            miembro.NombreConyugue = textNombreDelConyugue.Text; // Agregar nombre del cónyuge
            miembro.Bautizado = comboBautizado.Text; // Agregar campo de bautizado
            miembro.FechaDeBautizmo = dateFechaDeBautismo.Value;
            miembro.LugarBautizmo = textLugarBautizmo.Text; // Agregar lugar de bautismo
            miembro.PastorOficiante = comboPastorOficiante.Text; // Agregar pastor oficiante en el bautismo
            miembro.Sellado = comboSellado.Text; // Agregar campo de sellado
            miembro.SelladoRecuerdo = comboRecuerda.Text; // Agregar sellado recuerdo
            miembro.FechaPromesa = dateFechaEspirituSanto.Value; // Agregar fecha de promesa
            miembro.TiempoConversion = Convert.ToInt32(textTiempoDeConversion.Text); // Agregar tiempo de conversión
            miembro.TiempoPromesa = Convert.ToInt32(textTiempoPromesa.Text); // Agregar tiempo de promesa
            miembro.IglesiaProcedente = textIglesiaProcedente.Text; // Agregar iglesia de procedencia
            miembro.PastorAsistente = comboPastorAsistente.Text; // Agregar pastor asistente
            miembro.CargosDesempenados = textCargosDesempeñados.Text; // Agregar cargos desempeñados
            miembro.Acto = comboActoParaServir.Text; // Agregar acto
            miembro.FechaCorreccion = dateFechaDeCorreccion.Value; // Agregar fecha de corrección
            miembro.TiempoCorreccion = Convert.ToInt32(textTiempoCorreccion.Text); // Agregar tiempo de corrección
            miembro.Motivo = textMotivo.Text=="Motivo" || textMotivo.Text == "" ? "Ninguno" : textMotivo.Text;
            miembro.Membresia = comboMembresia.Text; // Agregar membresía
            miembro.LugarTraslado = textLugarDeTraslado.Text=="Lugar de traslado" || textLugarDeTraslado.Text == "" ? "Ninguno" : textLugarDeTraslado.Text; // Agregar lugar de traslado
            miembro.Observaciones = textObservaciones.Text=="Observaciones" || textObservaciones.Text == "" ?"Ninguna" : textObservaciones.Text; // Agregar observaciones

            return miembro;
        }
        private Simpatizante MapearDatosSimpatizante()
        {
            simpatizante = new Simpatizante();
            simpatizante.IdContacto = idContacto;
            simpatizante.Nombre = nombres;
            simpatizante.Apellido = apellidos;
            simpatizante.TipoDoc = comboTipoDocumento.Text;
            simpatizante.NumeroDoc = textNumeroDeDocumento.Text;
            simpatizante.FechaNacimiento = dateFechaDeNacimiento.Value;
            simpatizante.CalcularEdad();
            simpatizante.Genero = comboGeneroRegistrar.Text;
            simpatizante.Oficio = comboOficio.Text;
            simpatizante.Direccion = textDireccion.Text;
            simpatizante.Telefono = textTelefono.Text;
            // Mapeo de la imagen del perfil
            if (imagenPerfil != null)
            {
                if (imagenPicture.Length != imagenPerfil.Length)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        picturePerfil.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        simpatizante.ImagenPerfil = ms.ToArray();
                    }
                }
                else
                {
                    simpatizante.ImagenPerfil = imagenPicture;
                }
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    picturePerfil.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    simpatizante.ImagenPerfil = ms.ToArray();
                }
            }
            return simpatizante;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            ExtraerNombreYApellido();
            GenerarIdContacto();
            Contacto contacto = MapearDatosContacto();
            Miembro nuevoMiembro = MapearDatosMiembro();
            Simpatizante nuevoSimpatizante = MapearDatosSimpatizante();
            FiltroPorNombre(nombres);
            FiltroPorApellido(apellidos);
            try
            {
                // Guardamos en contactos la informacion
                //contactoService.Guardar(contacto);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef;
                if (comboBautizado.Text == "Si" || comboBautizado.Text == "si")
                {
                    // Guardamos local
                    //string mensaje = miembroService.Guardar(nuevoMiembro);
                    //Guardamos el miembro
                    var member = memberMaps.MemberMap(nuevoMiembro);
                    docRef = db.Collection("MembersData").Document(member.Folio.ToString());
                    docRef.SetAsync(member);
                    // Guardamos el contacto
                    var contact = contactMaps.ContactMap(contacto);
                    docRef = db.Collection("ContactsData").Document(contact.IdContacto.ToString());
                    docRef.SetAsync(contact);
                    MessageBox.Show("Se ha registrado el nuevo miembro correctamente", "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeMiembros();
                    LimpiarCampos();
                    CalcularFolio();
                    tabMiembros.SelectedIndex = 0;
                }
                else
                {
                    // Guardamos local
                    //string mensaje = simpatizanteService.Guardar(nuevoSimpatizante);
                    // Guardamos el contacto
                    var contact = contactMaps.ContactMap(contacto);
                    docRef = db.Collection("ContactsData").Document(contact.IdContacto.ToString());
                    docRef.SetAsync(contact);
                    //Guardamos el simpatizante
                    var simpatizante = sympathizerMaps.SympathizerMap(nuevoSimpatizante);
                    docRef = db.Collection("SympathizerData").Document(simpatizante.NumeroDoc);
                    docRef.SetAsync(simpatizante);
                    MessageBox.Show("Se ha registrado el nuevo miembro correctamente", "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeSimpatizante();
                    LimpiarCampos();
                    CalcularFolio();
                    tabMiembros.SelectedIndex = 0;
                }
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                if (encontradoNombre != true && encontradoApellido != true)
                {
                    contactoService.Guardar(contacto);
                    string mensaje = miembroService.Guardar(miembro);
                    MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeMiembros();
                    ConsultarYLlenarGridDeSimpatizante();
                    LimpiarCampos();
                    CalcularFolio();
                    tabMiembros.SelectedIndex = 0;
                }
            }
        }
        private void btnModificar_Click(object sender, EventArgs e)
        {
            ExtraerNombreYApellido();
            Contacto contacto = MapearDatosContacto();
            Miembro miembro = MapearDatosMiembro();
            FiltroPorNombre(nombres);
            FiltroPorApellido(apellidos);
            try
            {
                if (comboBautizado.Text == "Si" || comboBautizado.Text == "si")
                {
                    //contactoService.Modificar(contacto);
                    var db = FirebaseService.Database;
                    var member = memberMaps.MemberMap(miembro);
                    Google.Cloud.Firestore.DocumentReference docRef = db.Collection("MembersData").Document(member.Folio.ToString());
                    docRef.SetAsync(member);
                    MessageBox.Show("¿Seguro que desea modificar los datos para este miembro?", "Mensaje de modificacion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeMiembros();
                    LimpiarCampos();
                    tabMiembros.SelectedIndex = 0;
                    btnRegistrar.Enabled = true;
                }
                else
                {
                    //contactoService.Modificar(contacto);
                    var db = FirebaseService.Database;
                    var sympathizer = sympathizerMaps.SympathizerMap(simpatizante);
                    Google.Cloud.Firestore.DocumentReference docRef = db.Collection("SympathizerData").Document(sympathizer.NumeroDoc.ToString());
                    docRef.SetAsync(sympathizer);
                    MessageBox.Show("¿Seguro que desea modificar los datos para este simpatizante?", "Mensaje de modificacion", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeSimpatizante();
                    LimpiarCampos();
                    tabMiembros.SelectedIndex = 2;
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                if (encontradoNombre == true && encontradoApellido == true)
                {
                    if (comboBautizado.Text == "Si" || comboBautizado.Text == "si")
                    {
                        string mensaje = miembroService.Modificar(miembro);
                        contactoService.Modificar(contacto);
                        MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        ConsultarYLlenarGridDeMiembros();
                        LimpiarCampos();
                        tabMiembros.SelectedIndex = 0;
                    }
                    else
                    {
                        string mensaje = simpatizanteService.Modificar(simpatizante);
                        contactoService.Modificar(contacto);
                        MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                        ConsultarYLlenarGridDeSimpatizante();
                        LimpiarCampos();
                        tabMiembros.SelectedIndex = 2;
                    }
                }
            }
        }
        private void btnCargarFoto_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ventanaCargar = new System.Windows.Forms.OpenFileDialog();
            DialogResult dr = ventanaCargar.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (validaciones.ValidarImagen(ventanaCargar.FileName) == true)
                {
                    // La imagen cargada en picturePerfil es válida
                    picturePerfil.Image = Image.FromFile(ventanaCargar.FileName);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        picturePerfil.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); // Puedes cambiar el formato según sea necesario
                        imagenPicture = ms.ToArray();
                    }
                }
                else
                {
                    string mensaje = "Formato de imagen incorrecta";
                    MessageBox.Show(mensaje, "Error de carga de imagen", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                }
            }

        }
        void CalculosPorFecha()
        {
            miembro = new Miembro();
            miembro.FechaDeBautizmo = dateFechaDeBautismo.Value;
            miembro.CalcularTiempoDeConversion();
            textTiempoDeConversion.Text = miembro.TiempoConversion.ToString();
        }
        private void textTelefono_TextChanged(object sender, EventArgs e)
        {
            if (textTelefono.Text != "")
            {
                numeroWhatsapp = "+57" + textTelefono.Text;
            }
        }
        void EliminarMiembro(string id)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("MembersData").Document(id);
                docRef.DeleteAsync();
                MessageBox.Show("¿Desea elminar este registro?", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeMiembros();
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                string mensaje = miembroService.Eliminar(id);
                contactoService.Eliminar(idContacto);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private async void FiltroPorGenero(string filtro)
        {
            if (generoDelTipo=="Miembro")
            {
                try
                {
                    var db = FirebaseService.Database;
                    var miembrosQuery = db.Collection("MembersData");
                    var miembros = new List<MemberData>();
                    // Realizar la suma directamente en la consulta Firestore
                    var snapshot = await miembrosQuery.GetSnapshotAsync();
                    miembros = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                    // Filtrar elementos según el campo Valor y la variable id
                    var membersGenero = miembros.Where(miembro => miembro.Genero == filtro).ToList();
                    dataGridMiembros.DataSource = membersGenero;
                    Borrar.Visible = true;
                    // Obtener referencia al formulario principal
                    FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                    // Verificar si el formulario principal está abierto
                    if (formPrincipal != null)
                    {
                        // Lanzar el evento para notificar al formulario principal sobre la excepción
                        formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                    }
                }
                catch(Exception ex)
                {
                    // Obtener referencia al formulario principal
                    FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                    // Verificar si el formulario principal está abierto
                    if (formPrincipal != null)
                    {
                        // Lanzar el evento para notificar al formulario principal sobre la excepción
                        formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                    }
                    BusquedaMiembroRespuesta respuesta = new BusquedaMiembroRespuesta();
                    respuesta = miembroService.BuscarPorGenero(filtro);
                    var registro = respuesta.Miembro;
                    if (registro != null)
                    {
                        //dataGridMiembros.DataSource = null;
                        var miembros = new List<Miembro> { registro };
                        dataGridMiembros.DataSource = miembros;
                        foreach (DataGridViewRow row in dataGridMiembros.Rows)
                        {
                            DataGridViewImageCell cell = row.Cells["ImagenPerfil"] as DataGridViewImageCell;
                            if (cell != null)
                            {
                                byte[] imageBytes = cell.Value as byte[];
                                if (imageBytes != null)
                                {
                                    // Convierte los bytes en una imagen
                                    Image originalImage;
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        originalImage = Image.FromStream(ms);
                                    }

                                    // Redimensiona la imagen para que quepa en la celda
                                    Image resizedImage = ResizeImageToFitCell(cell, originalImage);

                                    // Asigna la imagen redimensionada a la celda
                                    cell.Value = resizedImage;
                                }
                            }
                        }
                    }
                    else
                    {
                        dataGridMiembros.DataSource = null;
                    }
                }

            }
            else
            {
                try
                {
                    var db = FirebaseService.Database;
                    var simpatizanteQuery = db.Collection("SympathizerData");
                    var simpatizantes = new List<SympathizerData>();
                    // Realizar la suma directamente en la consulta Firestore
                    var snapshot = await simpatizanteQuery.GetSnapshotAsync();
                    simpatizantes = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<SympathizerData>()).ToList();
                    // Filtrar elementos según el campo Valor y la variable id
                    var simpatizantesGenero = miembros.Where(simpatizante => simpatizante.Genero == filtro).ToList();
                    dataGridMiembros.DataSource = simpatizantesGenero;
                    Borrar.Visible = true;
                    // Obtener referencia al formulario principal
                    FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                    // Verificar si el formulario principal está abierto
                    if (formPrincipal != null)
                    {
                        // Lanzar el evento para notificar al formulario principal sobre la excepción
                        formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                    }
                }
                catch(Exception ex)
                {
                    // Obtener referencia al formulario principal
                    FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                    // Verificar si el formulario principal está abierto
                    if (formPrincipal != null)
                    {
                        // Lanzar el evento para notificar al formulario principal sobre la excepción
                        formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                    }
                    BusquedaMiembroRespuesta respuesta = new BusquedaMiembroRespuesta();
                    respuesta = miembroService.BuscarPorGenero(filtro);
                    var registro = respuesta.Miembro;
                    if (registro != null)
                    {
                        //dataGridMiembros.DataSource = null;
                        var miembros = new List<Miembro> { registro };
                        dataGridMiembros.DataSource = miembros;
                        foreach (DataGridViewRow row in dataGridMiembros.Rows)
                        {
                            DataGridViewImageCell cell = row.Cells["ImagenPerfil"] as DataGridViewImageCell;
                            if (cell != null)
                            {
                                byte[] imageBytes = cell.Value as byte[];
                                if (imageBytes != null)
                                {
                                    // Convierte los bytes en una imagen
                                    Image originalImage;
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        originalImage = Image.FromStream(ms);
                                    }

                                    // Redimensiona la imagen para que quepa en la celda
                                    Image resizedImage = ResizeImageToFitCell(cell, originalImage);

                                    // Asigna la imagen redimensionada a la celda
                                    cell.Value = resizedImage;
                                }
                            }
                        }
                    }
                    else
                    {
                        dataGridMiembros.DataSource = null;
                    }
                }
            }
        }
        private async void FiltroPorServicio(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var miembrosQuery = db.Collection("MembersData");
                var miembros = new List<MemberData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await miembrosQuery.GetSnapshotAsync();
                miembros = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var membersGenero = miembros.Where(miembro => miembro.Acto == filtro).ToList();
                dataGridMiembros.DataSource = membersGenero;
                Borrar.Visible = true;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                BusquedaMiembroRespuesta respuesta = new BusquedaMiembroRespuesta();
                respuesta = miembroService.BuscarPorGenero(filtro);
                var registro = respuesta.Miembro;
                if (registro != null)
                {
                    //dataGridMiembros.DataSource = null;
                    var miembros = new List<Miembro> { registro };
                    dataGridMiembros.DataSource = miembros;
                    foreach (DataGridViewRow row in dataGridMiembros.Rows)
                    {
                        DataGridViewImageCell cell = row.Cells["ImagenPerfil"] as DataGridViewImageCell;
                        if (cell != null)
                        {
                            byte[] imageBytes = cell.Value as byte[];
                            if (imageBytes != null)
                            {
                                // Convierte los bytes en una imagen
                                Image originalImage;
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    originalImage = Image.FromStream(ms);
                                }

                                // Redimensiona la imagen para que quepa en la celda
                                Image resizedImage = ResizeImageToFitCell(cell, originalImage);

                                // Asigna la imagen redimensionada a la celda
                                cell.Value = resizedImage;
                            }
                        }
                    }
                }
                else
                {
                    dataGridMiembros.DataSource = null;
                }
            }
        }
        private async void FiltroPorLugarBautizmo(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var miembrosQuery = db.Collection("MembersData");
                var miembros = new List<MemberData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await miembrosQuery.GetSnapshotAsync();
                miembros = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<MemberData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var membersGenero = new List<MemberData>();
                membersGenero = (filtro == "Gerizim")
                    ? miembros.Where(miembro => miembro.LugarBautizmo == filtro).ToList()
                    : miembros.Where(miembro => miembro.LugarBautizmo != filtro).ToList();
                dataGridMiembros.DataSource = membersGenero;
                Borrar.Visible = true;
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnSuccesfulOperations(new SuccesfullEventArgs("Succesfull"));
                }
            }
            catch(Exception ex)
            {
                // Obtener referencia al formulario principal
                FormMenu formPrincipal = Application.OpenForms.OfType<FormMenu>().FirstOrDefault();
                // Verificar si el formulario principal está abierto
                if (formPrincipal != null)
                {
                    // Lanzar el evento para notificar al formulario principal sobre la excepción
                    formPrincipal.OnExcepcionOcurrida(new ExcepcionEventArgs(ex.Message));
                }
                BusquedaMiembroRespuesta respuesta = new BusquedaMiembroRespuesta();
                respuesta = miembroService.BuscarPorGenero(filtro);
                var registro = respuesta.Miembro;
                if (registro != null)
                {
                    //dataGridMiembros.DataSource = null;
                    var miembros = new List<Miembro> { registro };
                    dataGridMiembros.DataSource = miembros;
                    foreach (DataGridViewRow row in dataGridMiembros.Rows)
                    {
                        DataGridViewImageCell cell = row.Cells["ImagenPerfil"] as DataGridViewImageCell;
                        if (cell != null)
                        {
                            byte[] imageBytes = cell.Value as byte[];
                            if (imageBytes != null)
                            {
                                // Convierte los bytes en una imagen
                                Image originalImage;
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    originalImage = Image.FromStream(ms);
                                }

                                // Redimensiona la imagen para que quepa en la celda
                                Image resizedImage = ResizeImageToFitCell(cell, originalImage);

                                // Asigna la imagen redimensionada a la celda
                                cell.Value = resizedImage;
                            }
                        }
                    }
                }
                else
                {
                    dataGridMiembros.DataSource = null;
                }
            }
        }
        private void comboGenero_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filtro = comboGenero.Text;
            generoDelTipo = "Miembro";
            if (comboGenero.Text != "" && comboGenero.Text != "Todos")
            {
                FiltroPorGenero(filtro);
            }
            else
            {
                ConsultarYLlenarGridDeMiembros();
            }
        }
        private void FiltroPorFamilia(string apellidos)
        {
            ConsultaMiembroRespuesta respuesta = new ConsultaMiembroRespuesta();
            respuesta = miembroService.BuscarPorFamilia(apellidos);
            miembros = respuesta.Miembros.ToList();
            if (respuesta.Miembros.Count != 0 && respuesta.Miembros != null)
            {
                dataGridMiembros.DataSource = respuesta.Miembros;
            }
        }
        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = false;
            btnCloseSearchLibreta.Visible = true;
            textSearch.Visible = true;
        }
        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = true;
            btnCloseSearchLibreta.Visible = false;
            textSearch.Visible = false;
            if (textSearch.Text != "Buscar")
            {
                textSearch.Text = "Buscar";
            }
        }
        private void textSearch_TextChanged(object sender, EventArgs e)
        {
            var filtro = textSearch.Text;
            if (textSearch.Text != "" && textSearch.Text != "Buscar")
            {
                FiltroPorFamilia(filtro);
                if (encontrado == false)
                {
                    dataGridMiembros.CurrentCell = null;
                    foreach (DataGridViewRow fila in dataGridMiembros.Rows)
                    {
                        fila.Visible = false;
                    };
                    foreach (DataGridViewRow fila in dataGridMiembros.Rows)
                    {
                        int i = 0;
                        foreach (DataGridViewCell celda in fila.Cells)
                        {
                            if (i >= 7 && i<=8)
                            {
                                if ((celda.Value.ToString().ToUpper()).IndexOf(textSearch.Text.ToUpper()) == 0)
                                {
                                    fila.Visible = true;
                                    break;
                                }
                                else
                                {
                                    if ((celda.Value.ToString() == (textSearch.Text.ToUpper())))
                                    {
                                        fila.Visible = true;
                                        break;
                                    }
                                }
                            }
                            i = i + 1;
                        }
                    }
                }
            }
            else
            {
                ConsultarYLlenarGridDeMiembros();
            }
        }
        private void textSearch_Enter(object sender, EventArgs e)
        {
            if (textSearch.Text == "Buscar")
            {
                textSearch.Text = "";
            }
        }
        private void textSearch_Leave(object sender, EventArgs e)
        {
            if (textSearch.Text == "")
            {
                textSearch.Text = "Buscar";
            }
        }
        //Validaciones de campos
        private void textNombre_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNombre.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombre.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNombre_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNombre.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombre.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textNumeroDeId_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNumeroDeDocumento.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNumeroDeDocumento.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNumeroDeId_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNumeroDeDocumento.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNumeroDeDocumento.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void comboTipoDocumento_Enter(object sender, EventArgs e)
        {
            string placeHolder = comboTipoDocumento.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboTipoDocumento.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void comboTipoDocumento_Leave(object sender, EventArgs e)
        {
            string placeHolder = comboTipoDocumento.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboTipoDocumento.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void comboGeneroRegistrar_Enter(object sender, EventArgs e)
        {
            string placeHolder = comboGeneroRegistrar.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboGeneroRegistrar.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void comboGeneroRegistrar_Leave(object sender, EventArgs e)
        {
            string placeHolder = comboGeneroRegistrar.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboGeneroRegistrar.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void comboOficio_Enter(object sender, EventArgs e)
        {
            string placeHolder = comboOficio.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboOficio.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void comboOficio_Leave(object sender, EventArgs e)
        {
            string placeHolder = comboOficio.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboOficio.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textDireccion_Enter(object sender, EventArgs e)
        {
            string placeHolder = textDireccion.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textDireccion.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textDireccion_Leave(object sender, EventArgs e)
        {
            string placeHolder = textDireccion.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textDireccion.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textTelefono_Enter(object sender, EventArgs e)
        {
            string placeHolder = textTelefono.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textTelefono.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textTelefono_Leave(object sender, EventArgs e)
        {
            string placeHolder = textTelefono.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textTelefono.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textNombreDelPadre_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNombreDelPadre.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombreDelPadre.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNombreDelPadre_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNombreDelPadre.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombreDelPadre.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textNombreDeLaMadre_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNombreDeLaMadre.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombreDeLaMadre.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNombreDeLaMadre_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNombreDeLaMadre.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombreDeLaMadre.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }
        private void textNumeroDeHijos_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNumeroDeHijos.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNumeroDeHijos.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNumeroDeHijos_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNumeroDeHijos.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNumeroDeHijos.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textNombreDelConyugue_Enter(object sender, EventArgs e)
        {
            string placeHolder = textNombreDelConyugue.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombreDelConyugue.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textNombreDelConyugue_Leave(object sender, EventArgs e)
        {
            string placeHolder = textNombreDelConyugue.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNombreDelConyugue.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textLugarBautizmo_Enter(object sender, EventArgs e)
        {
            string placeHolder = textLugarBautizmo.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textLugarBautizmo.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textLugarBautizmo_Leave(object sender, EventArgs e)
        {
            string placeHolder = textLugarBautizmo.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textLugarBautizmo.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textIglesiaProcedente_Enter(object sender, EventArgs e)
        {
            string placeHolder = textIglesiaProcedente.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textIglesiaProcedente.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textIglesiaProcedente_Leave(object sender, EventArgs e)
        {
            string placeHolder = textIglesiaProcedente.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textIglesiaProcedente.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textCargosDesempeñados_Enter(object sender, EventArgs e)
        {
            string placeHolder = textCargosDesempeñados.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textCargosDesempeñados.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textCargosDesempeñados_Leave(object sender, EventArgs e)
        {
            string placeHolder = textCargosDesempeñados.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textCargosDesempeñados.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textMotivo_Enter(object sender, EventArgs e)
        {
            string placeHolder = textMotivo.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textMotivo.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textMotivo_Leave(object sender, EventArgs e)
        {
            string placeHolder = textMotivo.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textMotivo.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textLugarTraslado_Enter(object sender, EventArgs e)
        {
            string placeHolder = textLugarDeTraslado.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textLugarDeTraslado.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textLugarTraslado_Leave(object sender, EventArgs e)
        {
            string placeHolder = textLugarDeTraslado.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textLugarDeTraslado.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void textObservaciones_Enter(object sender, EventArgs e)
        {
            string placeHolder = textObservaciones.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textObservaciones.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void textObservaciones_Leave(object sender, EventArgs e)
        {
            string placeHolder = textObservaciones.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textObservaciones.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void comboEstadoCivil_Enter(object sender, EventArgs e)
        {
            string placeHolder = comboEstadoCivil.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboEstadoCivil.Text = validaciones.TextoPlaceHolderEnter(placeHolder, nombreDelComponente);
        }

        private void comboEstadoCivil_Leave(object sender, EventArgs e)
        {
            string placeHolder = comboEstadoCivil.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboEstadoCivil.Text = validaciones.TextoPlaceHolderLeave(placeHolder, nombreDelComponente);
        }

        private void comboEstadoCivil_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = comboEstadoCivil.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textNumeroDeHijos.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
            textNombreDelConyugue.Enabled= validaciones.ComboResponse(item, nombreDelComponente);
        }
        private void comboBautizado_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = comboBautizado.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            dateFechaDeBautismo.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
            textLugarBautizmo.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
            comboPastorOficiante.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
        }
        private void comboSellado_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = comboSellado.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            comboRecuerda.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
        }
        private void comboRecuerda_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = comboRecuerda.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            dateFechaEspirituSanto.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
        }

        private void comboActoParaServir_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = comboActoParaServir.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            dateFechaDeCorreccion.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
            textMotivo.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
        }

        private void comboMembresia_SelectedIndexChanged(object sender, EventArgs e)
        {
            string item = comboMembresia.Text;
            string nombreDelComponente = ((System.Windows.Forms.Control)sender).Name;
            textLugarDeTraslado.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
            textObservaciones.Enabled = validaciones.ComboResponse(item, nombreDelComponente);
        }

        private void dateFechaDeBautismo_ValueChanged(object sender, EventArgs e)
        {
            CalculosPorFecha();
        }

        private void dateFechaEspirituSanto_ValueChanged(object sender, EventArgs e)
        {
            CalculosPorFecha();
        }

        private void dateFechaDeCorreccion_ValueChanged(object sender, EventArgs e)
        {
            CalculosPorFecha();
        }

        private void comboEstadoServicio_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filtro = comboEstadoServicio.Text;
            if (comboEstadoServicio.Text != "" && comboEstadoServicio.Text != "Todos")
            {
                FiltroPorServicio(filtro);
            }
            else
            {
                ConsultarYLlenarGridDeMiembros();
            }
        }

        private void comboEstadoBautizmo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filtro = comboLugarBautizmo.Text;
            if (comboLugarBautizmo.Text != "" && comboLugarBautizmo.Text != "Todos")
            {
                FiltroPorLugarBautizmo(filtro);
            }
            else
            {
                ConsultarYLlenarGridDeMiembros();
            }
        }

        private void FormMiembros_Load(object sender, EventArgs e)
        {
            if (tabMiembros.TabCount > 0)
            {
                tabPage = tabMiembros.TabPages["tabSimpatizantes"];
                tabMiembros.TabPages.RemoveAt(2);
            }
        }

        private void btnVerListaSimpatizantes_Click(object sender, EventArgs e)
        {
            tabMiembros.TabPages.Add(tabPage);
            tabMiembros.SelectedIndex = 2;
        }

        private void tabMiembros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabMiembros.TabCount > 2)
            {
                if (tabMiembros.SelectedIndex != 2)
                {
                    tabPage = tabMiembros.TabPages["tabSimpatizantes"];
                    tabMiembros.TabPages.RemoveAt(2);
                }
            }
        }

        private void comboGenero2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filtro = comboGenero2.Text;
            generoDelTipo = "Simpatizante";
            if (comboGenero2.Text != "" && comboGenero2.Text != "Todos")
            {
                FiltroPorGenero(filtro);
            }
            else
            {
                ConsultarYLlenarGridDeMiembros();
            }
        }

        private void dataGridMiembros_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridMiembros.DataSource != null)
            {
                if (dataGridMiembros.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridMiembros.CurrentRow.Cells["Folio"].Value.ToString());
                    EliminarMiembro(id);
                    ConsultarYLlenarGridDeMiembros();
                }
                else
                {
                    if (dataGridMiembros.Columns[e.ColumnIndex].Name == "Editar")
                    {
                        id = Convert.ToString(dataGridMiembros.CurrentRow.Cells["Folio"].Value.ToString());
                        FiltroPorIdentificacion(id);
                        tabMiembros.SelectedIndex = 1;
                        btnRegistrar.Enabled = false;
                    }
                    else
                    {
                        if (dataGridMiembros.Columns[e.ColumnIndex].Name == "Imprimir")
                        {
                            id = Convert.ToString(dataGridMiembros.CurrentRow.Cells["Folio"].Value.ToString());
                            FiltroPorIdentificacion(id);
                            Miembro miembro = MapearDatosMiembro();
                            List<Miembro> miembros = new List<Miembro>
                            {
                                miembro
                            };
                            reporte.GenerarLibroDeMiembrosSecretaria(miembros);
                        }
                    }
                }
            }
        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.Show();
        }
    }
}
