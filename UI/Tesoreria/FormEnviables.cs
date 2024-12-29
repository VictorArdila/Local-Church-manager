using BLL;
using Cloud;
using DocumentFormat.OpenXml.Office2010.Excel;
using Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Ldb;

namespace UI
{
    public partial class FormEnviables : Form
    {
        public readonly Validaciones validaciones;
        ShippableMaps shippableMaps;
        Enviable enviable;
        EnviableService enviableService;
        List<Enviable> enviables;
        string originalText;
        string id;
        string comite;
        bool encontrado = false;
        bool detallo = false;
        public FormEnviables()
        {
            enviableService = new EnviableService(DbConfig.ConnectionString);
            shippableMaps = new ShippableMaps();
            InitializeComponent();
            ConsultarEnvios();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGestionarEnviables_Click(object sender, EventArgs e)
        {
            tabEnviables.SelectedIndex = 1;
        }
        private void textDetalle_Enter(object sender, EventArgs e)
        {
            if (textDetalle.Text == "Detalle")
            {
                textDetalle.Text = "";
            }
        }

        private void textDetalle_Leave(object sender, EventArgs e)
        {
            if (textDetalle.Text == "")
            {
                textDetalle.Text = "Detalle";
            }
        }

        private void textDineroIngreso_Enter(object sender, EventArgs e)
        {
            originalText = textDineroIngreso.Text;
            if (textDineroIngreso.Text == "$ 000.00")
            {
                textDineroIngreso.Text = "";
            }
            else
            {

                if (textDineroIngreso.Text.StartsWith("$ "))
                {
                    // Borra el contenido del TextBox si comienza con "$ "
                    textDineroIngreso.Text = "";
                }
            }
        }

        private void textDineroIngreso_Leave(object sender, EventArgs e)
        {
            if (textDineroIngreso.Text == "")
            {
                textDineroIngreso.Text = "$ 000.00";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(textDineroIngreso.Text))
                {
                    // Restaura el contenido original al salir del TextBox
                    textDineroIngreso.Text = originalText;
                }
            }
        }
        private string LecturaCifra(int totalDeIngresos)
        {
            // Convierte el total de ingresos a una cadena con separadores de miles
            string cifraFormateada = totalDeIngresos.ToString("N0");

            // Muestra la cifra formateada en el TextBox o donde desees
            string valorFormateado = $"${cifraFormateada}";
            return valorFormateado;
        }
        private void textDineroIngreso_Validated(object sender, EventArgs e)
        {
            if (textDineroIngreso.Text != "" && textDineroIngreso.Text != "$ 000.00")
            {
                int sumIngreso = int.Parse(textDineroIngreso.Text);
                textDineroIngreso.Text = LecturaCifra(sumIngreso);
            }
        }

        private void dataGridEnviables_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridEnviables.DataSource != null)
            {
                if (dataGridEnviables.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridEnviables.CurrentRow.Cells["Id"].Value.ToString());
                    EliminarEnviable(id);
                    ConsultarEnvios();
                }
                else
                {
                    if (dataGridEnviables.Columns[e.ColumnIndex].Name == "Editar")
                    {
                        id = Convert.ToString(dataGridEnviables.CurrentRow.Cells["Id"].Value.ToString());
                        FiltroPorId(id);
                        tabEnviables.SelectedIndex = 1;
                        btnRegistrar.Enabled = false;
                    }
                }
            }
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

        private async void FiltroPorId(string id)
        {
            try
            {
                var db = FirebaseService.Database;
                var enviablesQuery = db.Collection("ShippableData");
                var enviables = new List<ShippableData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await enviablesQuery.GetSnapshotAsync();
                enviables = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ShippableData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var enviablesFiltrados = enviables.Where(ingreso => ingreso.Id == id).ToList();
                if (enviablesFiltrados.Any())
                {
                    var ingresoFiltrado = enviablesFiltrados.First(); // Obtener el primer elemento de la lista
                    //encontrado = true;
                    dateFechaEnviable.Value = DateTime.Parse(FormatearFecha(ingresoFiltrado.Id));
                    comboComite.Text = ingresoFiltrado.Comite;
                    comboConcepto.Text = ingresoFiltrado.Concepto;
                    textDineroIngreso.Text = ingresoFiltrado.Valor.ToString();
                    textDetalle.Text = ingresoFiltrado.Detalle;
                    tabEnviables.SelectedIndex = 1;
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
                BusquedaEnviableRespuesta respuesta = new BusquedaEnviableRespuesta();
                respuesta = enviableService.BuscarPorIdentificacion(id);
                var registro = respuesta.Enviable;
                if (registro != null)
                {
                    encontrado = true;
                    var ingresos = new List<Enviable> { registro };
                    dateFechaEnviable.Value = ingresos[0].FechaDeEnvio;
                    comboComite.Text = ingresos[0].Comite;
                    comboConcepto.Text = ingresos[0].Concepto;
                    textDineroIngreso.Text = ingresos[0].Valor.ToString();
                    textDetalle.Text = ingresos[0].Detalle;
                }
            }
        }
        private void EliminarEnviable(string id)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("ShippableData").Document(id);
                docRef.DeleteAsync();
                string mensaje = enviableService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEnvios();
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
                string mensaje = enviableService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEnvios();
            }
        }
        private async void TotalizarRegistros()
        {
            try
            {
                var db = FirebaseService.Database;
                var enviablesQuery = db.Collection("ShippableData");
                var snapshot = await enviablesQuery.GetSnapshotAsync();
                textTotalNube.Text = snapshot.Documents.Count().ToString();
                textTotalLocal.Text = enviableService.Totalizar().Cuenta.ToString();
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
                ConsultaEnviableRespuesta respuesta = new ConsultaEnviableRespuesta();
                respuesta = enviableService.ConsultarTodos();
                if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                {
                    dataGridEnviables.DataSource = null;
                    enviables = respuesta.Enviables.ToList();
                    if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                    {
                        textTotalLocal.Text = enviableService.Totalizar().Cuenta.ToString();
                        textTotalNube.Text = "0";
                    }
                    else
                    {
                        textTotalLocal.Text = "0";
                        textTotalNube.Text = "0";
                    }
                }
            }
        }

        private async void ConsultarEnvios()
        {
            TotalizarRegistros();
            int sumTotal = 0;
            try
            {
                var db = FirebaseService.Database;
                var shippableQuery = db.Collection("ShippableData");
                var enviables = new List<ShippableData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await shippableQuery.GetSnapshotAsync();
                enviables = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ShippableData>()).ToList();
                sumTotal= snapshot.Documents.Sum(doc => doc.ConvertTo<ShippableData>().Valor);
                if (enviables.Count > 0)
                {
                    dataGridEnviables.DataSource = null;
                    dataGridEnviables.DataSource = enviables;
                    textValorFecha.Text = "0";
                    comboFecha.Text = "Mes";
                    textTotalEnvios.Text = sumTotal.ToString();
                }
                else
                {
                    dataGridEnviables.DataSource = null;
                    textTotalLocal.Text = "0";
                    textValorFecha.Text = "0";
                    comboFecha.Text = "Mes";
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
                ConsultaEnviableRespuesta respuesta = new ConsultaEnviableRespuesta();
                respuesta = enviableService.ConsultarTodos();
                if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                {
                    dataGridEnviables.DataSource = null;
                    enviables = respuesta.Enviables.ToList();
                    dataGridEnviables.DataSource = respuesta.Enviables;
                    Borrar.Visible = true;
                    textTotalLocal.Text = enviableService.Totalizar().Cuenta.ToString();
                }
            }
        }

        private Enviable MapearEnviable()
        {
            enviable = new Enviable();
            enviable.GenerarCodigoEnvio();
            enviable.FechaDeEnvio = dateFechaEnviable.Value;
            enviable.Comite = comboComite.Text;
            enviable.Concepto = comboConcepto.Text;
            string cantidadConSigno = textDineroIngreso.Text; // Esto contiene "$ 30000"
            string cantidadSinSigno = cantidadConSigno.Replace("$", "").Trim(); // Esto quita el signo "$"
            string cantidadSinPuntos = cantidadSinSigno.Replace(".", "").Trim();
            int cantidadEntera = int.Parse(cantidadSinPuntos); // Convierte el valor a un entero
            enviable.Valor = cantidadEntera;
            enviable.Detalle = textDetalle.Text;
            return enviable;
        }
        private void Limpiar()
        {
            dateFechaEnviable.Value = DateTime.Now;
            comboComite.Text = "Comite";
            comboConcepto.Text = "Concepto";
            textDineroIngreso.Text = "$ 000.00";
            textDetalle.Text = "Detalle";
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            Enviable enviable = MapearEnviable();
            try
            {
                var msg = enviableService.Guardar(enviable);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var shippable = shippableMaps.ShippableMap(enviable);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("ShippableData").Document(shippable.Id);
                docRef.SetAsync(shippable);
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEnvios();
                Limpiar();
                tabEnviables.SelectedIndex = 0;
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
                int count = ex.Message.Length;
                if (count > 0)
                {
                    // Guardamos localmente
                    var msg = enviableService.Guardar(enviable);
                    MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarEnvios();
                    Limpiar();
                    tabEnviables.SelectedIndex = 0;
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            Enviable enviable = MapearEnviable();
            try
            {
                var msg = enviableService.Modificar(enviable);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var shippable = shippableMaps.ShippableMap(enviable);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("ShippableData").Document(shippable.Id);
                docRef.SetAsync(shippable);
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarEnvios();
                Limpiar();
                tabEnviables.SelectedIndex = 0;
                btnRegistrar.Enabled = true;
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
                string mensaje = enviableService.Modificar(enviable);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarEnvios();
                Limpiar();
                tabEnviables.SelectedIndex = 0;
            }
        }
        private string ObtenerMes(string mes)
        {
            string mesEncontrado = "";
            // Diccionario que mapea nombres de meses a números
            Dictionary<string, int> meses = new Dictionary<string, int>
            {
                {"Enero", 1},
                {"Febrero", 2},
                {"Marzo", 3},
                {"Abril", 4},
                {"Mayo", 5},
                {"Junio", 6},
                {"Julio", 7},
                {"Agosto", 8},
                {"Septiembre", 9},
                {"Octubre", 10},
                {"Noviembre", 11},
                {"Diciembre", 12}
            };

            // Intentar obtener el número del mes del diccionario
            if (meses.TryGetValue(mes, out int numeroMes))
            {
                // Devolver el número del mes como cadena
                mesEncontrado= numeroMes.ToString("D2");
            }
            return mesEncontrado;
        }
        private async void FiltroPorFecha(string filtro)
        {
            int sumTotal = 0;
            string mes = ObtenerMes(filtro);
            try
            {
                var db = FirebaseService.Database;
                var shippableQuery = db.Collection("ShippableData");
                var shippables = new List<ShippableData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await shippableQuery.GetSnapshotAsync();
                shippables = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ShippableData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var shippablesFecha = shippables.Where(enviable =>
                   enviable.FechaDeEnvio.Contains("/"+mes+"/")
                ).ToList();
                sumTotal = shippablesFecha.Sum(enviable => enviable.Valor);
                textValorFecha.Text = LecturaCifra(sumTotal);
                dataGridEnviables.DataSource = shippablesFecha;
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
                ConsultaEnviableRespuesta respuesta = new ConsultaEnviableRespuesta();
                respuesta = enviableService.FiltrarEnviablesPorFecha(filtro);
                if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                {
                    dataGridEnviables.DataSource = null;
                    enviables = respuesta.Enviables.ToList();
                    if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                    {
                        dataGridEnviables.DataSource = respuesta.Enviables;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridEnviables.DataSource = null;
                    enviables = respuesta.Enviables.ToList();
                }
            }
        }
        private async void FiltroPorComite(string filtro)
        {
            int sumTotal = 0;
            try
            {
                var db = FirebaseService.Database;
                var shippableQuery = db.Collection("ShippableData");
                var shippables = new List<ShippableData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await shippableQuery.GetSnapshotAsync();
                shippables = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ShippableData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var shippablesComite = shippables.Where(enviable => enviable.Comite==filtro).ToList();
                sumTotal = shippablesComite.Sum(enviable => enviable.Valor);
                textValorFecha.Text = LecturaCifra(sumTotal);
                dataGridEnviables.DataSource = shippablesComite;
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
                ConsultaEnviableRespuesta respuesta = new ConsultaEnviableRespuesta();
                respuesta = enviableService.FiltrarEnviablesPorFecha(filtro);
                if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                {
                    dataGridEnviables.DataSource = null;
                    enviables = respuesta.Enviables.ToList();
                    if (respuesta.Enviables.Count != 0 && respuesta.Enviables != null)
                    {
                        dataGridEnviables.DataSource = respuesta.Enviables;
                        Borrar.Visible = true;
                    }
                }
                else
                {
                    dataGridEnviables.DataSource = null;
                    enviables = respuesta.Enviables.ToList();
                }
            }
        }

        private void comboFecha_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFecha.Text;
            if (filtro == "Todos" || filtro == "Año")
            {
                ConsultarEnvios();
            }
            else
            {
                FiltroPorFecha(filtro);
            }
        }

        private void comboFiltroComite_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFecha.Text;
            if (filtro == "Todos" || filtro == "Comite")
            {
                ConsultarEnvios();
            }
            else
            {
                FiltroPorComite(filtro);
            }
        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.Show();
        }
    }
}
