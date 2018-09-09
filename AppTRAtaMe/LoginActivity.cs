//------------------------------------------------------------------------------
// Autora: Isabel Piñuel González 
// Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
// Universidad Internacional de la Rioja (UNIR)
// Este código se ofrece bajo licencia MIT
//------------------------------------------------------------------------------
using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using AppTRAtaMe.TRAtaMeWS;

namespace AppTRAtaMe
{
    [Activity(Label = "Control de Acceso")]
    public class LoginActivity : Activity
    {
        // aapt resource value: 0x7f050000
        public static cls_usuario datos_usuario;
        public static cls_paciente datos_paciente;

        protected override void OnCreate(Bundle savedInstanceState)
        {

            // Set our view from the "login" layout resource  
            SetContentView(Resource.Layout.Login);

            base.OnCreate(savedInstanceState);
            Button btn_login = FindViewById<Button>(Resource.Id.btn_login);
            btn_login.Click += validar_usuario;

        }

        private void validar_usuario(object sender, EventArgs eventArgs)
        {

            EditText txt_nombre = FindViewById<EditText>(Resource.Id.txt_nombre);
            EditText txt_password = FindViewById<EditText>(Resource.Id.txt_password);

            TextView result = FindViewById<TextView>(Resource.Id.lbl_Resultado);

            using (TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS())
            {
                result.Text = "";
                String mensaje = "Entrada de usuario a la aplicacón móvil.";
                if (TRAtaMe.paciente_en_TRA(txt_password.Text, ref mensaje))
                {
                    mensaje = "Devuelve el id del paciente";

                    int id_paciente = TRAtaMe.hay_paciente(txt_password.Text, ref mensaje);

                    datos_paciente = TRAtaMe.datos_paciente_HCIS(Int32.Parse(txt_password.Text), txt_nombre.Text, ref mensaje);

                    if (id_paciente == 0)
                    {
                        TRAtaMe.nuevo_paciente(ref datos_paciente , ref mensaje);
                        TRAtaMe.crea_usuario_paciente(datos_paciente, ref mensaje);
                    }
                    else
                    {
                        datos_paciente.id_paciente = id_paciente;
                        TRAtaMe.actualiza_paciente(datos_paciente, ref mensaje);
                    }

                    datos_usuario = TRAtaMe.valida_entrada(txt_nombre.Text, txt_password.Text);

                    if (datos_usuario.permiso > 0)
                    {
                        if (TRAtaMe.inserta_log(datos_usuario.dni, 0, ref mensaje))
                        {
                            txt_nombre.Text = "";
                            txt_password.Text = "";
                            Intent intent = new Intent(this, typeof(EpisodioActivity));
                            intent.PutExtra("datos_usuario", JsonConvert.SerializeObject(datos_usuario));
                            intent.PutExtra("datos_paciente", JsonConvert.SerializeObject(datos_paciente));
                            this.StartActivity(intent);
                        }
                        else
                        {
                            result.Text = "Sin acceso a log, contacte con informática." + mensaje;
                        }
                    }
                    else
                    {
                        TRAtaMe.borra_usuario_paciente(txt_nombre.Text, txt_password.Text, ref mensaje);
                        result.Text = "Credenciales no válidas";
                    }
                }
                else
                {
                    TRAtaMe.borra_usuario_paciente(txt_nombre.Text, txt_password.Text, ref mensaje);
                    if (mensaje == "")
                    {
                        mensaje = "Paciente sin intervención quirúrgica";
                    }
                    result.Text = mensaje;
                }
            }
        }
    }
}