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
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using System.Windows.Markup;
using System.Diagnostics;
using WhatsAppApi;
using System.Windows.Controls;
using System.Net.Http.Headers;
using System.Net.Http;
using Cloud;
using Application = System.Windows.Forms.Application;
using Ldb;

namespace UI
{
    public partial class FormDirectorio : Form
    {
        RutasTxtService rutasTxtService = new RutasTxtService();
        ContactoService contactoService;
        List<Contacto> contactos;
        Contacto contacto;
        ContactMaps contactMaps;
        string id = "";
        bool encontrado = false;
        public FormDirectorio()
        {
            contactoService = new ContactoService(DbConfig.ConnectionString);
            InitializeComponent();
            Inicializar();
        }
        private void Inicializar()
        {
            ConsultarYLlenarGridDeContactos();
        }
        private async void ConsultarYLlenarGridDeContactos()
        {
            try
            {
                var db = FirebaseService.Database;
                var contactQuery = db.Collection("ContactsData");
                var contacts = new List<ContactData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await contactQuery.GetSnapshotAsync();
                contacts = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ContactData>()).ToList();
                if (contacts.Count > 0)
                {
                    dataGridContactos.DataSource = null;
                    dataGridContactos.DataSource = contacts;
                    textTotal.Text = contacts.Count.ToString();
                }
                else
                {
                    dataGridContactos.DataSource = null;
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
                ConsultaContactoRespuesta respuesta = new ConsultaContactoRespuesta();
                string tipo = comboOficioLibreta.Text;
                if (tipo == "Oficio" || tipo == "Todos")
                {
                    textTotal.Enabled = true;
                    dataGridContactos.DataSource = null;
                    respuesta = contactoService.ConsultarTodos();
                    contactos = respuesta.Contactos.ToList();
                    if (respuesta.Contactos.Count != 0 && respuesta.Contactos != null)
                    {
                        dataGridContactos.DataSource = respuesta.Contactos;
                        Borrar.Visible = true;
                        textTotal.Text = contactoService.Totalizar().Cuenta.ToString();
                    }
                }
            }
        }
        void LimpiarCampos()
        {
            textNombre.Text= "Nombre";
            textApellido.Text = "Apellido";
            textCelular.Text = "Celular";
            textNumeroWhatsapp.Text = "Numero de whatsapp";
            comboOficioRegistrar.Text="Oficio";
        }
        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            btSearchLibreta.Visible = false;
            btnCloseSearchLibreta.Visible = true;
            textSerachLibreta.Visible = true;
        }
        void FiltroPorId(string id)
        {
            BusquedaContactoRespuesta respuesta = new BusquedaContactoRespuesta();
            respuesta = contactoService.BuscarPorIdentificacion(id);
            var registro = respuesta.Contacto;
            if (registro != null)
            {
                encontrado = true;
                var contactos = new List<Contacto> { registro };
                textNombre.Text = contactos[0].Nombre;
                textApellido.Text= contactos[0].Apellido;
                textCelular.Text= contactos[0].TelefonoContacto;
                textNumeroWhatsapp.Text= contactos[0].TelefonoWhatsapp;
                comboOficioRegistrar.Text= contactos[0].Oficio;
            }
        }
        void FiltroPorNombre(string filtro)
        {
            BusquedaContactoRespuesta respuesta = new BusquedaContactoRespuesta();
            respuesta = contactoService.BuscarPorNombre(filtro);
            var registro = respuesta.Contacto;
            if (registro != null)
            {
                dataGridContactos.DataSource = null;
                var contactos = new List<Contacto> { registro };
                dataGridContactos.DataSource = contactos;
                encontrado = true;
            } 
        }
        private async void FiltroPorOficio(string filtro)
        {
            try
            {
                var db = FirebaseService.Database;
                var contactsQuery = db.Collection("ContactsData");
                var contactos = new List<ContactData>();
                // Realizar la suma directamente en la consulta Firestore
                var snapshot = await contactsQuery.GetSnapshotAsync();
                contactos = snapshot.Documents.Select(docsnap => docsnap.ConvertTo<ContactData>()).ToList();
                // Filtrar elementos según el campo Valor y la variable id
                var contactosOficio = contactos.Where(contacto => contacto.Oficio == filtro).ToList();
                dataGridContactos.DataSource = contactosOficio;
                textTotal.Text = contactosOficio.Count.ToString();
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
                BusquedaContactoRespuesta respuesta = new BusquedaContactoRespuesta();
                respuesta = contactoService.BuscarPorOficio(filtro);
                var registro = respuesta.Contacto;
                if (registro != null)
                {
                    dataGridContactos.DataSource = null;
                    var contactos = new List<Contacto> { registro };
                    dataGridContactos.DataSource = contactos;
                }
                else
                {
                    dataGridContactos.DataSource = null;
                }
            }
        }

        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            btnCloseSearchLibreta.Visible = false;
            btSearchLibreta.Visible = true;
            textSerachLibreta.Visible = false;
            if (textSerachLibreta.Text != "")
            {
                textSerachLibreta.Text = "Buscar";
            }
        }

        private void textNombre_Enter(object sender, EventArgs e)
        {
            if(textNombre.Text == "Nombre")
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

        private void textApellido_Enter(object sender, EventArgs e)
        {
            if (textApellido.Text == "Apellido")
            {
                textApellido.Text = "";
            }
        }

        private void textApellido_Leave(object sender, EventArgs e)
        {
            if (textApellido.Text == "")
            {
                textApellido.Text = "Apellido";
            }
        }

        private void textCelular_Enter(object sender, EventArgs e)
        {
            if (textCelular.Text == "Celular")
            {
                textCelular.Text = "";
            }
        }

        private void textCelular_Leave(object sender, EventArgs e)
        {
            if (textCelular.Text == "")
            {
                textCelular.Text = "Celular";
            }
        }

        private void textNumeroWhatsapp_Enter(object sender, EventArgs e)
        {
            if (textNumeroWhatsapp.Text == "Numero de whatsapp")
            {
                textNumeroWhatsapp.Text = "";
            }
        }

        private void textNumeroWhatsapp_Leave(object sender, EventArgs e)
        {
            if (textNumeroWhatsapp.Text == "")
            {
                textNumeroWhatsapp.Text = "Numero de whatsapp";
            }
        }

        private void comboOficio_Enter(object sender, EventArgs e)
        {
            if (comboOficioRegistrar.Text == "Oficio")
            {
                comboOficioRegistrar.Text = "";
            }
        }

        private void comboOficio_Leave(object sender, EventArgs e)
        {
            if (comboOficioRegistrar.Text == "")
            {
                comboOficioRegistrar.Text = "Oficio";
            }
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

        private void btnGestionarContactos_Click(object sender, EventArgs e)
        {
            tabDirectorio.SelectedIndex = 1;
        }
        private Contacto MapearDatosContacto()
        {
            contacto = new Contacto();
            contacto.IdContacto = id;
            contacto.Nombre = textNombre.Text;
            contacto.Apellido = textApellido.Text;
            contacto.TelefonoContacto = textCelular.Text;
            contacto.TelefonoWhatsapp = textNumeroWhatsapp.Text;
            contacto.Oficio = comboOficioRegistrar.Text;
            return contacto;
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            Contacto contacto = MapearDatosContacto();
            try
            {
                //Guardamos la notas
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef;
                var contactoNueva = contactMaps.ContactMap(contacto);
                docRef = db.Collection("NotesData").Document(contactoNueva.IdContacto.ToString());
                docRef.SetAsync(contactoNueva);
                string mensaje = contactoService.Guardar(contacto);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeContactos();
                LimpiarCampos();
                tabDirectorio.SelectedIndex = 0;
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
                string mensaje = contactoService.Guardar(contacto);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeContactos();
                LimpiarCampos();
                tabDirectorio.SelectedIndex = 0;
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            Contacto contacto = MapearDatosContacto();
            try
            {
                //Guardamos el contacto
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef;
                var contactoNuevo = contactMaps.ContactMap(contacto);
                docRef = db.Collection("NotesData").Document(contactoNuevo.IdContacto.ToString());
                docRef.SetAsync(contactoNuevo);
                string mensaje = contactoService.Guardar(contacto);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeContactos();
                LimpiarCampos();
                tabDirectorio.SelectedIndex = 0;
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
                string mensaje = contactoService.Modificar(contacto);
                MessageBox.Show(mensaje, "Mensaje de campos", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeContactos();
                LimpiarCampos();
                tabDirectorio.SelectedIndex = 0;
            }
        }

        private void textCelular_TextChanged(object sender, EventArgs e)
        {
            if (textCelular.Text != "")
            {
                textNumeroWhatsapp.Text = "+57"+textCelular.Text;
            }
        }

        private void textNumeroWhatsapp_TextChanged(object sender, EventArgs e)
        {
            //if (textNumeroWhatsapp.Text.StartsWith("+57"))
            //{
            //    textCelular.Text = textNumeroWhatsapp.Text.Substring(3);
            //}
        }

        private void textSerachLibreta_TextChanged(object sender, EventArgs e)
        {
            var filtro = textSerachLibreta.Text;
            if (textSerachLibreta.Text != "" && textSerachLibreta.Text != "Buscar por nombre")
            {
                FiltroPorNombre(filtro);
                if (encontrado == false)
                {
                    dataGridContactos.CurrentCell = null;
                    foreach (DataGridViewRow fila in dataGridContactos.Rows)
                    {
                        fila.Visible = false;
                    };
                    foreach (DataGridViewRow fila in dataGridContactos.Rows)
                    {
                        int i = 0;
                        foreach (DataGridViewCell celda in fila.Cells)
                        {
                            if (i >= 5 && i<=6)
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
                ConsultarYLlenarGridDeContactos();
            }
        }
        void EliminarContacto(string id)
        {
            try
            {
                string mensaje = contactoService.Eliminar(id);
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("ContactsData").Document(id);
                docRef.DeleteAsync();
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarYLlenarGridDeContactos();
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
                string mensaje = contactoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void comboOficioLibreta_SelectedIndexChanged(object sender, EventArgs e)
        {
            var filtro = comboOficioLibreta.Text;
            if (comboOficioLibreta.Text != "" && comboOficioLibreta.Text != "Todos")
            {
                FiltroPorOficio(filtro);
            }
            else
            {
                ConsultarYLlenarGridDeContactos();
            }
        }

        private void comboOficioLibreta_TextChanged(object sender, EventArgs e)
        {
            var filtro = comboOficioLibreta.Text;
            if (comboOficioLibreta.Text != "")
            {
                FiltroPorOficio(filtro);
            }
        }
        async Task enviarMensajeAsync(string celular)
        {
            //Token
            string token = "EAAcZBA5ZAs09QBO5TC6HJrcI56BJVBSB3uyiYbguTYpjf67lL9cDczXSsYI4BiROHw2n9BIpp0nBuVmuvNv0AZCzIlIKEcwNZAnlD8AmZBf7dnIZAmPDmzxu3zbY2MGZAVuB9FLFn42UM8Cz3xG3Af3RAZBbHCdVADFF3vGgL7hhpX1ahQmCUFXFqaUC96fucu9GeAemsevGyAeRUSuEKo8ZD";
            //Identificador de número de teléfono
            string idTelefono = "115288798221114";
            //Nuestro telefono
            string telefono = $"57{celular}";
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://graph.facebook.com/v15.0/" + idTelefono + "/messages");
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Content = new StringContent("{ \"messaging_product\": \"whatsapp\", \"to\": \"" + telefono + "\", \"type\": \"template\", \"template\": { \"name\": \"hello_world\", \"language\": { \"code\": \"en_US\" } } }");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
        }
        private void dataGridContactos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var telefono = "";
            if (dataGridContactos.DataSource != null)
            {
                if (dataGridContactos.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridContactos.CurrentRow.Cells["IdContacto"].Value.ToString());
                    telefono = Convert.ToString(dataGridContactos.CurrentRow.Cells["TelefonoContacto"].Value.ToString());
                    EliminarContacto(id);
                    ConsultarYLlenarGridDeContactos();
                }
                else
                {
                    if (dataGridContactos.Columns[e.ColumnIndex].Name == "Whatsapp")
                    {
                        telefono = Convert.ToString(dataGridContactos.CurrentRow.Cells["TelefonoContacto"].Value.ToString());
                        enviarMensajeAsync(telefono);
                    }
                    else
                    {
                        if (dataGridContactos.Columns[e.ColumnIndex].Name == "Editar")
                        {
                            id = Convert.ToString(dataGridContactos.CurrentRow.Cells["IdContacto"].Value.ToString());
                            FiltroPorId(id);
                            tabDirectorio.SelectedIndex = 1;
                            btnRegistrar.Enabled = false;
                        }
                    }
                }
            }
        }
    }
}
