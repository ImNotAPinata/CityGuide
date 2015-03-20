using GuiaCiudad.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GuiaCiudad.viewModels
{
    public class GuiaViewModel : ViewModelBase
    {
        ObservableCollection<Foto> fotoList;
        public ObservableCollection<Foto> FotoList
        {
            get
            {
                if (fotoList == null)
                {
                    fotoList = new ObservableCollection<Foto>();
                }

                if (DesignerProperties.IsInDesignTool)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        fotoList.Add(new Foto() { imagen_url = "/Assets/AlignmentGrid.png" });
                    }
                }
                return fotoList;
            }
            set { fotoList = value; OnPropertyChanged(); }
        }

        ObservableCollection<Noticia> noticiaList;
        public ObservableCollection<Noticia> NoticiaList
        {
            get
            {
                if (noticiaList == null)
                {
                    noticiaList = new ObservableCollection<Noticia>();
                }

                if (DesignerProperties.IsInDesignTool)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        noticiaList.Add(new Noticia());
                    }
                }
                return noticiaList;
            }
            set { noticiaList = value; OnPropertyChanged(); }
        }

        Ciudad ciudad;
        public Ciudad Ciudad
        {
            get
            {
                if (ciudad == null)
                {
                    ciudad = new Ciudad();
                }
                return ciudad;
            }
            set { ciudad = value; OnPropertyChanged(); }
        }

        Clima clima;
        public Clima Clima
        {
            get
            {
                if (clima == null)
                {
                    clima = new Clima() { icon_url = "/Assets/AlignmentGrid.png" };
                }
                return clima;
            }
            set { clima = value; OnPropertyChanged(); }
        }

        ServiceModel serviceModel = new ServiceModel();

        public GuiaViewModel()
        {
            serviceModel.GetFotoCompleted += (s, a) =>
            {
                fotoList = new ObservableCollection<Foto>(a.ListResults);
                SavePhotos();
            };
            serviceModel.GetNoticiasCompleted += (s, a) =>
            {
                NoticiaList = new ObservableCollection<Noticia>(a.ListResults);
                SaveNoticias();
            };
            serviceModel.GetClimaCompleted += (s, a) =>
            {
                Clima = a.Results;
                SaveClima();
            };
            serviceModel.GetCiudadCompleted += (s, a) =>
            {
                Ciudad = a.Results;
                SaveCity();
            };
            this.GetDownloadCompleted += (s, a) =>
            {
                FotoList = fotoList;
            };
        }

        ActionCommand getGuiaCommand;
        public ActionCommand GetGuiaCommand
        {
            get
            {
                if (getGuiaCommand == null)
                {
                    getGuiaCommand = new ActionCommand(parametro =>
                    {
                        try
                        {
                            serviceModel.GetGuia(parametro.ToString());
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("Ocurrio un error!"+ex.Message);
                        }
                    });
                }

                return getGuiaCommand;
            }
        }

        public event EventHandler<GenericEventArgs<bool>> GetDownloadCompleted;

        void SavePhotos()
        {
            int i = 0;
            foreach (var item in fotoList)
	        {
                //Grabar Foto
                var url = item.imagen_url;
                WebClient client = new WebClient();
                client.OpenReadCompleted += (s, a) =>
                {
                    if (a.Error == null && !a.Cancelled)
                    {
                        using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (ISF.FileExists(string.Format("pic{0}.jpg",i)))
                                ISF.DeleteFile(string.Format("pic{0}.jpg",i));

                            IsolatedStorageFileStream fileStream = ISF.CreateFile(string.Format("pic{0}.jpg",i));

                            BitmapImage bitmap = new BitmapImage();
                            bitmap.SetSource(a.Result);
                            
                            WriteableBitmap wb = new WriteableBitmap(bitmap);
                            System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                            fileStream.Close();

                            //Grabar Objeto
                            item.imagen_url = string.Format("pic{0}.jpg", i);
                            SaveToStorage.WriteToXml<Foto>(item, string.Format("pic{0}.xml", i));

                            item.imagen_url = bitmap; 

                            //maximo de 20 imagenes
                            if (i == 19 && GetDownloadCompleted != null)
                            {
                                GetDownloadCompleted(this, new GenericEventArgs<bool>(true));
                            }
                            i++;
                        }
                    }
                };
                client.OpenReadAsync(new Uri(url.ToString(), UriKind.Absolute));
	        }
        }
        void SaveClima()
        {
            //Grabar FotoClima
            Clima localClima = new Clima();
            localClima = clima;
            var url = localClima.icon_url;

            WebClient client = new WebClient();
            client.OpenReadCompleted += (s, a) =>
            {
                if (a.Error == null && !a.Cancelled)
                {
                    using (IsolatedStorageFile ISF = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (ISF.FileExists("climapic.jpg"))
                            ISF.DeleteFile("climapic.jpg");

                        IsolatedStorageFileStream fileStream = ISF.CreateFile("climapic.jpg");
                        
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.SetSource(a.Result);
                        Clima.icon_url = bitmap;

                        WriteableBitmap wb = new WriteableBitmap(bitmap);
                        System.Windows.Media.Imaging.Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                        fileStream.Close();
                    }
                }

                //Grabar Objeto
                localClima.icon_url = "climapic.jpg";
                SaveToStorage.WriteToXml<Clima>(localClima, "clima.xml");
            };
            client.OpenReadAsync(new Uri(url.ToString(), UriKind.Absolute));
        }
        void SaveCity()
        {
            SaveToStorage.WriteToXml<Ciudad>(Ciudad, "ciudad.xml");
        }
        void SaveNoticias()
        {
            int i = 0;
            foreach (var item in NoticiaList)
            {
                SaveToStorage.WriteToXml<Noticia>(item, string.Format("noticia{0}.xml", i));
                i++;
            }
        }
    }
}
