﻿using Nancy.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaeminShopScrapping
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Thread th = new Thread(new ThreadStart(() =>
                {
                    string filePath = @"locationinfo.txt"; // Adjust the path to where your file is stored.

                    // Regular expression to match one or more spaces or tabs
                    Regex delimiterRegex = new Regex(@"[\s\t]+");

                    // List to hold the coordinate tuples
                    List<(string Latitude, string Longitude)> coordinates = new List<(string, string)>();

                    // Read the file line by line
                    try
                    {
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            string line;
                            bool isFirstLine = true; // Variable to skip the header

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (isFirstLine)
                                {
                                    isFirstLine = false; // Skip the first line which is the header
                                    continue;
                                }

                                // Split the line into latitude and longitude parts using the regex
                                string[] parts = delimiterRegex.Split(line.Trim());
                                if (parts.Length >= 2)
                                {
                                    // Add the latitude and longitude as a tuple to the list
                                    coordinates.Add((parts[0], parts[1]));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Can't find locationinfo.txt file, please check that");
                    }

                    string strUrl = string.Format(@"https://shopdp-api.baemin.com/display-groups/BAEMIN?latitude=37.5450159&longitude=127.1368066&sessionId=b4e3292329dfd570f054c8&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic");
                    var client = new RestClient(strUrl);
                    var request = new RestRequest();
                    request.AddHeader("Accept-Encoding", "gzip, deflate");
                    request.AddHeader("Connection", "Keep-Alive");
                    request.AddHeader("Host", "shopdp-api.baemin.com");
                    request.AddHeader("User-Agent", "and1_12.23.0");
                    request.AddHeader("USER-BAEDAL", "W/OnG34HSvOVmxn4McyeRzEK3Ldc9+ruPokFIKgQcm0zVU8aOlNuihy2TNW+7I7ZBORlK3kvRun7bOtwlyMA9PnUeLy01xw69qCQLwVBmJdm/hJB8mRTF8vkzRUt/1qkIjb9Tto92g2qIH9ldixRCvPKlFkepp+bOCN6lWvdTIvEx8s0W2jVWA4NWbjnwqqLvKR0wjQxP9pPG3heaCdvvA==");
                    string strReturn = client.ExecuteGet(request).Content;
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    dynamic data = jss.Deserialize<dynamic>(strReturn);
                    dynamic categories = data["data"]["displayCategories"];

                    // Output the list of coordinates
                    int locationNum = 0;
                    foreach (var (Latitude, Longitude) in coordinates)
                    {
                        locationNum++;
                        this.Invoke(new Action(() =>
                        {
                            Lat.Text = Latitude.ToString();
                            Lon.Text = Longitude.ToString();
                            LocationNum.Text = "Location" + locationNum.ToString();
                        }));
                        File.WriteAllText("log.txt", Environment.NewLine + Lat.Text + ", " + Lon.Text);

                        int catnum = 0;
                        foreach (var category in categories)
                        {
                            catnum++;
                            int shopcount = 2000;
                            int totalcount = 0;
                            for (int i = 0 ; i <= (int)(shopcount / 25); i++)
                            {
                                string strShop = string.Format(@"https://shopdp-api.baemin.com/v3/BAEMIN/shops?displayCategory={3}&longitude={0}&latitude={1}&sort=SORT__DEFAULT&filter=&offset={2}&limit=25&extension=&perseusSessionId=1718023403008.788454282780365941.FWy8AA9FNv&memberNumber=000000000000&sessionId=b4e3292329dfd570f054c8&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic", Longitude.ToString(), Latitude.ToString(), 25 * i, category["code"].ToString());
                                var clientshop = new RestClient(strShop);
                                string strReturnShop = clientshop.ExecuteGet(request).Content;
                                JavaScriptSerializer jssshop = new JavaScriptSerializer();
                                dynamic datashop = jssshop.Deserialize<dynamic>(strReturnShop);
                                dynamic shops = datashop["data"]["shops"];
                                shopcount = (int)datashop["data"]["totalCount"];
                                foreach (var shop in shops)
                                {
                                    totalcount++;
                                    string shopnumber = shop["shopInfo"]["shopNumber"].ToString();
                                    if(!string.IsNullOrEmpty(shopnumber))
                                    {         
                                        string detailurl = string.Format(@"https://shopdp-api.baemin.com/v8/shop/{0}/detail?lat={1}&lng={2}&limit=25&mem=&memid=&defaultreview=N&campaignId=2353465&displayGroup=BAEMIN&lat4Distance=37.5670653&lng4Distance=126.98168738&filter=&sessionId=1447226b282d5e40f677b5a1d37&carrier=302780&site=7jWXRELC2e&dvcid=OPUD6086af457479a7bb&adid=aede849f-5e9c-499f-827f-cb4e5c65d801&deviceModel=SM-G9500&appver=12.23.0&oscd=2&osver=32&dongCode=11140102&zipCode=04522&ActionTrackingKey=Organic", shopnumber, Latitude.ToString(), Longitude.ToString());
                                        var detailclient = new RestClient(detailurl);
                                        string detailresult = detailclient.ExecuteGet(request).Content;
                                        if(detailresult.Contains("SUCCESS"))
                                        {
                                            var dir = "Shops";
                                            Directory.CreateDirectory(dir);
                                            File.WriteAllText(string.Format(@"{0}\shop-{1}-{2}.json", dir, locationNum.ToString(), shopnumber), detailresult);
                                            this.Invoke(new Action(() =>
                                            {
                                                progressBar1.Value = (int)((10000 * catnum * totalcount) / (shopcount * 15));
                                            }));
                                        }
                                    }
                                }
                            }
                            
                        }

                    }
                    if(locationNum > 0)
                    {
                        MessageBox.Show("Successfully done!!!");
                    }
                }));
                th.Start();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                MessageBox.Show(LocationNum.Text);
            }
        }
    }
}
