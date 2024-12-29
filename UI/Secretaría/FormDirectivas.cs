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
    public partial class FormDirectivas : Form
    {

        RutasTxtService rutasTxtService = new RutasTxtService();
        DirectivaService directivaService;
        List<Directiva> directivas;
        Directiva directiva;
        DirectivesMaps directivesMaps;
        List<DirectivesData> directives;
        string id = "";
        bool encontrado = false;
        public FormDirectivas()
        {
            directivaService = new DirectivaService(DbConfig.ConnectionString);
            InitializeComponent();
            Inicializar();
        }
        private void Inicializar()
        {
            ConsultarYLlenarGridDeDirectivas();
            comboAño.Text = DateTime.Now.Year.ToString();
        }
        private async void ConsultarYLlenarGridDeDirectivas()
        {
            try
            {
                var db = FirebaseService.Database;
                var directivesQuery = db.Collection("DirectivesData");
                var directives = new List<DirectivesData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await directivesQuery.GetSnapshotAsync();
                directives = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<DirectivesData>()).ToList();
                if (directives.Count > 0)
                {
                    dataGridDirectiva.DataSource = null;
                    dataGridDirectiva.DataSource = directives;
                    textTotal.Text = directives.Count.ToString();
                }
                else
                {
                    dataGridDirectiva.DataSource = null;
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
                ConsultaDirectivaRespuesta respuesta = new ConsultaDirectivaRespuesta();
                string tipo = comboDirectiva.Text;
                if (tipo == "Directiva" || tipo == "Todos")
                {
                    textTotal.Enabled = true;
                    dataGridDirectiva.DataSource = null;
                    respuesta = directivaService.ConsultarTodos();
                    directivas = respuesta.Directivas.ToList();
                    if (respuesta.Directivas.Count != 0 && respuesta.Directivas != null)
                    {
                        dataGridDirectiva.DataSource = respuesta.Directivas;
                        Borrar.Visible = true;
                        textTotal.Text = directivaService.Totalizar().Cuenta.ToString();
                    }
                }
            }
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGestionarDirectivas_Click(object sender, EventArgs e)
        {
            tabDirectivas.SelectedIndex = 1;
        }
        private void LimpiarCampos()
        {
            comboAño.Text = DateTime.Now.Year.ToString();
            comboFiltroComite.Text = "Labores";
            textNombre.Text = "Nombre";
            comboCargo.Text = "Presidente";
            textObservacion.Text = "Observacion";
        }
        private Directiva MapearDatosDirectiva()
        {
            directiva = new Directiva();
            directiva.IdDirectiva = id;
            directiva.Vigencia = comboAño.Text;
            directiva.Comite = comboFiltroComite.Text;
            directiva.Nombre = textNombre.Text;
            directiva.Cargo = comboCargo.Text;
            directiva.Observacion = textObservacion.Text;
            return directiva;
        }
        private async void FiltroPorAño(string filtro)
        {
            try
            {
                var directivasPorAño = new List<DirectivesData>();
                var db = FirebaseService.Database;
                var ingresosQuery = db.Collection("DirectivesData");
                directives = new List<DirectivesData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await ingresosQuery.GetSnapshotAsync();
                directives = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<DirectivesData>()).ToList();
                for (int i = 0; i < directives.Count; i++)
                {
                    if (directives[i].Vigencia.Contains(filtro))
                    {
                        directivasPorAño.Add(directives[i]);
                    }
                }
                dataGridDirectiva.DataSource = directivasPorAño;
                textTotal.Text = directivasPorAño.Count.ToString();
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
            }
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            if(textNombre.Text!="Nombre" && textObservacion.Text != "Observacion" && textNombre.Text != "" && textObservacion.Text != "")
            {
                Directiva directiva = MapearDatosDirectiva();
                try
                {
                    //Guardamos la notas
                    var db = FirebaseService.Database;
                    Google.Cloud.Firestore.DocumentReference docRef;
                    var directivaNueva = directivesMaps.DirectivesMap(directiva);
                    docRef = db.Collection("DirectivesData").Document(directivaNueva.IdDirectiva.ToString());
                    docRef.SetAsync(directivaNueva);
                    //string mensaje = directivaService.Guardar(directiva);
                    MessageBox.Show("Se ha registrado el nuevo cargo correctamente", "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeDirectivas();
                    LimpiarCampos();
                    tabDirectivas.SelectedIndex = 0;
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
                    string mensaje = directivaService.Guardar(directiva);
                    MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeDirectivas();
                    LimpiarCampos();
                    tabDirectivas.SelectedIndex = 0;
                }
            }
            else
            {
                string mensaje = "No se puede registrar no ha llenado ningun campo";
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
        }
        private void EliminarServidor(string id)
        {
            try
            {
                //string mensaje = directivaService.Eliminar(id);
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("DirectivesData").Document(id);
                docRef.DeleteAsync();
                MessageBox.Show("Se ha eliminado el registrado correctamente", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeDirectivas();
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
                string mensaje = directivaService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (textNombre.Text != "Nombre" && textObservacion.Text != "Observacion" && textNombre.Text != "" && textObservacion.Text != "")
            { 
                Directiva directiva = MapearDatosDirectiva();
                try
                {
                    //Guardamos la notas
                    var db = FirebaseService.Database;
                    Google.Cloud.Firestore.DocumentReference docRef;
                    var directivaNueva = directivesMaps.DirectivesMap(directiva);
                    docRef = db.Collection("DirectivesData").Document(directivaNueva.IdDirectiva.ToString());
                    docRef.SetAsync(directivaNueva);
                    //string mensaje = directivaService.Modificar(directiva);
                    MessageBox.Show("Se ha modificado este registro correctamente", "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    ConsultarYLlenarGridDeDirectivas();
                    LimpiarCampos();
                    tabDirectivas.SelectedIndex = 0;
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
                    string mensaje = directivaService.Modificar(directiva);
                    MessageBox.Show(mensaje, "Mensaje de campos", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    comboDirectiva.Text = "Directiva";
                    ConsultarYLlenarGridDeDirectivas();
                    LimpiarCampos();
                    tabDirectivas.SelectedIndex = 0;
                }
            }
            else
            {
                string mensaje = "No se puede Modifica los campos no estan completos";
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }
        }

        private void textNombre_Enter(object sender, EventArgs e)
        {
            if (textNombre.Text == "Nombre")
            {
                textNombre.Text = "";
            }
        }

        private void textNombre_Leave(object sender, EventArgs e)
        {
            if (textNombre.Text == "")
            {
                textNombre.Text = "Nombre";
            }
        }

        private void textObservacion_Enter(object sender, EventArgs e)
        {
            if (textObservacion.Text == "Observacion")
            {
                textObservacion.Text = "";
            }
        }

        private void textObservacion_Leave(object sender, EventArgs e)
        {
            if (textObservacion.Text == "")
            {
                textObservacion.Text = "Observacion";
            }
        }

        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = false;
            btnCloseSearchLibreta.Visible = true;
            textSerachLibreta.Visible = true;
        }

        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            btnCloseSearchLibreta.Visible = false;
            btSearchLibreta.Visible = true;
            textSerachLibreta.Visible = false;
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar por miembro";
            }
        }

        private void textSerachLibreta_Enter(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text != "Buscar por nombre")
            {
                textSerachLibreta.Text = "";
            }
        }

        private void textSerachLibreta_Leave(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text != "")
            {
                textSerachLibreta.Text = "Buscar por nombre";
            }
        }
        private void FiltroPorId(string id)
        {
            try
            {

            }catch(Exception ex)
            {
                BusquedaDirectivaRespuesta respuesta = new BusquedaDirectivaRespuesta();
                respuesta = directivaService.BuscarPorIdentificacion(id);
                var registro = respuesta.Directiva;
                if (registro != null)
                {
                    encontrado = true;
                    var directivas = new List<Directiva> { registro };
                    textNombre.Text = directivas[0].Nombre;
                    comboAño.Text = directivas[0].Vigencia;
                    comboDirectiva.Text = directivas[0].Comite;
                    comboCargo.Text = directivas[0].Cargo;
                    textObservacion.Text = directivas[0].Observacion;
                }
            }
        }

        private void dataGridDirectiva_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridDirectiva.DataSource != null)
            {
                if (dataGridDirectiva.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridDirectiva.CurrentRow.Cells["IdDirectiva"].Value.ToString());
                    EliminarServidor(id);
                    ConsultarYLlenarGridDeDirectivas();
                }
                else
                {
                    if (dataGridDirectiva.Columns[e.ColumnIndex].Name == "Editar")
                    {
                        id = Convert.ToString(dataGridDirectiva.CurrentRow.Cells["IdDirectiva"].Value.ToString());
                        FiltroPorId(id);
                        tabDirectivas.SelectedIndex = 1;
                        btnRegistrar.Enabled = false;
                    }
                }
            }
        }
        void FiltroPorNombre(string filtro)
        {
            try
            {

            }catch(Exception ex)
            {
                ConsultaDirectivaRespuesta respuesta = new ConsultaDirectivaRespuesta();
                respuesta = directivaService.BuscarPorNombre(filtro);
                var registro = respuesta.Directivas;
                if (registro != null)
                {
                    dataGridDirectiva.DataSource = null;
                    dataGridDirectiva.DataSource = registro;
                    encontrado = true;
                }
            }
        }
        private void textSerachLibreta_TextChanged(object sender, EventArgs e)
        {
            var filtro = textSerachLibreta.Text;
            if (textSerachLibreta.Text != "" && textSerachLibreta.Text != "Buscar por miembro")
            {
                FiltroPorNombre(filtro);
                if (encontrado == false)
                {
                    dataGridDirectiva.CurrentCell = null;
                    foreach (DataGridViewRow fila in dataGridDirectiva.Rows)
                    {
                        fila.Visible = false;
                    };
                    foreach (DataGridViewRow fila in dataGridDirectiva.Rows)
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
            }
            else
            {
                ConsultarYLlenarGridDeDirectivas();
            }
        }
        private async void FiltroPorDirectiva(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var directivasQuery = db.Collection("DirectivesData");
                var directivas = new List<DirectivesData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await directivasQuery.GetSnapshotAsync();
                directivas = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<DirectivesData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var directivasComite = directivas.Where(miembro => miembro.Comite == filtro).ToList();
                dataGridDirectiva.DataSource = directivasComite;
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
                ConsultaDirectivaRespuesta respuesta = new ConsultaDirectivaRespuesta();
                respuesta = directivaService.BuscarPorDirectiva(filtro);
                var registro = respuesta.Directivas;
                if (registro != null)
                {
                    dataGridDirectiva.DataSource = null;
                    dataGridDirectiva.DataSource = registro;
                    encontrado = true;
                }
                else
                {
                    dataGridDirectiva.DataSource = null;
                }
            }
        }
        private void comboDirectiva_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filtro = comboDirectiva.Text;
            if (comboDirectiva.Text != "" && comboDirectiva.Text != "Todos")
            {
                FiltroPorDirectiva(filtro);
            }
            else
            {
                ConsultarYLlenarGridDeDirectivas();
            }
        }

        private void btnGenerarInforme_Click(object sender, EventArgs e)
        {
            FormGenerarDocumento formGenerarDocumento = new FormGenerarDocumento();
            formGenerarDocumento.Show();
        }

        private void FormDirectivas_Load(object sender, EventArgs e)
        {
            comboAño.Text = DateTime.Now.Year.ToString();
        }

        private void comboFiltroAño_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFiltroAño.Text;
            if (filtro == "Todos" || filtro == "")
            {
                ConsultarYLlenarGridDeDirectivas();
            }
            else
            {
                comboFiltroComite.Text = "Comite";
                FiltroPorAño(filtro);
            }
        }
    }
}
