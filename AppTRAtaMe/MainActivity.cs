//------------------------------------------------------------------------------
// Autora: Isabel Piñuel González 
// Este proyecto es parte del TFG del Curso de Adaptación al Grado de Informática
// Universidad Internacional de la Rioja (UNIR)
// Este código se ofrece bajo licencia MIT
//------------------------------------------------------------------------------
using Android.App;
using Android.Widget;
using Android.OS;
using Java.IO;
using Android.Graphics;

namespace AppTRAtaMe
{
    public static class App
    {
        public static File _file;
        public static File _filesal;
        public static File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "AppTRAtaMe", MainLauncher = true, Icon = "@drawable/icon")]

    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            StartActivity(typeof(LoginActivity));

        }


    }
}
