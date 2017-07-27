using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using Newtonsoft.Json;
using System.IO;

namespace BBCRssFeed
{
    class Program
    {
        static void Main(string[] args)
        {
            //reference class with methods
            methods m = new methods();
            string path = @"C:\Users\jorda\Desktop\feed";
            m.CreateFile(path);
            List<FeedData> list = m.CreateList();
            list = m.CompareLists(list, path);
            m.SaveFile(list, path);
        }
    }

    class methods
    {
        /**********************************
         Check if the folder has been created
         **********************************/
        public void CreateFile(string path)
        {
            //check if folder exists
            if (!Directory.Exists(path))
            {
                //if not create folder
                Directory.CreateDirectory(path);
            }

        }

        /***********************************
         Pull data from bbc feed 
         **********************************/
        public List<FeedData> CreateList()
        {
            List<FeedData> dataList = new List<FeedData>();

            string feed = "http://feeds.bbci.co.uk/news/uk/rss.xml";
            //set up xml reader
            XmlReader reader = XmlReader.Create(feed);
            //store information in the feed
            SyndicationFeed synd = SyndicationFeed.Load(reader);
            reader.Close();

            //split information from feed into individual class elements
            foreach (SyndicationItem item in synd.Items)
            {
                //reference class and split information
                FeedData fd = new FeedData();
                fd.Title = item.Title.Text;
                fd.Description = item.Summary.Text.Replace("\\", "").Replace("\"", "");
                fd.Link = item.Links[0].Uri.AbsoluteUri;
                fd.pubDate = item.PublishDate.DateTime.ToString();
                //save to list
                dataList.Add(fd);
            }

            return dataList;
        }

        /*******************************************************
         Search folder for files and check each file for duplicate 
         entries.  Delete any duplicate found.
         ******************************************************/
        public List<FeedData> CompareLists(List<FeedData> list, string path)
        {
            //get all files in folder
            string[] files = Directory.GetFiles(path);
            foreach (var item in files)
            {
                //check if the files are from same day 
                if (item.Contains(DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day))
                {
                    //set file information into string
                    string text = File.ReadAllText(item);
                    //check the list backwards so we can delete data without errors
                    foreach (var line in list.Reverse<FeedData>())
                    {
                        //check if file has all this information in it already
                        if (text.Contains(line.Title) &&
                            text.Contains(line.Description) &&
                            text.Contains(line.Link) &&
                            text.Contains(line.pubDate))
                        {
                            //delete if it is
                            list.Remove(line);
                        }
                    }
                }
            }
            //return modified list
            return list;
        }

        /***********************************************
         Save list back to a .Json file
         *********************************************/
        public void SaveFile(List<FeedData> list, string path)
        {
            
            foreach (var item in list)
            {
                //convert text to a formatted Json string
                string json = JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
                //check if string is empty
                if (json != "[]")
                {
                    //open a writer to write to files
                    using (StreamWriter file = new StreamWriter(path + @"\" + DateTime.Now.Date.Year + "-" + DateTime.Now.Month
                        + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + ".json", true))
                    {
                        file.WriteLine(json);
                    }
                }
            }
            //check if file exists
            if (!File.Exists(path + @"\" + DateTime.Now.Date.Year + "-" + DateTime.Now.Month
                       + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + ".json"))
            {
                //create empty one if not
                File.Create(path + @"\" + DateTime.Now.Date.Year + "-" + DateTime.Now.Month
                       + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + ".json");
            }
        }
    }
    
}
