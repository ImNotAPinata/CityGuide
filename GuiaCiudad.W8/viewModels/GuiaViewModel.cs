using GuiaCiudad.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace GuiaCiudad.W8.viewModels
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
                    clima = new Clima();
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
                FotoList = new ObservableCollection<Foto>(a.ListResults);
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

            LoadData();
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
                        serviceModel.GetGuia(parametro.ToString());
                    });
                }

                return getGuiaCommand;
            }
        }

        public event EventHandler<GenericEventArgs<bool>> GetDownloadCompleted;

        async void SavePhotos()
        {
            int i = 0;
            foreach (var item in fotoList)
            {
                //Grabar Foto
                var url = item.imagen_url;
                Uri source = new Uri(item.imagen_url.ToString());
                string destination = string.Format("pic{0}.jpg", i);

                StorageFile destinationFile = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(destination, CreationCollisionOption.ReplaceExisting);

                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                await StartDownloadAsync(download);

                item.imagen_url = destinationFile.Path;
                await Win8StorageHelper.SaveData(string.Format("pic{0}.xml", i),item);
                item.imagen_url = url;

                if (i == 19 && GetDownloadCompleted != null)
                {
                    GetDownloadCompleted(this, new GenericEventArgs<bool>(true));
                }
                i++;
            }
        }
        async void SaveClima()
        {
            //Grabar FotoClima
            var url = clima.icon_url;
            Uri source = new Uri(clima.icon_url.ToString());
            string destination = "climapic.jpg";


            StorageFile destinationFile = await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(destination, CreationCollisionOption.ReplaceExisting);

            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);
            await StartDownloadAsync(download);
            
            clima.icon_url = destinationFile.Path;
            await Win8StorageHelper.SaveData("clima.xml", clima);
            clima.icon_url = url;
            
        }
        async void SaveCity()
        {
            await Win8StorageHelper.SaveData("ciudad.xml", Ciudad);
        }
        async void SaveNoticias()
        {
            int i = 0;
            foreach (var item in NoticiaList)
            {
                await Win8StorageHelper.SaveData(string.Format("noticia{0}.xml", i), item);
                i++;
            }
        }
        private async Task StartDownloadAsync(DownloadOperation downloadOperation)
        {
            await downloadOperation.StartAsync();
        }

        async void LoadData()
        {
            try
            {
                //ciudad
                Ciudad = (GuiaCiudad.Common.Ciudad)await Win8StorageHelper.LoadData("ciudad.xml", typeof(GuiaCiudad.Common.Ciudad));
                //clima
                var climaData = (GuiaCiudad.Common.Clima)await Win8StorageHelper.LoadData("clima.xml", typeof(GuiaCiudad.Common.Clima));
                StorageFile File = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync("climapic.jpg");
                var file = await File.OpenAsync(FileAccessMode.Read);
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(file);
                climaData.icon_url = bitmap;
                Clima = climaData;

                // foto
                for (int i = 0; i < 20; i++)
                {
                    var pic = (GuiaCiudad.Common.Foto)await Win8StorageHelper.LoadData(string.Format("pic{0}.xml", i), typeof(GuiaCiudad.Common.Foto));
                    if (pic != null)
                    {
                        StorageFile FileX = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync(string.Format("pic{0}.jpg", i));
                        bitmap = new BitmapImage();
                        var fileY = await FileX.OpenAsync(FileAccessMode.Read);
                        bitmap.SetSource(fileY);
                        pic.imagen_url = bitmap;
                        FotoList.Add(pic);
                    }
                }

                for (int i = 0; i < 20; i++)
                {
                    var noticias = (GuiaCiudad.Common.Noticia)await Win8StorageHelper.LoadData(string.Format("noticia{0}.xml", i), typeof(GuiaCiudad.Common.Noticia));
                    if (noticias != null)
                    {
                        NoticiaList.Add(noticias);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
