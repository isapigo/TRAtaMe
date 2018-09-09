//------------------------------------------------------------------------------
// Autora: Isabel Piñuel González 
// Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
// Universidad Internacional de la Rioja (UNIR)
// Este código se ofrece bajo licencia MIT
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

using Android.Content.PM;
using Android.Graphics;
using Android.Provider;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;
using AppTRAtaMe.TRAtaMeWS;
using Newtonsoft.Json;

namespace AppTRAtaMe
{
    [Activity(Label = "Fotos")]
    public class FotoActivity : Activity
    {
        public static File _file;
        public static File _dir;
        public static Bitmap bitmap;
        private ImageView _imageView;
        public static cls_usuario datos_usuario;
        public static cls_paciente datos_paciente;
        public static cls_informacion datos_informacion;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Set our view from the "main" layout resource  
            SetContentView(Resource.Layout.Foto);

            base.OnCreate(savedInstanceState);
            datos_usuario = JsonConvert.DeserializeObject<cls_usuario>(Intent.GetStringExtra("datos_usuario"));
            datos_paciente = JsonConvert.DeserializeObject<cls_paciente>(Intent.GetStringExtra("datos_paciente"));
            datos_informacion = JsonConvert.DeserializeObject<cls_informacion>(Intent.GetStringExtra("datos_informacion"));

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
            TextView lbl_episodio_foto = FindViewById<TextView>(Resource.Id.lbl_episodio_foto);
            lbl_episodio_foto.Text = "Ep.:" + datos_informacion.id_episodio.ToString() + " Fecha:" + datos_informacion.fecha + " (" + datos_informacion.veo.ToString() + ")";

            if (datos_informacion.veo > 0) { Mostar_Foto(); }

            Button btn_borra_foto = FindViewById<Button>(Resource.Id.btn_borra_foto);
            btn_borra_foto.Click += btn_borra_foto_Click;

            Button btn_foto_mas = FindViewById<Button>(Resource.Id.btn_foto_mas);
            btn_foto_mas.Click += btn_foto_mas_Click;

            Button btn_foto_menos = FindViewById<Button>(Resource.Id.btn_foto_menos);
            btn_foto_menos.Click += btn_foto_menos_Click;

            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
                Button button = FindViewById<Button>(Resource.Id.btn_camara);
                button.Click += TomarFoto;
            }

        }
        
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Uri contentUri = Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            var options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            options.InPurgeable = true;

            // Calculate inSampleSize 
            options.InSampleSize = 4;

            // Decode bitmap with inSampleSize set
            options.InJustDecodeBounds = false;
            options.InPurgeable = true;
            Bitmap bitmap = BitmapFactory.DecodeFile(App._file.Path, options);

            //Ha dado boton atras y no ha hecho foto
            if (bitmap != null)
            {
                Matrix matrix = new Matrix();
                matrix.PostRotate(90.0f);  // La rotación debe ser decimal (float o double) viene girada y asi la pongo recta
                bitmap = Bitmap.CreateBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);

                byte[] bitmapData;
                using (var ms = new System.IO.MemoryStream())
                {
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, ms);
                    bitmapData = ms.ToArray();
                }

                TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
                string mensaje = "";
                TRAtaMe.mover_fichero(bitmapData, "F", ".jpg", datos_usuario, datos_paciente, ref datos_informacion, ref mensaje);
                while (mensaje == "")
                {
                    //Mientras no haya terminado de subir el fichero (mensaje = "FINAL"==>OK) o mensaje = "mensaje de error"
                }

                if (mensaje != "FINAL")
                {
                    object CrLf = System.Environment.NewLine;
                    ShowAlert("ERROR", "No ha subido el archivo " + CrLf + mensaje, "S");
                }

                string basePath = Android.App.Application.Context.GetExternalFilesDir(null).AbsolutePath;

                File[] files = Environment.GetExternalStoragePublicDirectory((Environment.DirectoryDcim) + "/Camera/").ListFiles();

                string fichero = "";
                string fecha = DateTime.Now.ToString("yyyyMMdd");
                int ind = 9999;

                for (int i = 0; i < files.Count(); i++)
                {
                    if (files[i].Name.Substring(0, 8) == fecha)
                    {
                        if (string.Compare(files[i].AbsolutePath, fichero) == 1)
                        {
                            ind = i;
                        }
                    }
                }

                if (ind != 9999) { files[ind].Delete(); }
                App._file.Delete();

                Mostar_Foto();

                GC.Collect();
            }
        }
        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
            PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }
        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
            Environment.GetExternalStoragePublicDirectory(
            Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }
        private void TomarFoto(object sender, EventArgs eventArgs)
        {

            string fichero;
            string formato = "yyyyMMdd";
            string fecha = DateTime.Now.ToString(formato);

            string ext = ".jpg";
            if (App._file != null) { ext = App._file.Name.Substring(App._file.Name.IndexOf(".")); }

            fichero = "F_" + fecha + "_0_0" + ext;//Nombre del fichero de trabajo
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format(fichero, Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);
        }
        private void Mostar_Foto()
        {
            TextView lbl_episodio_foto = FindViewById<TextView>(Resource.Id.lbl_episodio_foto);
            lbl_episodio_foto.Text = "Ep.:" + datos_informacion.id_episodio.ToString() + " Fecha:" + datos_informacion.fecha + " (" + datos_informacion.veo.ToString() + ")";

            //Trae el fichero y lo muestra en el ImageView
            TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
            byte[] bitmapData = TRAtaMe.traer_fichero("F", datos_informacion);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(bitmapData, 0, bitmapData.Length);
            _imageView = FindViewById<ImageView>(Resource.Id.imv_foto);

            _imageView.SetImageBitmap(bitmap);
        }
        private void btn_foto_mas_Click(object sender, EventArgs eventArgs)
        {

            TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
            string el_where = "";
            string tipo = "F";
            int veo_oigo = 0;
            if (tipo == "A")
            {
                veo_oigo = datos_informacion.oigo;
            }
            else
            {
                veo_oigo = datos_informacion.veo;
            }

            int el_id_MAX = TRAtaMe.dame_id_multimed(tipo, datos_informacion, "MAX");
            int el_id_MIN = TRAtaMe.dame_id_multimed(tipo, datos_informacion, "MIN");

            if (el_id_MAX > veo_oigo) { el_where = "id > " + veo_oigo; }
            else
            {
                if (el_id_MIN <= veo_oigo) { el_where = "id = " + el_id_MIN; }
            }

            el_where = el_where + " Order by id";

            datos_informacion = TRAtaMe.dame_img_aud(tipo, el_where, datos_informacion);
            Mostar_Foto();

        }
        private void btn_foto_menos_Click(object sender, EventArgs eventArgs)
        {

            TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
            string el_where = "";
            string tipo = "F";
            int veo_oigo = 0;
            if (tipo == "A")
            {
                veo_oigo = datos_informacion.oigo;
            }
            else
            {
                veo_oigo = datos_informacion.veo;
            }

            int el_id_MAX = TRAtaMe.dame_id_multimed(tipo, datos_informacion, "MAX");
            int el_id_MIN = TRAtaMe.dame_id_multimed(tipo, datos_informacion, "MIN");

            if (el_id_MIN < veo_oigo) { el_where = "id < " + veo_oigo; }
            else
            {
                if (el_id_MAX >= veo_oigo) { el_where = "id = " + el_id_MAX; }
            }

            el_where = el_where + " Order by id desc";

            datos_informacion = TRAtaMe.dame_img_aud(tipo, el_where, datos_informacion);
            Mostar_Foto();

        }
        private void btn_borra_foto_Click(object sender, EventArgs eventArgs)
        {
            object CrLf = System.Environment.NewLine;
            ShowAlert("ELIMINAR ARCHIVO", "Va a eliminar el archivo actual definitivamente." + CrLf + "¿Está seguro?", "SN");
        }
        private void Quitar_Foto()
        {
            TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();
            string tipo = "F";
            int veo_oigo = 0;

            if (tipo == "A")
            {
                veo_oigo = datos_informacion.oigo;
            }
            else
            {
                veo_oigo = datos_informacion.veo;
            }

            string mensaje = "";
            if (TRAtaMe.elimina_fichero(tipo, ref datos_informacion, ref mensaje))
            {
                ShowAlert("MULTIMEDIA", "Se ha eliminado el archivo correctamente.", "S");
            }
            else
            {
                object CrLf = System.Environment.NewLine;
                ShowAlert("ERROR", "No se ha eliminado el archivo" + CrLf + mensaje, "S");
            }
        }
        private void ShowAlert(string titulo, string texto, string opcs)
        {
            AlertDialog.Builder builder = null;
            TRAtaMeWS.TRAtaMeWS TRAtaMe = new TRAtaMeWS.TRAtaMeWS();

            if (opcs == "SN")
            {
                builder = new AlertDialog.Builder(this)
                        .SetTitle(titulo)
                        .SetMessage(texto)
                        .SetNegativeButton("No", (senderAlert, args) =>
                        {
                            ShowAlert(titulo, "No ha eliminado el fichero", "S");
                        })
                        .SetPositiveButton("Si", (senderAlert, args) =>
                        {
                            Quitar_Foto();
                        });
            }

            if (opcs == "S")
            {
                builder = new AlertDialog.Builder(this)
                        .SetTitle(titulo)
                        .SetMessage(texto)
                        .SetPositiveButton("Si", (senderAlert, args) =>
                        {
                            if (titulo == "MULTIMEDIA")
                            {
                                btn_foto_mas_Click(null, null);
                            }
                        });
            }
            builder.Create().Show();
        }
    }
}