using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Xml;
using System.Web;
using System.Xml.Linq;
using System.Web.Script.Serialization;

namespace WebScraper
{

    public class ImportRecord
    {
        public string Title { get; set; }
        public string Body {get; set;}
        public string ImageURL { get; set; }
        public string MainURL { get; set; }

        public bool Mature { get; set; }

        public bool Upload { get; set; }

        public bool UseImage { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WebClient webClient;
        private WebServiceHelper Heard = null;

        public MainWindow()
        {
            InitializeComponent();
            webClient = new WebClient();
            webClient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            Heard = new WebServiceHelper();
        }

        public string TargetUser
        {
            get { return TargetUserText.Text; }
            set
            {
                TargetUserText.Text = value;
            }
        }

        public string TargetChannel
        {
            get { return TargetChannelText.Text; }
            set
            {
                TargetChannelText.Text = value;
            }
        }

        public string TargetPassword
        {
            get { return TargetUserPassword.Text; }
            set
            {
                TargetUserPassword.Text = value;
            }
        }

        private void DeviantArt_Click(object sender, RoutedEventArgs e)
        {
            ClearImportList();
            TargetUser = "deviantartist";
            TargetChannel = "Lifestyle";
            TargetPassword = "All4Sheeple";
            List<ImportRecord> daData = DoDAImport();
            SetImportList(daData);
        }

        private void Reddit_Click(object sender, RoutedEventArgs e)
        {
            ClearImportList();
            TargetUser = "redditbot";
            TargetPassword = "All4Sheeple";
            List<ImportRecord> theData = DoRedditGroupImport(RedditString.Text);
            SetImportList(theData);
        }


        private void RedditBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string subReddit = "";

            switch (RedditBox.SelectedIndex)
            {
                case 0:
                    TargetChannel = "Tech Industry";
                    subReddit = "technology";
                    break;
                case 1:
                    TargetChannel = "Entertainment Industry";
                    subReddit = "entertainment";
                    break;
                case 2:
                    TargetChannel = "Public";
                    subReddit = "mildlyinteresting";
                    break;
            }

            RedditString.Text = "http://www.reddit.com/r/" + subReddit + "/hot.rss";

        }


        private void SetImportList(List<ImportRecord> theList)
        {
            ResultList.ItemsSource = theList;
            MessageBox.Show("Data Imported!");
        }

        private void ClearImportList()
        {
            ResultList.ItemsSource = null;
        }

        private void ImportToProd_Click(object sender, RoutedEventArgs e)
        {
            Heard.ImportToQA = false;
            ImportBlahs();
        }

        private void ImportToQA_Click(object sender, RoutedEventArgs e)
        {
            Heard.ImportToQA = true;
            ImportBlahs();
        }

        public class DAAuthToken
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string status { get; set; }
        }

        public class DA_Author
        {
            public string userid { get; set; }
            public string username { get; set; }
            public string usericon { get; set; }
            public string type { get; set; }
        }

        public class DA_Stats
        {
            public int comments { get; set; }
            public int favourites { get; set; }
        }

        public class DA_Preview
        {
            public string src { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }

        public class DA_Content
        {
            public string src { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }

        public class DA_Thumb
        {
            public string src { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }

        public class DA_Deviation
        {
            public string deviationid { get; set; }
            public object printid { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public string category { get; set; }
            public string category_path { get; set; }
            public bool is_favourited { get; set; }
            public bool is_deleted { get; set; }
            public DA_Author author { get; set; }
            public DA_Stats stats { get; set; }
            public int published_time { get; set; }
            public bool allows_comments { get; set; }
            public DA_Preview preview { get; set; }
            public DA_Content content { get; set; }
            public List<DA_Thumb> thumbs { get; set; }
        }

        public class DAResultList
        {
            public bool has_more { get; set; }
            public int next_offset { get; set; }
            public List<DA_Deviation> results { get; set; }
        }

        private DAAuthToken daAuth = null;

        private List<ImportRecord> DoDAImport()
        {
            if (daAuth == null)
            {
                string url = "https://www.deviantart.com/oauth2/token?client_id=2424&client_secret=03eea2ab4282d59692538b89deacc13e&grant_type=client_credentials";
                string result = webClient.DownloadString(url);
                daAuth = Newtonsoft.Json.JsonConvert.DeserializeObject<DAAuthToken>(result);
            }

            DAResultList resultList = null;
            if ((daAuth != null) && (!(String.IsNullOrEmpty(daAuth.access_token))))
            {
                string url = "https://www.deviantart.com/api/v1/oauth2/browse/undiscovered?limit=30&access_token=" + daAuth.access_token;
                string result = webClient.DownloadString(url);
                resultList = Newtonsoft.Json.JsonConvert.DeserializeObject<DAResultList>(result);
            }

            if ((resultList != null) && (resultList.results.Count > 0))
            {
                List<ImportRecord> importList = new List<ImportRecord>();

                foreach (DA_Deviation deviation in resultList.results)
                {
                    if (deviation.content != null)
                    {
                        ImportRecord newRec = new ImportRecord();
                        newRec.Upload = true;
                        newRec.UseImage = true;
                        newRec.Title = "";
                        newRec.Body = deviation.title + "\n" + "by " + deviation.author.username + "\n\n";
                        newRec.MainURL = deviation.url;
                        newRec.Mature = false;
                        newRec.ImageURL = deviation.content.src;
                        importList.Add(newRec);
                    }
                }
                return importList;
            }
            else
                return null;
        }

        public class Entry
        {
            public string title { get; set; }
            public string link { get; set; }
            public string author { get; set; }
            public string publishedDate { get; set; }
            public string contentSnippet { get; set; }
            public string content { get; set; }
            public List<object> categories { get; set; }
        }

        public class Feed
        {
            public string feedUrl { get; set; }
            public string title { get; set; }
            public string link { get; set; }
            public string author { get; set; }
            public string description { get; set; }
            public string type { get; set; }
            public List<Entry> entries { get; set; }
        }

        public class ResponseData
        {
            public Feed feed { get; set; }
        }

        public class GoogleResponse
        {
            public ResponseData responseData { get; set; }
            public object responseDetails { get; set; }
            public int responseStatus { get; set; }
        }

        private List<ImportRecord> DoRedditGroupImport(string subReddit, bool useTitles = true)
        {
            List<ImportRecord> newList = new List<ImportRecord>();
            string googleParserURL = "http://ajax.googleapis.com/ajax/services/feed/load?v=1.0&num=100&q=";
            string finalURL = googleParserURL + HttpUtility.UrlPathEncode(subReddit);
            byte[] urlData = webClient.DownloadData(subReddit);
            string theData = Encoding.UTF8.GetString(urlData, 0, urlData.Length);
            byte[] finalUrlData = webClient.DownloadData(finalURL);
            string googleData = Encoding.UTF8.GetString(finalUrlData, 0, finalUrlData.Length); ;


            GoogleResponse theResult = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleResponse>(googleData);
             


            XDocument document = XDocument.Parse(theData);

            foreach (XElement descendant in document.Descendants("item"))
            {
                string desc = descendant.Element("description").Value;
                string articleURL = GetRedditSourceURL(desc);
                ParsedObject parsed = ParseRedditDescription(articleURL);

                if (parsed != null)
                {
                    ImportRecord newRec = new ImportRecord();
                    newRec.Upload = true;
                    if (useTitles)
                    {
                        if (String.IsNullOrEmpty(parsed.title))
                            newRec.Title = descendant.Element("title").Value;
                        else
                            newRec.Title = parsed.title;
                    }
                    else
                        newRec.Title = null;

                    if (String.IsNullOrEmpty(parsed.description))
                        newRec.Body = descendant.Element("title").Value;
                    else
                        newRec.Body = parsed.description;

                    newRec.MainURL = parsed.url;
                    newRec.Mature = false;
                    if ((parsed.images != null) && (parsed.images.Count > 0))
                    {
                        newRec.ImageURL = parsed.images[0].url;
                        newRec.UseImage = true;
                    }
                    else newRec.UseImage = false;

                    newList.Add(newRec);
                }
            }

            return newList;
        }

        public class ParsedImage
        {
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }

        }

        public class ParsedObject
        {
            public string description { get; set; }
            public List<ParsedImage> images { get; set; }
            public string original_url { get; set; }
            public string url { get; set; }
            public string title { get; set; }
        }

        private string GetRedditSourceURL(string sourceURL)
        {
            int linkLoc = sourceURL.IndexOf("[link]");
            int urlEnd = sourceURL.LastIndexOf("\"", linkLoc, linkLoc - 1);
            int urlStart = sourceURL.LastIndexOf("\"", urlEnd - 1, urlEnd - 2);
            string articleURL = sourceURL.Substring(urlStart + 1, (urlEnd - urlStart) - 1);

            return articleURL;
        }


        private ParsedObject ParseRedditDescription(string sourceURL)
        {
            ParsedObject parsed = null;

            try
            {
                string queryStr = "http://api.embed.ly/1/extract?key=16357551b6a84e6c88debee64dcd8bf3&maxwidth=500&url=" + HttpUtility.UrlPathEncode(sourceURL);

                string result = webClient.DownloadString(queryStr);

                parsed = Newtonsoft.Json.JsonConvert.DeserializeObject<ParsedObject>(result);
            }
            catch (Exception exp)
            {
                // do nothing
            }

            
            return parsed;
        }

        private void ImportBlahs()
        {
            SetCurrentUser(TargetUser);
            List<ImportRecord> curList = ResultList.ItemsSource as List<ImportRecord>;
            int curCount = 1;
            int maxCount = curList.Count;
            Console.WriteLine("Importing " + maxCount + " records...");
            int retryCount = 0;
            int maxTries = 3;

            while (curList.Count > 0)
            {
                Console.Write("record " + curCount + " of " + maxCount + "...");
                ImportRecord curRec = curList[0];
                string resultStr;

                if (curRec.Upload)
                    resultStr = ImportBlah(curRec);
                else
                    resultStr = "skipped";
                Console.WriteLine(resultStr);
                if ((resultStr == "skipped") || (resultStr == "ok"))
                {
                    curList.Remove(curRec);
                    curCount++;
                }
                else
                {
                    retryCount++;
                    if (retryCount < maxTries)
                        Console.WriteLine("Retrying...");
                    else
                    {
                        Console.WriteLine("Giving up...");
                        curList.Remove(curRec);
                        curCount++;
                    }
                }
            }
            foreach (ImportRecord curRec in ResultList.ItemsSource)
            {
                string resultStr = ImportBlah(curRec);
                Console.WriteLine(resultStr);
            }

            MessageBox.Show("Imported " + curCount.ToString() + " of " + maxCount.ToString());
        }

        public string ImportBlah(ImportRecord curRec)
        {
            string resultStr = "failed.";

            try
            {
                if (!curRec.UseImage)
                    curRec.ImageURL = null;

                if (!String.IsNullOrEmpty(curRec.ImageURL))
                    resultStr = GenerateImageAndPostBlah(curRec);
                else
                    resultStr = CreateBlah(curRec);
            }
            catch (Exception exp)
            {
                // to do - something
                resultStr = exp.Message;
            }

            return resultStr;
        }

        private string UploadURL = null;

        private string GenerateImageAndPostBlah(ImportRecord curRec)
        {
            string resultStr = "";

            string formData = "imageurl=" + HttpUtility.UrlPathEncode(cleanUrlString(curRec.ImageURL));
            Uri postURL = new Uri("http://heard-test-001.appspot.com/api/image/url");
            string newURL = Heard.PostDataToService(postURL, formData, false);

            curRec.ImageURL = newURL;

            // now upload the image
            
       
            resultStr = CreateBlah(curRec);

            return resultStr;
        }

        private string cleanUrlString(string sourceStr)
        {
            int findLoc = sourceStr.IndexOf('?');
            if (findLoc != -1)
                return sourceStr.Substring(0, findLoc);
            else
                return sourceStr;
        }



        private string CreateBlah(ImportRecord curRec)
        {
            string resultStr = "failed.";

            string paramStr = "{";
            paramStr += createJsonParameter("G", GetChannelId()) + ", ";
            if (!String.IsNullOrEmpty(curRec.Title))
            {
                string theTitle = curRec.Title;
                if (theTitle.Length > 244)
                {
                    theTitle = theTitle.Substring(0, 244) + "...";
                }
                paramStr += createJsonParameter("T", theTitle) + ", ";
            }

            if (curRec.Mature)
                paramStr += createJsonParameter("XXX", "true") + ",";

            paramStr += createJsonParameter("Y", GetBlahTypeId());
            string bodyString = "";
                
            if (!String.IsNullOrEmpty(curRec.MainURL))
                bodyString += curRec.MainURL;

            if (!String.IsNullOrEmpty(curRec.Body))
            {
                string theBody = curRec.Body;
                if (theBody.Length + bodyString.Length > 2000)
                {
                    theBody = theBody.Substring(0, 2000 - bodyString.Length);
                    theBody += "...";
                }

                    bodyString = theBody + "\n\n" + bodyString;
            }

            if (!String.IsNullOrEmpty(bodyString))
                paramStr += ", " + createJsonParameter("F", bodyString);

            if (!String.IsNullOrEmpty(curRec.ImageURL))
                paramStr += "," + createJsonParameter("M", "[\"" + curRec.ImageURL + "\"]", false);
           

            paramStr += "}";
            try
            {
                string theBlah = Heard.CreateBlah(paramStr);

                if (theBlah != "")
                {
                    resultStr = "ok";
                }

            }
            catch (Exception exp)
            {
                resultStr = exp.Message;
            }

            return resultStr;
        }

        private string FormatJSONString(string inputStr)
        {
            string newStr;

            inputStr = inputStr.Replace("\r\n", "[_r;");
            inputStr = inputStr.Replace("\n", "[_r;");
            inputStr = inputStr.Replace("\r", "[_r;");
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            newStr = serializer.Serialize(inputStr);

            return newStr;
        }


        private string createJsonParameter(string paramName, string paramVal, bool quoteIt = true)
        {
            string resultStr = "";

            resultStr += "\"" + paramName + "\":";
            if (quoteIt)
                paramVal = FormatJSONString(paramVal);

            resultStr += paramVal;

            return resultStr;
        }

        private string CurrentUserName = "";
        private void SetCurrentUser(string userName)
        {
            if (userName != CurrentUserName)
            {
                if (CurrentUserName != "")
                {
                    Heard.LogoutUser();
                    CurrentUserName = "";
                }


                SignInUser();
            }
        }


        private void SignInUser()
        {
            Heard.SignInUser(TargetUser, TargetPassword);
            CurrentUserName = TargetUser;
        }

     


        // utilities
        private string GetChannelId()
        {
            return Heard.GetChannelId(TargetChannel.ToLower());
        }

      
        private string GetBlahTypeId()
        {
            return Heard.GetBlahTypeID("says");
        }



    }

    
}
