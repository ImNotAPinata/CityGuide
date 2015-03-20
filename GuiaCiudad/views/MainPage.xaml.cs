using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GuiaCiudad.Resources;
using GuiaCiudad.Common;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;

namespace GuiaCiudad
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            try
            {
            var vm = (this.Resources["ViewModel"] as viewModels.GuiaViewModel);
            vm.Ciudad = SaveToStorage.ReadFromXml<Ciudad>("ciudad.xml");
            vm.Clima = SaveToStorage.ReadFromXml<Clima>("clima.xml");
            
            
            for (int i = 0; i < 20;i++)
            {
                var pic = SaveToStorage.ReadFromXml<Foto>(string.Format("pic{0}.xml", i));
                if (pic != null)
                {
                    vm.FotoList.Add(pic);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                var noticias = SaveToStorage.ReadFromXml<Noticia>(string.Format("noticia{0}.xml", i));
                if (noticias != null)
                {
                    vm.NoticiaList.Add(noticias);
                }
            }

            using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //fotoClima
                if (ISF.FileExists("climapic.jpg"))
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (IsolatedStorageFileStream fileStream = ISF.OpenFile("climapic.jpg", FileMode.Open, FileAccess.Read))
                    {
                        bitmap.SetSource(fileStream);
                    }
                    vm.Clima.icon_url = bitmap;
                }

                //fotos
                for (int i = 0; i < 20; i++)
                {
                    if (ISF.FileExists(string.Format("pic{0}.jpg", i)))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        using (IsolatedStorageFileStream fileStream = ISF.OpenFile(string.Format("pic{0}.jpg", i), FileMode.Open, FileAccess.Read))
                        {
                            bitmap.SetSource(fileStream);
                        }
                        vm.FotoList[i].imagen_url = bitmap; 
                    }
                }                
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}