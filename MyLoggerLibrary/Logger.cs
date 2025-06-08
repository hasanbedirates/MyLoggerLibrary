using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyLoggerLibrary
{
    public enum LogLevel
    {
        Fatal = 1,   // Uygulamanın durmasına sebep olacak kadar ciddi hata
        Error = 2,   // Hata oluştu ama uygulama çalışmaya devam ediyor
        Warning = 3, // Uyarı, sorun olabilecek durum
        Info = 4,    // Genel bilgi (kullanıcı giriş yaptı, butona basıldı)
        Debug = 5    // Geliştiriciye özel detaylı bilgi (değişken, işlem akışı)
    }


    public static class Logger
    {
        private static string projeAdi = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
        private static int dosyaSayac = 0;

        private static int maxSatirSayisi = int.TryParse(ConfigurationManager.AppSettings["MaxLogSatiri"], out int sonuc) ? sonuc : 100;
        private static string logFolderPath = ConfigurationManager.AppSettings["LogKlasoru"] ?? "logs";
        private static string aktifLogDosyasi = YeniLogDosyaYoluOlustur();

        public static LogLevel CurrentLogLevel = (LogLevel)(int.TryParse(ConfigurationManager.AppSettings["MinLogSeviyesi"], out int sev) ? sev : 5);

        public static void Write(LogLevel level, string actionDescription, string yazilanVeri = "")
        {
            try
            {
                if ((int)level > (int)CurrentLogLevel)
                    return;

                if (!Directory.Exists(logFolderPath))
                    Directory.CreateDirectory(logFolderPath);

                if (File.Exists(aktifLogDosyasi))
                {
                    int satirSayisi = File.ReadAllLines(aktifLogDosyasi).Length;
                    if (satirSayisi >= maxSatirSayisi)
                    {
                        dosyaSayac++;
                        aktifLogDosyasi = YeniLogDosyaYoluOlustur();
                    }
                }

                var stack = new StackTrace(true);
                StackFrame frame = null;

                foreach (var f in stack.GetFrames())
                {
                    var method = f.GetMethod();
                    var type = method?.DeclaringType;
                    var file = f.GetFileName();

                    if (type != null &&
                        !type.FullName.StartsWith("System") &&
                        !type.FullName.Contains("Logger") &&
                        file != null &&
                        file.EndsWith(".cs"))
                    {
                        frame = f;
                        break;
                    }
                }

                string methodName = frame?.GetMethod()?.Name ?? "BilinmeyenMethod";
                string sourceFile = frame?.GetFileName() ?? "BilinmeyenForm.cs";
                int lineNumber = frame?.GetFileLineNumber() ?? 0;
                string formName = Path.GetFileNameWithoutExtension(sourceFile);

                if (!string.IsNullOrWhiteSpace(yazilanVeri))
                    actionDescription += $": {yazilanVeri}";

                string logMessage = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} || {(int)level} || {level} || {formName} || {methodName} || {lineNumber} || {actionDescription} ||";
                File.AppendAllText(aktifLogDosyasi, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Logger hatası: {ex.Message}");
            }
        }

        private static string YeniLogDosyaYoluOlustur()
        {
            string zamanDamgasi = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string dosyaAdi = $"{projeAdi}_{zamanDamgasi}_{dosyaSayac:D3}.txt";
            return Path.Combine(logFolderPath, dosyaAdi);
        }
    }
}

//namespace MyLoggerLibrary
//{
//    public enum LogLevel
//    {
//        Fatal = 1,
//        Error = 2,
//        Warning = 3,
//        Info = 4,
//        Debug = 5
//    }
//    public static class Logger
//    {
//        private static string projeAdi = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
//        private static int dosyaSayac = 0;

//        private static int maxSatirSayisi = int.TryParse(ConfigurationManager.AppSettings["MaxLogSatiri"], out int sonuc) ? sonuc : 100;
//        private static string logFolderPath = ConfigurationManager.AppSettings["LogKlasoru"] ?? "logs";
//        private static string aktifLogDosyasi = YeniLogDosyaYoluOlustur();

//        public static LogLevel CurrentLogLevel = (LogLevel)(int.TryParse(ConfigurationManager.AppSettings["MinLogSeviyesi"], out int sev) ? sev : 5);

//        public static void Log(LogLevel level, string actionDescription = "")
//        {
//            try
//            {
//                if ((int)level > (int)CurrentLogLevel)
//                    return;

//                if (!Directory.Exists(logFolderPath))
//                    Directory.CreateDirectory(logFolderPath);

//                if (File.Exists(aktifLogDosyasi))
//                {
//                    int satirSayisi = File.ReadAllLines(aktifLogDosyasi).Length;
//                    if (satirSayisi >= maxSatirSayisi)
//                    {
//                        dosyaSayac++;
//                        aktifLogDosyasi = YeniLogDosyaYoluOlustur();
//                    }
//                }

//                var stack = new StackTrace(true);
//                StackFrame frame = null;

//                foreach (var f in stack.GetFrames())
//                {
//                    var method = f.GetMethod();
//                    var type = method?.DeclaringType;
//                    var file = f.GetFileName();

//                    if (type != null &&
//                        !type.FullName.StartsWith("System") &&
//                        !type.FullName.Contains("Logger") &&
//                        file != null &&
//                        file.EndsWith(".cs"))
//                    {
//                        frame = f;
//                        break;
//                    }
//                }

//                string methodName = frame?.GetMethod()?.Name ?? "BilinmeyenMethod";
//                string sourceFile = frame?.GetFileName() ?? "BilinmeyenForm.cs";
//                int lineNumber = frame?.GetFileLineNumber() ?? 0;
//                string formName = Path.GetFileNameWithoutExtension(sourceFile);

//                if (string.IsNullOrWhiteSpace(actionDescription))
//                    actionDescription = TahminiEylem(methodName);

//                string logMessage = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} || {(int)level} || {level} || {formName} || {methodName} || {lineNumber} || {actionDescription} ||";
//                File.AppendAllText(aktifLogDosyasi, logMessage + Environment.NewLine);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"Logger hatası: {ex.Message}");
//            }
//        }

//        private static string YeniLogDosyaYoluOlustur()
//        {
//            string zamanDamgasi = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
//            string dosyaAdi = $"{projeAdi}_{zamanDamgasi}_{dosyaSayac.ToString("D3")}.txt";
//            return Path.Combine(logFolderPath, dosyaAdi);
//        }

//        private static string TahminiEylem(string methodName)
//        {
//            methodName = methodName.ToLower();

//            if (methodName.Contains("textchanged") || methodName.Contains("yaz"))
//                return "Yazı yazıldı";
//            if (methodName.Contains("click") || methodName.Contains("tikla") || methodName.Contains("btn") || methodName.Contains("buton"))
//                return "Butona basıldı";
//            if (methodName.Contains("giris"))
//                return "Giriş yapıldı";
//            if (methodName.Contains("cikis"))
//                return "Çıkış yapıldı";
//            if (methodName.Contains("ekle"))
//                return "Ekleme işlemi yapıldı";
//            if (methodName.Contains("sil") || methodName.Contains("silindi") || methodName.Contains("eksilt"))
//                return "Silme işlemi yapıldı";
//            if (methodName.Contains("guncelle") || methodName.Contains("duzenle"))
//                return "Güncelleme işlemi yapıldı";
//            if (methodName.Contains("degistir"))
//                return "Değiştirme işlemi yapıldı";
//            if (methodName.Contains("kaydet"))
//                return "Kaydetme işlemi yapıldı";
//            if (methodName.Contains("yukle") || methodName.Contains("load"))
//                return "Yükleme işlemi yapıldı";
//            if (methodName.Contains("ara"))
//                return "Arama yapıldı";
//            if (methodName.Contains("sec") || methodName.Contains("seç"))
//                return "Seçim yapıldı";
//            if (methodName.Contains("gizle") || methodName.Contains("kapat"))
//                return "Alan gizlendi";
//            if (methodName.Contains("ac") || methodName.Contains("goster") || methodName.Contains("aç"))
//                return "Alan gösterildi";

//            return "Kullanıcı eylemi belirtilmedi";
//        }
//    }
//}
///////////////////////////
/// 

//< appSettings >
//  < add key = "MinLogSeviyesi" value = "3" />
//  < add key = "MaxLogSatiri" value = "100" />
//  < add key = "LogKlasoru" value = "C:\logs\TelefonRehberi" />
//</ appSettings >
