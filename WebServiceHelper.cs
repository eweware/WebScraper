using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Win32;
using System.Windows;

namespace WebScraper
{
    /// <summary>
    /// Summary description for WebServiceHelper
    /// </summary>
    public class WebServiceHelper
    {
        public Dictionary<string, string> groupNames = null;
        public Dictionary<string, string> userGroupNames = null;
        public Dictionary<string, string> blahTypes = null;
        private CookieContainer sessionCookie = null;
        private bool importingFromQA = true;
        public String CurrentUser = "";
        public string DefaultFolderPath = "";

        public WebServiceHelper()
        {
            //
            // TODO: Add constructor logic here

            
 
        }

        public bool ImportToQA
        {
            set 
            {
                if (value != importingFromQA)
                { 
                    importingFromQA = value;
                }
            }
            get
            {
                return importingFromQA;
            }
        }

       

        // helper functions
        // RESTful helping methods // ec2-50-17-23-5.compute-1.amazonaws.com
        public static string prodURL = @"app.goheard.com"; // production
        public static string qaURL = @"qa.rest.goheard.com"; // production
        public static string devURL = @"localhost"; // production

        private string subURL = "v2";
        private bool _usingDefaultUser = true;

        public bool IsDefaultUser()
        {
            return _usingDefaultUser;
        }





        public Uri CreateURIfromBaseURL(string path, string query = "")
        {
            UriBuilder builder = new UriBuilder();
            if (ImportToQA)
            {
                builder.Host = devURL;// qaURL;
                builder.Path = path;
                builder.Scheme = "http";
                builder.Port = 8090;// 8080;
            }
            else
            {
                builder.Host = prodURL;
                builder.Path = path;
                builder.Scheme = "http"; 
            }

            builder.Query = query;
            Uri result = builder.Uri;

            return result;

        }

        private string CreateRESTBaseURL(string command)
        {
            return "/" + subURL + "/" + command;
        }



        public string GetDataFromService(Uri commandURI)
        {
            return DoWebServiceRequest("GET", commandURI);
        }

        public string PostDataToService(Uri commandURI, string JSONdata, bool useJSON = true)
        {
            return DoWebServiceRequest("POST", commandURI, JSONdata, useJSON);
        }

        public string PutDataToService(Uri commandURI, string JSONdata)
        {
            return DoWebServiceRequest("PUT", commandURI, JSONdata);
        }

        public string DeleteDataFromService(Uri commandURI)
        {
            return DoWebServiceRequest("DELETE", commandURI);
        }

        private string DoWebServiceRequest(string method, Uri commandURI, string JSONdata = "", bool useJSON = true)
        {
            int startTime = Environment.TickCount;
            DateTime callDate = DateTime.Now;

            WebRequest request = WebRequest.Create(commandURI);
            if (sessionCookie == null)
                sessionCookie = new CookieContainer();
            ((HttpWebRequest)request).CookieContainer = sessionCookie;
            request.Timeout = 200000;
            ((HttpWebRequest)request).KeepAlive = true;
            
            Stream dataStream;
            bool failed;

            request.Method = method;

            if (JSONdata != "")
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(JSONdata);
                if (useJSON)
                    request.ContentType = "application/json";
                else
                    request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }

            // Get the response.
            string responseFromServer = "";

            WebResponse response;
            try
            {
                response = request.GetResponse();
                failed = false;

            }
            catch (WebException exp)
            {
                response = exp.Response;
                failed = true;
            }

            HttpStatusCode resultCode = HttpStatusCode.Ambiguous;

            if (response != null)
            {
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);

                // Read the content.
                responseFromServer = reader.ReadToEnd();
                reader.Close();
                resultCode = ((HttpWebResponse)response).StatusCode;
                dataStream.Close();
                response.Close();
                response.Dispose();
            }

            
            // Clean up the streams.


            int endTime = Environment.TickCount;

            if (failed)
            {
                WebException exp = new WebException(responseFromServer);
                throw exp;
            }
            return responseFromServer;
        }





        /// <summary>
        /// Creates a new user with the given name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string CreateUser(string userName, string password)
        {
            string UserStr = CreateRESTBaseURL("users");
            Uri commandURI;
            commandURI = CreateURIfromBaseURL(UserStr);
            string json = "{\"N\":\"" + userName + "\", \"pwd\":\"" + password + "\"}";

            string newUserStr = PostDataToService(commandURI, json);

            string newID = GetJSONProperty(newUserStr, "_id");

            return newUserStr;
        }




        /// <summary>
        /// Given a userID, returns the user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GetUser(string userId)
        {
            Uri commandURI;
            string UserStr = CreateRESTBaseURL("users");
            UserStr += "/" + userId;
            commandURI = CreateURIfromBaseURL(UserStr);

            return GetDataFromService(commandURI);
        }


        /// <summary>
        /// Returns a list of all blah types
        /// </summary>
        /// <returns></returns>
        public string GetBlahTypes()
        {
            string TypesString = CreateRESTBaseURL("blahs/types");
            Uri commandURI;
            commandURI = CreateURIfromBaseURL(TypesString);

            return GetDataFromService(commandURI);
        }



        protected List<string> GetUserGroupIDs()
        {
            string userGroupsStr = GetUserGroups();
            string[] split = new string[] { "},{" };
            string[] groups = userGroupsStr.Split(split, StringSplitOptions.RemoveEmptyEntries);
            List<string> newList = new List<string>();
            groups[0] = groups[0].Substring(groups[0].IndexOf('{') + 1);

            foreach (string curGroup in groups)
            {
                string idString = GetJSONProperty(curGroup, "groupId", "");
                if (idString == "")
                    idString = GetJSONProperty(curGroup, "_id", "");

                newList.Add(idString);
            }
            return newList;
        }









        /// <summary>
        /// Creates a new Blah, given the text string, the ID, and the group
        /// </summary>
        /// <param name="blahText"></param>
        /// <param name="blahTypeID"></param>
        /// <param name="blahGroupID"></param>
        /// <returns></returns>
        public string CreateBlah(string paramStr)
        {

            string BlahStr = CreateRESTBaseURL("blahs");
   

            Uri commandURI = CreateURIfromBaseURL(BlahStr);

            return PostDataToService(commandURI, paramStr);
        }

        public string UpdateUserProfile(string paramStr)
        {
            string profileStr = CreateRESTBaseURL("users/profile/info");

            Uri commandURL = CreateURIfromBaseURL(profileStr);
            return PutDataToService(commandURL, paramStr);
        }






        /// <summary>
        /// AddBlahComment
        /// </summary>
        /// <param name="newComment"></param>
        /// <returns></returns>
        public string AddBlahComment(string newComment, string currentBlahID)
        {
            string BlahStr = CreateRESTBaseURL("comments");
            Uri commandURI = CreateURIfromBaseURL(BlahStr);
            //'{"authorId": authorId, "blahId": blahId, "text": text, "blahVote": blahVote}' 

            string jsonData = "{\"blahVote\":0, \"text\":\"" + newComment + "\"}";

            
            return PostDataToService(commandURI, jsonData);

        }


        public string GetDefaultBadges()
        {
            string badgeStr = "";
            StringReader jsonStr = new StringReader(CurrentUser);
            Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(jsonStr);

            while (reader.Read())
            {
                System.Console.WriteLine(reader.TokenType);
                if (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
                {
                    System.Console.WriteLine(" ==> " + reader.Value.ToString());
                    if (reader.Value.ToString() == "B")
                    {
                        reader.Read();
                        if (reader.TokenType == Newtonsoft.Json.JsonToken.StartArray)
                        {
                            reader.Read();

                            while (reader.TokenType != Newtonsoft.Json.JsonToken.EndArray)
                            {
                                if (badgeStr != "")
                                    badgeStr += ", ";
                                badgeStr += "\"" + reader.Value.ToString() + "\"";
                                reader.Read();
                            }

                            break;
                        }
                    }
                }
            }

            return badgeStr;
        }

        public string GetCurrentUserId()
        {
            string idStr = "";
            StringReader jsonStr = new StringReader(CurrentUser);
            Newtonsoft.Json.JsonTextReader reader = new Newtonsoft.Json.JsonTextReader(jsonStr);

            while (reader.Read())
            {
                if (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName)
                {
                    if (reader.Value.ToString() == "_id")
                    {
                        reader.Read();
                        idStr = reader.Value.ToString();
                        break;
                    }
                }
            }
            return idStr;
        }


        /// <summary>
        /// Helper function to return the value of a property from a JSON string
        /// </summary>
        /// <param name="json"></param>
        /// <param name="propName"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static string GetJSONProperty(string json, string propName, string defaultVal = "missing")
        {
            
            string resultStr = defaultVal;
            string searchStr = "\"" + propName + "\":";
            int startIndex = json.IndexOf(searchStr);

            if (startIndex != -1)
            {
                startIndex += propName.Length + 4;

                int endIndex = json.IndexOf('\"', startIndex + 1);
                if (endIndex == -1)
                {
                    endIndex = json.IndexOf("}", startIndex + 1);
                }

                resultStr = json.Substring(startIndex, endIndex - startIndex);
            }

            return resultStr;

        }


 



        /// <summary>
        /// returns the groups of the current user.
        /// </summary>
        /// <param name="statusStr"></param>
        /// <returns></returns>
        public string GetUserGroups()
        {
            string UserStr = CreateRESTBaseURL("userGroups");
            string queryArgs = "";

            Uri commandURI = CreateURIfromBaseURL(UserStr, queryArgs);

            string resultStr = GetDataFromService(commandURI);

            return resultStr; // AddNameAndDescToGroupList(resultStr);
        }

        public string GetAllGroups()
        {
            string UserStr = CreateRESTBaseURL("groups");
            string queryArgs = "";

            Uri commandURI = CreateURIfromBaseURL(UserStr, queryArgs);

            string resultStr = GetDataFromService(commandURI);

            return resultStr; // AddNameAndDescToGroupList(resultStr);
        }



        /// <summary>
        /// Joins a group
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public string JoinGroup(string groupID)
        {
            string GroupStr = CreateRESTBaseURL("userGroups");

            Uri commandURI = CreateURIfromBaseURL(GroupStr);
            string jsonData;

            jsonData = "{\"G\":\"" + groupID + "\"}";
            userGroupNames = null;
            return PostDataToService(commandURI, jsonData);


        }

        public Boolean CheckUserExists(string userName)
        {
            Boolean exists = false;
            string GroupStr = CreateRESTBaseURL("users/check/username/" + userName);

            Uri commandURI = CreateURIfromBaseURL(GroupStr);
            string jsonData;

            jsonData = "{}";

            try
            {
                string result = PostDataToService(commandURI, jsonData);

                exists = result.Contains("true");
            }
            catch (WebException exp)
            {
                exists = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            return exists;
        }

        public string SignInUser(string userName, string passWord)
        {
            string GroupStr = CreateRESTBaseURL("users/login");

            Uri commandURI = CreateURIfromBaseURL(GroupStr);
            string jsonData;

            jsonData = "{\"N\":\"" + userName + "\", \"pwd\":\"" + passWord + "\"}";

            PostDataToService(commandURI, jsonData);
            string userStr = CreateRESTBaseURL("users/info");
            commandURI = CreateURIfromBaseURL(userStr);
            CurrentUser = GetDataFromService(commandURI);

            return "";
        }

        public string LogoutUser()
        {
            string GroupStr = CreateRESTBaseURL("users/logout");

            Uri commandURI = CreateURIfromBaseURL(GroupStr);
            string jsonData;

            jsonData = "{}";
            userGroupNames = null;
            string resultStr = PostDataToService(commandURI, jsonData);
            sessionCookie = null;
            CurrentUser = "";
            return resultStr; 
        }

        public Boolean UserHasChannel(string channelName)
        {
            if (userGroupNames == null)
                LoadUserChannels();

            if (userGroupNames.Count == 0)
                return false;
            else
                return userGroupNames.Keys.Contains(channelName);

        }






        /// <summary>
        /// Returns the ID for a blah type given the name
        /// </summary>
        /// <param name="blahType"></param>
        /// <returns></returns>
        public string GetBlahTypeID(string blahType)
        {
            string typeID = "";

            if (blahTypes == null)
            {
                blahTypes = new Dictionary<string, string>();
                string resultStr = GetBlahTypes();
                resultStr = resultStr.Substring(1, resultStr.Length - 2);
                string[] split = new string[] { "},{" };
                string[] groups = resultStr.Split(split, StringSplitOptions.RemoveEmptyEntries);
                List<string> newList = new List<string>();
                groups[0] = groups[0].Substring(groups[0].IndexOf('{') + 1);

                foreach (string curGroup in groups)
                {
                    string idString = GetJSONProperty(curGroup, "_id");
                    string nameStr = GetJSONProperty(curGroup, "N");
                    blahTypes[nameStr] = idString;
                }
            }



            if (blahTypes.Keys.Contains(blahType))
                typeID = blahTypes[blahType];
            else
            {
                typeID = null;   
            }

            return typeID;
        }

        public void LoadUserChannels()
        {
            userGroupNames = new Dictionary<string, string>();
            string resultStr = GetUserGroups();
            resultStr = resultStr.Substring(1, resultStr.Length - 2);
            if (resultStr != "")
            {
                string[] split = new string[] { "},{" };
                string[] groups = resultStr.Split(split, StringSplitOptions.RemoveEmptyEntries);
                List<string> newList = new List<string>();
                groups[0] = groups[0].Substring(groups[0].IndexOf('{') + 1);

                foreach (string curGroup in groups)
                {
                    string idString = GetJSONProperty(curGroup, "_id");
                    string nameStr = GetJSONProperty(curGroup, "N");
                    userGroupNames[nameStr.ToLower()] = idString;
                }
            }
        }

        public void LoadAllChannels()
        {
            groupNames = new Dictionary<string, string>();
            string resultStr = GetAllGroups();
            resultStr = resultStr.Substring(1, resultStr.Length - 2);
            string[] split = new string[] { "},{" };
            string[] groups = resultStr.Split(split, StringSplitOptions.RemoveEmptyEntries);
            List<string> newList = new List<string>();
            groups[0] = groups[0].Substring(groups[0].IndexOf('{') + 1);

            foreach (string curGroup in groups)
            {
                string idString = GetJSONProperty(curGroup, "_id");
                string nameStr = GetJSONProperty(curGroup, "N");
                groupNames[nameStr.ToLower()] = idString;
            }

        }

        public string GetChannelId(string channelName)
        {
            string typeID = "";

            if (groupNames == null)
            {
                LoadAllChannels();
            }



            if (groupNames.Keys.Contains(channelName))
                typeID = groupNames[channelName];
            else
            {
                typeID = null;
            }

            return typeID;
        }

        public bool AddFileToBlah(string blahId, string fileName)
        {
            return AddFileToObject(blahId, fileName, "B");
        }

        public bool AddImageToUser(string fileName)
        {
            string userId = GetCurrentUserId();
            return AddFileToObject(userId, fileName, "U");
        }

        public bool AddURLToBlah(string objectId, string urlName)
        {
            bool didIt = false;

            string fileName = DownloadURLToFile(urlName);
            if (!String.IsNullOrEmpty(fileName))
            {
                didIt = AddFileToObject(objectId, fileName, "B");

                File.Delete(fileName);
            }

            return didIt;

        }

        public static string GetDefaultExtension(string mimeType)
        {
            string result;
            RegistryKey key;
            object value;

            key = Registry.ClassesRoot.OpenSubKey(@"MIME\Database\Content Type\" + mimeType, false);
            value = key != null ? key.GetValue("Extension", null) : null;
            result = value != null ? value.ToString() : string.Empty;

            return result;
        }

        public string DownloadURLToFile(string urlName)
        {
            string fileExt = "";
            string fileName = string.Empty;

            try
            {
                var request = HttpWebRequest.Create(urlName) as HttpWebRequest;
                if (request != null)
                {
                    var response = request.GetResponse() as HttpWebResponse;

                    string contentType = "";

                    if (response != null)
                    {
                        contentType = response.ContentType;
                        fileExt = GetDefaultExtension(contentType);
                        if (fileExt == "")
                            return "";
                        fileName = Path.GetTempFileName().Replace(".tmp", fileExt);

                        using (Stream output = File.Create(fileName))
                        {
                            using (Stream input = response.GetResponseStream())
                            {
                                input.CopyTo(output);
                            }
                        }

                    }
                }
            }
            catch (Exception exp)
            {
                fileName = "";
            }

            

            return fileName;
        }

        public bool AddFileToObject(string objectId, string fName, string objType)
        {
            string fileName = fName;
            if (File.Exists(fileName))
            {
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("objectType", objType);
                nvc.Add("primary", "true");
                nvc.Add("objectId", objectId);

                if (ImportToQA)
                    HttpUploadFile("http://qa.rest.goheard.com:8080/v2/images/upload", fileName, "file", "application/octet-stream", nvc);
                else
                    HttpUploadFile("http://app.goheard.com/v2/images/upload", fileName, "file", "application/octet-stream", nvc);
                return true;
            }
            else 
                return false;
        }

        private void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.CookieContainer = sessionCookie;
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //log.Debug(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
                wresp.Close();
                wresp.Dispose();
                wresp = null;
            }
            catch (Exception ex)
            {
                //log.Error("Error uploading file", ex);
                if (wresp != null)
                {
                    wresp.Close();
                    wresp.Dispose();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

    }
}
