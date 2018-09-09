//------------------------------------------------------------------------------
// Autora: Isabel Piñuel González 
// Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
// Universidad Internacional de la Rioja (UNIR)
// Este código se ofrece bajo licencia MIT
//------------------------------------------------------------------------------
using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AppTRAtaMe.TRAtaMeWS;
using Newtonsoft.Json;


namespace AppTRAtaMe
{
    [Activity(Label = "Gestión del Episodio")]
    public class EpisodioActivity : Activity
    {
        public static cls_usuario datos_usuario;
        public static cls_paciente datos_paciente;
        public static cls_informacion datos_informacion = new cls_informacion();

        Android.Text.Method.IKeyListener listener;
        bool editable = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Create your application here
            SetContentView(Resource.Layout.Episodio);

            base.OnCreate(savedInstanceState);

            Button btn_foto = FindViewById<Button>(Resource.Id.btn_foto);
            btn_foto.Click += btn_foto_Click;

            Button btn_episodio = FindViewById<Button>(Resource.Id.btn_episodio);
            btn_episodio.Click += btn_episodio_Click;
            btn_episodio.RequestFocus();

            Button btn_respuestas = FindViewById<Button>(Resource.Id.btn_respuestas);
            btn_respuestas.Click += btn_respuestas_Click;
            btn_respuestas.RequestFocus();

            datos_usuario = JsonConvert.DeserializeObject<cls_usuario>(Intent.GetStringExtra("datos_usuario"));
            datos_paciente = JsonConvert.DeserializeObject<cls_paciente>(Intent.GetStringExtra("datos_paciente"));

            TextView lbl_nombre = FindViewById<TextView>(Resource.Id.lbl_nombre);
            TextView lbl_edad = FindViewById<TextView>(Resource.Id.lbl_edad);

            DateTime date = DateTime.Now;
            int year = date.Year;
            int ano_nac = datos_paciente.fecnac.Year;
            int edad = year - ano_nac;

            int max = datos_paciente.nombre_com.Length;
            if (max > 20) { max = 23; }
            lbl_nombre.Text = "Id:" + datos_paciente.id_paciente.ToString() + " " + datos_paciente.nombre_com.Substring(0, max) + ".";
            lbl_edad.Text = edad.ToString();

            Spinner spn_episodio = FindViewById<Spinner>(Resource.Id.spn_episodio);
            spn_episodio.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spn_episodio_ItemSelected);

            EditText txt_pregunta = FindViewById<EditText>(Resource.Id.txt_pregunta);
            listener = txt_pregunta.KeyListener;

            txt_pregunta.KeyListener = null;
            editable = false;
            txt_pregunta.LongClick += txt_pregunta_LongClick;

            using (TRAtaMeWS.TRAtaMeWS  TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
            {
                string mensaje = "";
                char[] delimiterChars = { '[' };

                datos_informacion.id_episodio = TRAtaMe.existe_episodio(datos_paciente.id_paciente, ref mensaje);
                if (datos_informacion.id_episodio > 0)
                {
                    mensaje = "";
                    if (!monta_spinner_episodios(ref mensaje)) // Error
                    {
                        if (mensaje != "") { ShowAlert("ERROR", mensaje, "S", 0, null, null); }
                    }
                    else
                    {
                        if (datos_informacion.id_episodio > 0) // si viene con un episodio seleccionado
                        {
                            for (int i = 1; i < spn_episodio.Count; i++)
                            {
                                int episodio;
                                episodio = Int32.Parse(spn_episodio.GetItemAtPosition(i).ToString().Substring(0, spn_episodio.GetItemAtPosition(i).ToString().IndexOf(" - ")));
                                if (episodio == datos_informacion.id_episodio)
                                {
                                    datos_informacion.codser = "";//OJO necesita para coger los datos de informacion globales
                                    TRAtaMe.toda_informacion(episodio.ToString(), ref datos_paciente, ref datos_informacion, ref mensaje);// En este punto recupero toda la estructora de un episodio
                                    spn_episodio.SetSelection(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                else // Paciente sin episodios
                {
                    string[] lista = { "NUEVO EPISODIO (Genera uno nuevo)" };
                    var adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, lista);
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    spn_episodio.Adapter = adapter;

                    txt_pregunta.Text = "";
                }
            }
        }

        protected override void OnDestroy()
        {
            Finish();
            base.OnDestroy();
        }

        private bool monta_spinner_episodios(ref string mensaje)
        {
            string listax = "";
            string listan = "";
            char[] delimiterChars = { '[' };
            TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();

            if (TRAtaMe.dame_episodios_paciente(datos_paciente.id_paciente, ref mensaje, ref listax, ref listan)) // Paciente con episodios anteriores
            {
                Spinner spn_episodio = FindViewById<Spinner>(Resource.Id.spn_episodio);
                string[] lista = listax.Split(delimiterChars);
                string[] lista_r = listan.Split(delimiterChars);
                lista[0] = "NUEVO EPISODIO (Genera uno nuevo)";
                lista_r[0] = "0";

                var adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, lista);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spn_episodio.Adapter = adapter;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void txt_pregunta_LongClick(object sender, EventArgs e)
        {
            EditText txt_pregunta = (EditText)sender;
            if (editable)
            {
                editable = false;
                txt_pregunta.Text = datos_informacion.pregunta;
                txt_pregunta.KeyListener = null;
            }
            else
            {
                editable = true;
                txt_pregunta.Text = "";
                txt_pregunta.KeyListener = listener;
            }
        }

        private void spn_episodio_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e) // Episodio seleccionado
        {
            Spinner spinner = (Spinner)sender;
            Button btn_episodio = FindViewById<Button>(Resource.Id.btn_episodio);
            EditText txt_pregunta = FindViewById<EditText>(Resource.Id.txt_pregunta);

            if (e.Position == 0)
            {
                txt_pregunta.Text = "";
                editable = true;
                txt_pregunta.KeyListener = listener;

            }
            else
            {
                editable = false;
                txt_pregunta.Text = datos_informacion.pregunta;
                txt_pregunta.KeyListener = null;

                string literal = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
                int pos = literal.IndexOf(" - ", 2) + 3; // devuelve posicion del segundo guion +3
                txt_pregunta.Text = literal.Substring(pos);

                string mensaje = "";
             //   string listax = "";
             //   string listan = "";
                char[] delimiterChars = { '[' };
                int episodio = 0;

                TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
                pos = literal.IndexOf(" - ");// devuelde posicion del primer guion
                episodio = Int32.Parse(literal.Substring(0, pos));

                datos_informacion.codser = "";//OJO necesita para coger los datos de informacion globales

                TRAtaMe.toda_informacion(episodio.ToString(), ref datos_paciente, ref datos_informacion, ref mensaje);// En este punto recupero toda la estructora de un episodio

            }
        }

        private void btn_respuestas_Click(object sender, EventArgs e)
        {
            using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
            {
                string listax = "";
                string listar = "";
                string mensaje = "";
                char[] delimiterChars = { '[' };
                object CrLf = System.Environment.NewLine;
                Spinner spn_episodio = FindViewById<Spinner>(Resource.Id.spn_episodio);
                bool hay = false;

                if (spn_episodio.SelectedItemId != 0)
                {
                    hay = TRAtaMe.dame_respuestas_episodio(datos_informacion.id_episodio, ref mensaje, ref listax, ref listar);
                }

                if (mensaje != "")
                {
                    ShowAlert("ERROR", mensaje, "S", 0, null, null);

                }
                else
                {
                    if (hay)
                    {
                        string[] lista = listar.Split(delimiterChars);
                        mensaje = "Respuestas:" + CrLf;
                        for (int i = 0; i < lista.Count(); i++)
                        {
                            mensaje = mensaje + lista[i] + CrLf;
                        }
                    }
                    else
                    {
                        mensaje = "No existen respuestas";
                    }
                    ShowAlert("RESPUESTAS", mensaje, "S", 0, null, null);
                }
            }
        }

        private void btn_episodio_Click(object sender, EventArgs eventArgs)
        {
            EditText txt_pregunta = FindViewById<EditText>(Resource.Id.txt_pregunta);
            Spinner spn_episodio = FindViewById<Spinner>(Resource.Id.spn_episodio);
            object CrLf = System.Environment.NewLine;

            if (txt_pregunta.Text == "")
            {
                ShowAlert("ERROR", "Debe escribir alguna pregunta.", "S", 0, null, null);
            }
            else
            {
                //string listax = "";
                //string listan = "";
                //string mensaje = "";
                using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
                {

                    if (spn_episodio.SelectedItemId == 0)//NUEVO EPISODIO (Genera uno nuevo)
                    {
                        nuevo_episodio(); //Inserta primero el episodio

                        preguntar_a_gfh("TRA");
                    }
                    else
                    {
                        actualiza_texto(); //Actualiza la pregunta del Episodio y pone a nulo las fechas de respuesta de los servicios para que respondan
                    }
                }
            }
        }

        private void actualiza_texto()
        {
            EditText txt_pregunta = FindViewById<EditText>(Resource.Id.txt_pregunta);
            DateTime fecha_hora = new DateTime();
            Button btn_episodio = FindViewById<Button>(Resource.Id.btn_episodio);

            object CrLf = System.Environment.NewLine;
            string str_SQL;
            if (editable && txt_pregunta.Text != "")
            {
                using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
                {
                    fecha_hora = DateTime.Now;

                    string fecha = fecha_hora.ToString("dd/MM/yyyy HH:mm:ss");
                    string mensaje = "";
                    string cabecera = "ERROR";
                    datos_informacion.pregunta = "El " + fecha + CrLf + txt_pregunta.Text +
                                                  CrLf + datos_usuario.nombre + CrLf + datos_informacion.pregunta + CrLf;
                    str_SQL = "UPDATE episodio SET " +
                               "pregunta = '" + datos_informacion.pregunta + "' " +
                               "WHERE id = '" + datos_informacion.id_episodio + "';" +
                              "UPDATE respuesta SET " +
                               "fecha = NULL " +
                               "WHERE idepisodio = '" + datos_informacion.id_episodio + "';";
                    if (TRAtaMe.grabar_textos(datos_usuario, datos_paciente, datos_informacion, str_SQL, ref mensaje))
                    {
                        editable = false;
                        txt_pregunta.Text = datos_informacion.pregunta;
                        txt_pregunta.KeyListener = null;
                        mensaje = "Realizada pregunta al episodio " + datos_informacion.id_episodio + " y enviada la los servicios.";
                        if (TRAtaMe.inserta_log(datos_usuario.usuario, datos_informacion.id_episodio, ref mensaje)) mensaje = "";
                    }

                    if (mensaje == "")
                    {
                        mensaje = "Se ha incorporado su pregunta a los servicios seleccionados.";
                        cabecera = "CORRECTO";
                    }
                    else
                    {
                        cabecera = "ERROR";
                        mensaje = "Se ha producido el error Actualizando Textos: ." + mensaje;
                        ShowAlert(cabecera, mensaje, "S", 0, null, null);
                    }
                }
            }
        }

        private void nuevo_episodio()
        {
            EditText txt_pregunta = FindViewById<EditText>(Resource.Id.txt_pregunta);
            DateTime fecha_hora = new DateTime();
            Button btn_episodio = FindViewById<Button>(Resource.Id.btn_episodio);

            object CrLf = System.Environment.NewLine;

            using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
            {
                fecha_hora = DateTime.Now;

                string fecha = fecha_hora.ToString("dd/MM/yyyy HH:mm:ss");
                string mensaje = "";
                string cabecera = "ERROR";
                datos_informacion.pregunta = "El " + fecha + CrLf + txt_pregunta.Text + CrLf + datos_usuario.nombre + CrLf + CrLf;
                datos_informacion.fecha = fecha;
                datos_informacion.audios = 0;
                datos_informacion.fotos = 0;
                datos_informacion.id_paciente = datos_paciente.id_paciente;
                datos_informacion.nhc = datos_paciente.nhc;
                datos_informacion.videos = 0;
                datos_informacion.respuesta = "";
                datos_informacion.codser = "";
                datos_informacion.doctor = "";
                datos_informacion.id_episodio = 0;
                datos_informacion.fch_resp = "";
                if (TRAtaMe.nuevo_episodio(datos_usuario, ref datos_paciente, ref datos_informacion, ref mensaje))
                {
                    txt_pregunta.Text = datos_informacion.pregunta;
                    //                    btn_episodio.Enabled = false;
                    mensaje = "Nuevo episodio " + datos_informacion.id_episodio + " y pregunta enviada la los servicios.";
                    if (TRAtaMe.inserta_log(datos_usuario.usuario, datos_informacion.id_episodio, ref mensaje)) mensaje = "";
                }
                if (mensaje == "")
                {
                    if (monta_spinner_episodios(ref mensaje))
                    {
                        editable = false;
                        txt_pregunta.Text = datos_informacion.pregunta;
                        txt_pregunta.KeyListener = null;
                        Spinner spn_episodio = FindViewById<Spinner>(Resource.Id.spn_episodio);

                        spn_episodio.SetSelection(1);
                        mensaje = "Se ha incorporado su pregunta..";
                        cabecera = "CORRECTO";
                    }
                    else
                    {
                        cabecera = "ERROR";
                        mensaje = "Se ha producido el error: ." + mensaje;
                    }
                }
                else
                {
                    cabecera = "ERROR";
                    mensaje = "Se ha producido el error: ." + mensaje;
                }
                ShowAlert(cabecera, mensaje, "S", 0, null, null);

            }
        }
        private void preguntar_a_gfh(string codser)
        {

            using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
            {
                string mensaje = "";
                string cabecera = "ERROR";
                if (TRAtaMe.preguntar_a_gfh(datos_informacion.id_episodio, codser, ref mensaje))
                {
                    mensaje = "Se ha incorporado la pregunta del episodio " + datos_informacion.id_episodio.ToString() + " al servicio " + codser + ".";
                    if (TRAtaMe.inserta_log(datos_usuario.usuario, datos_informacion.id_episodio, ref mensaje)) mensaje = "";
                }
                if (mensaje != "")
                {
                    cabecera = "ERROR";
                    mensaje = "Se ha producido el error:" + mensaje;
                    ShowAlert(cabecera, mensaje, "S", 0, null, null);
                }
            }
        }

        private void btn_foto_Click(object sender, EventArgs eventArgs)
        {
            if (datos_informacion.id_episodio == 0)
            {
                string cabecera = "ERROR";
                string mensaje = "Antes de realizar fotos a " + datos_paciente.nombre_com + ", debe crear o seleccionar un episodio.";
                ShowAlert(cabecera, mensaje, "S", 0, null, null);
            }
            else
            {
                //Busca en el episodio la primera imagen 
                TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
                datos_informacion.veo_tipo = "F";
                datos_informacion.veo = TRAtaMe.dame_id_multimed(datos_informacion.veo_tipo, datos_informacion, "MIN");
                datos_informacion = TRAtaMe.dame_img_aud(datos_informacion.veo_tipo, " id = " + datos_informacion.veo.ToString() + " Order by id", datos_informacion);

                Intent intent = new Intent(this, typeof(FotoActivity));
                intent.PutExtra("datos_usuario", JsonConvert.SerializeObject(datos_usuario));
                intent.PutExtra("datos_paciente", JsonConvert.SerializeObject(datos_paciente));
                intent.PutExtra("datos_informacion", JsonConvert.SerializeObject(datos_informacion));
                this.StartActivity(intent);
            }
        }

        private void ShowAlert(string titulo, string texto, string opcs, int episodio, string[] lista_si, string[] lista_no)
        {
            AlertDialog.Builder builder = null;

            if (opcs == "SN") // Mensajes con opciones S o N
            {
                builder = new AlertDialog.Builder(this)
                        .SetTitle(titulo)
                        .SetMessage(texto)
                        .SetNegativeButton("No", (senderAlert, args) =>
                        {
                            ShowAlert("ERROR", "No se ha incorporado ni eliminado ningún servicio al episodio " + datos_informacion.id_episodio, "S", 0, null, null);
                        })
                        .SetPositiveButton("Si", (senderAlert, args) =>
                        {
                            //Elige SI a la eliminación e incorporacion de servicios al episodio
                            using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
                            {
                                for (int i = 0; i < lista_si.Count(); i++)
                                {
                                    if (lista_si[i] != "" && lista_si[i] != null)
                                    {
                                        string codser = TRAtaMe.dame_GFH(lista_si[i]);
                                        if (codser != "") { TRAtaMe.preguntar_a_gfh(datos_informacion.id_episodio, codser, ref texto); } //Va a insertar la pregunta al servicio
                                    }
                                }
                                for (int i = 0; i < lista_no.Count(); i++)
                                {
                                    if (lista_no[i] != "" && lista_no[i] != null)
                                    {
                                        string codser = TRAtaMe.dame_GFH(lista_no[i]);
                                        if (codser != "") { TRAtaMe.elimina_pregunta(datos_informacion.id_episodio, codser, ref texto); } //Va a eliminar al servicio de las preguntas
                                    }
                                }
                            }
                        });
            }

            if (opcs == "S") // Mensajes con opcion S
            {
                builder = new AlertDialog.Builder(this)
                        .SetTitle(titulo)
                        .SetMessage(texto)
                        .SetPositiveButton("Si", (senderAlert, args) =>
                        {
                        });
            }

            builder.Create().Show();
        }

    }
}