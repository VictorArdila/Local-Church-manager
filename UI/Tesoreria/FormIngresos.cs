﻿using BLL;
using Cloud;
using Entity;
using Google.Cloud.Firestore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;

namespace UI
{
    public partial class FormIngresos : Form
    {
        Ingreso ingreso;
        IngresoService ingresoService;
        List<Ingreso> ingresos;
        IngressMaps ingressMaps;
        TabPage tabPage;
        string originalText;
        string comprobante = "";
        string id;
        string comite;
        bool encontrado = false;
        bool detallo = false;
        public FormIngresos()
        {
            ingresoService = new IngresoService(ConfigConnection.ConnectionString);
            ingressMaps = new IngressMaps();
            InitializeComponent();
            ConsultarIngresos();
            CalcularComprobante();
        }

        private void btnAtras_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGestionarIngresos_Click_1(object sender, EventArgs e)
        {
            tabLibroIngresos.SelectedIndex = 1;
        }
        private string LecturaCifra(int totalDeIngresos)
        {
            // Convierte el total de ingresos a una cadena con separadores de miles
            string cifraFormateada = totalDeIngresos.ToString("N0");

            // Muestra la cifra formateada en el TextBox o donde desees
            string valorFormateado = $"${cifraFormateada}";
            return valorFormateado;
        }
        private async void TotalizarIngresos()
        {
            int sumIngreso = 0;
            try
            {
                var db = FirebaseService.Database;
                var ingresos = new List<IngressData>();
                Google.Cloud.Firestore.Query ingressQuery = db.Collection("IngressData");
                QuerySnapshot snap = await ingressQuery.GetSnapshotAsync();
                foreach (DocumentSnapshot docsnap in snap.Documents)
                {
                    IngressData ingressData = docsnap.ConvertTo<IngressData>();
                    sumIngreso = sumIngreso + ingressData.Valor;
                    if (sumIngreso > 0)
                    {
                        textTotalIngresos.Text = LecturaCifra(sumIngreso);
                    }
                }
            }
            catch(Exception ex)
            {
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.ConsultarTodos();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    ingresos = respuesta.Ingresos.ToList();
                    for(int i=0; i< ingresos.Count; i++)
                    {
                        sumIngreso = sumIngreso + respuesta.Ingresos[i].Valor;
                    }
                }
            }
        }
        private async void TotalizarRegistros()
        {
            ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
            respuesta = ingresoService.ConsultarTodos();
            if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
            {
                dataGridIngresos.DataSource = null;
                ingresos = respuesta.Ingresos.ToList();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    try
                    {
                        var db = FirebaseService.Database;
                        var ingresos = new List<IngressData>();
                        Google.Cloud.Firestore.Query ingressQuery = db.Collection("IngressData");
                        QuerySnapshot snap = await ingressQuery.GetSnapshotAsync();
                        foreach (DocumentSnapshot docsnap in snap.Documents)
                        {
                            IngressData ingressData = docsnap.ConvertTo<IngressData>();
                            ingresos.Add(ingressData);
                            textTotalNube.Text = ingresos.Count.ToString();
                        }
                        textTotalLocal.Text = ingresoService.Totalizar().Cuenta.ToString();
                    }
                    catch (Exception ex)
                    {
                        textTotalLocal.Text = ingresoService.Totalizar().Cuenta.ToString();
                        textTotalNube.Text = "0";
                    }
                }
                else
                {
                    textTotalLocal.Text = "0";
                    textTotalNube.Text = "0";
                }
            }
        }
        private void FormIngresos_Load(object sender, EventArgs e)
        {
            if (tabLibroIngresos.TabCount > 0)
            {
                tabPage = tabLibroIngresos.TabPages["tabDetalle"];
                tabLibroIngresos.TabPages.RemoveAt(2);
            }
        }
        private void CalcularComprobante()
        {
            ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
            respuesta = ingresoService.ConsultarTodos();
            ingresos = respuesta.Ingresos.ToList();
            if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
            {
                List<int> comprobantes = new List<int>();
                string numeroComprobanteFinal = "";
                //for(int i=0; i < respuesta.Ingresos.Count; i++)
                //{

                //}
                for (int i = 0; i < respuesta.Ingresos.Count; i++)
                {
                    // Obtener el número del comprobante del ingreso actual
                    int numeroComprobanteActual = int.Parse(respuesta.Ingresos[i].CodigoComprobante);

                    // Agregarlo a la lista de comprobantes
                    comprobantes.Add(numeroComprobanteActual);
                }

                // Verificar si hay elementos en la lista de comprobantes
                if (comprobantes.Count > 0)
                {
                    // Obtener el número mayor de la lista
                    int numeroMayor = comprobantes.Max();
                    numeroMayor = numeroMayor + 1;
                    // Asignar el número mayor a la variable final
                    // Supongamos que tienes una variable final llamada 'numeroComprobanteFinal'
                    numeroComprobanteFinal = numeroMayor.ToString("0000");
                    textNumeroComprobante.Text = numeroComprobanteFinal;
                }
            }
            else
            {
                var totalFolio = 1;
                comprobante = totalFolio.ToString("0000");
                textNumeroComprobante.Text = comprobante;
            }
        }
        private async void ConsultarIngresos()
        {
            TotalizarIngresos();
            TotalizarRegistros();
            try
            {
                bool isNotEmpty = false;
                var db = FirebaseService.Database;
                var ingresos = new List<IngressData>();
                Google.Cloud.Firestore.Query ingressQuery = db.Collection("IngressData");
                QuerySnapshot snap = await ingressQuery.GetSnapshotAsync();
                foreach(DocumentSnapshot docsnap in snap.Documents)
                {
                    IngressData ingressData = docsnap.ConvertTo<IngressData>();
                    ingresos.Add(ingressData);
                    if (ingresos.Count > 0)
                    {
                        isNotEmpty = true;
                    }
                }
                if (isNotEmpty)
                {
                    dataGridIngresos.DataSource = null;
                    dataGridIngresos.DataSource = ingresos;
                }
                else
                {
                    dataGridIngresos.DataSource = null;
                    textTotalLocal.Text = "0";
                }
            }
            catch (Exception e)
            {
                ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
                respuesta = ingresoService.ConsultarTodos();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    dataGridIngresos.DataSource = null;
                    ingresos = respuesta.Ingresos.ToList();
                    dataGridIngresos.DataSource = respuesta.Ingresos;
                    Borrar.Visible = true;
                    textTotalLocal.Text = ingresoService.Totalizar().Cuenta.ToString();
                }
            }
        }
        void FiltroPorId(string id)
        {
            BusquedaIngresoRespuesta respuesta = new BusquedaIngresoRespuesta();
            respuesta = ingresoService.BuscarPorIdentificacion(id);
            var registro = respuesta.Ingreso;
            if (registro != null)
            {
                encontrado = true;
                var ingresos = new List<Ingreso> { registro };
                textNumeroComprobante.Text = ingresos[0].CodigoComprobante;
                dateFechaIngreso.Value = ingresos[0].FechaDeIngreso;
                comboComite.Text = ingresos[0].Comite;
                comboConcepto.Text = ingresos[0].Concepto;
                textDineroIngreso.Text = ingresos[0].Valor.ToString();
                textDetalle.Text = ingresos[0].Detalle;
            }
        }
        void FiltroPorComite(string comite)
        {
            ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
            respuesta = ingresoService.FiltrarIngresosPorComite(comite);
            if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
            {
                dataGridIngresos.DataSource = null;
                ingresos = respuesta.Ingresos.ToList();
                if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
                {
                    dataGridIngresos.DataSource = respuesta.Ingresos;
                    Borrar.Visible = true;
                }
            }
            else
            {
                dataGridIngresos.DataSource = null;
                ingresos = respuesta.Ingresos.ToList();
            }
        }
        void FiltrarIngresosPorComite(string comite)
        {
            ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
            respuesta = ingresoService.FiltrarIngresosPorComite(comite);
            if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
            {
                dataGridDetalle.DataSource = null;
                int sumTotal = 0;
                ingresos = respuesta.Ingresos.ToList();
                dataGridDetalle.DataSource = respuesta.Ingresos;
                Borrar.Visible = true;
                textTotalComite.Text = ingresoService.TotalizarTipo(comite).Cuenta.ToString();
                for (int i = 0; i < respuesta.Ingresos.Count; i++)
                {
                    sumTotal = sumTotal + respuesta.Ingresos[i].Valor;
                    textValorConcepto.Text = sumTotal.ToString();
                }
            }
        }
        void FiltrarPorConcepto(string concepto)
        {
            ConsultaIngresoRespuesta respuesta = new ConsultaIngresoRespuesta();
            respuesta = ingresoService.FiltrarIngresosPorConcepto(concepto);
            if (respuesta.Ingresos.Count != 0 && respuesta.Ingresos != null)
            {
                var conceptos = new List<Ingreso>();
                var sumTotal = 0;
                dataGridDetalle.DataSource = null;
                ingresos = respuesta.Ingresos.ToList();
                for (int i = 0; i < ingresos.Count; i++)
                {
                    if (ingresos[i].Comite == comite)
                    {
                        conceptos.Add(ingresos[i]);
                        sumTotal = sumTotal + ingresos[i].Valor;
                    }
                }
                textValorConcepto.Text = sumTotal.ToString();
                dataGridDetalle.DataSource = conceptos;
                Borrar.Visible = true;
            }
            else
            {
                textValorConcepto.Text = "0";
                dataGridDetalle.DataSource = null;
            }
        }
        private void dataGridIngresos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridIngresos.DataSource != null)
            {
                if (dataGridIngresos.Columns[e.ColumnIndex].Name == "Borrar")
                {
                    id = Convert.ToString(dataGridIngresos.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                    EliminarIngreso(id);
                    ConsultarIngresos();
                }
                else
                {
                    if (dataGridIngresos.Columns[e.ColumnIndex].Name == "Detallar")
                    {
                        comite = Convert.ToString(dataGridIngresos.CurrentRow.Cells["Comite"].Value.ToString());
                        FiltrarIngresosPorComite(comite);
                        tabLibroIngresos.TabPages.Add(tabPage);
                        tabLibroIngresos.SelectedIndex = 2;
                        detallo = true;
                    }
                    else
                    {
                        if (dataGridIngresos.Columns[e.ColumnIndex].Name == "Editar")
                        {
                            id = Convert.ToString(dataGridIngresos.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                            FiltroPorId(id);
                            if (encontrado == true)
                            {
                                tabLibroIngresos.SelectedIndex = 1;
                            }
                        }
                    }
                }
            }
        }
        private void dataGridDetalle_CellClick(object sender, DataGridViewCellEventArgs e)
        {   
            if (dataGridDetalle.DataSource != null)
            {
                if (dataGridDetalle.Columns[e.ColumnIndex].Name == "Borrar2")
                {
                    id = Convert.ToString(dataGridDetalle.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                    EliminarIngreso(id);
                    ConsultarIngresos();
                    tabLibroIngresos.SelectedIndex = 0;
                }
                else
                {
                    if (dataGridDetalle.Columns[e.ColumnIndex].Name == "Editar2")
                    {
                        id = Convert.ToString(dataGridDetalle.CurrentRow.Cells["CodigoComprobante"].Value.ToString());
                        FiltroPorId(id);
                        if (encontrado == true)
                        {
                            tabLibroIngresos.SelectedIndex = 1;
                        }
                    }
                }
            }
        }

        private void textSerachLibreta_Enter(object sender, EventArgs e)
        {
            if(textSerachLibreta.Text== "Buscar por detalle")
            {
                textSerachLibreta.Text = "";
            }
        }

        private void textSerachLibreta_Leave(object sender, EventArgs e)
        {
            if (textSerachLibreta.Text == "")
            {
                textSerachLibreta.Text = "Buscar por detalle";
            }
        }

        private void textDineroIngreo_Enter(object sender, EventArgs e)
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

        private void textDineroIngreo_Leave(object sender, EventArgs e)
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

        private void comboComite_Enter(object sender, EventArgs e)
        {
            if (comboComite.Text == "Comite")
            {
                comboComite.Text = "";
            }
        }

        private void comboComite_Leave(object sender, EventArgs e)
        {
            if (comboComite.Text == "")
            {
                comboComite.Text = "Comite";
            }
        }

        private void comboConcepto_Enter(object sender, EventArgs e)
        {
            if (comboConcepto.Text == "Concepto")
            {
                comboConcepto.Text = "";
            }
        }

        private void comboConcepto_Leave(object sender, EventArgs e)
        {
            if (comboConcepto.Text == "")
            {
                comboConcepto.Text = "Concepto";
            }
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

        private void textDineroIngreso_Validated(object sender, EventArgs e)
        {
            if (textDineroIngreso.Text != "" && textDineroIngreso.Text != "$ 000.00")
            {
                int sumIngreso = int.Parse(textDineroIngreso.Text);
                textDineroIngreso.Text = LecturaCifra(sumIngreso);
            }
        }

        private void btSearchLibreta_Click(object sender, EventArgs e)
        {
            if (textSerachLibreta.Visible == false)
            {
                textSerachLibreta.Visible = true;
                btSearchLibreta.Visible = false;
                btnCloseSearchLibreta.Visible = true;
            }
        }

        private void btnCloseSearchLibreta_Click(object sender, EventArgs e)
        {
            if (textSerachLibreta.Visible == true)
            {
                textSerachLibreta.Visible = false;
                btSearchLibreta.Visible = true;
                btnCloseSearchLibreta.Visible = false;
            }
        }
        private Ingreso MapearIngreso()
        {
            ingreso = new Ingreso();
            ingreso.CodigoComprobante = textNumeroComprobante.Text;
            ingreso.FechaDeIngreso = dateFechaIngreso.Value;
            ingreso.Comite = comboComite.Text;
            ingreso.Concepto = comboConcepto.Text;
            string cantidadConSigno = textDineroIngreso.Text; // Esto contiene "$ 30000"
            string cantidadSinSigno = cantidadConSigno.Replace("$", "").Trim(); // Esto quita el signo "$"
            int cantidadEntera = int.Parse(cantidadSinSigno); // Convierte el valor a un entero
            ingreso.Valor = cantidadEntera;
            ingreso.Detalle = textDetalle.Text;
            return ingreso;
        }
        private void Limpiar()
        {
            int seguimiento =int.Parse(textNumeroComprobante.Text );
            textNumeroComprobante.Text = (seguimiento+1).ToString();
            CalcularComprobante();
            dateFechaIngreso.Value = DateTime.Now;
            comboComite.Text = "Comite";
            comboConcepto.Text = "Concepto";
            textDineroIngreso.Text = "$ 000.00";
            textDetalle.Text="Detalle";
        }
        private void btnRegistrar_Click(object sender, EventArgs e)
        {
            //Obtenemos los datos del usuario y construimos el dato de la nube
            Ingreso ingreso = MapearIngreso();
            try
            {
                var msg = ingresoService.Guardar(ingreso);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var ingress = ingressMaps.IngressMap(ingreso);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("IngressData").Document(ingress.CodigoComprobante.ToString());
                docRef.SetAsync(ingress);
                // Guardamos localmente
                MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresos();
                Limpiar();
                tabLibroIngresos.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                int count = ex.Message.Length;
                if (count > 0)
                {
                    // Guardamos localmente
                    var msg = ingresoService.Guardar(ingreso);
                    MessageBox.Show(msg, "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ConsultarIngresos();
                    Limpiar();
                    tabLibroIngresos.SelectedIndex = 0;
                }
            }
        }
        private void EliminarIngreso(string codigo)
        {
            try
            {
                //Elimina primero de firebase o la nube
                var db = FirebaseService.Database;
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("IngressData").Document(codigo);
                docRef.DeleteAsync();
                string mensaje = ingresoService.Eliminar(codigo);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresos();
            }
            catch(Exception e)
            {
                string mensaje = ingresoService.Eliminar(id);
                MessageBox.Show(mensaje, "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarIngresos();
            }
        }

        private void tabLibroIngresos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabLibroIngresos.SelectedIndex != 2 && detallo!=false)
            {
                tabPage = tabLibroIngresos.TabPages["tabDetalle"];
                tabLibroIngresos.TabPages.RemoveAt(2);
                detallo = false;
            }
        }
        private void btnModificar_Click(object sender, EventArgs e)
        {
            Ingreso nuevoIngreso = MapearIngreso();
            try
            {
                nuevoIngreso.CodigoComprobante = id;
                string mensaje = ingresoService.Modificar(nuevoIngreso);
                //Guardamos en la nube
                var db = FirebaseService.Database;
                var ingress = ingressMaps.IngressMap(nuevoIngreso);
                Google.Cloud.Firestore.DocumentReference docRef = db.Collection("IngressData").Document(ingress.CodigoComprobante.ToString());
                docRef.SetAsync(ingress);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarIngresos();
                Limpiar();
                tabLibroIngresos.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                string mensaje = ingresoService.Modificar(nuevoIngreso);
                MessageBox.Show(mensaje, "Mensaje de registro", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                ConsultarIngresos();
                Limpiar();
                tabLibroIngresos.SelectedIndex = 0;
            }
        }

        private void comboFiltroComite_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboFiltroComite.Text;
            if (filtro == "Todos")
            {
                ConsultarIngresos();
            }
            else
            {
                FiltroPorComite(filtro);
            }
        }

        private void comboConceptoDetalle_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filtro = comboConceptoDetalle.Text;
            if (filtro == "Ninguno")
            {
                FiltrarIngresosPorComite(comite);
            }
            else
            {
                FiltrarPorConcepto(filtro);
            }
        }

        private void btnImprimirDetalle_Click(object sender, EventArgs e)
        {
            // Ruta relativa del archivo .docx que quieres editar
            string filePath = "Doc's\\Tesoreria\\InformeIndividualTesoreria.docx";

            try
            {
                // Obtén la ruta completa del archivo dentro del directorio del proyecto
                string directorioProyecto = AppDomain.CurrentDomain.BaseDirectory;
                string rutaCompleta = Path.Combine(directorioProyecto, filePath);

                using (WordprocessingDocument doc = WordprocessingDocument.Open(rutaCompleta, true))
                {
                    // Obtén el cuerpo del documento
                    Body body = doc.MainDocumentPart.Document.Body;

                    // Busca la posición donde agregar la tabla (puedes ajustar esto según tu necesidad)
                    Paragraph ingresosParagraph = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("INGRESOS"));

                    if (ingresosParagraph != null)
                    {
                        // Crea una nueva tabla con 2 columnas y 5 filas
                        Table nuevaTabla = new Table(
                            new TableProperties(
                                new TableLayout { Type = TableLayoutValues.Fixed },
                                new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto }
                            )
                        );

                        for (int i = 0; i < 5; i++)
                        {
                            TableRow fila = new TableRow();
                            fila.Append(
                                new TableCell(new Paragraph(new Run(new Text($"Celda {i + 1}")))),
                                new TableCell(new Paragraph(new Run(new Text($"Otra Celda {i + 1}"))))
                            );
                            nuevaTabla.Append(fila);
                        }

                        // Inserta la tabla después del párrafo de "INGRESOS"
                        body.InsertAfter(nuevaTabla, ingresosParagraph);
                    }

                    // Guarda los cambios en el documento
                    doc.MainDocumentPart.Document.Save();
                }

                MessageBox.Show("Tabla agregada con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar el documento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
