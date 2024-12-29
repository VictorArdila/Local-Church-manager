using BLL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Entity;
using FirebaseAdmin.Messaging;
using Cloud;
using Ldb;

namespace UI
{
    public partial class FormApuntes : Form
    {
        RutasTxtService rutasTxtService = new RutasTxtService();
        ApunteService apunteService;
        List<Apunte> apuntes;
        Apunte apunte;
        NoteMaps noteMaps;
        string id = "";
        bool encontrado = false;
        public FormApuntes()
        {
            apunteService = new ApunteService(DbConfig.ConnectionString);
            InitializeComponent();
            Inicializar();
        }
        private void Inicializar()
        {
            ConsultarYLlenarGridDeApuntes();
        }
        private async void ConsultarYLlenarGridDeApuntes()
        {
            try
            {
                var db = FirebaseService.Database;
                var notesQuery = db.Collection("NotesData");
                var apuntes = new List<NotesData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await notesQuery.GetSnapshotAsync();
                apuntes = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<NotesData>()).ToList();
                if (apuntes.Count > 0)
                {
                    dataGridApunte.DataSource = null;
                    dataGridApunte.DataSource = apuntes;
                    textTotal.Text = apuntes.Count.ToString();
                }
                else
                {
                    dataGridApunte.DataSource = null;
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
                ConsultaApunteRespuesta respuesta = new ConsultaApunteRespuesta();
                textTotal.Enabled = true;
                dataGridApunte.DataSource = null;
                respuesta = apunteService.ConsultarTodos();
                apuntes = respuesta.Apuntes.ToList();
                if (respuesta.Apuntes.Count != 0 && respuesta.Apuntes != null)
                {
                    dataGridApunte.DataSource = respuesta.Apuntes;
                    Borrar.Visible = true;
                    textTotal.Text = apunteService.Totalizar().Cuenta.ToString();
                }
            }
        }
        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible=false;
            textSerachLibreta.Visible = true;
            btnCloseSearchLibreta.Visible = true;
        }

        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = true;
            textSerachLibreta.Visible = false;
            btnCloseSearchLibreta.Visible = false;
        }
        private void textTitulo_Enter(object sender, EventArgs e)
        {
            if (textTitulo.Text == "Titulo")
            {
                textTitulo.Text = "";
            }
        }

        private void textTitulo_Leave(object sender, EventArgs e)
        {
            if (textTitulo.Text == "")
            {
                textTitulo.Text = "Titulo";
            }
        }

        private void textNota_Enter(object sender, EventArgs e)
        {
            if (textNota.Text == "Redactar")
            {
                textNota.Text = "";
            }
        }

        private void textNota_Leave(object sender, EventArgs e)
        {
            if (textNota.Text == "")
            {
                textNota.Text = "Redactar";
            }
        }

        private void textSerachLibreta_Enter(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "Buscar por titulo")
            {
                textSerachLibreta.Text = "";
            }
        }

        private void textSerachLibreta_Leave(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar por titulo";
            }
        }


        private void btnGestionarContactos_Click(object sender, EventArgs e)
        {
            tabApuntes.SelectedIndex = 1;
        }

        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            tabApuntes.SelectedIndex = 0;
        }

        private void btnModificar_Click(object sender, EventArgs e)
        { 
            tabApuntes.SelectedIndex = 0;
        }

        private void textSerachLibreta_TextChanged(object sender, EventArgs e)
        {
            var filtro = textSerachLibreta.Text;
            if (textSerachLibreta.Text != "" && textSerachLibreta.Text != "Buscar por titulo por nombre")
            {
                dataGridApunte.CurrentCell = null;
                foreach (DataGridViewRow fila in dataGridApunte.Rows)
                {
                    fila.Visible = false;
                };
                foreach (DataGridViewRow fila in dataGridApunte.Rows)
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
                        else
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
                        }
                        i = i + 1;
                    }
                }

            }
            else
            {
                ConsultarYLlenarGridDeApuntes();
            }
        }
        private void LimpiarCampos()
        {
            textTitulo.Text = "";
            textNota.Text = "";
        }
        private Apunte MapearApunte()
        {
            apunte = new Apunte();
            apunte.IdNota = id;
            apunte.Titulo = textTitulo.Text;
            apunte.Nota = textNota.Text;
            return apunte;
        }
        private void btnRegistrar_Click_1(object sender, EventArgs e)
        {
            Apunte apunte = MapearApunte();
            try
            {
                //Guardamos la notas
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef;
                var notaNueva = noteMaps.NoteMap(apunte);
                docRef = db.Collection("NotesData").Document(notaNueva.IdNota.ToString());
                docRef.SetAsync(notaNueva);
                string mensaje = apunteService.Guardar(apunte);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeApuntes();
                LimpiarCampos();
                tabApuntes.SelectedIndex = 0;
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
                string mensaje = apunteService.Guardar(apunte);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeApuntes();
                LimpiarCampos();
                tabApuntes.SelectedIndex = 0;
            }
        }

        private void btnModificar_Click_1(object sender, EventArgs e)
        {
            Apunte nuevoApunte = MapearApunte();
            try
            {
                //Guardamos la nota
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef;
                var notaNueva = noteMaps.NoteMap(apunte);
                docRef = db.Collection("NotesData").Document(notaNueva.IdNota.ToString());
                docRef.SetAsync(notaNueva);
                string mensaje = apunteService.Guardar(apunte);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeApuntes();
                LimpiarCampos();
                tabApuntes.SelectedIndex = 0;
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
                string mensaje = apunteService.Modificar(nuevoApunte);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeApuntes();
                LimpiarCampos();
                tabApuntes.SelectedIndex = 0;
            }
        }
        private void FiltroPorId(string id)
        {
            BusquedaApunteRespuesta respuesta = new BusquedaApunteRespuesta();
            respuesta = apunteService.BuscarPorIdentificacion(id);
            var registro = respuesta.Apunte;
            if (registro != null)
            {
                encontrado = true;
                var apuntes = new List<Apunte> { registro };
                textTitulo.Text = apuntes[0].Titulo;
                textNota.Text = apuntes[0].Nota;
            }
        }
        private void EliminarApunte(string id)
        {
            try
            {
                string mensaje = apunteService.Eliminar(id);
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("MembersData").Document(id);
                docRef.DeleteAsync();
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeApuntes();
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
                string mensaje = apunteService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void dataGridApunte_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridApunte.DataSource != null)
            {
                if (dataGridApunte.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridApunte.CurrentRow.Cells["IdNota"].Value.ToString());
                    EliminarApunte(id);
                    ConsultarYLlenarGridDeApuntes();
                }
                else
                {
                    if (dataGridApunte.Columns[e.ColumnIndex].Name == "Editar")
                    {
                        id = Convert.ToString(dataGridApunte.CurrentRow.Cells["IdNota"].Value.ToString());
                        FiltroPorId(id);
                        tabApuntes.SelectedIndex = 1;
                        btnRegistrar.Enabled = false;
                    }
                }
            }
        }
    }
}
