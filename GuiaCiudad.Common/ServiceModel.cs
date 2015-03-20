using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GuiaCiudad.Common
{
    public class ServiceModel
    {
        public event EventHandler<GenericEventArgs<Ciudad>> GetCiudadCompleted;
        public event EventHandler<GenericEventArgs<Clima>> GetClimaCompleted;
        public event EventHandler<GenericListEventArgs<Foto>> GetFotoCompleted;
        public event EventHandler<GenericListEventArgs<Noticia>> GetNoticiasCompleted;

        public async void GetGuia(string searchedValue)
        {
            try
            {
                string ciudaduri = "http://api.geonames.org/search?q=" + searchedValue + "&maxRows=1&username=jerrysb";

                HttpClient client = new HttpClient();
                var result = await client.GetStringAsync(ciudaduri);

                var doc = XDocument.Parse(result);
                var singleXML = doc.Descendants("geonames").Single().Descendants("geoname").Single();
                var ciudadres = new Ciudad()
                {
                    codigo = singleXML.Element("countryCode").Value,
                    nombre = singleXML.Element("toponymName").Value,
                    pais = singleXML.Element("countryName").Value,
                    latitud = singleXML.Element("lat").Value,
                    longitud = singleXML.Element("lng").Value,
                };

                if (GetCiudadCompleted != null)
                {
                    GetCiudadCompleted(this, new GenericEventArgs<Ciudad>(ciudadres));
                }

                /* CLIMA */
                string climauri = "http://api.wunderground.com/api/ba11a0f6b2240f82/conditions/q/" + ciudadres.pais + "/" + ciudadres.nombre + ".xml";
                result = await client.GetStringAsync(climauri);
                
                doc = XDocument.Parse(result);
                singleXML = doc.Descendants("response").Single().Descendants("current_observation").Single();
                
                Clima climares = new Clima()
                {
                    clima = singleXML.Element("weather").Value,
                    humedad = singleXML.Element("relative_humidity").Value,
                    precipitaciones = singleXML.Element("precip_today_string").Value,
                    temperatura = singleXML.Element("temp_c").Value,
                    fechaobservacion = singleXML.Element("observation_time").Value,
                    viento = singleXML.Element("wind_string").Value,
                    icon_url = singleXML.Element("icon_url").Value,
                };

                if (GetClimaCompleted != null)
                {
                    GetClimaCompleted(this, new GenericEventArgs<Clima>(climares));
                }

                /* NOTICIAS */
                string noticiasuri = "https://news.google.com/news/feeds?q=" + ciudadres.nombre + "%20" + ciudadres.pais + "&output=rss";
                result = await client.GetStringAsync(noticiasuri);

                doc = XDocument.Parse(result);
                var noticiasquery = from c in doc.Descendants("item")
                                    select new Noticia()
                                    {
                                        titulo = c.Element("title").Value,
                                        cuerpo = getText(c.Element("title").Value,c.Element("description").Value),
                                        link = c.Element("link").Value,
                                        fuente = "News Google", 
                                    };

                var noticiasres = noticiasquery.ToList();
                
                if (GetNoticiasCompleted != null)
                {
                    GetNoticiasCompleted(this, new GenericListEventArgs<Noticia>(noticiasres));
                }

                /* FOTOS */
                string fotosuri = "http://api.flickr.com/services/rest/?method=flickr.photos.search&api_key=8b02a07f152120829631a827c76da856&text=" + ciudadres.nombre + "+" + ciudadres.pais + "&extras=path_alias%2C+url_s&per_page=20&format=rest";
                result = await client.GetStringAsync(fotosuri);

                doc = XDocument.Parse(result);
                var fotosquery = from c in doc.Descendants("photo")
                                 select new Foto()
                                 {
                                     imagen_url = c.Attribute("url_s").Value,
                                     height = c.Attribute("height_s").Value,
                                     width = c.Attribute("width_s").Value,
                                     id = c.Attribute("id").Value,
                                 };

                var fotosres = fotosquery.ToList();

                if (GetFotoCompleted != null)
                {
                    GetFotoCompleted(this, new GenericListEventArgs<Foto>(fotosres));
                }

            }
            catch (Exception)
            {

            }
            
        }

        string getText(string Title,string HtmlText)
        {
            if (HtmlText == null) return null;

            int maxLength = 200;
            int strLength = 0;
            string fixedString = "";

            // Remove HTML tags and newline characters from the text, and decode HTML encoded characters. 
            // This is a basic method. Additional code would be needed to more thoroughly  
            // remove certain elements, such as embedded Javascript. 

            // Remove HTML tags. 
            fixedString = Regex.Replace(HtmlText.ToString(), "<[^>]+>", string.Empty);

            // Remove newline characters.
            fixedString = fixedString.Replace("\r", "").Replace("\n", "");

            //quitar titulo
            fixedString = fixedString.Remove(0, Title.Length - 3);

            // Remove encoded HTML characters.
            //fixedString = HttpUtility.HtmlDecode(fixedString);
            
            strLength = fixedString.ToString().Length;

            // Some feed management tools include an image tag in the Description field of an RSS feed, 
            // so even if the Description field (and thus, the Summary property) is not populated, it could still contain HTML. 
            // Due to this, after we strip tags from the string, we should return null if there is nothing left in the resulting string. 
            if (strLength == 0)
            {
                return null;
            }

            // Truncate the text if it is too long. 
            else if (strLength >= maxLength)
            {
                fixedString = fixedString.Substring(0, maxLength);

                // Unless we take the next step, the string truncation could occur in the middle of a word.
                // Using LastIndexOf we can find the last space character in the string and truncate there. 
                fixedString = fixedString.Substring(0, fixedString.LastIndexOf(" "));
            }

            fixedString += "...";

            return fixedString.Trim('$','%','&','/','#',' ');
        }
    }
}
