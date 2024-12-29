using BLL;
using Cloud;
using Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ldb;

namespace UI
{
    public partial class FormReuniones : Form
    {
        RutasTxtService rutasTxtService = new RutasTxtService();
        ReunionService reunionService;
        List<Reunion> reuniones;
        Reunion reunion;
        MeetingsMaps meetingsMaps;
        bool encontrado = false;
        string id = "";
        string reunionId = "";
        public FormReuniones()
        {
            reunionService = new ReunionService(DbConfig.ConnectionString);
            InitializeComponent();
            Inicializar();
        }
        private void Inicializar()
        {
            Calcularreunion();
            ConsultarYLlenarGridDeReuniones();
        }
        private void Calcularreunion()
        {
            ConsultaReunionRespuesta respuesta = new ConsultaReunionRespuesta();
            respuesta = reunionService.ConsultarTodos();
            reuniones = respuesta.Reuniones.ToList();
            if (respuesta.Reuniones.Count != 0 && respuesta.Reuniones != null)
            {
                textTotal.Text = reunionService.Totalizar().Cuenta.ToString();
                var totalFolio = Convert.ToInt32(textTotal.Text);
                reunionId = (totalFolio + 1).ToString("0000");
                labelNumeroActa.Text = reunionId;
            }
            else
            {
                var totalFolio = 1;
                reunionId = totalFolio.ToString("0000");
                labelNumeroActa.Text = reunionId;
            }
        }
        private async void ConsultarYLlenarGridDeReuniones()
        {
            try
            {
                var db = FirebaseService.Database;
                var meetQuery = db.Collection("MeetingsData");
                var meets = new List<MeetingsData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await meetQuery.GetSnapshotAsync();
                meets = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<MeetingsData>()).ToList();
                if (meets.Count > 0)
                {
                    dataGridReunion.DataSource = null;
                    dataGridReunion.DataSource = meets;
                    textTotal.Text = meets.Count.ToString();
                }
                else
                {
                    dataGridReunion.DataSource = null;
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
                ConsultaReunionRespuesta respuesta = new ConsultaReunionRespuesta();
                textTotal.Enabled = true;
                dataGridReunion.DataSource = null;
                respuesta = reunionService.ConsultarTodos();
                reuniones = respuesta.Reuniones.ToList();
                if (respuesta.Reuniones.Count != 0 && respuesta.Reuniones != null)
                {
                    dataGridReunion.DataSource = respuesta.Reuniones;
                    Borrar.Visible = true;
                    textTotal.Text = reunionService.Totalizar().Cuenta.ToString();
                }
            }
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textSerachLibreta_Enter(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "Buscar")
            {
                textSerachLibreta.Text = "";
            }
        }

        private void textSerachLibreta_Leave(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar";
            }
        }
        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = false;
            textSerachLibreta.Visible = true;
            btnCloseSearchLibreta.Visible = true;
        }

        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = true;
            textSerachLibreta.Visible = false;
            btnCloseSearchLibreta.Visible = false;
        }

        private void textActa_Enter(object sender, EventArgs e)
        {
            if (textActa.Text == "Texto")
            {
                textActa.Text = "";
            }
        }

        private void textActa_Leave(object sender, EventArgs e)
        {
            if (textActa.Text == "")
            {
                textActa.Text = "Texto";
            }
        }

        private void textLugarReunion_Enter(object sender, EventArgs e)
        {
            if (textLugarReunion.Text == "Lugar")
            {
                textLugarReunion.Text = "";
            }
        }

        private void textLugarReunion_Leave(object sender, EventArgs e)
        {
            if (textLugarReunion.Text == "")
            {
                textLugarReunion.Text = "Lugar";
            }
        }

        private void textOrdenDja_Enter(object sender, EventArgs e)
        {
            if (textOrdenDja.Text == "Texto")
            {
                textOrdenDja.Text = "";
            }
        }

        private void textOrdenDja_Leave(object sender, EventArgs e)
        {
            if (textLugarReunion.Text == "")
            {
                textLugarReunion.Text = "Texto";
            }
        }

        private void btnGestionarReuniones_Click(object sender, EventArgs e)
        {
            tabReuniones.SelectedIndex = 1;
        }
        private Reunion MapearDatosReunion()
        {
            reunion = new Reunion();
            reunion.NumeroActa = labelNumeroActa.Text;
            reunion.FechaDeReunion = dateFechaDeReunion.Value;
            reunion.LugarDeReunion = textLugarReunion.Text;
            reunion.OrdenDelDia = textOrdenDja.Text;
            reunion.TextoActa = textActa.Text;
            return reunion;
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if(textLugarReunion.Text!="" && textOrdenDja.Text != "" && textActa.Text!="")
            {
                Reunion reunion = MapearDatosReunion();
                try
                {
                    //Guardamos la notas
                    var db = FirebaseService.Database;
                    Google.Cloud.Firestore.DocumentReference docRef;
                    var reunionNueva = meetingsMaps.MeetMap(reunion);
                    docRef = db.Collection("MeetingsData").Document(reunionNueva.NumeroActa.ToString());
                    docRef.SetAsync(reunionNueva);
                    string mensaje = reunionService.Guardar(reunion);
                    MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeReuniones();
                    Limpiar();
                    tabReuniones.SelectedIndex = 0;
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
                    string mensaje = reunionService.Guardar(reunion);
                    MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeReuniones();
                    Limpiar();
                    tabReuniones.SelectedIndex = 0;
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Reunion nuevaReunion = MapearDatosReunion();
            try
            {
                //Guardamos la notas
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef;
                var reunionNueva = meetingsMaps.MeetMap(reunion);
                docRef = db.Collection("MeetingsData").Document(reunionNueva.NumeroActa.ToString());
                docRef.SetAsync(reunionNueva);
                string mensaje = reunionService.Guardar(reunion);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeReuniones();
                Limpiar();
                tabReuniones.SelectedIndex = 0;
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
                string mensaje = reunionService.Modificar(nuevaReunion);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeReuniones();
                Limpiar();
                tabReuniones.SelectedIndex = 0;
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {

        }

        private void btnImprimirLista_Click(object sender, EventArgs e)
        {

        }

        private void FiltroPorId(string id)
        {
            BusquedaReunionRespuesta respuesta = new BusquedaReunionRespuesta();
            respuesta = reunionService.BuscarPorIdentificacion(id);
            var registro = respuesta.Reunion;
            if (registro != null)
            {
                encontrado = true;
                var directivas = new List<Reunion> { registro };
                labelNumeroActa.Text = registro.NumeroActa;
                dateFechaDeReunion.Value = registro.FechaDeReunion;
                textLugarReunion.Text = registro.LugarDeReunion;
                textOrdenDja.Text = registro.OrdenDelDia;
                textActa.Text = registro.TextoActa;
            }
        }
        private void Limpiar()
        {
            labelNumeroActa.Text = "*";
            dateFechaDeReunion.Value = DateTime.Now;
            textLugarReunion.Text = "";
            textOrdenDja.Text = "";
            textActa.Text = "";
        }
        private void EliminarPorId(string id)
        {
            try
            {
                string mensaje = reunionService.Eliminar(id);
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("MeetingsData").Document(id);
                docRef.DeleteAsync();
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeReuniones();
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
                string mensaje = reunionService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textTotal.Text = "0";
            }
        }
        private void dataGridReunion_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridReunion.DataSource != null)
            {
                if (dataGridReunion.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridReunion.CurrentRow.Cells["NumeroActa"].Value.ToString());
                    EliminarPorId(id);
                    ConsultarYLlenarGridDeReuniones();
                }
                else
                {
                    if (dataGridReunion.Columns[e.ColumnIndex].Name == "Editar")
                    {
                        id = Convert.ToString(dataGridReunion.CurrentRow.Cells["NumeroActa"].Value.ToString());
                        FiltroPorId(id);
                        tabReuniones.SelectedIndex = 1;
                        btnRegistrar.Enabled = false;
                    }
                }
            }
        }

        private void textSerachLibreta_TextChanged(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text != "" && textSerachLibreta.Text != "Buscar por miembro")
            {
                dataGridReunion.CurrentCell = null;
                foreach (DataGridViewRow fila in dataGridReunion.Rows)
                {
                    fila.Visible = false;
                };
                foreach (DataGridViewRow fila in dataGridReunion.Rows)
                {
                    int i = 0;
                    foreach (DataGridViewCell celda in fila.Cells)
                    {
                        if (i == 4)
                        {
                            if ((celda.Value.ToString().ToUpper()).IndexOf(textSerachLibreta.Text.ToUpper()) == 0)
                            {
                                fila.Visible = true;
                                break;
                            }
                            else
                            {
                                if ((celda.Value.ToString() == (textSerachLibreta.Text.ToUpper())))
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
            else
            {
                ConsultarYLlenarGridDeReuniones();
            }
        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.Show();
        }
    }
}
